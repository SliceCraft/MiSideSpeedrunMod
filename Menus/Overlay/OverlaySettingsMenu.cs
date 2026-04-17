using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Configs;
using SpeedrunMod.EventDisplay;
using SpeedrunMod.Menus.Keybinds;
using UnityEngine;

namespace SpeedrunMod.Menus.Overlay;

internal static class OverlaySettingsMenu
{
	private static MenuOption _overlayEnabledOption;

	private static MenuOption _logIntervalOption;

	private static MenuOption _overlayToggleKeyOption;

	private static string OverlayEnabledMenuLabel =>
		OverlayConfig.OverlayEnabled.Value
			? "Overlay: On"
			: "Overlay: Off";

	private static string LogIntervalMenuLabel =>
		OverlayConfig.OverlayLogInterval.Value <= 0f
			? "Overlay update: every frame"
			: $"Overlay update: every {OverlayConfig.OverlayLogInterval.Value:F2} s";

	internal static GameMenu CreateMenu(GameMenu previousMenu)
	{
		var gameMenu = new MenuFactory()
			.SetTitle("OVERLAY")
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
			.SetName($"Overlay toggle key: {OverlayConfig.OverlayToggleKeybind.Value}")
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
		if (KeybindCapture.IsCapturing() && !IsOverlaySettingsMenuVisible())
		{
			Plugin.Log.LogInfo("Overlay settings keybind capture cancelled (left OVERLAY menu).");
			KeybindCapture.CancelCapture();
			RefreshOverlayToggleKeyText();
		}
	}

	private static void ToggleOverlaySetting()
	{
		OverlayConfig.OverlayEnabled.Value = !OverlayConfig.OverlayEnabled.Value;
		RefreshOverlayEnabledText();
		Plugin.Log.LogInfo(OverlayEnabledMenuLabel);
		EventManager.ShowEvent(new ModEvent(OverlayEnabledMenuLabel));
	}

	private static void AdjustLogIntervalAndRefresh(float delta)
	{
		OverlayConfig.AdjustLogInterval(delta);
		RefreshLogIntervalText();
	}

	private static void BeginOverlayToggleKeyCapture()
	{
		SetMenuOptionText(_overlayToggleKeyOption, "Overlay toggle key: <press key... Esc to cancel>");
		KeybindCapture.BeginCapture(OnOverlayToggleKeyCaptureComplete);
	}

	private static void OnOverlayToggleKeyCaptureComplete(bool success, KeyCode keyCode)
	{
		if (success)
		{
			OverlayConfig.OverlayToggleKeybind.Value = keyCode;
			SetMenuOptionText(_overlayToggleKeyOption, $"Overlay toggle key: {keyCode}");
			Plugin.Log.LogInfo($"Overlay toggle key updated to {keyCode}.");
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
		SetMenuOptionText(_overlayToggleKeyOption, $"Overlay toggle key: {OverlayConfig.OverlayToggleKeybind.Value}");
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

	private static bool IsOverlaySettingsMenuVisible()
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
