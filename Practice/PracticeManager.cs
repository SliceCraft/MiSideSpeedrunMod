using SpeedrunMod.Practice._2DCutting;
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
        }
    }
}