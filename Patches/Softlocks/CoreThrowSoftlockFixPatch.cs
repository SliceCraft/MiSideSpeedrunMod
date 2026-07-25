using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch]
internal static class CoreThrowSoftlockFixPatch
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

        Plugin.Log.LogInfo("Core Softlock Fix: skipped post-throw AnimationPlayOnPlayer during Throw");
        return false;
    }

    private static bool IsCoreScene() => SceneManager.GetActiveScene().name == SceneName;

    private static bool IsThrowAnim(PlayerMove player) =>
        player?.scrAnimationNow != null
        && player.scrAnimationNow.gameObject.name == AnimationPlayerThrowName;

    private static bool IsPlayerOnThrow()
    {
        PlayerMove player = Object.FindObjectOfType<PlayerMove>();
        return IsThrowAnim(player);
    }
}
