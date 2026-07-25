using System;
using HarmonyLib;
using SpeedrunMod.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Softlocks.Core;

/// <summary>
/// Softlock Fix for The Core robot-Mita throw (Scene 15 — BasementAndDeath).
///
/// Root cause: Space-skip advances <see cref="Dialogue_3DText"/> UnityEvents but does
/// not keep the parallel throw controllers in sync — <c>AnimationPlayer Throw</c>
/// (<see cref="ObjectAnimationPlayer"/>) plus its sibling <see cref="Time_Events"/>
/// (Player 1 at t=8.5s) and <c>eventFinish</c> → Quest 5. Skipping DialogueMita 3 /
/// Player 1–3 or interrupting the throw anim leaves progression stuck.
///
/// Strategy (hybrid): block skip in the throw-critical window; repair by ensuring
/// Quest 5 activates when the throw anim stops or is force-stopped.
/// </summary>
internal static class CoreThrowSoftlockFix
{
    internal const string SceneName = "Scene 15 - BasementAndDeath";
    internal const string AnimationPlayerThrowName = "AnimationPlayer Throw";
    internal const string AnimationPlayerThrowHeadName = "AnimationPlayer ThrowHead";
    internal const string Quest5Name = "Quest 5 Выкинули обратно в дом";

    private static bool _throwSequenceActive;
    private static bool _quest5Ensured;

    internal static void Register()
    {
        SceneLoadedEvent.SceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != SceneName)
        {
            _throwSequenceActive = false;
            _quest5Ensured = false;
        }
    }

    internal static bool IsCoreScene() =>
        SceneManager.GetActiveScene().name == SceneName;

    internal static bool ShouldBlockSkip(Dialogue_3DText dialogue)
    {
        if (!IsCoreScene() || dialogue == null)
        {
            return false;
        }

        return IsThrowAnimationPlaying() || IsThrowCriticalDialogue(dialogue);
    }

    internal static bool IsThrowCriticalDialogue(Dialogue_3DText dialogue)
    {
        if (dialogue == null)
        {
            return false;
        }

        string name = dialogue.gameObject.name;
        int index = dialogue.indexString;

        return (name == "DialogueMita 3" && index == 26)
               || (name == "Player 1" && index == 68)
               || (name == "Player 2" && index == 69)
               || (name == "Player 3" && index == 70);
    }

    internal static bool IsThrowAnimationPlaying()
    {
        PlayerMove player = UnityEngine.Object.FindObjectOfType<PlayerMove>();
        if (player == null || player.scrAnimationNow == null)
        {
            return false;
        }

        string animName = player.scrAnimationNow.gameObject.name;
        return animName is AnimationPlayerThrowName or AnimationPlayerThrowHeadName;
    }

    /// <summary>
    /// True when Space-skip must be gated even with no active Dialogue_3DText.Update
    /// (throw body anim window before Player 1 appears).
    /// </summary>
    internal static bool ShouldBlockSkipGlobally()
    {
        if (!IsCoreScene() || _quest5Ensured)
        {
            return false;
        }

        return IsThrowAnimationPlaying() || _throwSequenceActive;
    }

    internal static void MarkThrowStarted(ObjectAnimationPlayer animationPlayer)
    {
        if (!IsCoreScene() || animationPlayer == null)
        {
            return;
        }

        string name = animationPlayer.gameObject.name;
        if (name == AnimationPlayerThrowName)
        {
            _throwSequenceActive = true;
            _quest5Ensured = false;
            Plugin.Log.LogInfo($"Core Softlock Fix: throw sequence started ({name})");
        }
        else if (name == AnimationPlayerThrowHeadName)
        {
            // Pre-throw head beat — block skip via IsThrowAnimationPlaying only;
            // do not arm Quest 5 repair.
            Plugin.Log.LogDebug("Core Softlock Fix: ThrowHead started");
        }
    }

    /// <summary>
    /// Repair: ensure Quest 5 (post-throw progression gate) is active after the throw
    /// animation finishes or is interrupted mid-sequence.
    /// </summary>
    internal static void EnsureThrowCompleted(string reason)
    {
        if (!IsCoreScene() || _quest5Ensured || !_throwSequenceActive)
        {
            return;
        }

        try
        {
            GameObject quest5 = FindNamedGameObject(Quest5Name);
            if (quest5 == null)
            {
                Plugin.Log.LogWarning($"Core Softlock Fix: Quest 5 not found ({reason})");
                return;
            }

            if (!quest5.activeSelf)
            {
                ObjectAnimationPlayer throwPlayer = FindNamedAnimationPlayer(AnimationPlayerThrowName);
                throwPlayer?.eventFinish?.Invoke();

                if (!quest5.activeSelf)
                {
                    quest5.SetActive(true);
                }

                Plugin.Log.LogInfo($"Core Softlock Fix: repaired Quest 5 ({reason})");
            }

            if (quest5.activeSelf)
            {
                _quest5Ensured = true;
                _throwSequenceActive = false;
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Core Softlock Fix repair failed: {ex}");
        }
    }

    private static ObjectAnimationPlayer FindNamedAnimationPlayer(string objectName)
    {
        ObjectAnimationPlayer[] players =
            UnityEngine.Object.FindObjectsByType<ObjectAnimationPlayer>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (ObjectAnimationPlayer player in players)
        {
            if (player != null && player.gameObject.name == objectName)
            {
                return player;
            }
        }

        return null;
    }

    private static GameObject FindNamedGameObject(string objectName)
    {
        Transform[] transforms =
            UnityEngine.Object.FindObjectsByType<Transform>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (Transform transform in transforms)
        {
            if (transform != null && transform.gameObject.name == objectName)
            {
                return transform.gameObject;
            }
        }

        return null;
    }
}

