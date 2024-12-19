using SpeedrunMod.RevealSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedrunMod.Toggles
{
    internal class RevealPlaceStopToggle
    {
        public static void Update()
        {
            if (!UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt) || !UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.P)) return;
            if (PlaceStop.IsRevealing())
            {
                Plugin.Log.LogInfo("Hiding placestops");
                PlaceStop.HidePlaceStops();
            }
            else
            {
                Plugin.Log.LogInfo("Revealing placestops");
                PlaceStop.RevealPlaceStops();
            }
        }
    }
}
