using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

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
        Plugin.Log.LogInfo("repaired CapMita room-entry greeting", nameof(KappiRoomEntrySoftlockPatch));
    }

    private static bool IsKappiScene() => SceneManager.GetActiveScene().name == SceneName;
}
