using SpeedrunMod.Notifications;
using SpeedrunMod.RevealSystems;

namespace SpeedrunMod.Toggles;

internal static class RevealTriggerToggle
{
    internal static void Update()
    {
        if (!UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt) || !UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.O)) return;
        if (Triggers.IsRevealing())
        {
            NotificationManager.Show(new NotificationMessage("Trigger Toggle turned off"));
            Plugin.Log.LogInfo("Toggling on show trigger");
            Triggers.HideTriggers();
        }
        else
        {
            NotificationManager.Show(new NotificationMessage("Trigger Toggle turned on"));
            Plugin.Log.LogInfo("Toggling off show trigger");
            Triggers.RevealTriggers();
        }
    }
}