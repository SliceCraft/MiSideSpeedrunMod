using SpeedrunMod.Configs;
using SpeedrunMod.EventDisplay;
using SpeedrunMod.Menus.Keybinds;
using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.Toggles;

internal static class OverlayToggle
{
    internal static void Update()
    {
        if (!Input.GetKeyDown(OverlayConfig.OverlayToggleKeybind.Value)) return;
        if (KeybindCapture.IsCapturing()) return;
        if (!GameUtil.IsInGame()) return;

        OverlayConfig.OverlayEnabled.Value = !OverlayConfig.OverlayEnabled.Value;
        bool on = OverlayConfig.OverlayEnabled.Value;
        EventManager.ShowEvent(new ModEvent(on ? "Overlay on" : "Overlay off"));
        Plugin.Log.LogInfo($"Overlay {(on ? "enabled" : "disabled")} (hotkey).");
    }
}
