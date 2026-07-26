using Colorful;
using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

/// <summary>
/// Room-entry Softlock: fast enter + Space-skip races CapMita 1 against door/StandUp
/// Time_Events that still own CapMita's animator/audio. Softlock Fix stops those timers,
/// wakes CapMita, resets voice, finishes Beyond's Door InRoom destroy (TextMita 7
/// ActiveObject — skip can leave that door stuck open beside Cap's DoorCage Bedroom-Hall),
/// and force-clears FastVignette / SobelOutline (CameraVignetteActive(false) only fades).
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
    /// <summary>Beyond→Kappi doorway door. TextMita 7 finish starts Animator_OneTimeDestroy on it.</summary>
    private const string BeyondDoorInRoomName = "Door InRoom";
    private const string BeyondDoorCageInRoomName = "Doorcage InRoom";

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
        string beyondDoor = RemoveBeyondDoorInRoom();
        string fx = ClearStuckLookEffects();

        KappiSoftlockDebugPatch.LogRepairAttempt(
            nameof(KappiRoomEntrySoftlockPatch),
            $"standUp={(standUp != null)} openDoor={(openDoor != null)} capDoor={(capDoor != null)} "
            + $"cap={(cap == null ? "null" : $"active={cap.activeSelf}")} wokeCap={wokeCap} "
            + $"speak={(speak != null)} {beyondDoor} {fx}");

        Plugin.Log.LogInfo("repaired CapMita room-entry greeting", nameof(KappiRoomEntrySoftlockPatch));
    }

    private static string RemoveBeyondDoorInRoom()
    {
        GameObject door = ComponentUtil.FindIncludingInactive(BeyondDoorInRoomName);
        GameObject cage = ComponentUtil.FindIncludingInactive(BeyondDoorCageInRoomName);

        bool destroyedDoor = false;
        if (door != null)
        {
            // Vanilla: TextMita 7 → Animator_OneTimeDestroy.ActiveObject (destroy anim).
            // Skip can leave Door InRoom stuck open in the Cap doorway beside DoorCage Bedroom-Hall.
            var oneTime = door.GetComponent<Animator_OneTimeDestroy>();
            if (oneTime != null)
            {
                oneTime.ActiveObject();
                oneTime.Finish();
            }

            Object.Destroy(door);
            destroyedDoor = true;
        }

        bool hidCage = false;
        if (cage != null && cage.activeSelf)
        {
            cage.SetActive(false);
            hidCage = true;
        }

        return $"beyondDoorInRoom={(door == null ? "null" : $"destroyed={destroyedDoor}")} "
            + $"beyondDoorCage={(cage == null ? "null" : $"hid={hidCage}/active={cage.activeSelf}")}";
    }

    private static string ClearStuckLookEffects()
    {
        PlayerMove player = Object.FindObjectOfType<PlayerMove>();
        bool clearedCast = false;
        if (player != null && player.objectCastInteractive != null)
        {
            player.objectCastInteractive = null;
            clearedCast = true;
        }

        WorldPlayer worldPlayer = Object.FindObjectOfType<WorldPlayer>();
        bool vignetteFlagOff = false;
        bool vignetteForcedOff = false;
        bool sobelOff = false;
        if (worldPlayer != null)
        {
            // Flag only starts a slow Darkness fade — force-disable FastVignette immediately.
            worldPlayer.CameraVignetteActive(false);
            vignetteFlagOff = true;
            worldPlayer.CameraSwitchTypeOutline(false);
            sobelOff = true;
        }

        FastVignette vignette = Object.FindObjectOfType<FastVignette>();
        if (vignette != null)
        {
            vignette.Darkness = 0f;
            vignette.enabled = false;
            vignetteForcedOff = true;
        }

        return $"clearedCast={clearedCast} vignetteFlagOff={vignetteFlagOff} "
            + $"vignetteForcedOff={vignetteForcedOff} sobelOutlineOff={sobelOff}";
    }

    private static bool IsKappiScene() => SceneManager.GetActiveScene().name == SceneName;
}
