namespace SpeedrunMod.Debug.Models;

internal record struct DebugOverlayContext(
	PlayerMove PlayerMove,
	DebugTime Time,
	DebugScreen Screen);
