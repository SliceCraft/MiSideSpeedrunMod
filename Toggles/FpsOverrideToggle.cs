using SpeedrunMod.Configs;
using SpeedrunMod.Notifications;
using SpeedrunMod.Menus.Keybinds;
using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.Toggles;

internal static class FpsOverrideToggle
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
        if (!_previousFps.HasValue || !_previousVSyncCount.HasValue)
        {
            NotificationManager.Show(new NotificationMessage("Unable to read previous FPS or VSync count"));
            Plugin.Log.LogError("Previous FPS not captured or VSync count not captured; cannot enable FPS override.");
            return;
        }

        int overrideFps = FpsConfig.GetTargetFps();
        FpsUtil.SetFps(overrideFps);
        FpsUtil.SetVSyncCount(0);

        _enabled = true;
        NotificationManager.Show(new NotificationMessage($"FPS set to {FpsUtil.FormatFps(overrideFps)} and VSync disabled"));
        Plugin.Log.LogInfo($"FPS override enabled, set to {FpsUtil.FormatFps(overrideFps)}  and VSync disabled.");
    }

    private static void RestoreFps()
    {
        if (!_previousFps.HasValue || !_previousVSyncCount.HasValue)
        {
            NotificationManager.Show(new NotificationMessage("Unable to read previous FPS or VSync count"));
            Plugin.Log.LogError("Previous FPS not captured or VSync count not caputred; cannot restore FPS.");
            return;
        }

        FpsUtil.SetFps(_previousFps.Value);
        FpsUtil.SetVSyncCount(_previousVSyncCount.Value);

        _enabled = false;
        NotificationManager.Show(new NotificationMessage($"VSync restored, FPS restored to {FpsUtil.FormatFps(_previousFps.Value)}"));
        Plugin.Log.LogInfo($"VSync restored, FPS override disabled, restored to {FpsUtil.FormatFps(_previousFps.Value)}.");
    }
}
