using System;
using SpeedrunMod.Configs;
using SpeedrunMod.Overlay.Context;
using SpeedrunMod.Overlay.Modules;
using SpeedrunMod.Overlay.Snapshots;

namespace SpeedrunMod.Overlay.Modules.Display;

internal sealed class DisplayOverlayModule : IOverlayModule
{
	internal static readonly DisplayOverlayModule Instance = new DisplayOverlayModule();

	public string Name => "Display";

	public string GroupKey => "Core";

	private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(Math.Max(0f, OverlayConfig.OverlayLogInterval.Value));

	public TimeSpan UpdateInterval => _updateInterval;

	private DisplayOverlayModule()
	{
	}

	public void Reset()
	{
	}

	public IOverlaySnapshot Update(in OverlayContext ctx)
	{
		return new DisplayOverlaySnapshot(in ctx);
	}

	IOverlaySnapshot IOverlayModule.Update(in OverlayContext ctx)
	{
		return Update(in ctx);
	}
}
