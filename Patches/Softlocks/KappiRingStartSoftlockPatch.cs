using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

/// <summary>
/// Ring Softlock: Time Mita Sit enables RingWork only after the sit clip, and RingWork lives
/// under Quest4. Skip can drop TextMita 29 → Встаёт → Location34.StartAddon, so Quest4 stays
/// inactive and RingWork never Starts. Bare SetActive(Quest4) half-boots House DoorCages
/// without eventStartAddon (ObjectDoor.Lock). Softlock Fix: call StartAddon instead.
/// </summary>
[HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
internal static class KappiRingStartSoftlockPatch
{
    private const string SceneName = "Scene 7 - Backrooms";
    private const string TimeMitaSitName = "Time Mita Sit";
    private const string RingWorkName = "RingWork";
    private const string Quest4Name = "Quest4 - Проводим время с Кепкой";

    [HarmonyPrefix]
    private static void YieldRestartPrefix(Time_Events __instance)
    {
        if (!IsKappiScene() || __instance == null)
        {
            return;
        }

        if (__instance.gameObject.name != TimeMitaSitName)
        {
            return;
        }

        EnsureQuest4StartedViaAddon();
    }

    private static void EnsureQuest4StartedViaAddon()
    {
        GameObject ringWork = ComponentUtil.FindIncludingInactive(RingWorkName);
        if (ringWork != null && ringWork.activeInHierarchy)
        {
            return;
        }

        GameObject quest4 = ComponentUtil.FindIncludingInactive(Quest4Name);
        if (quest4 == null)
        {
            Plugin.Log.LogWarning("Quest4 missing for ring Softlock Fix", nameof(KappiRingStartSoftlockPatch));
            return;
        }

        var comm = quest4.GetComponent<Location34_Communication>();
        if (comm == null)
        {
            Plugin.Log.LogWarning(
                "Location34_Communication missing on Quest4",
                nameof(KappiRingStartSoftlockPatch));
            return;
        }

        // StartAddon SetActive's Quest4, runs eventStartAddon (door Lock), then ActivationAddon.
        if (!quest4.activeInHierarchy)
        {
            comm.StartAddon();
            Plugin.Log.LogInfo(
                "StartAddon so sit timeline can start RingWork",
                nameof(KappiRingStartSoftlockPatch));
        }
    }

    private static bool IsKappiScene() => SceneManager.GetActiveScene().name == SceneName;
}
