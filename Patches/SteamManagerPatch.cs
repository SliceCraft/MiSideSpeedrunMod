using HarmonyLib;
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
            EnableRunToggle.Update();
            RevealPlaceStopToggle.Update();
            RevealTriggerToggle.Update();
        }
    }
}
