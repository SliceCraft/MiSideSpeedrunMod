using System;
using HarmonyLib;
using SpeedrunMod.Configs;
using SpeedrunMod.Notifications;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

[HarmonyPatch]
internal static class KappiRingSoftlockPatch
{
    private const string SceneName = "Scene 7 - Backrooms";

    private const string CapMitaGreetingName = "CapMita 1";
    private const int CapMitaGreetingIndex = 140;
    private const string SpeakCapMitaName = "Speak CapMita";
    private const string MitaCapName = "Mita Кепка";
    private const string StandUpEventsName = "TimeAnimationMitaK StandUp";
    private const string OpenDoorEventsName = "TimeAnimation MitaOpenDoor";
    private const string CapDoorEventsName = "MitaCap AnimDoor";

    private const string SitDialogueName = "KindMita 15";
    private const int SitDialogueIndex = 236;
    private const string TimeMitaSitName = "Time Mita Sit";
    private const string TimeMitaStandName = "Time Mita Stand";
    private const string RingWorkName = "RingWork";
    private const string Quest4Name = "Quest4 - Проводим время с Кепкой";
    private const string Quest5Name = "Quest5 - Пора уходить";
    private const string KindMitaName = "Mita Добрая";
    private const string KindPersonFutureName = "MitaPerson Future";
    private const string KindIkName = "IKLifeCharacter";
    private const string TakeRingName = "Interactive TakeRing";
    private const string TriggerNearName = "Trigger Near";
    private const string TriggerCameUpName = "Trigger Near CameUp";
    private const string HandHoldAlphaName = "Alpha";
    private const string HandHoldCheckName = "AnimationParticle Check";

    private const string EntryNotification = "Softlock Fix: Kappi entry";
    private const string RingStartNotification = "Softlock Fix: Kappi ring start";
    private const string RingEndNotification = "Softlock Fix: Kappi ring end";

    // Vanilla stand EventsOnTime: time=-0.1 + Mita Unsit (~4.33s) → interact at ~4.23s.
    private const float MinInteractDelaySeconds = 4.2f;

