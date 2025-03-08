using HarmonyLib;
using UnityEngine;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(Location5_World))]
// ReSharper disable once InconsistentNaming
internal class Location5_WorldPatch
{
    private static AchievementsController _achievementsController = null;
    
    [HarmonyPatch(nameof(Location5_World.Update))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static void UpdatePatch(object[] __args)
    {
        if (_achievementsController == null)
        {
            _achievementsController = Object.FindObjectOfType<AchievementsController>();
        }
        if(_achievementsController == null) return;
        foreach (DataAchievementsValues dataAchievement in _achievementsController.dataAchievements)
        {
            if (dataAchievement.steamAchievement.Equals("ACHI_ClickShadow"))
            {
                if (!dataAchievement.get)
                {
                    Plugin.Log.LogInfo("Creak in the Dark achievement obtained through mod");
                    _achievementsController.AchievementCompleted("ACHI_ClickShadow");
                }
                break;
            }
        }
    }
}