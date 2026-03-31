using HarmonyLib;
using SpeedrunMod.EventDisplay;
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
        KeybindCapture.Update();
        FpsToggle.Update();
        EnableRunToggle.Update();
        RevealTriggerToggle.Update();
        PracticeManager.Update();
        FpsSettingsMenu.Update();
        Triggers.Update();
    }
}