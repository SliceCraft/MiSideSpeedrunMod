using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Practice;
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

        return menu;
    }

    private static void Load2DCutting()
    {
        PracticeManager.SelectedGame = PracticeGames.TamagotchiCutting;
        GlobalGame.LoadingLevel = "Scene 1 - RealRoom";
        SceneManager.LoadScene("SceneLoading");
    }
}