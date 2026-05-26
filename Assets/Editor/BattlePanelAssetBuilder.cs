using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class BattlePanelAssetBuilder
{
    private const int Size = 256;
    private const string AllyPath = "Assets/Art/Battle/Panels/Panel_Ally_New.png";
    private const string EnemyPath = "Assets/Art/Battle/Panels/Panel_Enemy_New.png";
    private const string AllyPrefabPath = "Assets/Prefabs/Battle/PanelTile_Ally_New.prefab";
    private const string EnemyPrefabPath = "Assets/Prefabs/Battle/PanelTile_Enemy_New.prefab";

    [MenuItem("Tools/NeonCardia/Build New Battle Panels")]
    public static void BuildNewBattlePanels()
    {
        EnsureFolder("Assets/Art/Battle", "Panels");
        EnsureFolder("Assets/Prefabs", "Battle");

        WritePanel(AllyPath,
            new Color(0.18f, 0.02f, 0.015f, 1f),
            new Color(0.50f, 0.055f, 0.025f, 1f),
            new Color(1f, 0.25f, 0.045f, 1f),
            new Color(1f, 0.82f, 0.12f, 1f));

        WritePanel(EnemyPath,
            new Color(0.012f, 0.045f, 0.15f, 1f),
            new Color(0.025f, 0.18f, 0.46f, 1f),
            new Color(0.03f, 0.72f, 1f, 1f),
            new Color(0.45f, 0.96f, 1f, 1f));

        ConfigureSpriteImport(AllyPath);
        ConfigureSpriteImport(EnemyPath);
        CreatePanelPrefab(AllyPrefabPath, AllyPath);
        CreatePanelPrefab(EnemyPrefabPath, EnemyPath);
        AssetDatabase.SaveAssets();
        Debug.Log("Built new BattleScene panel sprites and prefabs.");
    }

    private static void EnsureFolder(string parent, string folderName)
    {
        string path = parent + "/" + folderName;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }

    private static void WritePanel(string assetPath, Color dark, Color mid, Color glow, Color hot)
    {
        Texture2D texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float nx = x / 255f;
                float ny = y / 255f;
                float vignette = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(nx, ny), new Vector2(0.5f, 0.55f)) * 1.15f);
                Color color = Color.Lerp(dark, mid, 0.28f + 0.5f * vignette + 0.10f * ny);
                float edge = Mathf.Min(Mathf.Min(x, Size - 1 - x), Mathf.Min(y, Size - 1 - y));
                if (edge < 3f)
                {
                    color = Color.Lerp(glow, hot, 0.45f);
                }
                else if (edge < 8f)
                {
                    color = Color.Lerp(color, glow, 0.42f - edge * 0.035f);
                }

                texture.SetPixel(x, y, color);
            }
        }

        DrawRect(texture, 0, 0, Size - 1, 2, hot);
        DrawRect(texture, 0, Size - 3, Size - 1, Size - 1, hot);
        DrawRect(texture, 0, 0, 2, Size - 1, hot);
        DrawRect(texture, Size - 3, 0, Size - 1, Size - 1, hot);
        DrawRect(texture, 18, 18, 24, Size - 19, glow);
        DrawRect(texture, Size - 25, 18, Size - 19, Size - 19, glow);
        DrawRect(texture, 18, 18, Size - 19, 24, glow);
        DrawRect(texture, 18, Size - 25, Size - 19, Size - 19, glow);
        DrawCornerMarks(texture, hot);
        DrawRect(texture, 54, 82, 112, 83, Color.Lerp(glow, dark, 0.25f));
        DrawRect(texture, 124, 128, 198, 129, Color.Lerp(glow, dark, 0.30f));
        DrawRect(texture, 92, 84, 93, 116, Color.Lerp(glow, dark, 0.35f));
        DrawRect(texture, 168, 136, 169, 181, Color.Lerp(glow, dark, 0.35f));
        DrawRect(texture, 58, 172, 146, 173, Color.Lerp(glow, dark, 0.40f));

        texture.Apply(false, false);
        string fullPath = Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar));
        File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
    }

    private static void DrawCornerMarks(Texture2D texture, Color color)
    {
        DrawRect(texture, 28, 28, 54, 32, color);
        DrawRect(texture, 28, 28, 32, 54, color);
        DrawRect(texture, Size - 55, 28, Size - 29, 32, color);
        DrawRect(texture, Size - 33, 28, Size - 29, 54, color);
        DrawRect(texture, 28, Size - 33, 54, Size - 29, color);
        DrawRect(texture, 28, Size - 55, 32, Size - 29, color);
        DrawRect(texture, Size - 55, Size - 33, Size - 29, Size - 29, color);
        DrawRect(texture, Size - 33, Size - 55, Size - 29, Size - 29, color);
    }

    private static void DrawRect(Texture2D texture, int xMin, int yMin, int xMax, int yMax, Color color)
    {
        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                if (x >= 0 && x < Size && y >= 0 && y < Size)
                {
                    texture.SetPixel(x, y, color);
                }
            }
        }
    }

    private static void ConfigureSpriteImport(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.alphaIsTransparency = false;
        importer.SaveAndReimport();
    }

    private static void CreatePanelPrefab(string prefabPath, string spritePath)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        GameObject gameObject = new GameObject(Path.GetFileNameWithoutExtension(prefabPath), typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Size, Size);

        Image image = gameObject.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.raycastTarget = true;

        PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
        Object.DestroyImmediate(gameObject);
    }
}
