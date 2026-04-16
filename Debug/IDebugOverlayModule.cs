using SpeedrunMod.Debug.Models;

namespace SpeedrunMod.Debug;

internal interface IDebugOverlayModule
{
	string Name { get; }

	void Reset();

	IDebugOverlaySnapshot Update(in DebugOverlayContext ctx);
}
