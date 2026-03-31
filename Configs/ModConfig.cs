using BepInEx.Configuration;
using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class ModConfig
{
    internal static ConfigEntry<bool> EnableDialogueSkip;
    internal static ConfigEntry<KeyCode> FpsToggleKeybind;
    internal static ConfigEntry<int> FpsOverrideTarget;

    internal static void Initialize(ConfigFile configFile)
    {
        EnableDialogueSkip = configFile.Bind(
            "Automatic",
            "EnableDialogueSkip",
            false,
            "Enable the dialogue skip on game startup (NOTE: This value is automatically controlled by the mod)");

        FpsToggleKeybind = configFile.Bind(
            "FPS",
            "ToggleKeybind",
            KeyCode.F1,
            "Key used to toggle FPS override mode.");

        FpsOverrideTarget = configFile.Bind(
            "FPS",
            "OverrideTarget",
            5,
            "Target FPS while override mode is enabled. Use 0 for unlimited.");
    }
}
