using SpeedrunMod.Practice._2DCutting;
using SpeedrunMod.Practice.MakeMannequin;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Practice;

public class PracticeManager
{
    public static PracticeGames SelectedGame = PracticeGames.None;
    
    public static void OnSceneLoad(Scene scene)
    {
        if(SelectedGame == PracticeGames.None) return;
        switch (SelectedGame)
        {
            case PracticeGames.TamagotchiCutting:
                if(scene.name == "Scene 1 - RealRoom") TamagotchiCutting.QueueLoad();
                break;
            case PracticeGames.MakeMannequin:
                if(scene.name == "Scene 10 - ManekenWorld") MannequinMinigame.QueueLoad();
                break;
        }
    }

    public static void Update()
    {
        if(SelectedGame == PracticeGames.None) return;
        switch (SelectedGame)
        {
            case PracticeGames.TamagotchiCutting:
                TamagotchiCutting.Update();
                break;
            case PracticeGames.MakeMannequin:
                MannequinMinigame.Update();
                break;
        }
    }
}