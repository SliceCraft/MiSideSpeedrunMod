using System;
using HarmonyLib;
using SpeedrunMod.Configs;
using SpeedrunMod.Notifications;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch(typeof(Location11_BlackRoom))]
internal static class GhostlyPuzzleSoftlockPatch
{
    private const string GhostMitaScene = "Scene 11 - Backrooms";
    private const string Notification = "Softlock Fix: Ghostly puzzle";
    private const float RepairDelaySeconds = 1.25f;

    private static Location11_BlackRoom _instance;
    private static float _realtimeSincePlayerSit;
    private static bool _repairApplied;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Location11_BlackRoom.PlayerSit))]
    private static void PlayerSitPostfix(Location11_BlackRoom __instance)
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableGhostlyPuzzle))
        {
            return;
        }

        if (__instance == null || !IsGhostMitaScene())
        {
            return;
        }

        _instance = __instance;
        _realtimeSincePlayerSit = Time.realtimeSinceStartup;
        _repairApplied = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    private static void UpdatePostfix(Location11_BlackRoom __instance)
    {
        try
        {
            if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableGhostlyPuzzle))
            {
                _instance = null;
                _realtimeSincePlayerSit = 0f;
                _repairApplied = false;
                return;
            }

            if (__instance == null || __instance != _instance || !IsGhostMitaScene())
            {
                return;
            }

            if (__instance.glueWork)
            {
                _instance = null;
                return;
            }

            if (__instance.playPuzle)
            {
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
            FinishPendingPlacements(room);
            EnableAssembleMode(room);
            _repairApplied = true;
            NotificationManager.Show(new NotificationMessage(Notification, cooldown: 5f));
            Plugin.Log.LogInfo("repaired assemble mode", nameof(GhostlyPuzzleSoftlockPatch));
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
            Plugin.Log.LogInfo($"finished pending placement slot={i}", nameof(GhostlyPuzzleSoftlockPatch));
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
}
