using UnityEngine;

namespace SpeedrunMod.Notifications;

internal class NotificationMessage(string text, float cooldown = 0f)
{
    public string Text { get; } = text;
    public float Cooldown { get; } = cooldown;
    public float CreatedAt { get; } = Time.realtimeSinceStartup;
    internal GameObject HintObject = null;
    internal float TimeUntilHide = 5f;
    internal float TimeUntilDestroy = 6f;
    internal bool OnScreen => HintObject != null;
    internal bool IsExpired => Time.realtimeSinceStartup - CreatedAt >= Cooldown;
}
