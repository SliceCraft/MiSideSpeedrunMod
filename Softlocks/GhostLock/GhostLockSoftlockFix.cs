using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Softlocks.GhostLock;

/// <summary>
/// GhostLock Softlock Fix (Scene 11 — Backrooms / GhostMita).
///
/// After <see cref="Location11_BlackRoom.PlayerSit"/>, Update waits on
/// <c>timeStartPlayPuzle</c> then either places remaining pieces (animation →
/// <c>PutPuzle</c>) or latches <c>playPuzle</c> + cursor + <c>mouseOverPlane</c>.
/// Skipping/pausing mid-handoff can leave <c>addedTable</c> set while play mode
/// never enables, so pieces stay unusable.
///
/// Strategy: repair interactable/quest state (prefer over block-skip).
/// </summary>
[HarmonyPatch(typeof(Location11_BlackRoom))]
internal static class GhostLockSoftlockFix
{
    private const string GhostScene = "Scene 11 - Backrooms";
    private const float RepairDelaySeconds = 1.25f;

    private static Location11_BlackRoom _pendingRoom;
    private static float _sitRealtime;
    private static bool _repairedThisSit;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Location11_BlackRoom.PlayerSit))]
    private static void PlayerSitPostfix(Location11_BlackRoom __instance)
    {
        if (__instance == null || !IsGhostScene())
        {
            return;
        }

        _pendingRoom = __instance;
        _sitRealtime = Time.realtimeSinceStartup;
        _repairedThisSit = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    private static void UpdatePostfix(Location11_BlackRoom __instance)
    {
        if (__instance == null || !IsGhostScene())
        {
            return;
        }

        if (__instance.glueWork)
        {
            ClearPending(__instance);
            return;
        }

        if (__instance.playPuzle)
        {
            EnsureAssembleInputUsable(__instance);
            ClearPending(__instance);
            return;
        }

        if (_pendingRoom != __instance || _repairedThisSit)
        {
            return;
        }

        if (Time.realtimeSinceStartup - _sitRealtime < RepairDelaySeconds)
        {
            return;
        }

        // Still waiting on the vanilla place-piece delay/animation — let it finish.
        if (__instance.timeStartPlayPuzle > 0f || __instance.timeStartPuzle > 0f)
        {
            return;
        }

        TryRepairAssembleMode(__instance, "post-sit timeout");
    }

    private static void TryRepairAssembleMode(Location11_BlackRoom room, string reason)
    {
        try
        {
            FinishPendingPlacements(room);
            EnableAssembleMode(room);
            _repairedThisSit = true;
            Plugin.Log.LogInfo($"GhostLock Softlock Fix: repaired assemble mode ({reason})");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"GhostLock Softlock Fix repair failed: {ex}");
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
            Location11_BlackRoom_Puzle frame = frames[i];
            if (frame?.puzle == null || !frame.puzle.activeSelf)
            {
                continue;
            }

            Location11_PaperPart paper = frame.puzle.GetComponent<Location11_PaperPart>();
            // Vanilla sets addedTable before animationAddPaper → PutPuzle. Softlock case
            // is often addedTable already true while Put() never ran (paper.put still false).
            bool placementIncomplete = paper == null || !paper.put;
            if (frame.addedTable && !placementIncomplete)
            {
                continue;
            }

            room.indexPuzleWork = i;
            frame.addedTable = true;
            room.PutPuzle();
        }
    }

    /// <summary>
    /// Mirrors the Ghidra path in <c>Location11_BlackRoom.Update</c> when
    /// <c>timeStartPlayPuzle</c> expires and every active slot is already
    /// <c>addedTable</c>.
    /// </summary>
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

        Interface_KeyHint_Key buttonExit = room.buttonExit;
        if (buttonExit != null)
        {
            Transform hintTransform = buttonExit.transform;
            if (hintTransform != null && hintTransform.parent != null)
            {
                hintTransform.parent.gameObject.SetActive(true);
            }

            buttonExit.hide = false;
        }
    }

    private static void EnsureAssembleInputUsable(Location11_BlackRoom room)
    {
        try
        {
            if (room.scrgc != null && !room.scrgc.showCursor)
            {
                room.scrgc.ShowCursor(true);
                Plugin.Log.LogInfo("GhostLock Softlock Fix: re-enabled cursor during assemble");
            }

            if (room.mouseOverPlane != null && !room.mouseOverPlane.activeSelf)
            {
                room.mouseOverPlane.SetActive(true);
                Plugin.Log.LogInfo("GhostLock Softlock Fix: re-enabled mouseOverPlane during assemble");
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"GhostLock Softlock Fix input repair failed: {ex}");
        }
    }

    private static void ClearPending(Location11_BlackRoom room)
    {
        if (_pendingRoom == room)
        {
            _pendingRoom = null;
        }
    }

    private static bool IsGhostScene() =>
        SceneManager.GetActiveScene().name == GhostScene;
}
