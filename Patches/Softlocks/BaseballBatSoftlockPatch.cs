using System;
using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch]
internal static class BaseballBatSoftlockPatch
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
                    TimeUtil.StopTimeEvents(TakeBatEventsName);
                    Plugin.Log.LogInfo("cleared TakeBat before HoldHeadBat", nameof(BaseballBatSoftlockPatch));
                    break;
                case StartNearEventsName:
                    TimeUtil.StopTimeEvents(HoldHeadBatEventsName);
                    TimeUtil.StopTimeEvents(TakeBatEventsName);
                    Plugin.Log.LogInfo("cleared bat timers before StartNear", nameof(BaseballBatSoftlockPatch));
                    break;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"YieldRestart failed: {ex}", nameof(BaseballBatSoftlockPatch));
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
            Plugin.Log.LogError($"StopNear Kick failed: {ex}", nameof(BaseballBatSoftlockPatch));
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
            TimeUtil.StopTimeEvents(TakeBatEventsName);
            Plugin.Log.LogInfo("cleared TakeBat on Mita 4/118", nameof(BaseballBatSoftlockPatch));
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Mita 4 failed: {ex}", nameof(BaseballBatSoftlockPatch));
        }
    }

    private static void TryForceStopNearKick(Time_Events stopNear)
    {
        GameObject canvasKick = ComponentUtil.FindIncludingInactive(CanvasKickName);
        if (canvasKick != null && canvasKick.activeInHierarchy)
        {
            return;
        }

        GameObject quest2 = ComponentUtil.FindIncludingInactive(Quest2StartName);
        if (quest2 != null && quest2.activeInHierarchy)
        {
            return;
        }

        TimePoint kick = FindTimePoint(stopNear, KickEventTime);
        if (kick?._event == null)
        {
            Plugin.Log.LogWarning("StopNear Kick TimePoint missing", nameof(BaseballBatSoftlockPatch));
            return;
        }

        stopNear.StopAllTime();
        kick._event.Invoke();
        Plugin.Log.LogInfo("invoked StopNear Kick TimePoint", nameof(BaseballBatSoftlockPatch));
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

    private static bool IsMobilePlayerScene() => SceneManager.GetActiveScene().name == SceneName;
}
