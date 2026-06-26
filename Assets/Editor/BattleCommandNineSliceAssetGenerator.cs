using System.IO;
using UnityEditor;
using UnityEngine;

public static class BattleCommandNineSliceAssetGenerator
{
    private const string Folder = "Assets/_Generated/UI/BattleCommand";

    [MenuItem("Tools/NeonCardia/Generate Battle Command 9-Slice UI")]
    public static void Generate()
    {
        EnsureFolder(Folder);

        GenerateRow("BC_CardRow_Normal.png", new Color(0.20f, 0.82f, 1.00f, 1f), false, true);
        GenerateRow("BC_CardRow_Selected.png", new Color(0.36f, 0.92f, 1.00f, 1f), true, true);
        GenerateRow("BC_Skills_Normal.png", new Color(0.26f, 1.00f, 0.36f, 1f), false, false);
        GenerateRow("BC_Skills_Selected.png", new Color(0.44f, 1.00f, 0.52f, 1f), true, false);
        GenerateRow("BC_Change_Normal.png", new Color(1.00f, 0.66f, 0.10f, 1f), false, false);
        GenerateRow("BC_Change_Selected.png", new Color(1.00f, 0.78f, 0.22f, 1f), true, false);
        GenerateRow("BC_Run_Normal.png", new Color(1.00f, 0.18f, 0.58f, 1f), false, false);
        GenerateRow("BC_Run_Selected.png", new Color(1.00f, 0.36f, 0.72f, 1f), true, false);
        GenerateCardOverlay();
        GenerateIconFrame();
        GenerateInfoInner();
        GenerateInfoFrame();
        GenerateInfoDividers();

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        ConfigureSprite("BC_CardRow_Normal.png", new Vector4(28f, 18f, 28f, 18f));
        ConfigureSprite("BC_CardRow_Selected.png", new Vector4(28f, 18f, 28f, 18f));
        ConfigureSprite("BC_Skills_Normal.png", new Vector4(28f, 18f, 28f, 18f));
        ConfigureSprite("BC_Skills_Selected.png", new Vector4(28f, 18f, 28f, 18f));
        ConfigureSprite("BC_Change_Normal.png", new Vector4(28f, 18f, 28f, 18f));
        ConfigureSprite("BC_Change_Selected.png", new Vector4(28f, 18f, 28f, 18f));
        ConfigureSprite("BC_Run_Normal.png", new Vector4(28f, 18f, 28f, 18f));
        ConfigureSprite("BC_Run_Selected.png", new Vector4(28f, 18f, 28f, 18f));
        ConfigureSprite("BC_CardOverlay_Circuit.png", Vector4.zero);
        ConfigureSprite("BC_LeftIconFrame.png", new Vector4(18f, 18f, 18f, 18f));
        ConfigureSprite("BC_InfoPanel_InnerBackground.png", new Vector4(48f, 44f, 48f, 44f));
        ConfigureSprite("BC_InfoPanel_OuterFrame.png", new Vector4(48f, 44f, 48f, 44f));
        ConfigureSprite("BC_InfoPanel_Dividers.png", Vector4.zero);
        AssetDatabase.SaveAssets();
    }

    private static void GenerateRow(string fileName, Color accent, bool selected, bool cardRow)
    {
        Texture2D texture = NewTexture(384, 56);
        Color fill = cardRow
            ? new Color(0.015f, 0.055f, 0.080f, selected ? 0.92f : 0.84f)
            : new Color(0.018f, 0.040f, 0.055f, selected ? 0.92f : 0.86f);
        if (!cardRow)
        {
            fill = Color.Lerp(fill, accent, selected ? 0.16f : 0.09f);
            fill.a = selected ? 0.94f : 0.88f;
        }

        DrawBeveledFill(texture, 0, 0, texture.width, texture.height, 16, fill);
        DrawBeveledRing(texture, 1, 1, texture.width - 2, texture.height - 2, 15, selected ? 4 : 3, WithAlpha(accent, selected ? 0.98f : 0.62f));
        DrawBeveledRing(texture, 7, 7, texture.width - 14, texture.height - 14, 9, 2, WithAlpha(accent, selected ? 0.70f : 0.38f));

        if (selected)
        {
            DrawBeveledRing(texture, 0, 0, texture.width, texture.height, 16, 8, WithAlpha(accent, 0.16f));
        }

        DrawRect(texture, 40, texture.height - 12, texture.width - 80, 2, WithAlpha(accent, selected ? 0.72f : 0.38f));
        DrawRect(texture, 40, 10, texture.width - 80, 2, WithAlpha(accent, selected ? 0.56f : 0.30f));
        DrawRect(texture, 68, texture.height - 18, 44, 2, WithAlpha(accent, selected ? 0.40f : 0.22f));
        DrawRect(texture, texture.width - 112, 16, 44, 2, WithAlpha(accent, selected ? 0.38f : 0.20f));

        DrawLine(texture, 18, 12, 32, 26, WithAlpha(accent, 0.48f), 2);
        DrawLine(texture, texture.width - 19, texture.height - 12, texture.width - 33, texture.height - 26, WithAlpha(accent, 0.48f), 2);
        DrawLine(texture, 16, texture.height - 15, 30, texture.height - 15, WithAlpha(Color.white, selected ? 0.80f : 0.42f), 2);
        DrawLine(texture, texture.width - 17, 15, texture.width - 31, 15, WithAlpha(Color.white, selected ? 0.80f : 0.42f), 2);

        Save(texture, fileName);
    }

