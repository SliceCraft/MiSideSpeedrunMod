using SpeedrunMod.Configs;
using SpeedrunMod.Notifications;
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
            int fps = FpsUtil.GetCurrentFps();
            _previousFps = fps;
        }

        if (!Input.GetKeyDown(FpsConfig.OverrideToggleKeybind.Value)) return;
        if (KeybindCapture.IsCapturing()) return;
        if (!GameUtil.IsInGame()) return;

        if (_enabled)
        {
            RestoreFps();
            return;
        }

        EnableOverride();
    }

    private static void EnableOverride()
    {
        if (!_previousFps.HasValue)
        {
            NotificationManager.Show(new NotificationMessage("Unable to read previous FPS"));
            Plugin.Log.LogError("Previous FPS not captured; cannot enable FPS override.");
            return;
        }

        int overrideFps = FpsConfig.GetTargetFps();
        FpsUtil.SetFps(overrideFps);

        _enabled = true;
        NotificationManager.Show(new NotificationMessage($"FPS set to {FpsUtil.FormatFps(overrideFps)}"));
        Plugin.Log.LogInfo($"FPS override enabled, set to {FpsUtil.FormatFps(overrideFps)}.");
    }

    private static void RestoreFps()
    {
        if (!_previousFps.HasValue)
        {
            NotificationManager.Show(new NotificationMessage("Unable to read previous FPS"));
            Plugin.Log.LogError("Previous FPS not captured; cannot restore FPS.");
            return;
        }

        FpsUtil.SetFps(_previousFps.Value);

        _enabled = false;
        NotificationManager.Show(new NotificationMessage($"FPS restored to {FpsUtil.FormatFps(_previousFps.Value)}"));
        Plugin.Log.LogInfo($"FPS override disabled, restored to {FpsUtil.FormatFps(_previousFps.Value)}.");
    }
}
