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

    private const string HpPlatePath = AssetFolder + "/UI_BattlePartyHUD_SimpleRectHpPlate.png";
    private const string CleanPortraitWolfPath = AssetFolder + "/UI_BattlePartyHUD_FaceClean_Wolf.png";
    private const string CleanPortraitMechPath = AssetFolder + "/UI_BattlePartyHUD_FaceClean_Mech.png";
    private const string CleanPortraitGirlPath = AssetFolder + "/UI_BattlePartyHUD_FaceClean_Girl.png";
    private const string ReservePortraitGreenPath = "Assets/Art/UI/Battle/Party/FaceIcons/ReserveFaceIcon_Green.png";
    private const string ReservePortraitPurplePath = "Assets/Art/UI/Battle/Party/FaceIcons/ReserveFaceIcon_Purple.png";
    private const string ReservePortraitGogglesPath = "Assets/Art/UI/Battle/Party/FaceIcons/ReserveFaceIcon_Goggles.png";

    private static readonly Vector2 HudAnchorMin = new Vector2(0.027f, 0.650f);
    private static readonly Vector2 HudAnchorMax = new Vector2(0.210f, 0.875f);
    private static readonly Vector2 RowSize = new Vector2(318f, 29f);
    private static readonly Vector2 NamePlateSize = new Vector2(150f, 29f);
    private static readonly Vector2 HpPlateSize = new Vector2(58f, 29f);
    private static readonly Vector2 FaceIconSize = new Vector2(95f, 29f);
    private const float RowGap = 5f;
    private const float PanelGap = 10f;
    private const float NameHpGap = 4f;
    private const float HpFaceGap = 6f;

    [MenuItem("Tools/NeonCardia/Apply Battle Party Status HUD Prefab Setup")]
    public static void Apply()
    {
        EnsureFolderPath("Assets/Prefabs/UI");
        EnsureFolderPath(AssetFolder);

        WriteSimpleRectHpPlate(HpPlatePath);
        ConfigureSprite(HpPlatePath, 1024, new Vector4(6f, 6f, 6f, 6f));
        ConfigureSprite(CleanPortraitWolfPath, 2048, Vector4.zero);
        ConfigureSprite(CleanPortraitMechPath, 2048, Vector4.zero);
        ConfigureSprite(CleanPortraitGirlPath, 2048, Vector4.zero);
        ConfigureSprite(ReservePortraitGreenPath, 2048, Vector4.zero);
        ConfigureSprite(ReservePortraitPurplePath, 2048, Vector4.zero);
        ConfigureSprite(ReservePortraitGogglesPath, 2048, Vector4.zero);

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
        Sprite rowBackground = LoadSprite(HpPlatePath);
        Sprite[] activePortraits =
        {
            LoadSprite(CleanPortraitWolfPath),
            LoadSprite(CleanPortraitMechPath),
            LoadSprite(CleanPortraitGirlPath)
        };
        Sprite[] reservePortraits =
        {
            LoadSprite(ReservePortraitGreenPath),
            LoadSprite(ReservePortraitPurplePath),
            LoadSprite(ReservePortraitGogglesPath)
        };

        GameObject hud = new GameObject("BattlePartyStatusHUD_Compact", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(BattlePartyStatusHUD));
        RectTransform hudRect = hud.GetComponent<RectTransform>();
        hudRect.anchorMin = HudAnchorMin;
        hudRect.anchorMax = HudAnchorMax;
        hudRect.pivot = new Vector2(0f, 1f);
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;
        hudRect.sizeDelta = Vector2.zero;

        Image hudImage = hud.GetComponent<Image>();
        hudImage.sprite = null;
        hudImage.type = Image.Type.Simple;
        hudImage.color = new Color(1f, 1f, 1f, 0f);
        hudImage.raycastTarget = false;

        GameObject battleHudRoot = CreateRectObject("BattleHud", hud.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        float panelHeight = RowSize.y * 3f + RowGap * 2f;
        GameObject activePanel = CreateMemberPanel("ActiveMemberPanel", battleHudRoot.transform, 0f, panelHeight);
        GameObject reservePanel = CreateMemberPanel("ReserveMemberPanel", battleHudRoot.transform, -(panelHeight + PanelGap), panelHeight);

        string[] activeNames = { "Cyber Wolf", "Armor Ally", "Blue Girl" };
        int[] activeCurrentHp = { 125, 150, 110 };
        int[] activeMaxHp = { 125, 150, 110 };
        int[] activeCurrentMp = { 58, 48, 68 };
        int[] activeMaxMp = { 100, 100, 100 };
        string[] reserveNames = { "Reserve Ally A", "Reserve Ally B", "Reserve Ally C" };
        int[] reserveCurrentHp = { 110, 104, 116 };
        int[] reserveMaxHp = { 110, 104, 116 };
        int[] reserveCurrentMp = { 32, 30, 34 };
        int[] reserveMaxMp = { 100, 100, 100 };
        float[] activePortraitZoom = { 1.00f, 1.00f, 1.00f };
        float[] reservePortraitZoom = { 1.00f, 1.00f, 1.00f };
        Vector2[] portraitOffset = { Vector2.zero, Vector2.zero, Vector2.zero };
        Vector2 portraitOverscan = new Vector2(2f, 1f);

        for (int i = 0; i < 3; i++)
        {
            CreateMemberRow(
                activePanel.transform,
                i,
                rowBackground,
                activeNames[i],
                activePortraits[i],
                activeCurrentHp[i],
                activeMaxHp[i],
                activeCurrentMp[i],
                activeMaxMp[i],
                activePortraitZoom[i],
                portraitOverscan,
                portraitOffset[i],
                i == 1,
                false);

            CreateMemberRow(
                reservePanel.transform,
                i,
                rowBackground,
                reserveNames[i],
                reservePortraits[i],
                reserveCurrentHp[i],
                reserveMaxHp[i],
                reserveCurrentMp[i],
                reserveMaxMp[i],
                reservePortraitZoom[i],
                portraitOverscan,
                portraitOffset[i],
                false,
                true);
        }

        BattlePartyStatusHUD hudView = hud.GetComponent<BattlePartyStatusHUD>();
        hudView.CacheReferences();
        ConfigurePreviewMembers(
            hudView,
            activeNames,
            activePortraits,
            activeCurrentHp,
            activeMaxHp,
            activeCurrentMp,
            activeMaxMp,
            reserveNames,
            reservePortraits,
            reserveCurrentHp,
            reserveMaxHp,
            reserveCurrentMp,
            reserveMaxMp);
        hudView.SetSelectedIndex(1);
        EditorUtility.SetDirty(hudView);

        PrefabUtility.SaveAsPrefabAsset(hud, HudPrefabPath);
        UnityEngine.Object.DestroyImmediate(hud);
    }

    private static GameObject CreateMemberPanel(string panelName, Transform parent, float topOffsetY, float panelHeight)
    {
        GameObject panel = CreateFixedRectObject(panelName, parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, topOffsetY), new Vector2(RowSize.x, panelHeight));
        CanvasGroup canvasGroup = panel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        return panel;
    }

    private static void CreateMemberRow(
        Transform panel,
        int index,
        Sprite rowBackground,
        string displayName,
        Sprite portraitSprite,
        int currentHp,
        int maxHp,
        int currentMp,
        int maxMp,
        float portraitZoom,
        Vector2 portraitOverscan,
        Vector2 portraitOffset,
        bool selected,
        bool reserve)
    {
        float rowY = -index * (RowSize.y + RowGap);
        GameObject row = CreateFixedRectObject("MemberStatusRow_" + index, panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, rowY), RowSize);
        CanvasGroup rowCanvasGroup = row.AddComponent<CanvasGroup>();
        rowCanvasGroup.alpha = reserve ? 0.74f : 1f;
        rowCanvasGroup.interactable = false;
        rowCanvasGroup.blocksRaycasts = false;

        PartyMemberStatusRowView rowView = row.AddComponent<PartyMemberStatusRowView>();

        Color namePlateColor = reserve
            ? new Color(0.58f, 0.68f, 0.72f, 0.66f)
            : selected ? new Color(1f, 1f, 1f, 1f) : new Color(0.88f, 0.96f, 1f, 0.96f);
        Color hpPlateColor = reserve
            ? new Color(0.52f, 0.62f, 0.67f, 0.70f)
            : selected ? new Color(0.95f, 1f, 1f, 1f) : new Color(0.80f, 0.94f, 1f, 0.96f);

        Image namePlate = CreateFixedImage("NamePlate", row.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, NamePlateSize, rowBackground, namePlateColor);
        namePlate.type = Image.Type.Sliced;

        Image hpPlate = CreateFixedImage("HpPlate", row.transform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(NamePlateSize.x + NameHpGap, 0f), HpPlateSize, rowBackground, hpPlateColor);
        hpPlate.type = Image.Type.Sliced;

        TextMeshProUGUI nameText = CreateText(
            "NameText",
            namePlate.transform,
            Vector2.zero,
            Vector2.one,
            new Vector2(9f, 0f),
            new Vector2(-6f, 0f),
            displayName,
            14f,
            TextAlignmentOptions.MidlineLeft,
            reserve ? new Color(0.68f, 0.82f, 0.86f, 0.92f) : new Color(0.86f, 0.98f, 1f, 1f));
        nameText.fontSizeMin = 10f;
        nameText.fontSizeMax = 14f;

        TextMeshProUGUI hpText = CreateText(
            "HpText",
            hpPlate.transform,
            Vector2.zero,
            Vector2.one,
            new Vector2(5f, 0f),
            new Vector2(-7f, 0f),
            currentHp.ToString(),
            16f,
            TextAlignmentOptions.MidlineRight,
            reserve ? new Color(0.76f, 0.90f, 0.94f, 0.94f) : new Color(0.92f, 0.98f, 1f, 1f));
        hpText.fontSizeMin = 13f;
        hpText.fontSizeMax = 16f;

        Image faceFrame = CreateFixedImage(
            "FaceIconRoot",
            row.transform,
            new Vector2(0f, 0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(NamePlateSize.x + NameHpGap + HpPlateSize.x + HpFaceGap, 0f),
            FaceIconSize,
            rowBackground,
            reserve ? new Color(0.42f, 0.50f, 0.54f, 0.62f) : new Color(0.78f, 0.94f, 1f, 0.82f));
        faceFrame.type = Image.Type.Sliced;
        faceFrame.maskable = false;

        Image clipMaskImage = CreateImage("PortraitClipMask", faceFrame.transform, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f), null, Color.white);
        clipMaskImage.type = Image.Type.Simple;
        clipMaskImage.useSpriteMesh = false;
        clipMaskImage.maskable = true;
        clipMaskImage.raycastTarget = false;
        Mask mask = clipMaskImage.gameObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        Image portrait = CreateImage(
            "FaceIconImage",
            clipMaskImage.transform,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero,
            portraitSprite,
            reserve ? new Color(0.72f, 0.82f, 0.86f, 0.74f) : Color.white);
        portrait.maskable = true;
        portrait.material = null;
        portrait.preserveAspect = true;

        UIPortraitCoverCrop coverCrop = faceFrame.gameObject.AddComponent<UIPortraitCoverCrop>();
        coverCrop.Configure(clipMaskImage.rectTransform, portrait, portraitZoom, portraitOverscan, portraitOffset);

        rowView.CacheReferences();
        rowView.SetStatus(displayName, portraitSprite, currentHp, maxHp, currentMp, maxMp, selected, reserve);
        EditorUtility.SetDirty(rowView);
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

    private static void ConfigurePreviewMembers(
        BattlePartyStatusHUD hudView,
        string[] activeNames,
        Sprite[] activePortraits,
        int[] activeCurrentHp,
        int[] activeMaxHp,
        int[] activeCurrentMp,
        int[] activeMaxMp,
        string[] reserveNames,
        Sprite[] reservePortraits,
        int[] reserveCurrentHp,
        int[] reserveMaxHp,
        int[] reserveCurrentMp,
        int[] reserveMaxMp)
    {
        SerializedObject serializedHud = new SerializedObject(hudView);
        SerializedProperty selectedIndexProperty = serializedHud.FindProperty("selectedIndex");
        if (selectedIndexProperty != null)
        {
            selectedIndexProperty.intValue = 1;
        }

        WritePreviewMemberArray(serializedHud.FindProperty("previewActiveMembers"), activeNames, activePortraits, activeCurrentHp, activeMaxHp, activeCurrentMp, activeMaxMp);
        WritePreviewMemberArray(serializedHud.FindProperty("previewReserveMembers"), reserveNames, reservePortraits, reserveCurrentHp, reserveMaxHp, reserveCurrentMp, reserveMaxMp);

        serializedHud.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void WritePreviewMemberArray(SerializedProperty membersProperty, string[] names, Sprite[] portraits, int[] currentHp, int[] maxHp, int[] currentMp, int[] maxMp)
    {
        if (membersProperty == null)
        {
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
    }

    private static void WriteSimpleRectHpPlate(string assetPath)
    {
        int width = Mathf.RoundToInt(NamePlateSize.x);
        int height = Mathf.RoundToInt(NamePlateSize.y);
        const float radius = 3.5f;

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color leftFill = new Color(0.018f, 0.034f, 0.052f, 0.94f);
        Color rightFill = new Color(0.046f, 0.086f, 0.120f, 0.88f);
        Color border = new Color(0.70f, 0.88f, 1f, 0.74f);
        Color innerLine = new Color(0.58f, 0.92f, 1f, 0.22f);
        Color softGlow = new Color(0.34f, 0.72f, 1f, 0.18f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float distance = RoundedRectSignedDistance(x + 0.5f, y + 0.5f, width, height, radius);
                Color pixel = Color.clear;

                if (distance <= 0.75f)
                {
                    float horizontal = (float)x / (width - 1);
                    float edgeAlpha = Mathf.Clamp01(0.75f - distance);
                    pixel = Color.Lerp(leftFill, rightFill, horizontal);
                    pixel.a *= edgeAlpha;

                    if (distance > -1.35f)
                    {
                        pixel = AlphaBlend(pixel, border);
                    }

                    bool topInner = y == height - 4 && x > 8 && x < width - 8;
                    bool bottomInner = y == 3 && x > 8 && x < width - 8;
                    if (topInner || bottomInner)
                    {
                        pixel = AlphaBlend(pixel, innerLine);
                    }
                }
                else if (distance <= 2.5f)
                {
                    float glowAlpha = Mathf.Clamp01((2.5f - distance) / 2.5f);
                    pixel = softGlow;
                    pixel.a *= glowAlpha;
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        string fullPath = System.IO.Path.GetFullPath(assetPath);
        string directory = System.IO.Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        System.IO.File.WriteAllBytes(fullPath, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
    }

    private static float RoundedRectSignedDistance(float px, float py, float width, float height, float radius)
    {
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;
        float qx = Mathf.Abs(px - halfWidth) - (halfWidth - radius);
        float qy = Mathf.Abs(py - halfHeight) - (halfHeight - radius);
        float outsideX = Mathf.Max(qx, 0f);
        float outsideY = Mathf.Max(qy, 0f);
        float outside = Mathf.Sqrt(outsideX * outsideX + outsideY * outsideY);
        float inside = Mathf.Min(Mathf.Max(qx, qy), 0f);
        return inside + outside - radius;
    }

    private static Color AlphaBlend(Color bottom, Color top)
    {
        float outputAlpha = top.a + bottom.a * (1f - top.a);
        if (outputAlpha <= 0.0001f)
        {
            return Color.clear;
        }

        float red = (top.r * top.a + bottom.r * bottom.a * (1f - top.a)) / outputAlpha;
        float green = (top.g * top.a + bottom.g * bottom.a * (1f - top.a)) / outputAlpha;
        float blue = (top.b * top.a + bottom.b * bottom.a * (1f - top.a)) / outputAlpha;
        return new Color(red, green, blue, outputAlpha);
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

    private static Image CreateFixedImage(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, Sprite sprite, Color color)
    {
        GameObject gameObject = CreateFixedRectObject(name, parent, anchor, pivot, anchoredPosition, sizeDelta);
        Image image = gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string text, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject gameObject = CreateRectObject(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        return ConfigureTextObject(gameObject, text, fontSize, alignment, color);
    }

    private static TextMeshProUGUI CreateFixedText(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, string text, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject gameObject = CreateFixedRectObject(name, parent, anchor, pivot, anchoredPosition, sizeDelta);
        return ConfigureTextObject(gameObject, text, fontSize, alignment, color);
    }

    private static TextMeshProUGUI ConfigureTextObject(GameObject gameObject, string text, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        TextMeshProUGUI label = gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.enableAutoSizing = true;
        label.fontSizeMin = Mathf.Max(8f, fontSize - 5f);
        label.fontSizeMax = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.alignment = alignment;
        label.color = color;
        label.overflowMode = TextOverflowModes.Truncate;
        label.textWrappingMode = TextWrappingModes.NoWrap;
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

    private static GameObject CreateFixedRectObject(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject gameObject = CreateRectObject(name, parent, anchor, anchor, Vector2.zero, Vector2.zero);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
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
