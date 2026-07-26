namespace SpeedrunMod.Utils;

internal static class TimeEventUtils
{
    internal static void StopAll(string name) =>
        GameObjectUtils.FindIncludingInactive(name)?.GetComponent<Time_Events>()?.StopAllTime();
}
