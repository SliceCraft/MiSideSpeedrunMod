using SpeedrunMod.ModSettings;
using SpeedrunMod.Practice;
using SpeedrunMod.RevealSystems;
using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Events
{
    internal class SceneLoadedEvent
    {
        public static void RegisterEvent()
        {
            SceneManager.sceneLoaded += (UnityEngine.Events.UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Refer to ModSettings/MonoGUITest for why this is disabled
            // if(!MonoGUIAvailable()) AttachMonoGUI();
            if (scene.name == "SceneMenu")
            {
                VersionText.Start();
                PracticeManager.SelectedGame = PracticeGames.None;
                if (PlaceStop.IsRevealing())
                {
                    PlaceStop.HidePlaceStops();
                }

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
            if (PlaceStop.IsRevealing())
            {
                Plugin.Log.LogInfo("Revealing newly loaded placestops");
                PlaceStop.RevealPlaceStops();
            }
            PracticeManager.OnSceneLoad(scene);
        }
        
        // Refer to ModSettings/MonoGUITest for why this is disabled
        // private static bool MonoGUIAvailable()
        // {
        //     return Object.FindObjectOfType<MonoGUITest>() != null;
        // }
        
        // Refer to ModSettings/MonoGUITest for why this is disabled
        // private static void AttachMonoGUI()
        // {
        //     SteamManager sm = Object.FindObjectOfType<SteamManager>();
        //     if (sm == null)
        //     {
        //         Plugin.Log.LogError("Unable to find the SteamManager, can't attach MonoGUI");
        //         return;
        //     }
        //
        //     sm.gameObject.AddComponent<MonoGUITest>();
        // }
    }
}
