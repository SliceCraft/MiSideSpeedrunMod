using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            if (Triggers.IsRevealing())
            {
                Plugin.Log.LogInfo("Revealing newly loaded triggers");
                Triggers.RevealTriggers();
            }
        }
    }
}
