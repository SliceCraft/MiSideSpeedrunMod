using HarmonyLib;
using SpeedrunMod.EventDisplay;
using SpeedrunMod.Menus.Debug;
using SpeedrunMod.Menus.Frames;
using SpeedrunMod.Menus.Keybinds;
using SpeedrunMod.Practice;
using SpeedrunMod.RevealSystems;
using SpeedrunMod.Toggles;
using SpeedrunMod.Utils;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(SteamManager))]
internal class SteamManagerPatch
{
    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    private static void UpdatePatch()
    {
        EventManager.Update();
        VersionText.Update();
        DebugOverlay.Update();
        KeybindCapture.Update();
        DebugOverlayToggle.Update();
        FpsOverrideToggle.Update();
        FpsUncapToggle.Update();
        EnableRunToggle.Update();
        RevealTriggerToggle.Update();
        PracticeManager.Update();
        FpsSettingsMenu.Update();
        DebugSettingsMenu.Update();
        Triggers.Update();
    }
}