using System;
using System.Text;
using HarmonyLib;
using SpeedrunMod.Events;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks.Debug;

/// <summary>
/// DEBUG ONLY (ticket 12). Do not merge to the public Softlock Fixes tip.
/// Grep BepInEx LogOutput for: [DEBUG-ghostly12]
/// </summary>
[HarmonyPatch]
internal static class GhostlyPuzzleSoftlockDebug
{
    private const string Tag = "DEBUG-ghostly12";
    private const string SceneName = "Scene 11 - Backrooms";
    // Slightly before Softlock Fix RepairDelaySeconds (1.25) so HITL still gets a stuck-state
    // dump when Softlock Fix is loaded and about to repair.
    private const float SoftlockCandidateDelaySeconds = 1.2f;

    private static bool _subscribed;
    private static Location11_BlackRoom _room;
    private static float _sitRealtime = -1f;
    private static bool _loggedPlayPuzle;
    private static bool _loggedGlueWork;
    private static bool _loggedSoftlockCandidate;
    private static string _lastPhase = "";

    static GhostlyPuzzleSoftlockDebug()
    {
        TrySubscribeSceneLoaded();
    }

    private static void TrySubscribeSceneLoaded()
    {
        if (_subscribed)
        {
            return;
        }

        SceneLoadedEvent.SceneLoaded += OnSceneLoaded;
        _subscribed = true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != SceneName)
        {
            return;
        }

