using UnityEngine.SceneManagement;

namespace SpeedrunMod.Practice.StartOfGame;

public static class FullRunStartOfGame
{
    public static void StartRun()
    {
        PracticeManager.SelectedGame = PracticeGames.FullRunStartOfGame;
        GlobalGame.LoadingLevel = "Scene 1 - RealRoom";
        SceneManager.LoadScene("SceneLoading");
    }
}