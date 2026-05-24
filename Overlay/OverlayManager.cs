using System.Collections.Generic;
using System.Text;
using SpeedrunMod.Configs;
using SpeedrunMod.Overlay.Modules;
using UnityEngine;
using UnityEngine.UI;
using SpeedrunMod.Menus.Keybinds;
using SpeedrunMod.Utils;

namespace SpeedrunMod.Overlay;

internal static class OverlayManager
{
	private const int OverlayFontSize = 20;

	private static readonly OverlayModulesRepository Modules = OverlayModulesRepository.Repository;

	private static readonly StringBuilder OverlayBuilder = new();
	
	private static readonly Dictionary<string, ModuleState> ModuleStates = new();

	private static bool IsEnabled => OverlayConfig.OverlayEnabled.Value;
	
	private static GameObject _overlayRoot;

	private static Text _overlayText;

	private static int _currentPageIndex;

	private const KeyCode NextGroupKey = KeyCode.PageDown;

	private const KeyCode PrevGroupKey = KeyCode.PageUp;

	internal static void Update()
	{
		if (!GameUtil.IsInGame() || !Modules.IsAnyPersistent && !IsEnabled)
		{
			Reset();
			return;
		}

		EnsureOverlay();
		ResetOverlay();

		UpdateModuleStates();
		HandleGroupPaging();
		RenderModules();

		UpdateOverlay();
	}

	private static void UpdateModuleStates()
	{
		var realtimeSinceStartup = Time.realtimeSinceStartup;
		var modules = IsEnabled ? Modules.GetAll() : Modules.GetPersistent();

		foreach (var module in modules)
		{
			var state = GetOrCreateModuleState(module);
			var updateIntervalSeconds = System.Math.Max(0, module.UpdateInterval.TotalSeconds);
			var shouldUpdateNow = state.CachedSnapshot is null || realtimeSinceStartup - state.LastUpdatedRealtime >= updateIntervalSeconds;

			if (!shouldUpdateNow) continue;
			state.CachedSnapshot = module.Update();
			state.LastUpdatedRealtime = realtimeSinceStartup;
		}
	}

	private static void ClearModuleState(IOverlayModule module)
	{
		var state = GetOrCreateModuleState(module);
		if (state.CachedSnapshot is null)
		{
			return;
		}

		module.Reset();
		state.Clear();
	}

	private static ModuleState GetOrCreateModuleState(IOverlayModule module)
	{
		if (ModuleStates.TryGetValue(module.Name, out var state)) return state;
		state = new ModuleState { ModuleName = module.Name };
		ModuleStates[module.Name] = state;

		return state;
	}

	private static void HandleGroupPaging()
	{
		var groupCount = Modules.GroupCount;
		if (groupCount <= 1 || KeybindCapture.IsCapturing())
		{
			_currentPageIndex = 0;
			return;
		}

		if (Input.GetKeyDown(NextGroupKey))
		{
			_currentPageIndex = (_currentPageIndex + 1) % groupCount;
		}
		else if (Input.GetKeyDown(PrevGroupKey))
		{
			_currentPageIndex = (_currentPageIndex - 1 + groupCount) % groupCount;
		}
		else
		{
			_currentPageIndex = Mathf.Clamp(_currentPageIndex, 0, groupCount - 1);
		}
	}

	private static void RenderModules()
	{
		var persistent = Modules.GetPersistent();
		foreach (var module in persistent)
		{
			RenderCurrentState(module);
		}

		if (!IsEnabled)
		{
			return;
		}

		var groupCount = Modules.GroupCount;
		var currentGroup = Modules.GetByGroupIndex(_currentPageIndex);

		if (groupCount > 1)
		{
			OverlayBuilder.AppendLine($"Overlay group {_currentPageIndex + 1}/{groupCount}: {currentGroup.Key}");
			OverlayBuilder.AppendLine($"Switch group: {PrevGroupKey}/{NextGroupKey}");
			OverlayBuilder.AppendLine();
		}

		foreach (var module in currentGroup.Value)
		{
			RenderCurrentState(module);
		}
	}

	private static void RenderCurrentState(IOverlayModule module)
	{
		var state = ModuleStates.GetValueOrDefault(module.Name);
		var content = state.CachedSnapshot?.Format() ?? string.Empty;

		if (string.IsNullOrWhiteSpace(content))
		{
			return;
		}
		
		OverlayBuilder.AppendLine($"[{state.ModuleName}]");
		OverlayBuilder.AppendLine(content);
		OverlayBuilder.AppendLine();
	}

	private static void UpdateOverlay()
	{
		if (_overlayText != null)
		{
			_overlayText.text = OverlayBuilder.ToString();
		}
	}

	private static void ResetOverlay()
	{
		OverlayBuilder.Clear();
	}

	private static void EnsureOverlay()
	{
		if (_overlayRoot != null)
		{
			return;
		}

		_overlayRoot = new GameObject("SpeedrunMod_Overlay");
		Object.DontDestroyOnLoad(_overlayRoot);

		var canvas = _overlayRoot.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 10000;

		var canvasScaler = _overlayRoot.AddComponent<CanvasScaler>();
		canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		canvasScaler.referenceResolution = new Vector2(1920f, 1080f);

		var text = new GameObject("Text");
		text.transform.SetParent(_overlayRoot.transform, false);
		
		var rectTransform = text.AddComponent<RectTransform>();
		rectTransform.anchorMin = new Vector2(0f, 1f);
		rectTransform.anchorMax = new Vector2(0f, 1f);
		rectTransform.pivot = new Vector2(0f, 1f);
		rectTransform.anchoredPosition = new Vector2(16f, -16f);
		rectTransform.sizeDelta = new Vector2(1200f, 1000f);

		_overlayText = text.AddComponent<Text>();

		_overlayText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		_overlayText.fontSize = OverlayFontSize;
		_overlayText.color = Color.white;
		_overlayText.alignment = TextAnchor.UpperLeft;
		_overlayText.horizontalOverflow = HorizontalWrapMode.Overflow;
		_overlayText.verticalOverflow = VerticalWrapMode.Overflow;
		_overlayText.raycastTarget = false;
		_overlayText.supportRichText = true;
	}

	private static void Reset()
	{
		ResetModules();
		ResetOverlay();
		TeardownOverlay();
	}

	private static void ResetModules()
	{
		foreach (var moduleName in ModuleStates.Keys)
		{
			var module = Modules.GetByName(moduleName);
			if (module != null)
			{
				ClearModuleState(module);
			}
		}
	}

	private static void TeardownOverlay()
	{
		_currentPageIndex = 0;
		if (_overlayRoot != null)
		{
			Object.Destroy(_overlayRoot);
			_overlayRoot = null;
			_overlayText = null;
		}
	}

}
