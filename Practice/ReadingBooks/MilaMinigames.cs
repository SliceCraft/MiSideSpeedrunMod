using SpeedrunMod.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace SpeedrunMod.Practice.ReadingBooks;

public static class MilaMinigames
{
    public enum MilaMinigameModes
    {
        Laser,
        Towers,
        Shapes,
        Invaders
    }
    
    private static int _loadQueued;
    private static int _startQueued;
    
    private static GameObject _minigame1Clone;
    private static GameObject _minigame2Clone;
    private static GameObject _minigame3Clone;
    private static GameObject _minigame4Clone;
    
    private static GameObject _minigameGameObject;
    private static Camera _camera;
    public static MilaMinigameModes MilaMinigameMode = MilaMinigameModes.Laser;
    public static bool LoopThroughAllMinigames = false;
    
    public static void QueueLoad()
    {
        // Is this a weird way to bypass animations, yes, yes it is.
        // Maybe one day I'll figure out a better way to update the state without letting animations play but-
        // I guess this is fine for now
        _loadQueued = 120;
        
        // Also resetting to initial state
        _startQueued = 0;
        _minigame1Clone = null;
        _minigame2Clone = null;
        _minigame3Clone = null;
        _minigame4Clone = null;
        _minigameGameObject = null;
        _camera = null;
        
        HideCamera();
        
        Time.timeScale = 10f;
    }

    // We want to hide the camera when we need to wait before loading 
    private static void HideCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            Plugin.Log.LogInfo("Unable to disable Camera because it can't be found");
        }
        else
        {
            _camera = camera;
            _camera.gameObject.active = false;
        }
    }

    internal static void Update()
    {
        if (_loadQueued > 0)
        {
            _loadQueued--;
            if (_loadQueued == 0)
            {
                _loadQueued = 0;
                Load();                
            }
        }
        
        if (_startQueued > 0)
        {
            _startQueued--;
            if (_startQueued == 0)
            {
                _startQueued = 0;
                Location19_GlitchGame game = _minigameGameObject.GetComponent<Location19_GlitchGame>();
                StartGame(game);
            }
        }

        if (_minigameGameObject == null && _minigame1Clone != null && _minigame2Clone != null && _minigame3Clone != null && _minigame4Clone != null)
        {
            ReloadGame();
        }
    }

    private static void Load()
    {
        Time.timeScale = 1f;
        CleanupStartingScene();
            
        Location19_GlitchGame[] games = Object.FindObjectsOfType<Location19_GlitchGame>(true);
        foreach (Location19_GlitchGame glitchGame in games)
        {
            switch (glitchGame.gameObject.name)
            {
                case "GlitchGame 1":
                    _minigame1Clone = Object.Instantiate(glitchGame.gameObject, glitchGame.gameObject.transform.parent);
                    _minigame1Clone.active = false;
                    break;
                case "GlitchGame 2":
                    _minigame2Clone = Object.Instantiate(glitchGame.gameObject, glitchGame.gameObject.transform.parent);
                    _minigame2Clone.active = false;
                    break;
                case "GlitchGame 3":
                    _minigame3Clone = Object.Instantiate(glitchGame.gameObject, glitchGame.gameObject.transform.parent);
                    _minigame3Clone.active = false;
                    break;
                case "GlitchGame 4":
                    _minigame4Clone = Object.Instantiate(glitchGame.gameObject, glitchGame.gameObject.transform.parent);
                    _minigame4Clone.active = false;
                    break;
            }
        }

        ReloadGame();
    }

    private static void QueueStartGame(Location19_GlitchGame game)
    {
        game.gameObject.active = true;
        _minigameGameObject = game.gameObject;
        _startQueued = 5;
    }

    private static void StartGame(Location19_GlitchGame game)
    { 
        Time.timeScale = 1f;
        if (_camera != null)
        {
            _camera.gameObject.active = true;
            _camera = null;
        }
        
        game.gameObject.active = true;
        _minigameGameObject = game.gameObject;

        game.PlayGame();
    }

    private static void CleanupStartingScene()
    {
        World gameWorld = Object.FindObjectOfType<World>();
        
        if (gameWorld == null)
        {
            Plugin.Log.LogError("World could not be found during LaserMinigame Practice CleanupStart");
            return;
        }
        
        Transform gameTransform = gameWorld.gameObject.transform;
        gameTransform.Find("Dialogues").gameObject.active = false;
        gameTransform.Find("Quests/General").gameObject.active = false;
        gameTransform.Find("Quests/Quest 1 Знакомство").gameObject.active = false;
        gameTransform.Find("Quests/Quest 2 Симулятор жизни").gameObject.active = true;
    }

    private static void ReloadGame()
    {
        if (LoopThroughAllMinigames)
        {
            MilaMinigameMode += 1;
            if (MilaMinigameMode > MilaMinigameModes.Invaders) MilaMinigameMode = MilaMinigameModes.Laser;
        }
        
        GameObject go = null;
        switch (MilaMinigameMode)
        {
            case MilaMinigameModes.Laser:
                go = Object.Instantiate(_minigame1Clone, _minigame1Clone.transform.parent);
                break;
            case MilaMinigameModes.Towers:
                go = Object.Instantiate(_minigame2Clone, _minigame2Clone.transform.parent);
                break;
            case MilaMinigameModes.Shapes:
                go = Object.Instantiate(_minigame3Clone, _minigame3Clone.transform.parent);
                break;
            case MilaMinigameModes.Invaders:
                go = Object.Instantiate(_minigame4Clone, _minigame4Clone.transform.parent);
                break;
        }

        if (go == null)
        {
            Plugin.Log.LogError("While playing Mila minigames and reloading a gameobject couldn't be created");
            return;
        }
         
        Location19_GlitchGame game = go.GetComponent<Location19_GlitchGame>();
        
        QueueStartGame(game);
    }

    public static void GameEnded(Location19_GlitchGame game)
    {
        Time.timeScale = 10f;
        game.eventReady = new UnityEvent();
        HideCamera();
    }
}