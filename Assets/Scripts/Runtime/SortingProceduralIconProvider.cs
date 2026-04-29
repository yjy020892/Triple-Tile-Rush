using System.Collections.Generic;
using UnityEngine;

// 모든 아이템 타입에 대해 둥글둥글하고 귀여운 폴백 아이콘을 절차적으로 생성.
// 실제 PNG 일러스트를 Resources/TileIcons/<TypeName>.png 에 두면 그게 우선 적용된다.
//
// 디자인 원칙
//   - Body: 메인 컬러 둥근 사각(쿠션) — 부드러운 안티에일리어싱
//   - Highlight: 좌상단 흰 광택
//   - 테마별 액센트: 동물 귀, 벌레 더듬이, 식물 잎, 음식 잎, 차량 바퀴 등
//   - "살아있는" 테마(동물/벌레/판타지)는 작은 눈 두 개 추가
public static class SortingProceduralIconProvider
{
    private const int Size = 96;
    private static readonly Dictionary<SortingItemType, Sprite> Cache = new Dictionary<SortingItemType, Sprite>();

    public static Sprite Get(SortingItemType type)
    {
        if (type == SortingItemType.None)
        {
            return null;
        }

        if (Cache.TryGetValue(type, out Sprite sprite))
        {
            return sprite;
        }

        SortingItemDef def = SortingItemCatalog.Get(type);
        if (def == null)
        {
            return null;
        }

        Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] buffer = new Color[Size * Size];
        for (int i = 0; i < buffer.Length; i++) buffer[i] = new Color(0f, 0f, 0f, 0f);
        texture.SetPixels(buffer);

        DrawIcon(texture, def);
        texture.Apply();

        sprite = Sprite.Create(texture, new Rect(0f, 0f, Size, Size), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = $"ProcIcon_{type}";
        Cache[type] = sprite;
        return sprite;
    }

    private static void DrawIcon(Texture2D tex, SortingItemDef def)
    {
        // 1) 메인 둥근 쿠션 본체
        Color body = def.mainColor;
        Color bodyShade = MultiplyColor(body, 0.82f);
        DrawCushion(tex, 14, 14, Size - 28, Size - 28, 22, body, bodyShade);

        // 2) 좌상단 광택
        DrawSoftEllipse(tex, 30, 64, 16, 8, new Color(1f, 1f, 1f, 0.55f));

        // 3) 테마별 액센트
        switch (def.theme)
        {
            case SortingTheme.Animal:
                DrawTriangle(tex, 22, 76, 12, 12, def.accentColor);   // 왼쪽 귀
                DrawTriangle(tex, 62, 76, 12, 12, def.accentColor);   // 오른쪽 귀
                DrawCutEyes(tex);
                break;
            case SortingTheme.Bug:
                DrawAntenna(tex, 28, 72, 10, 14, def.accentColor);
                DrawAntenna(tex, 60, 72, 10, 14, def.accentColor);
                DrawCutEyes(tex);
                break;
            case SortingTheme.Fantasy:
                DrawSparkle(tex, 70, 70, 6, def.accentColor);
                DrawSparkle(tex, 18, 24, 4, def.accentColor);
                DrawCutEyes(tex);
                break;
            case SortingTheme.Food:
                DrawLeaf(tex, 48, 76, 14, 8, def.accentColor);
                break;
            case SortingTheme.Sweet:
                DrawDrip(tex, 30, 18, 36, 14, def.accentColor);
                break;
            case SortingTheme.Plant:
                DrawLeaf(tex, 30, 78, 14, 8, def.accentColor);
                DrawLeaf(tex, 60, 70, 12, 7, def.accentColor);
                break;
            case SortingTheme.Vehicle:
                FillCircle(tex, 26, 16, 8, def.accentColor);
                FillCircle(tex, 70, 16, 8, def.accentColor);
                FillRectAA(tex, 32, 50, 32, 14, new Color(1f, 1f, 1f, 0.55f));
                break;
            case SortingTheme.Weather:
                DrawSparkle(tex, 24, 70, 5, def.accentColor);
                DrawSparkle(tex, 70, 24, 5, def.accentColor);
                break;
            case SortingTheme.Tool:
                FillRectAA(tex, 42, 18, 12, 36, def.accentColor);
                break;
        }

        // 4) 외곽 살짝 더 진한 1px 선
        DrawCushionOutline(tex, 14, 14, Size - 28, Size - 28, 22, MultiplyColor(body, 0.55f));
    }

