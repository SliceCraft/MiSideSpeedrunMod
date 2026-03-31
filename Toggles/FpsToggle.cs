using System;
using SpeedrunMod.Configs;
using SpeedrunMod.EventDisplay;
using UnityEngine;

namespace SpeedrunMod.Toggles;

internal static class FpsToggle
{
    private static int? _previousFps;

    internal static void Update()
    {
        if (!IsInGame()) return;

        if (!Input.GetKeyDown(FpsConfig.GetToggleKey())) return;

        if (_previousFps.HasValue)
        {
            RestoreFps();
            return;
        }

        EnableOverride();
    }

    private static bool IsInGame()
    {
        return UnityEngine.Object.FindObjectOfType<GameController>() != null;
    }

    private static void EnableOverride()
    {
        if (!TryReadCurrentFps(out int currentFps))
        {
            EventManager.ShowEvent(new ModEvent("Unable to read current FPS"));
            Plugin.Log.LogError("Failed to read current FPS before enabling FPS override.");
            return;
        }

        _previousFps = currentFps;

        int overrideFps = FpsConfig.GetTargetFps();
        if (!TryApplyFps(overrideFps))
        {
            EventManager.ShowEvent(new ModEvent("Unable to set FPS override"));
            Plugin.Log.LogError("Failed to apply FPS override.");
            return;
        }

        EventManager.ShowEvent(new ModEvent($"FPS set to {FormatFps(overrideFps)}"));
        Plugin.Log.LogInfo($"FPS override enabled, set to {FormatFps(overrideFps)}.");
    }

    private static void RestoreFps()
    {
        if (_previousFps is null)
        {
            Plugin.Log.LogWarning("Previous FPS is unknown, restoring to unlimited.");
        }

        int fpsToRestore = _previousFps ?? 0;

        if (!TryApplyFps(fpsToRestore))
        {
            EventManager.ShowEvent(new ModEvent("Unable to restore FPS"));
            Plugin.Log.LogError($"Failed to restore FPS to {FormatFps(fpsToRestore)}.");
            return;
        }

        _previousFps = null;
        EventManager.ShowEvent(new ModEvent($"FPS restored to {FormatFps(fpsToRestore)}"));
        Plugin.Log.LogInfo($"FPS override disabled, restored to {FormatFps(fpsToRestore)}.");
    }

    private static bool TryApplyFps(int fps)
    {
        if (TryApplyFpsDirect(fps) && IsFpsChanged(fps))
        {
            return true;
        }

        return TryApplyFpsViaCommand(fps) && IsFpsChanged(fps);
    }

    private static bool TryApplyFpsDirect(int fps)
    {
        if (fps <= 0)
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

    private static int ReadCurrentFps()
    {
        return Application.targetFrameRate <= 0 ? 0 : Application.targetFrameRate;
    }

    private static bool TryReadCurrentFps(out int fps)
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

    private static bool IsFpsChanged(int fps)
    {
        if (!TryReadCurrentFps(out int currentFps))
        {
            return false;
        }
        return fps <= 0 ? currentFps == 0 : currentFps == fps;
    }

    private static string FormatFps(int fps)
    {
        return fps <= 0 ? "unlimited" : fps.ToString();
    }
}
