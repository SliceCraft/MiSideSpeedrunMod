namespace SpeedrunMod.Overlay.Snapshots;

internal abstract class StyledOverlaySnapshot : IOverlaySnapshot
{
	private readonly OverlayTextStyle _style;

	protected StyledOverlaySnapshot()
		: this(OverlayTextStyle.None)
	{
	}

	protected StyledOverlaySnapshot(OverlayTextStyle style)
	{
		_style = style;
	}

	protected abstract string Body();

	public string Format()
	{
		var plain = Body() ?? string.Empty;
		return _style.ApplyTo(plain);
	}
}
