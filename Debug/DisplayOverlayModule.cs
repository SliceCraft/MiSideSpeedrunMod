using SpeedrunMod.Debug.Models;

namespace SpeedrunMod.Debug;

internal sealed class DisplayOverlayModule : IDebugOverlayModule
{
	internal static readonly DisplayOverlayModule Instance = new DisplayOverlayModule();

	public string Name => "Display";

	private DisplayOverlayModule()
	{
	}

	public void Reset()
	{
	}

	public IDebugOverlaySnapshot Update(in DebugOverlayContext ctx)
	{
		return new DisplayOverlaySnapshot(in ctx);
	}

	IDebugOverlaySnapshot IDebugOverlayModule.Update(in DebugOverlayContext ctx)
	{
		return Update(in ctx);
	}
}
