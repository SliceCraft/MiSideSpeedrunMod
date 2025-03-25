using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Practice;
using SpeedrunMod.Practice.StartOfGame;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Menus.Practice;

public static class StartOfGamePracticeMenu
{
    public static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("START OF GAME")
            .SetBackButton(previousMenu)
            .Build();
        
        new MenuOptionFactory()
            .SetName("FULL CHAPTER RUN")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu) 
            .SetOnClick(FullRunStartOfGame.StartRun)
            .Build();
        
        // TODO: Put tutorial close button here
        
        new MenuOptionFactory()
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .BuildMenuDivider();
        
        new MenuOptionFactory()
            .SetName("FULL TAMAGOTCHI RUN")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu) 
            .SetOnClick(FullTamagotchiRun.LoadChapter)
            .Build();
        
        new MenuOptionFactory()
            .SetName("CARROT CUTTING")
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