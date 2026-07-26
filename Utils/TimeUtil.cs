namespace SpeedrunMod.Utils;

internal static class TimeUtil
{
    internal static void StopTimeEvents(string name) =>
        ComponentUtil.FindIncludingInactive<Time_Events>(name)?.StopAllTime();
}
