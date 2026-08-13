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
internal static class GhostlyPuzzleSoftlockDebug
{
    private const string Tag = "DEBUG-ghostly12";
    private const string SceneName = "Scene 11 - Backrooms";
    private const KeyCode StuckDumpKey = KeyCode.F8;

    private const float SoftlockFixRepairDelaySeconds = 1.25f;
    private const float SoftlockStuckLateSeconds = 3.5f;

    private static bool _subscribedToSceneLoaded;
    private static Location11_BlackRoom _room;
    private static float _sitRealtime = -1f;
    private static bool _loggedPlayPuzle;
    private static bool _loggedGlueWork;
    private static bool _loggedIncompletePut;
    private static bool _loggedIdleNoAssemble;
    private static bool _loggedCandidateDelay;
    private static bool _loggedStuckLate;
    private static bool _loggedAssembleInputBroken;
    private static string _lastPhase = "";

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
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableGhostlyPuzzle))
        {
            return;
        }

        Plugin.Log.LogInfo(
            $"[{Tag}] entered {SceneName}; GhostLock debug armed (F8 = STUCK_DUMP)",
            nameof(GhostlyPuzzleSoftlockDebug));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location11_BlackRoom), nameof(Location11_BlackRoom.PlayerSit))]
    [HarmonyPriority(Priority.High)]
    private static void PlayerSitPostfix(Location11_BlackRoom __instance)
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableGhostlyPuzzle))
        {
            return;
        }

        if (!IsGhostMitaScene() || __instance == null)
        {
            return;
        }

        _room = __instance;
        _sitRealtime = Time.realtimeSinceStartup;
        _loggedPlayPuzle = false;
        _loggedGlueWork = false;
        _loggedIncompletePut = false;
        _loggedIdleNoAssemble = false;
        _loggedCandidateDelay = false;
        _loggedStuckLate = false;
        _loggedAssembleInputBroken = false;
        _lastPhase = "";
        LogState(__instance, "PlayerSit");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location11_BlackRoom), nameof(Location11_BlackRoom.PutPuzle))]
    private static void PutPuzlePostfix(Location11_BlackRoom __instance)
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableGhostlyPuzzle))
        {
            return;
        }

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
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableGhostlyPuzzle))
        {
            return;
        }

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
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableGhostlyPuzzle))
        {
            return;
        }

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
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableGhostlyPuzzle))
        {
            return;
        }

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
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableGhostlyPuzzle))
        {
            return;
        }

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
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableGhostlyPuzzle))
        {
            return;
        }

        if (!IsGhostMitaScene() || __instance == null)
        {
            return;
        }

        LogState(__instance, "ExitTable");
        ResetSession();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), "Update")]
    private static void GameControllerUpdatePostfix()
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableGhostlyPuzzle))
        {
            return;
        }

        if (!IsGhostMitaScene() || !Input.GetKeyDown(StuckDumpKey))
        {
            return;
        }

        Plugin.Log.LogWarning(
            $"[{Tag}] STUCK_DUMP key=F8 sitAge={SitAge():0.###}s " +
            $"incompletePut={_loggedIncompletePut} idleNoAssemble={_loggedIdleNoAssemble} " +
            $"candidateDelay={_loggedCandidateDelay} stuckLate={_loggedStuckLate} " +
            $"playPuzleLogged={_loggedPlayPuzle} glueLogged={_loggedGlueWork} " +
            $"inputBroken={_loggedAssembleInputBroken} lastPhase={_lastPhase} " +
            $"timeScale={Time.timeScale:0.###}",
            nameof(GhostlyPuzzleSoftlockDebug));

        var room = _room;
        if (room == null)
        {
            room = UnityEngine.Object.FindObjectOfType<Location11_BlackRoom>();
        }

        if (room != null)
        {
            LogState(room, "STUCK_DUMP");
        }
        else
        {
            Plugin.Log.LogWarning(
                $"[{Tag}] STUCK_DUMP no Location11_BlackRoom instance",
                nameof(GhostlyPuzzleSoftlockDebug));
        }
    }
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location11_BlackRoom), "Update")]
    [HarmonyPriority(Priority.High)]
    private static void UpdatePostfix(Location11_BlackRoom __instance)
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableGhostlyPuzzle))
        {
            return;
        }

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
                var afterIdle = _loggedIdleNoAssemble || _loggedCandidateDelay ||
                                _lastPhase is "waiting-idle-timers";
                LogState(__instance, afterIdle ? "playPuzle (after idle)" : "playPuzle");
            }

            TryLogAssembleInputBroken(__instance);
            return;
        }

        TryLogIncompletePut(__instance);
        TryLogWaitingPhase(__instance);
        TryLogIdleNoAssemble(__instance);
        TryLogCandidateDelay(__instance);
        TryLogStuckLate(__instance);
    }

    private static void TryLogIncompletePut(Location11_BlackRoom room)
    {
        if (_loggedIncompletePut)
        {
            return;
        }

        var frames = room.framesFound;
        if (frames == null)
        {
            return;
        }

        for (var i = 0; i < frames.Length; i++)
        {
            var frame = frames[i];
            if (frame?.puzle == null || !frame.puzle.activeSelf || !frame.addedTable)
            {
                continue;
            }

            var paper = frame.puzle.GetComponent<Location11_PaperPart>();
            if (paper != null && paper.put)
            {
                continue;
            }

            _loggedIncompletePut = true;
            Plugin.Log.LogWarning(
                $"[{Tag}] SIG_INCOMPLETE_PUT slot={i} addedTable=true put=false " +
                $"sitAge={SitAge():0.###}s timeScale={Time.timeScale:0.###}",
                nameof(GhostlyPuzzleSoftlockDebug));
            LogState(room, $"SIG_INCOMPLETE_PUT slot={i}");
            return;
        }
    }

    private static void TryLogIdleNoAssemble(Location11_BlackRoom room)
    {
        if (_loggedIdleNoAssemble || !TimersIdle(room))
        {
            return;
        }

        _loggedIdleNoAssemble = true;
        _lastPhase = "SIG_IDLE_NO_ASSEMBLE";
        Plugin.Log.LogWarning(
            $"[{Tag}] SIG_IDLE_NO_ASSEMBLE sitAge={SitAge():0.###}s timeScale={Time.timeScale:0.###}",
            nameof(GhostlyPuzzleSoftlockDebug));
        LogState(room, "SIG_IDLE_NO_ASSEMBLE");
    }

    private static void TryLogCandidateDelay(Location11_BlackRoom room)
    {
        if (_loggedCandidateDelay || SitAge() < SoftlockFixRepairDelaySeconds || !TimersIdle(room))
        {
            return;
        }

        _loggedCandidateDelay = true;
        _lastPhase = "CANDIDATE_DELAY";
        Plugin.Log.LogWarning(
            $"[{Tag}] CANDIDATE_DELAY sitAge={SitAge():0.###}s " +
            $"(Softlock Fix RepairDelaySeconds={SoftlockFixRepairDelaySeconds}) timeScale={Time.timeScale:0.###}",
            nameof(GhostlyPuzzleSoftlockDebug));
        LogState(room, "CANDIDATE_DELAY");
    }

    private static void TryLogStuckLate(Location11_BlackRoom room)
    {
        if (_loggedStuckLate || SitAge() < SoftlockStuckLateSeconds || !TimersIdle(room))
        {
            return;
        }

        _loggedStuckLate = true;
        _lastPhase = "STUCK_LATE";
        Plugin.Log.LogWarning(
            $"[{Tag}] STUCK_LATE sitAge={SitAge():0.###}s incompletePut={_loggedIncompletePut} " +
            $"timeScale={Time.timeScale:0.###}",
            nameof(GhostlyPuzzleSoftlockDebug));
        LogState(room, "STUCK_LATE");
    }

    private static void TryLogAssembleInputBroken(Location11_BlackRoom room)
    {
        if (_loggedAssembleInputBroken)
        {
            return;
        }

        var cursorOk = room.scrgc != null && room.scrgc.showCursor;
        var planeOk = room.mouseOverPlane != null && room.mouseOverPlane.activeSelf;
        if (cursorOk && planeOk)
        {
            return;
        }

        _loggedAssembleInputBroken = true;
        Plugin.Log.LogWarning(
            $"[{Tag}] ASSEMBLE_INPUT_BROKEN showCursor={cursorOk} mouseOverPlane={planeOk} " +
            $"sitAge={SitAge():0.###}s",
            nameof(GhostlyPuzzleSoftlockDebug));
        LogState(room, "ASSEMBLE_INPUT_BROKEN");
    }

    private static void TryLogWaitingPhase(Location11_BlackRoom room)
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

    private static bool TimersIdle(Location11_BlackRoom room) =>
        room.timeStartPlayPuzle <= 0f && room.timeStartPuzle <= 0f;

    private static float SitAge() =>
        _sitRealtime >= 0f ? Time.realtimeSinceStartup - _sitRealtime : -1f;

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

        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append(Tag);
        sb.Append("] ");
        sb.Append(phase);
        sb.Append(" sitAge=");
        sb.Append(SitAge().ToString("0.###"));
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
        _loggedIncompletePut = false;
        _loggedIdleNoAssemble = false;
        _loggedCandidateDelay = false;
        _loggedStuckLate = false;
        _loggedAssembleInputBroken = false;
        _lastPhase = "";
    }

    private static bool IsGhostMitaScene() => SceneManager.GetActiveScene().name == SceneName;
}
