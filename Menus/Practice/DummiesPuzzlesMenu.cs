using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Practice;
using SpeedrunMod.Practice.DummiesPuzzles;
using SpeedrunMod.Practice.StartOfGame;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Menus.Practice;

public class DummiesPuzzlesMenu
{
    public static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("DUMMIES AND FORGOTTEN PUZZLES")
            .SetBackButton(previousMenu)
            .Build();
        
        new MenuOptionFactory()
            .SetName("CONNECT THE DOTS")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(LoadConnectTheDots)
            .Build();
        
        new MenuOptionFactory()
            .SetName("CONNECT THE DOTS GAME 1")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(LoadConnectTheDotsGameOne)
            .Build();
        
        new MenuOptionFactory()
            .SetName("CONNECT THE DOTS GAME 2")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu)
            .SetOnClick(LoadConnectTheDotsGameTwo)
            .Build();

        return menu;
    }
    
    private static void LoadConnectTheDots()
    {
        // This value is set to 2 because during the first game load this value be switched to 2
        ConnectTheDots.PlayingGame = 2;
        ConnectTheDots.SwitchGames = true;
        PracticeManager.SelectedGame = PracticeGames.ConnectTheDots;
        GlobalGame.LoadingLevel = "Scene 11 - Backrooms";
        SceneManager.LoadScene("SceneLoading");
    }
    
    private static void LoadConnectTheDotsGameOne()
    {
        ConnectTheDots.PlayingGame = 1;
        ConnectTheDots.SwitchGames = false;
        PracticeManager.SelectedGame = PracticeGames.ConnectTheDots;
        GlobalGame.LoadingLevel = "Scene 11 - Backrooms";
        SceneManager.LoadScene("SceneLoading");
    }
    
    private static void LoadConnectTheDotsGameTwo()
    {
        ConnectTheDots.PlayingGame = 2;
        ConnectTheDots.SwitchGames = false;
        PracticeManager.SelectedGame = PracticeGames.ConnectTheDots;
        GlobalGame.LoadingLevel = "Scene 11 - Backrooms";
        SceneManager.LoadScene("SceneLoading");
    }
}