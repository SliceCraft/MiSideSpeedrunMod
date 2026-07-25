using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch]
internal static class CoreThrowSoftlockFixPatch
{
    private const string SceneName = "Scene 15 - BasementAndDeath";
    private const string AnimationPlayerThrowName = "AnimationPlayer Throw";
    private const string Quest5Name = "Quest 5 Выкинули обратно в дом";

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectAnimationPlayer), nameof(ObjectAnimationPlayer.AnimationStop))]
    private static void AnimationStopPostfix(ObjectAnimationPlayer __instance)
    {
        if (__instance != null && __instance.gameObject.name == AnimationPlayerThrowName)
        {
            EnsureQuest5();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerMove), nameof(PlayerMove.AnimationFastStop))]
    private static void AnimationFastStopPrefix(PlayerMove __instance)
    {
        if (IsThrowAnim(__instance))
        {
            EnsureQuest5();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerMove), nameof(PlayerMove.AnimationPlayStop))]
    private static void AnimationPlayStopPrefix(PlayerMove __instance)
    {
        // PlayStop ends the throw without always firing ObjectAnimationPlayer.eventFinish.
        if (IsThrowAnim(__instance))
        {
            EnsureQuest5();
        }
    }

    private static bool IsCoreScene() =>
        SceneManager.GetActiveScene().name == SceneName;

    private static bool IsThrowAnim(PlayerMove player) =>
        IsCoreScene()
        && player?.scrAnimationNow != null
        && player.scrAnimationNow.gameObject.name == AnimationPlayerThrowName;

    private static void EnsureQuest5()
    {
        if (!IsCoreScene())
        {
            return;
        }

        try
        {
            Transform questTransform = FindNamed<Transform>(Quest5Name);
            GameObject quest5 = questTransform != null ? questTransform.gameObject : null;
            if (quest5 == null || quest5.activeSelf)
            {
                return;
            }

            // eventFinish should enable Quest 5; SetActive if the UnityEvent was a no-op.
            FindNamed<ObjectAnimationPlayer>(AnimationPlayerThrowName)?.eventFinish?.Invoke();
            if (!quest5.activeSelf)
            {
                quest5.SetActive(true);
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Core Softlock Fix repair failed: {ex}");
        }
    }

    private static T FindNamed<T>(string objectName) where T : UnityEngine.Object
    {
        T[] found = UnityEngine.Object.FindObjectsByType<T>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (T item in found)
        {
            if (item == null)
            {
                continue;
            }

            string name = item is Component component
                ? component.gameObject.name
                : item.name;

            if (name == objectName)
            {
                return item;
            }
        }

        return null;
    }
}
