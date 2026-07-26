using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

/// <summary>
/// Kappi Softlocks in Scene 7 - Backrooms (room-entry silence + ring-start + post-ring).
/// Room entry: StopAllTime on Cap door/StandUp timers, wake CapMita, ResetVoice.
/// Ring start: wake Quest4 so sit can enable RingWork (no early RingWork / StartAddon /
/// House hide). Connect green halo: HandHold enables UI Image "Alpha" (lime) +
/// AnimationParticle Check; finish calls UI_Alpha.AlphaZeroDeactivated — skip can
/// drop that, so Softlock Fix clears Alpha/Check at give-ring (KindMita 15).
/// Post-ring: Time Mita Stand only SetActives Quest5 after clip wait — skip can leave
/// Interactive TakeRing (Kind Mita) unreachable; Softlock Fix wakes Quest5 on stand /
/// ReadyTime. Door Softlock Fix is intentionally out of scope.
/// </summary>
[HarmonyPatch]
internal static class KappiRingSoftlockPatch
{
    private const string SceneName = "Scene 7 - Backrooms";

    private const string CapMitaGreetingName = "CapMita 1";
    private const int CapMitaGreetingIndex = 140;
    private const string SpeakCapMitaName = "Speak CapMita";
    private const string MitaCapName = "Mita Кепка";
    private const string StandUpEventsName = "TimeAnimationMitaK StandUp";
    private const string OpenDoorEventsName = "TimeAnimation MitaOpenDoor";
    private const string CapDoorEventsName = "MitaCap AnimDoor";

