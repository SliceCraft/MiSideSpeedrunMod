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
                Plugin.Log.LogInfo("Hiding triggers");
                Triggers.HideTriggers();
            }
            else
            {
                Plugin.Log.LogInfo("Revealing triggers");
                Triggers.RevealTriggers();
            }
        }
    }
}
