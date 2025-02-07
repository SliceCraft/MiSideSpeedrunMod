using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace SpeedrunMod.Utils;

public class VersionText
{
    // I know this is a bad way of adding the text, too bad
    private static float TimeUntilShow = 7f;
    private static bool IsShowing;
    public static string NewestVersion = null;

    public static void SkipWait()
    {
        Plugin.Log.LogInfo(TimeUntilShow);
        TimeUntilShow = 2f;
        Plugin.Log.LogInfo(TimeUntilShow);
    }

    public static void Start()
    {
        TimeUntilShow = 7f;
        IsShowing = false;
        // Test();
    }

    public static void Update()
    {
        TimeUntilShow -= Time.deltaTime;
        if (TimeUntilShow <= 0 && !IsShowing)
        {
            IsShowing = true;
            AddVersionText();
        }
    }

    public static void AddVersionText()
    {
        global::Menu menu = Object.FindObjectOfType<global::Menu>(true);
        GameObject nameGameObject = GetNameGameObject(menu.gameObject);
        if(DoesVersionTextExist(nameGameObject)) return;
        GameObject textVersionObject = GetTextVersionObject(nameGameObject);
        GameObject speedrunVersionText = Object.Instantiate(textVersionObject, nameGameObject.transform);
        
        speedrunVersionText.name = "SpeedrunModVersionText";
        
        Vector3 pos = speedrunVersionText.transform.position;
        pos.y += 0.006f;
        speedrunVersionText.transform.position = pos;
            
        Text text = speedrunVersionText.GetComponent<Text>();
        if (!MyPluginInfo.PLUGIN_VERSION.Equals(NewestVersion) && NewestVersion != null)
        {
            text.text = $"Slice's Speedrun Mod V{MyPluginInfo.PLUGIN_VERSION} : Version outdated, newest version is V{NewestVersion}";
            text.resizeTextForBestFit = true;
        }
        else
        {
            text.text = $"Slice's Speedrun Mod : V{MyPluginInfo.PLUGIN_VERSION}";
        }
    }

    private static GameObject GetNameGameObject(GameObject menu)
    {
        for (int i = 0; i < menu.transform.childCount; i++)
        {
            GameObject child = menu.transform.GetChild(i).gameObject;
            if (child.name.Equals("Canvas"))
            {
                return GetNameGameObject(child);
            }
            
            if (child.name.Equals("NameGame"))
            {
                return child;
            }
        }
        return null;
    }

    private static bool DoesVersionTextExist(GameObject nameGameObject)
    {
        for (int i = 0; i < nameGameObject.transform.childCount; i++)
        {
            GameObject child = nameGameObject.transform.GetChild(i).gameObject; 
            if (child.name.Equals("SpeedrunModVersionText"))
            {
                return true;
            }
        }
        return false;
    }

    private static GameObject GetTextVersionObject(GameObject nameGameObject)
    {
        for (int i = 0; i < nameGameObject.transform.childCount; i++)
        {
            GameObject child = nameGameObject.transform.GetChild(i).gameObject; 
            if (child.name.Equals("TextVersion"))
            {
                return child;
            }
        }
        return null;
    }


    // TEST STARTS HERE
    // TEST STARTS HERE
    // TEST STARTS HERE
    // TEST STARTS HERE
    // TEST STARTS HERE
    // TEST STARTS HERE
    // TEST STARTS HERE
    // TEST STARTS HERE
    // TEST STARTS HERE
    
    public static void Test()
    {
        global::Menu menu = Object.FindObjectOfType<global::Menu>(true);
        if (menu == null)
        {
            Plugin.Log.LogError("No menu found for TEST");
            return;
        }

        GameObject go = GetFrameMenu(menu.gameObject);
        Plugin.Log.LogInfo(go.gameObject.name);
        GameObject location = GetLocationMenu(go);
        Plugin.Log.LogInfo(location.gameObject.name);
        GameObject clone = Object.Instantiate(location, go.transform);
        Plugin.Log.LogInfo(clone.gameObject.name);
        
        for (int i = 0; i < clone.transform.childCount; i++)
        {
            GameObject child = clone.transform.GetChild(i).gameObject;
            if (child.name.StartsWith("Button"))
            {
                PrepareButton(child);
            }
        }
    }

    private static void PrepareButton(GameObject go)
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            GameObject child = go.transform.GetChild(i).gameObject;
            if (child.name.Equals("Text"))
            {
                // child.Find();
                Object.Destroy(child.GetComponent<Localization_UIText>());
                child.GetComponent<Text>().text = "BUTTON MANIPULATED";
            }
        }
    }
    
    private static GameObject GetLocationMenu(GameObject frameMenu)
    {
        Plugin.Log.LogInfo("Searching for location menu");
        for (int i = 0; i < frameMenu.transform.childCount; i++)
        {
            GameObject child = frameMenu.transform.GetChild(i).gameObject;
            Plugin.Log.LogInfo(child.gameObject.name);
            if (child.name.Equals("Location Menu"))
            {
                Plugin.Log.LogInfo("Location menu found");
                return child;
            }
        }
        return null;
    }
    
    private static GameObject GetFrameMenu(GameObject menu)
    {
        Plugin.Log.LogInfo("Searching for frame menu");
        for (int i = 0; i < menu.transform.childCount; i++)
        {
            GameObject child = menu.transform.GetChild(i).gameObject;
            if (child.name.Equals("Canvas"))
            {
                Plugin.Log.LogInfo($"Canvas found {child.name}");
                return GetFrameMenu(child);
            }
            
            if (child.name.Equals("FrameMenu"))
            {
                Plugin.Log.LogInfo($"Frame menu found {child.name}");
                return child;
            }
        }
        return null;
    }
}