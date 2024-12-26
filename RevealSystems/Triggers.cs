using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpeedrunMod.RevealSystems
{
    internal class Triggers
    {
        private static List<GameObject> gameObjects = new List<GameObject>();
        private static bool isRevealing = false;

        public static void RevealTriggers()
        {
            HideTriggers();
            isRevealing = true;
            Trigger_DistanceCamera[] objectsDC = UnityEngine.Object.FindObjectsByType<Trigger_DistanceCamera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Trigger_DistanceCamera obj in objectsDC)
            {
                GameObject gameObject = obj.gameObject;
                AddTriggerRevealer(gameObject, "distancecamera");
            }

            Trigger_DistanceCheck[] objectsDCH = UnityEngine.Object.FindObjectsByType<Trigger_DistanceCheck>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Trigger_DistanceCheck obj in objectsDCH)
            {
                GameObject gameObject = obj.gameObject;
                AddTriggerRevealer(gameObject, "distancecheck");
            }

            Trigger_DistanceCircle[] objectsDCI = UnityEngine.Object.FindObjectsByType<Trigger_DistanceCircle>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Trigger_DistanceCircle obj in objectsDCI)
            {
                GameObject gameObject = obj.gameObject;
                AddTriggerRevealer(gameObject, "distancecircle");
            }

            Trigger_Event[] objectsE = UnityEngine.Object.FindObjectsByType<Trigger_Event>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Trigger_Event obj in objectsE)
            {
                GameObject gameObject = obj.gameObject;
                AddTriggerRevealer(gameObject, "event");
            }

            Trigger_MouseClick[] objectsMC = UnityEngine.Object.FindObjectsByType<Trigger_MouseClick>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Trigger_MouseClick obj in objectsMC)
            {
                GameObject gameObject = obj.gameObject;
                AddTriggerRevealer(gameObject, "mouseclick");
            }

            Trigger_MouseEvent[] objectsME = UnityEngine.Object.FindObjectsByType<Trigger_MouseEvent>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Trigger_MouseEvent obj in objectsME)
            {
                GameObject gameObject = obj.gameObject;
                AddTriggerRevealer(gameObject, "mouseevent");
            }

            Trigger_Teleport[] objectsTP = UnityEngine.Object.FindObjectsByType<Trigger_Teleport>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Trigger_Teleport obj in objectsTP)
            {
                GameObject gameObject = obj.gameObject;
                AddTriggerRevealer(gameObject, "teleport");
            }

            Trigger_Zoom[] objectsZ = UnityEngine.Object.FindObjectsByType<Trigger_Zoom>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Trigger_Zoom obj in objectsZ)
            {
                GameObject gameObject = obj.gameObject;
                AddTriggerRevealer(gameObject, "zoom");
            }
        }

        // TODO: Type should become an enum
        private static void AddTriggerRevealer(GameObject gameObject, string type)
        {
            GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObject.transform.position = gameObject.transform.position;
            newObject.transform.rotation = gameObject.transform.rotation; ;
            newObject.transform.parent = gameObject.transform;
            newObject.transform.localScale = new Vector3(1, 1, 1);

            newObject.name = "RevealBox" + gameObject.name;

            newObject.GetComponent<BoxCollider>().enabled = false;

            MeshRenderer meshRenderer = newObject.GetComponent<MeshRenderer>();

            Material mat = new Material(Shader.Find("Standard"));
            switch (type)
            {
                case "distancecamera":
                    mat.SetColor("_Color", new UnityEngine.Color(0.98f, 0.48f, 0.46f, .5f)); break;
                case "distancecheck":
                    mat.SetColor("_Color", new UnityEngine.Color(0.99f, 0.75f, 0.44f, .5f)); break;
                case "distancecircle":
                    mat.SetColor("_Color", new UnityEngine.Color(0.95f, 0.97f, 0.49f, .5f)); break;
                case "event":
                    mat.SetColor("_Color", new UnityEngine.Color(0.59f, 0.97f, 0.52f, .5f)); break;
                case "mouseclick":
                    mat.SetColor("_Color", new UnityEngine.Color(0.41f, 0.92f, 0.98f, .5f)); break;
                case "mouseevent":
                    mat.SetColor("_Color", new UnityEngine.Color(0.42f, 0.61f, 0.98f, .5f)); break;
                case "teleport":
                    mat.SetColor("_Color", new UnityEngine.Color(0.57f, 0.49f, 0.97f, .5f)); break;
                case "zoom":
                    mat.SetColor("_Color", new UnityEngine.Color(0.97f, 0.55f, 0.94f, .5f)); break;
            }
            mat.SetColor("_Color", new UnityEngine.Color(1, 0, 0, .5f));
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;

            UnityEngine.Color color = mat.color;
            color.a = .5f;
            switch (type)
            {
                case "distancecamera":
                    color.r = 0.98f;
                    color.g = 0.48f;
                    color.b = 0.46f;
                    break;
                case "distancecheck":
                    color.r = 0.99f;
                    color.g = 0.75f;
                    color.b = 0.44f;
                    break;
                case "distancecircle":
                    color.r = 0.95f;
                    color.g = 0.97f;
                    color.b = 0.49f;
                    break;
                case "event":
                    color.r = 0.59f;
                    color.g = 0.97f;
                    color.b = 0.52f;
                    break;
                case "mouseclick":
                    color.r = 0.41f;
                    color.g = 0.92f;
                    color.b = 0.98f;
                    break;
                case "mouseevent":
                    color.r = 0.42f;
                    color.g = 0.61f;
                    color.b = 0.98f;
                    break;
                case "teleport":
                    color.r = 0.57f;
                    color.g = 0.49f;
                    color.b = 0.97f;
                    break;
                case "zoom":
                    color.r = 0.97f;
                    color.g = 0.55f;
                    color.b = 0.94f;
                    break;
            }
            mat.color = color;

            meshRenderer.material = mat;

            gameObjects.Add(newObject);
        }

        public static void HideTriggers()
        {
            isRevealing = false;
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject == null) continue;
                UnityEngine.Object.Destroy(gameObject);
            }
            gameObjects.Clear();
        }

        public static bool IsRevealing()
        {
            return isRevealing;
        }
    }
}
