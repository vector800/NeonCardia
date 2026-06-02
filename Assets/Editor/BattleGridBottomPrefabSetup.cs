using UnityEditor;
using UnityEditor.Animations;
using TMPro;
using UnityEngine;

public static class BattleGridBottomPrefabSetup
{
    private const string PrefabPath = "Assets/Art/BattleField/NeonGrid/Prefabs/PF_BattleGrid_Full_3x6_Image2_Bottom.prefab";
    private const string AllyKnightSpritePath = "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_00.png";
    private const string AllyWolfSpritePath = "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_00.png";
    private const string AllyFairySpritePath = "Assets/Art/Characters/DigitalFairy/Frames/DigitalFairy_idle_00.png";
    private const string EnemyDrillMoleSpritePath = "Assets/Art/Enemies/DrillMole/Frames/Enemy_DrillMole_Idle_00.png";
    private const string EnemyElecGeckoSpritePath = "Assets/Art/Enemies/ElecGecko/Frames/Enemy_ElecGecko_Idle_00.png";
    private const string EnemyBladeBugSpritePath = "Assets/Art/Enemies/BladeBug/Frames/Enemy_BladeBug_Idle_00.png";
    private const string AllyAnimationRoot = "Assets/Animations/Allies";
    private const float AllyIdleFrameSeconds = 0.20f;
    private const float EnemyIdleFrameSeconds = 0.14f;

    private const float TextureWidthPixels = 1672f;
    private const float TextureHeightPixels = 941f;
    private const float PixelsPerUnit = 100f;
    private const float ColliderCornerTrim = 0.12f;
    private static readonly Vector3 AnchorOffset = new Vector3(0f, 0.30f, -0.05f);
    private static readonly Vector3 AllyKnightScale = new Vector3(2.01f, 2.01f, 1f);
    private static readonly Vector3 AllyWolfScale = new Vector3(1.71f, 1.71f, 1f);
    private static readonly Vector3 AllyFairyScale = new Vector3(1.65f, 1.65f, 1f);
    private static readonly Vector3 EnemyDrillMoleScale = new Vector3(1.447f, 1.447f, 1f);
    private static readonly Vector3 EnemyElecGeckoScale = new Vector3(1.853f, 1.853f, 1f);
    private static readonly Vector3 EnemyBladeBugScale = new Vector3(1.326f, 1.326f, 1f);
    private static readonly Vector3 AllyKnightDisplayOffset = new Vector3(0f, 0.27f, 0f);
    private static readonly Vector3 AllyWolfDisplayOffset = new Vector3(0f, 0.19f, 0f);
    private static readonly Vector3 AllyFairyDisplayOffset = new Vector3(0f, 0.22f, 0f);
    private static readonly Vector3 EnemyDrillMoleDisplayOffset = new Vector3(0f, 0.2800f, 0f);
    private static readonly Vector3 EnemyElecGeckoDisplayOffset = new Vector3(-0.0600f, 0.2500f, 0f);
    private static readonly Vector3 EnemyBladeBugDisplayOffset = new Vector3(0f, 0.4200f, 0f);
    private const float EnemyHpPanelYOffset = 0.10f;
    private static readonly Vector3 EnemyElecGeckoHpPanelVisualAdjustment = new Vector3(0f, -0.055f, 0f);
    private static readonly Vector3 EnemyDrillMoleHpPanelVisualAdjustment = Vector3.zero;
    private static readonly Vector3 EnemyBladeBugHpPanelVisualAdjustment = new Vector3(0f, -0.050f, 0f);
    private static readonly Vector3 EnemyHpFallbackLocalPosition = new Vector3(0f, -0.38f, -0.08f);
    private static readonly Vector3 EnemyHpTextLocalScale = new Vector3(0.74f, 0.74f, 1f);
    private static readonly Vector2 EnemyHpTextSize = new Vector2(2.2f, 0.8f);
    private static readonly Vector2[][] PanelCornerPixels = new Vector2[][]
    {
        new[] { new Vector2(205f, 198f), new Vector2(386f, 197f), new Vector2(367f, 292f), new Vector2(184f, 292f) },
        new[] { new Vector2(422f, 198f), new Vector2(598f, 197f), new Vector2(592f, 293f), new Vector2(409f, 291f) },
        new[] { new Vector2(643f, 199f), new Vector2(810f, 197f), new Vector2(814f, 293f), new Vector2(630f, 291f) },
        new[] { new Vector2(863f, 199f), new Vector2(1037f, 198f), new Vector2(1057f, 288f), new Vector2(867f, 294f) },
        new[] { new Vector2(1080f, 200f), new Vector2(1266f, 198f), new Vector2(1261f, 291f), new Vector2(1077f, 292f) },
        new[] { new Vector2(1297f, 197f), new Vector2(1497f, 201f), new Vector2(1518f, 289f), new Vector2(1294f, 293f) },
        new[] { new Vector2(168f, 320f), new Vector2(369f, 320f), new Vector2(356f, 429f), new Vector2(140f, 431f) },
        new[] { new Vector2(398f, 321f), new Vector2(592f, 321f), new Vector2(595f, 428f), new Vector2(380f, 431f) },
        new[] { new Vector2(628f, 321f), new Vector2(813f, 321f), new Vector2(811f, 430f), new Vector2(618f, 430f) },
        new[] { new Vector2(863f, 320f), new Vector2(1052f, 321f), new Vector2(1061f, 429f), new Vector2(854f, 430f) },
        new[] { new Vector2(1087f, 320f), new Vector2(1277f, 320f), new Vector2(1296f, 430f), new Vector2(1094f, 430f) },
        new[] { new Vector2(1320f, 318f), new Vector2(1530f, 322f), new Vector2(1534f, 431f), new Vector2(1320f, 429f) },
        new[] { new Vector2(123f, 461f), new Vector2(337f, 459f), new Vector2(337f, 586f), new Vector2(90f, 588f) },
        new[] { new Vector2(365f, 461f), new Vector2(585f, 459f), new Vector2(571f, 587f), new Vector2(348f, 589f) },
        new[] { new Vector2(611f, 459f), new Vector2(810f, 461f), new Vector2(811f, 587f), new Vector2(600f, 588f) },
        new[] { new Vector2(865f, 458f), new Vector2(1066f, 458f), new Vector2(1072f, 588f), new Vector2(853f, 588f) },
        new[] { new Vector2(1097f, 457f), new Vector2(1306f, 458f), new Vector2(1327f, 589f), new Vector2(1088f, 586f) },
        new[] { new Vector2(1331f, 457f), new Vector2(1560f, 458f), new Vector2(1589f, 589f), new Vector2(1316f, 586f) }
    };

