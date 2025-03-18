using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Menus.Practice;
using SpeedrunMod.Utils;
using UnityEngine;
namespace SpeedrunMod.Menus;

public static class ModMenu
{
    private static readonly bool Outdated = !MyPluginInfo.PLUGIN_VERSION.Equals(VersionText.NewestVersion) && VersionText.NewestVersion != null;
    
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
            .SetName(name: Outdated ? "INSTALL LATEST VERSION FROM GITHUB" : "GITHUB PAGE")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(OpenGithub)
            .Build();
    }
    
    private static void OpenGithub()
    {
        Application.OpenURL(
            url: Outdated
                ? $"https://github.com/SliceCraft/MiSideSpeedrunMod/releases/tag/{VersionText.NewestVersion}"
                : "https://github.com/SliceCraft/MiSideSpeedrunMod");
    }
}
