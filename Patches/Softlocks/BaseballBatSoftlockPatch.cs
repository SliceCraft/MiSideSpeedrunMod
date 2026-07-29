using System;
using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch]
internal static class BaseballBatSoftlockPatch
{
    private const string SceneName = "Scene 14 - MobilePlayer";
    private const string KickClipName = "Mita Kick";
    private const string Quest2StartName = "Quest 2 Start";
    private const int KickHandoffEventIndex = 1;

    // Mita Kick.anim fires NewEvent(1) near clip end (~0.78s of 0.8s).
    // Wait at least one Kick length so the smack can show before repair.
    private const float MinRepairDelaySeconds = 0.8f;

    private static Animator_FunctionsOverride _kickAnimator;
    private static float _kickArmedRealtime = -1f;
    private static float _repairDelaySeconds = MinRepairDelaySeconds;
    private static bool _repairApplied;
    private static bool _handoffSeen;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Animator_FunctionsOverride), nameof(Animator_FunctionsOverride.AnimationClipSimpleNext))]
    private static void AnimationClipSimpleNextPostfix(
        Animator_FunctionsOverride __instance,
        AnimationClip _animation)
    {
        if (!IsMobilePlayerScene() || __instance == null || _animation == null)
        {
            return;
        }

        if (_animation.name != KickClipName)
        {
            return;
        }

        _kickAnimator = __instance;
        _kickArmedRealtime = Time.realtimeSinceStartup;
        _repairDelaySeconds = Mathf.Max(MinRepairDelaySeconds, _animation.length);
        _repairApplied = false;
        _handoffSeen = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Animator_FunctionsOverride), nameof(Animator_FunctionsOverride.NewEvent))]
    private static void NewEventPostfix(int x)
    {
        if (!IsMobilePlayerScene() || x != KickHandoffEventIndex)
        {
            return;
        }

        _handoffSeen = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), "Update")]
    private static void GameControllerUpdatePostfix()
    {
        if (!IsMobilePlayerScene())
        {
            ResetSession();
            return;
        }

        try
        {
            TryRepairKickHandoff();
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Kick handoff repair failed: {ex}", nameof(BaseballBatSoftlockPatch));
        }
    }

    private static void TryRepairKickHandoff()
    {
        if (_repairApplied || _handoffSeen || _kickArmedRealtime < 0f)
        {
            return;
        }

        if (IsHandoffActive())
        {
            _handoffSeen = true;
            return;
        }

        if (Time.realtimeSinceStartup - _kickArmedRealtime < _repairDelaySeconds)
        {
            return;
        }

        if (_kickAnimator == null)
        {
            Plugin.Log.LogWarning("Kick animator missing for NewEvent repair", nameof(BaseballBatSoftlockPatch));
            _repairApplied = true;
            return;
        }

        _kickAnimator.NewEvent(KickHandoffEventIndex);
        _repairApplied = true;
        _handoffSeen = true;
        Plugin.Log.LogInfo("repaired Kick NewEvent(1) handoff", nameof(BaseballBatSoftlockPatch));
    }

    private static bool IsHandoffActive()
    {
        GameObject quest2 = ComponentUtil.FindIncludingInactive(Quest2StartName);
        return quest2 != null && quest2.activeInHierarchy;
    }

    private static void ResetSession()
    {
        _kickAnimator = null;
        _kickArmedRealtime = -1f;
        _repairDelaySeconds = MinRepairDelaySeconds;
        _repairApplied = false;
        _handoffSeen = false;
    }

    private static bool IsMobilePlayerScene() => SceneManager.GetActiveScene().name == SceneName;
}
