using System;
using HarmonyLib;
using SpeedrunMod.Configs;
using SpeedrunMod.Notifications;
using SpeedrunMod.Utils;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch(typeof(Location12))]
internal static class CreepyDialogueSoftlockPatch
{
    private const string SceneName = "Scene 12 - Freak";
    private const string Notification = "Softlock Fix: Creepy dialogue";

    private static readonly string[] AnimationTimers =
    {
        "TimeAniation CMita Ape 1",
        "TimeAniation CMita Ape 2",
        "TimeAniation CMita Ape 3",
    };

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Location12.QuestFinish))]
    private static void QuestFinishPrefix()
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableCreepyDialogue))
        {
            return;
        }

        if (!IsFreakScene())
        {
            return;
        }

        try
        {
            ClearAnimationTimers();
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"failed: {ex}", nameof(CreepyDialogueSoftlockPatch));
        }
    }

    private static void ClearAnimationTimers()
    {
        foreach (string name in AnimationTimers)
        {
            TimeUtil.StopTimeEvents(name);
            Plugin.Log.LogDebug($"StopTimeEvents on {name}", nameof(CreepyDialogueSoftlockPatch));
        }

        NotificationManager.Show(new NotificationMessage(Notification, cooldown: 5f));
        Plugin.Log.LogInfo("cleared animation timers before QuestFinish", nameof(CreepyDialogueSoftlockPatch));
    }

    private static bool IsFreakScene() => SceneManager.GetActiveScene().name == SceneName;
}
