using UnityEngine;

public static partial class Vector2Ext
{
    public static Vector3 ToVector3 (this Vector2 self)
    {
        return new Vector3(self.x, self.y, 0f);
    }

    public static Vector3 ToVector3(this Vector2 self, float z)
    {
        return new Vector3(self.x, self.y, z);
    }

    public static Vector3 ToVector3 (this Vector2 self, Vector3 sourceOfZ)
    {
        return new Vector3(self.x, self.y, sourceOfZ.z);
    }

    public static Vector2 Rotate (this Vector2 self, float z)
    {
        z = z * Mathf.Deg2Rad;
        float cosz = Mathf.Cos(z);
        float sinz = Mathf.Sin(z);
        return new Vector2 (self.x * cosz - self.y * sinz, self.y * cosz + self.x * sinz);
    }

    public static Vector2 InDirection (float z)
    {
        z = z * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(z), Mathf.Sin(z));
    }
}