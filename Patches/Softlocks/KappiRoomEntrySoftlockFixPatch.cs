using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch(typeof(Dialogue_3DText), "Start")]
internal static class KappiRoomEntrySoftlockFixPatch
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

        Stop(StandUpEventsName);
        Stop(OpenDoorEventsName);
        Stop(CapDoorEventsName);

        GameObject cap = FindIncludingInactive(MitaCapName);
        if (cap != null && !cap.activeSelf)
        {
            cap.SetActive(true);
        }

        GameObject.Find(SpeakCapMitaName)?.GetComponent<AudioDialogue>()?.ResetVoice();
        Plugin.Log.LogInfo("Kappi Softlock Fix: repaired CapMita room-entry greeting");
    }

    private static bool IsKappiScene() => SceneManager.GetActiveScene().name == SceneName;

    private static void Stop(string name) => GameObject.Find(name)?.GetComponent<Time_Events>()?.StopAllTime();

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
