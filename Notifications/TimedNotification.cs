using UnityEngine;

namespace SpeedrunMod.Notifications;

internal abstract class TimedNotification
{
    private float _nextAllowedShowAt;
    private float _nextPeriodicShowAt;
    private bool _initialized;

    protected virtual float CooldownSeconds => 10f;
    protected virtual float PeriodicIntervalSeconds => 30f;

    protected virtual void Initialize()
    {
    }

    protected void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        Initialize();
        _initialized = true;
    }

    public void Update()
    {
        EnsureInitialized();

        float now = Time.realtimeSinceStartup;
        if (now < _nextPeriodicShowAt)
        {
            return;
        }

        Show();
    }
    
    public void Show(bool force = false)
    {
        EnsureInitialized();

        float now = Time.realtimeSinceStartup;
        if (now < _nextAllowedShowAt && !force)
        {
            return;
        }

        ShowNotification(now);
    }

    protected void ShowNotification(float now)
    {
        var notificationMessage = GetNotification();
        if (notificationMessage == null)
        {
            return;
        }

        if (!NotificationManager.Show(notificationMessage))
        {
            return;
        }

        _nextAllowedShowAt = now + CooldownSeconds;
        _nextPeriodicShowAt = now + PeriodicIntervalSeconds;
    }

    protected abstract NotificationMessage GetNotification();
}
