using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

/// <summary>
/// Scene 15 bedroom paper Softlock: fast-skip through post-TakeItems dialogue can fire
/// StayMita / StayUp while <c>AnimationPlayer TakeItems</c> and Mita's TakeItems
/// <see cref="Time_Events"/> are still live, so she bugs out and the beat stalls.
/// Dialogue container is named KindMita; speaker is Future Mita handing PaperPassword.
/// </summary>
[HarmonyPatch]
internal static class KindMitaBedroomPaperSoftlockFixPatch
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

        try
        {
            if (!IsPlayerOnTakeItems())
            {
                return;
            }

            FinishTakeItemsHandoff();
            Plugin.Log.LogInfo("Kind Mita bedroom Softlock Fix: finished TakeItems before StayMita");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Kind Mita bedroom Softlock Fix (StayMita) failed: {ex}");
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void StayUpYieldRestartPrefix(Time_Events __instance)
    {
        if (__instance == null || __instance.gameObject.name != StayUpEventsName || !IsBasementScene())
        {
            return;
        }

        try
        {
            if (!IsPlayerOnTakeItems())
            {
                return;
            }

            FinishTakeItemsHandoff();
            Plugin.Log.LogInfo("Kind Mita bedroom Softlock Fix: finished TakeItems before StayUp");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Kind Mita bedroom Softlock Fix (StayUp) failed: {ex}");
        }
    }

    private static void FinishTakeItemsHandoff()
    {
        FindIncludingInactive(MitaTakeItemsTimeName)?.GetComponent<Time_Events>()?.StopAllTime();

        ObjectAnimationPlayer take = GameObject.Find(TakeItemsName)?.GetComponent<ObjectAnimationPlayer>();
        take?.eventStartLoop?.Invoke();

        PlayerMove player = UnityEngine.Object.FindObjectOfType<PlayerMove>();
        if (player != null && player.animationRun && IsTakeItemsAnim(player))
        {
            player.AnimationFastStop();
        }
    }

    private static bool IsBasementScene() => SceneManager.GetActiveScene().name == SceneName;

    private static bool IsTakeItemsAnim(PlayerMove player) =>
        player?.scrAnimationNow != null
        && player.scrAnimationNow.gameObject.name == TakeItemsName;

    private static bool IsPlayerOnTakeItems()
    {
        PlayerMove player = UnityEngine.Object.FindObjectOfType<PlayerMove>();
        return player != null && player.animationRun && IsTakeItemsAnim(player);
    }

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
