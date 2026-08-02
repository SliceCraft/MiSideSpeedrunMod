using BepInEx.Configuration;
using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Configs;

namespace SpeedrunMod.Menus;

internal static class SoftlocksSettingsMenu
{
    private static MenuOption _allSoftlocksOption;
    private static MenuOption _ghostlyPuzzleOption;
    private static MenuOption _ghostlyChapterLoadOption;
    private static MenuOption _sleepyDialogueOption;
    private static MenuOption _coreThrowOption;
    private static MenuOption _creepyDialogueOption;
    private static MenuOption _baseballBatOption;
    private static MenuOption _kindBedroomNoteOption;
    private static MenuOption _kappiRingOption;

    internal static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("SOFTLOCKS")
            .SetBackButton(previousMenu)
            .Build();

        _allSoftlocksOption = AddToggle(
            menu,
            SoftlockConfig.EnableAllSoftlocks,
            "All Softlocks",
            () => RefreshOption(_allSoftlocksOption, SoftlockConfig.EnableAllSoftlocks, "All Softlocks"));

        new MenuOptionFactory()
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .BuildMenuDivider();

        _ghostlyPuzzleOption = AddToggle(
            menu,
            SoftlockConfig.EnableGhostlyPuzzle,
            "Ghostly Puzzle",
            () => RefreshOption(_ghostlyPuzzleOption, SoftlockConfig.EnableGhostlyPuzzle, "Ghostly Puzzle"));

        _ghostlyChapterLoadOption = AddToggle(
            menu,
            SoftlockConfig.EnableGhostlyChapterLoad,
            "Ghostly Chapter Load",
            () => RefreshOption(_ghostlyChapterLoadOption, SoftlockConfig.EnableGhostlyChapterLoad, "Ghostly Chapter Load"));

        _sleepyDialogueOption = AddToggle(
            menu,
            SoftlockConfig.EnableSleepyDialogue,
            "Sleepy Dialogue",
            () => RefreshOption(_sleepyDialogueOption, SoftlockConfig.EnableSleepyDialogue, "Sleepy Dialogue"));

        _coreThrowOption = AddToggle(
            menu,
            SoftlockConfig.EnableCoreThrow,
            "Core Throw",
            () => RefreshOption(_coreThrowOption, SoftlockConfig.EnableCoreThrow, "Core Throw"));

        _creepyDialogueOption = AddToggle(
            menu,
            SoftlockConfig.EnableCreepyDialogue,
            "Creepy Dialogue",
            () => RefreshOption(_creepyDialogueOption, SoftlockConfig.EnableCreepyDialogue, "Creepy Dialogue"));

        _baseballBatOption = AddToggle(
            menu,
            SoftlockConfig.EnableBaseballBat,
            "Baseball Bat",
            () => RefreshOption(_baseballBatOption, SoftlockConfig.EnableBaseballBat, "Baseball Bat"));

        _kindBedroomNoteOption = AddToggle(
            menu,
            SoftlockConfig.EnableKindBedroomNote,
            "Kind Bedroom Note",
            () => RefreshOption(_kindBedroomNoteOption, SoftlockConfig.EnableKindBedroomNote, "Kind Bedroom Note"));

        _kappiRingOption = AddToggle(
            menu,
            SoftlockConfig.EnableKappiRing,
            "Kappi Ring",
            () => RefreshOption(_kappiRingOption, SoftlockConfig.EnableKappiRing, "Kappi Ring"));

        return menu;
    }

    private static MenuOption AddToggle(
        GameMenu menu,
        ConfigEntry<bool> entry,
        string label,
        System.Action onToggle)
    {
        return new MenuOptionFactory()
            .SetName(MenuLabel(entry, label))
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() =>
            {
                entry.Value = !entry.Value;
                onToggle();
                Plugin.Log.LogInfo(MenuLabel(entry, label));
            })
            .Build();
    }

    private static void RefreshOption(MenuOption option, ConfigEntry<bool> entry, string label) =>
        SetMenuOptionText(option, MenuLabel(entry, label));

    private static string MenuLabel(ConfigEntry<bool> entry, string label) =>
        entry.Value ? $"{label}: On" : $"{label}: Off";

    private static void SetMenuOptionText(MenuOption menuOption, string text)
    {
        if (menuOption == null) return;

        menuOption.Text = text;

        if (menuOption.TextComponent != null)
        {
            menuOption.TextComponent.text = text;
        }
    }
}
