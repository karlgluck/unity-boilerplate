using UnityEngine;

public static partial class Util
{
    static readonly char[] base62Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    public static string ToBase62String(ulong number)
    {
        var n = number;
        ulong basis = 62;
        var ret = "";
        while (n > 0)
        {
            var temp = n % basis;
            ret = base62Alphabet[(int)temp] + ret;
            n = (n / basis);

        }
        return ret;
    }
}