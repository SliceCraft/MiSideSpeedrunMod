using UnityEngine;

namespace SpeedrunMod.Utils;

internal static class GameUtil
{
    private static GameController _cachedGameController;
    private static float _gameControllerExpiry;

    internal static GameController GetGameController()
    {
        const float ttlSeconds = 5f;

        var now = Time.realtimeSinceStartup;
        if (now < _gameControllerExpiry)
        {
            return _cachedGameController;
        }

        _cachedGameController = Object.FindObjectOfType<GameController>();
        _gameControllerExpiry = now + ttlSeconds;
        return _cachedGameController;
    }

    internal static bool IsInGame() => GetGameController() != null;
}