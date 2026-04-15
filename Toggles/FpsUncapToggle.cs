using SpeedrunMod.Configs;
using SpeedrunMod.EventDisplay;
using SpeedrunMod.Menus.Keybinds;
using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.Toggles;

internal static class FpsUncapToggle
{
    private static int? _previousFps;
    private static int? _previousVSyncCount;
    private static bool _enabled;

    internal static void Update()
    {
        if (!_previousFps.HasValue || !_previousVSyncCount.HasValue)
        {
            int fps = FpsUtil.GetCurrentFps();
            int vSyncMode = FpsUtil.GetCurrentVSyncCount();
            _previousFps = fps;
            _previousVSyncCount = vSyncMode;
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
        return Object.FindObjectOfType<GameController>() != null;
    }

    private static void EnableUncap()
    {
        if (!_previousFps.HasValue || !_previousVSyncCount.HasValue)
        {
            EventManager.ShowEvent(new ModEvent("Unable to read previous FPS or VSync count"));
            Plugin.Log.LogError("Previous FPS not captured or VSync count not captured; cannot uncap.");
            return;
        }

        FpsUtil.UncapFps(disableVSync: true);

        _enabled = true;
        EventManager.ShowEvent(new ModEvent("FPS uncapped and VSync disabled"));
        Plugin.Log.LogInfo("FPS uncap enabled and VSync disabled.");
    }

    private static void RestoreFps()
    {
        if (!_previousFps.HasValue || !_previousVSyncCount.HasValue)
        {
            EventManager.ShowEvent(new ModEvent("Unable to read previous FPS or VSync count"));
            Plugin.Log.LogError("Previous FPS not captured or VSync count not captured; cannot restore FPS after uncap.");
            return;
        }

        FpsUtil.SetFps(_previousFps.Value);
        FpsUtil.SetVSyncCount(_previousVSyncCount.Value);

        _enabled = false;
        EventManager.ShowEvent(new ModEvent($"VSync restored, FPS uncap disabled and restored to {FpsUtil.FormatFps(_previousFps.Value)}"));
        Plugin.Log.LogInfo($"VSync restored, FPS uncap disabled and restored to {FpsUtil.FormatFps(_previousFps.Value)}.");
    }
}
