using HarmonyLib;
using SpeedrunMod.Configs;
using UnityEngine;

namespace SpeedrunMod.Patches;

[HarmonyPatch(typeof(Screen))]
[HarmonyPatch(nameof(Screen.currentResolution), MethodType.Getter)]
internal static class ScreenCurrentResolutionPatch
{
    [HarmonyPostfix]
    private static void Postfix(ref Resolution __result)
    {
        if (!RefreshRateConfig.OverrideEnabled.Value) return;

        int hz = RefreshRateConfig.GetActualHz();
        __result.refreshRate = hz;
        __result.m_RefreshRate = hz;
    }
}
