namespace SpeedrunMod.Overlay.Snapshots;

internal sealed class TextOverlaySnapshot : StyledOverlaySnapshot
{
	private readonly string _text;

	internal TextOverlaySnapshot(string text)
		: this(text, OverlayTextStyle.None)
	{
	}

	internal TextOverlaySnapshot(string text, OverlayTextStyle style)
		: base(style)
	{
		_text = text;
	}

	protected override string Body() => _text;
}
