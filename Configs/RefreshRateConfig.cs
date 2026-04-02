using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class RefreshRateConfig
{
    private const int MinHz = 1;
    private const int MaxHz = 1000;

    internal static int GetTargetHz()
    {
        return ClampHz(ModConfig.RefreshRateOverrideTarget.Value);
    }

    internal static void AdjustTargetHz(int delta)
    {
        ModConfig.RefreshRateOverrideTarget.Value = ClampHz(GetTargetHz() + delta);
    }

    internal static string GetTargetHzLabel()
    {
        return $"{GetTargetHz()} Hz";
    }

    private static int ClampHz(int hz)
    {
        return Mathf.Clamp(hz, MinHz, MaxHz);
    }
}
