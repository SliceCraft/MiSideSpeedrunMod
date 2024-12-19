using SpeedrunMod.RevealSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
