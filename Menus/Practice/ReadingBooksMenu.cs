using MenuLib.API;
using MenuLib.API.Factories;
using SpeedrunMod.Practice;
using SpeedrunMod.Practice.ReadingBooks;
using SpeedrunMod.Practice.StartOfGame;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Menus.Practice;

public static class ReadingBooksMenu
{
    public static GameMenu CreateMenu(GameMenu previousMenu)
    {
        GameMenu menu = new MenuFactory()
            .SetTitle("READING BOOKS")
            .SetBackButton(previousMenu)
            .Build();
        
        new MenuOptionFactory()
            .SetName("FULL CHAPTER RUN")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu) 
            .SetOnClick(FullRunReadingBooks.StartRun)
            .Build();
        
        new MenuOptionFactory()
            .SetName("PLAY ALL MINIGAMES")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu) 
            .SetOnClick(LoadAllMinigames)
            .Build();
        
        new MenuOptionFactory()
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .BuildMenuDivider();
        
        new MenuOptionFactory()
            .SetName("LASER MINIGAME")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu) 
            .SetOnClick(LoadLaserMinigame)
            .Build();
        
        new MenuOptionFactory()
            .SetName("SHAPES MINIGAME")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu) 
            .SetOnClick(LoadShapesMinigame)
            .Build();
        
        new MenuOptionFactory()
            .SetName("TOWERS MINIGAME")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu) 
            .SetOnClick(LoadTowersMinigame)
            .Build();
        
        new MenuOptionFactory()
            .SetName("INVADERS MINIGAME")
            .SetParent(menu)
            .PlaceOptionBefore(menu.MenuOptions.Count - 1)
            .SetNextLocation(menu) 
            .SetOnClick(LoadInvadersMinigame)
            .Build();

        return menu;
    }
    
    private static void LoadAllMinigames()
    {
        MilaMinigames.LoopThroughAllMinigames = true;
        // When `LoopThroughAllMinigames` is enabled it will go to the next game on reload
        // and it will also reload during the first time you load the minigame
        // If we set the minigame mode to Laser it will actually play towers
        MilaMinigames.MilaMinigameMode = MilaMinigames.MilaMinigameModes.Invaders;
        PracticeManager.SelectedGame = PracticeGames.MilaMinigames;
        GlobalGame.LoadingLevel = "Scene 19 - Glasses";
        SceneManager.LoadScene("SceneLoading");
    }
    
    private static void LoadLaserMinigame()
    {
        MilaMinigames.LoopThroughAllMinigames = false;
        MilaMinigames.MilaMinigameMode = MilaMinigames.MilaMinigameModes.Laser;
        PracticeManager.SelectedGame = PracticeGames.MilaMinigames;
        GlobalGame.LoadingLevel = "Scene 19 - Glasses";
        SceneManager.LoadScene("SceneLoading");
    }
    
    private static void LoadShapesMinigame()
    {
        MilaMinigames.LoopThroughAllMinigames = false;
        MilaMinigames.MilaMinigameMode = MilaMinigames.MilaMinigameModes.Shapes;
        PracticeManager.SelectedGame = PracticeGames.MilaMinigames;
        GlobalGame.LoadingLevel = "Scene 19 - Glasses";
        SceneManager.LoadScene("SceneLoading");
    }
    
    private static void LoadTowersMinigame()
    {
        MilaMinigames.LoopThroughAllMinigames = false;
        MilaMinigames.MilaMinigameMode = MilaMinigames.MilaMinigameModes.Towers;
        PracticeManager.SelectedGame = PracticeGames.MilaMinigames;
        GlobalGame.LoadingLevel = "Scene 19 - Glasses";
        SceneManager.LoadScene("SceneLoading");
    }
    
    private static void LoadInvadersMinigame()
    {
        MilaMinigames.LoopThroughAllMinigames = false;
        MilaMinigames.MilaMinigameMode = MilaMinigames.MilaMinigameModes.Invaders;
        PracticeManager.SelectedGame = PracticeGames.MilaMinigames;
        GlobalGame.LoadingLevel = "Scene 19 - Glasses";
        SceneManager.LoadScene("SceneLoading");
    }
}