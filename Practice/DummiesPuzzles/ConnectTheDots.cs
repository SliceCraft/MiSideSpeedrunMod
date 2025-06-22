using System;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SpeedrunMod.Practice.DummiesPuzzles;

internal class ConnectTheDots
{
    private static bool _loadQueued;
    private static bool _doorOpenQueued;
    private static int _gameStartQueued = -1;
    private static GameObject _lever1Clone;
    private static GameObject _lever2Clone;
    private static GameObject _lever1Object;
    private static GameObject _lever2Object;
    private static Transform _roomTransform;
    public static int PlayingGame = -1;
    public static bool SwitchGames = false;
    
    public static void QueueLoad()
    {
        _loadQueued = true;
        _gameStartQueued = -1;
    }

    internal static void Update()
    {
        if (_loadQueued)
        {
            _loadQueued = false;
            Load();
        }

        if (_doorOpenQueued)
        {
            _doorOpenQueued = false;
            DoorOpen();
        }

        if (_gameStartQueued >= 0)
        {
            if (_gameStartQueued == 0)
            {
                GameStart();
            }
            _gameStartQueued--;
        }
    }

    private static void Load()
    {
        PrepareScene();
        PrepareClones();
        _doorOpenQueued = true;
    }

    private static void PrepareScene()
    {
        World gameWorld = Object.FindObjectOfType<World>();
        
        if (gameWorld == null)
        {
            Plugin.Log.LogError("World could not be found during ConnectTheDots Practice PrepareScene");
            return;
        }
        
        Transform gameTransform = gameWorld.gameObject.transform;
        gameTransform.Find("World/Backrooms/Room 6 (City)").gameObject.active = true;
        gameTransform.Find("World/Backrooms/Room 7 (Fog)").gameObject.active = true;
    }

    private static void PrepareClones()
    {
        World gameWorld = Object.FindObjectOfType<World>();
        
        if (gameWorld == null)
        {
            Plugin.Log.LogError("World could not be found during ConnectTheDots Practice PrepareClones");
            return;
        }        
        
        _roomTransform = gameWorld.gameObject.transform.Find("World/Backrooms/Room 7 (Fog)/House/").gameObject.transform;
        _lever1Object = gameWorld.gameObject.transform.Find("World/Backrooms/Room 7 (Fog)/House/Switch 1").gameObject;
        _lever2Object = gameWorld.gameObject.transform.Find("World/Backrooms/Room 7 (Fog)/House/Switch 2").gameObject;
        _lever1Clone = Object.Instantiate(_lever1Object);
        _lever2Clone = Object.Instantiate(_lever2Object);
        _lever1Clone.active = false;
        _lever2Clone.active = false;
    }

    private static void DoorOpen()
    {
        World gameWorld = Object.FindObjectOfType<World>();
        
        if (gameWorld == null)
        {
            Plugin.Log.LogError("World could not be found during ConnectTheDots Practice DoorOpen");
            return;
        }
        
        Transform gameTransform = gameWorld.gameObject.transform;
        GameObject door = gameTransform.Find("World/Backrooms/Room 6 (City)/R6 Door").gameObject;
        ObjectInteractive objectInteractive = door.GetComponent<ObjectInteractive>();
        objectInteractive.Click();

        Time.timeScale = 10;
        
        PlayNextGame();
    }

    internal static void PlayNextGame()
    {
        if (SwitchGames) PlayingGame = PlayingGame == 1 ? 2 : 1;
        
        if (PlayingGame == 1)
        {
            PlayingGame = 1;
            GameObject game = PlayGame(_lever1Object, _lever1Clone);
            GameObject gameLines = game.transform.Find("GameLines 1/Interface Lines").gameObject;
            gameLines.active = true;
            _lever1Object = game;
        }
        else
        {
            GameObject game = PlayGame(_lever2Object, _lever2Clone);
            GameObject gameLines = game.transform.Find("GameLines 2/Interface Lines").gameObject;
            gameLines.active = true;
            _lever2Object = game;
        }

        Time.timeScale = 10;
        _gameStartQueued = 60;
    }

    private static GameObject PlayGame(GameObject objectToDestroy, GameObject cloneToPrepare)
    {
        Object.Destroy(objectToDestroy);
        GameObject game = Object.Instantiate(cloneToPrepare, _roomTransform);
        game.active = true;
        return game;
    }

    private static void GameStart()
    {
        Time.timeScale = 1;
        GameObject gameLines;
        if (PlayingGame == 1)
        {
            gameLines = _lever1Object.transform.Find("GameLines 1/Interface Lines").gameObject;
        }
        else
        {
            gameLines = _lever2Object.transform.Find("GameLines 2/Interface Lines").gameObject;
        }
        Location11_GameLinesMain game = gameLines.GetComponent<Location11_GameLinesMain>();
        game.eventStopLevel.AddListener((UnityAction) PlayNextGame);
        game.StartGame();
    }
}