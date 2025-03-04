using UnityEngine;

namespace SpeedrunMod.Practice.StartOfGame;

public static class FullTamagotchiRun
{
    private static bool _loadQueued;
    
    // Surely there is a better way then just waiting a few frames before performing the actions
    // It'd be nice to improve on this since it does cost a few frames, but it's not hindering too much
    private static int _queueMobileInteractiveClick;
    private static int _queueButtonActivate;
    private static int _queueButtonClick;

    private static GameObject _smartPhone;
    private static World _gameWorld;
    
    public static void QueueLoad()
    {
        _loadQueued = true;
        
        // We reset these values since QueueLoad is used to reset the environment back to zero
        // It's a bit more complicated then that but that's the general gist of it
        _queueMobileInteractiveClick = 0;
        _queueButtonActivate = 0;
        _queueButtonClick = 0;
    }

    internal static void Update()
    {
        if (_loadQueued)
        {
            _loadQueued = false;
            Load();
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
        
        GameObject gameControlerGameObject = Object.FindObjectOfType<GameController>().gameObject;
        gameControlerGameObject.transform.Find("Player").gameObject.active = true;
        
        GameObject mobileInteractive = _gameWorld.transform.Find("World RealRoom/Interactives/Interactive Mobile").gameObject;
        mobileInteractive.active = true;
        ObjectInteractive mobileInteractiveObject = mobileInteractive.GetComponent<ObjectInteractive>();
        mobileInteractiveObject.active = true;

        _queueMobileInteractiveClick = 3;

        // TODO: Reload when loading scene 2
    }

    private static void MobileButtonInteractiveClick()
    {
        CleanupStartingScene(_gameWorld);
        
        GameObject mobileInteractive = _gameWorld.transform.Find("World RealRoom/Interactives/Interactive Mobile").gameObject;
        ObjectInteractive mobileInteractiveObject = mobileInteractive.GetComponent<ObjectInteractive>();
        mobileInteractiveObject.Click();
        
        _queueButtonActivate = 3;
    }

    private static void ButtonActivate()
    {
        _smartPhone.transform.Find("3D HintKey OpenMessage").gameObject.active = false;
        
        GameObject playButton = _smartPhone.transform.Find("3D HintKey Play").gameObject;
        playButton.active = true;
        
        _queueButtonClick = 3;
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
        gameTransform.Find("CutScenes").gameObject.active = false;
    }
}