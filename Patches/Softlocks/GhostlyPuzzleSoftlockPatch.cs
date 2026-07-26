using System;
using System.Text;
using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch(typeof(Location11_BlackRoom))]
internal static class GhostlyPuzzleSoftlockPatch
{
    private const string GhostMitaScene = "Scene 11 - Backrooms";
    private const float RepairDelaySeconds = 1.25f;

    private static Location11_BlackRoom _instance;
    private static float _realtimeSincePlayerSit;
    private static bool _repairApplied;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Location11_BlackRoom.PlayerSit))]
    private static void PlayerSitPostfix(Location11_BlackRoom __instance)
    {
        if (__instance == null || !IsGhostMitaScene())
        {
            return;
        }

        _instance = __instance;
        _realtimeSincePlayerSit = Time.realtimeSinceStartup;
        _repairApplied = false;
        LogState(__instance, "PlayerSit");
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    private static void UpdatePostfix(Location11_BlackRoom __instance)
    {
        try
        {
            if (__instance == null || __instance != _instance || !IsGhostMitaScene())
            {
                return;
            }

            if (__instance.glueWork)
            {
                LogState(__instance, "glueWork");
                _instance = null;
                return;
            }

            if (__instance.playPuzle)
            {
                if (!_repairApplied)
                {
                    LogState(__instance, "playPuzle (vanilla)");
                }

                EnsureAssembleInputUsable(__instance);
                _instance = null;
                return;
            }

            if (_repairApplied)
            {
                return;
            }

            // realtime (not scaled): sit starts 0.25s play timer + optional place-piece
            // animations; don't repair until that window can finish even if timeScale is low.
            if (Time.realtimeSinceStartup - _realtimeSincePlayerSit < RepairDelaySeconds)
            {
                return;
            }

            // Vanilla still driving place/play timers — wait for them to go idle.
            if (__instance.timeStartPlayPuzle > 0f || __instance.timeStartPuzle > 0f)
            {
                return;
            }

            TryRepairAssembleMode(__instance);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Update failed: {ex}", nameof(GhostlyPuzzleSoftlockPatch));
        }
    }

    private static void TryRepairAssembleMode(Location11_BlackRoom room)
    {
        try
        {
            LogState(room, "repair before");
            FinishPendingPlacements(room);
            EnableAssembleMode(room);
            _repairApplied = true;
            Plugin.Log.LogInfo("repaired assemble mode", nameof(GhostlyPuzzleSoftlockPatch));
            LogState(room, "repair after");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"repair failed: {ex}", nameof(GhostlyPuzzleSoftlockPatch));
        }
    }

    private static void FinishPendingPlacements(Location11_BlackRoom room)
    {
        var frames = room.framesFound;
        if (frames == null)
        {
            return;
        }

        for (var i = 0; i < frames.Length; i++)
        {
            var frame = frames[i];
            if (frame?.puzle == null || !frame.puzle.activeSelf)
            {
                continue;
            }

            var paper = frame.puzle.GetComponent<Location11_PaperPart>();
            // Vanilla sets addedTable before animationAddPaper → PutPuzle; Softlock often
            // leaves addedTable true while Put() never ran (paper.put still false).
            var placementComplete = paper != null && paper.put;
            if (frame.addedTable && placementComplete)
            {
                continue;
            }

            room.indexPuzleWork = i;
            frame.addedTable = true;
            room.PutPuzle();
        }
    }

    // Same enable path as Location11_BlackRoom.Update when play timer expires cleanly.
    private static void EnableAssembleMode(Location11_BlackRoom room)
    {
        room.timeStartPlayPuzle = 0f;
        room.timeStartPuzle = 0f;
        room.indexPuzleHold = -1;

        if (room.scrgc != null)
        {
            room.scrgc.ShowCursor(true);
        }

        room.playPuzle = true;

        if (room.mouseOverPlane != null)
        {
            room.mouseOverPlane.SetActive(true);
        }

        var buttonExit = room.buttonExit;
        if (buttonExit != null)
        {
            var hintTransform = buttonExit.transform;
            if (hintTransform != null && hintTransform.parent != null)
            {
                hintTransform.parent.gameObject.SetActive(true);
            }

            buttonExit.hide = false;
        }
    }

    private static void EnsureAssembleInputUsable(Location11_BlackRoom room)
    {
        if (room.scrgc != null && !room.scrgc.showCursor)
        {
            room.scrgc.ShowCursor(true);
            Plugin.Log.LogInfo("re-enabled cursor during assemble", nameof(GhostlyPuzzleSoftlockPatch));
        }

        if (room.mouseOverPlane != null && !room.mouseOverPlane.activeSelf)
        {
            room.mouseOverPlane.SetActive(true);
            Plugin.Log.LogInfo("re-enabled mouseOverPlane during assemble", nameof(GhostlyPuzzleSoftlockPatch));
        }
    }

    private static bool IsGhostMitaScene() => SceneManager.GetActiveScene().name == GhostMitaScene;

    private static void LogState(Location11_BlackRoom room, string phase) =>
        Plugin.Log.LogInfo(DescribeState(room, phase), nameof(GhostlyPuzzleSoftlockPatch));

    private static string DescribeState(Location11_BlackRoom room, string phase)
    {
        var showCursor = room.scrgc != null && room.scrgc.showCursor;
        var mouseOverPlaneActive = room.mouseOverPlane != null && room.mouseOverPlane.activeSelf;
        var interactiveTableActive = room.interactiveTable != null && room.interactiveTable.activeSelf;

        var sb = new StringBuilder();
        sb.Append(phase);
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

                sb.Append(i);
                sb.Append(":active=");
                sb.Append(frame.puzle.activeSelf);
                sb.Append(" added=");
                sb.Append(frame.addedTable);
                sb.Append(" put=");
                sb.Append(put);
                sb.Append(" mouse=");
                sb.Append(mouse);
            }
        }

        sb.Append(']');
        return sb.ToString();
    }
}
