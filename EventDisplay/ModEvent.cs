using UnityEngine;

namespace SpeedrunMod.EventDisplay;

internal class ModEvent(string eventString)
{
    public string EventString { get; } = eventString;
    internal GameObject HintObject = null;
    internal float TimeUntilHide = 5f;
    internal float TimeUntilDestroy = 6f;
}