    private const string SitDialogueName = "KindMita 15";
    private const int SitDialogueIndex = 236;
    private const string TimeMitaSitName = "Time Mita Sit";
    private const string TimeMitaStandName = "Time Mita Stand";
    private const string RingWorkName = "RingWork";
    private const string Quest4Name = "Quest4 - Проводим время с Кепкой";
    private const string Quest5Name = "Quest5 - Пора уходить";
    private const string HandHoldAlphaName = "Alpha";
    private const string HandHoldCheckName = "AnimationParticle Check";

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Dialogue_3DText), "Start")]
    private static void CapMitaGreetingPostfix(Dialogue_3DText __instance)
    {
        if (!IsKappiScene())
        {
            if (__instance != null
                && __instance.gameObject.name == CapMitaGreetingName
                && __instance.indexString == CapMitaGreetingIndex)
            {
                KappiSoftlockDebugPatch.LogGateMiss(
                    nameof(KappiRingSoftlockPatch),
                    "IsKappiScene=false (active scene gate)",
                    __instance);
            }

            return;
        }

        if (__instance?.gameObject.name != CapMitaGreetingName || __instance.indexString != CapMitaGreetingIndex)
        {
            return;
        }

        GameObject standUp = GameObject.Find(StandUpEventsName);
        GameObject openDoor = GameObject.Find(OpenDoorEventsName);
        GameObject capDoor = GameObject.Find(CapDoorEventsName);
        standUp?.GetComponent<Time_Events>()?.StopAllTime();
        openDoor?.GetComponent<Time_Events>()?.StopAllTime();
        capDoor?.GetComponent<Time_Events>()?.StopAllTime();

        GameObject cap = ComponentUtil.FindIncludingInactive(MitaCapName);
        bool wokeCap = false;
        if (cap != null && !cap.activeSelf)
        {
            cap.SetActive(true);
            wokeCap = true;
        }

        GameObject speak = GameObject.Find(SpeakCapMitaName);
        speak?.GetComponent<AudioDialogue>()?.ResetVoice();

        KappiSoftlockDebugPatch.LogRepairAttempt(
            nameof(KappiRingSoftlockPatch),
            $"room-entry standUp={(standUp != null)} openDoor={(openDoor != null)} capDoor={(capDoor != null)} "
            + $"cap={(cap == null ? "null" : $"active={cap.activeSelf}")} wokeCap={wokeCap} "
            + $"speak={(speak != null)}");

        Plugin.Log.LogInfo("repaired CapMita room-entry greeting", nameof(KappiRingSoftlockPatch));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Dialogue_3DText), "Start")]
    private static void KindMitaGiveRingPostfix(Dialogue_3DText __instance)
    {
        if (!IsKappiScene())
        {
            return;
        }

        if (__instance?.gameObject.name != SitDialogueName || __instance.indexString != SitDialogueIndex)
        {
            return;
        }

        // HandHold (connect/check) is over by give-ring; clear overlay if skip dropped finish.
        string fx = ClearStuckHandHoldHalo();
        KappiSoftlockDebugPatch.LogRepairAttempt(
            nameof(KappiRingSoftlockPatch),
            $"give-ring halo clear {fx}");
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void TimeMitaSitOrStandYieldRestartPrefix(Time_Events __instance)
    {
        if (!IsKappiScene() || __instance == null)
        {
            return;
        }

        string name = __instance.gameObject.name;
        if (name == TimeMitaSitName)
        {
            EnsureQuest4ReadyForRingWork();
            ClearStuckHandHoldHalo();
            return;
        }

        if (name == TimeMitaStandName)
        {
            EnsureQuest5ReadyForTakeRing();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location7_RingWork), nameof(Location7_RingWork.ReadyTime))]
    private static void RingWorkReadyTimePostfix(Location7_RingWork __instance)
    {
        if (!IsKappiScene() || __instance == null)
        {
            return;
        }

        // eventReady YieldRestarts Time Mita Stand, whose clip-wait SetActives Quest5 —
        // Softlock Fix arms Quest5 here too if skip drops that wait.
        EnsureQuest5ReadyForTakeRing();
    }

    private static void EnsureQuest4ReadyForRingWork()
    {
        GameObject ringWork = ComponentUtil.FindIncludingInactive(RingWorkName);
        if (ringWork != null && ringWork.activeInHierarchy)
        {
            KappiSoftlockDebugPatch.LogRepairAttempt(
                nameof(KappiRingSoftlockPatch),
                "ring early-return: RingWork already activeInHierarchy");
            return;
        }

        GameObject quest4 = ComponentUtil.FindIncludingInactive(Quest4Name);
        if (quest4 == null)
        {
            Plugin.Log.LogWarning("Quest4 missing for ring Softlock Fix", nameof(KappiRingSoftlockPatch));
            return;
        }

        bool wokeQuest4 = false;
        if (!quest4.activeSelf)
        {
            quest4.SetActive(true);
            wokeQuest4 = true;
        }

        KappiSoftlockDebugPatch.LogRepairAttempt(
            nameof(KappiRingSoftlockPatch),
            $"ring wokeQuest4={wokeQuest4} "
            + $"quest4=active={quest4.activeSelf}/hier={quest4.activeInHierarchy} "
            + $"ringWork={(ringWork == null ? "null" : $"active={ringWork.activeSelf}/hier={ringWork.activeInHierarchy}")}");

        Plugin.Log.LogInfo(
            "armed Quest4 so sit timeline can start RingWork after sit",
            nameof(KappiRingSoftlockPatch));
    }

    private static void EnsureQuest5ReadyForTakeRing()
    {
        GameObject quest5 = ComponentUtil.FindIncludingInactive(Quest5Name);
        if (quest5 == null)
        {
            Plugin.Log.LogWarning("Quest5 missing for post-ring Softlock Fix", nameof(KappiRingSoftlockPatch));
            return;
        }

        if (quest5.activeInHierarchy)
        {
            KappiSoftlockDebugPatch.LogRepairAttempt(
                nameof(KappiRingSoftlockPatch),
                "post-ring early-return: Quest5 already activeInHierarchy");
            return;
        }

        bool wokeQuest5 = false;
        if (!quest5.activeSelf)
        {
            quest5.SetActive(true);
            wokeQuest5 = true;
        }

        GameObject takeRing = ComponentUtil.FindIncludingInactive("Interactive TakeRing");
        KappiSoftlockDebugPatch.LogRepairAttempt(
            nameof(KappiRingSoftlockPatch),
            $"post-ring wokeQuest5={wokeQuest5} "
            + $"quest5=active={quest5.activeSelf}/hier={quest5.activeInHierarchy} "
            + $"takeRing={(takeRing == null ? "null" : $"active={takeRing.activeSelf}/hier={takeRing.activeInHierarchy}")}");

        Plugin.Log.LogInfo(
            "armed Quest5 so Interactive TakeRing (Kind Mita) is reachable after ring",
            nameof(KappiRingSoftlockPatch));
    }

    /// <summary>
    /// Connect/check green halo is UI Image "Alpha" (lime, UI_Alpha) + AnimationParticle Check,
    /// armed by AnimationPlayer HandHold and normally cleared via AlphaZeroDeactivated on finish.
    /// Softlock Fix mirrors AlphaZeroInstant then deactivates both. Scene has many GOs named
    /// Alpha — only clear the one that owns UI_Alpha (HandHold overlay), not ScreenKick / others.
    /// </summary>
    private static string ClearStuckHandHoldHalo()
    {
        bool alphaZeroed = false;
        bool alphaDeactivated = false;
        foreach (UI_Alpha uiAlpha in Object.FindObjectsOfType<UI_Alpha>(true))
        {
            if (uiAlpha == null || uiAlpha.gameObject.name != HandHoldAlphaName)
            {
                continue;
            }

            uiAlpha.AlphaZeroInstant();
            alphaZeroed = true;
            if (uiAlpha.gameObject.activeSelf)
            {
                uiAlpha.gameObject.SetActive(false);
                alphaDeactivated = true;
            }
        }

        bool checkOff = false;
        GameObject check = ComponentUtil.FindIncludingInactive(HandHoldCheckName);
        if (check != null && check.activeSelf)
        {
            check.SetActive(false);
            checkOff = true;
        }

        return $"alphaZeroed={alphaZeroed} alphaDeactivated={alphaDeactivated} checkOff={checkOff}";
    }

    private static bool IsKappiScene() => SceneManager.GetActiveScene().name == SceneName;
}
