using UnityEngine;
using System.Collections;

public static partial class GameObjectExt
{
    public static void DestroyChildrenByName (GameObject gameObject, string name)
    {
        var transform = gameObject.transform;
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            var child = transform.GetChild(i);
            if (child.name.Equals (name))
            {
                GameObject.Destroy(child.gameObject);
            }
        }
    }
}