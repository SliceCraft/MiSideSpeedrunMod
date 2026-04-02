using SpeedrunMod.Configs;
using SpeedrunMod.EventDisplay;
using SpeedrunMod.Menus.Keybinds;
using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.Toggles;

internal static class FpsOverrideToggle
{
    private static int? _previousFps;
    private static bool _enabled;

    internal static void Update()
    {
        if (!_previousFps.HasValue) 
        {
            if (!FpsUtil.TryReadCurrentFps(out int fps)) return;
            _previousFps = fps;
        }

        if (!IsInGame()) return;
        if (KeybindCapture.IsCapturing()) return;
        if (!Input.GetKeyDown(FpsConfig.OverrideToggleKeybind.Value)) return;

        if (_enabled)
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
        if (!_previousFps.HasValue)
        {
            EventManager.ShowEvent(new ModEvent("Unable to read previous FPS"));
            Plugin.Log.LogError("Previous FPS not captured; cannot enable FPS override.");
            return;
        }

        int overrideFps = FpsConfig.GetTargetFps();
        if (!FpsUtil.TryApplyFps(overrideFps))
        {
            EventManager.ShowEvent(new ModEvent("Unable to set FPS override"));
            Plugin.Log.LogError("Failed to apply FPS override.");
            return;
        }

        _enabled = true;
        EventManager.ShowEvent(new ModEvent($"FPS set to {FpsUtil.FormatFps(overrideFps)}"));
        Plugin.Log.LogInfo($"FPS override enabled, set to {FpsUtil.FormatFps(overrideFps)}.");
    }

    private static void RestoreFps()
    {
        if (!_previousFps.HasValue)
        {
            EventManager.ShowEvent(new ModEvent("Unable to read previous FPS"));
            Plugin.Log.LogError("Previous FPS not captured; cannot restore FPS.");
            return;
        }

        if (!FpsUtil.TryApplyFps(_previousFps.Value))
        {
            EventManager.ShowEvent(new ModEvent("Unable to restore FPS"));
            Plugin.Log.LogError($"Failed to restore FPS to {FpsUtil.FormatFps(_previousFps.Value)}.");
            return;
        }

        _enabled = false;
        EventManager.ShowEvent(new ModEvent($"FPS restored to {FpsUtil.FormatFps(_previousFps.Value)}"));
        Plugin.Log.LogInfo($"FPS override disabled, restored to {FpsUtil.FormatFps(_previousFps.Value)}.");
    }
}
