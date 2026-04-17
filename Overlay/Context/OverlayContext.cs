namespace SpeedrunMod.Overlay.Context;

internal record struct OverlayContext(
	PlayerMove PlayerMove,
	TimeContext Time,
	ScreenContext Screen);
