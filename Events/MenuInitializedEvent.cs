using MenuLib.API;
using MenuLib.API.Events;
using MenuLib.ModSettings;
using SpeedrunMod.Menus;

namespace SpeedrunMod.Events;

public class MenuInitializedEvent
{
    public static void RegisterEvent()
    {
        InitializedEvent.AddEventListener(OnMenuInitialized);
    }

    private static void OnMenuInitialized()
    {
        GameMenu menu = SettingsManager.GetModMenu("SPEEDRUN MOD");
        
        ModMenu.CreateMenu(menu);
    }
}