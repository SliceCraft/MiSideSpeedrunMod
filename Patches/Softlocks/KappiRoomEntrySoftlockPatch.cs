using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

/// <summary>
/// Room-entry Softlock: fast enter + Space-skip races CapMita 1 against door/StandUp
/// Time_Events that still own CapMita's animator/audio. Softlock Fix stops those timers,
/// wakes CapMita, resets voice, and snaps the bedroom-hall door closed so skip cannot leave
/// DoorCage Bedroom-Hall stuck open (duplicate open door / edge outline).
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
    private const string BedroomHallDoorCageName = "DoorCage Bedroom-Hall";

    [HarmonyPostfix]
    private static void StartPostfix(Dialogue_3DText __instance)
    {
        if (!IsKappiScene())
        {
            return;
        }

        if (__instance?.gameObject.name != CapMitaGreetingName || __instance.indexString != CapMitaGreetingIndex)
        {
            return;
        }

        GameObject.Find(StandUpEventsName)?.GetComponent<Time_Events>()?.StopAllTime();
        GameObject.Find(OpenDoorEventsName)?.GetComponent<Time_Events>()?.StopAllTime();
        GameObject.Find(CapDoorEventsName)?.GetComponent<Time_Events>()?.StopAllTime();

        GameObject cap = ComponentUtil.FindIncludingInactive(MitaCapName);
        if (cap != null && !cap.activeSelf)
        {
            cap.SetActive(true);
        }

        GameObject.Find(SpeakCapMitaName)?.GetComponent<AudioDialogue>()?.ResetVoice();
        CloseBedroomHallDoor();

        Plugin.Log.LogInfo("repaired CapMita room-entry greeting", nameof(KappiRoomEntrySoftlockPatch));
    }

    private static void CloseBedroomHallDoor()
    {
        GameObject cage = ComponentUtil.FindIncludingInactive(BedroomHallDoorCageName);
        if (cage == null)
        {
            return;
        }

        ObjectDoor door = cage.GetComponentInChildren<ObjectDoor>(true);
        if (door == null)
        {
            return;
        }

        door.AnimationStop();
        door.ResetOriginRotation();
        door.open = false;
    }

    private static bool IsKappiScene() => SceneManager.GetActiveScene().name == SceneName;
}
