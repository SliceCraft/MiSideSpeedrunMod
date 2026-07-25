using MenuLib.API;
using MenuLib.API.Events;
using SpeedrunMod.Menus;

namespace SpeedrunMod.Events;

internal static class MenuInitializedEvent
{
    internal static void RegisterEvent()
    {
        InitializedEvent.AddEventListener(OnMenuInitialized);
    }

    private static void OnMenuInitialized()
    {
        GameMenu menu = SettingsManager.GetModMenu("SPEEDRUN MOD");
        
        ModMenu.CreateMenu(menu);
    }
}