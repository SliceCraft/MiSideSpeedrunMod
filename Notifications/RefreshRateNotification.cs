using SpeedrunMod.Events;
using UnityEngine;

namespace SpeedrunMod.Notifications;

internal sealed class RefreshRateNotification : TimedNotification
{
    protected override float PeriodicIntervalSeconds => 300f;
    
    protected override float CooldownSeconds => 150f;
    
    protected override void Initialize()
    {
        SceneLoadedEvent.SceneLoaded += (_, _) => Show();
    }

    protected override NotificationMessage GetNotification()
    {
        int hz = Screen.currentResolution.refreshRate;
        return new NotificationMessage($"Refresh rate: {hz} Hz");
    }
}
