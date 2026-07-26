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
    private const string StopNearEventsName = "TimeAnimationMita StopNear";
    private const string CanvasKickName = "Canvas Kick";
    private const string Quest2StartName = "Quest 2 Start";
    private const string HoldHeadDialogueName = "Mita 4";
    private const int HoldHeadDialogueIndex = 118;
    private const float KickEventTime = 1f;

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
            switch (__instance.gameObject.name)
            {
                case HoldHeadBatEventsName:
                    StopTimedEvents(TakeBatEventsName);
                    Plugin.Log.LogInfo("Baseball Softlock Fix: cleared TakeBat before HoldHeadBat");
                    break;
                case StartNearEventsName:
                    StopTimedEvents(HoldHeadBatEventsName);
                    StopTimedEvents(TakeBatEventsName);
                    Plugin.Log.LogInfo("Baseball Softlock Fix: cleared bat timers before StartNear");
                    break;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Baseball Softlock Fix (YieldRestart) failed: {ex}");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void YieldRestartPostfix(Time_Events __instance)
    {
        if (!IsMobilePlayerScene() || __instance == null || __instance.gameObject.name != StopNearEventsName)
        {
            return;
        }

        try
        {
            TryForceStopNearKick(__instance);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Baseball Softlock Fix (StopNear Kick) failed: {ex}");
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

    private static void TryForceStopNearKick(Time_Events stopNear)
    {
        GameObject canvasKick = FindIncludingInactive(CanvasKickName);
        if (canvasKick != null && canvasKick.activeInHierarchy)
        {
            return;
        }

        GameObject quest2 = FindIncludingInactive(Quest2StartName);
        if (quest2 != null && quest2.activeInHierarchy)
        {
            return;
        }

        TimePoint kick = FindTimePoint(stopNear, KickEventTime);
        if (kick?._event == null)
        {
            Plugin.Log.LogWarning("Baseball Softlock Fix: StopNear Kick TimePoint missing");
            return;
        }

        stopNear.StopAllTime();
        kick._event.Invoke();
        Plugin.Log.LogInfo("Baseball Softlock Fix: invoked StopNear Kick TimePoint");
    }

    private static TimePoint FindTimePoint(Time_Events events, float time)
    {
        var points = events?.EventsOnTime;
        if (points == null)
        {
            return null;
        }

        for (int i = 0; i < points.Length; i++)
        {
            TimePoint point = points[i];
            if (point != null && Mathf.Approximately(point.time, time))
            {
                return point;
            }
        }

        return null;
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
