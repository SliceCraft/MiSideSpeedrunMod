using UnityEngine;

namespace SpeedrunMod.EventDisplay;

public class ModEvent
{
    public string EventString { get; }
    public GameObject HintObject = null;
    public float TimeUntilHide = 5f;
    public float TimeUntilDestroy = 6f;
    
    public ModEvent(string eventString)
    {
        EventString = eventString;
    }
}