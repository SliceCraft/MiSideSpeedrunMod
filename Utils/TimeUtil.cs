namespace SpeedrunMod.Utils;

internal static class TimeUtil
{
    internal static void StopTimeEvents(string name) =>
        ComponentUtil.FindIncludingInactive(name)?.GetComponent<Time_Events>()?.StopAllTime();
}
