using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Configs;

namespace SpeedrunMod.Menus.Frames;

internal static class RefreshRateSettingsMenu
{
    private static MenuOption _overrideEnabledOption;
    private static MenuOption _targetHzOption;

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
            .SetName(GetOverrideEnabledLabel())
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(ToggleOverrideEnabled)
            .Build();

        _targetHzOption = new MenuOptionFactory()
            .SetName($"Target Hz: {RefreshRateConfig.GetTargetHzLabel()}")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
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
            .SetName("+5 Hz")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetHzAndRefresh(5))
            .Build();

        new MenuOptionFactory()
            .SetName("-5 Hz")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetHzAndRefresh(-5))
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

        return menu;
    }

    private static void AdjustTargetHzAndRefresh(int delta)
    {
        RefreshRateConfig.AdjustTargetHz(delta);
        RefreshTargetHzText();
    }

    private static void ToggleOverrideEnabled()
    {
        ModConfig.RefreshRateOverrideEnabled.Value = !ModConfig.RefreshRateOverrideEnabled.Value;
        RefreshOverrideEnabledText();
        Plugin.Log.LogInfo($"Refresh rate override {(ModConfig.RefreshRateOverrideEnabled.Value ? "enabled" : "disabled")}.");
    }

    private static string GetOverrideEnabledLabel()
    {
        return ModConfig.RefreshRateOverrideEnabled.Value
            ? "Refresh override: On"
            : "Refresh override: Off";
    }

    private static void RefreshOverrideEnabledText()
    {
        SetMenuOptionText(_overrideEnabledOption, GetOverrideEnabledLabel());
    }

    private static void RefreshTargetHzText()
    {
        SetMenuOptionText(_targetHzOption, $"Target Hz: {RefreshRateConfig.GetTargetHzLabel()}");
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