    private static float _interactArmedRealtime = -1f;
    private static float _interactDelaySeconds = MinInteractDelaySeconds;
    private static bool _interactApplied;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Dialogue_3DText), "Start")]
    private static void DialogueStartPostfix(Dialogue_3DText __instance)
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableKappiRing))
        {
            return;
        }

        if (!IsKappiScene() || __instance == null)
        {
            return;
        }

        string name = __instance.gameObject.name;
        int idx = __instance.indexString;

        if (name == CapMitaGreetingName && idx == CapMitaGreetingIndex)
        {
            RepairRoomEntryGreeting();
            return;
        }

        if (name == SitDialogueName && idx == SitDialogueIndex)
        {
            ClearHaloEffect();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void TimeMitaSitYieldRestartPrefix(Time_Events __instance)
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableKappiRing))
        {
            return;
        }

        if (!IsKappiScene() || __instance == null || __instance.gameObject.name != TimeMitaSitName)
        {
            return;
        }

        GameObject ringWork = ComponentUtil.FindIncludingInactive(RingWorkName);
        if (ringWork != null && ringWork.activeInHierarchy)
        {
            return;
        }

        ComponentUtil.Enable(Quest4Name, true);
        ClearHaloEffect();
        NotificationManager.Show(new NotificationMessage(RingStartNotification, cooldown: 5f));
        Plugin.Log.LogInfo("armed Quest4 for RingWork after sit", nameof(KappiRingSoftlockPatch));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void TimeMitaStandYieldRestartPostfix(Time_Events __instance)
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableKappiRing))
        {
            return;
        }

        if (!IsKappiScene() || __instance == null || __instance.gameObject.name != TimeMitaStandName)
        {
            return;
        }

        ComponentUtil.Enable(Quest5Name, true);
        ArmPostRingInteract(__instance);
        Plugin.Log.LogInfo(
            $"armed post-ring Kind Mita interact delay={_interactDelaySeconds:0.###}s",
            nameof(KappiRingSoftlockPatch));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), "Update")]
    private static void GameControllerUpdatePostfix()
    {
        if (!SoftlockConfig.IsEnabled(SoftlockConfig.EnableKappiRing))
        {
            ResetInteractionTrigger();
            return;
        }

        if (!IsKappiScene())
        {
            ResetInteractionTrigger();
            return;
        }

        try
        {
            TryEnablePostRingInteract();
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"post-ring interact Softlock Fix failed: {ex}", nameof(KappiRingSoftlockPatch));
        }
    }

    private static void ArmPostRingInteract(Time_Events standEvents)
    {
        _interactArmedRealtime = Time.realtimeSinceStartup;
        _interactDelaySeconds = ResolveInteractDelaySeconds(standEvents);
        _interactApplied = false;
    }

    private static float ResolveInteractDelaySeconds(Time_Events standEvents)
    {
        TimePoint[] points = standEvents != null ? standEvents.EventsOnTime : null;
        if (points == null || points.Length == 0)
        {
            return MinInteractDelaySeconds;
        }

        TimePoint point = points[0];
        float wait = point.time;
        if (point.timeAnimationClip != null)
        {
            wait += point.timeAnimationClip.length;
        }

        return Mathf.Max(MinInteractDelaySeconds, wait);
    }

    private static void TryEnablePostRingInteract()
    {
        if (_interactApplied || _interactArmedRealtime < 0f)
        {
            return;
        }

        if (Time.realtimeSinceStartup - _interactArmedRealtime < _interactDelaySeconds)
        {
            return;
        }

        EnableKindMitaInteract();
        _interactApplied = true;
        NotificationManager.Show(new NotificationMessage(RingEndNotification, cooldown: 5f));
        Plugin.Log.LogInfo("enabled post-ring Kind Mita interact", nameof(KappiRingSoftlockPatch));
    }

    private static void ResetInteractionTrigger()
    {
        _interactArmedRealtime = -1f;
        _interactDelaySeconds = MinInteractDelaySeconds;
        _interactApplied = false;
    }

    private static void RepairRoomEntryGreeting()
    {
        TimeUtil.StopTimeEvents(StandUpEventsName);
        TimeUtil.StopTimeEvents(OpenDoorEventsName);
        TimeUtil.StopTimeEvents(CapDoorEventsName);

        ComponentUtil.Enable(MitaCapName, true);
        ComponentUtil.FindIncludingInactive<AudioDialogue>(SpeakCapMitaName)?.ResetVoice();
        NotificationManager.Show(new NotificationMessage(EntryNotification, cooldown: 5f));
        Plugin.Log.LogInfo("repaired CapMita room-entry greeting", nameof(KappiRingSoftlockPatch));
    }

    private static void EnableKindMitaInteract()
    {
        MitaPerson mita = ComponentUtil.FindIncludingInactive<MitaPerson>(KindMitaName);
        if (mita != null)
        {
            mita.MagnetOff();

            Transform personFuture = mita.transform.Find(KindPersonFutureName);
            if (personFuture != null)
            {
                CapsuleCollider capsule = personFuture.GetComponent<CapsuleCollider>();
                if (capsule != null)
                {
                    capsule.enabled = true;
                }

                Transform ikT = personFuture.Find(KindIkName);
                Character_Look look = ikT != null ? ikT.GetComponent<Character_Look>() : null;
                if (look != null)
                {
                    look.ForwardReTransform(personFuture);
                    look.Activation(true);
                    look.ActivationRotateBody(true);
                    look.LookOnPlayer();
                }
            }
        }

        ComponentUtil.FindIncludingInactive<Trigger_Event>(TriggerNearName)?.DestroyMe();
        ComponentUtil.Enable(TriggerCameUpName, true);
        ComponentUtil.FindIncludingInactive<ObjectInteractive>(TakeRingName)?.Activation(true);
    }

    private static void ClearHaloEffect()
    {
        foreach (UI_Alpha uiAlpha in UnityEngine.Object.FindObjectsOfType<UI_Alpha>(true))
        {
            if (uiAlpha == null || uiAlpha.gameObject.name != HandHoldAlphaName)
            {
                continue;
            }

            uiAlpha.AlphaZeroInstant();
            uiAlpha.gameObject.SetActive(false);
        }

        ComponentUtil.Enable(HandHoldCheckName, false);
    }

    private static bool IsKappiScene() => SceneManager.GetActiveScene().name == SceneName;
}
