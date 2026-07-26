using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

/// <summary>
/// Room-entry Softlock: fast enter + Space-skip races CapMita 1 against door/StandUp
/// Time_Events that still own CapMita's animator/audio. Softlock Fix stops those timers,
/// wakes CapMita, resets voice, snaps CapMita's DoorCage Bedroom-Hall closed (do not hide —
/// DoorCage Bedroom - Hall under Doors is destroyed when Cap opens hers), and clears the
/// stuck SobelOutline / FastVignette green halo from the interrupted open-door beat.
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
    /// <summary>CapMita-opened cage (hyphen). Permanent "DoorCage Bedroom - Hall" is destroyed on open.</summary>
    private const string CapMitaBedroomHallDoorCageName = "DoorCage Bedroom-Hall";

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
        string door = CloseCapMitaBedroomHallDoor();
        string fx = ClearStuckLookEffects();

        KappiSoftlockDebugPatch.LogRepairAttempt(
            nameof(KappiRoomEntrySoftlockPatch),
            $"standUp={(standUp != null)} openDoor={(openDoor != null)} capDoor={(capDoor != null)} "
            + $"cap={(cap == null ? "null" : $"active={cap.activeSelf}")} wokeCap={wokeCap} "
            + $"speak={(speak != null)} {door} {fx}");

        Plugin.Log.LogInfo("repaired CapMita room-entry greeting", nameof(KappiRoomEntrySoftlockPatch));
    }

    private static string CloseCapMitaBedroomHallDoor()
    {
        GameObject cage = ComponentUtil.FindIncludingInactive(CapMitaBedroomHallDoorCageName);
        if (cage == null)
        {
            return "closedCapDoor=false (cage null)";
        }

        // Cap open destroys DoorCage Bedroom - Hall; keep this cage, only snap the hinge shut.
        if (!cage.activeSelf)
        {
            cage.SetActive(true);
        }

        ObjectDoor door = cage.GetComponentInChildren<ObjectDoor>(true);
        if (door == null)
        {
            return $"closedCapDoor=false cageActive={cage.activeSelf} (door null)";
        }

        door.AnimationStop();
        door.ResetOriginRotation();
        door.open = false;
        door.LockSharply();

        return $"closedCapDoor=True cageActive={cage.activeSelf} doorOpen={door.open}";
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
        bool vignetteOff = false;
        bool sobelOff = false;
        if (worldPlayer != null)
        {
            // FastVignette (rarely the green edge) + SobelOutline (green full-screen halo).
            worldPlayer.CameraVignetteActive(false);
            vignetteOff = true;
            worldPlayer.CameraSwitchTypeOutline(false);
            sobelOff = true;
        }

        return $"clearedCast={clearedCast} vignetteOff={vignetteOff} sobelOutlineOff={sobelOff}";
    }

    private static bool IsKappiScene() => SceneManager.GetActiveScene().name == SceneName;
}
