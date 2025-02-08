using HarmonyLib;
using SpeedrunMod.Practice;
using SpeedrunMod.Practice.MakeMannequin;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(MinigamesController))]
public class MinigamesControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MinigamesController.ExitGame))]
    public static void ExitGamePatch()
    {
        // Plugin.Log.LogInfo("Exiting game?");
        // if (PracticeManager.SelectedGame == PracticeGames.MakeMannequin)
        // {
        //     Plugin.Log.LogInfo("Exiting game");
        //     // MannequinMinigame.Reload();
        // }
    }
}