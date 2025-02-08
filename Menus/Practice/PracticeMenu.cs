using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Practice;
using SpeedrunMod.Practice.MakeMannequin;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Menus.Practice;

public class PracticeMenu
{
    public static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("PRACTICE")
            .SetBackButton(previousMenu)
            .Build();
        
        // Yes it's redirecting to itself, the onclick action will
        // go to a different scene meaning that we don't need to load a next location 
        new MenuOptionFactory()
            .SetName("2D CUTTING")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu) 
            .SetOnClick(Load2DCutting)
            .Build();
        
        // Disabled for now until I figure out how to make this work
        // new MenuOptionFactory()
        //     .SetName("MAKE MANNEQUIN")
        //     .SetParent(menu)
        //     .PlaceOptionBefore(menu.MenuOptions.Count - 1)
        //     .SetNextLocation(menu) 
        //     .SetOnClick(LoadMakeMannequin)
        //     .Build();

        return menu;
    }

    private static void Load2DCutting()
    {
        PracticeManager.SelectedGame = PracticeGames.TamagotchiCutting;
        GlobalGame.LoadingLevel = "Scene 1 - RealRoom";
        SceneManager.LoadScene("SceneLoading");
    }

    private static void LoadMakeMannequin()
    {
        PracticeManager.SelectedGame = PracticeGames.MakeMannequin;
        GlobalGame.LoadingLevel = "Scene 10 - ManekenWorld";
        SceneManager.LoadScene("SceneLoading");
    }
}