    private static void GenerateCardOverlay()
    {
        Texture2D texture = NewTexture(384, 56);
        Color line = new Color(0.38f, 0.90f, 1f, 0.30f);
        DrawLine(texture, 42, 16, 110, 16, line, 2);
        DrawLine(texture, 110, 16, 146, 31, line, 2);
        DrawLine(texture, 146, 31, 222, 31, line, 2);
        DrawLine(texture, 222, 31, 250, 20, line, 2);
        DrawLine(texture, 250, 20, 332, 20, line, 2);
        DrawRect(texture, 118, 27, 5, 5, WithAlpha(line, 0.75f));
        DrawRect(texture, 244, 17, 5, 5, WithAlpha(line, 0.75f));
        DrawRect(texture, 304, 18, 4, 4, WithAlpha(line, 0.55f));
        Save(texture, "BC_CardOverlay_Circuit.png");
    }

    private static void GenerateIconFrame()
    {
        Texture2D texture = NewTexture(72, 72);
        Color accent = new Color(0.78f, 0.95f, 1f, 1f);
        DrawBeveledRing(texture, 4, 4, 64, 64, 14, 4, WithAlpha(accent, 0.90f));
        DrawBeveledRing(texture, 11, 11, 50, 50, 8, 2, WithAlpha(accent, 0.45f));
        DrawRect(texture, 31, 8, 10, 2, WithAlpha(Color.white, 0.70f));
        DrawRect(texture, 31, 62, 10, 2, WithAlpha(Color.white, 0.70f));
        Save(texture, "BC_LeftIconFrame.png");
    }

    private static void GenerateInfoInner()
    {
        Texture2D texture = NewTexture(640, 260);
        DrawBeveledFill(texture, 0, 0, texture.width, texture.height, 28, new Color(0.035f, 0.010f, 0.070f, 0.90f));
        DrawBeveledFill(texture, 18, 18, texture.width - 36, texture.height - 36, 18, new Color(0.060f, 0.018f, 0.120f, 0.46f));
        DrawRect(texture, 48, texture.height - 56, texture.width - 96, 3, new Color(0.64f, 0.25f, 1f, 0.36f));
        DrawRect(texture, 48, 100, texture.width - 96, 2, new Color(0.64f, 0.25f, 1f, 0.28f));
        Save(texture, "BC_InfoPanel_InnerBackground.png");
    }

    private static void GenerateInfoFrame()
    {
        Texture2D texture = NewTexture(640, 260);
        Color accent = new Color(0.72f, 0.26f, 1f, 1f);
        DrawBeveledRing(texture, 1, 1, texture.width - 2, texture.height - 2, 28, 5, WithAlpha(accent, 0.90f));
        DrawBeveledRing(texture, 12, 12, texture.width - 24, texture.height - 24, 17, 2, WithAlpha(accent, 0.42f));
        DrawRect(texture, 82, texture.height - 14, 134, 3, WithAlpha(Color.white, 0.70f));
        DrawRect(texture, texture.width - 216, texture.height - 14, 134, 3, WithAlpha(Color.white, 0.70f));
        DrawRect(texture, 82, 11, 134, 3, WithAlpha(Color.white, 0.54f));
        DrawRect(texture, texture.width - 216, 11, 134, 3, WithAlpha(Color.white, 0.54f));
        Save(texture, "BC_InfoPanel_OuterFrame.png");
    }

