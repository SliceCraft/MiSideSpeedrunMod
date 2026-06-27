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

        void Refresh()
        {
            _cachedGameController = Object.FindObjectOfType<GameController>();
            _gameControllerExpiry = now + ttlSeconds;
        }

        if (_cachedGameController == null)
        {
            // GameController is null only when we are still in the main menu.
            // So we need to refresh the cached game controller
            // ignoring the TTL, until the game starts.
            // This ensures that at the start of the game
            // callers don't have to wait for the TTL to expire.
            Refresh();
            return _cachedGameController;
        }

        if (now < _gameControllerExpiry)
        {
            return _cachedGameController;
        }

        Refresh();
        return _cachedGameController;
    }

    internal static bool IsInGame() => GetGameController() != null;
}