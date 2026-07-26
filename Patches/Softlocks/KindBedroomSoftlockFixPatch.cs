using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch]
internal static class KindBedroomSoftlockFixPatch
{
    private const string SceneName = "Scene 15 - BasementAndDeath";
    private const string TakeItemsName = "AnimationPlayer TakeItems";
    private const string StayMitaName = "AnimationPlayer StayMita";
    private const string StayUpEventsName = "TimeAnimation Mita StayUp";
    private const string MitaTakeItemsTimeName = "TimeAnimation Mita TakeItems";

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectAnimationPlayer), nameof(ObjectAnimationPlayer.AnimationPlay))]
    private static void StayMitaAnimationPlayPrefix(ObjectAnimationPlayer __instance)
    {
        if (__instance == null || __instance.gameObject.name != StayMitaName || !IsBasementScene())
        {
            return;
        }

        TryFinishTakeItemsHandoff("StayMita");
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void StayUpYieldRestartPrefix(Time_Events __instance)
    {
        if (__instance == null || __instance.gameObject.name != StayUpEventsName || !IsBasementScene())
        {
            return;
        }

        TryFinishTakeItemsHandoff("StayUp");
    }

    private static void TryFinishTakeItemsHandoff(string seam)
    {
        try
        {
            PlayerMove player = UnityEngine.Object.FindObjectOfType<PlayerMove>();
            if (player == null || !player.animationRun || !IsTakeItemsAnim(player))
            {
                return;
            }

            FindIncludingInactive(MitaTakeItemsTimeName)?.GetComponent<Time_Events>()?.StopAllTime();
            GameObject.Find(TakeItemsName)?.GetComponent<ObjectAnimationPlayer>()?.eventStartLoop?.Invoke();
            player.AnimationFastStop();
            Plugin.Log.LogInfo($"Kind bedroom Softlock Fix: finished TakeItems before {seam}");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Kind bedroom Softlock Fix ({seam}) failed: {ex}");
        }
    }

    private static bool IsBasementScene() => SceneManager.GetActiveScene().name == SceneName;

    private static bool IsTakeItemsAnim(PlayerMove player) =>
        player.scrAnimationNow != null
        && player.scrAnimationNow.gameObject.name == TakeItemsName;

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
