using BepInEx.Configuration;
using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class RefreshRateConfig
{
    private const int MinHz = 1;
    private const int MaxHz = 100000;

    internal const int DefaultOverrideHz = 540;
    internal const int InvalidThresholdHz = 540;

    internal static ConfigEntry<bool> OverrideEnabled;
    internal static ConfigEntry<int> OverrideTarget;
    
    private static int _cachedRefreshRateHz;

    internal static void Initialize(ConfigFile configFile)
    {
        OverrideEnabled = configFile.Bind(
            "RefreshRate",
            "OverrideEnabled",
            false,
            "When true, Screen.currentResolution reports OverrideTarget Hz. Restart the game after changing this if the game caches refresh rate once at launch (e.g. MiSide).");

        OverrideTarget = configFile.Bind(
            "RefreshRate",
            "OverrideTarget",
            DefaultOverrideHz,
            "Reported refresh rate (Hz) when OverrideEnabled is true (menu: Target Hz).");

        _cachedRefreshRateHz = OverrideEnabled.Value ? OverrideTarget.Value : Screen.currentResolution.refreshRate;
        Plugin.Log.LogInfo($"Cached refresh rate: {_cachedRefreshRateHz} Hz");
    }

    internal static int GetActualHz()
    {
        if (!OverrideEnabled.Value)
        {
            return Screen.currentResolution.refreshRate;
        }

        // Because the actual refresh rate is cached at startup
        // and requires full game restart to take effect
        // we need to return the cached value if it has been changed
        if (_cachedRefreshRateHz != OverrideTarget.Value)
        {
            Plugin.Log.LogWarning(
                $"Refresh rate has been changed since the last call ({OverrideTarget.Value} Hz), returning cached value ({_cachedRefreshRateHz} Hz)",
                context: nameof(RefreshRateConfig),
                throttleSeconds: 60);

            return _cachedRefreshRateHz;
        }

        return OverrideTarget.Value;
    }

    internal static int GetTargetHz()
    {
        return Mathf.Clamp(OverrideTarget.Value, MinHz, MaxHz);
    }

    internal static void SetTargetHz(int hz)
    {
        OverrideTarget.Value = Mathf.Clamp(hz, MinHz, MaxHz);
    }
}
