using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Menus.Practice;
using SpeedrunMod.Utils;
using UnityEngine;
namespace SpeedrunMod.Menus;

public static class ModMenu
{
    private static bool _outdated = (!MyPluginInfo.PLUGIN_VERSION.Equals(VersionText.NewestVersion) && VersionText.NewestVersion != null);
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
        
        new MenuOptionFactory()
            .SetName(name: _outdated?"INSTALL LATEST VERSION FROM GITHUB":"GITHUB PAGE")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(OpenGithub)
            .Build();
    }
    
    private static void OpenGithub()
    {
        Application.OpenURL(
            url: _outdated
                ? $"https://github.com/SliceCraft/MiSideSpeedrunMod/releases/tag/{VersionText.NewestVersion}"
                : $"https://github.com/SliceCraft/MiSideSpeedrunMod");
    }
}
