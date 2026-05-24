using SpeedrunMod.Overlay.Snapshots;

namespace SpeedrunMod.Overlay.Modules.Display;

internal readonly struct DisplayOverlaySnapshot : IOverlaySnapshot
{
	private readonly int _width;

	private readonly int _height;

	private readonly int _refreshRate;

	private readonly float _fps;
    
    private readonly int _frame;

	internal DisplayOverlaySnapshot(int width, int height, int refreshRate, float fps, int frame)
	{
		_width = width;
		_height = height;
		_refreshRate = refreshRate;
		_fps = fps;
		_frame = frame;
	}

	public string Format()
	{
		return $"Resolution:\t{_width}x{_height}@{_refreshRate} Hz\nFPS:\t{_fps:F1}\nFrame:\t{_frame}";
	}

	public override string ToString()
	{
		return Format();
	}
}
