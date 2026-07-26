using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

/// <summary>
/// Kind Mita "baseball" / reset Softlock: spam-skip through KindMita 17–20 while she is
/// seated after MitaD Drop leaves sit <see cref="Time_Events"/> running; those late sit
/// events fight <c>МитаДобрая Встаёт обратно</c> and she stays seated / scene stalls.
/// </summary>
[HarmonyPatch]
internal static class BaseballSoftlockFixPatch
{
    private const string SceneName = "Scene 7 - Backrooms";
    private const string StandUpEventsName = "МитаДобрая Встаёт обратно";
    private const string SitAfterDropEventsName = "TimeAnimation MitaK Sit";
    private const string DropEventsName = "TimeAnimation MitaD Drop";
    private const string AnimationSitEventsName = "AnimationMita Sit";
    private const string StandDialogueName = "KindMita 20";
    private const int StandDialogueIndex = 169;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void YieldRestartPrefix(Time_Events __instance)
    {
        if (!IsBackroomsScene() || __instance == null || __instance.gameObject.name != StandUpEventsName)
        {
            return;
        }

        try
        {
            ClearCompetingSitTimers();
            Plugin.Log.LogInfo("Baseball Softlock Fix: cleared sit/drop timers before Kind Mita stand-up");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Baseball Softlock Fix (stand-up prefix) failed: {ex}");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Dialogue_3DText), "Start")]
    private static void DialogueStartPostfix(Dialogue_3DText __instance)
    {
        if (!IsBackroomsScene())
        {
            return;
        }

        if (__instance?.gameObject.name != StandDialogueName || __instance.indexString != StandDialogueIndex)
        {
            return;
        }

        try
        {
            // Clear sits as soon as the stand-up line appears, before eventFinish YieldRestart.
            ClearCompetingSitTimers();
            Plugin.Log.LogInfo("Baseball Softlock Fix: cleared sit/drop timers on KindMita 20");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Baseball Softlock Fix (KindMita 20) failed: {ex}");
        }
    }

    private static void ClearCompetingSitTimers()
    {
        StopTimedEvents(SitAfterDropEventsName);
        StopTimedEvents(DropEventsName);
        StopTimedEvents(AnimationSitEventsName);
    }

    private static void StopTimedEvents(string name)
    {
        GameObject go = FindIncludingInactive(name);
        if (go == null)
        {
            return;
        }

        var events = go.GetComponent<Time_Events>();
        if (events == null)
        {
            return;
        }

        events.StopAllTime();
        Plugin.Log.LogDebug($"Baseball Softlock Fix: StopAllTime on {name}");
    }

    private static bool IsBackroomsScene() => SceneManager.GetActiveScene().name == SceneName;

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
