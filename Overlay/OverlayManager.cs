using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpeedrunMod.Configs;
using SpeedrunMod.Overlay.Context;
using SpeedrunMod.Overlay.Modules;
using SpeedrunMod.Overlay.Modules.Display;
using SpeedrunMod.Overlay.Modules.Movement;
using SpeedrunMod.Overlay.Modules.Persistent;
using UnityEngine;
using UnityEngine.UI;
using SpeedrunMod.Menus.Keybinds;

namespace SpeedrunMod.Overlay;

internal static class OverlayManager
{
	private const int OverlayFontSize = 20;

	private static readonly IOverlayModule[] Modules = new IOverlayModule[]
	{
		MovementOverlayModule.Instance,
		DisplayOverlayModule.Instance,
		RefreshRateWarningOverlayModule.Instance
	};

	private static readonly StringBuilder OverlayBuilder = new StringBuilder();
	
	private static readonly Dictionary<string, ModuleState> ModuleStates = new Dictionary<string, ModuleState>();

	private static GameController _controller;

	private static GameObject _overlayRoot;

	private static Text _overlayText;

	private static int _currentPageIndex;

	private static readonly KeyCode NextGroupKey = KeyCode.PageDown;
	private static readonly KeyCode PrevGroupKey = KeyCode.PageUp;

	internal static void Update()
	{
		if (TryInitialize(out var controller))
		{
			Update(controller);
		}
	}

	private static bool TryInitialize(out GameController controller)
	{
		controller = Object.FindObjectOfType<GameController>();
		if (controller == null)
		{
			TeardownOverlay();
			ResetModules();
			_controller = null;
			return false;
		}

		if (controller != _controller)
		{
			_controller = controller;
			TeardownOverlay();
			ResetModules();
			return false;
		}

		return true;
	}

	private static void Update(GameController controller)
	{
		var ctx = BuildContext(controller);
		ResetOverlay();

		var moduleStates = GetModuleStates(in ctx);
		if (moduleStates.Count == 0)
		{
			TeardownOverlay();
			return;
		}

		HandleGroupPaging(moduleStates.Count);
		EnsureOverlay();

		RenderCurrentGroup(moduleStates);
		UpdateOverlay();
	}

	private static Dictionary<string, List<ModuleState>> GetModuleStates(in OverlayContext ctx)
	{
		var globalEnabled = OverlayConfig.OverlayEnabled.Value;
		var groupToStates = new Dictionary<string, List<ModuleState>>();

		foreach (var module in Modules)
		{
			var shouldRender = globalEnabled || module is IPersistentOverlayModule;
			if (!shouldRender)
			{
				ClearModuleState(module);
				continue;
			}

			var state = GetOrCreateModuleState(module);
			var realtimeSinceStartup = ctx.Time.RealtimeSinceStartup;
			var updateIntervalSeconds = System.Math.Max(0, module.UpdateInterval.TotalSeconds);
			var shouldUpdateNow = state.CachedSnapshot is null || realtimeSinceStartup - state.LastUpdatedRealtime >= updateIntervalSeconds;

			if (shouldUpdateNow)
			{
				state.CachedSnapshot = module.Update(in ctx);
				state.LastUpdatedRealtime = realtimeSinceStartup;
			}

			var groupKey = string.IsNullOrWhiteSpace(module.GroupKey) ? "General" : module.GroupKey;
			if (!groupToStates.TryGetValue(groupKey, out var statesInGroup))
			{
				statesInGroup = new List<ModuleState>();
				groupToStates[groupKey] = statesInGroup;
			}

			statesInGroup.Add(state);
		}

		return groupToStates;
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
		if (!ModuleStates.TryGetValue(module.Name, out var state))
		{
			state = new ModuleState { ModuleName = module.Name };
			ModuleStates[module.Name] = state;
		}

		return state;
	}

	private static void HandleGroupPaging(int groupCount)
	{
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

	private static void RenderCurrentGroup(Dictionary<string, List<ModuleState>> groupsByKey)
	{
		var groupCount = groupsByKey.Count;
		_currentPageIndex = Mathf.Clamp(_currentPageIndex, 0, groupCount - 1);

		var keyIndex = 0;
		string activeKey = null;
		List<ModuleState> activeStates = null;
		
		foreach (var kv in groupsByKey.OrderBy(kv => kv.Key))
		{
			if (keyIndex == _currentPageIndex)
			{
				activeKey = kv.Key;
				activeStates = kv.Value;
				break;
			}

			keyIndex++;
		}

		if (activeStates == null)
		{
			return;
		}

		if (groupCount > 1)
		{
			OverlayBuilder.AppendLine($"Overlay group {_currentPageIndex + 1}/{groupCount}: {activeKey}");
			OverlayBuilder.AppendLine($"Switch group: {PrevGroupKey}/{NextGroupKey}");
			OverlayBuilder.AppendLine();
		}

		foreach (var state in activeStates)
		{
			var content = state.CachedSnapshot?.Format() ?? string.Empty;

			if (string.IsNullOrWhiteSpace(content))
			{
				continue;
			}

			OverlayBuilder.AppendLine($"[{state.ModuleName}]");
			OverlayBuilder.AppendLine(content);
			OverlayBuilder.AppendLine();
		}
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

	private static void ResetModules()
	{
		foreach (var module in Modules)
		{
			module.Reset();
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

	private static OverlayContext BuildContext(GameController controller)
	{
		var playerMove = ResolvePlayerMove(controller);
		var time = new TimeContext(Time.deltaTime, Time.unscaledDeltaTime, Time.fixedDeltaTime, Time.timeScale, Time.realtimeSinceStartup, Time.frameCount);
		var screen = new ScreenContext(Screen.width, Screen.height, Screen.currentResolution);
		return new OverlayContext(playerMove, time, screen);
	}

	private static PlayerMove ResolvePlayerMove(GameController controller)
	{
		var playerMove = Object.FindObjectOfType<PlayerMove>();
		if (playerMove != null)
		{
			return playerMove;
		}

		var player = controller.transform.Find("Player");
		if (player != null)
		{
			return player.GetComponent<PlayerMove>();
		}

		return null;
	}
}
