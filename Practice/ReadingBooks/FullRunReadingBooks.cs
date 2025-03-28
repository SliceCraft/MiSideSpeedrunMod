using UnityEngine.SceneManagement;

namespace SpeedrunMod.Practice.ReadingBooks;

public class FullRunReadingBooks
{
    public static void StartRun()
    {
        PracticeManager.SelectedGame = PracticeGames.FullMilaRun;
        GlobalGame.LoadingLevel = "Scene 19 - Glasses";
        SceneManager.LoadScene("SceneLoading");
    }
}