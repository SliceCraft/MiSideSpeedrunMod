using SpeedrunMod.Practice.DummiesPuzzles;
using SpeedrunMod.Practice.MakeMannequin;
using SpeedrunMod.Practice.ReadingBooks;
using SpeedrunMod.Practice.StartOfGame;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Practice;

public static class PracticeManager
{
    public static PracticeGames SelectedGame { get; set; }  = PracticeGames.None;

    internal static void OnSceneLoad(Scene scene)
    {
        if(SelectedGame == PracticeGames.None) return;
        switch (SelectedGame)
        {
            case PracticeGames.TamagotchiCutting:
                if(scene.name == "Scene 1 - RealRoom") TamagotchiCutting.QueueLoad();
                break;
            case PracticeGames.FullTamagotchiRun:
                if(scene.name == "Scene 1 - RealRoom") FullTamagotchiRun.QueueLoad();
                if(scene.name == "Scene 2 - InGame") FullTamagotchiRun.LoadChapter();
                break;
            case PracticeGames.MakeMannequin:
                if(scene.name == "Scene 10 - ManekenWorld") MannequinMinigame.QueueLoad();
                break;
            case PracticeGames.FullRunStartOfGame:
                if(scene.name == "Scene 2 - InGame") FullRunStartOfGame.StartRun();
                break;
            case PracticeGames.ConnectTheDots:
                if(scene.name == "Scene 11 - Backrooms") ConnectTheDots.QueueLoad();
                break;
            case PracticeGames.FullMilaRun:
                if(scene.name == "Scene 20 - FightMita") FullRunReadingBooks.StartRun();
                break;
            case PracticeGames.MilaMinigames:
                if(scene.name == "Scene 19 - Glasses") MilaMinigames.QueueLoad();
                break;
            case PracticeGames.None:
            default:
                break;
        }
    }

    internal static void Update()
    {
        if(SelectedGame == PracticeGames.None) return;
        switch (SelectedGame)
        {
            case PracticeGames.TamagotchiCutting:
                TamagotchiCutting.Update();
                break;
            case PracticeGames.FullTamagotchiRun:
                FullTamagotchiRun.Update();
                break;
            case PracticeGames.MakeMannequin:
                MannequinMinigame.Update();
                break;
            case PracticeGames.ConnectTheDots:
                ConnectTheDots.Update();
                break;
            case PracticeGames.MilaMinigames:
                MilaMinigames.Update();
                break;
            case PracticeGames.None:
            case PracticeGames.FullRunStartOfGame:
            default:
                break;
        }
    }
}