using Colorful;
using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using VertexFragment;

namespace SpeedrunMod.Patches.Softlocks;

/// <summary>
/// Room-entry Softlock: fast enter + Space-skip races CapMita 1 against door/StandUp
/// Time_Events that still own CapMita's animator/audio. Softlock Fix stops those timers,
/// wakes CapMita, resets voice, and clears stuck green camera halo (FastVignette + both
/// outline paths). Door Softlock Fix is intentionally not handled here.
/// </summary>
[HarmonyPatch(typeof(Dialogue_3DText), "Start")]
internal static class KappiRoomEntrySoftlockPatch
{
    private const string SceneName = "Scene 7 - Backrooms";
    private const string CapMitaGreetingName = "CapMita 1";
    private const int CapMitaGreetingIndex = 140;
    private const string SpeakCapMitaName = "Speak CapMita";
    private const string MitaCapName = "Mita Кепка";
    private const string StandUpEventsName = "TimeAnimationMitaK StandUp";
    private const string OpenDoorEventsName = "TimeAnimation MitaOpenDoor";
    private const string CapDoorEventsName = "MitaCap AnimDoor";

    [HarmonyPostfix]
    private static void StartPostfix(Dialogue_3DText __instance)
    {
        if (!IsKappiScene())
        {
            if (__instance != null
                && __instance.gameObject.name == CapMitaGreetingName
                && __instance.indexString == CapMitaGreetingIndex)
            {
                KappiSoftlockDebugPatch.LogGateMiss(
                    nameof(KappiRoomEntrySoftlockPatch),
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
            nameof(KappiRoomEntrySoftlockPatch),
            $"standUp={(standUp != null)} openDoor={(openDoor != null)} capDoor={(capDoor != null)} "
            + $"cap={(cap == null ? "null" : $"active={cap.activeSelf}")} wokeCap={wokeCap} "
            + $"speak={(speak != null)} {fx}");

        Plugin.Log.LogInfo("repaired CapMita room-entry greeting", nameof(KappiRoomEntrySoftlockPatch));
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

        // Softlock Fix both outline modes off (false enables OutlinesPostprocessed).
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
