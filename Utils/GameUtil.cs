using UnityEngine;

namespace SpeedrunMod.Utils;

internal static class GameUtil
{
    internal static GameController GetGameController()
    {
        return Object.FindObjectOfType<GameController>();
    }

    internal static bool IsInGame() => GetGameController() != null;
}