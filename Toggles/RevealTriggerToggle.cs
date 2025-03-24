using SpeedrunMod.EventDisplay;
using SpeedrunMod.RevealSystems;

namespace SpeedrunMod.Toggles;

internal static class RevealTriggerToggle
{
    internal static void Update()
    {
        if (!UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt) || !UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.O)) return;
        Plugin.Log.LogInfo("Toggling trigger");
        if (Triggers.IsRevealing())
        {
            EventManager.ShowEvent(new ModEvent("Trigger Toggle turned off"));
            Triggers.HideTriggers();
        }
        else
        {
            EventManager.ShowEvent(new ModEvent("Trigger Toggle turned on"));
            Triggers.RevealTriggers();
        }
    }
}