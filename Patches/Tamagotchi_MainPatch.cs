using HarmonyLib;
using SpeedrunMod.Practice;
using SpeedrunMod.Practice.StartOfGame;
using UnityEngine;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(Tamagotchi_Main))]
public class Tamagotchi_MainPatch
{
    [HarmonyPatch(nameof(Tamagotchi_Main.GameStart))]
    [HarmonyPrefix]
    public static void GameStartPatch()
    {
        if (PracticeManager.SelectedGame == PracticeGames.FullTamagotchiRun)
        {
            FullTamagotchiRun.TamagotchiLoaded();
        }
    }
}