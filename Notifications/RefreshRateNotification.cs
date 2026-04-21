using SpeedrunMod.Configs;
using SpeedrunMod.Events;

namespace SpeedrunMod.Notifications;

internal sealed class RefreshRateNotification : TimedNotification
{
    protected override void Initialize()
    {
        SceneLoadedEvent.SceneLoaded += (_, _) => Show();
    }

    protected override NotificationMessage GetNotification()
    {
        int hz = RefreshRateConfig.CurrentRefreshRateHz;
        return new NotificationMessage($"Refresh rate: {hz} Hz");
    }
}
