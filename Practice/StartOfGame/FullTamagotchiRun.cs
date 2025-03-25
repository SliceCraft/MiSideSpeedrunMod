using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpeedrunMod.Practice.StartOfGame;

public static class FullTamagotchiRun
{
    private static int _loadQueued;
    
    // Surely there is a better way then just waiting a few frames before performing the actions
    // It'd be nice to improve on this since it does cost a few frames, but it's not hindering too much
    private static int _queueMobileInteractiveClick;
    private static int _queueButtonActivate;
    private static int _queueButtonClick;

    private static GameObject _smartPhone;
    private static World _gameWorld;
    private static Camera _camera;
    
    public static void QueueLoad()
    {
        _loadQueued = 30;
        
        // We reset these values since QueueLoad is used to reset the environment back to zero
        // It's a bit more complicated then that but that's the general gist of it
        _queueMobileInteractiveClick = 0;
        _queueButtonActivate = 0;
        _queueButtonClick = 0;

        _smartPhone = null;
        _gameWorld = null;
        _camera = null;
    }

    internal static void Update()
    {
        if (_loadQueued > 0)
        {
            _loadQueued--;
            if (_loadQueued == 0)
            {
                Load();
            }
        }
        
        if (_queueMobileInteractiveClick > 0)
        {
            _queueMobileInteractiveClick--;
            if (_queueMobileInteractiveClick == 0)
            {
                MobileButtonInteractiveClick();                
            }
        }

        if (_queueButtonActivate > 0)
        {
            _queueButtonActivate--;
            if (_queueButtonActivate == 0)
            {
                ButtonActivate();                
            }
        }

        // ReSharper disable once InvertIf
        if (_queueButtonClick > 0)
        {
            _queueButtonClick--;
            if (_queueButtonClick == 0)
            {
                ButtonClick();                
            }
        }
    }

    private static void Load()
    {
        _gameWorld = Object.FindObjectOfType<World>();

        _smartPhone = _gameWorld.transform.Find("World RealRoom/Smartphone").gameObject;
        
        // We need to enable the player otherwise the smartphone grab action won't work
        GameObject gameControlerGameObject = Object.FindObjectOfType<GameController>().gameObject;
        gameControlerGameObject.transform.Find("Player").gameObject.active = true;
        
        // We enable the mobile interactive so it can be used in a next frame
        GameObject mobileInteractive = _gameWorld.transform.Find("World RealRoom/Interactives/Interactive Mobile").gameObject;
        mobileInteractive.active = true;
        ObjectInteractive mobileInteractiveObject = mobileInteractive.GetComponent<ObjectInteractive>();
        mobileInteractiveObject.active = true;
        
        // By cleaning up the starting scene we can skip the cutscene
        CleanupStartingScene(_gameWorld);
        
        // To prevent the player from seeing the game fastforward we disable the camera shotly
        _camera = _gameWorld.transform.Find("CutScenes/CutScene 1 (ДЕНЬ 1)/Camera/MainCamera").GetComponent<Camera>();
        _camera.gameObject.active = false;

        // By speeding up the game 10 times it'll play the animations faster
        // The phone grab animation has to finish before we can continue, I don't know why
        _queueMobileInteractiveClick = 1;
        Time.timeScale = 10f;
    }

    private static void MobileButtonInteractiveClick()
    {
        GameObject mobileInteractive = _gameWorld.transform.Find("World RealRoom/Interactives/Interactive Mobile").gameObject;
        ObjectInteractive mobileInteractiveObject = mobileInteractive.GetComponent<ObjectInteractive>();
        mobileInteractiveObject.Click();
        
        _queueButtonActivate = 300;
    }

    private static void ButtonActivate()
    {
        _smartPhone.transform.Find("3D HintKey OpenMessage").gameObject.active = false;
        
        GameObject playButton = _smartPhone.transform.Find("3D HintKey Play").gameObject;
        playButton.active = true;
        
        _queueButtonClick = 2;
    }

    private static void ButtonClick()
    {
        GameObject playButton = _smartPhone.transform.Find("3D HintKey Play").gameObject;
        Interface_KeyHint_Key keyHint = playButton.GetComponent<Interface_KeyHint_Key>();
        keyHint.KeyDown();
    }

    private static void CleanupStartingScene(World gameWorld)
    {
        if (gameWorld == null)
        {
            Plugin.Log.LogError("World could not be found during 2DCutting Practice CleanupStart");
            return;
        }
        
        Transform gameTransform = gameWorld.gameObject.transform;
        gameTransform.Find("CutScenes/CutScene 1 (ДЕНЬ 1)").gameObject.active = false;
    }

    public static void TamagotchiLoaded()
    {
        // Once the tamagotchi game has been loaded we can reset the timescale and enable the camera again
        Time.timeScale = 1f;
        _camera.gameObject.active = true;
    }

    public static void LoadChapter()
    {
        PracticeManager.SelectedGame = PracticeGames.FullTamagotchiRun;
        GlobalGame.LoadingLevel = "Scene 1 - RealRoom";
        SceneManager.LoadScene("SceneLoading");
    }
}