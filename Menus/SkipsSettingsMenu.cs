using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Configs;

namespace SpeedrunMod.Menus;

internal static class SkipsSettingsMenu
{
    private static MenuOption _cappieRingSkipOption;

    private static string CappieRingSkipMenuLabel =>
        ModConfig.EnableCappieRingSkip.Value
            ? "Cappie ring skip: On"
            : "Cappie ring skip: Off";

    internal static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("SKIPS")
            .SetBackButton(previousMenu)
            .Build();

        _cappieRingSkipOption = new MenuOptionFactory()
            .SetName(CappieRingSkipMenuLabel)
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(ToggleCappieRingSkip)
            .Build();
        
        return menu;
    }

    private static void ToggleCappieRingSkip()
    {
        ModConfig.EnableCappieRingSkip.Value = !ModConfig.EnableCappieRingSkip.Value;
        RefreshCappieRingSkipText();
        Plugin.Log.LogInfo(CappieRingSkipMenuLabel);
    }

    private static void RefreshCappieRingSkipText()
    {
        SetMenuOptionText(_cappieRingSkipOption, CappieRingSkipMenuLabel);
    }

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
