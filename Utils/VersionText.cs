using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunMod.Utils;

internal static class VersionText
{
    // I know this is a bad way of adding the text, too bad
    private static float _timeUntilShow = 7f;
    private static bool _isShowing;
    internal static string NewestVersion { get; set; }

    internal static void Start()
    {
        _timeUntilShow = 7f;
        _isShowing = false;
        // Test();
    }

    internal static void Update()
    {
        _timeUntilShow -= Time.deltaTime;
        if (!(_timeUntilShow <= 0) || _isShowing) return;
        _isShowing = true;
        AddVersionText();
    }

    private static void AddVersionText()
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
        if (!MyPluginInfo.PLUGIN_VERSION.Equals(NewestVersion) && NewestVersion != null)
        {
            text.text = $"Slice's Speedrun Mod V{MyPluginInfo.PLUGIN_VERSION} : Version outdated, newest version is V{NewestVersion}";
            text.resizeTextForBestFit = true;
        }
        else
        {
            text.text = $"Slice's Speedrun Mod : V{MyPluginInfo.PLUGIN_VERSION}";
        }

        Localization_UIText uiText = speedrunVersionText.GetComponent<Localization_UIText>();
        if (uiText != null)
        {
            Object.Destroy(uiText);
        }
    }

    private static GameObject GetNameGameObject(GameObject menu)
    {
        for (int i = 0; i < menu.transform.childCount; i++)
        {
            GameObject child = menu.transform.GetChild(i).gameObject;
            switch (child.name)
            {
                case "Canvas":
                    return GetNameGameObject(child);
                case "NameGame":
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