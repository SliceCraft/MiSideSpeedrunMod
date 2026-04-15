using HarmonyLib;
using SpeedrunMod.Configs;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(ConsoleCommandsGame))]
public class ConsoleCommandsGamePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ConsoleCommandsGame.Command))]
    public static void CommandPostfix()
    {
        ModConfig.EnableDialogueSkip.Value = GlobalGame.canSkipDialogue;
    }
}