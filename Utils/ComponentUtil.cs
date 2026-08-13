using UnityEngine;

namespace SpeedrunMod.Utils;

internal static class ComponentUtil
{
    internal static GameObject FindIncludingInactive(string name)
    {
        foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t != null && t.gameObject.name == name)
            {
                return t.gameObject;
            }
        }

        return null;
    }

    internal static T FindIncludingInactive<T>(string name) where T : Component
    {
        GameObject go = FindIncludingInactive(name);
        return go != null ? go.GetComponent<T>() : null;
    }

    internal static void Enable(string name, bool enabled)
    {
        GameObject go = FindIncludingInactive(name);
        if (go != null && go.activeSelf != enabled)
        {
            go.SetActive(enabled);
        }
    }
}