    [MenuItem("Tools/NeonCardia/Setup Bottom Battle Grid Hitboxes")]
    public static void Setup()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            RemoveChildIfExists(root.transform, "PanelHitboxes");
            RemoveChildIfExists(root.transform, "GridColliders");
            RemoveChildIfExists(root.transform, "Units");

            Transform hitboxRoot = CreateChild(root.transform, "GridColliders").transform;
            Transform unitRoot = CreateChild(root.transform, "Units").transform;

            BattleGridPanelHitbox[,] allyPanels = new BattleGridPanelHitbox[3, 3];
            BattleGridPanelHitbox[,] enemyPanels = new BattleGridPanelHitbox[3, 3];

            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    allyPanels[row, column] = CreatePanelHitbox(hitboxRoot, GridSide.Player, row, column);
                    enemyPanels[row, column] = CreatePanelHitbox(hitboxRoot, GridSide.Enemy, row, column);
                }
            }

            CreateUnit(unitRoot, "Ally_Front_CyberKnight", AllyKnightSpritePath, allyPanels[1, 1].UnitAnchor, AllyKnightScale, AllyKnightDisplayOffset, false, 42, "Assets/Animations/Allies/CyberKnight/CyberKnight_Idle.anim", "Assets/Animations/Allies/CyberKnight/CyberKnight.controller", new[]
            {
                "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_00.png",
                "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_01.png",
                "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_02.png",
                "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_03.png"
            }, AllyIdleFrameSeconds);
            CreateUnit(unitRoot, "Ally_Middle_CyberWolf", AllyWolfSpritePath, allyPanels[0, 1].UnitAnchor, AllyWolfScale, AllyWolfDisplayOffset, false, 40, "Assets/Animations/Allies/CyberWolf/CyberWolf_Idle.anim", "Assets/Animations/Allies/CyberWolf/CyberWolf.controller", new[]
            {
                "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_00.png",
                "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_01.png",
                "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_02.png",
                "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_03.png"
            }, AllyIdleFrameSeconds);
            CreateUnit(unitRoot, "Ally_Back_DigitalFairy", AllyFairySpritePath, allyPanels[2, 1].UnitAnchor, AllyFairyScale, AllyFairyDisplayOffset, false, 44, "Assets/Animations/Allies/DigitalFairy/DigitalFairy_Idle.anim", "Assets/Animations/Allies/DigitalFairy/DigitalFairy.controller", new[]
            {
                "Assets/Art/Characters/DigitalFairy/Frames/DigitalFairy_idle_00.png",
                "Assets/Art/Characters/DigitalFairy/Frames/DigitalFairy_idle_01.png",
                "Assets/Art/Characters/DigitalFairy/Frames/DigitalFairy_idle_02.png",
                "Assets/Art/Characters/DigitalFairy/Frames/DigitalFairy_idle_03.png"
            }, AllyIdleFrameSeconds);

            CreateUnit(unitRoot, "Enemy_DrillMole", EnemyDrillMoleSpritePath, enemyPanels[1, 1].transform, EnemyDrillMoleScale, EnemyDrillMoleDisplayOffset, true, 42, "Assets/Animations/Enemies/DrillMole/Enemy_DrillMole_Idle.anim", "Assets/Animations/Enemies/DrillMole/Enemy_DrillMole.controller", new[]
            {
                "Assets/Art/Enemies/DrillMole/Frames/Enemy_DrillMole_Idle_00.png",
                "Assets/Art/Enemies/DrillMole/Frames/Enemy_DrillMole_Idle_01.png",
                "Assets/Art/Enemies/DrillMole/Frames/Enemy_DrillMole_Idle_02.png",
                "Assets/Art/Enemies/DrillMole/Frames/Enemy_DrillMole_Idle_03.png",
                "Assets/Art/Enemies/DrillMole/Frames/Enemy_DrillMole_Idle_04.png",
                "Assets/Art/Enemies/DrillMole/Frames/Enemy_DrillMole_Idle_05.png"
            }, EnemyIdleFrameSeconds);
            CreateUnit(unitRoot, "Enemy_ElecGecko", EnemyElecGeckoSpritePath, enemyPanels[0, 2].transform, EnemyElecGeckoScale, EnemyElecGeckoDisplayOffset, false, 40, "Assets/Animations/Enemies/ElecGecko/Enemy_ElecGecko_Idle.anim", "Assets/Animations/Enemies/ElecGecko/Enemy_ElecGecko.controller", new[]
            {
                "Assets/Art/Enemies/ElecGecko/Frames/Enemy_ElecGecko_Idle_00.png",
                "Assets/Art/Enemies/ElecGecko/Frames/Enemy_ElecGecko_Idle_01.png",
                "Assets/Art/Enemies/ElecGecko/Frames/Enemy_ElecGecko_Idle_02.png",
                "Assets/Art/Enemies/ElecGecko/Frames/Enemy_ElecGecko_Idle_03.png",
                "Assets/Art/Enemies/ElecGecko/Frames/Enemy_ElecGecko_Idle_04.png",
                "Assets/Art/Enemies/ElecGecko/Frames/Enemy_ElecGecko_Idle_05.png"
            }, EnemyIdleFrameSeconds);
            CreateUnit(unitRoot, "Enemy_BladeBug", EnemyBladeBugSpritePath, enemyPanels[2, 2].transform, EnemyBladeBugScale, EnemyBladeBugDisplayOffset, false, 44, "Assets/Animations/Enemies/BladeBug/Enemy_BladeBug_Idle.anim", "Assets/Animations/Enemies/BladeBug/Enemy_BladeBug.controller", new[]
            {
                "Assets/Art/Enemies/BladeBug/Frames/Enemy_BladeBug_Idle_00.png",
                "Assets/Art/Enemies/BladeBug/Frames/Enemy_BladeBug_Idle_01.png",
                "Assets/Art/Enemies/BladeBug/Frames/Enemy_BladeBug_Idle_02.png",
                "Assets/Art/Enemies/BladeBug/Frames/Enemy_BladeBug_Idle_03.png",
                "Assets/Art/Enemies/BladeBug/Frames/Enemy_BladeBug_Idle_04.png",
                "Assets/Art/Enemies/BladeBug/Frames/Enemy_BladeBug_Idle_05.png"
            }, EnemyIdleFrameSeconds);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Configured bottom battle grid colliders and units.");
    }

    [MenuItem("Tools/NeonCardia/Apply Enemy Visual Root Layout")]
    public static void ApplyEnemyVisualRootLayout()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            Transform units = root.transform.Find("Units");
            if (units == null)
            {
                Debug.LogError("Missing Units root in " + PrefabPath);
                return;
            }

            ConfigureEnemyVisualRoot(units, "Enemy_DrillMole", "GridColliders/Enemy_Cell_R1_C1", EnemyDrillMoleScale, EnemyDrillMoleDisplayOffset);
            ConfigureEnemyVisualRoot(units, "Enemy_ElecGecko", "GridColliders/Enemy_Cell_R0_C2", EnemyElecGeckoScale, EnemyElecGeckoDisplayOffset);
            ConfigureEnemyVisualRoot(units, "Enemy_BladeBug", "GridColliders/Enemy_Cell_R2_C2", EnemyBladeBugScale, EnemyBladeBugDisplayOffset);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Applied enemy visual root layout.");
    }

    private static BattleGridPanelHitbox CreatePanelHitbox(Transform parent, GridSide side, int row, int column)
    {
        string sideName = side == GridSide.Player ? "Ally" : "Enemy";
        GameObject panel = CreateChild(parent, sideName + "_Cell_R" + row + "_C" + column);
        Vector2 center;
        Vector2[] colliderPoints = BuildColliderPoints(side, row, column, out center);
        panel.transform.localPosition = new Vector3(center.x, center.y, -0.1f);

        PolygonCollider2D collider = panel.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;
        collider.pathCount = 1;
        collider.SetPath(0, colliderPoints);

        GameObject anchor = CreateChild(panel.transform, "UnitAnchor");
        anchor.transform.localPosition = AnchorOffset;

        BattleGridPanelHitbox hitbox = panel.AddComponent<BattleGridPanelHitbox>();
        hitbox.Configure(side, row, column, anchor.transform);
        return hitbox;
    }

    private static Vector2[] BuildColliderPoints(GridSide side, int row, int column, out Vector2 center)
    {
        int globalColumn = side == GridSide.Player ? column : column + 3;
        int index = row * 6 + globalColumn;
        Vector2[] corners = PanelCornerPixels[index];
        Vector2[] pixelPoints = CreateBeveledPixelPolygon(corners);
        Vector2[] localPoints = new Vector2[pixelPoints.Length];
        center = Vector2.zero;

        for (int i = 0; i < pixelPoints.Length; i++)
        {
            localPoints[i] = ConvertPixelToLocal(pixelPoints[i]);
            center += localPoints[i];
        }

        center /= localPoints.Length;
        for (int i = 0; i < localPoints.Length; i++)
        {
            localPoints[i] -= center;
        }

        return localPoints;
    }

    private static Vector2[] CreateBeveledPixelPolygon(Vector2[] corners)
    {
        Vector2 topLeft = corners[0];
        Vector2 topRight = corners[1];
        Vector2 bottomRight = corners[2];
        Vector2 bottomLeft = corners[3];

        return new[]
        {
            Vector2.Lerp(topLeft, topRight, ColliderCornerTrim),
            Vector2.Lerp(topLeft, topRight, 1f - ColliderCornerTrim),
            Vector2.Lerp(topRight, bottomRight, ColliderCornerTrim),
            Vector2.Lerp(topRight, bottomRight, 1f - ColliderCornerTrim),
            Vector2.Lerp(bottomRight, bottomLeft, ColliderCornerTrim),
            Vector2.Lerp(bottomRight, bottomLeft, 1f - ColliderCornerTrim),
            Vector2.Lerp(bottomLeft, topLeft, ColliderCornerTrim),
            Vector2.Lerp(bottomLeft, topLeft, 1f - ColliderCornerTrim)
        };
    }

    private static Vector2 ConvertPixelToLocal(Vector2 pixel)
    {
        return new Vector2(
            (pixel.x - TextureWidthPixels * 0.5f) / PixelsPerUnit,
            (TextureHeightPixels * 0.5f - pixel.y) / PixelsPerUnit);
    }

    private static void CreateUnit(Transform parent, string name, string spritePath, Transform anchor, Vector3 scale, Vector3 displayOffset, bool flipX, int sortingOrder, string clipPath, string controllerPath, string[] framePaths, float frameSeconds)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        GameObject unit = CreateChild(parent, name);
        bool usesVisualRoot = name.StartsWith("Enemy_", System.StringComparison.Ordinal);
        GameObject visual = usesVisualRoot ? CreateChild(unit.transform, "Visual") : unit;
        if (usesVisualRoot)
        {
            unit.transform.position = anchor.position;
            unit.transform.localScale = Vector3.one;
            visual.transform.localPosition = displayOffset;
            visual.transform.localScale = scale;
        }
        else
        {
            unit.transform.position = anchor.position + displayOffset;
            unit.transform.localScale = scale;
        }

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.flipX = flipX;
        renderer.sortingOrder = sortingOrder;

        Animator animator = visual.AddComponent<Animator>();
        animator.runtimeAnimatorController = EnsureIdleAnimatorController(clipPath, controllerPath, framePaths, frameSeconds);
        animator.enabled = true;
        animator.speed = 1f;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.updateMode = AnimatorUpdateMode.Normal;
        animator.applyRootMotion = false;

        if (usesVisualRoot)
        {
            EnsureEnemyHpText(unit.transform, name, anchor);
        }
    }

    private static void ConfigureEnemyVisualRoot(Transform units, string unitName, string anchorPath, Vector3 visualScale, Vector3 visualOffset)
    {
        Transform unit = units.Find(unitName);
        Transform anchor = units.parent.Find(anchorPath);
        if (unit == null || anchor == null)
        {
            Debug.LogError("Cannot configure enemy visual root: " + unitName);
            return;
        }

        Transform visual = unit.Find("Visual");
        if (visual == null)
        {
            visual = CreateChild(unit, "Visual").transform;
        }

        SpriteRenderer rootRenderer = unit.GetComponent<SpriteRenderer>();
        SpriteRenderer visualRenderer = visual.GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            if (visualRenderer == null)
            {
                visualRenderer = visual.gameObject.AddComponent<SpriteRenderer>();
            }

            EditorUtility.CopySerialized(rootRenderer, visualRenderer);
            Object.DestroyImmediate(rootRenderer, true);
        }

        Animator rootAnimator = unit.GetComponent<Animator>();
        Animator visualAnimator = visual.GetComponent<Animator>();
        if (rootAnimator != null)
        {
            if (visualAnimator == null)
            {
                visualAnimator = visual.gameObject.AddComponent<Animator>();
            }

            EditorUtility.CopySerialized(rootAnimator, visualAnimator);
            Object.DestroyImmediate(rootAnimator, true);
        }

        if (visualAnimator != null)
        {
            visualAnimator.enabled = true;
            visualAnimator.speed = 1f;
            visualAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            visualAnimator.updateMode = AnimatorUpdateMode.Normal;
            visualAnimator.applyRootMotion = false;
        }

        unit.position = anchor.position;
        unit.localScale = Vector3.one;
        unit.localRotation = Quaternion.identity;
        visual.localPosition = visualOffset;
        visual.localScale = visualScale;
        visual.localRotation = Quaternion.identity;
        EnsureEnemyHpText(unit, unitName, anchor);
        EditorUtility.SetDirty(unit);
        EditorUtility.SetDirty(visual);
    }

