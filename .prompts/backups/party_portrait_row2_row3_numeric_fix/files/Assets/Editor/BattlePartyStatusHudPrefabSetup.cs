using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BattlePartyStatusHudPrefabSetup
{
    private const string HudPrefabPath = "Assets/Prefabs/UI/BattlePartyStatusHUD.prefab";
    private const string BattleScenePath = "Assets/Scenes/BattleScene.unity";
    private const string AssetFolder = "Assets/Art/UI/Battle/PartyStatusHUD";

    private const string FramePath = AssetFolder + "/UI_BattlePartyHUD_Frame_9Slice.png";
    private const string RowFramePath = AssetFolder + "/UI_BattlePartyHUD_RowFrame.png";
    private const string LeftSlotFramePath = AssetFolder + "/UI_BattlePartyHUD_LeftSlotFrame.png";
    private const string HpBackPath = AssetFolder + "/UI_BattlePartyHUD_HPBar_Back.png";
    private const string HpFillPath = AssetFolder + "/UI_BattlePartyHUD_HPFill.png";
    private const string MpBackPath = AssetFolder + "/UI_BattlePartyHUD_MPBar_Back.png";
    private const string MpFillPath = AssetFolder + "/UI_BattlePartyHUD_MPFill.png";
    private const string SelectedHighlightPath = AssetFolder + "/UI_BattlePartyHUD_SelectedHighlight.png";
    private const string SlashDividerPath = AssetFolder + "/UI_BattlePartyHUD_SlashDivider.png";
    private const string PortraitFramePath = AssetFolder + "/UI_BattlePartyHUD_PortraitFrame.png";
    private const string PortraitClipMaskPath = AssetFolder + "/UI_BattleHUD_PortraitClipMask.png";
    private const string PortraitMaskPath = AssetFolder + "/UI_BattlePartyHUD_PortraitMask.png";
    private const string Portrait01Path = AssetFolder + "/UI_BattlePartyHUD_Portrait_01.png";
    private const string Portrait02Path = AssetFolder + "/UI_BattlePartyHUD_Portrait_02.png";
    private const string Portrait03Path = AssetFolder + "/UI_BattlePartyHUD_Portrait_03.png";
    private const string CleanPortraitWolfPath = AssetFolder + "/UI_BattlePartyHUD_FaceClean_Wolf.png";
    private const string CleanPortraitMechPath = AssetFolder + "/UI_BattlePartyHUD_FaceClean_Mech.png";
    private const string CleanPortraitGirlPath = AssetFolder + "/UI_BattlePartyHUD_FaceClean_Girl.png";

    [MenuItem("Tools/NeonCardia/Apply Battle Party Status HUD Prefab Setup")]
    public static void Apply()
    {
        EnsureFolderPath("Assets/Prefabs/UI");
        EnsureFolderPath(AssetFolder);

        ConfigureSprite(FramePath, 2048, Vector4.zero);
        ConfigureSprite(RowFramePath, 2048, new Vector4(42f, 14f, 42f, 14f));
        ConfigureSprite(LeftSlotFramePath, 1024, new Vector4(36f, 36f, 36f, 36f));
        ConfigureSprite(SelectedHighlightPath, 2048, new Vector4(42f, 14f, 42f, 14f));
        ConfigureSprite(HpBackPath, 2048, new Vector4(36f, 12f, 36f, 12f));
        ConfigureSprite(HpFillPath, 2048, Vector4.zero);
        ConfigureSprite(MpBackPath, 2048, new Vector4(36f, 10f, 36f, 10f));
        ConfigureSprite(MpFillPath, 2048, Vector4.zero);
        ConfigureSprite(SlashDividerPath, 2048, Vector4.zero);
        ConfigureSprite(PortraitFramePath, 2048, new Vector4(20f, 10f, 20f, 10f));
        ConfigureSprite(PortraitClipMaskPath, 1024, Vector4.zero, SpriteMeshType.Tight);
        ConfigureSprite(PortraitMaskPath, 2048, Vector4.zero);
        ConfigureSprite(Portrait01Path, 1024, Vector4.zero);
        ConfigureSprite(Portrait02Path, 1024, Vector4.zero);
        ConfigureSprite(Portrait03Path, 1024, Vector4.zero);
        ConfigureSprite(CleanPortraitWolfPath, 2048, Vector4.zero);
        ConfigureSprite(CleanPortraitMechPath, 2048, Vector4.zero);
        ConfigureSprite(CleanPortraitGirlPath, 2048, Vector4.zero);

        CreateHudPrefab();
        bool linkedScene = LinkBattleSceneManager();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("BattlePartyStatusHUD prefab setup complete. BattleScene linked: " + linkedScene + ".");
    }

    [MenuItem("Tools/NeonCardia/Capture Battle Party Status HUD FHD Check")]
    public static void CaptureFhdCheck()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
        if (prefab == null)
        {
            Debug.LogError("BattlePartyStatusHUD prefab was not found at " + HudPrefabPath + ".");
            return;
        }

        string outputPath = System.IO.Path.GetFullPath("Screenshots/battle_party_status_hud_unity_render.png");
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath));

        GameObject cameraObject = new GameObject("Temp_BattlePartyStatusHUD_RenderCamera");
        GameObject canvasObject = new GameObject("Temp_BattlePartyStatusHUD_FHDCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        RenderTexture renderTexture = null;
        Texture2D outputTexture = null;
        RenderTexture previous = RenderTexture.active;

        try
        {
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.015f, 0.018f, 0.024f, 1f);
            camera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            camera.orthographic = true;
            camera.orthographicSize = 540f;
            camera.aspect = 16f / 9f;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = 100f;
            camera.transform.position = new Vector3(0f, 0f, -10f);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.pixelPerfect = true;
            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(1920f, 1080f);
            canvasRect.position = Vector3.zero;
            canvasRect.localScale = Vector3.one;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            Image background = CreateImage("Temp_Background", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, null, new Color(0.015f, 0.018f, 0.024f, 1f));
            background.raycastTarget = false;

            GameObject hudInstance = UnityEngine.Object.Instantiate(prefab, canvasObject.transform, false);
            hudInstance.name = "BattlePartyStatusHUD_VerificationInstance";
            SetLayerRecursively(canvasObject, LayerMask.NameToLayer("UI"));

            BattlePartyStatusHUD hud = hudInstance.GetComponent<BattlePartyStatusHUD>();
            if (hud != null)
            {
                hud.RefreshPreviewMembers();
            }

            Canvas.ForceUpdateCanvases();
            foreach (UIPortraitCoverCrop crop in hudInstance.GetComponentsInChildren<UIPortraitCoverCrop>(true))
            {
                crop.Apply();
            }

            Canvas.ForceUpdateCanvases();

            renderTexture = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = renderTexture;
            Canvas.ForceUpdateCanvases();
            camera.Render();
            RenderTexture.active = renderTexture;

            outputTexture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
            outputTexture.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            outputTexture.Apply();
            System.IO.File.WriteAllBytes(outputPath, outputTexture.EncodeToPNG());
            camera.targetTexture = null;
            Debug.Log("BattlePartyStatusHUD FHD check screenshot saved: " + outputPath);
        }
        finally
        {
            RenderTexture.active = previous;
            Camera camera = cameraObject != null ? cameraObject.GetComponent<Camera>() : null;
            if (camera != null)
            {
                camera.targetTexture = null;
            }

            if (outputTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(outputTexture);
            }

            if (renderTexture != null)
            {
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }

            UnityEngine.Object.DestroyImmediate(canvasObject);
            UnityEngine.Object.DestroyImmediate(cameraObject);
        }
    }

    private static void CreateHudPrefab()
    {
        Sprite frame = LoadSprite(FramePath);
        Sprite rowFrame = LoadSprite(RowFramePath);
        Sprite hpBack = LoadSprite(HpBackPath);
        Sprite hpFill = LoadSprite(HpFillPath);
        Sprite mpBack = LoadSprite(MpBackPath);
        Sprite mpFill = LoadSprite(MpFillPath);
        Sprite selectedHighlight = LoadSprite(SelectedHighlightPath);
        Sprite slashDivider = LoadSprite(SlashDividerPath);
        Sprite portraitFrame = LoadSprite(PortraitFramePath);
        Sprite portraitClipMask = LoadSprite(PortraitClipMaskPath);
        Sprite[] portraits =
        {
            LoadSprite(CleanPortraitWolfPath),
            LoadSprite(CleanPortraitMechPath),
            LoadSprite(CleanPortraitGirlPath)
        };

        GameObject hud = new GameObject("BattlePartyStatusHUD_Compact", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(BattlePartyStatusHUD));
        RectTransform hudRect = hud.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0.027f, 0.755f);
        hudRect.anchorMax = new Vector2(0.246f, 0.875f);
        hudRect.pivot = new Vector2(0f, 1f);
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;
        hudRect.sizeDelta = Vector2.zero;

        Image hudImage = hud.GetComponent<Image>();
        hudImage.sprite = frame;
        hudImage.type = Image.Type.Simple;
        hudImage.color = new Color(1f, 1f, 1f, 0f);
        hudImage.raycastTarget = false;

        GameObject rowsRoot = CreateRectObject("Rows", hud.transform, Vector2.zero, Vector2.one, new Vector2(4f, 3f), new Vector2(-4f, -3f));

        string[] names = { "Cyber Wolf", "Armor Ally", "Blue Girl" };
        int[] currentHp = { 125, 150, 110 };
        int[] maxHp = { 125, 150, 110 };
        int[] currentMp = { 58, 48, 68 };
        int[] maxMp = { 100, 100, 100 };
        float[] portraitZoom = { 1.00f, 1.00f, 1.00f };
        Vector2[] portraitOffset = { Vector2.zero, Vector2.zero, Vector2.zero };
        Vector2 portraitOverscan = new Vector2(4f, 2f);

        for (int i = 0; i < 3; i++)
        {
            float minY = 1f - (i + 1f) / 3f + 0.012f;
            float maxY = 1f - i / 3f - 0.012f;
            GameObject row = CreateRectObject("PartyMemberStatusRow_" + (i + 1).ToString("00"), rowsRoot.transform, new Vector2(0f, minY), new Vector2(1f, maxY), Vector2.zero, Vector2.zero);
            PartyMemberStatusRowView rowView = row.AddComponent<PartyMemberStatusRowView>();

            Image frameImage = CreateImage("RowFrame_Image", row.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, rowFrame, Color.white);
            frameImage.type = Image.Type.Sliced;

            Image selectedImage = CreateImage("SelectedHighlight_Image", row.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, selectedHighlight, Color.white);
            selectedImage.type = Image.Type.Sliced;
            if (i == 1)
            {
                selectedImage.rectTransform.anchorMax = new Vector2(0.600f, 1f);
                selectedImage.rectTransform.offsetMin = Vector2.zero;
                selectedImage.rectTransform.offsetMax = Vector2.zero;
                selectedImage.color = new Color(1f, 1f, 1f, 0f);
            }

            selectedImage.gameObject.SetActive(i == 1);

            TextMeshProUGUI hpValue = CreateText("HPValue_TMP", row.transform, new Vector2(0.120f, 0.155f), new Vector2(0.565f, 0.845f), Vector2.zero, Vector2.zero, currentHp[i] + "/" + maxHp[i], 18f, TextAlignmentOptions.MidlineRight, Color.white);
            hpValue.fontSizeMin = 14f;

            Image hpBackImage = CreateImage("HPBar_Background", row.transform, new Vector2(0.205f, 0.430f), new Vector2(0.545f, 0.585f), Vector2.zero, Vector2.zero, hpBack, Color.white);
            hpBackImage.type = Image.Type.Sliced;
            Image hp = CreateImage("HPBar_Fill", hpBackImage.transform, Vector2.zero, Vector2.one, new Vector2(3f, 2f), new Vector2(-3f, -2f), hpFill, Color.white);
            hp.type = Image.Type.Filled;
            hp.fillMethod = Image.FillMethod.Horizontal;
            hp.fillOrigin = 0;
            hp.fillAmount = (float)currentHp[i] / maxHp[i];

            Image mpBackImage = CreateImage("MPBar_Background", row.transform, new Vector2(0.205f, 0.245f), new Vector2(0.505f, 0.385f), Vector2.zero, Vector2.zero, mpBack, Color.white);
            mpBackImage.type = Image.Type.Sliced;
            Image mp = CreateImage("MPBar_Fill", mpBackImage.transform, Vector2.zero, Vector2.one, new Vector2(3f, 2f), new Vector2(-3f, -2f), mpFill, Color.white);
            mp.type = Image.Type.Filled;
            mp.fillMethod = Image.FillMethod.Horizontal;
            mp.fillOrigin = 0;
            mp.fillAmount = (float)currentMp[i] / maxMp[i];

            GameObject portraitArea = CreateRectObject("PortraitArea", row.transform, new Vector2(0.600f, 0.030f), new Vector2(0.980f, 0.970f), Vector2.zero, Vector2.zero);

            Image clipMaskImage = CreateImage("PortraitClipMask", portraitArea.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, portraitClipMask, Color.white);
            clipMaskImage.type = Image.Type.Simple;
            clipMaskImage.useSpriteMesh = true;
            clipMaskImage.maskable = true;
            clipMaskImage.raycastTarget = false;
            Mask mask = clipMaskImage.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            Image portrait = CreateImage("PortraitImage", clipMaskImage.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, portraits[i], Color.white);
            portrait.maskable = true;
            portrait.material = null;
            portrait.preserveAspect = true;

            UIPortraitCoverCrop coverCrop = portraitArea.AddComponent<UIPortraitCoverCrop>();
            coverCrop.Configure(clipMaskImage.rectTransform, portrait, portraitZoom[i], portraitOverscan, portraitOffset[i]);

            Image portraitFrameImage = CreateImage("PortraitFrame_Image", portraitArea.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, portraitFrame, Color.white);
            portraitFrameImage.type = Image.Type.Simple;
            portraitFrameImage.raycastTarget = false;
            portraitFrameImage.gameObject.SetActive(false);

            Image divider = CreateImage("SlashDivider_Image", portraitArea.transform, new Vector2(-0.150f, -0.240f), new Vector2(0.160f, 1.240f), Vector2.zero, Vector2.zero, slashDivider, Color.white);
            divider.preserveAspect = false;
            divider.gameObject.SetActive(false);

            CreateText("NameText_TMP", row.transform, new Vector2(0.135f, 0.635f), new Vector2(0.500f, 0.950f), Vector2.zero, Vector2.zero, names[i].ToUpperInvariant(), 20f, TextAlignmentOptions.MidlineLeft, new Color(0.92f, 0.98f, 1f, 1f)).gameObject.SetActive(false);
            CreateText("HPLabel_TMP", row.transform, new Vector2(0.145f, 0.395f), new Vector2(0.195f, 0.585f), Vector2.zero, Vector2.zero, "HP", 12f, TextAlignmentOptions.MidlineLeft, new Color(0.70f, 1f, 0.88f, 1f)).gameObject.SetActive(false);
            CreateText("MPLabel_TMP", row.transform, new Vector2(0.145f, 0.210f), new Vector2(0.195f, 0.385f), Vector2.zero, Vector2.zero, "MP", 12f, TextAlignmentOptions.MidlineLeft, new Color(0.68f, 0.78f, 1f, 1f)).gameObject.SetActive(false);
            CreateText("MPValue_TMP", row.transform, new Vector2(0.480f, 0.185f), new Vector2(0.598f, 0.395f), Vector2.zero, Vector2.zero, currentMp[i] + "/" + maxMp[i], 13f, TextAlignmentOptions.MidlineRight, new Color(0.86f, 0.91f, 1f, 1f)).gameObject.SetActive(false);
            hp.gameObject.SetActive(false);
            mp.gameObject.SetActive(false);
            hpBackImage.gameObject.SetActive(false);
            mpBackImage.gameObject.SetActive(false);

            rowView.CacheReferences();
            rowView.SetStatus(names[i], portraits[i], currentHp[i], maxHp[i], currentMp[i], maxMp[i], i == 1);
            EditorUtility.SetDirty(rowView);
        }

        BattlePartyStatusHUD hudView = hud.GetComponent<BattlePartyStatusHUD>();
        hudView.CacheReferences();
        ConfigurePreviewMembers(hudView, names, portraits, currentHp, maxHp, currentMp, maxMp);
        hudView.SetSelectedIndex(1);
        EditorUtility.SetDirty(hudView);

        PrefabUtility.SaveAsPrefabAsset(hud, HudPrefabPath);
        UnityEngine.Object.DestroyImmediate(hud);
    }

    private static bool LinkBattleSceneManager()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!string.Equals(scene.path, BattleScenePath, StringComparison.Ordinal))
        {
            scene = EditorSceneManager.OpenScene(BattleScenePath);
        }

        GameObject prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
        BattlePartyStatusHUD hudPrefab = prefabObject != null ? prefabObject.GetComponent<BattlePartyStatusHUD>() : null;
        BattleManager battleManager = UnityEngine.Object.FindFirstObjectByType<BattleManager>();
        if (hudPrefab == null || battleManager == null)
        {
            return false;
        }

        SerializedObject serializedManager = new SerializedObject(battleManager);
        SerializedProperty prefabProperty = serializedManager.FindProperty("battlePartyStatusHudPrefab");
        if (prefabProperty == null)
        {
            return false;
        }

        prefabProperty.objectReferenceValue = hudPrefab;
        serializedManager.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(battleManager);
        EditorSceneManager.MarkSceneDirty(scene);
        return EditorSceneManager.SaveScene(scene);
    }

    private static void ConfigurePreviewMembers(BattlePartyStatusHUD hudView, string[] names, Sprite[] portraits, int[] currentHp, int[] maxHp, int[] currentMp, int[] maxMp)
    {
        SerializedObject serializedHud = new SerializedObject(hudView);
        SerializedProperty selectedIndexProperty = serializedHud.FindProperty("selectedIndex");
        if (selectedIndexProperty != null)
        {
            selectedIndexProperty.intValue = 1;
        }

        SerializedProperty membersProperty = serializedHud.FindProperty("previewMembers");
        if (membersProperty == null)
        {
            serializedHud.ApplyModifiedPropertiesWithoutUndo();
            return;
        }

        membersProperty.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            SerializedProperty memberProperty = membersProperty.GetArrayElementAtIndex(i);
            memberProperty.FindPropertyRelative("DisplayName").stringValue = names[i];
            memberProperty.FindPropertyRelative("Portrait").objectReferenceValue = portraits[i];
            memberProperty.FindPropertyRelative("CurrentHp").intValue = currentHp[i];
            memberProperty.FindPropertyRelative("MaxHp").intValue = maxHp[i];
            memberProperty.FindPropertyRelative("CurrentMp").intValue = currentMp[i];
            memberProperty.FindPropertyRelative("MaxMp").intValue = maxMp[i];
        }

        serializedHud.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Sprite sprite, Color color)
    {
        GameObject gameObject = CreateRectObject(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Image image = gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string text, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject gameObject = CreateRectObject(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        TextMeshProUGUI label = gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.enableAutoSizing = true;
        label.fontSizeMin = Mathf.Max(12f, fontSize - 8f);
        label.fontSizeMax = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.alignment = alignment;
        label.color = color;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.enableWordWrapping = false;
        label.margin = Vector4.zero;
        label.raycastTarget = false;
        label.outlineWidth = 0.08f;
        label.outlineColor = new Color(0f, 0f, 0f, 0.92f);
        return label;
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

    private static void SetLayerRecursively(GameObject gameObject, int layer)
    {
        if (gameObject == null || layer < 0)
        {
            return;
        }

        gameObject.layer = layer;
        Transform transform = gameObject.transform;
        for (int i = 0; i < transform.childCount; i++)
        {
            SetLayerRecursively(transform.GetChild(i).gameObject, layer);
        }
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void ConfigureSprite(string assetPath, int maxTextureSize, Vector4 spriteBorder)
    {
        ConfigureSprite(assetPath, maxTextureSize, spriteBorder, SpriteMeshType.FullRect);
    }

    private static void ConfigureSprite(string assetPath, int maxTextureSize, Vector4 spriteBorder, SpriteMeshType meshType)
    {
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
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
        importer.spritePixelsPerUnit = 100f;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = meshType;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        settings.spriteBorder = spriteBorder;
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
