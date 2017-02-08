using UnityEngine;

[System.Serializable]
public struct Vector2i
{
    public int x, y;

    public Vector2i(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public override int GetHashCode()
    {
        var X = (ulong)(x >= 0 ? 2 * (long)x : -2 * (long)x - 1);
        var Y = (ulong)(y >= 0 ? 2 * (long)y : -2 * (long)y - 1);
        var K = (long)((X >= Y ? X * X + X + Y : X + Y * Y) / 2);
        var perfectHash = x < 0 && y < 0 || x >= 0 && y >= 0 ? K : -K - 1;
        return (int)perfectHash;
    }

    public override bool Equals(object other)
    {
        if (other is Vector2i)
        {
            var otherActual = (Vector2i)other;
            return otherActual.x == this.x && otherActual.y == this.y;
        }
        return false;
    }

    public override string ToString()
    {
        return "{" + this.x + "," + this.y + "}";
    }

    public static readonly Vector2i zero = new Vector2i(0,0);
}