    // ── 그리기 도우미 ─────────────────────────────────────────────────────

    // 부드러운 쿠션(라운드 직사각형) — 위 밝게, 아래 약간 어둡게 그라데이션
    private static void DrawCushion(Texture2D tex, int x, int y, int w, int h, int radius, Color top, Color bottom)
    {
        int right = x + w - 1;
        int topY = y + h - 1;
        for (int py = y; py <= topY; py++)
        {
            float ty = (py - y) / (float)(h - 1);
            Color row = Color.Lerp(bottom, top, Mathf.SmoothStep(0f, 1f, ty));
            for (int px = x; px <= right; px++)
            {
                float a = ComputeRoundedAlpha(px - x, py - y, w, h, radius);
                if (a <= 0f) continue;
                BlendPixel(tex, px, py, new Color(row.r, row.g, row.b, row.a * a));
            }
        }
    }

    private static void DrawCushionOutline(Texture2D tex, int x, int y, int w, int h, int radius, Color color)
    {
        int right = x + w - 1;
        int topY = y + h - 1;
        for (int py = y; py <= topY; py++)
        {
            for (int px = x; px <= right; px++)
            {
                float a0 = ComputeRoundedAlpha(px - x, py - y, w, h, radius);
                float a1 = 0f;
                if (px > x && px < right && py > y && py < topY)
                {
                    a1 = ComputeRoundedAlpha(px - x, py - y, w, h, radius - 1);
                }
                float edge = Mathf.Clamp01(a0 - a1) * 0.7f;
                if (edge <= 0f) continue;
                BlendPixel(tex, px, py, new Color(color.r, color.g, color.b, color.a * edge));
            }
        }
    }

    private static float ComputeRoundedAlpha(int x, int y, int w, int h, int r)
    {
        if (r <= 0) return 1f;
        bool inCornerX = x < r || x >= w - r;
        bool inCornerY = y < r || y >= h - r;
        if (!inCornerX || !inCornerY) return 1f;

        int cx = x < r ? r : w - r - 1;
        int cy = y < r ? r : h - r - 1;
        float dx = x - cx;
        float dy = y - cy;
        float dist = Mathf.Sqrt(dx * dx + dy * dy);
        return Mathf.Clamp01(r - dist + 0.5f);
    }

    private static void DrawSoftEllipse(Texture2D tex, int cx, int cy, int rx, int ry, Color color)
    {
        for (int py = cy - ry; py <= cy + ry; py++)
        {
            for (int px = cx - rx; px <= cx + rx; px++)
            {
                float dx = (px - cx) / (float)rx;
                float dy = (py - cy) / (float)ry;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                if (d > 1f) continue;
                float a = (1f - d) * color.a;
                BlendPixel(tex, px, py, new Color(color.r, color.g, color.b, a));
            }
        }
    }

    private static void FillCircle(Texture2D tex, int cx, int cy, int radius, Color color)
    {
        for (int py = cy - radius; py <= cy + radius; py++)
        {
            for (int px = cx - radius; px <= cx + radius; px++)
            {
                float d = Mathf.Sqrt((px - cx) * (px - cx) + (py - cy) * (py - cy));
                float a = Mathf.Clamp01(radius - d + 0.5f) * color.a;
                if (a <= 0f) continue;
                BlendPixel(tex, px, py, new Color(color.r, color.g, color.b, a));
            }
        }
    }

    private static void FillRectAA(Texture2D tex, int x, int y, int w, int h, Color color)
    {
        for (int py = y; py < y + h; py++)
            for (int px = x; px < x + w; px++)
                BlendPixel(tex, px, py, color);
    }

