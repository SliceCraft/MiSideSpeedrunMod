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

        // eventStart already armed Trigger Near; Softlock Fix finishes what EventsOnTime skips.
        ComponentUtil.Enable(Quest5Name, true);
        EnableKindMitaInteract();
        NotificationManager.Show(new NotificationMessage(RingEndNotification, cooldown: 5f));
        Plugin.Log.LogInfo("enabled post-ring Kind Mita interact", nameof(KappiRingSoftlockPatch));
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
        foreach (UI_Alpha uiAlpha in Object.FindObjectsOfType<UI_Alpha>(true))
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
