using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

/// <summary>
/// Creepy Softlock: after a DialogueChanger option, <see cref="Location12.Quest"/> arms
/// <c>TimeAniation CMita Ape N</c> while the answer dialogue chain is delayed 1.5s.
/// Fast Space-skip reaches <see cref="Location12.QuestFinish"/> (and often chase / prefinish)
/// while those ape timers are still live; their late <c>AnimationClipSimpleNext</c> fights
/// the post-answer state and Softlocks the chapter.
/// </summary>
[HarmonyPatch(typeof(Location12), nameof(Location12.QuestFinish))]
internal static class CreepySoftlockFixPatch
{
    private const string SceneName = "Scene 12 - Freak";

    private static readonly string[] ApeTimerNames =
    {
        "TimeAniation CMita Ape 1",
        "TimeAniation CMita Ape 2",
        "TimeAniation CMita Ape 3",
    };

    [HarmonyPrefix]
    private static void QuestFinishPrefix()
    {
        if (!IsFreakScene())
        {
            return;
        }

        try
        {
            ClearApeTimers();
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Creepy Softlock Fix failed: {ex}");
        }
    }

    private static void ClearApeTimers()
    {
        var stopped = false;
        foreach (string name in ApeTimerNames)
        {
            GameObject go = FindIncludingInactive(name);
            var events = go?.GetComponent<Time_Events>();
            if (events == null)
            {
                continue;
            }

            events.StopAllTime();
            stopped = true;
            Plugin.Log.LogDebug($"Creepy Softlock Fix: StopAllTime on {name}");
        }

        if (stopped)
        {
            Plugin.Log.LogInfo("Creepy Softlock Fix: cleared CMita Ape timers before QuestFinish");
        }
    }

    private static bool IsFreakScene() => SceneManager.GetActiveScene().name == SceneName;

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
