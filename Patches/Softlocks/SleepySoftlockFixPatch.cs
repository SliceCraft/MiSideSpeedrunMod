using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

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
                Plugin.Log.LogDebug("Sleepy Softlock Fix: TryChair not found");
                return;
            }

            var tryChairEvents = tryChair.GetComponent<Time_Events>();
            if (tryChairEvents != null)
            {
                // Pending TryChair timers (esp. late WakeUp Idle) race StandChair under fast skip.
                tryChairEvents.StopAllTime();
                Plugin.Log.LogInfo("Sleepy Softlock Fix: stopped TryChair timers before StandChair");
            }
            else
            {
                Plugin.Log.LogDebug("Sleepy Softlock Fix: TryChair has no Time_Events");
            }

            // Click also locks the player in TryChair's ObjectAnimationPlayer (Player WakeUp).
            // Dialogue skip does not end that anim; StandChair leaves the player frozen.
            UnlockTryChairPlayer(tryChair);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Sleepy Softlock Fix failed: {ex}");
        }
    }

    private static void UnlockTryChairPlayer(GameObject tryChair)
    {
        var player = Object.FindObjectOfType<PlayerMove>();
        if (player == null || !player.animationRun || player.scrAnimationNow == null)
        {
            Plugin.Log.LogDebug("Sleepy Softlock Fix: player not locked in a chair anim");
            return;
        }

        if (player.scrAnimationNow.gameObject != tryChair)
        {
            Plugin.Log.LogDebug(
                $"Sleepy Softlock Fix: player anim is {player.scrAnimationNow.gameObject.name}, not TryChair");
            return;
        }

        player.AnimationFastStop();
        Plugin.Log.LogInfo("Sleepy Softlock Fix: AnimationFastStop on TryChair player lock");
    }

    private static bool IsDreamerScene() => SceneManager.GetActiveScene().name == DreamerScene;
}
