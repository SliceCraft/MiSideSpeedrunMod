using HarmonyLib;
using SpeedrunMod.Configs;
using SpeedrunMod.Notifications;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(Location7_RingWork))]
internal class Location7_RingWorkPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    private static void StartPostfix(Location7_RingWork __instance)
    {
        if (!ModConfig.EnableCappieRingSkip.Value)
        {
            return;
        }

        try
        {
            __instance.ReadyTime();
            Plugin.Log.LogInfo("Cappie ring wait skipped");
            NotificationManager.Show(new NotificationMessage("Cappie ring wait skipped"));
        }
        catch (System.Exception ex)
        {
            Plugin.Log.LogError($"Failed to skip Cappie ring wait: {ex}");
        }
    }
}
