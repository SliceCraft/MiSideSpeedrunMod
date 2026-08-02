#if DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using SpeedrunMod.Configs;
using SpeedrunMod.Events;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks.Debug;

[HarmonyPatch]
internal static class KindBedroomNoteSoftlockDebug
{
    private const string Tag = "DEBUG-kind07";
    private const string SceneName = "Scene 15 - BasementAndDeath";
    private const string TakeItemsName = "AnimationPlayer TakeItems";
    private const string ShowItemsName = "AnimationPlayer ShowItems";
    private const string StayMitaName = "AnimationPlayer StayMita";
    private const string StayUpEventsName = "TimeAnimation Mita StayUp";
    private const string MitaTakeItemsTimeName = "TimeAnimation Mita TakeItems";
    private const string StartShowItemsTimeName = "TimeAnimation Mita StartShowItems";
    private const string HintKeyName = "3D HintKey";
    private const string Dialogue12Name = "PlayerDialogue 12 - Continue";
    private const string Dialogue14Name = "PlayerDialogue 14 - Continue";
    private const string Quest4Name = "Quest 4 Ядро";
    private const string PaperPasswordName = "PaperPassword";
    private const string StartuseName = "Startuse";
    private const KeyCode StuckDumpKey = KeyCode.F8;
    private const int MaxBufferedEvents = 256;

    private static readonly string[] StateDumpNames =
    {
        TakeItemsName,
        ShowItemsName,
        StayMitaName,
        StayUpEventsName,
        MitaTakeItemsTimeName,
        StartShowItemsTimeName,
        HintKeyName,
        Dialogue12Name,
        Dialogue14Name,
        Quest4Name,
        PaperPasswordName,
        StartuseName,
        "Ring",
        "Quest 3 Мита ждет",
    };

    private static readonly List<string> _buffer = new(MaxBufferedEvents);

    private static bool _subscribedToSceneLoaded;
    private static float _sceneRealtime = -1f;
    private static float _takeItemsRealtime = -1f;
    private static bool _sawShowItems;
    private static bool _sawHintKey;
    private static bool _sawTakeItems;
    private static bool _sawDialogue12;
    private static bool _sawDialogue14;
    private static bool _sawStayMita;
    private static bool _sawStayUp;
    private static bool _sawQuest4;
    private static bool _loggedHandoffOk;
    private static bool _armed;

