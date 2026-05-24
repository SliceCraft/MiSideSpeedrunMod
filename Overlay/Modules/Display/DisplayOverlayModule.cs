using System;
using SpeedrunMod.Configs;
using SpeedrunMod.Overlay.Snapshots;
using UnityEngine;

namespace SpeedrunMod.Overlay.Modules.Display;

internal sealed class DisplayOverlayModule : IOverlayModule
{
	internal static readonly DisplayOverlayModule Instance = new();

	public string Name => "Display";

	public string GroupKey => "Core";

	public TimeSpan UpdateInterval { get; } = TimeSpan.FromSeconds(Math.Max(0f, OverlayConfig.OverlayLogInterval.Value));

	public void Reset()
	{
	}

	public IOverlaySnapshot Update()
	{
		return new DisplayOverlaySnapshot(
			Screen.width,
			Screen.height,
			RefreshRateConfig.GetActualHz(),
			1f / Time.unscaledDeltaTime,
			Time.frameCount
		);
	}
}
