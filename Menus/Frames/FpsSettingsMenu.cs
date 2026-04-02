using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Configs;
using SpeedrunMod.Menus.Keybinds;

namespace SpeedrunMod.Menus.Frames;

internal static class FpsSettingsMenu
{
    private static MenuOption _targetFpsToggleKeyOption;
    private static MenuOption _uncapFpsToggleKeyOption;
    private static MenuOption _targetFpsOption;

    internal static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("FPS SETTINGS")
            .SetBackButton(previousMenu)
            .Build();

        _targetFpsToggleKeyOption = new MenuOptionFactory()
            .SetName($"Target FPS toggle key: {FpsConfig.GetOverrideToggleKey()}")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(BeginTargetFpsToggleKeyCapture)
            .Build();

        _uncapFpsToggleKeyOption = new MenuOptionFactory()
            .SetName($"Uncap FPS toggle key: {FpsConfig.GetUncapToggleKey()}")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(BeginUncapFpsToggleKeyCapture)
            .Build();

        new MenuOptionFactory()
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .BuildMenuDivider();

        _targetFpsOption = new MenuOptionFactory()
            .SetName($"Target FPS: {FpsConfig.GetTargetFpsLabel()}")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .Build();

        new MenuOptionFactory()
            .SetName("+100 FPS")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetFpsAndRefresh(100))
            .Build();

        new MenuOptionFactory()
            .SetName("+10 FPS")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetFpsAndRefresh(10))
            .Build();

        new MenuOptionFactory()
            .SetName("+5 FPS")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetFpsAndRefresh(5))
            .Build();

        new MenuOptionFactory()
            .SetName("-5 FPS")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetFpsAndRefresh(-5))
            .Build();

        new MenuOptionFactory()
            .SetName("-10 FPS")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetFpsAndRefresh(-10))
            .Build();

        new MenuOptionFactory()
            .SetName("-100 FPS")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetFpsAndRefresh(-100))
            .Build();

        return menu;
    }

    internal static void Update()
    {
        if (!KeybindCapture.IsCapturing()) return;
        if (IsFpsSettingsMenuVisible()) return;

        Plugin.Log.LogInfo("FPS settings keybind capture cancelled (left FPS settings menu).");

        KeybindCapture.CancelCapture();
        RefreshTargetFpsToggleKeyText();
        RefreshUncapFpsToggleKeyText();
    }

    private static void AdjustTargetFpsAndRefresh(int delta)
    {
        FpsConfig.AdjustTargetFps(delta);
        RefreshTargetFpsText();
    }

    private static void BeginTargetFpsToggleKeyCapture()
    {
        SetMenuOptionText(_targetFpsToggleKeyOption, "Target FPS toggle key: <press key... Esc to cancel>");
        KeybindCapture.BeginCapture(OnTargetFpsToggleKeyCaptureComplete);
    }

    private static void OnTargetFpsToggleKeyCaptureComplete(bool success, UnityEngine.KeyCode keyCode)
    {
        if (success)
        {
            FpsConfig.SetOverrideToggleKey(keyCode);
            SetMenuOptionText(_targetFpsToggleKeyOption, $"Target FPS toggle key: {keyCode}");
            Plugin.Log.LogInfo($"Target FPS toggle key updated to {keyCode}.");
            return;
        }

        RefreshTargetFpsToggleKeyText();
    }

    private static void BeginUncapFpsToggleKeyCapture()
    {
        SetMenuOptionText(_uncapFpsToggleKeyOption, "Uncap FPS toggle key: <press key... Esc to cancel>");
        KeybindCapture.BeginCapture(OnUncapFpsToggleKeyCaptureComplete);
    }

    private static void OnUncapFpsToggleKeyCaptureComplete(bool success, UnityEngine.KeyCode keyCode)
    {
        if (success)
        {
            FpsConfig.SetUncapToggleKey(keyCode);
            SetMenuOptionText(_uncapFpsToggleKeyOption, $"Uncap FPS toggle key: {keyCode}");
            Plugin.Log.LogInfo($"Uncap FPS toggle key updated to {keyCode}.");
            return;
        }

        RefreshUncapFpsToggleKeyText();
    }

    private static void RefreshTargetFpsText()
    {
        SetMenuOptionText(_targetFpsOption, $"Target FPS: {FpsConfig.GetTargetFpsLabel()}");
    }

    private static void RefreshTargetFpsToggleKeyText()
    {
        SetMenuOptionText(_targetFpsToggleKeyOption, $"Target FPS toggle key: {FpsConfig.GetOverrideToggleKey()}");
    }

    private static void RefreshUncapFpsToggleKeyText()
    {
        SetMenuOptionText(_uncapFpsToggleKeyOption, $"Uncap FPS toggle key: {FpsConfig.GetUncapToggleKey()}");
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

    private static bool IsFpsSettingsMenuVisible()
    {
        return IsMenuRowVisible(_targetFpsToggleKeyOption) ||
               IsMenuRowVisible(_uncapFpsToggleKeyOption);
    }

    private static bool IsMenuRowVisible(MenuOption option)
    {
        return option != null &&
               option.TextComponent != null &&
               option.TextComponent.gameObject.activeInHierarchy;
    }
}