private static void EnsureEnemyHpText(Transform unit, string unitName, Transform panelCell)
    {
        Transform hpTransform = unit.Find("HPText");
        if (hpTransform == null)
        {
            hpTransform = CreateChild(unit, "HPText").transform;
        }

        TextMeshPro hpText = hpTransform.GetComponent<TextMeshPro>();
        if (hpText == null)
        {
            hpText = hpTransform.gameObject.AddComponent<TextMeshPro>();
        }

        hpTransform = hpText.transform;
        hpTransform.localPosition = GetEnemyHpLocalPosition(unit, panelCell, unitName);
        hpTransform.localRotation = Quaternion.identity;
        hpTransform.localScale = EnemyHpTextLocalScale;

        hpText.text = GetDefaultEnemyHpText(unitName);
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.fontSize = 3.2f;
        hpText.fontStyle = FontStyles.Bold;
        hpText.enableWordWrapping = false;
        hpText.richText = false;
        hpText.raycastTarget = false;
        hpText.color = new Color(1f, 0.98f, 0.82f, 1f);

        RectTransform rectTransform = hpText.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = EnemyHpTextSize;

        MeshRenderer meshRenderer = hpTransform.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 120;
        }

        EditorUtility.SetDirty(hpText.transform);
        EditorUtility.SetDirty(hpText);
    }

