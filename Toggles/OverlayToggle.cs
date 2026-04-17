using SpeedrunMod.Configs;
using SpeedrunMod.EventDisplay;
using SpeedrunMod.Menus.Keybinds;
using UnityEngine;

namespace SpeedrunMod.Toggles;

internal static class OverlayToggle
{
    internal static void Update()
    {
        if (!IsInGame()) return;
        if (KeybindCapture.IsCapturing()) return;
        if (!Input.GetKeyDown(OverlayConfig.OverlayToggleKeybind.Value)) return;

        OverlayConfig.OverlayEnabled.Value = !OverlayConfig.OverlayEnabled.Value;
        bool on = OverlayConfig.OverlayEnabled.Value;
        EventManager.ShowEvent(new ModEvent(on ? "Overlay on" : "Overlay off"));
        Plugin.Log.LogInfo($"Overlay {(on ? "enabled" : "disabled")} (hotkey).");
    }

    private static bool IsInGame()
    {
        return Object.FindObjectOfType<GameController>() != null;
    }
}
