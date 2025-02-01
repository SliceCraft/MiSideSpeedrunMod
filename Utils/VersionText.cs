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
        Menu menu = Object.FindObjectOfType<Menu>(true);
        GameObject nameGameObject = GetNameGameObject(menu.gameObject);
        if(DoesVersionTextExist(nameGameObject)) return;
        GameObject textVersionObject = GetTextVersionObject(nameGameObject);
        GameObject speedrunVersionText = Object.Instantiate(textVersionObject, nameGameObject.transform);
        
        speedrunVersionText.name = "SpeedrunModVersionText";
        
        Vector3 pos = speedrunVersionText.transform.position;
        pos.y += 0.006f;
        speedrunVersionText.transform.position = pos;
            
        Text text = speedrunVersionText.GetComponent<Text>();
        if (MyPluginInfo.PLUGIN_VERSION.Equals(NewestVersion))
        {
            text.text = $"Slice's Speedrun Mod : {MyPluginInfo.PLUGIN_VERSION}";
        }
        else
        {
            text.text = $"Slice's Speedrun Mod V{MyPluginInfo.PLUGIN_VERSION} : Version outdated, newest version is V{NewestVersion}";
            text.resizeTextForBestFit = true;
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
}