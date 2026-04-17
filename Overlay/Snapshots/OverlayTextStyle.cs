using UnityEngine;

namespace SpeedrunMod.Overlay.Snapshots;

internal readonly struct OverlayTextStyle
{
	public static OverlayTextStyle None => default;

	public static OverlayTextStyle Warning => new OverlayTextStyle(textColor: new Color(1f, 0.4f, 0.4f), bold: true);

	public int? FontSize { get; }

	public Color? TextColor { get; }

	public bool Bold { get; }

	public OverlayTextStyle(int? fontSize = null, Color? textColor = null, bool bold = false)
	{
		FontSize = fontSize;
		TextColor = textColor;
		Bold = bold;
	}

	public string ApplyTo(string body)
	{
		if (string.IsNullOrEmpty(body))
		{
			return body;
		}

		var t = body;
		if (Bold)
		{
			t = $"<b>{t}</b>";
		}

		if (FontSize.HasValue)
		{
			t = $"<size={FontSize.Value}>{t}</size>";
		}

		if (TextColor.HasValue)
		{
			var c = TextColor.Value;
			t = $"<color=#{ColorToHex(c)}>{t}</color>";
		}

		return t;
	}

	private static string ColorToHex(Color c)
	{
		var r = Mathf.RoundToInt(Mathf.Clamp01(c.r) * 255f);
		var g = Mathf.RoundToInt(Mathf.Clamp01(c.g) * 255f);
		var b = Mathf.RoundToInt(Mathf.Clamp01(c.b) * 255f);
		var a = Mathf.RoundToInt(Mathf.Clamp01(c.a) * 255f);
		return $"{r:X2}{g:X2}{b:X2}{a:X2}";
	}
}
