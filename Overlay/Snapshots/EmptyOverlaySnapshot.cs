namespace SpeedrunMod.Overlay.Snapshots;

internal sealed class EmptyOverlaySnapshot : IOverlaySnapshot
{
	internal static readonly EmptyOverlaySnapshot Instance = new EmptyOverlaySnapshot();

	private EmptyOverlaySnapshot()
	{
	}

	public string Format()
	{
		return string.Empty;
	}
}
