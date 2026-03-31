using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Configs;
using SpeedrunMod.Menus.Keybinds;

namespace SpeedrunMod.Menus.Frames;

internal static class FpsSettingsMenu
{
    private static MenuOption _keybindOption;
    private static MenuOption _targetFpsOption;

    internal static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("FPS SETTINGS")
            .SetBackButton(previousMenu)
            .Build();

        _keybindOption = new MenuOptionFactory()
            .SetName($"FPS keybind: {FpsConfig.GetToggleKey()}")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(BeginFpsKeyCapture)
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
            .SetName("+Target FPS")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(IncreaseFpsTarget)
            .Build();

        new MenuOptionFactory()
            .SetName("-Target FPS")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(DecreaseFpsTarget)
            .Build();

        return menu;
    }

    internal static void Update()
    {
        if (!KeybindCapture.IsCapturing()) return;
        if (IsFpsSettingsMenuVisible()) return;

        Plugin.Log.LogInfo("FPS keybind capture cancelled (left FPS settings menu).");

        KeybindCapture.CancelCapture();
        RefreshKeybindText();
    }

    private static void IncreaseFpsTarget()
    {
        FpsConfig.IncreaseTargetFps();
        RefreshTargetFpsText();
    }

    private static void DecreaseFpsTarget()
    {
        FpsConfig.DecreaseTargetFps();
        RefreshTargetFpsText();
    }

    private static void BeginFpsKeyCapture()
    {
        SetMenuOptionText(_keybindOption, "FPS keybind: <press key... Esc to cancel>");
        KeybindCapture.BeginCapture(OnFpsKeyCaptureComplete);
    }

    private static void OnFpsKeyCaptureComplete(bool success, UnityEngine.KeyCode keyCode)
    {
        if (success)
        {
            FpsConfig.SetToggleKey(keyCode);
            Plugin.Log.LogInfo($"FPS keybind updated to {keyCode}.");
        }

        RefreshKeybindText();
    }

    private static void RefreshTargetFpsText()
    {
        SetMenuOptionText(_targetFpsOption, $"Target FPS: {FpsConfig.GetTargetFpsLabel()}");
    }

    private static void RefreshKeybindText()
    {
        SetMenuOptionText(_keybindOption, $"FPS keybind: {FpsConfig.GetToggleKey()}");
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
        return _keybindOption != null &&
               _keybindOption.TextComponent != null &&
               _keybindOption.TextComponent.gameObject.activeInHierarchy;
    }
}
