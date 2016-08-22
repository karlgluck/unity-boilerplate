using UnityEngine;

public class BytesBlob
{
    public byte[] Bytes = null;

    public string GetBytesAsString()
    {
        return System.Text.Encoding.UTF8.GetString(this.Bytes);
    }

    public Texture2D GetBytesAsTexture()
    {
        var texture = new Texture2D(2, 2);
        texture.LoadImage(this.Bytes, false);
        return texture;
    }
}