private static Vector3 GetEnemyHpLocalPosition(Transform unit, Transform panelCell, string unitName)
    {
        if (unit == null || panelCell == null)
        {
            return EnemyHpFallbackLocalPosition;
        }

        Physics2D.SyncTransforms();
        Collider2D cellCollider = panelCell.GetComponent<Collider2D>();
        if (cellCollider == null)
        {
            return EnemyHpFallbackLocalPosition;
        }

        Bounds bounds = cellCollider.bounds;
        Vector3 worldPosition = new Vector3(bounds.center.x, bounds.min.y + EnemyHpPanelYOffset, unit.position.z)
            + GetEnemyHpPanelVisualAdjustment(unitName);
        Vector3 localPosition = unit.InverseTransformPoint(worldPosition);
        localPosition.z = EnemyHpFallbackLocalPosition.z;
        return localPosition;
    }

private static Vector3 GetEnemyHpPanelVisualAdjustment(string unitName)
    {
        if (unitName == "Enemy_ElecGecko")
        {
            return EnemyElecGeckoHpPanelVisualAdjustment;
        }

        if (unitName == "Enemy_DrillMole")
        {
            return EnemyDrillMoleHpPanelVisualAdjustment;
        }

        if (unitName == "Enemy_BladeBug")
        {
            return EnemyBladeBugHpPanelVisualAdjustment;
        }

        return Vector3.zero;
    }


    private static string GetDefaultEnemyHpText(string unitName)
    {
        switch (unitName)
        {
            case "Enemy_ElecGecko":
                return "70";
            case "Enemy_DrillMole":
                return "90";
            case "Enemy_BladeBug":
                return "75";
            default:
                return string.Empty;
        }
    }

    private static RuntimeAnimatorController EnsureIdleAnimatorController(string clipPath, string controllerPath, string[] framePaths, float frameSeconds)
    {
        AnimationClip idleClip = EnsureIdleClip(clipPath, framePaths, frameSeconds);
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            EnsureFolderForAsset(controllerPath);
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState idleState = null;
        ChildAnimatorState[] states = stateMachine.states;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].state.name == "Idle")
            {
                idleState = states[i].state;
                break;
            }
        }

        if (idleState == null)
        {
            idleState = stateMachine.AddState("Idle");
        }

        idleState.motion = idleClip;
        idleState.speed = 1f;
        idleState.writeDefaultValues = true;
        stateMachine.defaultState = idleState;
        EditorUtility.SetDirty(idleState);
        EditorUtility.SetDirty(stateMachine);
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static AnimationClip EnsureIdleClip(string clipPath, string[] framePaths, float frameSeconds)
    {
        EnsureFolderForAsset(clipPath);
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, clipPath);
        }

        clip.frameRate = 1f / Mathf.Max(0.01f, frameSeconds);
        clip.wrapMode = WrapMode.Loop;
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[framePaths.Length + 1];
        for (int i = 0; i < framePaths.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i * frameSeconds,
                value = LoadPrimarySprite(framePaths[i])
            };
        }

        keyframes[keyframes.Length - 1] = new ObjectReferenceKeyframe
        {
            time = framePaths.Length * frameSeconds,
            value = keyframes[0].value
        };

        EditorCurveBinding spriteBinding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static Sprite LoadPrimarySprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite != null)
        {
            return sprite;
        }

        Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
        for (int i = 0; i < assets.Length; i++)
        {
            sprite = assets[i] as Sprite;
            if (sprite != null)
            {
                return sprite;
            }
        }

        Debug.LogError("Missing idle sprite frame: " + path);
        return null;
    }

    private static void EnsureFolderForAsset(string assetPath)
    {
        string folder = System.IO.Path.GetDirectoryName(assetPath);
        if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string[] parts = folder.Split('/', '\\');
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

    private static GameObject CreateChild(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;
        return child;
    }

    private static void RemoveChildIfExists(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            Object.DestroyImmediate(child.gameObject);
        }
    }
}