    private static GameObject _hintKey;
    private static GameObject _dialogue12;
    private static GameObject _dialogue14;
    private static GameObject _quest4;
    private static bool _cacheValid;

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
            _armed = false;
            return;
        }

        ResetSessionFlags();
        CacheSceneObjects();
        _armed = SoftlockConfig.IsEnabled(SoftlockConfig.EnableKindBedroomNote);
        if (!_armed)
        {
            return;
        }

        _sceneRealtime = Time.realtimeSinceStartup;
        Record($"entered {SceneName}; Softlock Debug buffering (F8=FLUSH_BUFFER + STUCK_DUMP)");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ObjectAnimationPlayer), nameof(ObjectAnimationPlayer.AnimationPlay))]
    private static void AnimationPlayPostfix(ObjectAnimationPlayer __instance)
    {
        if (!_armed || __instance == null)
        {
            return;
        }

        string name = SafeName(__instance);
        if (name == ShowItemsName)
        {
            _sawShowItems = true;
            Record("SHOWITEMS_START");
            return;
        }

        if (name == TakeItemsName)
        {
            _sawTakeItems = true;
            _takeItemsRealtime = Time.realtimeSinceStartup;
            _loggedHandoffOk = false;
            Record($"TAKEITEMS_START playerAnim={DescribePlayerAnimCheap()}");
            return;
        }

        if (name == StayMitaName)
        {
            _sawStayMita = true;
            Record($"STAYMITA_START takeItemsAge={TakeItemsAge():F2}s playerAnim={DescribePlayerAnimCheap()}");
            MaybeRecordHandoffOk("StayMita");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void YieldRestartPostfix(Time_Events __instance)
    {
        if (!_armed || __instance == null)
        {
            return;
        }

        string name = SafeName(__instance);
        if (name is not (MitaTakeItemsTimeName or StayUpEventsName or StartShowItemsTimeName))
        {
            return;
        }

        if (name == StayUpEventsName)
        {
            _sawStayUp = true;
        }

        Record($"YieldRestart name={name} active={__instance.gameObject.activeInHierarchy}");

        if (name == StayUpEventsName)
        {
            MaybeRecordHandoffOk("StayUp");
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.StopAllTime))]
    private static void StopAllTimePostfix(Time_Events __instance)
    {
        if (!_armed || __instance == null)
        {
            return;
        }

        string name = SafeName(__instance);
        if (name is not (MitaTakeItemsTimeName or StayUpEventsName or StartShowItemsTimeName))
        {
            return;
        }

        Record($"StopAllTime name={name} {DescribeFlags()}");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Dialogue_3DText), "Start")]
    private static void DialogueStartPostfix(Dialogue_3DText __instance)
    {
        if (!_armed || __instance == null)
        {
            return;
        }

        int index = __instance.indexString;
        if (index is < 40 or > 60)
        {
            return;
        }

        string name = SafeName(__instance);
        Record($"DialogueStart name={name} index={index}");

        if (name == Dialogue12Name || index == 46)
        {
            _sawDialogue12 = true;
        }

        if (name == Dialogue14Name || index == 50)
        {
            _sawDialogue14 = true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameController), "Update")]
    private static void GameControllerUpdatePostfix()
    {
        if (!_armed)
        {
            return;
        }

        if (_cacheValid)
        {
            if (IsCachedActive(_hintKey))
            {
                _sawHintKey = true;
            }

            if (IsCachedActive(_dialogue12))
            {
                _sawDialogue12 = true;
            }

            if (IsCachedActive(_dialogue14))
            {
                _sawDialogue14 = true;
            }

            if (IsCachedActive(_quest4))
            {
                _sawQuest4 = true;
                MaybeRecordHandoffOk("Quest4");
            }
        }

        if (Input.GetKeyDown(StuckDumpKey))
        {
            FlushBufferToLog(stuck: true);
        }
    }

    private static void MaybeRecordHandoffOk(string seam)
    {
        if (_loggedHandoffOk || !_sawTakeItems)
        {
            return;
        }

        if (!_sawQuest4 && !_sawStayMita && seam != "StayUp")
        {
            return;
        }

        _loggedHandoffOk = true;
        Record(
            $"HANDOFF_OK seam={seam} takeItemsAge={TakeItemsAge():F2}s {DescribeFlags()} " +
            $"playerAnim={DescribePlayerAnimCheap()}");
    }

    private static void Record(string message)
    {
        if (_buffer.Count >= MaxBufferedEvents)
        {
            _buffer.RemoveAt(0);
        }

        float t = _sceneRealtime < 0f ? 0f : Time.realtimeSinceStartup - _sceneRealtime;
        _buffer.Add($"t={t:F3}s {message}");
    }

    private static void FlushBufferToLog(bool stuck)
    {
        string verdict = _loggedHandoffOk
            ? "HANDOFF_OK"
            : _sawTakeItems
                ? "NO_HANDOFF"
                : "NO_TAKEITEMS";

        Plugin.Log.LogWarning(
            $"[{Tag}] {(stuck ? "STUCK_DUMP" : "FLUSH")} key=F8 verdict={verdict} " +
            $"events={_buffer.Count} {DescribeFlags()} playerAnim={DescribePlayerAnimCheap()}",
            nameof(KindBedroomNoteSoftlockDebug));

        for (int i = 0; i < _buffer.Count; i++)
        {
            Plugin.Log.LogInfo($"[{Tag}] BUF[{i}] {_buffer[i]}", nameof(KindBedroomNoteSoftlockDebug));
        }

        TryDumpHeavy(stuck ? "STUCK_DUMP" : "FLUSH");
    }

    private static void CacheSceneObjects()
    {
        _hintKey = ComponentUtil.FindIncludingInactive(HintKeyName);
        _dialogue12 = ComponentUtil.FindIncludingInactive(Dialogue12Name);
        _dialogue14 = ComponentUtil.FindIncludingInactive(Dialogue14Name);
        _quest4 = ComponentUtil.FindIncludingInactive(Quest4Name);
        _cacheValid = true;
    }

    private static void TryDumpHeavy(string reason)
    {
        var sb = new StringBuilder();
        sb.Append($"[{Tag}] STATE reason={reason}");
        foreach (string name in StateDumpNames)
        {
            GameObject go = ComponentUtil.FindIncludingInactive(name);
            if (go == null)
            {
                sb.Append($" | {name}=MISSING");
                continue;
            }

            sb.Append($" | {name}=self:{go.activeSelf}/hier:{go.activeInHierarchy}");
        }

        sb.Append($" | {DescribeFlags()} | playerAnim={DescribePlayerAnimCheap()}");
        Plugin.Log.LogInfo(sb.ToString(), nameof(KindBedroomNoteSoftlockDebug));
    }

    private static string DescribeFlags() =>
        $"showItems={_sawShowItems} hintKey={_sawHintKey} takeItems={_sawTakeItems} " +
        $"d12={_sawDialogue12} d14={_sawDialogue14} stayMita={_sawStayMita} stayUp={_sawStayUp} quest4={_sawQuest4}";

    private static string DescribePlayerAnimCheap()
    {
        try
        {
            PlayerMove player = UnityEngine.Object.FindObjectOfType<PlayerMove>();
            if (player == null)
            {
                return "player=null";
            }

            string animName = player.scrAnimationNow != null && player.scrAnimationNow.gameObject != null
                ? player.scrAnimationNow.gameObject.name
                : "none";
            return $"run={player.animationRun} now={animName}";
        }
        catch (Exception ex)
        {
            return $"err:{ex.GetType().Name}";
        }
    }

    private static float TakeItemsAge() =>
        _takeItemsRealtime < 0f ? -1f : Time.realtimeSinceStartup - _takeItemsRealtime;

    private static bool IsCachedActive(GameObject go)
    {
        try
        {
            return go != null && go.activeInHierarchy;
        }
        catch
        {
            return false;
        }
    }

    private static void ResetSessionFlags()
    {
        _buffer.Clear();
        _sceneRealtime = -1f;
        _takeItemsRealtime = -1f;
        _sawShowItems = false;
        _sawHintKey = false;
        _sawTakeItems = false;
        _sawDialogue12 = false;
        _sawDialogue14 = false;
        _sawStayMita = false;
        _sawStayUp = false;
        _sawQuest4 = false;
        _loggedHandoffOk = false;
        _hintKey = null;
        _dialogue12 = null;
        _dialogue14 = null;
        _quest4 = null;
        _cacheValid = false;
    }

    private static string SafeName(Component c)
    {
        try
        {
            return c != null && c.gameObject != null ? c.gameObject.name : "?";
        }
        catch
        {
            return "?";
        }
    }
}
#endif
