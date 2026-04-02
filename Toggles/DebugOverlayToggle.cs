using SpeedrunMod.Configs;
using SpeedrunMod.EventDisplay;
using SpeedrunMod.Menus.Debug;
using SpeedrunMod.Menus.Keybinds;
using UnityEngine;

namespace SpeedrunMod.Toggles;

internal static class DebugOverlayToggle
{
    internal static void Update()
    {
        if (!IsInGame()) return;
        if (KeybindCapture.IsCapturing()) return;
        if (!Input.GetKeyDown(DebugConfig.OverlayToggleKeybind.Value)) return;

        DebugConfig.OverlayEnabled.Value = !DebugConfig.OverlayEnabled.Value;
        bool on = DebugConfig.OverlayEnabled.Value;
        EventManager.ShowEvent(new ModEvent(on ? "Debug overlay on" : "Debug overlay off"));
        Plugin.Log.LogInfo($"Debug overlay {(on ? "enabled" : "disabled")} (hotkey).");
    }

    private static bool IsInGame()
    {
        return Object.FindObjectOfType<GameController>() != null;
    }
}
