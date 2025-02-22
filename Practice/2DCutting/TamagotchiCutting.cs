using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.Practice._2DCutting;

public static class TamagotchiCutting
{
    private static bool _loadQueued;
    private static GameObject _cookingClone;
    private static GameObject _cookingGameObject;
    private static Tamagotchi_Main _tamagotchiMain;
    
    public static void QueueLoad()
    {
        _loadQueued = true;
    }

    public static void Update()
    {
        if (_loadQueued)
        {
            _loadQueued = false;
            Load();
        }

        if (_cookingGameObject == null && _cookingClone != null)
        {
            ReloadGame();
        }
    }

    private static void Load()
    {
        CleanupStartingScene();
        _tamagotchiMain = TamagotchiUtil.StartTamagotchi(0);

        TamagotchiGame_Cooking game = Object.FindObjectOfType<TamagotchiGame_Cooking>(true);
        if (game == null)
        {
            Plugin.Log.LogError("Cooking Game could not be found during 2DCutting Practice load");
            return;
        }
        
        _cookingClone = Object.Instantiate(game.gameObject, game.gameObject.transform.parent);
        StartGame(game);
    }

    private static void StartGame(TamagotchiGame_Cooking game)
    { 
        game.gameObject.active = true;
        _cookingGameObject = game.gameObject;
        Tamagotchi_MiniGame miniGame = game.gameObject.GetComponent<Tamagotchi_MiniGame>();
        
        if (miniGame == null)
        {
            Plugin.Log.LogError("Cooking MiniGame could not be found during 2DCutting Practice StartGame");
            return;
        }
        
        _tamagotchiMain.MiniGamePlay(miniGame);
    }

    private static void CleanupStartingScene()
    {
        World gameWorld = Object.FindObjectOfType<World>();
        
        if (gameWorld == null)
        {
            Plugin.Log.LogError("World could not be found during 2DCutting Practice CleanupStart");
            return;
        }
        
        Transform gameTransform = gameWorld.gameObject.transform;
        gameTransform.Find("CutScenes").gameObject.active = false;
        gameTransform.Find("Quests").gameObject.active = false;
    }

    private static void ReloadGame()
    {
        GameObject go = Object.Instantiate(_cookingClone, _cookingClone.transform.parent);

        TamagotchiGame_Cooking game = go.GetComponent<TamagotchiGame_Cooking>();
        if (game == null)
        {
            Plugin.Log.LogError("Cooking Game could not be found during 2DCutting Practice Reload");
            return;
        }
        
        StartGame(game);
    }
}