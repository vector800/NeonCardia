using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PanelAlignedBattleSceneController : MonoBehaviour
{
    private const int Rows = 3;
    private const int ColsPerSide = 3;
    private const int TotalCols = ColsPerSide * 2;

    private const float BoardTextureWidthPixels = 1672f;
    private const float BoardTextureHeightPixels = 941f;
    private const float BoardPixelsPerUnit = 100f;
    private const float ColliderCornerTrim = 0.12f;

    private const int BoardSortingOrder = 5;
    private const int PanelSortingOrder = 10;
    private const int RangeSortingOrder = 20;
    private const int UnitSortingOrderBase = 40;
    private const int EffectSortingOrder = 70;
    private const int LabelSortingOrder = 90;

    [Header("Debug")]
    [SerializeField] private bool showDebugLabels;
    [SerializeField] private bool showRangePreview;
    [SerializeField] private bool autoCyclePreviews;
    [SerializeField] private float autoCycleSeconds = 2.4f;
    [SerializeField] private bool showActionOrderHud;

    [Header("Layout")]
    [SerializeField] private Vector2 cellSize = new Vector2(1.45f, 0.82f);
    [SerializeField] private Vector2 gridCenter = new Vector2(0f, -1.15f);
    [SerializeField] private float boardScale = 0.88f;

    [Header("HUD")]
    [SerializeField] private BattleTimelineHudView battleTimelineHudPrefab;

    [Header("Panel Assets")]
    [SerializeField] private Sprite battleBoardSprite;
    [SerializeField] private Sprite allyPanelSprite;
    [SerializeField] private Sprite enemyPanelSprite;
    [SerializeField] private Sprite rangeOverlaySingleSprite;
    [SerializeField] private Sprite rangeOverlayAreaSprite;

    [Header("Effect Assets")]
    [SerializeField] private Sprite effectHitSingleSprite;
    [SerializeField] private Sprite effectHitLineHorizontalSprite;
    [SerializeField] private Sprite effectHitLineVerticalSprite;
    [SerializeField] private Sprite effectHitAreaSprite;
    [SerializeField] private Sprite effectProjectileSimpleSprite;

    [Header("Unit Assets")]
    [SerializeField] private Sprite allyASprite;
    [SerializeField] private Sprite allyBSprite;
    [SerializeField] private Sprite enemy1Sprite;
    [SerializeField] private Sprite enemy2Sprite;
    [SerializeField] private Sprite enemy3Sprite;

    private readonly PanelCell[,] allyCells = new PanelCell[Rows, ColsPerSide];
    private readonly PanelCell[,] enemyCells = new PanelCell[Rows, ColsPerSide];
    private readonly List<BattleUnitView> units = new List<BattleUnitView>();
    private readonly List<SpriteRenderer> activeRangeRenderers = new List<SpriteRenderer>();
    private readonly List<PanelCell> currentRangeCells = new List<PanelCell>();

    private Transform gridRoot;
    private Transform panelRoot;
    private Transform rangeRoot;
    private Transform unitRoot;
    private Transform effectRoot;
    private Canvas debugCanvas;
    private BattleTimelineHudView battleTimelineHudView;
    private Sprite fallbackWhiteSprite;
    private RangePreviewPattern activePattern = RangePreviewPattern.Row;
    private float autoCycleTimer;
    private int autoCycleIndex = 1;

    private static readonly Vector2[][] PanelCornerPixels =
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

    private static readonly RangePreviewPattern[] PreviewCycle =
    {
        RangePreviewPattern.Single,
        RangePreviewPattern.Row,
        RangePreviewPattern.Column,
        RangePreviewPattern.Cross,
        RangePreviewPattern.Area3x3,
        RangePreviewPattern.EnemyAll,
        RangePreviewPattern.AllyAll
    };

    private enum RangePreviewPattern
    {
        Single,
        Row,
        Column,
        Cross,
        Area3x3,
        EnemyAll,
        AllyAll
    }

    private sealed class BattleUnitView
    {
        public string Id;
        public bool IsAlly;
        public int Hp;
        public int MaxHp;
        public PanelCell Cell;
        public Transform Root;
        public Transform Visual;
        public Transform AttackSocket;
        public SpriteRenderer Renderer;
        public TextMeshPro HpText;
        public TextMeshPro NameText;
        public TextMesh HpMesh;
        public TextMesh NameMesh;
    }

    private void Awake()
    {
        ConfigureCamera();
        EnsureEventSystem();
        BuildScene();
    }

    private void Start()
    {
        if (showRangePreview)
        {
            SelectPreviewPattern(RangePreviewPattern.Row, false);
        }
    }

    private void Update()
    {
        if (showDebugLabels || showRangePreview)
        {
            HandleKeyboardPreviewInput();
        }

        if (!autoCyclePreviews || !showRangePreview)
        {
            return;
        }

        autoCycleTimer += Time.deltaTime;
        if (autoCycleTimer < Mathf.Max(0.5f, autoCycleSeconds))
        {
            return;
        }

        autoCycleTimer = 0f;
        autoCycleIndex = (autoCycleIndex + 1) % PreviewCycle.Length;
        SelectPreviewPattern(PreviewCycle[autoCycleIndex], true);
    }

    private void BuildScene()
    {
        gridRoot = CreateRoot("PanelAlignedBattleRoot");
        gridRoot.position = new Vector3(gridCenter.x, gridCenter.y, 0f);
        gridRoot.localScale = new Vector3(boardScale, boardScale, 1f);

        CreateRoot("BoardVisual", gridRoot);
        panelRoot = CreateRoot("Panels", gridRoot);
        rangeRoot = CreateRoot("RangeOverlays", gridRoot);
        unitRoot = CreateRoot("Units", gridRoot);
        effectRoot = CreateRoot("Effects", gridRoot);

        BuildBoardVisual();
        BuildGrid();
        BuildUnits();
        if (showActionOrderHud)
        {
            BuildHud();
            RefreshTimelineHud();
        }

        if (showDebugLabels)
        {
            BuildDebugCanvas();
        }
    }

    private void ConfigureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 4f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.transform.rotation = Quaternion.identity;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.01f, 0.012f, 0.018f, 1f);
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
            InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            ConfigureInputModule(inputModule);
            return;
        }

        InputSystemUIInputModule existingInputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (existingInputModule == null)
        {
            BaseInputModule legacyModule = eventSystem.GetComponent<BaseInputModule>();
            if (legacyModule != null)
            {
                Destroy(legacyModule);
            }

            existingInputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        ConfigureInputModule(existingInputModule);
    }

    private static void ConfigureInputModule(InputSystemUIInputModule inputModule)
    {
        if (inputModule.point == null)
        {
            inputModule.point = CreateInputActionReference("Point", InputActionType.PassThrough, "<Pointer>/position");
        }

        if (inputModule.leftClick == null)
        {
            inputModule.leftClick = CreateInputActionReference("Click", InputActionType.Button, "<Pointer>/press");
        }

        if (inputModule.rightClick == null)
        {
            inputModule.rightClick = CreateInputActionReference("Right Click", InputActionType.Button, "<Mouse>/rightButton");
        }

        if (inputModule.middleClick == null)
        {
            inputModule.middleClick = CreateInputActionReference("Middle Click", InputActionType.Button, "<Mouse>/middleButton");
        }

        if (inputModule.scrollWheel == null)
        {
            inputModule.scrollWheel = CreateInputActionReference("Scroll Wheel", InputActionType.PassThrough, "<Mouse>/scroll");
        }
    }

    private static InputActionReference CreateInputActionReference(string name, InputActionType type, string binding)
    {
        InputAction action = new InputAction(name, type, binding);
        return InputActionReference.Create(action);
    }

    private void BuildBoardVisual()
    {
        Transform boardRoot = gridRoot.Find("BoardVisual");
        if (boardRoot == null)
        {
            boardRoot = CreateRoot("BoardVisual", gridRoot);
        }

        SpriteRenderer boardRenderer = boardRoot.gameObject.AddComponent<SpriteRenderer>();
        boardRenderer.sprite = battleBoardSprite;
        boardRenderer.sortingOrder = BoardSortingOrder;
        boardRenderer.color = Color.white;
    }

    private void BuildGrid()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < ColsPerSide; col++)
            {
                allyCells[row, col] = CreatePanelCell(PanelCellSide.Ally, row, col);
                enemyCells[row, col] = CreatePanelCell(PanelCellSide.Enemy, row, col);
            }
        }
    }

    private PanelCell CreatePanelCell(PanelCellSide side, int row, int col)
    {
        int globalCol = side == PanelCellSide.Ally ? col : col + ColsPerSide;
        Vector2 center;
        Vector2[] colliderPoints = BuildColliderPoints(row, globalCol, out center);
        GameObject cellObject = new GameObject("PanelCell_" + side + "_R" + row + "_C" + col);
        cellObject.transform.SetParent(panelRoot, false);
        cellObject.transform.localPosition = new Vector3(center.x, center.y, -0.05f);

        if (battleBoardSprite == null)
        {
            SpriteRenderer renderer = cellObject.AddComponent<SpriteRenderer>();
            renderer.sprite = side == PanelCellSide.Ally ? GetSpriteOrFallback(allyPanelSprite, new Color(1f, 0.24f, 0.06f, 1f)) : GetSpriteOrFallback(enemyPanelSprite, new Color(0.02f, 0.62f, 1f, 1f));
            renderer.sortingOrder = PanelSortingOrder;
            ScaleSpriteRendererToSize(renderer, cellSize);
        }

        PolygonCollider2D collider = cellObject.AddComponent<PolygonCollider2D>();
        collider.isTrigger = true;
        collider.pathCount = 1;
        collider.SetPath(0, colliderPoints);

        PanelCell panelCell = cellObject.AddComponent<PanelCell>();
        panelCell.Configure(side, row, col, null);

        if (showDebugLabels)
        {
            TextMeshPro label = CreateWorldLabel(
                "CellLabel_" + side + "_R" + row + "_C" + col,
                cellObject.transform,
                side.ToString()[0] + row.ToString() + col.ToString(),
                2.7f,
                new Color(1f, 1f, 1f, 0.72f),
                new Vector3(0f, 0.02f, -0.2f));
        }

        return panelCell;
    }

    private static Vector2[] BuildColliderPoints(int row, int globalCol, out Vector2 center)
    {
        int index = row * TotalCols + globalCol;
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
            (pixel.x - BoardTextureWidthPixels * 0.5f) / BoardPixelsPerUnit,
            (BoardTextureHeightPixels * 0.5f - pixel.y) / BoardPixelsPerUnit);
    }

    private void BuildUnits()
    {
        units.Clear();
        CreateUnit("A", "A", PanelCellSide.Ally, 1, 1, allyASprite, 1.95f, 180, true);
        CreateUnit("B", "B", PanelCellSide.Ally, 2, 0, allyBSprite, 1.68f, 150, true);
        CreateUnit("E1", "E1", PanelCellSide.Enemy, 0, 2, enemy2Sprite, 1.82f, 70, false);
        CreateUnit("E2", "E2", PanelCellSide.Enemy, 1, 1, enemy1Sprite, 1.42f, 90, false);
        CreateUnit("E3", "E3", PanelCellSide.Enemy, 2, 2, enemy3Sprite, 1.32f, 75, false);
    }

    private void CreateUnit(string id, string displayName, PanelCellSide side, int row, int col, Sprite sprite, float visualScale, int hp, bool isAlly)
    {
        PanelCell cell = GetCell(side, row, col);
        if (cell == null)
        {
            Debug.LogWarning("PanelAlignedBattleScene missing cell for unit " + id);
            return;
        }

        Bounds cellBounds = cell.Bounds;
        Vector3 rootPosition = new Vector3(
            cell.centerWorldPosition.x,
            cellBounds.center.y - cellBounds.size.y * 0.08f,
            -0.28f);

        GameObject rootObject = new GameObject("UnitRoot_" + id);
        rootObject.transform.SetParent(unitRoot, false);
        rootObject.transform.position = rootPosition;

        GameObject visualObject = new GameObject("UnitVisual");
        visualObject.transform.SetParent(rootObject.transform, false);
        visualObject.transform.localPosition = new Vector3(0f, cellBounds.size.y * 0.24f, 0f);
        visualObject.transform.localScale = new Vector3(visualScale, visualScale, 1f);

        SpriteRenderer renderer = visualObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite != null ? sprite : GetSpriteOrFallback(null, isAlly ? new Color(1f, 0.58f, 0.2f, 1f) : new Color(0.22f, 0.9f, 1f, 1f));
        renderer.flipX = !isAlly;
        renderer.sortingOrder = UnitSortingOrderBase + row * 12 + (isAlly ? 3 : 5);

        Transform attackSocket = new GameObject("AttackSocket").transform;
        attackSocket.SetParent(rootObject.transform, false);
        attackSocket.localPosition = new Vector3(isAlly ? cellBounds.size.x * 0.28f : -cellBounds.size.x * 0.28f, cellBounds.size.y * 0.38f, 0f);

        TextMeshPro hpText = showDebugLabels ? CreateWorldLabel("DebugHPText_" + id, rootObject.transform, hp + "/" + hp, 3.2f, new Color(1f, 0.96f, 0.72f, 1f), new Vector3(0f, -cellBounds.size.y * 0.17f, -0.1f)) : null;
        TextMeshPro nameText = showDebugLabels ? CreateWorldLabel("DebugNameText_" + id, rootObject.transform, displayName, 2.8f, Color.white, new Vector3(0f, cellBounds.size.y * 0.92f, -0.1f)) : null;
        float hpX = isAlly ? -cellBounds.size.x * 0.42f : cellBounds.size.x * 0.24f;
        float hpY = isAlly ? -cellBounds.size.y * 0.02f : cellBounds.size.y * 0.12f;
        TextMesh hpMesh = CreateLegacyWorldLabel("VisibleHP_" + id, rootObject.transform, hp.ToString(), 0.064f, Color.white, new Vector3(hpX, hpY, -0.16f));
        TextMesh nameMesh = showDebugLabels ? CreateLegacyWorldLabel("VisibleName_" + id, rootObject.transform, displayName, 0.042f, Color.white, new Vector3(hpX, cellBounds.size.y * 0.42f, -0.16f)) : null;

        units.Add(new BattleUnitView
        {
            Id = id,
            IsAlly = isAlly,
            Hp = hp,
            MaxHp = hp,
            Cell = cell,
            Root = rootObject.transform,
            Visual = visualObject.transform,
            AttackSocket = attackSocket,
            Renderer = renderer,
            HpText = hpText,
            NameText = nameText,
            HpMesh = hpMesh,
            NameMesh = nameMesh
        });
    }

    private TextMeshPro CreateWorldLabel(string name, Transform parent, string text, float fontSize, Color color, Vector3 localPosition)
    {
        GameObject labelObject = new GameObject(name);
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = localPosition;
        labelObject.transform.localRotation = Quaternion.identity;
        labelObject.transform.localScale = Vector3.one * 0.12f;

        TextMeshPro label = labelObject.AddComponent<TextMeshPro>();
        label.text = text;
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = fontSize;
        label.fontStyle = FontStyles.Bold;
        label.color = color;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;
        label.rectTransform.sizeDelta = new Vector2(4.2f, 1.0f);

        MeshRenderer meshRenderer = label.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = LabelSortingOrder;
        }

        return label;
    }

    private TextMesh CreateLegacyWorldLabel(string name, Transform parent, string text, float characterSize, Color color, Vector3 localPosition)
    {
        GameObject labelObject = new GameObject(name);
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.localPosition = localPosition;
        labelObject.transform.localRotation = Quaternion.identity;
        labelObject.transform.localScale = Vector3.one;

        TextMesh label = labelObject.AddComponent<TextMesh>();
        label.text = text;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = characterSize;
        label.fontSize = 64;
        label.color = color;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (label.font == null)
        {
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        MeshRenderer meshRenderer = label.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = LabelSortingOrder + 2;
            if (label.font != null)
            {
                meshRenderer.sharedMaterial = label.font.material;
            }
        }

        return label;
    }

    private void BuildHud()
    {
        GameObject canvasObject = new GameObject("PanelAlignedBattleHudCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        if (battleTimelineHudPrefab != null)
        {
            battleTimelineHudView = Instantiate(battleTimelineHudPrefab, canvasObject.transform, false);
            battleTimelineHudView.name = "BattleTimelineHud_Reused";
            RectTransform hudRect = battleTimelineHudView.transform as RectTransform;
            if (hudRect != null)
            {
                hudRect.anchorMin = new Vector2(0.012f, 0.748f);
                hudRect.anchorMax = new Vector2(0.865f, 0.985f);
                hudRect.offsetMin = Vector2.zero;
                hudRect.offsetMax = Vector2.zero;
                hudRect.localScale = Vector3.one;
            }

            battleTimelineHudView.CacheReferences();
            return;
        }

        Text fallback = CreateUiText("FallbackActionOrderHud", canvasObject.transform, new Vector2(0.02f, 0.91f), new Vector2(0.78f, 0.985f), "ACTION ORDER  A  B  E1  E2  E3", 28, TextAnchor.MiddleLeft, Color.white);
        fallback.raycastTarget = false;
    }

    private void RefreshTimelineHud()
    {
        if (battleTimelineHudView == null)
        {
            return;
        }

        battleTimelineHudView.SetCurrentHp(180, 180);
        BattleTimelineSlotView[] slots = battleTimelineHudView.Slots;
        string[] labels = { "A", "B", "E1", "E2", "E3", "A", "E2", "B" };
        bool[] allies = { true, true, false, false, false, true, false, true };
        Color[] colors =
        {
            new Color(1f, 0.42f, 0.12f, 1f),
            new Color(1f, 0.68f, 0.2f, 1f),
            new Color(0.1f, 0.85f, 1f, 1f),
            new Color(0.15f, 0.95f, 0.78f, 1f),
            new Color(0.35f, 0.65f, 1f, 1f),
            new Color(1f, 0.42f, 0.12f, 1f),
            new Color(0.15f, 0.95f, 0.78f, 1f),
            new Color(1f, 0.68f, 0.2f, 1f)
        };

        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            BattleTimelineSlotView slot = slots[i];
            if (slot == null)
            {
                continue;
            }

            int labelIndex = i % labels.Length;
            slot.SetTimelineLabelsVisible(true);
            slot.SetIndex(i + 1);
            slot.SetStateText(labels[labelIndex]);
            slot.SetIcon(null, colors[labelIndex]);
            slot.SetActiveVisual(i == 0, allies[labelIndex], colors[labelIndex]);
            SetSlotOverlayLabel(slot, labels[labelIndex], i == 0);
        }
    }

    private void SetSlotOverlayLabel(BattleTimelineSlotView slot, string label, bool isCurrent)
    {
        if (slot == null || slot.Root == null)
        {
            return;
        }

        Transform existing = slot.Root.Find("PanelAlignedSlotLabel");
        Text text = existing != null ? existing.GetComponent<Text>() : null;
        if (text == null)
        {
            text = CreateUiText(
                "PanelAlignedSlotLabel",
                slot.Root,
                new Vector2(0.08f, 0.08f),
                new Vector2(0.92f, 0.92f),
                label,
                isCurrent ? 26 : 22,
                TextAnchor.MiddleCenter,
                Color.white);
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = isCurrent ? 26 : 22;
        }

        text.text = label;
        text.color = isCurrent ? new Color(0.08f, 0.04f, 0f, 1f) : Color.white;
        text.fontSize = isCurrent ? 26 : 22;
    }

    private void BuildDebugCanvas()
    {
        if (!showDebugLabels)
        {
            return;
        }

        GameObject canvasObject = new GameObject("PanelAlignedDebugCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        debugCanvas = canvasObject.GetComponent<Canvas>();
        debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        debugCanvas.sortingOrder = 120;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        string[] labels = { "Single", "Row", "Column", "Cross", "3x3", "Enemy", "Ally", "Effect" };
        for (int i = 0; i < labels.Length; i++)
        {
            int capturedIndex = i;
            Button button = CreateDebugButton(labels[i], canvasObject.transform, i);
            button.onClick.AddListener(() =>
            {
                if (capturedIndex == 7)
                {
                    PlayEffectForCurrentRange();
                    return;
                }

                autoCyclePreviews = false;
                SelectPreviewPattern(PreviewCycle[capturedIndex], true);
            });
        }
    }

    private Button CreateDebugButton(string label, Transform parent, int index)
    {
        float minY = 0.70f - index * 0.045f;
        float maxY = minY + 0.035f;
        GameObject buttonObject = new GameObject("DebugButton_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0.875f, minY);
        rect.anchorMax = new Vector2(0.985f, maxY);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.02f, 0.08f, 0.12f, 0.78f);

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        CreateUiText(label + "_Text", buttonObject.transform, Vector2.zero, Vector2.one, label, 15, TextAnchor.MiddleCenter, Color.white);
        return button;
    }

    private Text CreateUiText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string text, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Text label = textObject.GetComponent<Text>();
        label.text = text;
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (label.font == null)
        {
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        label.fontSize = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    private void HandleKeyboardPreviewInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame) { autoCyclePreviews = false; SelectPreviewPattern(RangePreviewPattern.Single, true); }
        if (keyboard.digit2Key.wasPressedThisFrame) { autoCyclePreviews = false; SelectPreviewPattern(RangePreviewPattern.Row, true); }
        if (keyboard.digit3Key.wasPressedThisFrame) { autoCyclePreviews = false; SelectPreviewPattern(RangePreviewPattern.Column, true); }
        if (keyboard.digit4Key.wasPressedThisFrame) { autoCyclePreviews = false; SelectPreviewPattern(RangePreviewPattern.Cross, true); }
        if (keyboard.digit5Key.wasPressedThisFrame) { autoCyclePreviews = false; SelectPreviewPattern(RangePreviewPattern.Area3x3, true); }
        if (keyboard.digit6Key.wasPressedThisFrame) { autoCyclePreviews = false; SelectPreviewPattern(RangePreviewPattern.EnemyAll, true); }
        if (keyboard.digit7Key.wasPressedThisFrame) { autoCyclePreviews = false; SelectPreviewPattern(RangePreviewPattern.AllyAll, true); }
        if (keyboard.eKey.wasPressedThisFrame) { PlayEffectForCurrentRange(); }
    }

    private void SelectPreviewPattern(RangePreviewPattern pattern, bool playEffect)
    {
        activePattern = pattern;
        currentRangeCells.Clear();
        currentRangeCells.AddRange(GetRangeCells(pattern));
        RefreshRangeOverlay();
        RefreshHitLabels();

        if (playEffect)
        {
            PlayEffectForCurrentRange();
        }
    }

    private List<PanelCell> GetRangeCells(RangePreviewPattern pattern)
    {
        switch (pattern)
        {
            case RangePreviewPattern.Single:
                return new List<PanelCell> { enemyCells[1, 1] };
            case RangePreviewPattern.Row:
                return GetRowCells(PanelCellSide.Enemy, 1);
            case RangePreviewPattern.Column:
                return GetColumnCells(PanelCellSide.Enemy, 1);
            case RangePreviewPattern.Cross:
                return GetCrossCells(PanelCellSide.Enemy, 1, 1);
            case RangePreviewPattern.Area3x3:
                return GetSideCells(PanelCellSide.Enemy);
            case RangePreviewPattern.EnemyAll:
                return GetSideCells(PanelCellSide.Enemy);
            case RangePreviewPattern.AllyAll:
                return GetSideCells(PanelCellSide.Ally);
            default:
                return new List<PanelCell>();
        }
    }

    private List<PanelCell> GetRowCells(PanelCellSide side, int row)
    {
        List<PanelCell> cells = new List<PanelCell>(ColsPerSide);
        for (int col = 0; col < ColsPerSide; col++)
        {
            cells.Add(GetCell(side, row, col));
        }

        return cells;
    }

    private List<PanelCell> GetColumnCells(PanelCellSide side, int col)
    {
        List<PanelCell> cells = new List<PanelCell>(Rows);
        for (int row = 0; row < Rows; row++)
        {
            cells.Add(GetCell(side, row, col));
        }

        return cells;
    }

    private List<PanelCell> GetCrossCells(PanelCellSide side, int centerRow, int centerCol)
    {
        List<PanelCell> cells = new List<PanelCell>(5);
        AddCellIfValid(cells, side, centerRow, centerCol);
        AddCellIfValid(cells, side, centerRow - 1, centerCol);
        AddCellIfValid(cells, side, centerRow + 1, centerCol);
        AddCellIfValid(cells, side, centerRow, centerCol - 1);
        AddCellIfValid(cells, side, centerRow, centerCol + 1);
        return cells;
    }

    private List<PanelCell> GetSideCells(PanelCellSide side)
    {
        List<PanelCell> cells = new List<PanelCell>(Rows * ColsPerSide);
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < ColsPerSide; col++)
            {
                cells.Add(GetCell(side, row, col));
            }
        }

        return cells;
    }

    private void AddCellIfValid(List<PanelCell> cells, PanelCellSide side, int row, int col)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= ColsPerSide)
        {
            return;
        }

        cells.Add(GetCell(side, row, col));
    }

    private PanelCell GetCell(PanelCellSide side, int row, int col)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= ColsPerSide)
        {
            return null;
        }

        return side == PanelCellSide.Ally ? allyCells[row, col] : enemyCells[row, col];
    }

    private void RefreshRangeOverlay()
    {
        ClearRangeOverlay();
        if (!showRangePreview)
        {
            return;
        }

        Sprite overlaySprite = activePattern == RangePreviewPattern.Single || activePattern == RangePreviewPattern.Row || activePattern == RangePreviewPattern.Column
            ? rangeOverlaySingleSprite
            : rangeOverlayAreaSprite;
        overlaySprite = GetSpriteOrFallback(overlaySprite, new Color(1f, 0.78f, 0.1f, 0.5f));

        for (int i = 0; i < currentRangeCells.Count; i++)
        {
            PanelCell cell = currentRangeCells[i];
            if (cell == null)
            {
                continue;
            }

            GameObject overlayObject = new GameObject("RangeOverlay_" + cell.Side + "_R" + cell.Row + "_C" + cell.Col);
            overlayObject.transform.SetParent(rangeRoot, false);
            overlayObject.transform.position = new Vector3(cell.centerWorldPosition.x, cell.centerWorldPosition.y, -0.18f);
            SpriteRenderer renderer = overlayObject.AddComponent<SpriteRenderer>();
            renderer.sprite = overlaySprite;
            renderer.color = new Color(1f, 1f, 1f, 0.72f);
            renderer.sortingOrder = RangeSortingOrder;
            ScaleSpriteRendererToSize(renderer, cell.Bounds.size);
            activeRangeRenderers.Add(renderer);
        }
    }

    private void ClearRangeOverlay()
    {
        for (int i = 0; i < activeRangeRenderers.Count; i++)
        {
            if (activeRangeRenderers[i] != null)
            {
                Destroy(activeRangeRenderers[i].gameObject);
            }
        }

        activeRangeRenderers.Clear();
    }

    private void RefreshHitLabels()
    {
        for (int i = 0; i < units.Count; i++)
        {
            BattleUnitView unit = units[i];
            bool inRange = currentRangeCells.Contains(unit.Cell);
            if (unit.NameText != null)
            {
                unit.NameText.color = inRange ? new Color(1f, 0.95f, 0.32f, 1f) : Color.white;
            }

            if (unit.HpText != null)
            {
                unit.HpText.color = inRange ? new Color(1f, 0.88f, 0.35f, 1f) : new Color(1f, 0.96f, 0.72f, 1f);
            }

            if (unit.NameMesh != null)
            {
                unit.NameMesh.color = inRange ? new Color(1f, 0.95f, 0.32f, 1f) : Color.white;
            }

            if (unit.HpMesh != null)
            {
                unit.HpMesh.color = inRange ? new Color(1f, 0.88f, 0.35f, 1f) : new Color(1f, 0.96f, 0.72f, 1f);
            }
        }
    }

    private void PlayEffectForCurrentRange()
    {
        if (currentRangeCells.Count == 0)
        {
            return;
        }

        BattleUnitView attacker = FindUnit("A");
        PanelCell primaryTarget = currentRangeCells[0];
        for (int i = 0; i < currentRangeCells.Count; i++)
        {
            PanelCell candidate = currentRangeCells[i];
            if (candidate != null && candidate.Side == PanelCellSide.Enemy && candidate.Row == 1)
            {
                primaryTarget = candidate;
                break;
            }
        }

        if (attacker != null && primaryTarget != null)
        {
            StartCoroutine(PlayProjectile(attacker.AttackSocket.position, primaryTarget.centerWorldPosition));
        }

        Sprite effectSprite = ResolveEffectSprite(activePattern);
        Bounds rangeBounds = CombineCellBounds(currentRangeCells);
        GameObject effectObject = new GameObject("PanelAlignedEffect_" + activePattern);
        effectObject.transform.SetParent(effectRoot, false);
        effectObject.transform.position = new Vector3(rangeBounds.center.x, rangeBounds.center.y, -0.42f);

        SpriteRenderer renderer = effectObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSpriteOrFallback(effectSprite, new Color(1f, 0.92f, 0.32f, 1f));
        renderer.sortingOrder = EffectSortingOrder;

        Vector2 effectSize = ResolveEffectSize(activePattern, rangeBounds);
        ScaleSpriteRendererToSize(renderer, effectSize);
        StartCoroutine(FadeAndDestroy(effectObject, 0.42f, true));
    }

    private IEnumerator PlayProjectile(Vector3 start, Vector3 end)
    {
        GameObject projectile = new GameObject("Effect_Projectile_Simple");
        projectile.transform.SetParent(effectRoot, false);
        SpriteRenderer renderer = projectile.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSpriteOrFallback(effectProjectileSimpleSprite, new Color(0.45f, 0.95f, 1f, 1f));
        renderer.sortingOrder = EffectSortingOrder + 1;
        ScaleSpriteRendererToSize(renderer, new Vector2(cellSize.x * 0.55f, cellSize.y * 0.25f));

        float elapsed = 0f;
        const float duration = 0.28f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            projectile.transform.position = Vector3.Lerp(start, new Vector3(end.x, end.y, start.z), t);
            yield return null;
        }

        Destroy(projectile);
    }

    private Sprite ResolveEffectSprite(RangePreviewPattern pattern)
    {
        switch (pattern)
        {
            case RangePreviewPattern.Single:
                return effectHitSingleSprite;
            case RangePreviewPattern.Row:
                return effectHitLineHorizontalSprite;
            case RangePreviewPattern.Column:
                return effectHitLineVerticalSprite;
            case RangePreviewPattern.Cross:
            case RangePreviewPattern.Area3x3:
            case RangePreviewPattern.EnemyAll:
            case RangePreviewPattern.AllyAll:
                return effectHitAreaSprite;
            default:
                return effectHitSingleSprite;
        }
    }

    private Vector2 ResolveEffectSize(RangePreviewPattern pattern, Bounds rangeBounds)
    {
        switch (pattern)
        {
            case RangePreviewPattern.Row:
                return new Vector2(rangeBounds.size.x, cellSize.y * 0.35f);
            case RangePreviewPattern.Column:
                return new Vector2(cellSize.x * 0.38f, rangeBounds.size.y);
            case RangePreviewPattern.Single:
                return cellSize * 0.85f;
            default:
                return new Vector2(rangeBounds.size.x, rangeBounds.size.y) * 0.95f;
        }
    }

    private static Bounds CombineCellBounds(List<PanelCell> cells)
    {
        Bounds combined = new Bounds(Vector3.zero, Vector3.zero);
        bool initialized = false;
        for (int i = 0; i < cells.Count; i++)
        {
            PanelCell cell = cells[i];
            if (cell == null)
            {
                continue;
            }

            if (!initialized)
            {
                combined = cell.Bounds;
                initialized = true;
            }
            else
            {
                combined.Encapsulate(cell.Bounds);
            }
        }

        return initialized ? combined : new Bounds(Vector3.zero, Vector3.one);
    }

    private IEnumerator FadeAndDestroy(GameObject target, float duration, bool pulse)
    {
        SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>();
        Vector3 startScale = target.transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (pulse)
            {
                target.transform.localScale = startScale * Mathf.Lerp(0.92f, 1.12f, Mathf.Sin(t * Mathf.PI));
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Color color = renderers[i].color;
                color.a = 1f - t;
                renderers[i].color = color;
            }

            yield return null;
        }

        Destroy(target);
    }

    private BattleUnitView FindUnit(string id)
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].Id == id)
            {
                return units[i];
            }
        }

        return null;
    }

    private Sprite GetSpriteOrFallback(Sprite sprite, Color color)
    {
        if (sprite != null)
        {
            return sprite;
        }

        if (fallbackWhiteSprite == null)
        {
            Texture2D texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            fallbackWhiteSprite = Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        }

        return fallbackWhiteSprite;
    }

    private static void ScaleSpriteRendererToSize(SpriteRenderer renderer, Vector2 targetSize)
    {
        if (renderer == null || renderer.sprite == null)
        {
            return;
        }

        Vector2 spriteSize = renderer.sprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        renderer.transform.localScale = new Vector3(targetSize.x / spriteSize.x, targetSize.y / spriteSize.y, 1f);
    }

    private static Transform CreateRoot(string name)
    {
        return new GameObject(name).transform;
    }

    private static Transform CreateRoot(string name, Transform parent)
    {
        Transform root = new GameObject(name).transform;
        root.SetParent(parent, false);
        return root;
    }
}
