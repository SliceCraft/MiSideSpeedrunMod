using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Menus.Practice;
using SpeedrunMod.Utils;
using UnityEngine;
namespace SpeedrunMod.Menus;

public static class ModMenu
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
        
        new MenuOptionFactory()
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .BuildMenuDivider();
        //The text inside the button could be changed to be shorter - Erik
        if (!MyPluginInfo.PLUGIN_VERSION.Equals(VersionText.NewestVersion) && VersionText.NewestVersion != null)
        {
            new MenuOptionFactory()
                .SetName("INSTALL LATEST VERSION FROM GITHUB")
                .SetParent(menu)
                .PlaceOptionBefore(menu.MenuOptions.Count - 1)
                .SetNextLocation(menu)
                .SetOnClick(OpenGithub)
                .Build();
        }
        else
        {
            new MenuOptionFactory()
                .SetName("GITHUB PAGE")
                .SetParent(menu)
                .PlaceOptionBefore(menu.MenuOptions.Count - 1)
                .SetNextLocation(menu)
                .SetOnClick(OpenGithub)
                .Build();
        }
    }
    
    private static void OpenGithub()
    {
        if (!MyPluginInfo.PLUGIN_VERSION.Equals(VersionText.NewestVersion) && VersionText.NewestVersion != null)
        {
            Application.OpenURL(
                url: $"https://github.com/SliceCraft/MiSideSpeedrunMod/releases/tag/{VersionText.NewestVersion}");
        }
        else
        {
            Application.OpenURL(
                url: $"https://github.com/SliceCraft/MiSideSpeedrunMod");
        }
    }
}
