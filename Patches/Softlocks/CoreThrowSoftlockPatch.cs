using HarmonyLib;
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

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ObjectAnimationPlayer), nameof(ObjectAnimationPlayer.AnimationPlayOnPlayer))]
    private static bool AnimationPlayOnPlayerPrefix(ObjectAnimationPlayer __instance)
    {
        if (__instance == null || __instance.gameObject.name != PostThrowAnimationName)
        {
            return true;
        }

        if (!IsCoreScene() || !IsPlayerOnThrow())
        {
            return true;
        }

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
