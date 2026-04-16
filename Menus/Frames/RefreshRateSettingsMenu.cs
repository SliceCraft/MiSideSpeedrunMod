using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Configs;

namespace SpeedrunMod.Menus.Frames;

internal static class RefreshRateSettingsMenu
{
    private static MenuOption _overrideEnabledOption;
    private static MenuOption _targetHzOption;

    private static string OverrideEnabledMenuLabel =>
        RefreshRateConfig.OverrideEnabled.Value
            ? "Refresh override: On"
            : "Refresh override: Off";

    private static string TargetHzMenuLabel =>
        $"Target Hz: {RefreshRateConfig.GetTargetHz()}";

    internal static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("REFRESH RATE")
            .SetBackButton(previousMenu)
            .Build();

        new MenuOptionFactory()
            .SetName("Restart the game for these settings to apply in-game.")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .Build();

        new MenuOptionFactory()
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .BuildMenuDivider();

        _overrideEnabledOption = new MenuOptionFactory()
            .SetName(OverrideEnabledMenuLabel)
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(ToggleOverrideEnabled)
            .Build();

        _targetHzOption = new MenuOptionFactory()
            .SetName(TargetHzMenuLabel)
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .Build();
        
        new MenuOptionFactory()
            .SetName("+1000 Hz")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetHzAndRefresh(1000))
            .Build();

        new MenuOptionFactory()
            .SetName("+100 Hz")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetHzAndRefresh(100))
            .Build();

        new MenuOptionFactory()
            .SetName("+10 Hz")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetHzAndRefresh(10))
            .Build();

        new MenuOptionFactory()
            .SetName("+1 Hz")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetHzAndRefresh(1))
            .Build();

        new MenuOptionFactory()
            .SetName("-1 Hz")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetHzAndRefresh(-1))
            .Build();

        new MenuOptionFactory()
            .SetName("-10 Hz")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetHzAndRefresh(-10))
            .Build();

        new MenuOptionFactory()
            .SetName("-100 Hz")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetHzAndRefresh(-100))
            .Build();

        new MenuOptionFactory()
            .SetName("-1000 Hz")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetHzAndRefresh(-1000))
            .Build();

        return menu;
    }

    private static void AdjustTargetHzAndRefresh(int delta)
    {
        RefreshRateConfig.SetTargetHz(RefreshRateConfig.GetTargetHz() + delta);
        RefreshTargetHzText();
    }

    private static void ToggleOverrideEnabled()
    {
        RefreshRateConfig.OverrideEnabled.Value = !RefreshRateConfig.OverrideEnabled.Value;
        RefreshOverrideEnabledText();
        Plugin.Log.LogInfo(OverrideEnabledMenuLabel);
    }

    private static void RefreshOverrideEnabledText()
    {
        SetMenuOptionText(_overrideEnabledOption, OverrideEnabledMenuLabel);
    }

    private static void RefreshTargetHzText()
    {
        SetMenuOptionText(_targetHzOption, TargetHzMenuLabel);
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
