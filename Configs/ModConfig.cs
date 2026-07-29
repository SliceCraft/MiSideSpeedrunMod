using BepInEx.Configuration;

namespace SpeedrunMod.Configs;

internal static class ModConfig
{
    internal static ConfigEntry<bool> EnableDialogueSkip;
    internal static ConfigEntry<bool> EnableCappieRingSkip;

    internal static void Initialize(ConfigFile configFile)
    {
        EnableDialogueSkip = configFile.Bind(
            "Automatic",
            "EnableDialogueSkip",
            false,
            "Enable the dialogue skip on game startup (NOTE: This value is automatically controlled by the mod)");

        EnableCappieRingSkip = configFile.Bind(
            "Skips",
            "EnableCappieRingSkip",
            true,
            "Skip the Cappie chapter ring wait.");

        FpsConfig.Initialize(configFile);
        RefreshRateConfig.Initialize(configFile);
        OverlayConfig.Initialize(configFile);
        SoftlockConfig.Initialize(configFile);
    }
}
