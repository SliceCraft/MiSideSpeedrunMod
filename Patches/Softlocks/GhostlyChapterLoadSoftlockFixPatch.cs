using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch(typeof(World))]
internal static class GhostlyChapterLoadSoftlockFixPatch
{
    private const string SceneName = "Scene 11 - Backrooms";
    private const string Room9Name = "Room 9 (Picture)";
    private const int GhostMitaLevelLoad = 1;
    private static readonly Vector3 ChapterSpawn = new(0.5f, 0f, 0f);
    private const float ChapterSpawnRotation = 90f;
    private const float NearSpawnSqr = 25f;

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    private static void StartPostfix(World __instance)
    {
        if (__instance == null || !IsBackroomsScene() || GlobalGame.levelLoad != GhostMitaLevelLoad)
        {
            return;
        }

        try
        {
            PlayerMove player = UnityEngine.Object.FindObjectOfType<PlayerMove>();
            if (player == null || IsNear(player.transform.position, ChapterSpawn))
            {
                return;
            }

            SetRoomActive();
            Physics.SyncTransforms();
            TeleportToChapterSpawn(player);
            Plugin.Log.LogInfo("Ghostly Softlock Fix: repaired chapter-load spawn");
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Ghostly Softlock Fix failed: {ex}");
        }
    }

    private static void SetRoomActive()
    {
        GameObject room9 = FindIncludingInactive(Room9Name);
        if (room9 == null)
        {
            return;
        }

        var arks = room9.GetComponent<Location11_InfinityArks>();
        if (arks != null)
        {
            arks.LookPictureOnlyRoom();
            return;
        }

        room9.SetActive(true);
        room9.transform.position = Vector3.zero;
    }

    private static void TeleportToChapterSpawn(PlayerMove player)
    {
        player.TeleportPlayer(ChapterSpawn, ChapterSpawnRotation, 0f);
        if (IsNear(player.transform.position, ChapterSpawn))
        {
            return;
        }

        // TeleportPlayer no-ops when its short down-raycast misses; place on a tall ray hit.
        Vector3 highOrigin = ChapterSpawn + Vector3.up * 20f;
        if (!Physics.Raycast(highOrigin, Vector3.down, out RaycastHit hit, 40f))
        {
            Plugin.Log.LogWarning("Ghostly Softlock Fix: no floor under chapter spawn");
            return;
        }

        player.TeleportPlayer(hit.point, ChapterSpawnRotation, 0f);
        if (IsNear(player.transform.position, hit.point))
        {
            return;
        }

        Transform t = player.transform;
        t.position = hit.point;
        t.rotation = Quaternion.Euler(0f, ChapterSpawnRotation, 0f);

        var body = player.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.rotation = t.rotation;
            body.position = hit.point;
        }
    }

    private static bool IsNear(Vector3 a, Vector3 b) => (a - b).sqrMagnitude < NearSpawnSqr;

    private static bool IsBackroomsScene() => SceneManager.GetActiveScene().name == SceneName;

    private static GameObject FindIncludingInactive(string name)
    {
        foreach (var t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t != null && t.gameObject.name == name)
            {
                return t.gameObject;
            }
        }

        return null;
    }
}