    private static void DrawTriangle(Texture2D tex, int cx, int cyBottom, int width, int height, Color color)
    {
        for (int py = 0; py < height; py++)
        {
            float t = py / (float)(height - 1); // 0=base, 1=tip
            int halfW = Mathf.RoundToInt((1f - t) * width * 0.5f);
            for (int px = -halfW; px <= halfW; px++)
            {
                BlendPixel(tex, cx + px, cyBottom + py, color);
            }
        }
    }

    private static void DrawAntenna(Texture2D tex, int cxBase, int cyBase, int width, int height, Color color)
    {
        int dir = cxBase < Size / 2 ? -1 : 1;
        for (int i = 0; i < height; i++)
        {
            int x = cxBase + dir * (i / 3);
            BlendPixel(tex, x, cyBase + i, color);
            BlendPixel(tex, x + dir, cyBase + i, new Color(color.r, color.g, color.b, color.a * 0.6f));
        }
        FillCircle(tex, cxBase + dir * (height / 3), cyBase + height, Mathf.Max(2, width / 4), color);
    }

    private static void DrawSparkle(Texture2D tex, int cx, int cy, int size, Color color)
    {
        for (int i = -size; i <= size; i++)
        {
            float a = 1f - Mathf.Abs(i) / (float)size;
            BlendPixel(tex, cx + i, cy, new Color(color.r, color.g, color.b, a));
            BlendPixel(tex, cx, cy + i, new Color(color.r, color.g, color.b, a));
        }
    }

    private static void DrawLeaf(Texture2D tex, int cx, int cy, int width, int height, Color color)
    {
        for (int py = 0; py <= height; py++)
        {
            int half = Mathf.RoundToInt(width * 0.5f * (1f - Mathf.Abs((py / (float)height) - 0.5f) * 2f));
            for (int px = -half; px <= half; px++)
            {
                BlendPixel(tex, cx + px, cy + py, color);
            }
        }
    }

    private static void DrawDrip(Texture2D tex, int x, int y, int w, int h, Color color)
    {
        for (int py = 0; py < h; py++)
        {
            float t = py / (float)(h - 1);
            int sineOffset = Mathf.RoundToInt(Mathf.Sin(t * Mathf.PI * 2f) * 3f);
            for (int px = 0; px < w; px++)
            {
                if (py < 2 || (px + sineOffset) % 6 < 4)
                {
                    BlendPixel(tex, x + px, y + py, color);
                }
            }
        }
    }

    private static void DrawCutEyes(Texture2D tex)
    {
        FillCircle(tex, Size / 2 - 10, Size / 2 + 4, 3, new Color(0.10f, 0.08f, 0.10f, 1f));
        FillCircle(tex, Size / 2 + 10, Size / 2 + 4, 3, new Color(0.10f, 0.08f, 0.10f, 1f));
        // 작은 하이라이트
        FillCircle(tex, Size / 2 - 9, Size / 2 + 5, 1, new Color(1f, 1f, 1f, 0.95f));
        FillCircle(tex, Size / 2 + 11, Size / 2 + 5, 1, new Color(1f, 1f, 1f, 0.95f));
    }

    // ── 픽셀 유틸 ─────────────────────────────────────────────────────────

    private static void BlendPixel(Texture2D tex, int x, int y, Color color)
    {
        if (x < 0 || y < 0 || x >= Size || y >= Size) return;
        if (color.a <= 0f) return;

        Color current = tex.GetPixel(x, y);
        float outA = color.a + current.a * (1f - color.a);
        if (outA <= 0f) return;
        float r = (color.r * color.a + current.r * current.a * (1f - color.a)) / outA;
        float g = (color.g * color.a + current.g * current.a * (1f - color.a)) / outA;
        float b = (color.b * color.a + current.b * current.a * (1f - color.a)) / outA;
        tex.SetPixel(x, y, new Color(r, g, b, outA));
    }

    private static Color MultiplyColor(Color c, float factor)
    {
        return new Color(c.r * factor, c.g * factor, c.b * factor, c.a);
    }
}
