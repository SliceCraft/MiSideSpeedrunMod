using BepInEx.Configuration;
using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class RefreshRateConfig
{
    private const int MinHz = 1;
    private const int MaxHz = 100000;

    internal static ConfigEntry<bool> OverrideEnabled;
    internal static ConfigEntry<int> OverrideTarget;

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
            60,
            "Reported refresh rate (Hz) when OverrideEnabled is true (menu: Target Hz).");
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
