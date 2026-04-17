using System;
using SpeedrunMod.Configs;
using SpeedrunMod.Overlay.Context;
using SpeedrunMod.Overlay.Modules;
using SpeedrunMod.Overlay.Snapshots;

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

	public IOverlaySnapshot Update(in OverlayContext ctx)
	{
		if (ctx.Screen.Resolution.refreshRate <= RefreshRateConfig.InvalidThresholdHz)
		{
			return EmptyOverlaySnapshot.Instance;
		}

		return new TextOverlaySnapshot(
			$"Invalid run: refresh rate {ctx.Screen.Resolution.refreshRate} Hz exceeds {RefreshRateConfig.InvalidThresholdHz} Hz.",
			OverlayTextStyle.Warning);
	}
}
