using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches;

/// <summary>
/// Sleepy Softlock Fix (Scene 17 - Dreamer): fast dialogue skip can finish the
/// chair dialogue chain and start <c>AnimationMita StandChair</c> while
/// <c>AnimationMita TryChair</c> still has pending timed events (notably
/// late WakeUp Idle). Those events fight StandChair's stand-up/quest unlock
/// and Softlock the chapter.
///
/// Repair: when StandChair restarts, stop TryChair timers and flush its
/// remaining timed side-effects so skip stays usable.
/// </summary>
[HarmonyPatch(typeof(Time_Events))]
internal static class SleepySoftlockFixPatch
{
    private const string DreamerScene = "Scene 17 - Dreamer";
    private const string StandChairName = "AnimationMita StandChair";
    private const string TryChairName = "AnimationMita TryChair";

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Time_Events.YieldRestart))]
    private static void YieldRestartPrefix(Time_Events __instance)
    {
        if (SceneManager.GetActiveScene().name != DreamerScene)
        {
            return;
        }

        if (__instance == null || __instance.gameObject.name != StandChairName)
        {
            return;
        }

        var tryChair = GameObject.Find(TryChairName);
        if (tryChair == null)
        {
            return;
        }

        var tryChairEvents = tryChair.GetComponent<Time_Events>();
        if (tryChairEvents == null)
        {
            return;
        }

        tryChairEvents.StopAllTime();
        FlushTimedEvents(tryChairEvents);
        Plugin.Log.LogInfo("Sleepy Softlock Fix: completed TryChair before StandChair");
    }

    private static void FlushTimedEvents(Time_Events timeEvents)
    {
        var eventsOnTime = timeEvents.EventsOnTime;
        if (eventsOnTime == null)
        {
            return;
        }

        for (var i = 0; i < eventsOnTime.Length; i++)
        {
            eventsOnTime[i]?._event?.Invoke();
        }
    }
}
