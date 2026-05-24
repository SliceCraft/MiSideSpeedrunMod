using System;
using SpeedrunMod.Configs;
using SpeedrunMod.Overlay.Snapshots;

namespace SpeedrunMod.Overlay.Modules.Persistent;

internal sealed class RefreshRateWarningOverlayModule : IPersistentOverlayModule
{
	internal static readonly RefreshRateWarningOverlayModule Instance = new RefreshRateWarningOverlayModule();

	public string Name => "Refresh Rate Warning";

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
		var refreshRate = RefreshRateConfig.GetActualHz();
		if (refreshRate <= RefreshRateConfig.InvalidThresholdHz)
		{
			return EmptyOverlaySnapshot.Instance;
		}

		return new TextOverlaySnapshot(
			$"The refresh rate ({refreshRate} Hz) exceeds the maximum allowed limit ({RefreshRateConfig.InvalidThresholdHz} Hz); this run will be invalidated.",
			OverlayTextStyle.Warning);
	}
}
