using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch]
internal static class BaseballSoftlockFixPatch
{
    private const string SceneName = "Scene 14 - MobilePlayer";
    private const string TakeBatEventsName = "TimeAnimationMita TakeBat";
    private const string HoldHeadBatEventsName = "TimeAnimationMita HoldHeadBat";
    private const string StartNearEventsName = "TimeAnimationMita StartNear";
    private const string HoldHeadDialogueName = "Mita 4";
    private const int HoldHeadDialogueIndex = 118;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void YieldRestartPrefix(Time_Events __instance)
    {
        if (!IsMobilePlayerScene() || __instance == null)
        {
            return;
        }

        try
        {
            string name = __instance.gameObject.name;
            if (name == HoldHeadBatEventsName)
            {
                StopTimedEvents(TakeBatEventsName);
                Plugin.Log.LogInfo("Baseball Softlock Fix: cleared TakeBat before HoldHeadBat");
            }
            else if (name == StartNearEventsName)
            {
                StopTimedEvents(HoldHeadBatEventsName);
                StopTimedEvents(TakeBatEventsName);
                Plugin.Log.LogInfo("Baseball Softlock Fix: cleared bat timers before StartNear");
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Baseball Softlock Fix (YieldRestart) failed: {ex}");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Dialogue_3DText), "Start")]
    private static void DialogueStartPostfix(Dialogue_3DText __instance)
    {
        if (!IsMobilePlayerScene())
        {
            return;
        }

        if (__instance?.gameObject.name != HoldHeadDialogueName || __instance.indexString != HoldHeadDialogueIndex)
        {
            return;
        }

        try
        {
            StopTimedEvents(TakeBatEventsName);
            Plugin.Log.LogInfo("Baseball Softlock Fix: cleared TakeBat on Mita 4/118");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Baseball Softlock Fix (Mita 4) failed: {ex}");
        }
    }

    private static void StopTimedEvents(string name)
    {
        GameObject go = FindIncludingInactive(name);
        go?.GetComponent<Time_Events>()?.StopAllTime();
    }

    private static bool IsMobilePlayerScene() => SceneManager.GetActiveScene().name == SceneName;

    private static GameObject FindIncludingInactive(string name)
    {
        foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t != null && t.gameObject.name == name)
            {
                return t.gameObject;
            }
        }

        return null;
    }
}
