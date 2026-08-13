using System;
using HarmonyLib;
using SpeedrunMod.Configs;
using SpeedrunMod.Notifications;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch]
internal static class KindBedroomNoteSoftlockPatch
{
    private const string SceneName = "Scene 15 - BasementAndDeath";
    private const string TakeItemsName = "AnimationPlayer TakeItems";
    private const string StayMitaName = "AnimationPlayer StayMita";
    private const string ShowItemsName = "AnimationPlayer ShowItems";
    private const string StayUpEventsName = "TimeAnimation Mita StayUp";
    private const string MitaTakeItemsTimeName = "TimeAnimation Mita TakeItems";
    private const string StartShowItemsTimeName = "TimeAnimation Mita StartShowItems";
    private const string Notification = "Softlock Fix: Kind bedroom note";

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectAnimationPlayer), nameof(ObjectAnimationPlayer.AnimationPlay))]
    private static void AnimationPlayPrefix(ObjectAnimationPlayer __instance)
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableKindBedroomNote))
        {
            return;
        }

        if (__instance == null || !IsBasementScene())
        {
            return;
        }

        string name = __instance.gameObject.name;

        if (name == TakeItemsName)
        {
            ClearShowItemsConflictBeforeTakeItems();
            return;
        }

        if (name == StayMitaName)
        {
            TryFinishLeftoverTakeItems("StayMita");
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void StayUpYieldRestartPrefix(Time_Events __instance)
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableKindBedroomNote))
        {
            return;
        }

        if (__instance == null || __instance.gameObject.name != StayUpEventsName || !IsBasementScene())
        {
            return;
        }

        TryFinishLeftoverTakeItems("StayUp");
    }

    private static void ClearShowItemsConflictBeforeTakeItems()
    {
        try
        {
            bool repaired = false;

            Time_Events startShow = ComponentUtil.FindIncludingInactive<Time_Events>(StartShowItemsTimeName);
            if (startShow != null && startShow.gameObject.activeInHierarchy)
            {
                startShow.StopAllTime();
                repaired = true;
            }

            PlayerMove player = UnityEngine.Object.FindObjectOfType<PlayerMove>();
            if (player != null && player.animationRun && IsPlayerAnim(player, ShowItemsName))
            {
                player.AnimationFastStop();
                repaired = true;
            }

            if (!repaired)
            {
                return;
            }

            NotificationManager.Show(new NotificationMessage(Notification, cooldown: 5f));
            Plugin.Log.LogInfo(
                "cleared ShowItems conflict before TakeItems (fast E race)",
                nameof(KindBedroomNoteSoftlockPatch));
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"ShowItems conflict Softlock Fix failed: {ex}", nameof(KindBedroomNoteSoftlockPatch));
        }
    }

    private static void TryFinishLeftoverTakeItems(string seam)
    {
        try
        {
            PlayerMove player = UnityEngine.Object.FindObjectOfType<PlayerMove>();
            if (player == null || !player.animationRun || !IsPlayerAnim(player, TakeItemsName))
            {
                return;
            }

            TimeUtil.StopTimeEvents(MitaTakeItemsTimeName);
            ComponentUtil.FindIncludingInactive<ObjectAnimationPlayer>(TakeItemsName)?.eventStartLoop?.Invoke();
            player.AnimationFastStop();

            NotificationManager.Show(new NotificationMessage(Notification, cooldown: 5f));
            Plugin.Log.LogInfo($"finished TakeItems before {seam}", nameof(KindBedroomNoteSoftlockPatch));
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"{seam} leftover TakeItems Softlock Fix failed: {ex}", nameof(KindBedroomNoteSoftlockPatch));
        }
    }

    private static bool IsPlayerAnim(PlayerMove player, string animName) =>
        player != null
        && player.scrAnimationNow != null
        && player.scrAnimationNow.gameObject != null
        && player.scrAnimationNow.gameObject.name == animName;

    private static bool IsBasementScene() => SceneManager.GetActiveScene().name == SceneName;
}
