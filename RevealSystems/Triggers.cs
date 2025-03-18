using System.Collections.Generic;
using System.Linq;
using SpeedrunMod.EventDisplay;
using UnityEngine;

namespace SpeedrunMod.RevealSystems
{
    //First commit has all the comments, will be deleted with subsequent commits.
    internal static class Triggers
    {
        private static readonly List<GameObject> GameObjects = new List<GameObject>();
        private static bool _isRevealing;
        private static readonly int Color = Shader.PropertyToID("_Color");
        private static readonly int Mode = Shader.PropertyToID("_Mode");
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");

        internal static void RevealTriggers()
        {
            EventManager.ShowEvent(new ModEvent("Revealing Triggers"));
            HideTriggers();
            _isRevealing = true;

            // Process each type of trigger
            ProcessTriggers<Trigger_DistanceCamera>("distancecamera");
            ProcessTriggers<Trigger_DistanceCheck>("distancecheck");
            ProcessTriggers<Trigger_DistanceCircle>("distancecircle");
            ProcessTriggers<Trigger_Event>("event");
            ProcessTriggers<Trigger_MouseClick>("mouseclick");
            ProcessTriggers<Trigger_MouseEvent>("mouseevent");
            ProcessTriggers<Trigger_Teleport>("teleport");
            ProcessTriggers<Trigger_Zoom>("zoom");
        }

        // General method to process triggers based on type, will be changed to handle an enumerable instead.
        private static void ProcessTriggers<T>(string type) where T : Component
        {
            var objects = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var obj in objects)
            {
                GameObject gameObject = obj.gameObject;
                AddTriggerRevealer(gameObject, type);
            }
        }
        
        //Separated into different methods to keep everything tidy and readable.
        private static void AddTriggerRevealer(GameObject gameObject, string type)
        {
            GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObject.transform.SetParent(gameObject.transform, false);
            newObject.name = "RevealBox" + gameObject.name;
            //Currently working on bringing these to life.
            //GameObject canvasGUI = new GameObject("Canvas " + gameObject.name);
            //GameObject textGUI = new GameObject("Text " + gameObject.name);
            //Plugin.Log.LogInfo("Creating canvas and text");

            // Set up material for the new object
            SetMaterialForObject(newObject, type);

            // Disable collider and store in list
            newObject.GetComponent<BoxCollider>().enabled = false;
            GameObjects.Add(newObject);
        }
        
        private static void SetMaterialForObject(GameObject newObject, string type)
        {
            MeshRenderer meshRenderer = newObject.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));

            switch (type)
            {
                case "distancecamera":
                    mat.SetColor(Color, new Color(0.98f, 0.0f, 0.0f, .2f)); break;
                case "distancecheck":
                    mat.SetColor(Color, new Color(1f, 0.4f, 0.0f, .2f)); break;
                case "distancecircle":
                    mat.SetColor(Color, new Color(0.0f, 0.2f, 0.98f, .2f)); break;
                case "event":
                    mat.SetColor(Color, new Color(0.0f, 0.97f, 0.0f, .2f)); break;
                case "mouseclick":
                    mat.SetColor(Color, new Color(0.3f, 1f, 1f, .2f)); break;
                case "mouseevent":
                    mat.SetColor(Color, new Color(0.99f, 0.9f, 0.0f, .2f)); break;
                case "teleport":
                    mat.SetColor(Color, new Color(0.9f, 0.2f, 0.7f, .2f)); break;
                case "zoom":
                    mat.SetColor(Color, new Color(0.8f, 0.8f, 0.8f, .2f)); break;
            }
            mat.SetColor("_EmissionColor", mat.color);
            mat.EnableKeyword("_EMISSION");
            
            mat.SetFloat(Mode, 3);
            mat.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;

            meshRenderer.material = mat;
        }

        internal static void HideTriggers()
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
}
