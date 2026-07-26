using System;
using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch(typeof(Time_Events))]
internal static class SleepyDialogueSoftlockPatch
{
    private const string DreamerScene = "Scene 17 - Dreamer";
    private const string StandChairName = "AnimationMita StandChair";
    private const string TryChairName = "AnimationMita TryChair";

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Time_Events.YieldRestart))]
    private static void YieldRestartPrefix(Time_Events __instance)
    {
        if (!IsDreamerScene())
        {
            return;
        }

        try
        {
            if (__instance.gameObject.name != StandChairName)
            {
                return;
            }

            var tryChair = GameObject.Find(TryChairName);
            if (tryChair == null)
            {
                Plugin.Log.LogDebug("TryChair not found", nameof(SleepyDialogueSoftlockPatch));
                return;
            }

            TryStopChairTimers(tryChair);
            TryUnlockPlayer(tryChair);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"failed: {ex}", nameof(SleepyDialogueSoftlockPatch));
        }
    }

    private static void TryStopChairTimers(GameObject chair)
    {
        var events = chair.GetComponent<Time_Events>();
        if (events != null)
        {
            events.StopAllTime();
            Plugin.Log.LogInfo("stopped TryChair timers before StandChair", nameof(SleepyDialogueSoftlockPatch));
        }
        else
        {
            Plugin.Log.LogDebug("TryChair has no Time_Events", nameof(SleepyDialogueSoftlockPatch));
        }
    }

    private static void TryUnlockPlayer(GameObject animationObject)
    {
        var player = UnityEngine.Object.FindObjectOfType<PlayerMove>();
        if (player == null || !player.animationRun || player.scrAnimationNow == null)
        {
            Plugin.Log.LogDebug("player not locked in a chair anim", nameof(SleepyDialogueSoftlockPatch));
            return;
        }

        if (player.scrAnimationNow.gameObject != animationObject)
        {
            Plugin.Log.LogDebug(
                $"player anim is {player.scrAnimationNow.gameObject.name}, not TryChair",
                nameof(SleepyDialogueSoftlockPatch));
            return;
        }

        player.AnimationFastStop();
        Plugin.Log.LogInfo("AnimationFastStop on TryChair player lock", nameof(SleepyDialogueSoftlockPatch));
    }

    private static bool IsDreamerScene() => SceneManager.GetActiveScene().name == DreamerScene;
}
