using HarmonyLib;
using SpeedrunMod.EventDisplay;
using SpeedrunMod.Practice;
using SpeedrunMod.Toggles;
using SpeedrunMod.Utils;

namespace SpeedrunMod.Patches
{
    [HarmonyPatch(typeof(SteamManager))]
    internal class SteamManagerPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void UpdatePatch()
        {
            EventManager.Update();
            VersionText.Update();
            EnableRunToggle.Update();
            RevealPlaceStopToggle.Update();
            RevealTriggerToggle.Update();
            PracticeManager.Update();
        }
    }
}
