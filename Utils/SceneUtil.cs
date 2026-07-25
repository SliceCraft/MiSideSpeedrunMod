using UnityEngine.SceneManagement;

namespace SpeedrunMod.Utils;

internal static class SceneUtil
{
    // Scene 7 is also named "Backrooms" (Cappie); keep these distinct.
    internal const string Scene11Backrooms = "Scene 11 - Backrooms";

    internal static bool IsActive(string sceneName) =>
        SceneManager.GetActiveScene().name == sceneName;
}
