using System;
using UnityEngine;

namespace SpeedrunMod.Utils;

internal static class FpsUtil
{
    internal static bool TryApplyFps(int fps)
    {
        if (TryApplyFpsDirect(fps) && IsFpsChanged(fps))
        {
            return true;
        }

        return TryApplyFpsViaCommand(fps) && IsFpsChanged(fps);
    }

    private static bool TryApplyFpsDirect(int fps)
    {
        if (fps < 0)
        {
            return false;
        }

        Application.targetFrameRate = fps;
        Plugin.Log.LogInfo($"FPS change method: direct API (Application.targetFrameRate), target={FormatFps(fps)}.");
        return true;
    }

    private static bool TryApplyFpsViaCommand(int fps)
    {
        try
        {
            ConsoleCommandsGame.Command($"setfps {fps}");
            Plugin.Log.LogInfo($"FPS change method: setfps command fallback, target={FormatFps(fps)}.");
            return true;
        }
        catch (Exception exception)
        {
            Plugin.Log.LogError($"setfps fallback failed: {exception}");
            return false;
        }
    }

    internal static bool TryReadCurrentFps(out int fps)
    {
        try
        {
            fps = ReadCurrentFps();
            return true;
        }
        catch (Exception exception)
        {
            Plugin.Log.LogError($"Unable to read current FPS: {exception}");
            fps = 0;
            return false;
        }
    }

    private static int ReadCurrentFps()
    {
        return Application.targetFrameRate <= 0 ? 0 : Application.targetFrameRate;
    }

    internal static bool IsFpsChanged(int fps)
    {
        if (!TryReadCurrentFps(out int currentFps))
        {
            return false;
        }

        return fps <= 0 ? currentFps == 0 : currentFps == fps;
    }

    internal static string FormatFps(int fps)
    {
        return fps <= 0 ? "uncapped" : fps.ToString();
    }
}
