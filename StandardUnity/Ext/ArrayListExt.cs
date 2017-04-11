using UnityEngine;
using System.Collections;

public static partial class ArrayListExt
{
    public static object Last (this ArrayList self)
    {
        return self.Count > 0 ? self[self.Count - 1] : null;
    }

    public static T Last<T> (this ArrayList self)
    {
        return (T)self.Last ();
    }

    public static T SafeIndex<T> (this ArrayList self, int i)
    {
        return (T)self.SafeIndex(i);
    }

    public static object SafeIndex (this ArrayList self, int i)
    {
        if (self != null)
        {
            int max = self.Count - 1;
            if (max >= 0)
            {
                return self[i < 0 ? 0 : (i > max ? max : i)];
            }
        }
        return null;
    }
    
}