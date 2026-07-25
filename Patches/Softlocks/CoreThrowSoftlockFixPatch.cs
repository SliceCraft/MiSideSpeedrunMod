using System;
using System.Collections.Generic;
using HarmonyLib;
using SpeedrunMod.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch]
internal static class CoreThrowSoftlockFixPatch
{
    private const string SceneName = "Scene 15 - BasementAndDeath";
    private const string AnimationPlayerThrowName = "AnimationPlayer Throw";
    private const string AnimationPlayerThrowHeadName = "AnimationPlayer ThrowHead";
    private const string Quest5Name = "Quest 5 Выкинули обратно в дом";

    private static readonly Dictionary<string, int> CriticalDialogues = new()
    {
        ["DialogueMita 3"] = 26,
        ["Player 1"] = 68,
        ["Player 2"] = 69,
        ["Player 3"] = 70,
    };

    // True from AnimationPlayer Throw start until Quest 5 is active (or scene reload).
    private static bool _throwWindowOpen;
    private static bool _blockSpaceThisFrame;

    static CoreThrowSoftlockFixPatch()
    {
        SceneLoadedEvent.SceneLoaded += (_, _) => _throwWindowOpen = false;
    }

    private static bool IsCoreScene() =>
        SceneManager.GetActiveScene().name == SceneName;

    private static bool IsThrowCriticalDialogue(Dialogue_3DText dialogue) =>
        dialogue != null
        && CriticalDialogues.TryGetValue(dialogue.gameObject.name, out int index)
        && index == dialogue.indexString;

    private static bool IsThrowAnimationPlaying()
    {
        PlayerMove player = UnityEngine.Object.FindObjectOfType<PlayerMove>();
        if (player?.scrAnimationNow == null)
        {
            return false;
        }

        string name = player.scrAnimationNow.gameObject.name;
        return name is AnimationPlayerThrowName or AnimationPlayerThrowHeadName;
    }

    private static bool ShouldBlockSkip(Dialogue_3DText dialogue) =>
        IsCoreScene() && (IsThrowAnimationPlaying() || IsThrowCriticalDialogue(dialogue));

    private static void MarkThrowStarted(ObjectAnimationPlayer animationPlayer)
    {
        if (!IsCoreScene() || animationPlayer == null)
        {
            return;
        }

        if (animationPlayer.gameObject.name == AnimationPlayerThrowName)
        {
            _throwWindowOpen = true;
        }
    }

    private static void EnsureThrowCompleted()
    {
        if (!IsCoreScene() || !_throwWindowOpen)
        {
            return;
        }

        try
        {
            GameObject quest5 = FindNamedObject(Quest5Name);
            if (quest5 == null)
            {
                return;
            }

            // eventFinish should enable Quest 5; SetActive is the fallback if UnityEvent was a no-op.
            if (!quest5.activeSelf)
            {
                FindNamed<ObjectAnimationPlayer>(AnimationPlayerThrowName)?.eventFinish?.Invoke();
            }

            if (!quest5.activeSelf)
            {
                quest5.SetActive(true);
            }

            if (quest5.activeSelf)
            {
                _throwWindowOpen = false;
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

    private static GameObject FindNamedObject(string objectName)
    {
        Transform transform = FindNamed<Transform>(objectName);
        return transform != null ? transform.gameObject : null;
    }

    // Update inlines Space-skip (does not call SkipDialogue); block Space for this Update only.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Dialogue_3DText), "Update")]
    private static void DialogueUpdatePrefix(Dialogue_3DText __instance)
    {
        _blockSpaceThisFrame = ShouldBlockSkip(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Dialogue_3DText), "Update")]
    private static void DialogueUpdatePostfix()
    {
        _blockSpaceThisFrame = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Input), nameof(Input.GetKeyDown), typeof(KeyCode))]
    private static bool GetKeyDownPrefix(KeyCode key, ref bool __result)
    {
        if (key != KeyCode.Space || !_blockSpaceThisFrame)
        {
            return true;
        }

        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Dialogue_3DText), "SkipDialogue")]
    private static bool SkipDialoguePrefix(Dialogue_3DText __instance) =>
        !ShouldBlockSkip(__instance);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectAnimationPlayer), nameof(ObjectAnimationPlayer.AnimationPlay))]
    private static void AnimationPlayPostfix(ObjectAnimationPlayer __instance) =>
        MarkThrowStarted(__instance);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectAnimationPlayer), nameof(ObjectAnimationPlayer.AnimationPlayFast))]
    private static void AnimationPlayFastPostfix(ObjectAnimationPlayer __instance) =>
        MarkThrowStarted(__instance);

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectAnimationPlayer), nameof(ObjectAnimationPlayer.AnimationStop))]
    private static void AnimationStopPostfix(ObjectAnimationPlayer __instance)
    {
        if (__instance != null && __instance.gameObject.name == AnimationPlayerThrowName)
        {
            EnsureThrowCompleted();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerMove), nameof(PlayerMove.AnimationFastStop))]
    private static void AnimationFastStopPrefix(PlayerMove __instance)
    {
        if (__instance?.scrAnimationNow != null
            && __instance.scrAnimationNow.gameObject.name == AnimationPlayerThrowName)
        {
            EnsureThrowCompleted();
        }
    }
}
