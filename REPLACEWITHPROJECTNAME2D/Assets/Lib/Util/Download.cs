using UnityEngine;
using System.Collections;
using System.IO;

public static partial class Util
{
    private enum DownloadCacheUsage
    {
        DownloadAlways,
        DownloadOrReadCache,
        ReadCacheOrDownload,
    }

    public static IEnumerator DownloadAsync (string url, BytesBlob result)
    {
        return downloadImplementation(url, DownloadCacheUsage.DownloadAlways, result);
    }

    public static IEnumerator DownloadOrReadCacheAsync(string url, BytesBlob result)
    {
        return downloadImplementation(url, DownloadCacheUsage.DownloadOrReadCache, result);
    }

    public static IEnumerator ReadCacheOrDownloadAsync (string url, BytesBlob result)
    {
        return downloadImplementation(url, DownloadCacheUsage.ReadCacheOrDownload, result);
    }

    private static IEnumerator downloadImplementation (string url, DownloadCacheUsage cacheUsage, BytesBlob result)
    {
        var filename = Application.persistentDataPath + "/dl" + Util.ToBase62String(url.GetJenkinsHash());
        bool existsOnDisk = File.Exists(filename);
        if (existsOnDisk && cacheUsage == DownloadCacheUsage.ReadCacheOrDownload)
        {
            yield return null;
            result.Bytes = File.ReadAllBytes(filename);
            yield break;
        }

        {
            var www = new WWW(url);
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                if (existsOnDisk && cacheUsage == DownloadCacheUsage.DownloadOrReadCache)
                {
                    result.Bytes = File.ReadAllBytes(filename);
                }
                yield break;
            }
            result.Bytes = www.bytes;
            try
            {
                File.WriteAllBytes (filename, result.Bytes);
            }
            catch (IOException)
            {
            }
        }
    }

    private static IEnumerator DownloadSpriteAsync (string url, object objectWithSpriteField, string spriteFieldToSet)
    {
        BytesBlob data = new BytesBlob();
        {
            var enumerator = Util.ReadCacheOrDownloadAsync(url, data);
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
        Sprite sprite = null;
        if (null != data.Bytes)
        {
            var texture = data.GetBytesAsTexture();
            sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        var fieldInfo = objectWithSpriteField.GetType().GetField(spriteFieldToSet);
        fieldInfo.SetValue(objectWithSpriteField, sprite);
    }
}
