using BepInEx.Configuration;
using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Configs;

namespace SpeedrunMod.Menus;

internal static class SoftlocksSettingsMenu
{
    private static MenuOption _allSoftlockFixesOption;
    private static MenuOption _ghostlyPuzzleOption;
    private static MenuOption _ghostlyChapterLoadOption;
    private static MenuOption _sleepyDialogueOption;
    private static MenuOption _coreThrowOption;
    private static MenuOption _creepyDialogueOption;
    private static MenuOption _baseballBatOption;
    private static MenuOption _kindBedroomNoteOption;
    private static MenuOption _cappieRingOption;

    internal static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("SOFTLOCKS")
            .SetBackButton(previousMenu)
            .Build();

        _allSoftlockFixesOption = AddToggle(
            menu,
            SoftlockConfig.EnableAllSoftlockFixes,
            "All Softlock Fixes",
            () => RefreshOption(_allSoftlockFixesOption, SoftlockConfig.EnableAllSoftlockFixes, "All Softlock Fixes"));

        new MenuOptionFactory()
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .BuildMenuDivider();

        _ghostlyPuzzleOption = AddToggle(
            menu,
            SoftlockConfig.EnableGhostlyPuzzle,
            "Ghostly Puzzle Fix",
            () => RefreshOption(_ghostlyPuzzleOption, SoftlockConfig.EnableGhostlyPuzzle, "Ghostly Puzzle Fix"));

        _ghostlyChapterLoadOption = AddToggle(
            menu,
            SoftlockConfig.EnableGhostlyChapterLoad,
            "Ghostly Chapter Load Fix",
            () => RefreshOption(_ghostlyChapterLoadOption, SoftlockConfig.EnableGhostlyChapterLoad, "Ghostly Chapter Load Fix"));

        _sleepyDialogueOption = AddToggle(
            menu,
            SoftlockConfig.EnableSleepyDialogue,
            "Sleepy Dialogue Fix",
            () => RefreshOption(_sleepyDialogueOption, SoftlockConfig.EnableSleepyDialogue, "Sleepy Dialogue Fix"));

        _coreThrowOption = AddToggle(
            menu,
            SoftlockConfig.EnableCoreThrow,
            "Core Throw Fix",
            () => RefreshOption(_coreThrowOption, SoftlockConfig.EnableCoreThrow, "Core Throw Fix"));

        _creepyDialogueOption = AddToggle(
            menu,
            SoftlockConfig.EnableCreepyDialogue,
            "Creepy Dialogue Fix",
            () => RefreshOption(_creepyDialogueOption, SoftlockConfig.EnableCreepyDialogue, "Creepy Dialogue Fix"));

        _baseballBatOption = AddToggle(
            menu,
            SoftlockConfig.EnableBaseballBat,
            "Baseball Bat Fix",
            () => RefreshOption(_baseballBatOption, SoftlockConfig.EnableBaseballBat, "Baseball Bat Fix"));

        _kindBedroomNoteOption = AddToggle(
            menu,
            SoftlockConfig.EnableKindBedroomNote,
            "Kind Bedroom Note Fix",
            () => RefreshOption(_kindBedroomNoteOption, SoftlockConfig.EnableKindBedroomNote, "Kind Bedroom Note Fix"));

        _cappieRingOption = AddToggle(
            menu,
            SoftlockConfig.EnableCappieRing,
            "Cappie Ring Fix",
            () => RefreshOption(_cappieRingOption, SoftlockConfig.EnableCappieRing, "Cappie Ring Fix"));

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
