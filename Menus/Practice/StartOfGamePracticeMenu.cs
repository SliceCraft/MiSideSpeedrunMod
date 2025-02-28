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
        
        // new MenuOptionFactory()
        //     .SetName("FULL TAMAGOTCHI RUN")
        //     .SetParent(menu)
        //     .PlaceOptionBefore(menu.MenuOptions.Count - 1)
        //     .SetNextLocation(menu) 
        //     .SetOnClick(LoadTamagotchiRun)
        //     .Build();

        return menu;
    }
    
    private static void LoadTamagotchiRun()
    {
        PracticeManager.SelectedGame = PracticeGames.FullTamagotchiRun;
        GlobalGame.LoadingLevel = "Scene 1 - RealRoom";
        SceneManager.LoadScene("SceneLoading");
    }
}