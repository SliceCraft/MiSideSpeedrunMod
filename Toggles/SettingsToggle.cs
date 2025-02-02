using SpeedrunMod.ModSettings;
using UnityEngine;

namespace SpeedrunMod.Toggles;

public class SettingsToggle
{
    public static void Update()
    {
        if (!Input.GetKey(KeyCode.F1)) return;
        MonoGUITest monoGUITest = Object.FindObjectOfType<MonoGUITest>();
        if (monoGUITest == null)
        {
            Plugin.Log.LogInfo("Unable to toggle settings since MonoGUI hasn't been created yet");
            return;
        }
        monoGUITest.ToggleMenu();
    }
}