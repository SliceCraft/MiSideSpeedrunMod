using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Practice;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Menus.Practice;

public static class PracticeMenu
{
    public static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("PRACTICE")
            .SetBackButton(previousMenu)
            .Build();

        GameMenu startOfGameMenu = StartOfGamePracticeMenu.CreateMenu(menu);
        
        new MenuOptionFactory()
            .SetName("START OF GAME")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(startOfGameMenu) 
            .Build();
        
        GameMenu dummiesPuzzlesMenu = DummiesPuzzlesMenu.CreateMenu(menu);
        
        new MenuOptionFactory()
            .SetName("DUMMIES AND FORGOTTEN PUZZLES")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(dummiesPuzzlesMenu)
            .Build();
        
        GameMenu readingBooksMenu = ReadingBooksMenu.CreateMenu(menu);
        
        new MenuOptionFactory()
            .SetName("READING BOOKS, DESTROYING GLITCHES")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(readingBooksMenu) 
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

    private static void LoadMakeMannequin()
    {
        PracticeManager.SelectedGame = PracticeGames.MakeMannequin;
        GlobalGame.LoadingLevel = "Scene 10 - ManekenWorld";
        SceneManager.LoadScene("SceneLoading");
    }
}