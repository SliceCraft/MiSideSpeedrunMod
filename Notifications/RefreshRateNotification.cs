using SpeedrunMod.Configs;
using SpeedrunMod.Events;

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
        int hz = RefreshRateConfig.GetActualHz();
        return new NotificationMessage($"Refresh rate: {hz} Hz");
    }
}
