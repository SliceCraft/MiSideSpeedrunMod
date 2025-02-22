using HarmonyLib;
using UnityEngine;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(ButtonMouseClick))]
internal class ButtonMouseClickPatch
{
    [HarmonyPatch(nameof(ButtonMouseClick.PointerDown))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void PointerDownPatch(ButtonMouseClick __instance)
    {
        /* I'm assuming that if we hit the reset button we can just execute the code.
         * Been having a lot of issues with finding out if the button is allowed to be pressed so I will just assume that this button will never be disabled.
         * This is a bad approach and should get a better investigation in the future but if it works it works.
         */
        if (!IsResetButton(__instance.gameObject)) return;
        AchievementsController achievementsController = Object.FindAnyObjectByType<AchievementsController>();
        if (achievementsController == null)
        {
            Plugin.Log.LogInfo("AchievementsController does not exist");
            return;
        }
            
        foreach (DataAchievementsValues dataAchievementsValues in achievementsController.dataAchievements)
        {
            dataAchievementsValues.get = false;
        }
            
        Menu menu = Object.FindAnyObjectByType<Menu>();
        if (menu == null)
        {
            Plugin.Log.LogInfo("Menu does not exist");
            return;
        }
        menu.textAchievementProgress.text = achievementsController.ProgressAchievementString();
    }

    private static bool IsResetButton(GameObject gameObject)
    {
        if(!gameObject.name.Equals("Button Yes")) return false;
        return gameObject.transform.parent != null && gameObject.transform.parent.gameObject.name.Equals("Location FullClear");
    }
}