        ResetSession();
        Plugin.Log.LogInfo(
            $"[{Tag}] entered {SceneName}; GhostLock debug diagnostics armed",
            nameof(GhostlyPuzzleSoftlockDebug));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location11_BlackRoom), nameof(Location11_BlackRoom.PlayerSit))]
    private static void PlayerSitPostfix(Location11_BlackRoom __instance)
    {
        TrySubscribeSceneLoaded();
        if (!IsGhostMitaScene() || __instance == null)
        {
            return;
        }

        _room = __instance;
        _sitRealtime = Time.realtimeSinceStartup;
        _loggedPlayPuzle = false;
        _loggedGlueWork = false;
        _loggedSoftlockCandidate = false;
        _lastPhase = "";
        LogState(__instance, "PlayerSit");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location11_BlackRoom), nameof(Location11_BlackRoom.PutPuzle))]
    private static void PutPuzlePostfix(Location11_BlackRoom __instance)
    {
        if (!IsGhostMitaScene() || __instance == null)
        {
            return;
        }

        LogState(__instance, "PutPuzle");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location11_BlackRoom), nameof(Location11_BlackRoom.PuzleTake))]
    private static void PuzleTakePostfix(Location11_BlackRoom __instance, int _indexPuzle)
    {
        if (!IsGhostMitaScene() || __instance == null)
        {
            return;
        }

        LogState(__instance, $"PuzleTake index={_indexPuzle}");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location11_BlackRoom), nameof(Location11_BlackRoom.PuzleDrop))]
    private static void PuzleDropPostfix(Location11_BlackRoom __instance)
    {
        if (!IsGhostMitaScene() || __instance == null)
        {
            return;
        }

        LogState(__instance, "PuzleDrop");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location11_BlackRoom), nameof(Location11_BlackRoom.PuzleReady))]
    private static void PuzleReadyPostfix(Location11_BlackRoom __instance)
    {
        if (!IsGhostMitaScene() || __instance == null)
        {
            return;
        }

        LogState(__instance, "PuzleReady");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location11_BlackRoom), nameof(Location11_BlackRoom.StartWorkGlue))]
    private static void StartWorkGluePostfix(Location11_BlackRoom __instance)
    {
        if (!IsGhostMitaScene() || __instance == null)
        {
            return;
        }

        LogState(__instance, "StartWorkGlue");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location11_BlackRoom), nameof(Location11_BlackRoom.ExitTable))]
    private static void ExitTablePostfix(Location11_BlackRoom __instance)
    {
        if (!IsGhostMitaScene() || __instance == null)
        {
            return;
        }

        LogState(__instance, "ExitTable");
        ResetSession();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location11_BlackRoom), "Update")]
    private static void UpdatePostfix(Location11_BlackRoom __instance)
    {
        if (!IsGhostMitaScene() || __instance == null || _sitRealtime < 0f)
        {
            return;
        }

        if (_room != null && __instance != _room)
        {
            return;
        }

        _room = __instance;

        if (__instance.glueWork)
        {
            if (!_loggedGlueWork)
            {
                _loggedGlueWork = true;
                LogState(__instance, "glueWork");
            }

            return;
        }

        if (__instance.playPuzle)
        {
            if (!_loggedPlayPuzle)
            {
                _loggedPlayPuzle = true;
                // Softlock Fix EnableAssembleMode (or vanilla) just latched — pair with
                // waiting-idle-timers / SOFTLOCK_CANDIDATE dumps as repair before→after.
                var afterIdle = _lastPhase is "waiting-idle-timers" or "SOFTLOCK_CANDIDATE";
                LogState(__instance, afterIdle ? "playPuzle (after idle)" : "playPuzle");
            }

            return;
        }

        MaybeLogSoftlockCandidate(__instance);
        MaybeLogWaitingPhase(__instance);
    }

    private static void MaybeLogSoftlockCandidate(Location11_BlackRoom room)
    {
        if (_loggedSoftlockCandidate || _sitRealtime < 0f)
        {
            return;
        }

        if (Time.realtimeSinceStartup - _sitRealtime < SoftlockCandidateDelaySeconds)
        {
            return;
        }

        if (room.timeStartPlayPuzle > 0f || room.timeStartPuzle > 0f)
        {
            return;
        }

        _loggedSoftlockCandidate = true;
        _lastPhase = "SOFTLOCK_CANDIDATE";
        Plugin.Log.LogWarning(
            $"[{Tag}] SOFTLOCK_CANDIDATE sitAge={(Time.realtimeSinceStartup - _sitRealtime):F2}s " +
            $"timeScale={Time.timeScale:0.###} playPuzle={room.playPuzle} glueWork={room.glueWork}",
            nameof(GhostlyPuzzleSoftlockDebug));
        LogState(room, "SOFTLOCK_CANDIDATE");
    }

    private static void MaybeLogWaitingPhase(Location11_BlackRoom room)
    {
        string phase;
        if (room.timeStartPlayPuzle > 0f)
        {
            phase = "waiting-play-timer";
        }
        else if (room.timeStartPuzle > 0f)
        {
            phase = "waiting-place-timer";
        }
        else
        {
            phase = "waiting-idle-timers";
        }

        if (phase == _lastPhase)
        {
            return;
        }

        _lastPhase = phase;
        LogState(room, phase);
    }

    private static void LogState(Location11_BlackRoom room, string phase) =>
        Plugin.Log.LogInfo(DescribeState(room, phase), nameof(GhostlyPuzzleSoftlockDebug));

    private static string DescribeState(Location11_BlackRoom room, string phase)
    {
        var showCursor = room.scrgc != null && room.scrgc.showCursor;
        var mouseOverPlaneActive = room.mouseOverPlane != null && room.mouseOverPlane.activeSelf;
        var interactiveTableActive = room.interactiveTable != null && room.interactiveTable.activeSelf;
        var buttonExitHide = room.buttonExit != null && room.buttonExit.hide;
        var exitHintActive = false;
        if (room.buttonExit != null)
        {
            var hintTransform = room.buttonExit.transform;
            if (hintTransform != null && hintTransform.parent != null)
            {
                exitHintActive = hintTransform.parent.gameObject.activeSelf;
            }
        }

        var sitAge = _sitRealtime >= 0f ? Time.realtimeSinceStartup - _sitRealtime : -1f;

        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append(Tag);
        sb.Append("] ");
        sb.Append(phase);
        sb.Append(" sitAge=");
        sb.Append(sitAge.ToString("0.###"));
        sb.Append(" timeScale=");
        sb.Append(Time.timeScale.ToString("0.###"));
        sb.Append(" playPuzle=");
        sb.Append(room.playPuzle);
        sb.Append(" glueWork=");
        sb.Append(room.glueWork);
        sb.Append(" timeStartPlayPuzle=");
        sb.Append(room.timeStartPlayPuzle.ToString("0.###"));
        sb.Append(" timeStartPuzle=");
        sb.Append(room.timeStartPuzle.ToString("0.###"));
        sb.Append(" indexPuzleWork=");
        sb.Append(room.indexPuzleWork);
        sb.Append(" indexPuzleHold=");
        sb.Append(room.indexPuzleHold);
        sb.Append(" showCursor=");
        sb.Append(showCursor);
        sb.Append(" mouseOverPlane=");
        sb.Append(mouseOverPlaneActive);
        sb.Append(" interactiveTable=");
        sb.Append(interactiveTableActive);
        sb.Append(" buttonExit.hide=");
        sb.Append(buttonExitHide);
        sb.Append(" exitHint=");
        sb.Append(exitHintActive);
        sb.Append(" pieces=[");

        var frames = room.framesFound;
        if (frames != null)
        {
            for (var i = 0; i < frames.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append("; ");
                }

                var frame = frames[i];
                if (frame?.puzle == null)
                {
                    sb.Append(i);
                    sb.Append(":null");
                    continue;
                }

                var paper = frame.puzle.GetComponent<Location11_PaperPart>();
                var put = paper != null && paper.put;
                var mouse = paper != null && paper.mouse;
                var paperEnabled = paper != null && paper.enabled;

                sb.Append(i);
                sb.Append(":active=");
                sb.Append(frame.puzle.activeSelf);
                sb.Append(" added=");
                sb.Append(frame.addedTable);
                sb.Append(" put=");
                sb.Append(put);
                sb.Append(" mouse=");
                sb.Append(mouse);
                sb.Append(" enabled=");
                sb.Append(paperEnabled);
            }
        }

        sb.Append(']');
        return sb.ToString();
    }

    private static void ResetSession()
    {
        _room = null;
        _sitRealtime = -1f;
        _loggedPlayPuzle = false;
        _loggedGlueWork = false;
        _loggedSoftlockCandidate = false;
        _lastPhase = "";
    }

    private static bool IsGhostMitaScene() => SceneManager.GetActiveScene().name == SceneName;
}
