using BepInEx.Configuration;

namespace SpeedrunMod.Configs;

internal static class SoftlockConfig
{
    private const string Section = "Softlocks";

    internal static ConfigEntry<bool> EnableAllSoftlockFixes;
    internal static ConfigEntry<bool> EnableGhostlyPuzzle;
    internal static ConfigEntry<bool> EnableGhostlyChapterLoad;
    internal static ConfigEntry<bool> EnableSleepyDialogue;
    internal static ConfigEntry<bool> EnableCoreThrow;
    internal static ConfigEntry<bool> EnableCreepyDialogue;
    internal static ConfigEntry<bool> EnableBaseballBat;
    internal static ConfigEntry<bool> EnableKindBedroomNote;
    internal static ConfigEntry<bool> EnableCappieRing;

    internal static void Initialize(ConfigFile configFile)
    {
        EnableAllSoftlockFixes = Bind(configFile, "EnableAllSoftlockFixes", "Master Softlock Fixes gate (menu: SOFTLOCKS).");
        EnableGhostlyPuzzle = Bind(configFile, "EnableGhostlyPuzzle", "Ghostly Puzzle Softlock Fix (menu: SOFTLOCKS).");
        EnableGhostlyChapterLoad = Bind(configFile, "EnableGhostlyChapterLoad", "Ghostly Chapter Load Softlock Fix (menu: SOFTLOCKS).");
        EnableSleepyDialogue = Bind(configFile, "EnableSleepyDialogue", "Sleepy Dialogue Softlock Fix (menu: SOFTLOCKS).");
        EnableCoreThrow = Bind(configFile, "EnableCoreThrow", "Core Throw Softlock Fix (menu: SOFTLOCKS).");
        EnableCreepyDialogue = Bind(configFile, "EnableCreepyDialogue", "Creepy Dialogue Softlock Fix (menu: SOFTLOCKS).");
        EnableBaseballBat = Bind(configFile, "EnableBaseballBat", "Baseball Bat Softlock Fix (menu: SOFTLOCKS).");
        EnableKindBedroomNote = Bind(configFile, "EnableKindBedroomNote", "Kind Bedroom Note Softlock Fix (menu: SOFTLOCKS).");
        EnableCappieRing = Bind(configFile, "EnableCappieRing", "Cappie Ring Softlock Fix (menu: SOFTLOCKS).");
    }

    internal static bool IsEnabled(ConfigEntry<bool> softlock) =>
        EnableAllSoftlockFixes.Value && softlock.Value;

    private static ConfigEntry<bool> Bind(ConfigFile configFile, string key, string description) =>
        configFile.Bind(Section, key, true, description);
}
