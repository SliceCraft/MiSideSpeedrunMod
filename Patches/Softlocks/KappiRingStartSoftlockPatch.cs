using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

/// <summary>
/// Ring Softlock: Time Mita Sit enables RingWork after the sit clip, but RingWork lives under
/// Quest4/Игры. Skip can leave Quest4 inactive (or drop the sit wait) so RingWork never Starts.
/// Softlock Fix wakes Quest4, keeps Quest4/House off (duplicate DoorCages), and SetActive's
/// RingWork directly. Do not call StartAddon here — vanilla Встаёт does that later.
/// </summary>
[HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
internal static class KappiRingStartSoftlockPatch
{
    private const string SceneName = "Scene 7 - Backrooms";
    private const string TimeMitaSitName = "Time Mita Sit";
    private const string RingWorkName = "RingWork";
    private const string Quest4Name = "Quest4 - Проводим время с Кепкой";
    private const string Quest4HouseName = "House";

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

        EnsureQuest4ReadyForRingWork();
    }

    private static void EnsureQuest4ReadyForRingWork()
    {
        GameObject ringWork = ComponentUtil.FindIncludingInactive(RingWorkName);
        if (ringWork != null && ringWork.activeInHierarchy)
        {
            KappiSoftlockDebugPatch.LogRepairAttempt(
                nameof(KappiRingStartSoftlockPatch),
                "early-return: RingWork already activeInHierarchy");
            return;
        }

        GameObject quest4 = ComponentUtil.FindIncludingInactive(Quest4Name);
        if (quest4 == null)
        {
            Plugin.Log.LogWarning("Quest4 missing for ring Softlock Fix", nameof(KappiRingStartSoftlockPatch));
            return;
        }

        bool wokeQuest4 = false;
        if (!quest4.activeSelf)
        {
            quest4.SetActive(true);
            wokeQuest4 = true;
        }

        // Quest4/House DoorCages duplicate main Doors; keep House off until vanilla StartAddon
        // (RingWork lives under Quest4/Игры, not House).
        Transform house = quest4.transform.Find(Quest4HouseName);
        bool hidHouse = false;
        if (house != null && house.gameObject.activeSelf)
        {
            house.gameObject.SetActive(false);
            hidHouse = true;
        }

        // Sit EventsOnTime SetActive(RingWork) after clip wait — skip can drop that wait.
        bool wokeRingWork = false;
        if (ringWork != null && !ringWork.activeSelf)
        {
            ringWork.SetActive(true);
            wokeRingWork = true;
        }

        KappiSoftlockDebugPatch.LogRepairAttempt(
            nameof(KappiRingStartSoftlockPatch),
            $"wokeQuest4={wokeQuest4} hidHouse={hidHouse} wokeRingWork={wokeRingWork} "
            + $"quest4=active={quest4.activeSelf}/hier={quest4.activeInHierarchy} "
            + $"ringWork={(ringWork == null ? "null" : $"active={ringWork.activeSelf}/hier={ringWork.activeInHierarchy}")}");

        Plugin.Log.LogInfo(
            "armed Quest4 (House off) and RingWork so sit Softlock cannot strand the ring job",
            nameof(KappiRingStartSoftlockPatch));
    }

    private static bool IsKappiScene() => SceneManager.GetActiveScene().name == SceneName;
}