    private static void GenerateInfoDividers()
    {
        Texture2D texture = NewTexture(640, 96);
        Color accent = new Color(0.72f, 0.26f, 1f, 0.52f);
        DrawRect(texture, 0, 47, texture.width, 2, accent);
        DrawRect(texture, texture.width / 2 - 1, 8, 2, texture.height - 16, accent);
        DrawRect(texture, 20, 86, texture.width - 40, 2, WithAlpha(accent, 0.55f));
        DrawRect(texture, 20, 8, texture.width - 40, 2, WithAlpha(accent, 0.42f));
        Save(texture, "BC_InfoPanel_Dividers.png");
    }

    private static Texture2D NewTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        texture.SetPixels(pixels);
        return texture;
    }

    private static void DrawBeveledFill(Texture2D texture, int x, int y, int width, int height, int bevel, Color color)
    {
        for (int py = y; py < y + height; py++)
        {
            for (int px = x; px < x + width; px++)
            {
                if (InsideBeveled(px, py, x, y, width, height, bevel))
                {
                    BlendPixel(texture, px, py, color);
                }
            }
        }
    }

    private static void DrawBeveledRing(Texture2D texture, int x, int y, int width, int height, int bevel, int thickness, Color color)
    {
        for (int py = y; py < y + height; py++)
        {
            for (int px = x; px < x + width; px++)
            {
                bool outer = InsideBeveled(px, py, x, y, width, height, bevel);
                bool inner = InsideBeveled(px, py, x + thickness, y + thickness, width - thickness * 2, height - thickness * 2, Mathf.Max(0, bevel - thickness));
                if (outer && !inner)
                {
                    BlendPixel(texture, px, py, color);
                }
            }
        }
    }

    private static bool InsideBeveled(int px, int py, int x, int y, int width, int height, int bevel)
    {
        int lx = px - x;
        int ly = py - y;
        if (lx < 0 || ly < 0 || lx >= width || ly >= height)
        {
            return false;
        }

        int rx = width - 1 - lx;
        int ty = height - 1 - ly;
        return lx + ly >= bevel
            && rx + ly >= bevel
            && lx + ty >= bevel
            && rx + ty >= bevel;
    }

    private static void DrawRect(Texture2D texture, int x, int y, int width, int height, Color color)
    {
        for (int py = y; py < y + height; py++)
        {
            for (int px = x; px < x + width; px++)
            {
                BlendPixel(texture, px, py, color);
            }
        }
    }

    private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color color, int thickness)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;
        int x = x0;
        int y = y0;

        while (true)
        {
            DrawRect(texture, x - thickness / 2, y - thickness / 2, thickness, thickness, color);
            if (x == x1 && y == y1)
            {
                break;
            }

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }
    }

    private static void BlendPixel(Texture2D texture, int x, int y, Color src)
    {
        if (x < 0 || y < 0 || x >= texture.width || y >= texture.height || src.a <= 0f)
        {
            return;
        }

        Color dst = texture.GetPixel(x, y);
        float outA = src.a + dst.a * (1f - src.a);
        if (outA <= 0f)
        {
            texture.SetPixel(x, y, Color.clear);
            return;
        }

        Color output = new Color(
            (src.r * src.a + dst.r * dst.a * (1f - src.a)) / outA,
            (src.g * src.a + dst.g * dst.a * (1f - src.a)) / outA,
            (src.b * src.a + dst.b * dst.a * (1f - src.a)) / outA,
            outA);
        texture.SetPixel(x, y, output);
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private static void Save(Texture2D texture, string fileName)
    {
        texture.Apply(false, false);
        File.WriteAllBytes(ToAbsolutePath(fileName), texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
    }

    private static void ConfigureSprite(string fileName, Vector4 border)
    {
        string path = Folder + "/" + fileName;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.spritePixelsPerUnit = 100f;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }

    private static string ToAbsolutePath(string fileName)
    {
        return Path.Combine(Application.dataPath, "_Generated/UI/BattleCommand/" + fileName);
    }

    private static void EnsureFolder(string assetFolder)
    {
        if (AssetDatabase.IsValidFolder(assetFolder))
        {
            return;
        }

        string[] parts = assetFolder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}
