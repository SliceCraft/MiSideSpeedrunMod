#if DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SpeedrunMod.Events;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks.Debug;

[HarmonyPatch]
internal static class BaseballBatSoftlockDebug
{
    private const string Tag = "DEBUG-baseball06";
    private const string SceneName = "Scene 14 - MobilePlayer";
    private const float SoftlockTimeoutSeconds = 12f;
    private const KeyCode StuckDumpKey = KeyCode.F8;

    private static readonly HashSet<string> WatchedTimeEvents = new(StringComparer.Ordinal)
    {
        "TimeAnimationMita TakeBat",
        "TimeAnimationMita HoldHeadBat",
        "TimeAnimationMita StartNear",
        "TimeAnimationMita StopNear",
        "TimeAnimationMita ThrowPlayer",
        "TimeAnimationMita TakePlayer",
    };

    private static readonly string[] StateDumpNames =
    {
        "TimeAnimationMita TakeBat",
        "TimeAnimationMita HoldHeadBat",
        "TimeAnimationMita StartNear",
        "TimeAnimationMita StopNear",
        "TimeAnimationMita ThrowPlayer",
        "TimeAnimationMita TakePlayer",
        "Canvas Kick",
        "Quest 2 Start",
        "Bat",
        "Audio Kick",
    };

    private static bool _subscribedToSceneLoaded;
    private static float _stopNearRealtime = -1f;
    private static bool _sawKickClip;
    private static bool _sawNewEvent1;
    private static bool _sawCanvasKick;
    private static bool _sawQuest2;
    private static bool _stopNearWasActive;
    private static bool _loggedTimeoutCandidate;
    private static bool _loggedInactiveCandidate;
    private static bool _loggedStopAllCandidate;

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

