using Colorful;
using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using VertexFragment;

namespace SpeedrunMod.Patches.Softlocks;

/// <summary>
/// Kappi Softlocks in Scene 7 - Backrooms (room-entry silence + ring-start).
/// Room entry: StopAllTime on Cap door/StandUp timers, wake CapMita, ResetVoice, clear
/// stuck green camera halo. Ring: wake Quest4 so sit can enable RingWork (no early
/// RingWork / StartAddon / House hide). Door Softlock Fix is intentionally out of scope.
/// </summary>
[HarmonyPatch]
internal static class KappiSoftlockPatch
{
    private const string SceneName = "Scene 7 - Backrooms";

    private const string CapMitaGreetingName = "CapMita 1";
    private const int CapMitaGreetingIndex = 140;
    private const string SpeakCapMitaName = "Speak CapMita";
    private const string MitaCapName = "Mita Кепка";
    private const string StandUpEventsName = "TimeAnimationMitaK StandUp";
    private const string OpenDoorEventsName = "TimeAnimation MitaOpenDoor";
    private const string CapDoorEventsName = "MitaCap AnimDoor";

    private const string TimeMitaSitName = "Time Mita Sit";
    private const string RingWorkName = "RingWork";
    private const string Quest4Name = "Quest4 - Проводим время с Кепкой";

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
                    nameof(KappiSoftlockPatch),
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
        string fx = ClearStuckCameraHalo();

        KappiSoftlockDebugPatch.LogRepairAttempt(
            nameof(KappiSoftlockPatch),
            $"room-entry standUp={(standUp != null)} openDoor={(openDoor != null)} capDoor={(capDoor != null)} "
            + $"cap={(cap == null ? "null" : $"active={cap.activeSelf}")} wokeCap={wokeCap} "
            + $"speak={(speak != null)} {fx}");

        Plugin.Log.LogInfo("repaired CapMita room-entry greeting", nameof(KappiSoftlockPatch));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void TimeMitaSitYieldRestartPrefix(Time_Events __instance)
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
                nameof(KappiSoftlockPatch),
                "ring early-return: RingWork already activeInHierarchy");
            return;
        }

        GameObject quest4 = ComponentUtil.FindIncludingInactive(Quest4Name);
        if (quest4 == null)
        {
            Plugin.Log.LogWarning("Quest4 missing for ring Softlock Fix", nameof(KappiSoftlockPatch));
            return;
        }

        bool wokeQuest4 = false;
        if (!quest4.activeSelf)
        {
            quest4.SetActive(true);
            wokeQuest4 = true;
        }

        KappiSoftlockDebugPatch.LogRepairAttempt(
            nameof(KappiSoftlockPatch),
            $"ring wokeQuest4={wokeQuest4} "
            + $"quest4=active={quest4.activeSelf}/hier={quest4.activeInHierarchy} "
            + $"ringWork={(ringWork == null ? "null" : $"active={ringWork.activeSelf}/hier={ringWork.activeInHierarchy}")}");

        Plugin.Log.LogInfo(
            "armed Quest4 so sit timeline can start RingWork after sit",
            nameof(KappiSoftlockPatch));
    }

    /// <summary>
    /// Decompiled: CameraVignetteActive(false) only sets fxFV and fades Darkness in Update.
    /// CameraSwitchTypeOutline(false) disables Sobel but enables OutlinesPostprocessed —
    /// Softlock Fix forces FastVignette off and disables both outline paths.
    /// </summary>
    private static string ClearStuckCameraHalo()
    {
        WorldPlayer worldPlayer = Object.FindObjectOfType<WorldPlayer>();
        bool vignetteFlagOff = false;
        if (worldPlayer != null)
        {
            worldPlayer.CameraVignetteActive(false);
            vignetteFlagOff = true;
        }

        bool vignetteForcedOff = false;
        foreach (FastVignette vignette in Object.FindObjectsOfType<FastVignette>(true))
        {
            if (vignette == null)
            {
                continue;
            }

            vignette.Darkness = 0f;
            vignette.enabled = false;
            vignetteForcedOff = true;
        }

        bool outlinesOff = false;
        foreach (OutlinesPostprocessed outlines in Object.FindObjectsOfType<OutlinesPostprocessed>(true))
        {
            if (outlines != null && outlines.enabled)
            {
                outlines.enabled = false;
                outlinesOff = true;
            }
        }

        bool sobelOff = false;
        foreach (PostProcessVolume volume in Object.FindObjectsOfType<PostProcessVolume>(true))
        {
            if (volume == null || volume.profile == null)
            {
                continue;
            }

            if (volume.profile.TryGetSettings(out SobelOutline sobel) && sobel != null && sobel.enabled)
            {
                sobel.enabled.value = false;
                sobelOff = true;
            }
        }

        return $"vignetteFlagOff={vignetteFlagOff} vignetteForcedOff={vignetteForcedOff} "
            + $"outlinesPostOff={outlinesOff} sobelOff={sobelOff}";
    }

    private static bool IsKappiScene() => SceneManager.GetActiveScene().name == SceneName;
}
