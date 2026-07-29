using System;
using HarmonyLib;
using SpeedrunMod.Configs;
using SpeedrunMod.Notifications;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch]
internal static class KindBedroomPaperSoftlockPatch
{
    private const string SceneName = "Scene 15 - BasementAndDeath";
    private const string TakeItemsName = "AnimationPlayer TakeItems";
    private const string StayMitaName = "AnimationPlayer StayMita";
    private const string StayUpEventsName = "TimeAnimation Mita StayUp";
    private const string MitaTakeItemsTimeName = "TimeAnimation Mita TakeItems";
    private const string Notification = "Softlock Fix: Kind bedroom paper";

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectAnimationPlayer), nameof(ObjectAnimationPlayer.AnimationPlay))]
    private static void StayMitaAnimationPlayPrefix(ObjectAnimationPlayer __instance)
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableKindBedroomPaper))
        {
            return;
        }

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
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableKindBedroomPaper))
        {
            return;
        }

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

            TimeUtil.StopTimeEvents(MitaTakeItemsTimeName);
            ComponentUtil.FindIncludingInactive<ObjectAnimationPlayer>(TakeItemsName)?.eventStartLoop?.Invoke();
            player.AnimationFastStop();
            NotificationManager.Show(new NotificationMessage(Notification, cooldown: 5f));
            Plugin.Log.LogInfo($"finished TakeItems before {seam}", nameof(KindBedroomPaperSoftlockPatch));
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"{seam} failed: {ex}", nameof(KindBedroomPaperSoftlockPatch));
        }
    }

    private static bool IsBasementScene() => SceneManager.GetActiveScene().name == SceneName;

    private static bool IsTakeItemsAnim(PlayerMove player) =>
        player.scrAnimationNow != null
        && player.scrAnimationNow.gameObject.name == TakeItemsName;
}
