using BepInEx.Configuration;
using UnityEngine;

namespace SpeedrunMod.Configs;

internal static class ModConfig
{
    internal static ConfigEntry<bool> EnableDialogueSkip;
    internal static ConfigEntry<KeyCode> FpsOverrideToggleKeybind;
    internal static ConfigEntry<KeyCode> FpsUncapToggleKeybind;
    internal static ConfigEntry<int> FpsOverrideTarget;
    internal static ConfigEntry<bool> RefreshRateOverrideEnabled;
    internal static ConfigEntry<int> RefreshRateOverrideTarget;

    internal static void Initialize(ConfigFile configFile)
    {
        EnableDialogueSkip = configFile.Bind(
            "Automatic",
            "EnableDialogueSkip",
            false,
            "Enable the dialogue skip on game startup (NOTE: This value is automatically controlled by the mod)");

        FpsOverrideToggleKeybind = configFile.Bind(
            "FPS",
            "OverrideToggleKeybind",
            KeyCode.F1,
            "In-game: toggle between your configured target FPS and the previous FPS.");

        FpsOverrideTarget = configFile.Bind(
            "FPS",
            "OverrideTarget",
            5,
            "Target FPS when the target-FPS override toggle is on (menu: Target FPS). Use 0 for uncapped.");

        FpsUncapToggleKeybind = configFile.Bind(
            "FPS",
            "UncapToggleKeybind",
            KeyCode.F2,
            "In-game: toggle uncapped FPS (setfps 0) and restore previous FPS.");

        RefreshRateOverrideEnabled = configFile.Bind(
            "RefreshRate",
            "OverrideEnabled",
            false,
            "When true, Screen.currentResolution reports OverrideTarget Hz. Restart the game after changing this if the game caches refresh rate once at launch (e.g. MiSide).");

        RefreshRateOverrideTarget = configFile.Bind(
            "RefreshRate",
            "OverrideTarget",
            60,
            "Reported refresh rate (Hz) when OverrideEnabled is true (menu: Target Hz).");
    }
}
