using HarmonyLib;
using SpeedrunMod.Configs;
using SpeedrunMod.Notifications;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch]
internal static class CoreThrowSoftlockPatch
{
    private const string SceneName = "Scene 15 - BasementAndDeath";
    private const string AnimationPlayerThrowName = "AnimationPlayer Throw";
    private const string PostThrowAnimationName = "Animation";
    private const string Notification = "Softlock Fix: Core throw";

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectAnimationPlayer), nameof(ObjectAnimationPlayer.AnimationPlayOnPlayer))]
    private static bool AnimationPlayOnPlayerPrefix(ObjectAnimationPlayer __instance)
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableCoreThrow))
        {
            return true;
        }

        if (__instance == null || __instance.gameObject.name != PostThrowAnimationName)
        {
            return true;
        }

        if (!IsCoreScene() || !IsPlayerOnThrow())
        {
            return true;
        }

        NotificationManager.Show(new NotificationMessage(Notification, cooldown: 5f));
        Plugin.Log.LogInfo("skipped post-throw AnimationPlayOnPlayer during Throw", nameof(CoreThrowSoftlockPatch));
        return false;
    }

    private static bool IsCoreScene() => SceneManager.GetActiveScene().name == SceneName;

    private static bool IsThrowAnim(PlayerMove player) =>
        player?.scrAnimationNow != null &&
        player.scrAnimationNow.gameObject.name == AnimationPlayerThrowName;

    private static bool IsPlayerOnThrow()
    {
        PlayerMove player = UnityEngine.Object.FindObjectOfType<PlayerMove>();
        return IsThrowAnim(player);
    }
}
