using UnityEngine;

public static partial class Util
{
    public static float EaseElasticOut(float time, float start, float end, float duration)
    {
        if ((time /= duration) == 1)
            return start + end;

        float p = duration * .3f;
        float s = p / 4;

        return (end * Mathf.Pow(2, -10 * time) * Mathf.Sin((time * duration - s) * (2 * Mathf.PI) / p) + end + start);
    }

    public static float EaseElasticOut2(float t, float amplitude, float period)
    {
        var s = Mathf.Asin(1 / (amplitude = Mathf.Max(1, amplitude))) * (period /= (2 * Mathf.PI));

        return 1 - amplitude * Mathf.Pow(2, -10 * (t)) * Mathf.Sin((t + s) / period);
    }

    public static float EaseJump(float t)
    {
        return 4 * t * (1 - t);
    }

    public static float EaseJumpBounce(float t)
    {
        // a + 2a + 4a == 1
        // 1 / 7
        float s = 1f;
        if (0 <= t && t < 4 / 8f)
        {
            t = Mathf.InverseLerp(0, 4 / 8f, t);
        }
        else if (t <= 6 / 8f)
        {
            s = 0.4f;
            t = Mathf.InverseLerp(4 / 8f, 6 / 8f, t);
        }
        else
        {
            s = 0.4f * 0.4f;
            t = Mathf.InverseLerp(6 / 8f, 1f, t);
        }
        return s * 4 * t * (1 - t);
    }
}