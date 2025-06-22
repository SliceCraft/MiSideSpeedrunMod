using SpeedrunMod.Practice;
using SpeedrunMod.RevealSystems;
using SpeedrunMod.Utils;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Events;

internal static class SceneLoadedEvent
{
    internal static void RegisterEvent()
    {
        SceneManager.sceneLoaded += (UnityEngine.Events.UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
#if DEBUG
        Plugin.Log.LogInfo($"Loading scene: {scene.name}");
#endif
        if (scene.name == "SceneMenu")
        {
            VersionText.Start();
            PracticeManager.SelectedGame = PracticeGames.None;

            if (Triggers.IsRevealing())
            {
                Triggers.HideTriggers();
            }
        }
        if (Triggers.IsRevealing())
        {
            Plugin.Log.LogInfo("Revealing newly loaded triggers");
            Triggers.RevealTriggers();
        }
        PracticeManager.OnSceneLoad(scene);
    }
}