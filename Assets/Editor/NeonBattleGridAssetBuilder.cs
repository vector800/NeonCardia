using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class NeonBattleGridAssetBuilder
{
    private const int PanelSize = 256;
    private const int FloorWidth = 1024;
    private const int FloorHeight = 512;
    private const float PixelsPerUnit = 100f;
    private const string Root = "Assets/Art/BattleField/NeonGrid";
    private const string Sprites = Root + "/Sprites";
    private const string Textures = Root + "/Textures";
    private const string Materials = Root + "/Materials";
    private const string Prefabs = Root + "/Prefabs";
    private const string ScenePath = "Assets/Scenes/BattleScene.unity";

    private const string RedPanelPath = Sprites + "/Panel_RedOrange_Base.png";
    private const string CyanPanelPath = Sprites + "/Panel_CyanBlue_Base.png";
    private const string RedGlowPath = Sprites + "/Panel_RedOrange_Glow.png";
    private const string CyanGlowPath = Sprites + "/Panel_CyanBlue_Glow.png";
    private const string FloorPath = Textures + "/CyberFloor_Base.png";
    private const string Image2FullGridPath = Textures + "/BattleGrid_Full_Image2.png";
    private const string RedMaterialPath = Materials + "/MAT_Panel_RedOrange.mat";
    private const string CyanMaterialPath = Materials + "/MAT_Panel_CyanBlue.mat";
    private const string FloorMaterialPath = Materials + "/MAT_CyberFloor.mat";
    private const string Image2FullGridMaterialPath = Materials + "/MAT_BattleGrid_Full_Image2.mat";
    private const string RedPanelPrefabPath = Prefabs + "/PF_Panel_RedOrange.prefab";
    private const string CyanPanelPrefabPath = Prefabs + "/PF_Panel_CyanBlue.prefab";
    private const string AllyGridPrefabPath = Prefabs + "/PF_BattleGrid_Ally_3x3.prefab";
    private const string EnemyGridPrefabPath = Prefabs + "/PF_BattleGrid_Enemy_3x3.prefab";
    private const string FullGridPrefabPath = Prefabs + "/PF_BattleGrid_Full_3x6.prefab";
    private const string Image2FullGridPrefabPath = Prefabs + "/PF_BattleGrid_Full_3x6_Image2.prefab";

    [MenuItem("Tools/NeonCardia/Build Neon Battle Grid Field")]
    public static void Build()
    {
        EnsureFolders();
        WritePanelTexture(RedPanelPath, Palette.Red);
        WritePanelTexture(CyanPanelPath, Palette.Cyan);
        WriteGlowTexture(RedGlowPath, Palette.Red);
        WriteGlowTexture(CyanGlowPath, Palette.Cyan);
        WriteFloorTexture(FloorPath);

        ConfigureSpriteImport(RedPanelPath, true);
        ConfigureSpriteImport(CyanPanelPath, true);
        ConfigureSpriteImport(RedGlowPath, true);
        ConfigureSpriteImport(CyanGlowPath, true);
        ConfigureSpriteImport(FloorPath, false);
        if (File.Exists(GetFullAssetPath(Image2FullGridPath)))
        {
            ConfigureSpriteImport(Image2FullGridPath, true);
        }

        Material redMaterial = CreateSpriteMaterial(RedMaterialPath);
        Material cyanMaterial = CreateSpriteMaterial(CyanMaterialPath);
        Material floorMaterial = CreateSpriteMaterial(FloorMaterialPath);
        Material image2Material = CreateSpriteMaterial(Image2FullGridMaterialPath);

        CreatePanelPrefab(RedPanelPrefabPath, RedPanelPath, RedGlowPath, redMaterial, Palette.Red);
        CreatePanelPrefab(CyanPanelPrefabPath, CyanPanelPath, CyanGlowPath, cyanMaterial, Palette.Cyan);
        CreateGridPrefab(AllyGridPrefabPath, RedPanelPrefabPath, null, false);
        CreateGridPrefab(EnemyGridPrefabPath, CyanPanelPrefabPath, null, false);
        CreateGridPrefab(FullGridPrefabPath, RedPanelPrefabPath, CyanPanelPrefabPath, true, floorMaterial);
        if (File.Exists(GetFullAssetPath(Image2FullGridPath)))
        {
            CreateImage2FullGridPrefab(image2Material);
            PlaceGridInBattleScene(Image2FullGridPrefabPath, "PF_BattleGrid_Full_3x6_Image2", new Vector3(0f, -0.46f, 0.1f), new Vector3(0.62f, 0.62f, 1f));
        }
        else
        {
            PlaceGridInBattleScene(FullGridPrefabPath, "PF_BattleGrid_Full_3x6", new Vector3(0f, -1.16f, 0.1f), new Vector3(0.78f, 0.78f, 1f));
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log("Built Neon Battle Grid assets and placed the active full-grid prefab in BattleScene.");
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Art", "BattleField");
        EnsureFolder("Assets/Art/BattleField", "NeonGrid");
        EnsureFolder(Root, "Sprites");
        EnsureFolder(Root, "Materials");
        EnsureFolder(Root, "Prefabs");
        EnsureFolder(Root, "Textures");
    }

    private static void EnsureFolder(string parent, string folderName)
    {
        string path = parent + "/" + folderName;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }

    private static void WritePanelTexture(string assetPath, Palette palette)
    {
        Texture2D texture = new Texture2D(PanelSize, PanelSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < PanelSize; y++)
        {
            for (int x = 0; x < PanelSize; x++)
            {
                float edge = ChamferedEdgeDistance(x, y, PanelSize, 22f);
                if (edge < 0f)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float nx = x / (PanelSize - 1f);
                float ny = y / (PanelSize - 1f);
                float vignette = Mathf.Clamp01(1.2f - Vector2.Distance(new Vector2(nx, ny), new Vector2(0.5f, 0.54f)) * 1.55f);
                float scan = Mathf.Sin((x + y * 0.65f) * 0.095f) * 0.5f + 0.5f;
                Color color = Color.Lerp(palette.FloorDark, palette.FloorMid, 0.22f + vignette * 0.44f);
                color = Color.Lerp(color, palette.FloorCool, scan * 0.045f);

                if (edge < 5f)
                {
                    color = Color.Lerp(palette.Hot, Color.white, edge < 1.4f ? 0.28f : 0.08f);
                }
                else if (edge < 13f)
                {
                    color = Color.Lerp(color, palette.Glow, Mathf.InverseLerp(13f, 5f, edge) * 0.72f);
                }

                float bottomLift = Mathf.InverseLerp(56f, 0f, y);
                if (bottomLift > 0f)
                {
                    color = Color.Lerp(color, palette.EdgeShadow, bottomLift * 0.34f);
                }

                texture.SetPixel(x, y, color);
            }
        }

        DrawPanelLines(texture, palette);
        WritePng(texture, assetPath);
    }

    private static void DrawPanelLines(Texture2D texture, Palette palette)
    {
        Color outer = palette.Hot;
        Color line = Color.Lerp(palette.Glow, Color.white, 0.2f);
        Color dim = WithAlpha(Color.Lerp(palette.Glow, palette.FloorDark, 0.48f), 0.78f);

        DrawChamferedRect(texture, 11, 11, PanelSize - 12, PanelSize - 12, 17, 4, outer);
        DrawChamferedRect(texture, 18, 19, PanelSize - 19, PanelSize - 25, 14, 2, line);
        DrawChamferedRect(texture, 30, 34, PanelSize - 31, PanelSize - 42, 12, 1, dim);

        DrawCornerCircuit(texture, 34, 34, 1, 1, line, dim);
        DrawCornerCircuit(texture, PanelSize - 35, 34, -1, 1, line, dim);
        DrawCornerCircuit(texture, 34, PanelSize - 43, 1, -1, line, dim);
        DrawCornerCircuit(texture, PanelSize - 35, PanelSize - 43, -1, -1, line, dim);

        DrawLine(texture, 58, 92, 106, 92, dim, 2);
        DrawLine(texture, 106, 92, 128, 112, dim, 1);
        DrawLine(texture, 130, 134, 202, 134, dim, 2);
        DrawLine(texture, 202, 134, 216, 150, dim, 1);
        DrawLine(texture, 73, 173, 136, 173, dim, 2);
        DrawLine(texture, 136, 173, 153, 188, dim, 1);
        DrawLine(texture, 93, 83, 93, 120, dim, 1);
        DrawLine(texture, 172, 146, 172, 190, dim, 1);
        DrawNode(texture, 113, 92, palette.Hot);
        DrawNode(texture, 202, 134, palette.Glow);
        DrawNode(texture, 73, 173, palette.Glow);

        DrawLine(texture, 0, 10, PanelSize - 1, 10, WithAlpha(palette.Glow, 0.35f), 1);
        DrawLine(texture, 0, 28, PanelSize - 1, 28, WithAlpha(palette.Glow, 0.16f), 1);
        DrawLine(texture, 0, PanelSize - 18, PanelSize - 1, PanelSize - 18, WithAlpha(palette.Hot, 0.42f), 2);
    }

    private static void WriteGlowTexture(string assetPath, Palette palette)
    {
        Texture2D texture = new Texture2D(PanelSize, PanelSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < PanelSize; y++)
        {
            for (int x = 0; x < PanelSize; x++)
            {
                float edge = ChamferedEdgeDistance(x, y, PanelSize, 22f);
                if (edge < -24f || edge > 26f)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float alpha = 0f;
                if (edge >= 0f)
                {
                    alpha = Mathf.InverseLerp(26f, 0f, edge) * 0.55f;
                }
                else
                {
                    alpha = Mathf.InverseLerp(-24f, 0f, edge) * 0.72f;
                }

                Color color = Color.Lerp(palette.Glow, palette.Hot, Mathf.Clamp01(1f - Mathf.Abs(edge) / 24f));
                color.a = Mathf.Clamp01(alpha);
                texture.SetPixel(x, y, color);
            }
        }

        DrawChamferedRect(texture, 10, 10, PanelSize - 11, PanelSize - 11, 17, 8, WithAlpha(palette.Hot, 0.9f));
        WritePng(texture, assetPath);
    }

    private static void WriteFloorTexture(string assetPath)
    {
        Texture2D texture = new Texture2D(FloorWidth, FloorHeight, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color top = new Color(0.008f, 0.012f, 0.026f, 1f);
        Color bottom = new Color(0.018f, 0.026f, 0.045f, 1f);
        Color midBlue = new Color(0.02f, 0.06f, 0.12f, 1f);

        for (int y = 0; y < FloorHeight; y++)
        {
            for (int x = 0; x < FloorWidth; x++)
            {
                float nx = x / (FloorWidth - 1f);
                float ny = y / (FloorHeight - 1f);
                float vignette = Mathf.Clamp01(Vector2.Distance(new Vector2(nx, ny), new Vector2(0.5f, 0.48f)) * 1.2f);
                Color color = Color.Lerp(bottom, top, ny * 0.65f);
                color = Color.Lerp(color, midBlue, Mathf.Clamp01(1f - vignette) * 0.13f);
                texture.SetPixel(x, y, color);
            }
        }

        DrawFloorCircuitry(texture);
        WritePng(texture, assetPath);
    }

    private static void DrawFloorCircuitry(Texture2D texture)
    {
        Color red = new Color(1f, 0.12f, 0.03f, 0.58f);
        Color orange = new Color(1f, 0.58f, 0.05f, 0.50f);
        Color cyan = new Color(0.0f, 0.88f, 1f, 0.58f);
        Color blue = new Color(0.0f, 0.30f, 1f, 0.50f);
        Color divider = new Color(0.40f, 0.78f, 1f, 0.38f);

        for (int y = 34; y < FloorHeight; y += 58)
        {
            Color color = y % 116 == 34 ? red : orange;
            DrawLine(texture, 0, y, FloorWidth / 2 - 16, y + Mathf.RoundToInt((y - 250) * 0.08f), WithAlpha(color, 0.24f), 1);
            DrawLine(texture, FloorWidth / 2 + 16, y + Mathf.RoundToInt((250 - y) * 0.07f), FloorWidth - 1, y, WithAlpha(y % 116 == 34 ? cyan : blue, 0.24f), 1);
        }

        DrawLine(texture, FloorWidth / 2, 28, FloorWidth / 2, FloorHeight - 20, divider, 2);
        DrawLine(texture, FloorWidth / 2 - 10, 92, FloorWidth / 2 - 10, FloorHeight - 70, WithAlpha(red, 0.22f), 1);
        DrawLine(texture, FloorWidth / 2 + 10, 92, FloorWidth / 2 + 10, FloorHeight - 70, WithAlpha(cyan, 0.22f), 1);

        int[] redXs = { 50, 112, 188, 270, 356, 444 };
        int[] cyanXs = { 580, 654, 734, 820, 902, 978 };
        for (int i = 0; i < redXs.Length; i++)
        {
            int x = redXs[i];
            DrawCircuitPath(texture, x, 48 + i * 38, 1, i % 2 == 0 ? red : orange);
        }

        for (int i = 0; i < cyanXs.Length; i++)
        {
            int x = cyanXs[i];
            DrawCircuitPath(texture, x, 70 + i * 35, -1, i % 2 == 0 ? cyan : blue);
        }

        for (int i = 0; i < 44; i++)
        {
            int x = i < 22 ? 38 + (i * 43) % 438 : 542 + (i * 47) % 440;
            int y = 28 + (i * 73) % 438;
            Color c = i < 22 ? red : cyan;
            DrawNode(texture, x, y, WithAlpha(c, 0.52f));
        }

        DrawLine(texture, 90, 86, 420, 30, WithAlpha(red, 0.18f), 2);
        DrawLine(texture, 602, 30, 948, 92, WithAlpha(cyan, 0.18f), 2);
        DrawLine(texture, 58, 430, 456, 464, WithAlpha(orange, 0.20f), 2);
        DrawLine(texture, 570, 458, 984, 418, WithAlpha(blue, 0.20f), 2);
    }

    private static void DrawCircuitPath(Texture2D texture, int startX, int startY, int direction, Color color)
    {
        int x = startX;
        int y = Mathf.Clamp(startY, 18, FloorHeight - 18);
        int lenA = 48 + Mathf.Abs((startX + startY) % 52);
        int lenB = 24 + Mathf.Abs((startX * 3 + startY) % 46);
        DrawLine(texture, x, y, x + direction * lenA, y, WithAlpha(color, 0.34f), 2);
        x += direction * lenA;
        DrawLine(texture, x, y, x, y + 26, WithAlpha(color, 0.22f), 1);
        y += 26;
        DrawLine(texture, x, y, x + direction * lenB, y, WithAlpha(color, 0.30f), 1);
        DrawNode(texture, x + direction * lenB, y, WithAlpha(color, 0.56f));
    }

    private static void CreatePanelPrefab(string prefabPath, string baseSpritePath, string glowSpritePath, Material material, Palette palette)
    {
        Sprite baseSprite = AssetDatabase.LoadAssetAtPath<Sprite>(baseSpritePath);
        Sprite glowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(glowSpritePath);
        GameObject root = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));

        SpriteRenderer shadow = CreateSpriteChild(root.transform, "Panel_Shadow", baseSprite, null, new Vector3(0.05f, -0.08f, 0.02f), new Vector3(1.04f, 1.04f, 1f), new Color(0f, 0f, 0f, 0.45f), 0);
        shadow.sortingOrder = 0;

        SpriteRenderer glow = CreateSpriteChild(root.transform, palette.Name + "_Glow", glowSprite, material, new Vector3(0f, -0.005f, 0f), new Vector3(1.18f, 1.18f, 1f), Color.white, 1);
        glow.sortingOrder = 1;

        SpriteRenderer baseRenderer = CreateSpriteChild(root.transform, "Panel_Base", baseSprite, material, Vector3.zero, Vector3.one, Color.white, 2);
        baseRenderer.sortingOrder = 2;

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
    }

    private static SpriteRenderer CreateSpriteChild(Transform parent, string name, Sprite sprite, Material material, Vector3 position, Vector3 scale, Color color, int sortingOrder)
    {
        GameObject child = new GameObject(name, typeof(SpriteRenderer));
        child.transform.SetParent(parent, false);
        child.transform.localPosition = position;
        child.transform.localScale = scale;
        SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = material;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private static void CreateGridPrefab(string prefabPath, string allyPanelPrefabPath, string enemyPanelPrefabPath, bool fullGrid, Material floorMaterial = null)
    {
        GameObject root = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
        const float stepX = 2.36f;
        const float stepY = 1.42f;
        const float panelYScale = 0.58f;

        if (fullGrid)
        {
            Sprite floorSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FloorPath);
            SpriteRenderer floor = CreateSpriteChild(root.transform, "CyberFloor_Base", floorSprite, floorMaterial, new Vector3(0f, -0.10f, 0.18f), new Vector3(1.66f, 1.08f, 1f), Color.white, -4);
            floor.sortingOrder = -4;
        }

        int startColumn = fullGrid ? 0 : 0;
        int totalColumns = fullGrid ? 6 : 3;
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < totalColumns; column++)
            {
                bool allySide = !fullGrid || column < 3;
                string sourcePath = allySide ? allyPanelPrefabPath : enemyPanelPrefabPath;
                if (string.IsNullOrEmpty(sourcePath))
                {
                    continue;
                }

                GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                GameObject panel = PrefabUtility.InstantiatePrefab(source) as GameObject;
                panel.name = (allySide ? "AllyPanel_RedOrange_" : "EnemyPanel_CyanBlue_") + row + "_" + (fullGrid ? column : column + startColumn);
                panel.transform.SetParent(root.transform, false);
                float x = (column - (totalColumns - 1) * 0.5f) * stepX;
                float y = (1 - row) * stepY;
                float depthScale = 1f + (row - 1) * 0.035f;
                panel.transform.localPosition = new Vector3(x, y, -row * 0.01f);
                panel.transform.localScale = new Vector3(depthScale, panelYScale * depthScale, 1f);
            }
        }

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
    }

    private static void CreateImage2FullGridPrefab(Material material)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Image2FullGridPath);
        GameObject root = new GameObject("PF_BattleGrid_Full_3x6_Image2");
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = material;
        renderer.sortingOrder = -2;
        PrefabUtility.SaveAsPrefabAsset(root, Image2FullGridPrefabPath);
        Object.DestroyImmediate(root);
    }

    private static void PlaceGridInBattleScene(string prefabPath, string instanceName, Vector3 position, Vector3 scale)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Skipped BattleScene placement because the editor is entering or leaving Play Mode.");
            return;
        }

        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        string[] oldNames = { "PF_BattleGrid_Full_3x6", "PF_BattleGrid_Full_3x6_Image2" };
        foreach (string oldName in oldNames)
        {
            GameObject existing = GameObject.Find(oldName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
        instance.name = instanceName;
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.identity;
        instance.transform.localScale = scale;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static Material CreateSpriteMaterial(string path)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }

        material.color = Color.white;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ConfigureSpriteImport(string assetPath, bool alphaTransparency)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.alphaIsTransparency = alphaTransparency;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    private static void WritePng(Texture2D texture, string assetPath)
    {
        texture.Apply(false, false);
        string fullPath = GetFullAssetPath(assetPath);
        File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
    }

    private static string GetFullAssetPath(string assetPath)
    {
        return Path.Combine(Application.dataPath, assetPath.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar));
    }

    private static float ChamferedEdgeDistance(int x, int y, int size, float chamfer)
    {
        float left = x;
        float right = size - 1 - x;
        float bottom = y;
        float top = size - 1 - y;
        float edge = Mathf.Min(Mathf.Min(left, right), Mathf.Min(bottom, top));

        if (left < chamfer && bottom < chamfer)
        {
            edge = Mathf.Min(edge, left + bottom - chamfer * 0.74f);
        }
        if (right < chamfer && bottom < chamfer)
        {
            edge = Mathf.Min(edge, right + bottom - chamfer * 0.74f);
        }
        if (left < chamfer && top < chamfer)
        {
            edge = Mathf.Min(edge, left + top - chamfer * 0.74f);
        }
        if (right < chamfer && top < chamfer)
        {
            edge = Mathf.Min(edge, right + top - chamfer * 0.74f);
        }

        return edge;
    }

    private static void DrawChamferedRect(Texture2D texture, int xMin, int yMin, int xMax, int yMax, int chamfer, int thickness, Color color)
    {
        for (int i = 0; i < thickness; i++)
        {
            DrawLine(texture, xMin + chamfer, yMin + i, xMax - chamfer, yMin + i, color, 1);
            DrawLine(texture, xMin + chamfer, yMax - i, xMax - chamfer, yMax - i, color, 1);
            DrawLine(texture, xMin + i, yMin + chamfer, xMin + i, yMax - chamfer, color, 1);
            DrawLine(texture, xMax - i, yMin + chamfer, xMax - i, yMax - chamfer, color, 1);
            DrawLine(texture, xMin + i, yMin + chamfer, xMin + chamfer, yMin + i, color, 1);
            DrawLine(texture, xMax - chamfer, yMin + i, xMax - i, yMin + chamfer, color, 1);
            DrawLine(texture, xMin + i, yMax - chamfer, xMin + chamfer, yMax - i, color, 1);
            DrawLine(texture, xMax - chamfer, yMax - i, xMax - i, yMax - chamfer, color, 1);
        }
    }

    private static void DrawCornerCircuit(Texture2D texture, int x, int y, int xDirection, int yDirection, Color bright, Color dim)
    {
        DrawLine(texture, x, y, x + xDirection * 26, y, bright, 3);
        DrawLine(texture, x, y, x, y + yDirection * 26, bright, 3);
        DrawLine(texture, x + xDirection * 34, y + yDirection * 8, x + xDirection * 58, y + yDirection * 8, dim, 1);
        DrawLine(texture, x + xDirection * 8, y + yDirection * 34, x + xDirection * 8, y + yDirection * 55, dim, 1);
    }

    private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color color, int thickness)
    {
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int error = dx + dy;

        while (true)
        {
            DrawBrush(texture, x0, y0, thickness, color);
            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = 2 * error;
            if (e2 >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    private static void DrawBrush(Texture2D texture, int centerX, int centerY, int radius, Color color)
    {
        for (int y = centerY - radius; y <= centerY + radius; y++)
        {
            for (int x = centerX - radius; x <= centerX + radius; x++)
            {
                if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
                {
                    continue;
                }

                if (Vector2.Distance(new Vector2(centerX, centerY), new Vector2(x, y)) <= radius + 0.35f)
                {
                    Color existing = texture.GetPixel(x, y);
                    texture.SetPixel(x, y, AlphaBlend(existing, color));
                }
            }
        }
    }

    private static void DrawNode(Texture2D texture, int centerX, int centerY, Color color)
    {
        DrawBrush(texture, centerX, centerY, 3, color);
        DrawBrush(texture, centerX, centerY, 1, Color.Lerp(color, Color.white, 0.35f));
    }

    private static Color AlphaBlend(Color under, Color over)
    {
        float alpha = over.a + under.a * (1f - over.a);
        if (alpha <= 0f)
        {
            return Color.clear;
        }

        Color result = (over * over.a + under * under.a * (1f - over.a)) / alpha;
        result.a = alpha;
        return result;
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    private struct Palette
    {
        public static readonly Palette Red = new Palette(
            "RedOrange",
            new Color(0.020f, 0.010f, 0.014f, 1f),
            new Color(0.135f, 0.026f, 0.018f, 1f),
            new Color(0.045f, 0.022f, 0.040f, 1f),
            new Color(1f, 0.16f, 0.025f, 1f),
            new Color(1f, 0.58f, 0.045f, 1f),
            new Color(0.012f, 0.006f, 0.008f, 1f));

        public static readonly Palette Cyan = new Palette(
            "CyanBlue",
            new Color(0.008f, 0.017f, 0.036f, 1f),
            new Color(0.018f, 0.078f, 0.170f, 1f),
            new Color(0.014f, 0.042f, 0.095f, 1f),
            new Color(0.0f, 0.82f, 1f, 1f),
            new Color(0.05f, 0.32f, 1f, 1f),
            new Color(0.006f, 0.010f, 0.020f, 1f));

        public readonly string Name;
        public readonly Color FloorDark;
        public readonly Color FloorMid;
        public readonly Color FloorCool;
        public readonly Color Glow;
        public readonly Color Hot;
        public readonly Color EdgeShadow;

        private Palette(string name, Color floorDark, Color floorMid, Color floorCool, Color glow, Color hot, Color edgeShadow)
        {
            Name = name;
            FloorDark = floorDark;
            FloorMid = floorMid;
            FloorCool = floorCool;
            Glow = glow;
            Hot = hot;
            EdgeShadow = edgeShadow;
        }
    }
}
