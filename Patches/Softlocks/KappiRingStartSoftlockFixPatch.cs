using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch(typeof(Dialogue_3DText), "Start")]
internal static class KappiRingStartSoftlockFixPatch
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
        GameObject ringWork = FindIncludingInactive(RingWorkName);
        if (ringWork != null && ringWork.activeInHierarchy)
        {
            return;
        }

        GameObject sit = FindIncludingInactive(TimeMitaSitName);
        if (sit != null)
        {
            sit.SetActive(true);
            sit.GetComponent<Time_Events>()?.YieldRestart();
        }
        else
        {
            ringWork?.SetActive(true);
        }

        Plugin.Log.LogInfo("Kappi Softlock Fix: ensured RingWork start after give-ring dialogue");
    }

    private static bool IsKappiScene() => SceneManager.GetActiveScene().name == SceneName;

    private static GameObject FindIncludingInactive(string name)
    {
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t != null && t.gameObject.name == name)
            {
                return t.gameObject;
            }
        }

        return null;
    }
}
