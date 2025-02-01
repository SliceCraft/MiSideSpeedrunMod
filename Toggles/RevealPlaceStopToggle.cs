using SpeedrunMod.RevealSystems;

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
