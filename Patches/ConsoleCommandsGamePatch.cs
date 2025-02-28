using HarmonyLib;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(ConsoleCommandsGame))]
public class ConsoleCommandsGamePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ConsoleCommandsGame.Command))]
    public static void CommandPostfix()
    {
        Plugin.configEnableDialogueSkip.Value = GlobalGame.canSkipDialogue;
    }
}