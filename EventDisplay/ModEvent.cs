using UnityEngine;

namespace SpeedrunMod.EventDisplay;

public class ModEvent(string eventString)
{
    public string EventString { get; } = eventString;
    public GameObject HintObject = null;
    public float TimeUntilHide = 5f;
    public float TimeUntilDestroy = 6f;
}