using System.IO;
using UnityEditor;
using UnityEngine;

public static class SortingLauncherIconGenerator
{
    private const string IconPath = "Assets/AppIcon/LauncherIcon.png";

    [MenuItem("Tools/Sorting Puzzle/Generate & Apply Launcher Icon")]
    public static void GenerateAndApply()
    {
        string dir = Path.GetDirectoryName(IconPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        Texture2D tex = DrawIcon(1024);
        File.WriteAllBytes(IconPath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        AssetDatabase.Refresh();

        var importer = (TextureImporter)AssetImporter.GetAtPath(IconPath);
        importer.textureType  = TextureImporterType.Default;
        importer.npotScale    = TextureImporterNPOTScale.None;
        importer.mipmapEnabled = false;
        importer.isReadable   = true;
        importer.maxTextureSize = 1024;
        importer.SaveAndReimport();

        var iconTex = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (iconTex == null)
        {
            Debug.LogError("[LauncherIcon] 아이콘 로드 실패.");
            return;
        }

#pragma warning disable CS0618
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new[] { iconTex });
#pragma warning restore CS0618

        AssetDatabase.SaveAssets();
        Debug.Log($"[LauncherIcon] 적용 완료 → {IconPath}");
        EditorUtility.DisplayDialog("런처 아이콘 적용 완료",
            "아이콘이 생성되고 Android Player Settings에 등록됐습니다.\n\n" +
            "Build > Sorting Puzzle > Build Android APK 로 다시 빌드하세요.",
            "확인");
    }


    private static Texture2D DrawIcon(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];

        var top    = new Color(0.22f, 0.08f, 0.52f, 1f);
        var bottom = new Color(0.05f, 0.18f, 0.52f, 1f);
        for (int y = 0; y < size; y++)
        {
            float t = (float)y / (size - 1);
            Color bg = Color.Lerp(bottom, top, t);
            for (int x = 0; x < size; x++)
                pixels[y * size + x] = bg;
        }

        int tileW  = size * 11 / 32;   // ~352 px
        int tileH  = size * 11 / 32;
        int radius = tileW / 7;
        int gapX   = size / 24;
        int gapY   = size / 28;

        int midX  = size / 2;
        int topY  = size * 58 / 100;
        int botY  = size * 35 / 100;
        int leftX = midX - tileW / 2 - gapX / 2;
        int rightX = midX + tileW / 2 + gapX / 2;

        int shadowOfs = size / 80;
        DrawRoundedRect(pixels, size, midX  + shadowOfs, topY - shadowOfs, tileW, tileH, radius, new Color(0,0,0,0.35f));
        DrawRoundedRect(pixels, size, leftX + shadowOfs, botY - shadowOfs, tileW, tileH, radius, new Color(0,0,0,0.35f));
        DrawRoundedRect(pixels, size, rightX + shadowOfs, botY - shadowOfs, tileW, tileH, radius, new Color(0,0,0,0.35f));

        DrawRoundedRect(pixels, size, midX,   topY, tileW, tileH, radius, new Color(1.00f, 0.60f, 0.10f, 1f));
        DrawRoundedRect(pixels, size, leftX,  botY, tileW, tileH, radius, new Color(0.18f, 0.82f, 0.42f, 1f));
        DrawRoundedRect(pixels, size, rightX, botY, tileW, tileH, radius, new Color(0.35f, 0.65f, 1.00f, 1f));

        int hlH = tileH / 5;
        DrawRoundedRect(pixels, size, midX,   topY + (tileH / 2) - hlH / 2, tileW - radius, hlH, radius / 2, new Color(1,1,1,0.22f));
        DrawRoundedRect(pixels, size, leftX,  botY + (tileH / 2) - hlH / 2, tileW - radius, hlH, radius / 2, new Color(1,1,1,0.22f));
        DrawRoundedRect(pixels, size, rightX, botY + (tileH / 2) - hlH / 2, tileW - radius, hlH, radius / 2, new Color(1,1,1,0.22f));

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private static void DrawRoundedRect(Color[] pixels, int texSize,
        int cx, int cy, int w, int h, int r, Color col)
    {
        int x0 = cx - w / 2;
        int y0 = cy - h / 2;
        int x1 = x0 + w;
        int y1 = y0 + h;

        for (int py = y0; py < y1; py++)
        for (int px = x0; px < x1; px++)
        {
            if (px < 0 || px >= texSize || py < 0 || py >= texSize) continue;

            int dx = 0, dy = 0;
            if      (px < x0 + r) dx = px - (x0 + r);
            else if (px > x1 - r) dx = px - (x1 - r);
            if      (py < y0 + r) dy = py - (y0 + r);
            else if (py > y1 - r) dy = py - (y1 - r);
            if (dx * dx + dy * dy > r * r) continue;

            int idx = py * texSize + px;
            if (col.a >= 1f)
            {
                pixels[idx] = col;
            }
            else
            {
                // alpha blend
                pixels[idx] = Color.Lerp(pixels[idx], col, col.a);
            }
        }
    }
}
