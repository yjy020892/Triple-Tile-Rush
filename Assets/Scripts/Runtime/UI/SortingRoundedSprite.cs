using System.Collections.Generic;
using UnityEngine;

public static class SortingRoundedSprite
{
    private const int RoundedSize = 128;
    private const int CircleSize = 256;

    private static readonly Dictionary<int, Sprite> roundedCache = new Dictionary<int, Sprite>();
    private static Sprite circleSprite;

    public static Sprite GetRounded(int cornerRadius)
    {
        int r = Mathf.Clamp(cornerRadius, 1, RoundedSize / 2 - 2);
        if (roundedCache.TryGetValue(r, out Sprite cached) && cached != null)
        {
            return cached;
        }

        Texture2D tex = new Texture2D(RoundedSize, RoundedSize, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[RoundedSize * RoundedSize];
        for (int y = 0; y < RoundedSize; y++)
        {
            for (int x = 0; x < RoundedSize; x++)
            {
                float a = ComputeRoundedAlpha(x, y, RoundedSize, RoundedSize, r);
                pixels[y * RoundedSize + x] = new Color(1f, 1f, 1f, a);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();

        int border = r + 2;
        Sprite sprite = Sprite.Create(
            tex,
            new Rect(0f, 0f, RoundedSize, RoundedSize),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(border, border, border, border));

        roundedCache[r] = sprite;
        return sprite;
    }

    public static Sprite GetCircle()
    {
        if (circleSprite != null)
        {
            return circleSprite;
        }

        Texture2D tex = new Texture2D(CircleSize, CircleSize, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[CircleSize * CircleSize];
        float cx = (CircleSize - 1) * 0.5f;
        float cy = (CircleSize - 1) * 0.5f;
        float maxR = CircleSize * 0.5f;

        for (int y = 0; y < CircleSize; y++)
        {
            for (int x = 0; x < CircleSize; x++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(maxR - dist + 0.5f);
                pixels[y * CircleSize + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();

        circleSprite = Sprite.Create(
            tex,
            new Rect(0f, 0f, CircleSize, CircleSize),
            new Vector2(0.5f, 0.5f),
            100f);
        return circleSprite;
    }

    private static float ComputeRoundedAlpha(int x, int y, int w, int h, int r)
    {
        int cx;
        int cy;
        bool inCorner = false;

        if (x < r)
        {
            cx = r;
            inCorner = true;
        }
        else if (x >= w - r)
        {
            cx = w - r - 1;
            inCorner = true;
        }
        else
        {
            cx = x;
        }

        if (y < r)
        {
            cy = r;
            inCorner = true;
        }
        else if (y >= h - r)
        {
            cy = h - r - 1;
            inCorner = true;
        }
        else
        {
            cy = y;
        }

        if (!inCorner)
        {
            return 1f;
        }

        float dx = x - cx;
        float dy = y - cy;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);
        return Mathf.Clamp01(r - dist + 0.5f);
    }
}
