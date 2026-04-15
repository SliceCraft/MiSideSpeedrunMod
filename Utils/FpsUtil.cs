using UnityEngine;

namespace SpeedrunMod.Utils;

internal static class FpsUtil
{
    internal static void SetFps(int fps)
    {
        if (fps < 0)
        {
            UncapFps(disableVSync: false);
            return;
        }

        Application.targetFrameRate = fps;
        Plugin.Log.LogInfo($"FPS set to {fps}.");
    }

    internal static void UncapFps(bool disableVSync = false)
    {
        Application.targetFrameRate = -1;

        if (disableVSync)
        {
            QualitySettings.vSyncCount = 0;
        }

        Plugin.Log.LogInfo("FPS uncapped.");
    }

    internal static void SetVSyncCount(int vSyncMode)
    {
        QualitySettings.vSyncCount = vSyncMode;
    }

    internal static int GetCurrentFps() => Application.targetFrameRate;

    internal static int GetCurrentVSyncCount() => QualitySettings.vSyncCount;

    internal static string FormatFps(int fps) => fps <= 0 ? "uncapped" : fps.ToString();
}
