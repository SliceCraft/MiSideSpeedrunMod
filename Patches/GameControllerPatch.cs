using HarmonyLib;
using SpeedrunMod.Practice;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(GameController))]
public class GameControllerPatch
{
    [HarmonyPatch(nameof(GameController.ExitGame))]
    [HarmonyPrefix]
    private static void ExitGamePatch()
    {
        PracticeManager.SelectedGame = PracticeGames.None;
    }
}