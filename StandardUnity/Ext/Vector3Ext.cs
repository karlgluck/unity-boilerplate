using UnityEngine;
using System.Collections;

public static partial class Vector3Ext
{
    public static float GetMagnitudeXY(this Vector3 self)
    {
        return Mathf.Sqrt (self.x * self.x + self.y * self.y);
    }

    public static Vector2 ToVector2(this Vector3 self)
    {
        return new Vector2 (self.x, self.y);
    }
}
