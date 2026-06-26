using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BattlePartyStatusPanelPrefabSetup
{
    private const string EntryPrefabPath = "Assets/Prefabs/UI/BattlePartyStatusEntry.prefab";
    private const string PanelPrefabPath = "Assets/Prefabs/UI/BattlePartyStatusPanel.prefab";
    private const string BattleScenePath = "Assets/Scenes/BattleScene.unity";
    private const string WolfIconPath = "Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_02.png";
    private const string ArmorIconPath = "Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_01.png";
    private const string BlueIconPath = "Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_03.png";
    private const string GreenIconPath = "Assets/Art/UI/Battle/Party/FaceIcons/ReserveFaceIcon_Green.png";
    private const string PurpleIconPath = "Assets/Art/UI/Battle/Party/FaceIcons/ReserveFaceIcon_Purple.png";
    private const string GogglesIconPath = "Assets/Art/UI/Battle/Party/FaceIcons/ReserveFaceIcon_Goggles.png";

    [MenuItem("Tools/NeonCardia/Apply Battle Party Status Panel Setup")]
    public static void Apply()
    {
        EnsureFolderPath("Assets/Prefabs/UI");
        EnsureFolderPath("Assets/Art/UI/Battle/Party/FaceIcons");

        ConfigureSprite(GreenIconPath, 512);
        ConfigureSprite(PurpleIconPath, 512);
        ConfigureSprite(GogglesIconPath, 512);

        GameObject entryPrefab = CreateEntryPrefab();
        CreatePanelPrefab(entryPrefab);
        bool linkedScene = LinkBattleSceneManager();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("BattlePartyStatusPanel setup complete. BattleScene linked: " + linkedScene + ".");
    }

    private static GameObject CreateEntryPrefab()
    {
        GameObject root = new GameObject("BattlePartyStatusEntry", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(BattlePartyStatusEntryView));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(260f, 32f);

        Image rootImage = root.GetComponent<Image>();
        rootImage.color = new Color(0.012f, 0.040f, 0.055f, 0.92f);
        rootImage.raycastTarget = false;

        Outline rootOutline = root.AddComponent<Outline>();
        rootOutline.effectColor = new Color(0.10f, 0.86f, 1f, 0.28f);
        rootOutline.effectDistance = new Vector2(1f, -1f);

        CreateImage("Accent", root.transform, new Vector2(0.000f, 0.070f), new Vector2(0.015f, 0.930f), new Color(0.12f, 0.92f, 1f, 0.95f));
        Image frame = CreateImage("PortraitFrame", root.transform, new Vector2(0.030f, 0.035f), new Vector2(0.170f, 0.965f), new Color(0.64f, 1f, 1f, 0.96f));
        frame.type = Image.Type.Simple;
        CreateImage("PortraitImage", frame.transform, new Vector2(0.055f, 0.055f), new Vector2(0.945f, 0.945f), Color.white).preserveAspect = true;
        CreateText("NameText", root.transform, new Vector2(0.205f, 0.570f), new Vector2(0.585f, 0.930f), "CYBER WOLF", 9.4f, TextAlignmentOptions.MidlineLeft, new Color(0.88f, 1f, 1f, 1f));
        CreateText("HpText", root.transform, new Vector2(0.555f, 0.565f), new Vector2(0.960f, 0.930f), "126 / 126", 10.6f, TextAlignmentOptions.MidlineRight, Color.white);

        Image hpBack = CreateImage("HpBack", root.transform, new Vector2(0.205f, 0.130f), new Vector2(0.960f, 0.420f), new Color(0.00f, 0.026f, 0.036f, 0.96f));
        Image hpFill = CreateImage("HpFill", hpBack.transform, Vector2.zero, Vector2.one, new Color(0.18f, 0.96f, 0.55f, 1f));
        RectTransform hpFillRect = hpFill.transform as RectTransform;
        hpFillRect.offsetMin = new Vector2(2f, 2f);
        hpFillRect.offsetMax = new Vector2(-2f, -2f);
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        hpFill.fillOrigin = 0;
        hpFill.fillAmount = 1f;

        root.GetComponent<BattlePartyStatusEntryView>().CacheReferences();
        PrefabUtility.SaveAsPrefabAsset(root, EntryPrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        return AssetDatabase.LoadAssetAtPath<GameObject>(EntryPrefabPath);
    }

    private static void CreatePanelPrefab(GameObject entryPrefab)
    {
        Sprite wolfIcon = AssetDatabase.LoadAssetAtPath<Sprite>(WolfIconPath);
        Sprite armorIcon = AssetDatabase.LoadAssetAtPath<Sprite>(ArmorIconPath);
        Sprite blueIcon = AssetDatabase.LoadAssetAtPath<Sprite>(BlueIconPath);
        Sprite greenIcon = AssetDatabase.LoadAssetAtPath<Sprite>(GreenIconPath);
        Sprite purpleIcon = AssetDatabase.LoadAssetAtPath<Sprite>(PurpleIconPath);
        Sprite gogglesIcon = AssetDatabase.LoadAssetAtPath<Sprite>(GogglesIconPath);

        GameObject panel = new GameObject("BattlePartyStatusPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup), typeof(BattlePartyStatusPanelController));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.022f, 0.590f);
        panelRect.anchorMax = new Vector2(0.214f, 0.865f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        panelRect.sizeDelta = new Vector2(246f, 198f);

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.006f, 0.020f, 0.030f, 0.90f);
        panelImage.raycastTarget = false;

        Shadow panelShadow = panel.AddComponent<Shadow>();
        panelShadow.effectColor = new Color(0f, 0.85f, 1f, 0.18f);
        panelShadow.effectDistance = new Vector2(0f, -3f);

        Outline panelOutline = panel.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0.28f, 0.95f, 1f, 0.62f);
        panelOutline.effectDistance = new Vector2(1.6f, -1.6f);

        CreateImage("PanelTopLine", panel.transform, new Vector2(0.025f, 0.970f), new Vector2(0.975f, 0.988f), new Color(0.40f, 1f, 1f, 0.95f));
        CreateImage("PanelInner", panel.transform, new Vector2(0.025f, 0.030f), new Vector2(0.975f, 0.965f), new Color(0.010f, 0.028f, 0.040f, 0.54f));
        CreateImage("ActiveDivider", panel.transform, new Vector2(0.055f, 0.510f), new Vector2(0.945f, 0.523f), new Color(0.12f, 0.92f, 1f, 0.46f));
        CreateText("ActiveLabel", panel.transform, new Vector2(0.060f, 0.930f), new Vector2(0.520f, 0.990f), "ACTIVE", 10.8f, TextAlignmentOptions.MidlineLeft, new Color(0.90f, 1f, 1f, 1f));
        CreateText("ReserveLabel", panel.transform, new Vector2(0.060f, 0.474f), new Vector2(0.560f, 0.520f), "RESERVE", 10.2f, TextAlignmentOptions.MidlineLeft, new Color(0.70f, 0.88f, 0.94f, 0.92f));

        GameObject activeGroup = CreateRectObject("ActiveEntries", panel.transform, new Vector2(0.045f, 0.535f), new Vector2(0.955f, 0.920f), Vector2.zero, Vector2.zero);
        GameObject reserveGroup = CreateRectObject("ReserveEntries", panel.transform, new Vector2(0.045f, 0.055f), new Vector2(0.955f, 0.465f), Vector2.zero, Vector2.zero);

        for (int i = 0; i < 3; i++)
        {
            float maxY = 1f - i / 3f - 0.005f;
            float minY = 1f - (i + 1f) / 3f + 0.005f;
            InstantiateEntry(entryPrefab, activeGroup.transform, "ActiveEntry_" + (i + 1).ToString("00"), new Vector2(0f, minY), new Vector2(1f, maxY));
            InstantiateEntry(entryPrefab, reserveGroup.transform, "ReserveEntry_" + (i + 1).ToString("00"), new Vector2(0f, minY), new Vector2(1f, maxY));
        }

        BattlePartyStatusPanelController controller = panel.GetComponent<BattlePartyStatusPanelController>();
        controller.CacheReferences();
        controller.SetActiveMember(0, "Cyber Wolf", wolfIcon, 126, 126);
        controller.SetActiveMember(1, "Armor Ally", armorIcon, 142, 142);
        controller.SetActiveMember(2, "Blue Girl", blueIcon, 98, 98);
        controller.SetReserveMember(0, "Reserve Ally A", greenIcon, 110, 110);
        controller.SetReserveMember(1, "Reserve Ally B", purpleIcon, 104, 104);
        controller.SetReserveMember(2, "Reserve Ally C", gogglesIcon, 116, 116);
        EditorUtility.SetDirty(controller);

        PrefabUtility.SaveAsPrefabAsset(panel, PanelPrefabPath);
        UnityEngine.Object.DestroyImmediate(panel);
    }

    private static bool LinkBattleSceneManager()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!string.Equals(scene.path, BattleScenePath, StringComparison.Ordinal))
        {
            scene = EditorSceneManager.OpenScene(BattleScenePath);
        }

        GameObject panelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PanelPrefabPath);
        BattlePartyStatusPanelController panelController = panelPrefab != null ? panelPrefab.GetComponent<BattlePartyStatusPanelController>() : null;
        BattleManager battleManager = UnityEngine.Object.FindAnyObjectByType<BattleManager>();
        if (panelController == null || battleManager == null)
        {
            return false;
        }

        SerializedObject serializedManager = new SerializedObject(battleManager);
        serializedManager.FindProperty("battlePartyStatusPanelPrefab").objectReferenceValue = panelController;
        serializedManager.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(battleManager);
        EditorSceneManager.MarkSceneDirty(scene);
        return EditorSceneManager.SaveScene(scene);
    }

    private static GameObject InstantiateEntry(GameObject entryPrefab, Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject instance = PrefabUtility.InstantiatePrefab(entryPrefab) as GameObject;
        instance.name = name;
        instance.transform.SetParent(parent, false);
        SetStretch(instance.transform as RectTransform, anchorMin, anchorMax);
        return instance;
    }

    private static GameObject CreateRectObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
        return gameObject;
    }

    private static Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject gameObject = CreateRectObject(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        Image image = gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string text, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject gameObject = CreateRectObject(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        TextMeshProUGUI label = gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.enableAutoSizing = true;
        label.fontSizeMin = Mathf.Max(6f, fontSize - 3f);
        label.fontSizeMax = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.alignment = alignment;
        label.color = color;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.margin = Vector4.zero;
        label.raycastTarget = false;
        label.outlineWidth = 0.08f;
        label.outlineColor = new Color(0f, 0.02f, 0.05f, 0.95f);
        return label;
    }

    private static void SetStretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static void ConfigureSprite(string assetPath, int maxTextureSize)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = maxTextureSize;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    private static void EnsureFolderPath(string folderPath)
    {
        string[] parts = folderPath.Split('/');
        if (parts.Length == 0)
        {
            return;
        }

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
