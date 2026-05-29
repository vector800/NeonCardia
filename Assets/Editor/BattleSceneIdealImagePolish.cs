using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BattleSceneIdealImagePolish
{
    private const string BattleScenePath = "Assets/Scenes/BattleScene.unity";
    private const string HudPrefabPath = "Assets/Prefabs/UI/BattleTimelineHud.prefab";
    private const string GridPrefabPath = "Assets/Art/BattleField/NeonGrid/Prefabs/PF_BattleGrid_Full_3x6_Image2_Bottom.prefab";
    private const string GridObjectName = "PF_BattleGrid_Full_3x6_Image2_Bottom";

    private const string HudFramePath = "Assets/Art/UI/BattleTimelineHud/Backgrounds/UI_BattleTopHudFrame_CyberCleanGenerated_Filled.png";
    private const string CurrentHpPanelSquarePath = "Assets/Art/UI/BattleTimelineHud/Panels/UI_CurrentHpPanel_SquareGenerated.png";
    private const string CurrentHpGaugeBackPath = "Assets/Art/UI/BattleTimelineHud/Panels/UI_CurrentHpGauge_Back.png";
    private const string CurrentHpGaugeFillPath = "Assets/Art/UI/BattleTimelineHud/Panels/UI_CurrentHpGauge_Fill.png";
    private const string ProgressRailPath = "Assets/Art/UI/BattleTimelineHud/Decorations/UI_TimelineProgressRail_Bright.png";
    private const string CurrentCardPath = "Assets/Art/UI/BattleTimelineHud/Cards/UI_TimelineCard_Current.png";
    private const string AllyCardPath = "Assets/Art/UI/BattleTimelineHud/Cards/UI_TimelineCard_Ally.png";
    private const string EnemyCardPath = "Assets/Art/UI/BattleTimelineHud/Cards/UI_TimelineCard_Enemy.png";
    private const string MarkerPath = "Assets/Art/UI/BattleTimelineHud/Decorations/UI_TimelineMarker_Current.png";
    private const string ArrowPath = "Assets/Art/UI/BattleTimelineHud/Decorations/UI_TimelineArrowRight.png";
    private const float HudAnchorMinX = 0.012f;
    private const float HudAnchorMaxX = 0.865f;
    private const float HudAnchorMinY = 0.728f;
    private const float HudAnchorMaxY = 0.982f;

    [MenuItem("Tools/NeonCardia/Apply Ideal Battle Scene Visual Polish")]
    public static void Apply()
    {
        EnsureHudSprites();
        ApplyHudPrefabPolish();
        ApplyGridPrefabPolish();
        ApplyBattleSceneComposition();
        AssetDatabase.SaveAssets();
        Debug.Log("Applied ideal BattleScene visual polish: compact top HUD, square HP panel, right-side enemy name space, grid positioning, and HP anchors.");
    }

    private static void EnsureHudSprites()
    {
        EnsureFolder("Assets/Art/UI/BattleTimelineHud", "Backgrounds");
        EnsureFolder("Assets/Art/UI/BattleTimelineHud", "Panels");
        EnsureFolder("Assets/Art/UI/BattleTimelineHud", "Decorations");
        EnsureFolder("Assets/Art/UI/BattleTimelineHud", "Cards");

        // These frame assets are imagegen-produced; keep them intact so Apply does not flatten the metallic shells.
        WriteGaugeBack(CurrentHpGaugeBackPath);
        WriteGaugeFill(CurrentHpGaugeFillPath);
        WriteProgressRail(ProgressRailPath);
        ConfigureSprite(HudFramePath, new Vector4(330f, 42f, 120f, 42f));
        ConfigureSprite(CurrentHpPanelSquarePath, Vector4.zero);
        ConfigureSprite(CurrentHpGaugeBackPath, new Vector4(18f, 12f, 18f, 12f));
        ConfigureSprite(CurrentHpGaugeFillPath, Vector4.zero);
        ConfigureSprite(ProgressRailPath, new Vector4(18f, 8f, 18f, 8f));
        ConfigureSprite(CurrentCardPath, new Vector4(34f, 34f, 34f, 34f));
        ConfigureSprite(AllyCardPath, new Vector4(30f, 30f, 30f, 30f));
        ConfigureSprite(EnemyCardPath, new Vector4(30f, 30f, 30f, 30f));
    }

    private static void ApplyHudPrefabPolish()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(HudPrefabPath);
        try
        {
            RemoveChildrenByName(root.transform, "CurrentTab");
            RemoveChildrenByName(root.transform, "RailDot");
            RemoveChildrenByName(root.transform, "CurrentHpIcon");
            RemoveChildrenByName(root.transform, "CurrentHpGaugeFlash");

            ConfigureImage(root.GetComponent<Image>(), HudFramePath, Image.Type.Sliced, Color.white);
            ConfigureHudLayout(root.transform);
            HideHpTimelineGapMask(root.transform);

            Transform leftPanel = root.transform.Find("LeftPanel");
            if (leftPanel != null)
            {
                ClearImage(leftPanel.GetComponent<Image>());
                ConfigureCurrentHpPanelImage(leftPanel);
                ConfigureLeftPanelText(leftPanel);
                ConfigureCurrentHpGauge(leftPanel);
            }

            ConfigureSlots(root.transform);
            ConfigureImage(FindImage(root.transform, "ConnectorLine"), ProgressRailPath, Image.Type.Sliced, Color.white);
            Image currentMarker = FindImage(root.transform, "CurrentMarker");
            ConfigureImage(currentMarker, MarkerPath, Image.Type.Simple, Color.white);
            if (currentMarker != null)
            {
                currentMarker.gameObject.SetActive(false);
                EditorUtility.SetDirty(currentMarker.gameObject);
            }
            ClearImage(FindImage(root.transform, "RightArrow"));

            BattleTimelineHudView hudView = root.GetComponent<BattleTimelineHudView>();
            if (hudView != null)
            {
                hudView.CacheReferences();
                EditorUtility.SetDirty(hudView);
            }

            PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ConfigureHudLayout(Transform root)
    {
        RectTransform hudRect = root as RectTransform;
        SetStretch(hudRect, new Vector2(HudAnchorMinX, HudAnchorMinY), new Vector2(HudAnchorMaxX, HudAnchorMaxY));

        RectTransform leftPanel = root.Find("LeftPanel") as RectTransform;
        SetStretch(leftPanel, new Vector2(0.047f, 0.257f), new Vector2(0.194f, 0.743f));

        RectTransform actionOrderText = root.Find("LeftPanel/ActionOrderText") as RectTransform;
        SetStretch(actionOrderText, new Vector2(0.500f, 0.500f), new Vector2(0.500f, 0.500f));

        RectTransform currentHpLabel = root.Find("LeftPanel/CurrentHpLabel") as RectTransform;
        SetStretch(currentHpLabel, new Vector2(0.500f, 0.500f), new Vector2(0.500f, 0.500f));

        RectTransform currentHpValue = root.Find("LeftPanel/CurrentHpValue") as RectTransform;
        SetStretch(currentHpValue, new Vector2(0.185735f, 0.280f), new Vector2(0.770735f, 0.860f));

        RectTransform slotsRoot = root.Find("SlotsRoot") as RectTransform;
        SetStretch(slotsRoot, new Vector2(0.165f, 0.105f), new Vector2(0.930f, 0.925f));

        RectTransform connectorLine = root.Find("ConnectorLine") as RectTransform;
        SetStretch(connectorLine, new Vector2(0.215f, 0.070f), new Vector2(0.905f, 0.105f));

        RectTransform currentMarker = root.Find("CurrentMarker") as RectTransform;
        SetStretch(currentMarker, new Vector2(0.178f, 0.000f), new Vector2(0.225f, 0.080f));

        RectTransform rightArrow = root.Find("RightArrow") as RectTransform;
        SetStretch(rightArrow, new Vector2(0.940f, 0.390f), new Vector2(0.985f, 0.610f));
    }

    private static void HideHpTimelineGapMask(Transform root)
    {
        Image mask = EnsureImage(root, "HpTimelineGapMask");
        ClearImage(mask);
        mask.gameObject.SetActive(false);
        mask.transform.SetAsFirstSibling();
        SetStretch(mask.transform as RectTransform, new Vector2(0.130f, 0.255f), new Vector2(0.164f, 0.745f));
        EditorUtility.SetDirty(mask.gameObject);
    }

    private static void ConfigureLeftPanelText(Transform leftPanel)
    {
        Text title = FindText(leftPanel, "ActionOrderText");
        if (title != null)
        {
            HideText(title);
        }

        Text label = FindText(leftPanel, "CurrentHpLabel");
        if (label != null)
        {
            HideText(label);
        }

        Text value = FindText(leftPanel, "CurrentHpValue");
        if (value != null)
        {
            value.gameObject.SetActive(true);
            value.fontSize = 30;
            value.resizeTextForBestFit = true;
            value.resizeTextMinSize = 18;
            value.resizeTextMaxSize = 30;
            value.fontStyle = FontStyle.Bold;
            value.alignment = TextAnchor.MiddleCenter;
            value.horizontalOverflow = HorizontalWrapMode.Overflow;
            value.verticalOverflow = VerticalWrapMode.Overflow;
            value.color = Color.white;
            AddTextOutline(value, new Color(0f, 0.08f, 0.13f, 0.96f), new Vector2(2.4f, -2.4f));
            EditorUtility.SetDirty(value);
        }
    }

    private static void ConfigureCurrentHpPanelImage(Transform leftPanel)
    {
        Image panel = EnsureImage(leftPanel, "CurrentHpPanelImage");
        panel.gameObject.SetActive(true);
        ConfigureImage(panel, CurrentHpPanelSquarePath, Image.Type.Simple, Color.white);
        panel.enabled = true;
        panel.transform.SetAsFirstSibling();
        SetStretch(panel.transform as RectTransform, new Vector2(0.130735f, 0.240f), new Vector2(0.820735f, 0.900f));
        EditorUtility.SetDirty(panel.gameObject);
    }

    private static void ConfigureCurrentHpGauge(Transform leftPanel)
    {
        Transform gaugeBack = leftPanel.Find("CurrentHpGaugeBack");
        if (gaugeBack != null)
        {
            SetChildActiveIfPresent(gaugeBack, "CurrentHpGaugeFill", false);
        }

        SetChildActiveIfPresent(leftPanel, "CurrentHpGaugeBack", false);
    }

    private static void ConfigureSlots(Transform root)
    {
        Sprite currentSprite = LoadSprite(CurrentCardPath);
        Sprite allySprite = LoadSprite(AllyCardPath);
        Sprite enemySprite = LoadSprite(EnemyCardPath);

        Transform slotsRoot = root.Find("SlotsRoot");
        if (slotsRoot == null)
        {
            return;
        }

        for (int i = 0; i < slotsRoot.childCount; i++)
        {
            Transform slot = slotsRoot.GetChild(i);
            BattleTimelineSlotView slotView = slot.GetComponent<BattleTimelineSlotView>();
            Image background = slot.GetComponent<Image>();
            bool current = i == 0;
            bool ally = i == 1 || i == 3 || i == 6 || i == 7;
            Sprite defaultSprite = current ? currentSprite : ally ? allySprite : enemySprite;

            ConfigureImage(background, defaultSprite, Image.Type.Sliced, Color.white);
            if (background != null)
            {
                background.enabled = true;
                background.color = Color.white;
                background.raycastTarget = false;
                EditorUtility.SetDirty(background);
            }

            if (slotView != null)
            {
                slotView.SetBackgroundSprites(currentSprite, allySprite, enemySprite);
                slotView.CacheReferences();
                EditorUtility.SetDirty(slotView);
            }

            Text index = FindText(slot, "IndexText");
            if (index != null)
            {
                index.fontStyle = FontStyle.Bold;
                index.alignment = TextAnchor.MiddleCenter;
                index.color = current ? new Color(0.05f, 0.04f, 0.00f, 1f) : Color.white;
                index.text = string.Empty;
                index.gameObject.SetActive(false);
                AddTextOutline(index, current ? new Color(1f, 0.92f, 0.30f, 0.35f) : new Color(0f, 0.08f, 0.14f, 0.88f), new Vector2(1.5f, -1.5f));
            }

            Text state = FindText(slot, "StateText");
            if (state != null)
            {
                state.fontStyle = FontStyle.Bold;
                state.alignment = TextAnchor.MiddleCenter;
                state.color = current ? new Color(0.08f, 0.045f, 0.00f, 1f) : new Color(0.90f, 1f, 1f, 0.96f);
                state.text = string.Empty;
                state.gameObject.SetActive(false);
                AddTextOutline(state, current ? new Color(1f, 0.88f, 0.24f, 0.28f) : new Color(0f, 0.08f, 0.14f, 0.80f), new Vector2(1.2f, -1.2f));
            }

            Image icon = FindImage(slot, "Icon");
            if (icon != null)
            {
                icon.preserveAspect = true;
                icon.raycastTarget = false;
            }

            ConfigureSlotLayout(slot as RectTransform, i, current);
            if (slotView != null)
            {
                slotView.SetTimelineLabelsVisible(false);
            }
        }
    }

    private static void ConfigureSlotLayout(RectTransform slot, int index, bool current)
    {
        if (slot == null)
        {
            return;
        }

        float[] minX = { 0.000f, 0.146f, 0.268f, 0.390f, 0.512f, 0.634f, 0.756f, 0.878f };
        float[] maxX = { 0.142f, 0.264f, 0.386f, 0.508f, 0.630f, 0.752f, 0.874f, 0.996f };
        if (index < 0 || index >= minX.Length)
        {
            return;
        }

        SetStretch(slot, new Vector2(minX[index], current ? 0.005f : 0.100f), new Vector2(maxX[index], current ? 0.985f : 0.900f));

        RectTransform icon = slot.Find("Icon") as RectTransform;
        SetStretch(
            icon,
            new Vector2(current ? 0.090f : 0.075f, current ? 0.110f : 0.095f),
            new Vector2(current ? 0.910f : 0.925f, current ? 0.900f : 0.905f));

        RectTransform indexText = slot.Find("IndexText") as RectTransform;
        SetStretch(
            indexText,
            new Vector2(0.500f, 0.500f),
            new Vector2(0.500f, 0.500f));

        RectTransform stateText = slot.Find("StateText") as RectTransform;
        SetStretch(
            stateText,
            new Vector2(0.500f, 0.500f),
            new Vector2(0.500f, 0.500f));
    }

    private static void RemoveChildrenByName(Transform root, string objectName)
    {
        if (root == null)
        {
            return;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = children.Length - 1; i >= 0; i--)
        {
            Transform child = children[i];
            if (child != null && child != root && string.Equals(child.name, objectName, StringComparison.Ordinal))
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    private static void ApplyGridPrefabPolish()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(GridPrefabPath);
        try
        {
            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    Transform cell = root.transform.Find("GridColliders/Enemy_Cell_R" + row + "_C" + column);
                    if (cell == null)
                    {
                        continue;
                    }

                    Transform hpAnchor = EnsureTransform(cell, "HpAnchor");
                    hpAnchor.localPosition = new Vector3(0f, -0.32f, -0.08f);
                    hpAnchor.localRotation = Quaternion.identity;
                    hpAnchor.localScale = Vector3.one;
                    EditorUtility.SetDirty(hpAnchor);
                }
            }

            PrefabUtility.SaveAsPrefabAsset(root, GridPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void ApplyBattleSceneComposition()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!string.Equals(scene.path, BattleScenePath, StringComparison.Ordinal))
        {
            scene = EditorSceneManager.OpenScene(BattleScenePath);
        }

        GameObject grid = GameObject.Find(GridObjectName);
        if (grid != null)
        {
            grid.transform.position = new Vector3(0f, -1.36f, 0.1f);
            grid.transform.localScale = new Vector3(0.68f, 0.68f, 1f);
            EditorUtility.SetDirty(grid.transform);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void WriteHudFrame(string path)
    {
        Texture2D texture = CreateTexture(1600, 210, new Color(0f, 0f, 0f, 0f));
        FillBeveledRect(texture, 30, 20, 1570, 190, 34, new Color(0.015f, 0.060f, 0.085f, 0.88f));
        FillBeveledRect(texture, 62, 48, 1538, 166, 22, new Color(0.005f, 0.020f, 0.040f, 0.70f));
        DrawGlowBeveledRect(texture, 28, 18, 1572, 192, 36, new Color(0.10f, 0.95f, 1f, 0.42f), 9);
        DrawBeveledRect(texture, 34, 24, 1566, 186, 30, 5, new Color(0.74f, 1f, 1f, 0.96f));
        DrawBeveledRect(texture, 54, 42, 1546, 170, 22, 2, new Color(0.04f, 0.83f, 1f, 0.70f));
        DrawLine(texture, 560, 58, 1450, 58, new Color(0.16f, 0.95f, 1f, 0.50f), 3);
        DrawLine(texture, 500, 152, 1380, 152, new Color(1f, 0.24f, 0.72f, 0.36f), 2);
        DrawCircuitTicks(texture, 760, 42, 1340, 60, new Color(0.42f, 1f, 1f, 0.46f));
        DrawChevrons(texture, 1510, 92, new Color(0.38f, 1f, 1f, 0.90f));
        SavePng(texture, path);
    }

    private static void WriteCurrentHpPanel(string path)
    {
        Texture2D texture = CreateTexture(460, 154, new Color(0f, 0f, 0f, 0f));
        FillBeveledRect(texture, 18, 12, 442, 142, 28, new Color(0.015f, 0.055f, 0.080f, 0.92f));
        FillBeveledRect(texture, 42, 34, 418, 122, 18, new Color(0.004f, 0.018f, 0.038f, 0.72f));
        DrawGlowBeveledRect(texture, 15, 9, 445, 145, 30, new Color(0.14f, 0.94f, 1f, 0.42f), 7);
        DrawBeveledRect(texture, 20, 14, 440, 140, 26, 4, new Color(0.58f, 1f, 1f, 0.95f));
        DrawBeveledRect(texture, 36, 30, 424, 126, 18, 2, new Color(0.08f, 0.74f, 1f, 0.60f));
        DrawLine(texture, 54, 70, 398, 70, new Color(0.26f, 1f, 1f, 0.34f), 2);
        DrawLine(texture, 54, 112, 190, 112, new Color(0.82f, 1f, 1f, 0.52f), 2);
        DrawLine(texture, 308, 36, 402, 36, new Color(1f, 0.25f, 0.74f, 0.34f), 2);
        SavePng(texture, path);
    }

    private static void WriteGaugeBack(string path)
    {
        Texture2D texture = CreateTexture(512, 48, new Color(0f, 0f, 0f, 0f));
        FillBeveledRect(texture, 4, 6, 508, 42, 10, new Color(0.00f, 0.015f, 0.028f, 0.92f));
        DrawBeveledRect(texture, 5, 7, 507, 41, 9, 2, new Color(0.20f, 0.85f, 1f, 0.58f));
        DrawLine(texture, 22, 17, 490, 17, new Color(0.05f, 0.40f, 0.48f, 0.55f), 2);
        DrawLine(texture, 22, 31, 490, 31, new Color(0.05f, 0.40f, 0.48f, 0.42f), 1);
        SavePng(texture, path);
    }

    private static void WriteGaugeFill(string path)
    {
        Texture2D texture = CreateTexture(512, 32, new Color(0f, 0f, 0f, 0f));
        for (int x = 0; x < texture.width; x++)
        {
            float t = x / (texture.width - 1f);
            Color color = Color.Lerp(new Color(0.10f, 0.92f, 1f, 0.92f), new Color(0.90f, 1f, 1f, 1f), t);
            for (int y = 4; y < texture.height - 4; y++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        DrawLine(texture, 0, 6, texture.width - 1, 6, new Color(1f, 1f, 1f, 0.34f), 2);
        DrawLine(texture, 0, texture.height - 7, texture.width - 1, texture.height - 7, new Color(0.02f, 0.52f, 0.78f, 0.40f), 2);
        SavePng(texture, path);
    }

    private static void WriteProgressRail(string path)
    {
        Texture2D texture = CreateTexture(900, 44, new Color(0f, 0f, 0f, 0f));
        DrawGlowLine(texture, 10, 24, 890, 24, new Color(0.10f, 0.88f, 1f, 0.44f), 9);
        DrawLine(texture, 10, 24, 890, 24, new Color(0.78f, 1f, 1f, 0.82f), 3);
        DrawLine(texture, 10, 30, 890, 30, new Color(1f, 0.24f, 0.72f, 0.34f), 1);
        for (int i = 0; i < 8; i++)
        {
            int x = 38 + i * 106;
            FillRect(texture, x - 4, 16, x + 4, 32, new Color(0.74f, 1f, 1f, 0.68f));
            DrawRect(texture, x - 7, 13, x + 7, 35, 1, new Color(0.10f, 0.92f, 1f, 0.40f));
        }

        SavePng(texture, path);
    }

    private static void WriteTimelineCard(string path, Color accent, Color fill, bool current)
    {
        int width = current ? 240 : 210;
        int height = current ? 300 : 260;
        Texture2D texture = CreateTexture(width, height, new Color(0f, 0f, 0f, 0f));

        FillTimelineGlass(texture, fill, accent, current);

        DrawTimelineGlowRect(texture, 0, 0, width - 1, height - 1, accent, current ? 13 : 10);
        DrawTimelineSideChannels(texture, accent, current);

        DrawRect(texture, 4, 4, width - 5, height - 5, 1, TimelineColor(accent, 0.28f, 0.38f));
        DrawRect(texture, 8, 8, width - 9, height - 9, current ? 5 : 4, TimelineColor(accent, current ? 0.98f : 0.86f, 0.10f));
        DrawRect(texture, 15, 15, width - 16, height - 16, 2, TimelineColor(accent, current ? 0.84f : 0.62f, 0.54f));
        DrawRect(texture, 25, 27, width - 26, height - 28, 2, TimelineColor(accent, current ? 0.42f : 0.34f, 0.08f));
        DrawRect(texture, 36, 45, width - 37, height - 46, 1, TimelineColor(accent, 0.22f, 0.18f));

        DrawTimelineCornerBrackets(texture, accent, current);
        DrawTimelineTechLines(texture, accent, current);
        DrawTimelineLensHighlights(texture, accent, current);

        SavePng(texture, path);
    }

    private static void FillTimelineGlass(Texture2D texture, Color fill, Color accent, bool current)
    {
        int width = texture.width;
        int height = texture.height;
        Color top = TimelineColor(fill, 0.94f, current ? 0.18f : 0.10f, current ? 1.42f : 1.22f);
        Color bottom = TimelineColor(fill, 0.96f, 0f, current ? 0.58f : 0.50f);
        Color centerTint = TimelineColor(accent, current ? 0.22f : 0.15f, 0.16f);

        for (int y = 0; y < height; y++)
        {
            float vertical = y / (height - 1f);
            for (int x = 0; x < width; x++)
            {
                float horizontal = x / (width - 1f);
                float center = 1f - Mathf.Abs(horizontal - 0.5f) * 2f;
                Color color = Color.Lerp(bottom, top, vertical);
                color = Color.Lerp(color, centerTint, center * 0.18f);
                color.a = Mathf.Clamp01(0.88f + center * 0.07f);
                texture.SetPixel(x, y, color);
            }
        }

        FillRect(texture, 18, 18, width - 19, height - 19, new Color(0f, 0.006f, 0.014f, current ? 0.54f : 0.48f));
        FillRect(texture, 31, 34, width - 32, height - 35, new Color(0f, 0.012f, 0.024f, current ? 0.44f : 0.38f));

        for (int y = 13; y < height - 13; y += 7)
        {
            DrawLine(texture, 15, y, width - 16, y, TimelineColor(accent, current ? 0.065f : 0.050f, 0.20f), 1);
        }
    }

    private static void DrawTimelineGlowRect(Texture2D texture, int xMin, int yMin, int xMax, int yMax, Color accent, int radius)
    {
        for (int i = radius; i >= 1; i--)
        {
            float t = (radius - i + 1f) / radius;
            Color glow = TimelineColor(accent, 0.22f * t * t, 0.24f);
            DrawRect(texture, xMin + i, yMin + i, xMax - i, yMax - i, 1, glow);
        }
    }

    private static void DrawTimelineSideChannels(Texture2D texture, Color accent, bool current)
    {
        int width = texture.width;
        int height = texture.height;
        Color channel = TimelineColor(accent, current ? 0.32f : 0.24f, 0.18f);
        Color dark = new Color(0f, 0f, 0f, current ? 0.42f : 0.36f);

        FillRect(texture, 6, 28, 9, height - 29, dark);
        FillRect(texture, width - 10, 28, width - 7, height - 29, dark);
        FillRect(texture, 12, 24, 14, height - 25, channel);
        FillRect(texture, width - 15, 24, width - 13, height - 25, channel);

        for (int y = 43; y < height - 42; y += current ? 38 : 34)
        {
            FillRect(texture, 18, y, 24, y + 3, TimelineColor(accent, 0.34f, 0.44f));
            FillRect(texture, width - 25, y, width - 19, y + 3, TimelineColor(accent, 0.34f, 0.44f));
        }
    }

    private static void DrawTimelineCornerBrackets(Texture2D texture, Color accent, bool current)
    {
        int width = texture.width;
        int height = texture.height;
        int span = current ? 56 : 46;
        int inset = current ? 20 : 18;
        int thickness = current ? 5 : 4;
        Color hot = TimelineColor(accent, current ? 0.96f : 0.78f, 0.36f);
        Color whiteHot = TimelineColor(accent, current ? 0.62f : 0.46f, 0.72f);
        Color low = TimelineColor(accent, 0.28f, 0.06f);

        DrawLine(texture, inset, inset, inset + span, inset, hot, thickness);
        DrawLine(texture, inset, inset, inset, inset + span, hot, thickness);
        DrawLine(texture, width - inset - span, inset, width - inset, inset, hot, thickness);
        DrawLine(texture, width - inset, inset, width - inset, inset + span, hot, thickness);
        DrawLine(texture, inset, height - inset, inset + span, height - inset, hot, thickness);
        DrawLine(texture, inset, height - inset - span, inset, height - inset, hot, thickness);
        DrawLine(texture, width - inset - span, height - inset, width - inset, height - inset, hot, thickness);
        DrawLine(texture, width - inset, height - inset - span, width - inset, height - inset, hot, thickness);

        DrawLine(texture, inset + 8, inset + 12, inset + span - 8, inset + 12, whiteHot, 1);
        DrawLine(texture, width - inset - span + 8, inset + 12, width - inset - 8, inset + 12, whiteHot, 1);
        DrawLine(texture, inset + 8, height - inset - 12, inset + span - 8, height - inset - 12, low, 1);
        DrawLine(texture, width - inset - span + 8, height - inset - 12, width - inset - 8, height - inset - 12, low, 1);
    }

    private static void DrawTimelineTechLines(Texture2D texture, Color accent, bool current)
    {
        int width = texture.width;
        int height = texture.height;
        Color line = TimelineColor(accent, current ? 0.40f : 0.30f, 0.28f);
        Color dim = TimelineColor(accent, current ? 0.24f : 0.18f, 0.04f);
        Color bright = TimelineColor(accent, current ? 0.68f : 0.50f, 0.58f);

        int topY = height - (current ? 72 : 62);
        int bottomY = current ? 65 : 56;
        DrawLine(texture, 42, topY, width - 43, topY, line, 2);
        DrawLine(texture, 52, topY - 17, 94, topY - 17, dim, 1);
        DrawLine(texture, width - 95, topY - 17, width - 53, topY - 17, dim, 1);
        DrawLine(texture, 45, bottomY, width - 46, bottomY, line, 2);
        DrawLine(texture, 58, bottomY + 18, 102, bottomY + 18, dim, 1);
        DrawLine(texture, width - 103, bottomY + 18, width - 59, bottomY + 18, dim, 1);

        DrawLine(texture, 64, bottomY + 29, 64, bottomY + 54, dim, 1);
        DrawLine(texture, width - 65, bottomY + 29, width - 65, bottomY + 54, dim, 1);
        DrawLine(texture, 82, topY - 55, 82, topY - 28, dim, 1);
        DrawLine(texture, width - 83, topY - 55, width - 83, topY - 28, dim, 1);

        int moduleY = height / 2;
        FillRect(texture, 30, moduleY - 4, 36, moduleY + 4, bright);
        FillRect(texture, width - 37, moduleY - 4, width - 31, moduleY + 4, bright);
        DrawLine(texture, 40, moduleY, 72, moduleY, dim, 1);
        DrawLine(texture, width - 73, moduleY, width - 41, moduleY, dim, 1);

        for (int i = 0; i < 5; i++)
        {
            int x = 48 + i * ((width - 96) / 4);
            FillRect(texture, x - 2, 32, x + 2, 36, TimelineColor(accent, 0.34f, 0.50f));
            FillRect(texture, x - 2, height - 37, x + 2, height - 33, TimelineColor(accent, 0.34f, 0.50f));
        }
    }

    private static void DrawTimelineLensHighlights(Texture2D texture, Color accent, bool current)
    {
        int width = texture.width;
        int height = texture.height;
        Color high = TimelineColor(accent, current ? 0.56f : 0.42f, 0.84f);
        Color low = TimelineColor(accent, current ? 0.30f : 0.22f, 0.24f);

        DrawLine(texture, 18, height - 18, width - 19, height - 18, high, 1);
        DrawLine(texture, 24, height - 31, width - 25, height - 31, low, 1);
        DrawLine(texture, 18, 18, width - 19, 18, low, 1);
        DrawLine(texture, 24, 31, width - 25, 31, high, 1);

        FillRect(texture, 48, height - 25, 95, height - 21, TimelineColor(accent, current ? 0.52f : 0.36f, 0.70f));
        FillRect(texture, width - 96, height - 25, width - 49, height - 21, TimelineColor(accent, current ? 0.52f : 0.36f, 0.70f));
        FillRect(texture, 62, 21, 112, 24, TimelineColor(accent, current ? 0.30f : 0.22f, 0.42f));
        FillRect(texture, width - 113, 21, width - 63, 24, TimelineColor(accent, current ? 0.30f : 0.22f, 0.42f));
    }

    private static Color TimelineColor(Color color, float alpha, float whiteMix, float intensity = 1f)
    {
        Color result = Color.Lerp(color, Color.white, whiteMix);
        result.r = Mathf.Clamp01(result.r * intensity);
        result.g = Mathf.Clamp01(result.g * intensity);
        result.b = Mathf.Clamp01(result.b * intensity);
        result.a = Mathf.Clamp01(alpha);
        return result;
    }

    private static Texture2D CreateTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, color);
            }
        }

        return texture;
    }

    private static void SavePng(Texture2D texture, string assetPath)
    {
        texture.Apply();
        File.WriteAllBytes(ToFullPath(assetPath), texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
    }

    private static void ConfigureSprite(string path, Vector4 border)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.spritePixelsPerUnit = 100f;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }

    private static void FillBeveledRect(Texture2D texture, int xMin, int yMin, int xMax, int yMax, int bevel, Color color)
    {
        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                if (IsInsideBeveledRect(x, y, xMin, yMin, xMax, yMax, bevel))
                {
                    BlendPixel(texture, x, y, color);
                }
            }
        }
    }

    private static void DrawGlowBeveledRect(Texture2D texture, int xMin, int yMin, int xMax, int yMax, int bevel, Color color, int radius)
    {
        for (int i = radius; i >= 1; i--)
        {
            Color glow = color;
            glow.a *= (radius - i + 1f) / (radius * radius);
            DrawBeveledRect(texture, xMin - i, yMin - i, xMax + i, yMax + i, bevel + i, 1, glow);
        }
    }

    private static void DrawBeveledRect(Texture2D texture, int xMin, int yMin, int xMax, int yMax, int bevel, int thickness, Color color)
    {
        for (int i = 0; i < thickness; i++)
        {
            DrawLine(texture, xMin + bevel, yMin + i, xMax - bevel, yMin + i, color, 1);
            DrawLine(texture, xMin + bevel, yMax - i, xMax - bevel, yMax - i, color, 1);
            DrawLine(texture, xMin + i, yMin + bevel, xMin + i, yMax - bevel, color, 1);
            DrawLine(texture, xMax - i, yMin + bevel, xMax - i, yMax - bevel, color, 1);
            DrawLine(texture, xMin + i, yMin + bevel, xMin + bevel, yMin + i, color, 1);
            DrawLine(texture, xMax - bevel, yMin + i, xMax - i, yMin + bevel, color, 1);
            DrawLine(texture, xMin + i, yMax - bevel, xMin + bevel, yMax - i, color, 1);
            DrawLine(texture, xMax - bevel, yMax - i, xMax - i, yMax - bevel, color, 1);
        }
    }

    private static bool IsInsideBeveledRect(int x, int y, int xMin, int yMin, int xMax, int yMax, int bevel)
    {
        if (x < xMin || x > xMax || y < yMin || y > yMax)
        {
            return false;
        }

        if (x < xMin + bevel && y < yMin + bevel && (x - xMin) + (y - yMin) < bevel)
        {
            return false;
        }

        if (x > xMax - bevel && y < yMin + bevel && (xMax - x) + (y - yMin) < bevel)
        {
            return false;
        }

        if (x < xMin + bevel && y > yMax - bevel && (x - xMin) + (yMax - y) < bevel)
        {
            return false;
        }

        if (x > xMax - bevel && y > yMax - bevel && (xMax - x) + (yMax - y) < bevel)
        {
            return false;
        }

        return true;
    }

    private static void DrawCircuitTicks(Texture2D texture, int xMin, int yMin, int xMax, int yMax, Color color)
    {
        int step = 70;
        for (int x = xMin; x < xMax; x += step)
        {
            DrawLine(texture, x, yMin, x + 26, yMin, color, 2);
            DrawLine(texture, x + 31, yMin, x + 31, yMax, color, 1);
            DrawLine(texture, x + 31, yMax, x + 48, yMax, color, 1);
        }
    }

    private static void DrawChevrons(Texture2D texture, int x, int y, Color color)
    {
        for (int i = 0; i < 3; i++)
        {
            int offset = i * 26;
            DrawLine(texture, x + offset, y - 30, x + offset + 22, y, color, 4);
            DrawLine(texture, x + offset + 22, y, x + offset, y + 30, color, 4);
        }
    }

    private static void DrawGlowLine(Texture2D texture, int x0, int y0, int x1, int y1, Color color, int radius)
    {
        for (int i = radius; i >= 1; i--)
        {
            Color glow = color;
            glow.a *= (radius - i + 1f) / (radius * radius);
            DrawLine(texture, x0, y0, x1, y1, glow, i);
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
            FillRect(texture, x - thickness / 2, y - thickness / 2, x + thickness / 2, y + thickness / 2, color);
            if (x == x1 && y == y1)
            {
                break;
            }

            int e2 = err * 2;
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

    private static void DrawRect(Texture2D texture, int xMin, int yMin, int xMax, int yMax, int thickness, Color color)
    {
        FillRect(texture, xMin, yMin, xMax, yMin + thickness - 1, color);
        FillRect(texture, xMin, yMax - thickness + 1, xMax, yMax, color);
        FillRect(texture, xMin, yMin, xMin + thickness - 1, yMax, color);
        FillRect(texture, xMax - thickness + 1, yMin, xMax, yMax, color);
    }

    private static void FillRect(Texture2D texture, int xMin, int yMin, int xMax, int yMax, Color color)
    {
        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                BlendPixel(texture, x, y, color);
            }
        }
    }

    private static void BlendPixel(Texture2D texture, int x, int y, Color color)
    {
        if (x < 0 || x >= texture.width || y < 0 || y >= texture.height)
        {
            return;
        }

        Color existing = texture.GetPixel(x, y);
        texture.SetPixel(x, y, Color.Lerp(existing, color, color.a));
    }

    private static void ConfigureImage(Image image, string spritePath, Image.Type imageType, Color color)
    {
        ConfigureImage(image, LoadSprite(spritePath), imageType, color);
    }

    private static void ConfigureImage(Image image, Sprite sprite, Image.Type imageType, Color color)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = sprite;
        image.color = color;
        image.type = imageType;
        image.preserveAspect = false;
        image.raycastTarget = false;
        EditorUtility.SetDirty(image);
    }

    private static void ClearImage(Image image)
    {
        if (image == null)
        {
            return;
        }

        image.sprite = null;
        image.color = Color.clear;
        image.enabled = false;
        image.raycastTarget = false;
        EditorUtility.SetDirty(image);
    }

    private static Image EnsureImage(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child == null)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            child = gameObject.transform;
            child.SetParent(parent, false);
        }

        CanvasRenderer canvasRenderer = child.GetComponent<CanvasRenderer>();
        if (canvasRenderer == null)
        {
            child.gameObject.AddComponent<CanvasRenderer>();
        }

        Image image = child.GetComponent<Image>();
        if (image == null)
        {
            image = child.gameObject.AddComponent<Image>();
        }

        return image;
    }

    private static Transform EnsureTransform(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child;
        }

        GameObject gameObject = new GameObject(name);
        child = gameObject.transform;
        child.SetParent(parent, false);
        return child;
    }

    private static void SetStretch(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
        EditorUtility.SetDirty(rectTransform);
    }

    private static Text FindText(Transform parent, string path)
    {
        Transform child = parent.Find(path);
        return child != null ? child.GetComponent<Text>() : null;
    }

    private static Image FindImage(Transform parent, string path)
    {
        Transform child = parent.Find(path);
        return child != null ? child.GetComponent<Image>() : null;
    }

    private static void HideText(Text text)
    {
        if (text == null)
        {
            return;
        }

        text.text = string.Empty;
        text.gameObject.SetActive(false);
        EditorUtility.SetDirty(text);
        EditorUtility.SetDirty(text.gameObject);
    }

    private static void SetChildActiveIfPresent(Transform parent, string path, bool active)
    {
        if (parent == null)
        {
            return;
        }

        Transform child = parent.Find(path);
        if (child == null)
        {
            return;
        }

        child.gameObject.SetActive(active);
        EditorUtility.SetDirty(child.gameObject);
    }

    private static void AddTextOutline(Text text, Color color, Vector2 distance)
    {
        if (text == null)
        {
            return;
        }

        Outline outline = text.GetComponent<Outline>();
        if (outline == null)
        {
            outline = text.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
        EditorUtility.SetDirty(outline);
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
        {
            return sprite;
        }

        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
        for (int i = 0; i < assets.Length; i++)
        {
            sprite = assets[i] as Sprite;
            if (sprite != null)
            {
                return sprite;
            }
        }

        return null;
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static string ToFullPath(string assetPath)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), assetPath).Replace("\\", "/");
    }
}
