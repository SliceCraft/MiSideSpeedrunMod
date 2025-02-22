using HarmonyLib;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(MinigamesController))]
internal class MinigamesControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MinigamesController.ExitGame))]
    private static void ExitGamePatch()
    {
        // Plugin.Log.LogInfo("Exiting game?");
        // if (PracticeManager.SelectedGame == PracticeGames.MakeMannequin)
        // {
        //     Plugin.Log.LogInfo("Exiting game");
        //     // MannequinMinigame.Reload();
        // }
    }
}