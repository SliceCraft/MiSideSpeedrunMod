using System.Text;
using HarmonyLib;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Patches.Softlocks;

/// <summary>
/// Temporary instrumentation for ticket 08 retest. Tag: [DEBUG-kappi08]. Remove after diagnosis.
/// </summary>
[HarmonyPatch]
internal static class KappiSoftlockDebugPatch
{
    private const string DebugTag = "[DEBUG-kappi08]";
    private const string Scene7 = "Scene 7 - Backrooms";
    private const string Scene6 = "Scene 6 - BasementFirst";
    private const string ContinueName = "Continue";

    private static bool _sceneHooks;

    private static string T() => $"t={Time.realtimeSinceStartup:F3}";

    static KappiSoftlockDebugPatch()
    {
        EnsureSceneHooks();
    }

    private static void EnsureSceneHooks()
    {
        if (_sceneHooks)
        {
            return;
        }

        _sceneHooks = true;
        SceneManager.sceneLoaded += (UnityEngine.Events.UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
        SceneManager.sceneUnloaded += (UnityEngine.Events.UnityAction<Scene>)OnSceneUnloaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Plugin.Log.LogInfo(
            $"{DebugTag} {T()} sceneLoaded name={scene.name} mode={mode} "
            + $"active={SceneManager.GetActiveScene().name} "
            + $"scene6={Scene6Loaded()} scene7={Scene7Loaded()} {SceneSnapshot()} "
            + PlayerAnimSnapshot(),
            nameof(KappiSoftlockDebugPatch));
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        Plugin.Log.LogInfo(
            $"{DebugTag} {T()} sceneUnloaded name={scene.name} "
            + $"active={SceneManager.GetActiveScene().name} "
            + $"scene6={Scene6Loaded()} scene7={Scene7Loaded()} {SceneSnapshot()}",
            nameof(KappiSoftlockDebugPatch));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Dialogue_3DText), "Start")]
    private static void DialogueStartPostfix(Dialogue_3DText __instance)
    {
        EnsureSceneHooks();

        if (__instance == null)
        {
            return;
        }

        string name = __instance.gameObject.name;
        int idx = __instance.indexString;

        // Milestone dialogues for unload / room / ring Softlock Fix windows.
        bool milestone =
            (name.StartsWith("TextMita") && idx is 130 or 136 or 137 or 347)
            || (name == CapMitaName && idx == 140)
            || (name == SitDialogueName && idx == 236)
            || (name.StartsWith("KindMita") && idx is 235 or 236);

        bool watched =
            milestone
            || name.StartsWith("CapMita")
            || name.StartsWith("KindMita")
            || name.StartsWith("TextMita")
            || name == "Player 4";

        if (!watched)
        {
            return;
        }

        string msg =
            $"{DebugTag} {T()} Dialogue.Start name={name} idx={idx} "
            + $"activeSelf={__instance.gameObject.activeSelf} "
            + $"activeScene={SceneManager.GetActiveScene().name} "
            + $"scene7Loaded={Scene7Loaded()} scene6Loaded={Scene6Loaded()} "
            + $"isKappiActiveCheck={IsActiveScene7()} "
            + $"{ContinueSnapshot()} {PlayerAnimSnapshot()} {SceneSnapshot()}";

        if (milestone)
        {
            Plugin.Log.LogInfo(msg, nameof(KappiSoftlockDebugPatch));
            if (Scene6Loaded())
            {
                Plugin.Log.LogInfo(
                    $"{DebugTag} {T()} SCENE6-STILL-LOADED at dialogue {name} idx={idx}",
                    nameof(KappiSoftlockDebugPatch));
            }
        }
        else
        {
            Plugin.Log.LogDebug(msg, nameof(KappiSoftlockDebugPatch));
        }
    }

    private const string CapMitaName = "CapMita 1";
    private const string SitDialogueName = "KindMita 15";

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Scene_Load), nameof(Scene_Load.UnloadScene))]
    private static void UnloadScenePrefix(string _nameScene)
    {
        Plugin.Log.LogInfo(
            $"{DebugTag} {T()} Scene_Load.UnloadScene('{_nameScene}') "
            + $"active={SceneManager.GetActiveScene().name} "
            + $"{ContinueSnapshot()} {PlayerAnimSnapshot()} {SceneSnapshot()}",
            nameof(KappiSoftlockDebugPatch));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Player_EventWhenAnimationStop), nameof(Player_EventWhenAnimationStop.Play))]
    private static void ContinuePlayPostfix(Player_EventWhenAnimationStop __instance)
    {
        if (__instance == null)
        {
            return;
        }

        string goName = __instance.gameObject.name;
        Plugin.Log.LogInfo(
            $"{DebugTag} {T()} Player_EventWhenAnimationStop.Play go={goName} "
            + $"play={__instance.play} activeSelf={__instance.gameObject.activeSelf} "
            + $"scene6={Scene6Loaded()} scene7={Scene7Loaded()} "
            + $"{PlayerAnimSnapshot()} {SceneSnapshot()}",
            nameof(KappiSoftlockDebugPatch));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location7_RingWork), "Start")]
    private static void RingWorkStartPostfix(Location7_RingWork __instance)
    {
        Plugin.Log.LogInfo(
            $"{DebugTag} {T()} Location7_RingWork.Start activeInHierarchy={__instance.gameObject.activeInHierarchy} "
            + $"activeScene={SceneManager.GetActiveScene().name} {SceneSnapshot()}",
            nameof(KappiSoftlockDebugPatch));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Location34_Communication), nameof(Location34_Communication.StartAddon))]
    private static void StartAddonPostfix(Location34_Communication __instance)
    {
        if (__instance == null || !Scene7Loaded())
        {
            return;
        }

        GameObject house = null;
        Transform houseT = __instance.transform.Find("House");
        if (houseT != null)
        {
            house = houseT.gameObject;
        }

        GameObject capDoor = ComponentUtil.FindIncludingInactive("DoorCage Bedroom-Hall");
        GameObject beyondDoor = ComponentUtil.FindIncludingInactive("Door InRoom");
        GameObject beyondCage = ComponentUtil.FindIncludingInactive("Doorcage InRoom");
        ObjectDoor capPhysic = capDoor != null ? capDoor.GetComponentInChildren<ObjectDoor>(true) : null;
        Plugin.Log.LogInfo(
            $"{DebugTag} {T()} Location34.StartAddon go={__instance.gameObject.name} "
            + $"active={__instance.gameObject.activeSelf}/hier={__instance.gameObject.activeInHierarchy} "
            + $"house={(house == null ? "null" : $"active={house.activeSelf}")} "
            + $"capDoorCage={(capDoor == null ? "null" : $"active={capDoor.activeSelf}")} "
            + $"capDoorOpen={(capPhysic == null ? "null" : capPhysic.open.ToString())} "
            + $"beyondDoorInRoom={(beyondDoor == null ? "null" : $"active={beyondDoor.activeSelf}")} "
            + $"beyondDoorCage={(beyondCage == null ? "null" : $"active={beyondCage.activeSelf}")} "
            + SceneSnapshot(),
            nameof(KappiSoftlockDebugPatch));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Time_Events), nameof(Time_Events.YieldRestart))]
    private static void YieldRestartPostfix(Time_Events __instance)
    {
        if (__instance == null)
        {
            return;
        }

        string name = __instance.gameObject.name;
        if (name is not ("Time Mita Sit" or "TimeAnimationMitaK StandUp" or "TimeAnimation MitaOpenDoor" or "Встаёт"))
        {
            return;
        }

        GameObject ring = ComponentUtil.FindIncludingInactive("RingWork");
        GameObject quest4 = ComponentUtil.FindIncludingInactive("Quest4 - Проводим время с Кепкой");
        Plugin.Log.LogInfo(
            $"{DebugTag} {T()} YieldRestart on '{name}' "
            + $"ringWork={(ring == null ? "null" : $"active={ring.activeSelf}/hier={ring.activeInHierarchy}")} "
            + $"quest4={(quest4 == null ? "null" : $"active={quest4.activeSelf}/hier={quest4.activeInHierarchy}")} "
            + $"scene6={Scene6Loaded()} {PlayerAnimSnapshot()} "
            + $"activeScene={SceneManager.GetActiveScene().name} {SceneSnapshot()}",
            nameof(KappiSoftlockDebugPatch));
    }

    internal static void LogRepairAttempt(string patch, string detail)
    {
        Plugin.Log.LogInfo(
            $"{DebugTag} {T()} REPAIR {patch}: {detail} "
            + $"activeScene={SceneManager.GetActiveScene().name} "
            + $"isKappiActiveCheck={IsActiveScene7()} scene7Loaded={Scene7Loaded()} scene6Loaded={Scene6Loaded()} "
            + $"{ContinueSnapshot()} {PlayerAnimSnapshot()} {SceneSnapshot()}",
            nameof(KappiSoftlockDebugPatch));
    }

    internal static void LogGateMiss(string patch, string reason, Dialogue_3DText dialogue)
    {
        string name = dialogue != null ? dialogue.gameObject.name : "?";
        int idx = dialogue != null ? dialogue.indexString : -1;
        Plugin.Log.LogInfo(
            $"{DebugTag} {T()} GATE-MISS {patch}: {reason} name={name} idx={idx} "
            + $"activeScene={SceneManager.GetActiveScene().name} "
            + $"isKappiActiveCheck={IsActiveScene7()} scene7Loaded={Scene7Loaded()} scene6Loaded={Scene6Loaded()} "
            + PlayerAnimSnapshot(),
            nameof(KappiSoftlockDebugPatch));
    }

    private static string ContinueSnapshot()
    {
        GameObject cont = ComponentUtil.FindIncludingInactive(ContinueName);
        if (cont == null)
        {
            return "continue=null";
        }

        var stop = cont.GetComponent<Player_EventWhenAnimationStop>();
        return stop == null
            ? $"continue=active={cont.activeSelf}/hier={cont.activeInHierarchy} stop=null"
            : $"continue=active={cont.activeSelf}/hier={cont.activeInHierarchy} play={stop.play}";
    }

    private static string PlayerAnimSnapshot()
    {
        var player = Object.FindObjectOfType<PlayerMove>();
        if (player == null)
        {
            return "player=null";
        }

        string animObj = player.scrAnimationNow != null ? player.scrAnimationNow.gameObject.name : "none";
        return $"playerAnimRun={player.animationRun} animObj={animObj}";
    }

    private static bool IsActiveScene7() => SceneManager.GetActiveScene().name == Scene7;

    private static bool Scene7Loaded() => IsSceneLoaded(Scene7);

    private static bool Scene6Loaded() => IsSceneLoaded(Scene6);

    private static bool IsSceneLoaded(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == sceneName)
            {
                return true;
            }
        }

        return false;
    }

    private static string SceneSnapshot()
    {
        var sb = new StringBuilder("scenes=[");
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene s = SceneManager.GetSceneAt(i);
            if (i > 0)
            {
                sb.Append("; ");
            }

            sb.Append(s.name).Append(s.isLoaded ? "" : "(unloading)");
        }

        sb.Append(']');
        return sb.ToString();
    }
}
