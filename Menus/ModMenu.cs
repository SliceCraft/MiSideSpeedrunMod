using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Menus.Practice;

namespace SpeedrunMod.Menus;

public class ModMenu
{
    public static void CreateMenu(GameMenu menu)
    {
        GameMenu practiceMenu = PracticeMenu.CreateMenu(menu);
        
        new MenuOptionFactory()
            .SetName("PRACTICE")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(practiceMenu)
            .Build();
    }
}