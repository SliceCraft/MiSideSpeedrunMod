using System.Collections.Generic;
using Il2CppSystem.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunMod.EventDisplay;

public class EventManager
{
    private static GameObject HintScreenTemplate;
    private static GameObject InterfaceObject;
    private static List<ModEvent> EventObjects = new();
    
    public static void ShowEvent(ModEvent modEvent)
    {
        EnsureObjectsSelected();
        GameObject go = Object.Instantiate(HintScreenTemplate, InterfaceObject.gameObject.transform);

        Text text = go.GetComponentInChildren<Text>();
        text.text = modEvent.EventString;
        
        modEvent.HintObject = go;
        EventObjects.Add(modEvent);
        UpdatePositions();
        
        go.SetActive(true);
    }

    public static void UpdatePositions()
    {
        for (int i = EventObjects.Count - 1; i >= 0; i--)
        {
            GameObject go = EventObjects[i].HintObject;
            Vector3 pos = go.transform.position;
            pos.y = i * 100 + 100;
            go.transform.position = pos;
        }
    }

    public static void Update()
    {
        List<ModEvent> objectsToBeRemoved = new();
        
        foreach (ModEvent modEvent in EventObjects)
        {
            modEvent.TimeUntilHide -= Time.deltaTime;
            modEvent.TimeUntilDestroy -= Time.deltaTime;
            
            if (modEvent.TimeUntilHide <= 0)
            {
                modEvent.HintObject.GetComponent<Animator>().SetBool("Hide", true);
                // Prevent the hide animation from playing again
                modEvent.TimeUntilHide = 1e10f;
            }
            
            if (modEvent.TimeUntilDestroy <= 0)
            {
                objectsToBeRemoved.Add(modEvent);
            }
        }

        for(int i = 0; i < objectsToBeRemoved.Count; i++)
        {
            ModEvent modEvent = objectsToBeRemoved[i];
            EventObjects.Remove(modEvent);
            Object.Destroy(modEvent.HintObject);
        }

        if (objectsToBeRemoved.Count > 0)
        {
            UpdatePositions();
        }
    }

    private static void EnsureObjectsSelected()
    {
        if (HintScreenTemplate != null || InterfaceObject != null)
        {
            return;
        }
        GameController gc = Object.FindObjectOfType<GameController>();
        if (!gc)
        {
            Plugin.Log.LogInfo("Tried finding hint screen but a GameController couldn't be found.");
            return;
        }

        GameObject interfaceObject = null;
        for (int i = 0; i < gc.transform.childCount; i++)
        {
            GameObject go = gc.transform.GetChild(i).gameObject;
            if (go.name == "Interface")
            {
                interfaceObject = go;
            }
        }

        if (interfaceObject == null)
        {
            Plugin.Log.LogInfo("Tried finding hint screen but an interface couldn't be found in the GameController.");
            return;
        }

        InterfaceObject = interfaceObject;
        
        GameObject hintScreenObject = null;
        for (int i = 0; i < interfaceObject.transform.childCount; i++)
        {
            GameObject go = interfaceObject.transform.GetChild(i).gameObject;
            if (go.name == "HintScreen")
            {
                hintScreenObject = go;
            }
        }

        if (hintScreenObject == null)
        {
            Plugin.Log.LogInfo("Tried finding hint screen but was unable to find it in the GameController.");
            return;
        }

        HintScreenTemplate = hintScreenObject;
    } 
}