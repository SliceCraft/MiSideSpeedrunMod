using System.Collections.Generic;
using System.Linq;
using SpeedrunMod.EventDisplay;
using UnityEngine;
using UnityEngine.UI;

namespace SpeedrunMod.RevealSystems;

internal static class Triggers
{
    internal static class Triggers
    {
        private static readonly List<GameObject> GameObjects = new List<GameObject>();
        private static readonly List<GameObject> Labels = new List<GameObject>();
        private static bool _isRevealing;
        private static readonly int Color = Shader.PropertyToID("_Color");
        private static readonly int Mode = Shader.PropertyToID("_Mode");
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");

        internal static void RevealTriggers()
        {
            //EventManager.ShowEvent(new ModEvent("Revealing Triggers"));
            HideTriggers();
            _isRevealing = true;
            
        ProcessTriggers<Trigger_DistanceCamera>("distancecamera");
        ProcessTriggers<Trigger_DistanceCheck>("distancecheck");
        ProcessTriggers<Trigger_DistanceCircle>("distancecircle");
        ProcessTriggers<Trigger_Event>("event");
        ProcessTriggers<Trigger_MouseClick>("mouseclick");
        ProcessTriggers<Trigger_MouseEvent>("mouseevent");
        ProcessTriggers<Trigger_Teleport>("teleport");
        ProcessTriggers<Trigger_Zoom>("zoom");
    }
        
    private static void ProcessTriggers<T>(string type) where T : Component
    {
        var objects = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var obj in objects)
        {
            GameObject gameObject = obj.gameObject;
            AddTriggerRevealer(gameObject, type);
        }

        private static void AddTriggerRevealer(GameObject gameObject, string type)
        {
            // Create a new primitive cube for visualization
            GameObject newObject = CreateRevealCube(gameObject);

            // Create and configure a canvas and text for display
            GameObject canvasGUI = CreateCanvas(gameObject);
            GameObject textGUI = CreateTextUI(canvasGUI, type, gameObject.name);
            
            SetMaterialForObject(newObject, type);
            newObject.GetComponent<BoxCollider>().enabled = false;

            //Add the objects to the collections
            //Plugin.Log.LogInfo("Revealing trigger for " + gameObject.name);
            GameObjects.Add(newObject);
            Labels.Add(canvasGUI);
        }
        private static GameObject CreateRevealCube(GameObject parent)
        {
            GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObject.transform.SetParent(parent.transform, false);
            newObject.name = "RevealBox" + parent.name;
            return newObject;
        }
        
        private static GameObject CreateCanvas(GameObject parent)
        {
            GameObject canvasGUI = new GameObject("Canvas " + parent.name);
            canvasGUI.AddComponent<Canvas>();
            canvasGUI.AddComponent<CanvasScaler>();
            canvasGUI.AddComponent<GraphicRaycaster>();
            canvasGUI.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            canvasGUI.transform.SetParent(parent.transform, false);
            canvasGUI.transform.localPosition = Vector3.up * .5f;
            canvasGUI.GetComponent<RectTransform>().localScale = new Vector3(0.01f, 0.01f);
            return canvasGUI;
        }
        
        private static GameObject CreateTextUI(GameObject parent, string type, string name)
        {
            GameObject textGUI = new GameObject("Text " + parent.name);
            textGUI.transform.SetParent(parent.transform, false);
            Text text = textGUI.AddComponent<Text>();
            text.text = type + " : " + name;
            text.fontSize = 30;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.color = GetColorForTrigger(type);
            text.color = new Color(text.color.r, text.color.g, text.color.b, 1f);

            // Adjust the position, scale, and rotation
            text.rectTransform.localPosition = Vector3.zero;
            text.rectTransform.localScale = new Vector3(0.3f, 0.3f, 1);
            text.rectTransform.rotation = Quaternion.Euler(0, 180, 0);
            return textGUI;
        }
        
        private static void SetMaterialForObject(GameObject newObject, string type)
        {
            MeshRenderer meshRenderer = newObject.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));
            
            mat.SetColor(Color, GetColorForTrigger(type));
            mat.SetColor("_EmissionColor", mat.color);
            mat.EnableKeyword("_EMISSION");
            
            mat.SetFloat(Mode, 3);
            mat.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            //This fucking sucks, please help me
            //Some objects hide even the labels. It seems to be inconsistent and I can't for the life of me make it work
            //Perhaps implementing a brand-new shader to make it so we on't have to go thru all these hoops.
            //newObject.layer = LayerMask.NameToLayer(LayerMask.LayerToName(10));
            meshRenderer.material = mat;
        }
        
        private static Color GetColorForTrigger(string type)
        {
            switch (type)
            {
                case "distancecamera": return new Color(0.98f, 0.0f, 0.0f, .2f);
                case "distancecheck": return new Color(1f, 0.4f, 0.0f, .2f);
                case "distancecircle": return new Color(0.0f, 0.2f, 0.98f, .2f);
                case "event": return new Color(0.0f, 0.97f, 0.0f, .2f);
                case "mouseclick": return new Color(0.3f, 1f, 1f, .2f);
                case "mouseevent": return new Color(0.9f, 0.9f, 0.0f, .2f);
                case "teleport": return new Color(0.9f, 0.2f, 0.7f, .2f);
                case "zoom": return new Color(0.8f, 0.8f, 0.8f, .2f);
                default: return new Color(1f, 1f, 1f, .2f);
            }
        }

    internal static void HideTriggers()
    {
        _isRevealing = false;
        foreach (GameObject gameObject in GameObjects.Where(gameObject => gameObject != null))
        {
            _isRevealing = false;
            foreach (GameObject gameObject in GameObjects.Where(gameObject => gameObject != null))
            {
                Object.Destroy(gameObject);
            }
            foreach (GameObject gameObject in Labels.Where(gameObject => gameObject != null))
            {
                Object.Destroy(gameObject);
            }
            GameObjects.Clear();
            Labels.Clear();
        }
        GameObjects.Clear();
    }
      
        public static bool IsRevealing()
        {
            return _isRevealing;
        }

        internal static void Update()
        {
            if (_isRevealing)
            {
                foreach (var label in Labels)
                {
                    if (label != null && label.transform != null && Camera.main != null)
                    {
                        label.transform.LookAt(Camera.main.transform);
                    }
                }
            }
        }
    }
}