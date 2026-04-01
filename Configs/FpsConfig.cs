using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class FpsConfig
{
    private const int MinFps = 0;
    private const int MaxFps = 1000;
    private const int StepFps = 5;

    internal static KeyCode GetOverrideToggleKey()
    {
        return ModConfig.FpsOverrideToggleKeybind.Value;
    }

    internal static void SetOverrideToggleKey(KeyCode keyCode)
    {
        ModConfig.FpsOverrideToggleKeybind.Value = keyCode;
    }

    internal static KeyCode GetUncapToggleKey()
    {
        return ModConfig.FpsUncapToggleKeybind.Value;
    }

    internal static void SetUncapToggleKey(KeyCode keyCode)
    {
        ModConfig.FpsUncapToggleKeybind.Value = keyCode;
    }

    internal static int GetTargetFps()
    {
        return ClampFps(ModConfig.FpsOverrideTarget.Value);
    }

    internal static void IncreaseTargetFps()
    {
        ModConfig.FpsOverrideTarget.Value = ClampFps(GetTargetFps() + StepFps);
    }

    internal static void DecreaseTargetFps()
    {
        ModConfig.FpsOverrideTarget.Value = ClampFps(GetTargetFps() - StepFps);
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
