namespace SpeedrunMod.Debug.Models;

internal record struct DebugTime(
	float DeltaTime,
	float UnscaledDeltaTime,
	float FixedDeltaTime,
	float TimeScale,
	float RealtimeSinceStartup,
	int FrameCount);