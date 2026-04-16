using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Configs;
using SpeedrunMod.EventDisplay;
using SpeedrunMod.Menus.Keybinds;
using UnityEngine;

namespace SpeedrunMod.Menus.Debug;

internal static class DebugSettingsMenu
{
	private static MenuOption _overlayEnabledOption;

	private static MenuOption _logIntervalOption;

	private static MenuOption _overlayToggleKeyOption;

	private static string OverlayEnabledMenuLabel =>
		DebugConfig.OverlayEnabled.Value
			? "Debug overlay: On"
			: "Debug overlay: Off";

	private static string LogIntervalMenuLabel =>
		DebugConfig.OverlayLogInterval.Value <= 0f
			? "Overlay update: every frame"
			: $"Overlay update: every {DebugConfig.OverlayLogInterval.Value:F2} s";

	internal static GameMenu CreateMenu(GameMenu previousMenu)
	{
		var gameMenu = new MenuFactory()
			.SetTitle("DEBUG")
			.SetBackButton(previousMenu)
			.Build();

		_overlayEnabledOption = new MenuOptionFactory()
			.SetName(OverlayEnabledMenuLabel)
			.SetParent(gameMenu)
			.PlaceOptionBefore(gameMenu.MenuOptions.Count - 1)
			.SetNextLocation(gameMenu)
			.SetOnClick(ToggleOverlaySetting)
			.Build();

		_overlayToggleKeyOption = new MenuOptionFactory()
			.SetName($"Debug overlay toggle key: {DebugConfig.OverlayToggleKeybind.Value}")
			.SetParent(gameMenu)
			.PlaceOptionBefore(gameMenu.MenuOptions.Count - 1)
			.SetNextLocation(gameMenu)
			.SetOnClick(BeginOverlayToggleKeyCapture)
			.Build();

		new MenuOptionFactory()
			.SetParent(gameMenu)
			.PlaceOptionBefore(gameMenu.MenuOptions.Count - 1)
			.BuildMenuDivider();
		
		_logIntervalOption = new MenuOptionFactory()
			.SetName(LogIntervalMenuLabel)
			.SetParent(gameMenu)
			.PlaceOptionBefore(gameMenu.MenuOptions.Count - 1)
			.SetNextLocation(gameMenu)
			.Build();

		new MenuOptionFactory()
			.SetName("+1 s")
			.SetParent(gameMenu)
			.PlaceOptionBefore(gameMenu.MenuOptions.Count - 1)
			.SetNextLocation(gameMenu)
			.SetOnClick(() => AdjustLogIntervalAndRefresh(1f))
			.Build();

		new MenuOptionFactory()
			.SetName("+0.1 s")
			.SetParent(gameMenu)
			.PlaceOptionBefore(gameMenu.MenuOptions.Count - 1)
			.SetNextLocation(gameMenu)
			.SetOnClick(() => AdjustLogIntervalAndRefresh(0.1f))
			.Build();

		new MenuOptionFactory()
			.SetName("+0.01 s")
			.SetParent(gameMenu)
			.PlaceOptionBefore(gameMenu.MenuOptions.Count - 1)
			.SetNextLocation(gameMenu)
			.SetOnClick(() => AdjustLogIntervalAndRefresh(0.01f))
			.Build();

		new MenuOptionFactory()
			.SetName("-0.01 s")
			.SetParent(gameMenu)
			.PlaceOptionBefore(gameMenu.MenuOptions.Count - 1)
			.SetNextLocation(gameMenu)
			.SetOnClick(() => AdjustLogIntervalAndRefresh(-0.01f))
			.Build();
		
		new MenuOptionFactory()
			.SetName("-0.1 s")
			.SetParent(gameMenu)
			.PlaceOptionBefore(gameMenu.MenuOptions.Count - 1)
			.SetNextLocation(gameMenu)
			.SetOnClick(() => AdjustLogIntervalAndRefresh(-0.1f))
			.Build();
		
		new MenuOptionFactory()
			.SetName("-1 s")
			.SetParent(gameMenu)
			.PlaceOptionBefore(gameMenu.MenuOptions.Count - 1)
			.SetNextLocation(gameMenu)
			.SetOnClick(() => AdjustLogIntervalAndRefresh(-1f))
			.Build();

		return gameMenu;
	}

	internal static void Update()
	{
		if (KeybindCapture.IsCapturing() && !IsDebugSettingsMenuVisible())
		{
			Plugin.Log.LogInfo("Debug settings keybind capture cancelled (left DEBUG menu).");
			KeybindCapture.CancelCapture();
			RefreshOverlayToggleKeyText();
		}
	}

	private static void ToggleOverlaySetting()
	{
		DebugConfig.OverlayEnabled.Value = !DebugConfig.OverlayEnabled.Value;
		RefreshOverlayEnabledText();
		Plugin.Log.LogInfo(OverlayEnabledMenuLabel);
		EventManager.ShowEvent(new ModEvent(OverlayEnabledMenuLabel));
	}

	private static void AdjustLogIntervalAndRefresh(float delta)
	{
		DebugConfig.AdjustLogInterval(delta);
		RefreshLogIntervalText();
	}

	private static void BeginOverlayToggleKeyCapture()
	{
		SetMenuOptionText(_overlayToggleKeyOption, "Debug overlay toggle key: <press key... Esc to cancel>");
		KeybindCapture.BeginCapture(OnOverlayToggleKeyCaptureComplete);
	}

	private static void OnOverlayToggleKeyCaptureComplete(bool success, KeyCode keyCode)
	{
		if (success)
		{
			DebugConfig.OverlayToggleKeybind.Value = keyCode;
			SetMenuOptionText(_overlayToggleKeyOption, $"Debug overlay toggle key: {keyCode}");
			Plugin.Log.LogInfo($"Debug overlay toggle key updated to {keyCode}.");
		}
		else
		{
			RefreshOverlayToggleKeyText();
		}
	}

	private static void RefreshOverlayEnabledText()
	{
		SetMenuOptionText(_overlayEnabledOption, OverlayEnabledMenuLabel);
	}

	private static void RefreshLogIntervalText()
	{
		SetMenuOptionText(_logIntervalOption, LogIntervalMenuLabel);
	}

	private static void RefreshOverlayToggleKeyText()
	{
		SetMenuOptionText(_overlayToggleKeyOption, $"Debug overlay toggle key: {DebugConfig.OverlayToggleKeybind.Value}");
	}

	private static void SetMenuOptionText(MenuOption menuOption, string text)
	{
		if (menuOption != null)
		{
			menuOption.Text = text;
			if (menuOption.TextComponent != null)
			{
				menuOption.TextComponent.text = text;
			}
		}
	}

	private static bool IsDebugSettingsMenuVisible()
	{
		if (!IsMenuRowVisible(_overlayToggleKeyOption))
		{
			return IsMenuRowVisible(_overlayEnabledOption);
		}

		return true;
	}

	private static bool IsMenuRowVisible(MenuOption option)
	{
		if (option != null && option.TextComponent != null)
		{
			return option.TextComponent.gameObject.activeInHierarchy;
		}

		return false;
	}
}
