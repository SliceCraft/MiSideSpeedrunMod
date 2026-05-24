using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Configs;
using SpeedrunMod.Menus.Keybinds;

namespace SpeedrunMod.Menus.Frames;

internal static class FpsSettingsMenu
{
    private const string CaptureContext = "FpsSettingsMenu";

    private static MenuOption _targetFpsToggleKeyOption;
    private static MenuOption _uncapFpsToggleKeyOption;
    private static MenuOption _targetFpsOption;

    private static string TargetFpsToggleKeyMenuLabel =>
        $"Target FPS toggle key: {FpsConfig.OverrideToggleKeybind.Value}";

    private static string UncapFpsToggleKeyMenuLabel =>
        $"Uncap FPS toggle key: {FpsConfig.UncapToggleKeybind.Value}";

    private static string TargetFpsMenuLabel =>
        $"Target FPS: {FpsConfig.GetTargetFpsLabel()}";

    internal static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("FPS SETTINGS")
            .SetBackButton(previousMenu)
            .Build();

        _targetFpsToggleKeyOption = new MenuOptionFactory()
            .SetName(TargetFpsToggleKeyMenuLabel)
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(BeginTargetFpsToggleKeyCapture)
            .Build();

        _uncapFpsToggleKeyOption = new MenuOptionFactory()
            .SetName(UncapFpsToggleKeyMenuLabel)
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
            .SetName(TargetFpsMenuLabel)
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
            .SetName("+1 FPS")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetFpsAndRefresh(1))
            .Build();

        new MenuOptionFactory()
            .SetName("-1 FPS")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(() => AdjustTargetFpsAndRefresh(-1))
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
        if (!KeybindCapture.IsCapturing(CaptureContext)) return;
		if (!IsFpsSettingsMenuVisible() && KeybindCapture.CancelCapture(CaptureContext))
		{
            RefreshTargetFpsToggleKeyText();
            RefreshUncapFpsToggleKeyText();
            Plugin.Log.LogInfo("FPS settings keybind capture cancelled (left FPS settings menu).");
		}
    }

    private static void AdjustTargetFpsAndRefresh(int delta)
    {
        FpsConfig.AdjustTargetFps(delta);
        RefreshTargetFpsText();
    }

    private static void BeginTargetFpsToggleKeyCapture()
    {
        SetMenuOptionText(_targetFpsToggleKeyOption, "Target FPS toggle key: <press key... Esc to cancel>");
        KeybindCapture.BeginCapture(CaptureContext, OnTargetFpsToggleKeyCaptureComplete);
    }

    private static void OnTargetFpsToggleKeyCaptureComplete(bool success, UnityEngine.KeyCode keyCode)
    {
        if (success)
        {
            FpsConfig.OverrideToggleKeybind.Value = keyCode;
            SetMenuOptionText(_targetFpsToggleKeyOption, $"Target FPS toggle key: {keyCode}");
            Plugin.Log.LogInfo($"Target FPS toggle key updated to {keyCode}.");
            return;
        }

        RefreshTargetFpsToggleKeyText();
    }

    private static void BeginUncapFpsToggleKeyCapture()
    {
        SetMenuOptionText(_uncapFpsToggleKeyOption, "Uncap FPS toggle key: <press key... Esc to cancel>");
        KeybindCapture.BeginCapture(CaptureContext, OnUncapFpsToggleKeyCaptureComplete);
    }

    private static void OnUncapFpsToggleKeyCaptureComplete(bool success, UnityEngine.KeyCode keyCode)
    {
        if (success)
        {
            FpsConfig.UncapToggleKeybind.Value = keyCode;
            SetMenuOptionText(_uncapFpsToggleKeyOption, $"Uncap FPS toggle key: {keyCode}");
            Plugin.Log.LogInfo($"Uncap FPS toggle key updated to {keyCode}.");
            return;
        }

        RefreshUncapFpsToggleKeyText();
    }

    private static void RefreshTargetFpsText()
    {
        SetMenuOptionText(_targetFpsOption, TargetFpsMenuLabel);
    }

    private static void RefreshTargetFpsToggleKeyText()
    {
        SetMenuOptionText(_targetFpsToggleKeyOption, TargetFpsToggleKeyMenuLabel);
    }

    private static void RefreshUncapFpsToggleKeyText()
    {
        SetMenuOptionText(_uncapFpsToggleKeyOption, UncapFpsToggleKeyMenuLabel);
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
