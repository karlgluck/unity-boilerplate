public static partial class StringExt
{
    public static uint GetJenkinsHash(this string str)
    {
        char[] charArray = str.ToCharArray();
        uint hash = 0, i = 0;
        while (i < charArray.Length)
        {
            hash += charArray[i];
            hash += (hash << 10);
            hash ^= (hash >> 6);
            ++i;
        }
        hash += (hash << 3);
        hash ^= (hash >> 11);
        hash += (hash << 15);
        return hash;
    }
}