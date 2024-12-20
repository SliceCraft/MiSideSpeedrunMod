﻿using System;
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
            GameObject[] objects = UnityEngine.Object.FindObjectsOfType<GameObject>(true);
            foreach (GameObject obj in objects)
            {
                if (obj.name.StartsWith("Trigger"))
                {
                    GameObject newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    newObject.transform.position = obj.transform.position;
                    newObject.transform.rotation = obj.transform.rotation;;
                    newObject.transform.parent = obj.transform;
                    newObject.transform.localScale = new Vector3(1, 1, 1);

                    newObject.name = "RevealBox" + obj.name;

                    newObject.GetComponent<BoxCollider>().enabled = false;

                    MeshRenderer meshRenderer = newObject.GetComponent<MeshRenderer>();

                    Material mat = new Material(Shader.Find("Standard"));
                    mat.SetColor("_Color", new UnityEngine.Color(1, 0, 0, .5f));
                    mat.SetFloat("_Mode", 3);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.renderQueue = 3000;

                    UnityEngine.Color color = mat.color;
                    color.a = .5f;
                    color.r = 1;
                    color.g = 0;
                    color.b = 0;
                    mat.color = color;

                    meshRenderer.material = mat;

                    gameObjects.Add(newObject);
                }
            }
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
