using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.UnityEngine;
using UnityEngine;

namespace SpeedrunMod.Patches
{
    [HarmonyPatch(typeof(SteamManager))]
    internal class SteamManagerPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void UpdatePatch()
        {
            if(UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt) && UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.O)) {
                Plugin.Log.LogInfo("Revealing triggers");
                Triggers.RevealTriggers();
            }
            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt) && UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.P))
            {
                Plugin.Log.LogInfo("Hiding triggers");
                Triggers.HideTriggers();
            }
        }
    }
}
