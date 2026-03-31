using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class FpsConfig
{
    private const int MinFps = 0;
    private const int MaxFps = 1000;
    private const int StepFps = 5;

    internal static KeyCode GetToggleKey()
    {
        return ModConfig.FpsToggleKeybind.Value;
    }

    internal static void SetToggleKey(KeyCode keyCode)
    {
        ModConfig.FpsToggleKeybind.Value = keyCode;
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
        int fps = GetTargetFps();
        return fps <= 0 ? "unlimited" : fps.ToString();
    }

    private static int ClampFps(int fps)
    {
        return Mathf.Clamp(fps, MinFps, MaxFps);
    }
}
