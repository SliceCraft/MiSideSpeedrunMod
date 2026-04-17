namespace SpeedrunMod.Overlay.Context;

internal record struct TimeContext(
	float DeltaTime,
	float UnscaledDeltaTime,
	float FixedDeltaTime,
	float TimeScale,
	float RealtimeSinceStartup,
	int FrameCount);
