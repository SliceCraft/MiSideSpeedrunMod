#if DEBUG
using System;
using System.Text;
using HarmonyLib;
using SpeedrunMod.Configs;
using SpeedrunMod.Events;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks.Debug;

[HarmonyPatch]
internal static class CappieRingSoftlockDebug
{
    private const string Tag = "DEBUG-cappie16";
    private const string SceneName = "Scene 7 - Backrooms";
    private const string TimeMitaStandName = "Time Mita Stand";
    private const string Quest5Name = "Quest5 - Пора уходить";
    private const string KindMitaName = "Mita Добрая";
    private const string KindPersonFutureName = "MitaPerson Future";
    private const string TakeRingName = "Interactive TakeRing";
    private const string TriggerNearName = "Trigger Near";
    private const string TriggerCameUpName = "Trigger Near CameUp";
    private const KeyCode StuckDumpKey = KeyCode.F8;

    private static bool _subscribedToSceneLoaded;
    private static float _standArmedRealtime = -1f;
    private static float _standDelaySeconds = -1f;
    private static bool _loggedStandYield;
    private static bool _loggedArmed;
    private static bool _loggedEnableOrSkip;
    private static bool _loggedVanillaBeat;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SceneLoadedEvent), nameof(SceneLoadedEvent.RegisterEvent))]
    private static void RegisterEventPostfix()
    {
        if (_subscribedToSceneLoaded)
        {
            return;
        }

        SceneLoadedEvent.SceneLoaded += OnSceneLoaded;
        _subscribedToSceneLoaded = true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != SceneName)
        {
            return;
        }

        ResetSession();
        
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableCappieRing))
        {
            return;
        }

        Plugin.Log.LogInfo(
            $"[{Tag}] entered {SceneName}; post-ring timing debug armed (F8=STUCK_DUMP)",
            nameof(CappieRingSoftlockDebug));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void YieldRestartPostfix(Time_Events __instance)
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableCappieRing))
        {
            return;
        }

        if (!IsCappieScene() || __instance == null || __instance.gameObject.name != TimeMitaStandName)
        {
            return;
        }

        _standArmedRealtime = Time.realtimeSinceStartup;
        _standDelaySeconds = ResolveStandDelaySeconds(__instance);
        _loggedStandYield = true;
        _loggedArmed = false;
        _loggedEnableOrSkip = false;
        _loggedVanillaBeat = false;

        Plugin.Log.LogInfo(
            $"[{Tag}] STAND_YIELDRestart delay={_standDelaySeconds:0.###}s " +
            $"quest5={DescribeActive(Quest5Name)} " +
            $"state={DescribeInteractState()}",
            nameof(CappieRingSoftlockDebug));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), "Update")]
    private static void GameControllerUpdatePostfix()
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableCappieRing) || !IsCappieScene())
        {
            return;
        }

        try
        {
            if (Input.GetKeyDown(StuckDumpKey))
            {
                DumpStuck("F8");
            }

            if (!_loggedStandYield || _standArmedRealtime < 0f)
            {
                return;
            }

            if (!_loggedArmed)
            {
                _loggedArmed = true;
                Plugin.Log.LogInfo(
                    $"[{Tag}] STAND_ARMED wait={_standDelaySeconds:0.###}s " +
                    $"quest5={DescribeActive(Quest5Name)} " +
                    $"state={DescribeInteractState()}",
                    nameof(CappieRingSoftlockDebug));
            }

            if (!_loggedVanillaBeat && IsKindMitaAlreadyInteractable())
            {
                _loggedVanillaBeat = true;
                float age = Time.realtimeSinceStartup - _standArmedRealtime;
                Plugin.Log.LogInfo(
                    $"[{Tag}] VANILLA_OR_PRIOR_INTERACT age={age:0.###}s " +
                    $"delay={_standDelaySeconds:0.###}s state={DescribeInteractState()}",
                    nameof(CappieRingSoftlockDebug));
            }

            if (_loggedEnableOrSkip || Time.realtimeSinceStartup - _standArmedRealtime < _standDelaySeconds)
            {
                return;
            }

            _loggedEnableOrSkip = true;
            float elapsed = Time.realtimeSinceStartup - _standArmedRealtime;
            Plugin.Log.LogInfo(
                $"[{Tag}] DELAY_ELAPSED elapsed={elapsed:0.###}s delay={_standDelaySeconds:0.###}s " +
                $"interactable={IsKindMitaAlreadyInteractable()} state={DescribeInteractState()} " +
                $"(Softlock Fix always enables TakeRing after delay)",
                nameof(CappieRingSoftlockDebug));
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"[{Tag}] Update failed: {ex}", nameof(CappieRingSoftlockDebug));
        }
    }

    private static float ResolveStandDelaySeconds(Time_Events standEvents)
    {
        TimePoint[] points = standEvents.EventsOnTime;
        if (points == null || points.Length == 0)
        {
            return 4.2f;
        }

        TimePoint point = points[0];
        float wait = point.time;
        if (point.timeAnimationClip != null)
        {
            wait += point.timeAnimationClip.length;
            Plugin.Log.LogInfo(
                $"[{Tag}] STAND_SLOT0 time={point.time:0.###} clip={point.timeAnimationClip.name} " +
                $"len={point.timeAnimationClip.length:0.###} wait={wait:0.###}",
                nameof(CappieRingSoftlockDebug));
        }

        return Mathf.Max(4.2f, wait);
    }

    private static bool IsKindMitaAlreadyInteractable()
    {
        GameObject cameUp = ComponentUtil.FindIncludingInactive(TriggerCameUpName);
        if (cameUp != null && cameUp.activeInHierarchy)
        {
            return true;
        }

        MitaPerson mita = ComponentUtil.FindIncludingInactive<MitaPerson>(KindMitaName);
        Transform personFuture = mita != null ? mita.transform.Find(KindPersonFutureName) : null;
        CapsuleCollider capsule = personFuture != null ? personFuture.GetComponent<CapsuleCollider>() : null;
        return capsule != null && capsule.enabled;
    }

    private static string DescribeInteractState()
    {
        GameObject near = ComponentUtil.FindIncludingInactive(TriggerNearName);
        GameObject cameUp = ComponentUtil.FindIncludingInactive(TriggerCameUpName);
        ObjectInteractive takeRing = ComponentUtil.FindIncludingInactive<ObjectInteractive>(TakeRingName);
        MitaPerson mita = ComponentUtil.FindIncludingInactive<MitaPerson>(KindMitaName);
        Transform personFuture = mita != null ? mita.transform.Find(KindPersonFutureName) : null;
        CapsuleCollider capsule = personFuture != null ? personFuture.GetComponent<CapsuleCollider>() : null;

        return $"near={(near != null && near.activeInHierarchy)} " +
               $"cameUp={(cameUp != null && cameUp.activeInHierarchy)} " +
               $"capsule={(capsule != null && capsule.enabled)} " +
               $"takeRing={(takeRing != null && takeRing.gameObject.activeInHierarchy)}";
    }

    private static string DescribeActive(string name)
    {
        GameObject go = ComponentUtil.FindIncludingInactive(name);
        if (go == null)
        {
            return "missing";
        }

        return go.activeInHierarchy ? "on" : "off";
    }

    private static void DumpStuck(string reason)
    {
        StringBuilder sb = new();
        sb.Append($"[{Tag}] STUCK_DUMP reason={reason} ");
        sb.Append($"armed={_standArmedRealtime >= 0f} ");
        if (_standArmedRealtime >= 0f)
        {
            sb.Append($"age={Time.realtimeSinceStartup - _standArmedRealtime:0.###}s ");
            sb.Append($"delay={_standDelaySeconds:0.###}s ");
        }

        sb.Append($"quest5={DescribeActive(Quest5Name)} ");
        sb.Append($"state={DescribeInteractState()}");
        Plugin.Log.LogInfo(sb.ToString(), nameof(CappieRingSoftlockDebug));
    }

    private static void ResetSession()
    {
        _standArmedRealtime = -1f;
        _standDelaySeconds = -1f;
        _loggedStandYield = false;
        _loggedArmed = false;
        _loggedEnableOrSkip = false;
        _loggedVanillaBeat = false;
    }

    private static bool IsCappieScene() => SceneManager.GetActiveScene().name == SceneName;
}
#endif
