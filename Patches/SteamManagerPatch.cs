using HarmonyLib;
using SpeedrunMod.EventDisplay;
using SpeedrunMod.Toggles;

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
            EnableRunToggle.Update();
            RevealPlaceStopToggle.Update();
            RevealTriggerToggle.Update();
        }
    }
}
