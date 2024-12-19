using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.UnityEngine;
using UnityEngine;
using SpeedrunMod.RevealSystems;
using SpeedrunMod.Toggles;

namespace SpeedrunMod.Patches
{
    [HarmonyPatch(typeof(SteamManager))]
    internal class SteamManagerPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void UpdatePatch()
        {
            EnableRunToggle.Update();
            RevealPlaceStopToggle.Update();
            RevealTriggerToggle.Update();
        }
    }
}