[HarmonyPatch]
internal static class CoreThrowDialogueSkipPatches
{
    private static bool _forcedBlock;
    private static bool _savedCanSkip;

    // Update inlines the Space-skip path (does not call SkipDialogue). Gate canSkipDialogue
    // for the duration of Update when this line is throw-critical.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Dialogue_3DText), "Update")]
    private static void UpdatePrefix(Dialogue_3DText __instance)
    {
        _forcedBlock = false;
        if (!CoreThrowSoftlockFix.ShouldBlockSkip(__instance))
        {
            return;
        }

        _savedCanSkip = GlobalGame.canSkipDialogue;
        if (!_savedCanSkip)
        {
            return;
        }

        GlobalGame.canSkipDialogue = false;
        _forcedBlock = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Dialogue_3DText), "Update")]
    private static void UpdatePostfix()
    {
        if (!_forcedBlock)
        {
            return;
        }

        GlobalGame.canSkipDialogue = _savedCanSkip;
        _forcedBlock = false;
    }

    // External callers (e.g. MitaPerson) invoke SkipDialogue directly.
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Dialogue_3DText), "SkipDialogue")]
    private static bool SkipDialoguePrefix(Dialogue_3DText __instance)
    {
        if (!CoreThrowSoftlockFix.ShouldBlockSkip(__instance))
        {
            return true;
        }

        Plugin.Log.LogDebug(
            $"Core Softlock Fix: blocked skip on {__instance.gameObject.name} [{__instance.indexString}]");
        return false;
    }
}

[HarmonyPatch(typeof(ObjectAnimationPlayer))]
internal static class CoreThrowAnimationPlayerPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ObjectAnimationPlayer.AnimationPlay))]
    private static void AnimationPlayPostfix(ObjectAnimationPlayer __instance)
    {
        CoreThrowSoftlockFix.MarkThrowStarted(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ObjectAnimationPlayer.AnimationPlayFast))]
    private static void AnimationPlayFastPostfix(ObjectAnimationPlayer __instance)
    {
        CoreThrowSoftlockFix.MarkThrowStarted(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ObjectAnimationPlayer.AnimationStop))]
    private static void AnimationStopPostfix(ObjectAnimationPlayer __instance)
    {
        if (__instance != null
            && __instance.gameObject.name == CoreThrowSoftlockFix.AnimationPlayerThrowName)
        {
            CoreThrowSoftlockFix.EnsureThrowCompleted("AnimationPlayer Throw stop");
        }
    }
}

[HarmonyPatch(typeof(PlayerMove))]
internal static class CoreThrowPlayerMovePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(PlayerMove.AnimationFastStop))]
    private static void AnimationFastStopPrefix(PlayerMove __instance)
    {
        if (!CoreThrowSoftlockFix.IsCoreScene() || __instance?.scrAnimationNow == null)
        {
            return;
        }

        // Only repair the main throw body anim — never arm Quest 5 from ThrowHead.
        if (__instance.scrAnimationNow.gameObject.name
            == CoreThrowSoftlockFix.AnimationPlayerThrowName)
        {
            CoreThrowSoftlockFix.EnsureThrowCompleted("AnimationFastStop during throw");
        }
    }
}

/// <summary>
/// Frame-level skip gate for the throw anim window when no dialogue Update is running.
/// </summary>
[HarmonyPatch(typeof(GameController), "Update")]
internal static class CoreThrowGameControllerPatch
{
    private static bool _forcedBlock;
    private static bool _savedCanSkip;

    [HarmonyPrefix]
    private static void UpdatePrefix()
    {
        _forcedBlock = false;
        if (!CoreThrowSoftlockFix.ShouldBlockSkipGlobally())
        {
            return;
        }

        _savedCanSkip = GlobalGame.canSkipDialogue;
        if (!_savedCanSkip)
        {
            return;
        }

        GlobalGame.canSkipDialogue = false;
        _forcedBlock = true;
    }

    [HarmonyPostfix]
    private static void UpdatePostfix()
    {
        if (!_forcedBlock)
        {
            return;
        }

        GlobalGame.canSkipDialogue = _savedCanSkip;
        _forcedBlock = false;
    }
}
