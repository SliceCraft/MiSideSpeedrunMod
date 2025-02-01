using SpeedrunMod.EventDisplay;
using SpeedrunMod.RevealSystems;

namespace SpeedrunMod.Toggles
{
    internal class RevealTriggerToggle
    {
        public static void Update()
        {
            if (!UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt) || !UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.O)) return;
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
}