        ResetSessionFlags();
        Plugin.Log.LogInfo(
            $"[{Tag}] entered {SceneName}; debug armed (F8=STUCK_DUMP; look for HANDOFF_OK / SOFTLOCK_CANDIDATE_*)",
            nameof(BaseballBatSoftlockDebug));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void YieldRestartPostfix(Time_Events __instance)
    {
        if (!IsMobilePlayer() || __instance == null)
        {
            return;
        }

        string name = SafeName(__instance);
        if (!WatchedTimeEvents.Contains(name))
        {
            return;
        }

        if (name == "TimeAnimationMita StopNear")
        {
            _stopNearRealtime = Time.realtimeSinceStartup;
            _stopNearWasActive = true;
            _loggedTimeoutCandidate = false;
            _loggedInactiveCandidate = false;
            _loggedStopAllCandidate = false;
        }

        Plugin.Log.LogInfo(
            $"[{Tag}] YieldRestart name={name} activeSelf={__instance.gameObject.activeSelf} " +
            $"activeInHierarchy={__instance.gameObject.activeInHierarchy} events={DescribeEvents(__instance)}",
            nameof(BaseballBatSoftlockDebug));
        TryDump("after-YieldRestart-" + name);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.StopAllTime))]
    private static void StopAllTimePostfix(Time_Events __instance)
    {
        if (!IsMobilePlayer() || __instance == null)
        {
            return;
        }

        string name = SafeName(__instance);
        if (!WatchedTimeEvents.Contains(name) && !name.Contains("StopNear", StringComparison.Ordinal))
        {
            return;
        }

        Plugin.Log.LogInfo($"[{Tag}] StopAllTime name={name}", nameof(BaseballBatSoftlockDebug));

        // Event path: StopNear aborted without Kick handoff.
        if (!_loggedStopAllCandidate &&
            !_sawNewEvent1 &&
            !_sawQuest2 &&
            (name == "TimeAnimationMita StopNear" || name.Contains("StopNear", StringComparison.Ordinal)))
        {
            _loggedStopAllCandidate = true;
            Plugin.Log.LogWarning(
                $"[{Tag}] SOFTLOCK_CANDIDATE_STOPALL sawKickClip={_sawKickClip} sawNewEvent1={_sawNewEvent1} " +
                $"sawCanvasKick={_sawCanvasKick} sawQuest2={_sawQuest2}",
                nameof(BaseballBatSoftlockDebug));
            TryDump("SOFTLOCK_CANDIDATE_STOPALL");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Animator_FunctionsOverride), nameof(Animator_FunctionsOverride.AnimationClipSimpleNext))]
    private static void AnimationClipSimpleNextPostfix(AnimationClip _animation)
    {
        if (!IsMobilePlayer() || _animation == null)
        {
            return;
        }

        string clipName = _animation.name ?? "?";
        bool interesting =
            clipName.Contains("Kick", StringComparison.OrdinalIgnoreCase) ||
            clipName.Contains("Bat", StringComparison.OrdinalIgnoreCase) ||
            clipName.Contains("Near", StringComparison.OrdinalIgnoreCase) ||
            clipName.Contains("Take", StringComparison.OrdinalIgnoreCase) ||
            clipName.Contains("Hold", StringComparison.OrdinalIgnoreCase) ||
            clipName.Contains("Throw", StringComparison.OrdinalIgnoreCase);
        if (!interesting)
        {
            return;
        }

        if (clipName.Contains("Kick", StringComparison.OrdinalIgnoreCase))
        {
            _sawKickClip = true;
        }

        Plugin.Log.LogInfo(
            $"[{Tag}] AnimationClipSimpleNext clip={clipName} length={_animation.length:F3}",
            nameof(BaseballBatSoftlockDebug));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Animator_FunctionsOverride), nameof(Animator_FunctionsOverride.NewEvent))]
    private static void NewEventPostfix(int x)
    {
        if (!IsMobilePlayer())
        {
            return;
        }

        if (x == 1)
        {
            _sawNewEvent1 = true;
        }

        Plugin.Log.LogInfo($"[{Tag}] NewEvent index={x}", nameof(BaseballBatSoftlockDebug));
        if (x == 1)
        {
            Plugin.Log.LogInfo(
                $"[{Tag}] HANDOFF_OK Kick NewEvent(1) sawKickClip={_sawKickClip}",
                nameof(BaseballBatSoftlockDebug));
            TryDump("after-NewEvent-1");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Dialogue_3DText), "Start")]
    private static void DialogueStartPostfix(Dialogue_3DText __instance)
    {
        if (!IsMobilePlayer() || __instance == null)
        {
            return;
        }

        string name = SafeName(__instance);
        int index = __instance.indexString;
        bool watched = index is >= 115 and <= 130;
        if (!watched)
        {
            return;
        }

        Plugin.Log.LogInfo(
            $"[{Tag}] DialogueStart name={name} index={index}",
            nameof(BaseballBatSoftlockDebug));
        TryDump($"after-dialogue-{index}");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), "Update")]
    private static void GameControllerUpdatePostfix()
    {
        if (!IsMobilePlayer())
        {
            return;
        }

        if (IsActive("Canvas Kick"))
        {
            _sawCanvasKick = true;
        }

        if (IsActive("Quest 2 Start"))
        {
            _sawQuest2 = true;
        }

        // Primary HITL path: player presses F8 when stuck — no delay involved.
        if (Input.GetKeyDown(StuckDumpKey))
        {
            Plugin.Log.LogWarning(
                $"[{Tag}] STUCK_DUMP key=F8 sawKickClip={_sawKickClip} sawNewEvent1={_sawNewEvent1} " +
                $"sawCanvasKick={_sawCanvasKick} sawQuest2={_sawQuest2} " +
                $"handoffOk={_sawNewEvent1} stopNearArmed={_stopNearRealtime >= 0f}",
                nameof(BaseballBatSoftlockDebug));
            TryDump("STUCK_DUMP");
        }

        bool stopNearActive = IsActive("TimeAnimationMita StopNear");

        // Event path: StopNear was running, then went inactive, still no Kick handoff.
        if (_stopNearWasActive &&
            !stopNearActive &&
            !_sawNewEvent1 &&
            !_sawQuest2 &&
            !_loggedInactiveCandidate)
        {
            _loggedInactiveCandidate = true;
            Plugin.Log.LogWarning(
                $"[{Tag}] SOFTLOCK_CANDIDATE_INACTIVE sawKickClip={_sawKickClip} sawNewEvent1={_sawNewEvent1} " +
                $"sawCanvasKick={_sawCanvasKick} sawQuest2={_sawQuest2}",
                nameof(BaseballBatSoftlockDebug));
            TryDump("SOFTLOCK_CANDIDATE_INACTIVE");
        }

        if (stopNearActive)
        {
            _stopNearWasActive = true;
        }

        // Weak secondary: wall-clock timeout after StopNear with no handoff.
        if (_stopNearRealtime >= 0f &&
            !_loggedTimeoutCandidate &&
            !_sawQuest2 &&
            !_sawNewEvent1 &&
            Time.realtimeSinceStartup - _stopNearRealtime >= SoftlockTimeoutSeconds)
        {
            _loggedTimeoutCandidate = true;
            Plugin.Log.LogWarning(
                $"[{Tag}] SOFTLOCK_CANDIDATE_TIMEOUT stopNearAge=" +
                $"{(Time.realtimeSinceStartup - _stopNearRealtime):F2}s sawKickClip={_sawKickClip} " +
                $"sawNewEvent1={_sawNewEvent1} sawCanvasKick={_sawCanvasKick} sawQuest2={_sawQuest2}",
                nameof(BaseballBatSoftlockDebug));
            TryDump("SOFTLOCK_CANDIDATE_TIMEOUT");
        }
    }

    private static void TryDump(string reason)
    {
        var sb = new StringBuilder();
        sb.Append($"[{Tag}] STATE reason={reason}");
        foreach (string name in StateDumpNames)
        {
            GameObject go = ComponentUtil.FindIncludingInactive(name);
            if (go == null)
            {
                sb.Append($" | {name}=MISSING");
                continue;
            }

            sb.Append($" | {name}=self:{go.activeSelf}/hier:{go.activeInHierarchy}");
            Time_Events te = go.GetComponent<Time_Events>();
            if (te != null)
            {
                sb.Append($" teEvents={DescribeEvents(te)}");
            }
        }

        sb.Append(
            $" | flags stopNear={_stopNearRealtime >= 0f} kickClip={_sawKickClip} newEvent1={_sawNewEvent1} " +
            $"canvasKick={_sawCanvasKick} quest2={_sawQuest2}");
        Plugin.Log.LogInfo(sb.ToString(), nameof(BaseballBatSoftlockDebug));
    }

    private static string DescribeEvents(Time_Events te)
    {
        try
        {
            TimePoint[] points = te.EventsOnTime;
            if (points == null)
            {
                return "null";
            }

            var parts = new List<string>(points.Length);
            for (int i = 0; i < points.Length; i++)
            {
                TimePoint p = points[i];
                if (p == null)
                {
                    parts.Add($"{i}:null");
                    continue;
                }

                string clip = p.timeAnimationClip != null ? p.timeAnimationClip.name : "none";
                parts.Add($"{i}:t={p.time:F2}/clip={clip}");
            }

            return "[" + string.Join(", ", parts) + "]";
        }
        catch (Exception ex)
        {
            return $"err:{ex.GetType().Name}";
        }
    }

    private static bool IsActive(string name)
    {
        GameObject go = ComponentUtil.FindIncludingInactive(name);
        return go != null && go.activeInHierarchy;
    }

    private static void ResetSessionFlags()
    {
        _stopNearRealtime = -1f;
        _sawKickClip = false;
        _sawNewEvent1 = false;
        _sawCanvasKick = false;
        _sawQuest2 = false;
        _stopNearWasActive = false;
        _loggedTimeoutCandidate = false;
        _loggedInactiveCandidate = false;
        _loggedStopAllCandidate = false;
    }

    private static bool IsMobilePlayer() => SceneManager.GetActiveScene().name == SceneName;

    private static string SafeName(Component c)
    {
        try
        {
            return c != null && c.gameObject != null ? c.gameObject.name : "?";
        }
        catch
        {
            return "?";
        }
    }
}
#endif
