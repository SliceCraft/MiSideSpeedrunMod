using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod.Practice.MakeMannequin;

public class MannequinMinigame
{
    private static bool _loadQueued;
    private static bool _automateLoadQueued;
    private static MinigamesAutomate _minigamesAutomate;

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

        if (_automateLoadQueued)
        {
            _automateLoadQueued = false;
            _minigamesAutomate.StartLoading();
        }

        if (_minigamesAutomate != null)
        {
            if (_minigamesAutomate.asyncLoading.isDone && _minigamesAutomate.loading)
            {
                _minigamesAutomate.loading = false;
                _minigamesAutomate.StartGame();
            }
        }

        // if (_cookingGameObject == null && _cookingClone != null)
        // {
        //     ReloadGame();
        // }
    }

    public static void Load()
    {
        _minigamesAutomate = Object.FindObjectOfType<MinigamesAutomate>(true);
        if (_minigamesAutomate == null)
        {
            Plugin.Log.LogInfo("No minigamesAutomate found while loading MakeMannequin");
            return;
        }

        EnsureParentsLoaded(_minigamesAutomate.gameObject);
        _automateLoadQueued = true;
    }

    private static void EnsureParentsLoaded(GameObject go)
    {
        go.active = true;
        if(go.transform.parent != null) EnsureParentsLoaded(go.transform.parent.gameObject);
    }

}