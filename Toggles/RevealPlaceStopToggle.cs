using SpeedrunMod.EventDisplay;
using SpeedrunMod.RevealSystems;

namespace SpeedrunMod.Toggles;

internal static class RevealPlaceStopToggle
{
    internal static void Update()
    {
        if (!UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftAlt) || !UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.P)) return;
        if (PlaceStop.IsRevealing())
        {
            Plugin.Log.LogInfo("Hiding placestops");
            EventManager.ShowEvent(new ModEvent("Place stop toggle turned off"));
            PlaceStop.HidePlaceStops();
        }
        else
        {
            Plugin.Log.LogInfo("Revealing placestops");
            EventManager.ShowEvent(new ModEvent("Deprecated: Place stop toggle turned on"));
            PlaceStop.RevealPlaceStops();
        }
    }
}