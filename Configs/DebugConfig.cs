using BepInEx.Configuration;
using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class DebugConfig
{
    private const float MinLogInterval = 0f;
    private const float MaxLogInterval = 120f;

    internal static ConfigEntry<bool> OverlayEnabled;
    internal static ConfigEntry<float> OverlayLogInterval;
    internal static ConfigEntry<KeyCode> OverlayToggleKeybind;

    internal static void Initialize(ConfigFile configFile)
    {
        OverlayEnabled = configFile.Bind(
            "Debug",
            "OverlayEnabled",
            false,
            "On-screen debug overlay (menu: DEBUG).");

        OverlayLogInterval = configFile.Bind(
            "Debug",
            "OverlayLogInterval",
            2f,
            "Seconds between overlay refreshes while the overlay is on; 0 = refresh every frame (menu: DEBUG). " +
            "Movement speed uses displacement over this real-time gap (one value when > 0, roughly per-frame when 0).");

        OverlayToggleKeybind = configFile.Bind(
            "Debug",
            "OverlayToggleKeybind",
            KeyCode.F4,
            "In-game: toggle debug overlay (menu: DEBUG).");
    }

    internal static void AdjustLogInterval(float delta)
    {
        OverlayLogInterval.Value = Mathf.Clamp(
            OverlayLogInterval.Value + delta,
            MinLogInterval,
            MaxLogInterval);
    }
}
