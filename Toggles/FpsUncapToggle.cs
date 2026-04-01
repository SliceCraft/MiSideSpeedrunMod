using SpeedrunMod.Configs;
using SpeedrunMod.EventDisplay;
using SpeedrunMod.Menus.Keybinds;
using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.Toggles;

internal static class FpsUncapToggle
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
        if (!Input.GetKeyDown(FpsConfig.GetUncapToggleKey())) return;

        if (_enabled)
        {
            RestoreFps();
            return;
        }

        EnableUncap();
    }

    private static bool IsInGame()
    {
        return UnityEngine.Object.FindObjectOfType<GameController>() != null;
    }

    private static void EnableUncap()
    {
        if (!_previousFps.HasValue)
        {
            EventManager.ShowEvent(new ModEvent("Unable to read previous FPS"));
            Plugin.Log.LogError("Previous FPS not captured; cannot uncap.");
            return;
        }

        if (!FpsUtil.TryApplyFps(0))
        {
            EventManager.ShowEvent(new ModEvent("Unable to uncap FPS"));
            Plugin.Log.LogError("Failed to apply uncapped FPS.");
            return;
        }

        _enabled = true;
        EventManager.ShowEvent(new ModEvent("FPS uncapped"));
        Plugin.Log.LogInfo("FPS uncap enabled.");
    }

    private static void RestoreFps()
    {
        if (!_previousFps.HasValue)
        {
            EventManager.ShowEvent(new ModEvent("Unable to read previous FPS"));
            Plugin.Log.LogError("Previous FPS not captured; cannot restore FPS after uncap.");
            return;
        }

        if (!FpsUtil.TryApplyFps(_previousFps.Value))
        {
            EventManager.ShowEvent(new ModEvent("Unable to restore FPS after uncap"));
            Plugin.Log.LogError($"Failed to restore FPS to {FpsUtil.FormatFps(_previousFps.Value)}.");
            return;
        }

        _enabled = false;
        EventManager.ShowEvent(new ModEvent($"FPS restored to {FpsUtil.FormatFps(_previousFps.Value)}"));
        Plugin.Log.LogInfo($"FPS uncap disabled, restored to {FpsUtil.FormatFps(_previousFps.Value)}.");
    }
}
