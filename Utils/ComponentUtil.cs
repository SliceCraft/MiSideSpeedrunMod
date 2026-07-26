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
}
