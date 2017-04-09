using UnityEngine;
using System.Collections;

public static partial class ArrayExt
{
    public static T Last (this T[] self)
    {
        return self.Length > 0 ? self[self.Length - 1] : default(T);
    }

    public static T SafeIndex<T>(this T[] self, int i)
    {
        if (self != null)
        {
            int max = self.Length - 1;
            if (max >= 0)
            {
                return array[i < 0 ? 0 : (i > max ? max : i)];
            }
        }
        return default(T);
    }
}