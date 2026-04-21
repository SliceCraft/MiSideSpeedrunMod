using UnityEngine;

namespace SpeedrunMod.Notifications;

internal class NotificationMessage(string text)
{
    public string Text { get; } = text;
    internal GameObject HintObject = null;
    internal float TimeUntilHide = 5f;
    internal float TimeUntilDestroy = 6f;
}