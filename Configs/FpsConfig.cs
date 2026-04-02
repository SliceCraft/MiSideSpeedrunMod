using BepInEx.Configuration;
using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class FpsConfig
{
    private const int MinFps = 0;
    private const int MaxFps = 1000;

    internal static ConfigEntry<KeyCode> OverrideToggleKeybind;
    internal static ConfigEntry<KeyCode> UncapToggleKeybind;
    internal static ConfigEntry<int> OverrideTarget;

    internal static void Initialize(ConfigFile configFile)
    {
        OverrideToggleKeybind = configFile.Bind(
            "FPS",
            "OverrideToggleKeybind",
            KeyCode.F1,
            "In-game: toggle between your configured target FPS and the previous FPS.");

        OverrideTarget = configFile.Bind(
            "FPS",
            "OverrideTarget",
            5,
            "Target FPS when the target-FPS override toggle is on (menu: Target FPS). Use 0 for uncapped.");

        UncapToggleKeybind = configFile.Bind(
            "FPS",
            "UncapToggleKeybind",
            KeyCode.F2,
            "In-game: toggle uncapped FPS (setfps 0) and restore previous FPS.");
    }

    internal static int GetTargetFps()
    {
        return ClampFps(OverrideTarget.Value);
    }

    internal static void AdjustTargetFps(int delta)
    {
        OverrideTarget.Value = ClampFps(GetTargetFps() + delta);
    }

    internal static string GetTargetFpsLabel()
    {
        return FpsUtil.FormatFps(GetTargetFps());
    }

    private static int ClampFps(int fps)
    {
        return Mathf.Clamp(fps, MinFps, MaxFps);
    }
}
