using System.Net.Http;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using SpeedrunMod.Events;
using SpeedrunMod.ModSettings;
using SpeedrunMod.Patches;
using SpeedrunMod.Utils;
using UnityEngine;

namespace SpeedrunMod;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("SliceCraft.MenuLib")]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;

    private readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

    public override void Load()
    {
        // Plugin startup logic
        Log = base.Log;

        harmony.PatchAll();

        SceneLoadedEvent.RegisterEvent();
        MenuInitializedEvent.RegisterEvent();

        // Refer to ModSettings/MonoGUITest for why this is disabled
        // ClassInjector.RegisterTypeInIl2Cpp<MonoGUITest>();

        GetVersion().Wait();

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }
    
    static async Task GetVersion()
    {
        // Call asynchronous network methods in a try/catch block to handle exceptions.
        try
        {
            HttpClient client = new HttpClient();
            using HttpResponseMessage response = await client.GetAsync("https://tuyu.slicegames.nl/assets/speedrunmodversion");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            VersionText.NewestVersion = responseBody;
        }
        catch (HttpRequestException e)
        {
            Log.LogError("Unable to request version");
        }
    }
}
