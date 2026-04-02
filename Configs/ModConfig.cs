using BepInEx.Configuration;

namespace SpeedrunMod.Configs;

internal static class ModConfig
{
    internal static ConfigEntry<bool> EnableDialogueSkip;

    internal static void Initialize(ConfigFile configFile)
    {
        EnableDialogueSkip = configFile.Bind(
            "Automatic",
            "EnableDialogueSkip",
            false,
            "Enable the dialogue skip on game startup (NOTE: This value is automatically controlled by the mod)");

        FpsConfig.Initialize(configFile);
        RefreshRateConfig.Initialize(configFile);
        DebugConfig.Initialize(configFile);
    }
}
