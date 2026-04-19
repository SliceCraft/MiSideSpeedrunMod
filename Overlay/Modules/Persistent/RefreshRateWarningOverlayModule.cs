using System;
using SpeedrunMod.Configs;
using SpeedrunMod.Overlay.Snapshots;
using UnityEngine;

namespace SpeedrunMod.Overlay.Modules.Persistent;

internal sealed class RefreshRateWarningOverlayModule : IPersistentOverlayModule
{
	internal static readonly RefreshRateWarningOverlayModule Instance = new RefreshRateWarningOverlayModule();

	public string Name => "Refresh Warning";

	public string GroupKey => "Warnings";

	public TimeSpan UpdateInterval => TimeSpan.FromSeconds(5);

	private RefreshRateWarningOverlayModule()
	{
	}

	public void Reset()
	{
	}

	public IOverlaySnapshot Update()
	{
		var refreshRate = Screen.currentResolution.refreshRate;
		if (refreshRate <= RefreshRateConfig.InvalidThresholdHz)
		{
			return EmptyOverlaySnapshot.Instance;
		}

		return new TextOverlaySnapshot(
			$"Invalid run: refresh rate {refreshRate} Hz exceeds {RefreshRateConfig.InvalidThresholdHz} Hz.",
			OverlayTextStyle.Warning);
	}
}
