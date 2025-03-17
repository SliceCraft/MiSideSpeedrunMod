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
        
        //Since this will handle the color of the material, Im going to change it to use an enumerable instead.
        //The text will also get its color from here.
        //The material will be changed from the standard material to the unlit material to save on resources and to avoid them being hidden during dark scenes (like in "The Loop")
        private static void SetMaterialForObject(GameObject newObject, string type)
        {
            MeshRenderer meshRenderer = newObject.GetComponent<MeshRenderer>();
            Material mat = new Material(Shader.Find("Standard"));

            switch (type)
            {
                case "distancecamera":
                    mat.SetColor(Color, new Color(0.98f, 0.0f, 0.0f, .5f)); break; //When is this ever used? 
                case "distancecheck":
                    mat.SetColor(Color, new Color(1f, 0.4f, 0.0f, .5f)); break; //Wonder why have 3 similar types of triggers.
                case "distancecircle":
                    mat.SetColor(Color, new Color(0.0f, 0.2f, 0.98f, .5f)); break;//Since this is a "circle", would it be better to render it as a different primitive?
                case "event":
                    mat.SetColor(Color, new Color(0.0f, 0.97f, 0.0f, .5f)); break; //Green being distinctive is the best, as this is the most common trigger type.
                case "mouseclick":
                    mat.SetColor(Color, new Color(0.3f, 1f, 1f, .5f)); break; //The most problematic trigger, it clutters the screen during the beggining of the game. Do minigames afterwards use a different type of trigger?
                case "mouseevent":
                    mat.SetColor(Color, new Color(0.99f, 0.9f, 0.0f, .5f)); break; //From my testing, this name is deceptive, it check if the camara is looking at it, used in tandem with a distance check
                case "teleport":
                    mat.SetColor(Color, new Color(0.9f, 0.2f, 0.7f, .5f)); break; //Despite my best efforts, this still looks blueish.
                case "zoom":
                    mat.SetColor(Color, new Color(0.8f, 0.8f, 0.8f, .5f)); break; //Not the best, but works well enough - it either shows shadowy or darkens any color as it overlays.
            }
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
