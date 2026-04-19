using SpeedrunMod.Overlay.Snapshots;

namespace SpeedrunMod.Overlay.Modules.Display;

internal readonly struct DisplayOverlaySnapshot : IOverlaySnapshot
{
	internal int Width { get; }

	internal int Height { get; }

	internal int RefreshRate { get; }

	internal float Fps { get; }

	internal DisplayOverlaySnapshot(int width, int height, int refreshRate, float fps)
	{
		Width = width;
		Height = height;
		RefreshRate = refreshRate;
		Fps = fps;
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
