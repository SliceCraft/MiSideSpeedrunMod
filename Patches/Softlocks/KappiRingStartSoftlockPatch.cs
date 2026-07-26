using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch(typeof(Dialogue_3DText), "Start")]
internal static class KappiRingStartSoftlockPatch
{
    private const string SceneName = "Scene 7 - Backrooms";
    private const string SitDialogueName = "KindMita 15";
    private const int SitDialogueIndex = 236;
    private const string TimeMitaSitName = "Time Mita Sit";
    private const string RingWorkName = "RingWork";

    [HarmonyPostfix]
    private static void StartPostfix(Dialogue_3DText __instance)
    {
        if (!IsKappiScene())
        {
            return;
        }

        if (__instance?.gameObject.name != SitDialogueName || __instance.indexString != SitDialogueIndex)
        {
            return;
        }

        EnsureRingWorkStarted();
    }

    private static void EnsureRingWorkStarted()
    {
        GameObject ringWork = ComponentUtil.FindIncludingInactive(RingWorkName);
        if (ringWork != null && ringWork.activeInHierarchy)
        {
            return;
        }

        GameObject sit = ComponentUtil.FindIncludingInactive(TimeMitaSitName);
        if (sit != null)
        {
            sit.SetActive(true);
            sit.GetComponent<Time_Events>()?.YieldRestart();
        }
        else
        {
            ringWork?.SetActive(true);
        }

        Plugin.Log.LogInfo("ensured RingWork start after give-ring dialogue", nameof(KappiRingStartSoftlockPatch));
    }

    private static bool IsKappiScene() => SceneManager.GetActiveScene().name == SceneName;
}
