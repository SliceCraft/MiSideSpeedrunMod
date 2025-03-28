using HarmonyLib;
using SpeedrunMod.Practice;
using SpeedrunMod.Practice.ReadingBooks;
using UnityEngine;
using UnityEngine.Events;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(Location19_GlitchGame))]
public class Location19_GlitchGamePatch
{
    [HarmonyPatch(nameof(Location19_GlitchGame.StopGame))]
    [HarmonyPrefix]
    public static void StopGamePatch(Location19_GlitchGame __instance)
    {
        if (PracticeManager.SelectedGame == PracticeGames.MilaMinigames)
        {
            MilaMinigames.GameEnded(__instance);
        }
    }
}