using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class PanelAlignedBattleSceneBuilder
{
    public const string ScenePath = "Assets/Scenes/BattleScene_PanelAligned.unity";

    private const string HudPrefabPath = "Assets/Prefabs/UI/BattleTimelineHud.prefab";
    private const string BoardSpritePath = "Assets/Art/BattleField/NeonGrid/Textures/BattleGrid_Full_Image2.png";
    private const string GeneratedAssetFolder = "Assets/Art/BattleField/PanelAlignedGenerated";

    private const string AllyPanelPath = GeneratedAssetFolder + "/BattlePanel_Ally_Base.png";
    private const string EnemyPanelPath = GeneratedAssetFolder + "/BattlePanel_Enemy_Base.png";
    private const string RangeSinglePath = GeneratedAssetFolder + "/RangeOverlay_Single.png";
    private const string RangeAreaPath = GeneratedAssetFolder + "/RangeOverlay_Area.png";
    private const string EffectSinglePath = GeneratedAssetFolder + "/Effect_Hit_Single.png";
    private const string EffectLineHorizontalPath = GeneratedAssetFolder + "/Effect_Hit_LineHorizontal.png";
    private const string EffectLineVerticalPath = GeneratedAssetFolder + "/Effect_Hit_LineVertical.png";
    private const string EffectAreaPath = GeneratedAssetFolder + "/Effect_Hit_Area.png";
    private const string EffectProjectilePath = GeneratedAssetFolder + "/Effect_Projectile_Simple.png";

    private const string AllyASpritePath = "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_00.png";
    private const string AllyBSpritePath = "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_00.png";
    private const string Enemy1SpritePath = "Assets/Art/Enemies/DrillMole/Frames/Enemy_DrillMole_Idle_00.png";
    private const string Enemy2SpritePath = "Assets/Art/Enemies/ElecGecko/Frames/Enemy_ElecGecko_Idle_00.png";
    private const string Enemy3SpritePath = "Assets/Art/Enemies/BladeBug/Frames/Enemy_BladeBug_Idle_00.png";

    [MenuItem("Tools/NeonCardia/Create Panel Aligned BattleScene")]
    public static void CreatePanelAlignedBattleScene()
    {
        ConfigureGeneratedSpriteImports();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "BattleScene_PanelAligned";

        CreateCamera();
        CreateGlobalLight();
        CreateController();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created panel-aligned battle scene at " + ScenePath);
    }

    private static void ConfigureGeneratedSpriteImports()
    {
        string[] spritePaths =
        {
            AllyPanelPath,
            EnemyPanelPath,
            RangeSinglePath,
            RangeAreaPath,
            EffectSinglePath,
            EffectLineHorizontalPath,
            EffectLineVerticalPath,
            EffectAreaPath,
            EffectProjectilePath
        };

        for (int i = 0; i < spritePaths.Length; i++)
        {
            ConfigureSpriteImport(spritePaths[i], 420f);
        }
    }

    private static void ConfigureSpriteImport(string assetPath, float pixelsPerUnit)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("PanelAlignedBattleSceneBuilder missing generated sprite: " + assetPath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 4f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.01f, 0.012f, 0.018f, 1f);
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<UniversalAdditionalCameraData>();
    }

    private static void CreateGlobalLight()
    {
        GameObject lightObject = new GameObject("Global Light 2D");
        Light2D light = lightObject.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;
    }

    private static void CreateController()
    {
        GameObject controllerObject = new GameObject("Panel Aligned Battle Controller");
        PanelAlignedBattleSceneController controller = controllerObject.AddComponent<PanelAlignedBattleSceneController>();
        SerializedObject serializedController = new SerializedObject(controller);

        AssignBool(serializedController, "showDebugLabels", false);
        AssignBool(serializedController, "showRangePreview", false);
        AssignBool(serializedController, "autoCyclePreviews", false);
        AssignBool(serializedController, "showActionOrderHud", false);
        AssignVector2(serializedController, "gridCenter", new Vector2(0f, -1.15f));
        AssignFloat(serializedController, "boardScale", 0.88f);
        AssignHud(serializedController, "battleTimelineHudPrefab", HudPrefabPath);
        AssignSprite(serializedController, "battleBoardSprite", BoardSpritePath);
        AssignSprite(serializedController, "allyPanelSprite", AllyPanelPath);
        AssignSprite(serializedController, "enemyPanelSprite", EnemyPanelPath);
        AssignSprite(serializedController, "rangeOverlaySingleSprite", RangeSinglePath);
        AssignSprite(serializedController, "rangeOverlayAreaSprite", RangeAreaPath);
        AssignSprite(serializedController, "effectHitSingleSprite", EffectSinglePath);
        AssignSprite(serializedController, "effectHitLineHorizontalSprite", EffectLineHorizontalPath);
        AssignSprite(serializedController, "effectHitLineVerticalSprite", EffectLineVerticalPath);
        AssignSprite(serializedController, "effectHitAreaSprite", EffectAreaPath);
        AssignSprite(serializedController, "effectProjectileSimpleSprite", EffectProjectilePath);
        AssignSprite(serializedController, "allyASprite", AllyASpritePath);
        AssignSprite(serializedController, "allyBSprite", AllyBSpritePath);
        AssignSprite(serializedController, "enemy1Sprite", Enemy1SpritePath);
        AssignSprite(serializedController, "enemy2Sprite", Enemy2SpritePath);
        AssignSprite(serializedController, "enemy3Sprite", Enemy3SpritePath);

        serializedController.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void AssignFloat(SerializedObject serializedObject, string propertyName, float value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
        }
    }

    private static void AssignVector2(SerializedObject serializedObject, string propertyName, Vector2 value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.vector2Value = value;
        }
    }

    private static void AssignHud(SerializedObject serializedObject, string propertyName, string prefabPath)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        property.objectReferenceValue = prefab != null ? prefab.GetComponent<BattleTimelineHudView>() : null;
    }

    private static void AssignSprite(SerializedObject serializedObject, string propertyName, string assetPath)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.objectReferenceValue = LoadSprite(assetPath);
    }

    private static Sprite LoadSprite(string assetPath)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null)
        {
            return sprite;
        }

        Object[] representations = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
        for (int i = 0; i < representations.Length; i++)
        {
            sprite = representations[i] as Sprite;
            if (sprite != null)
            {
                return sprite;
            }
        }

        Debug.LogWarning("PanelAlignedBattleSceneBuilder could not load sprite at " + assetPath);
        return null;
    }
}
