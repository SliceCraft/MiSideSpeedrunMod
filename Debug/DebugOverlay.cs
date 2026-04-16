using System.Text;
using SpeedrunMod.Configs;
using SpeedrunMod.Debug.Models;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunMod.Debug;

internal static class DebugOverlay
{
	private static readonly IDebugOverlayModule[] Modules = new IDebugOverlayModule[2]
	{
		MovementOverlayModule.Instance,
		DisplayOverlayModule.Instance
	};

	private static readonly StringBuilder OverlayBuilder = new StringBuilder();

	private static GameController _controller;

	private static GameObject _overlayRoot;

	private static Text _overlayText;

	private static float _lastOverlayRefreshTime;

	internal static void Update()
	{
		if (TryInitialize(out var controller))
		{
			Update(controller);
		}
	}

	private static bool TryInitialize(out GameController controller)
	{
		controller = null;
		if (!DebugConfig.OverlayEnabled.Value)
		{
			TeardownOverlay();
			ResetModules();
			_controller = null;
			return false;
		}
		
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
		var value = DebugConfig.OverlayLogInterval.Value;
		if (value > 0f)
		{
			var realtimeSinceStartup = Time.realtimeSinceStartup;
			if (realtimeSinceStartup - _lastOverlayRefreshTime < value)
			{
				return;
			}

			_lastOverlayRefreshTime = realtimeSinceStartup;
		}

		var ctx = BuildContext(controller);
		ResetOverlay();

		foreach (var module in Modules)
		{
			UpdateModule(module, in ctx);
		}

		EnsureOverlay();
		UpdateOverlay();
	}

	private static void UpdateModule(IDebugOverlayModule m, in DebugOverlayContext ctx)
	{
		OverlayBuilder.AppendLine($"[DebugOverlay] {m.Name}:");
		OverlayBuilder.AppendLine(m.Update(in ctx).Format());
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

		_overlayRoot = new GameObject("SpeedrunMod_DebugOverlay");
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

		_overlayText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		_overlayText.fontSize = 16;
		_overlayText.color = Color.white;
		_overlayText.alignment = 0;
		_overlayText.horizontalOverflow = 0;
		_overlayText.verticalOverflow = 0;
		_overlayText.raycastTarget = false;
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
		if (_overlayRoot != null)
		{
			Object.Destroy(_overlayRoot);
			_overlayRoot = null;
			_overlayText = null;
		}
	}

	private static DebugOverlayContext BuildContext(GameController controller)
	{
		var playerMove = ResolvePlayerMove(controller);
		var time = new DebugTime(Time.deltaTime, Time.unscaledDeltaTime, Time.fixedDeltaTime, Time.timeScale, Time.realtimeSinceStartup, Time.frameCount);
		var screen = new DebugScreen(Screen.width, Screen.height, Screen.currentResolution);
		return new DebugOverlayContext(playerMove, time, screen);
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
