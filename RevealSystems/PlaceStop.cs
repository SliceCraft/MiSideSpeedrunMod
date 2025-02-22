using System.Collections.Generic;
using System.Linq;
using SpeedrunMod.EventDisplay;
using UnityEngine;

namespace SpeedrunMod.RevealSystems;

internal static class PlaceStop
{
    private static readonly List<GameObject> GameObjects = [];
    private static bool _isRevealing;
    private static readonly int Color = Shader.PropertyToID("_Color");
    private static readonly int Mode = Shader.PropertyToID("_Mode");
    private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
    private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
    
    internal static void RevealPlaceStops()
    {
        EventManager.ShowEvent(new ModEvent("Revealing placestops"));
        HidePlaceStops();
        _isRevealing = true;
        GameObject[] objects = Object.FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in objects)
        {
            if (!obj.name.StartsWith("PlaceStop")) continue;
            GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObject.transform.position = obj.transform.position;
            newObject.transform.rotation = obj.transform.rotation;
            newObject.transform.parent = obj.transform;
            newObject.transform.localScale = new Vector3(1, 1, 1);

            newObject.name = "RevealBox" + obj.name;

            newObject.GetComponent<BoxCollider>().enabled = false;

            MeshRenderer meshRenderer = newObject.GetComponent<MeshRenderer>();

            Material mat = new Material(Shader.Find("Standard"));
            mat.SetColor(Color, new Color(0, 1, 0, .5f));
            mat.SetFloat(Mode, 3);
            mat.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;

            Color color = mat.color;
            color.a = .5f;
            color.r = 0;
            color.g = 1;
            color.b = 0;
            mat.color = color;

            meshRenderer.material = mat;

            GameObjects.Add(newObject);
        }
    }

    internal static void HidePlaceStops()
    {
        _isRevealing = false;
        foreach (GameObject gameObject in GameObjects.Where(gameObject => gameObject != null))
        {
            Object.Destroy(gameObject);
        }
        GameObjects.Clear();
    }

    public static bool IsRevealing()
    {
        return _isRevealing;
    }
}