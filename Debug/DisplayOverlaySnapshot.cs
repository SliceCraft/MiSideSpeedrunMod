using SpeedrunMod.Debug.Models;

namespace SpeedrunMod.Debug;

internal readonly struct DisplayOverlaySnapshot : IDebugOverlaySnapshot
{
	internal int Width { get; }

	internal int Height { get; }

	internal int RefreshRate { get; }

	internal float Fps { get; }

	internal DisplayOverlaySnapshot(in DebugOverlayContext ctx)
	{
		Width = ctx.Screen.Width;
		Height = ctx.Screen.Height;
		var resolution = ctx.Screen.Resolution;
		RefreshRate = resolution.refreshRate;
		var unscaledDeltaTime = ctx.Time.UnscaledDeltaTime;
		Fps = 1f / unscaledDeltaTime;
	}

	public string Format()
	{
		return $"Resolution:\t{Width}x{Height}@{RefreshRate} Hz\nFPS:\t{Fps:F1}";
	}

	public override string ToString()
	{
		return Format();
	}
}
