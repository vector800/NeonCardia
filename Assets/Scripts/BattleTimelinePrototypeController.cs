using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class BattleTimelinePrototypeController : MonoBehaviour
{
    private const int HandSize = 5;
    private const int MaxQueuedActions = 3;
    private const int TimelinePreviewCount = 8;
    private const int WeaponPower = 18;

    private readonly List<AllyUnit> allies = new List<AllyUnit>();
    private readonly List<EnemyUnit> enemies = new List<EnemyUnit>();
    private readonly List<CardData> drawPile = new List<CardData>();
    private readonly List<CardData> hand = new List<CardData>();
    private readonly List<CardData> discardPile = new List<CardData>();
    private readonly List<QueuedAction> queuedActions = new List<QueuedAction>();
    private readonly bool[] queuedHandSlots = new bool[HandSize];

    private readonly List<TimelineSlotView> timelineViews = new List<TimelineSlotView>();
    private readonly Dictionary<PartyPosition, AllyView> allyViews = new Dictionary<PartyPosition, AllyView>();
    private readonly EnemyCellView[,] enemyCellViews = new EnemyCellView[3, 3];
    private readonly List<CardButtonView> handViews = new List<CardButtonView>();

    private Font uiFont;
    private Text turnText;
    private Text statusText;
    private Text queueText;
    private Text deckText;
    private Text selectedText;
    private Button weaponButton;
    private Button confirmButton;
    private Button resetButton;
    private Button swapFrontMiddleButton;
    private Button swapMiddleBackButton;
    private Button swapFrontBackButton;

    private int currentTick;
    private int activeUnitSequence;
    private AllyUnit selectedAlly;
    private EnemyUnit selectedEnemy;
    private TimelineUnit activeUnit;
    private System.Random random;

    private enum PartyPosition
    {
        Front,
        Middle,
        Back
    }

    private enum ActionKind
    {
        Card,
        Weapon,
        Swap
    }

    private sealed class AllyUnit
    {
        public string Name;
        public int Hp;
        public int MaxHp;
        public PartyPosition Position;
        public int Speed;
        public string Status;
        public int NextReadyTick;

        public bool IsAlive
        {
            get { return Hp > 0; }
        }
    }

    private sealed class EnemyUnit
    {
        public string Name;
        public int Hp;
        public int MaxHp;
        public CardAttribute Attribute;
        public Vector2Int GridPosition;
        public string NextAction;
        public int Speed;
        public int NextReadyTick;

        public bool IsAlive
        {
            get { return Hp > 0; }
        }
    }

    private sealed class TimelineUnit
    {
        public AllyUnit Ally;
        public EnemyUnit Enemy;
        public bool IsAlly;
        public int ReadyTick;
        public int Sequence;

        public string DisplayName
        {
            get { return IsAlly ? Ally.Name : Enemy.Name; }
        }

        public int Speed
        {
            get { return IsAlly ? Ally.Speed : Enemy.Speed; }
        }
    }

    private sealed class QueuedAction
    {
        public ActionKind Kind;
        public CardData Card;
        public int HandIndex = -1;
        public AllyUnit Actor;
        public AllyUnit AllyTarget;
        public EnemyUnit EnemyTarget;
        public PartyPosition SwapA;
        public PartyPosition SwapB;
        public bool ConsumesAction;
        public string Label;
    }

    private sealed class TimelineSlotView
    {
        public Image Panel;
        public Image Accent;
        public Text NameText;
        public Text DetailText;
    }

    private sealed class AllyView
    {
        public Image Panel;
        public Image Accent;
        public Text PositionText;
        public Text NameText;
        public Text DetailText;
        public Button Button;
    }

    private sealed class EnemyCellView
    {
        public Image Panel;
        public Text Label;
        public Button Button;
    }

    private sealed class CardButtonView
    {
        public Image Panel;
        public Text NameText;
        public Text DetailText;
        public Button Button;
    }

    private struct TimelinePreview
    {
        public TimelineUnit Unit;
        public int DeltaTick;
    }

    private void Awake()
    {
        random = new System.Random(17);
        uiFont = CreateJapaneseFont();
        if (uiFont == null)
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        if (uiFont == null)
        {
            uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        EnsureCamera();
        EnsureEventSystem();
        InitializeBattle();
        BuildUi();
        RefreshAll("BattleTimelinePrototypeScene ready. Select a card, weapon, or swap, then confirm.");
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
        {
            Confirm();
        }
        else if (keyboard.rKey.wasPressedThisFrame)
        {
            ResetSelection();
        }
        else if (keyboard.digit1Key.wasPressedThisFrame)
        {
            QueueCardFromHand(0);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            QueueCardFromHand(1);
        }
        else if (keyboard.digit3Key.wasPressedThisFrame)
        {
            QueueCardFromHand(2);
        }
        else if (keyboard.digit4Key.wasPressedThisFrame)
        {
            QueueCardFromHand(3);
        }
        else if (keyboard.digit5Key.wasPressedThisFrame)
        {
            QueueCardFromHand(4);
        }
    }

    private static Font CreateJapaneseFont()
    {
        string[] fontNames = { "Meiryo UI", "Yu Gothic UI", "Meiryo", "Yu Gothic", "Noto Sans CJK JP", "Arial" };
        return Font.CreateDynamicFontFromOSFont(fontNames, 24);
    }

    private void EnsureCamera()
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
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.018f, 0.022f, 0.032f);
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
            InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            ConfigureInputModule(inputModule);
            return;
        }

        InputSystemUIInputModule inputModuleOnObject = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModuleOnObject == null)
        {
            BaseInputModule legacyModule = eventSystem.GetComponent<BaseInputModule>();
            if (legacyModule != null)
            {
                Destroy(legacyModule);
            }

            inputModuleOnObject = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        ConfigureInputModule(inputModuleOnObject);
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

    private void InitializeBattle()
    {
        allies.Clear();
        enemies.Clear();
        drawPile.Clear();
        hand.Clear();
        discardPile.Clear();
        queuedActions.Clear();
        ClearQueuedHandSlots();

        currentTick = 0;
        activeUnitSequence = 0;

        allies.Add(new AllyUnit { Name = "AllyFront", Hp = 150, MaxHp = 150, Position = PartyPosition.Front, Speed = 48, Status = "Normal", NextReadyTick = 0 });
        allies.Add(new AllyUnit { Name = "AllyMiddle", Hp = 125, MaxHp = 125, Position = PartyPosition.Middle, Speed = 58, Status = "Normal", NextReadyTick = 8 });
        allies.Add(new AllyUnit { Name = "AllyBack", Hp = 110, MaxHp = 110, Position = PartyPosition.Back, Speed = 68, Status = "Normal", NextReadyTick = 16 });

        enemies.Add(new EnemyUnit { Name = "Enemy1", Hp = 90, MaxHp = 90, Attribute = CardAttribute.Neutral, GridPosition = new Vector2Int(1, 1), NextAction = "Claw", Speed = 42, NextReadyTick = 12 });
        enemies.Add(new EnemyUnit { Name = "Enemy2", Hp = 70, MaxHp = 70, Attribute = CardAttribute.Fire, GridPosition = new Vector2Int(0, 2), NextAction = "Shot", Speed = 36, NextReadyTick = 24 });
        enemies.Add(new EnemyUnit { Name = "Enemy3", Hp = 75, MaxHp = 75, Attribute = CardAttribute.Ice, GridPosition = new Vector2Int(2, 2), NextAction = "Guard Break", Speed = 32, NextReadyTick = 32 });

        selectedAlly = allies[0];
        selectedEnemy = enemies[0];

        LoadPrototypeDeck();
        Shuffle(drawPile);
        DrawToHand();
        activeUnit = GetCurrentActiveUnit();
    }

    private void LoadPrototypeDeck()
    {
        CardData[] loadedCards = Resources.LoadAll<CardData>("Cards");
        for (int i = 0; i < loadedCards.Length; i++)
        {
            CardData card = loadedCards[i];
            if (card != null && card.Effect != CardEffectType.Move)
            {
                drawPile.Add(card);
            }
        }

        if (drawPile.Count == 0)
        {
            List<CardData> starterDeck = CardData.CreateStarterDeck();
            for (int i = 0; i < starterDeck.Count; i++)
            {
                if (starterDeck[i] != null && starterDeck[i].Effect != CardEffectType.Move)
                {
                    drawPile.Add(starterDeck[i]);
                }
            }
        }
    }

    private void BuildUi()
    {
        GameObject canvasObject = new GameObject("Battle Timeline Prototype Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        Image background = CreateImage("Background", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.016f, 0.02f, 0.03f, 1f));
        background.raycastTarget = false;

        BuildBackgroundGrid(canvasObject.transform);
        BuildHeader(canvasObject.transform);
        BuildTimeline(canvasObject.transform);
        BuildAllies(canvasObject.transform);
        BuildEnemyGrid(canvasObject.transform);
        BuildHandAndCommands(canvasObject.transform);
    }

    private void BuildBackgroundGrid(Transform parent)
    {
        Color verticalColor = new Color(0.10f, 0.72f, 0.86f, 0.075f);
        Color horizontalColor = new Color(0.86f, 0.95f, 0.24f, 0.055f);
        for (int i = 0; i <= 24; i++)
        {
            float x = i / 24f;
            CreateImage("Grid Vertical " + i, parent, new Vector2(x, 0f), new Vector2(x, 1f), new Vector2(-1f, 0f), new Vector2(1f, 0f), verticalColor).raycastTarget = false;
        }

        for (int i = 0; i <= 12; i++)
        {
            float y = i / 12f;
            CreateImage("Grid Horizontal " + i, parent, new Vector2(0f, y), new Vector2(1f, y), new Vector2(0f, -1f), new Vector2(0f, 1f), horizontalColor).raycastTarget = false;
        }
    }

    private void BuildHeader(Transform parent)
    {
        CreateImage("Header Panel", parent, new Vector2(0.035f, 0.865f), new Vector2(0.965f, 0.965f), Vector2.zero, Vector2.zero, new Color(0.012f, 0.04f, 0.055f, 0.92f)).raycastTarget = false;
        CreateImage("Header Accent", parent, new Vector2(0.035f, 0.865f), new Vector2(0.965f, 0.873f), Vector2.zero, Vector2.zero, new Color(0.1f, 0.88f, 1f, 0.92f)).raycastTarget = false;
        CreateText("Title", parent, new Vector2(0.055f, 0.91f), new Vector2(0.5f, 0.955f), Vector2.zero, Vector2.zero, "BATTLE TIMELINE PROTOTYPE", 30, TextAnchor.MiddleLeft, new Color(0.9f, 1f, 1f));
        CreateText("Purpose", parent, new Vector2(0.055f, 0.873f), new Vector2(0.58f, 0.915f), Vector2.zero, Vector2.zero, "Separate MVP scene. Existing BattleScene is not used by this prototype.", 16, TextAnchor.MiddleLeft, new Color(0.72f, 0.9f, 0.96f));
        turnText = CreateText("Turn Text", parent, new Vector2(0.58f, 0.91f), new Vector2(0.945f, 0.955f), Vector2.zero, Vector2.zero, string.Empty, 24, TextAnchor.MiddleRight, new Color(1f, 0.88f, 0.28f));
        selectedText = CreateText("Selected Text", parent, new Vector2(0.58f, 0.873f), new Vector2(0.945f, 0.915f), Vector2.zero, Vector2.zero, string.Empty, 15, TextAnchor.MiddleRight, new Color(0.9f, 1f, 0.92f));
    }

    private void BuildTimeline(Transform parent)
    {
        RectTransform panel = CreatePanel("Timeline Panel", parent, new Vector2(0.035f, 0.72f), new Vector2(0.965f, 0.85f), new Color(0.018f, 0.035f, 0.045f, 0.94f));
        CreateText("Timeline Label", panel, new Vector2(0.015f, 0.66f), new Vector2(0.17f, 0.95f), Vector2.zero, Vector2.zero, "TIMELINE", 18, TextAnchor.MiddleLeft, new Color(0.88f, 1f, 1f));
        CreateText("Timeline Hint", panel, new Vector2(0.17f, 0.66f), new Vector2(0.98f, 0.95f), Vector2.zero, Vector2.zero, "Speed controls how quickly each unit returns after acting. Leftmost unit is active.", 14, TextAnchor.MiddleRight, new Color(0.68f, 0.84f, 0.9f));

        for (int i = 0; i < TimelinePreviewCount; i++)
        {
            float minX = 0.02f + i * 0.12f;
            float maxX = minX + 0.105f;
            RectTransform slot = CreateRect("Timeline Slot " + i, panel, new Vector2(minX, 0.12f), new Vector2(maxX, 0.62f), Vector2.zero, Vector2.zero);
            Image slotPanel = slot.gameObject.AddComponent<Image>();
            slotPanel.color = new Color(0.035f, 0.07f, 0.09f, 0.96f);
            Image accent = CreateImage("Timeline Slot Accent " + i, slot, new Vector2(0f, 0.88f), Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            accent.raycastTarget = false;
            Text name = CreateText("Timeline Slot Name " + i, slot, new Vector2(0.06f, 0.45f), new Vector2(0.94f, 0.88f), Vector2.zero, Vector2.zero, string.Empty, 15, TextAnchor.MiddleCenter, Color.white);
            Text detail = CreateText("Timeline Slot Detail " + i, slot, new Vector2(0.06f, 0.06f), new Vector2(0.94f, 0.45f), Vector2.zero, Vector2.zero, string.Empty, 12, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.35f));
            timelineViews.Add(new TimelineSlotView { Panel = slotPanel, Accent = accent, NameText = name, DetailText = detail });
        }
    }

    private void BuildAllies(Transform parent)
    {
        RectTransform panel = CreatePanel("Ally Panel", parent, new Vector2(0.035f, 0.295f), new Vector2(0.45f, 0.695f), new Color(0.014f, 0.032f, 0.04f, 0.94f));
        CreateText("Ally Label", panel, new Vector2(0.04f, 0.88f), new Vector2(0.62f, 0.98f), Vector2.zero, Vector2.zero, "ALLY PARTY", 21, TextAnchor.MiddleLeft, new Color(0.88f, 1f, 1f));
        CreateText("Ally Hint", panel, new Vector2(0.48f, 0.88f), new Vector2(0.96f, 0.98f), Vector2.zero, Vector2.zero, "Front / Middle / Back", 15, TextAnchor.MiddleRight, new Color(0.78f, 0.92f, 0.96f));

        CreateAllyView(panel, PartyPosition.Front, 0.62f);
        CreateAllyView(panel, PartyPosition.Middle, 0.36f);
        CreateAllyView(panel, PartyPosition.Back, 0.10f);
    }

    private void CreateAllyView(Transform parent, PartyPosition position, float y)
    {
        RectTransform root = CreateRect("Ally " + position, parent, new Vector2(0.04f, y), new Vector2(0.96f, y + 0.20f), Vector2.zero, Vector2.zero);
        Image panel = root.gameObject.AddComponent<Image>();
        panel.color = new Color(0.028f, 0.055f, 0.07f, 0.96f);
        Button button = root.gameObject.AddComponent<Button>();
        button.targetGraphic = panel;
        PartyPosition capturedPosition = position;
        button.onClick.AddListener(() => SelectAllyAtPosition(capturedPosition));

        Image accent = CreateImage("Ally Accent " + position, root, Vector2.zero, new Vector2(0.018f, 1f), Vector2.zero, Vector2.zero, GetPositionColor(position));
        accent.raycastTarget = false;
        Text positionText = CreateText("Ally Position " + position, root, new Vector2(0.05f, 0.58f), new Vector2(0.25f, 0.95f), Vector2.zero, Vector2.zero, position.ToString().ToUpperInvariant(), 18, TextAnchor.MiddleLeft, GetPositionColor(position));
        Text nameText = CreateText("Ally Name " + position, root, new Vector2(0.25f, 0.55f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero, string.Empty, 20, TextAnchor.MiddleLeft, Color.white);
        Text detailText = CreateText("Ally Detail " + position, root, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.55f), Vector2.zero, Vector2.zero, string.Empty, 15, TextAnchor.MiddleLeft, new Color(0.82f, 0.94f, 0.96f));

        allyViews[position] = new AllyView { Panel = panel, Accent = accent, PositionText = positionText, NameText = nameText, DetailText = detailText, Button = button };
    }

    private void BuildEnemyGrid(Transform parent)
    {
        RectTransform panel = CreatePanel("Enemy Grid Panel", parent, new Vector2(0.485f, 0.295f), new Vector2(0.965f, 0.695f), new Color(0.014f, 0.032f, 0.04f, 0.94f));
        CreateText("Enemy Label", panel, new Vector2(0.04f, 0.88f), new Vector2(0.48f, 0.98f), Vector2.zero, Vector2.zero, "ENEMY 3x3 GRID", 21, TextAnchor.MiddleLeft, new Color(0.88f, 1f, 1f));
        CreateText("Enemy Hint", panel, new Vector2(0.48f, 0.88f), new Vector2(0.96f, 0.98f), Vector2.zero, Vector2.zero, "Click an enemy cell to choose target", 15, TextAnchor.MiddleRight, new Color(0.78f, 0.92f, 0.96f));

        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                float minX = 0.09f + column * 0.28f;
                float maxX = minX + 0.22f;
                float maxY = 0.82f - row * 0.25f;
                float minY = maxY - 0.18f;
                RectTransform cell = CreateRect("Enemy Cell " + row + "-" + column, panel, new Vector2(minX, minY), new Vector2(maxX, maxY), Vector2.zero, Vector2.zero);
                Image image = cell.gameObject.AddComponent<Image>();
                image.color = new Color(0.025f, 0.052f, 0.065f, 0.98f);
                Button button = cell.gameObject.AddComponent<Button>();
                button.targetGraphic = image;
                int capturedRow = row;
                int capturedColumn = column;
                button.onClick.AddListener(() => SelectEnemyAt(capturedRow, capturedColumn));
                Text label = CreateText("Enemy Cell Label " + row + "-" + column, cell, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f), string.Empty, 15, TextAnchor.MiddleCenter, Color.white);
                enemyCellViews[row, column] = new EnemyCellView { Panel = image, Label = label, Button = button };
            }
        }
    }

    private void BuildHandAndCommands(Transform parent)
    {
        RectTransform panel = CreatePanel("Command Panel", parent, new Vector2(0.035f, 0.035f), new Vector2(0.965f, 0.27f), new Color(0.012f, 0.026f, 0.034f, 0.96f));
        CreateText("Hand Label", panel, new Vector2(0.02f, 0.77f), new Vector2(0.24f, 0.96f), Vector2.zero, Vector2.zero, "COMMON HAND", 18, TextAnchor.MiddleLeft, new Color(0.88f, 1f, 1f));
        queueText = CreateText("Queue Text", panel, new Vector2(0.24f, 0.77f), new Vector2(0.72f, 0.96f), Vector2.zero, Vector2.zero, string.Empty, 15, TextAnchor.MiddleLeft, new Color(1f, 0.92f, 0.55f));
        deckText = CreateText("Deck Text", panel, new Vector2(0.72f, 0.77f), new Vector2(0.98f, 0.96f), Vector2.zero, Vector2.zero, string.Empty, 14, TextAnchor.MiddleRight, new Color(0.74f, 0.88f, 0.94f));

        for (int i = 0; i < HandSize; i++)
        {
            float minX = 0.02f + i * 0.132f;
            float maxX = minX + 0.122f;
            Button button = CreateButton("Hand Card " + i, panel, new Vector2(minX, 0.18f), new Vector2(maxX, 0.72f), Vector2.zero, Vector2.zero, string.Empty, 14, new Color(0.12f, 0.17f, 0.23f, 1f));
            int capturedIndex = i;
            button.onClick.AddListener(() => QueueCardFromHand(capturedIndex));
            Text[] labels = button.GetComponentsInChildren<Text>();
            handViews.Add(new CardButtonView { Panel = button.GetComponent<Image>(), NameText = labels[0], DetailText = CreateText("Hand Detail " + i, button.transform, new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.36f), Vector2.zero, Vector2.zero, string.Empty, 12, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.62f)), Button = button });
        }

        weaponButton = CreateButton("Weapon Button", panel, new Vector2(0.70f, 0.54f), new Vector2(0.82f, 0.72f), Vector2.zero, Vector2.zero, "Weapon", 15, new Color(0.23f, 0.30f, 0.38f, 1f));
        weaponButton.onClick.AddListener(QueueWeapon);

        swapFrontMiddleButton = CreateButton("Swap Front Middle", panel, new Vector2(0.84f, 0.54f), new Vector2(0.98f, 0.72f), Vector2.zero, Vector2.zero, "Swap F/M", 14, new Color(0.22f, 0.26f, 0.42f, 1f));
        swapFrontMiddleButton.onClick.AddListener(() => QueueSwap(PartyPosition.Front, PartyPosition.Middle));

        swapMiddleBackButton = CreateButton("Swap Middle Back", panel, new Vector2(0.70f, 0.34f), new Vector2(0.82f, 0.50f), Vector2.zero, Vector2.zero, "Swap M/B", 13, new Color(0.22f, 0.26f, 0.42f, 1f));
        swapMiddleBackButton.onClick.AddListener(() => QueueSwap(PartyPosition.Middle, PartyPosition.Back));

        swapFrontBackButton = CreateButton("Swap Front Back", panel, new Vector2(0.84f, 0.34f), new Vector2(0.98f, 0.50f), Vector2.zero, Vector2.zero, "Swap F/B", 13, new Color(0.22f, 0.26f, 0.42f, 1f));
        swapFrontBackButton.onClick.AddListener(() => QueueSwap(PartyPosition.Front, PartyPosition.Back));

        resetButton = CreateButton("Reset Selection", panel, new Vector2(0.70f, 0.14f), new Vector2(0.82f, 0.30f), Vector2.zero, Vector2.zero, "Reset", 15, new Color(0.36f, 0.24f, 0.22f, 1f));
        resetButton.onClick.AddListener(ResetSelection);

        confirmButton = CreateButton("Confirm Button", panel, new Vector2(0.84f, 0.14f), new Vector2(0.98f, 0.30f), Vector2.zero, Vector2.zero, "Confirm", 16, new Color(0.10f, 0.38f, 0.24f, 1f));
        confirmButton.onClick.AddListener(Confirm);

        statusText = CreateText("Status Text", panel, new Vector2(0.02f, 0.02f), new Vector2(0.68f, 0.16f), Vector2.zero, Vector2.zero, string.Empty, 14, TextAnchor.MiddleLeft, new Color(0.92f, 1f, 0.86f));
    }

    private void QueueCardFromHand(int handIndex)
    {
        if (!IsPlayerTurn())
        {
            RefreshAll("Enemy turn. Press Confirm to resolve the enemy action.");
            return;
        }

        if (handIndex < 0 || handIndex >= hand.Count || hand[handIndex] == null)
        {
            return;
        }

        if (queuedHandSlots[handIndex])
        {
            RefreshAll("That card is already queued. Use Reset to change the plan.");
            return;
        }

        CardData card = hand[handIndex];
        bool consumesAction = !card.IsClearCard;
        if (consumesAction && GetQueuedActionCost() >= MaxQueuedActions)
        {
            RefreshAll("Action queue is full. Clear cards are still free, but normal actions are capped at 3.");
            return;
        }

        QueuedAction action = new QueuedAction
        {
            Kind = ActionKind.Card,
            Card = card,
            HandIndex = handIndex,
            Actor = activeUnit.Ally,
            AllyTarget = selectedAlly != null && selectedAlly.IsAlive ? selectedAlly : activeUnit.Ally,
            EnemyTarget = selectedEnemy != null && selectedEnemy.IsAlive ? selectedEnemy : GetFirstAliveEnemy(),
            ConsumesAction = consumesAction,
            Label = GetCardDisplayName(card)
        };
        queuedActions.Add(action);
        queuedHandSlots[handIndex] = true;
        RefreshAll("Queued card: " + action.Label + ".");
    }

    private void QueueWeapon()
    {
        if (!IsPlayerTurn())
        {
            RefreshAll("Enemy turn. Press Confirm to resolve the enemy action.");
            return;
        }

        if (GetQueuedActionCost() >= MaxQueuedActions)
        {
            RefreshAll("Action queue is full.");
            return;
        }

        queuedActions.Add(new QueuedAction
        {
            Kind = ActionKind.Weapon,
            Actor = activeUnit.Ally,
            EnemyTarget = selectedEnemy != null && selectedEnemy.IsAlive ? selectedEnemy : GetFirstAliveEnemy(),
            ConsumesAction = true,
            Label = "Weapon"
        });
        RefreshAll("Queued Weapon.");
    }

    private void QueueSwap(PartyPosition a, PartyPosition b)
    {
        if (!IsPlayerTurn())
        {
            RefreshAll("Enemy turn. Press Confirm to resolve the enemy action.");
            return;
        }

        if (GetQueuedActionCost() >= MaxQueuedActions)
        {
            RefreshAll("Action queue is full.");
            return;
        }

        queuedActions.Add(new QueuedAction
        {
            Kind = ActionKind.Swap,
            Actor = activeUnit.Ally,
            SwapA = a,
            SwapB = b,
            ConsumesAction = true,
            Label = "Swap " + ShortPosition(a) + "/" + ShortPosition(b)
        });
        RefreshAll("Queued swap: " + a + " <-> " + b + ".");
    }

    private void Confirm()
    {
        activeUnit = GetCurrentActiveUnit();
        if (activeUnit == null)
        {
            RefreshAll("Battle is over.");
            return;
        }

        currentTick = Mathf.Max(currentTick, activeUnit.ReadyTick);

        if (!activeUnit.IsAlly)
        {
            ResolveEnemyTurn(activeUnit.Enemy);
            ClearQueuedActions();
            AdvanceActiveUnit(activeUnit, 1);
            RefreshAll(activeUnit.Enemy.Name + " acted and returned to the timeline.");
            return;
        }

        if (queuedActions.Count == 0)
        {
            RefreshAll("Select a card, weapon, or swap before confirming.");
            return;
        }

        string summary = activeUnit.Ally.Name + " resolved " + queuedActions.Count + " action(s).";
        int actionLoad = Mathf.Max(1, GetQueuedActionCost());
        ResolveQueuedActions();
        DiscardQueuedCards();
        ClearQueuedActions();
        DrawToHand();
        AdvanceActiveUnit(activeUnit, actionLoad);
        RefreshAll(summary);
    }

    private void ResolveQueuedActions()
    {
        for (int i = 0; i < queuedActions.Count; i++)
        {
            QueuedAction action = queuedActions[i];
            if (action.Kind == ActionKind.Card)
            {
                ResolveCard(action);
            }
            else if (action.Kind == ActionKind.Weapon)
            {
                ResolveWeapon(action);
            }
            else if (action.Kind == ActionKind.Swap)
            {
                ResolveSwap(action.SwapA, action.SwapB);
            }
        }
    }

    private void ResolveCard(QueuedAction action)
    {
        CardData card = action.Card;
        if (card == null)
        {
            return;
        }

        switch (card.Effect)
        {
            case CardEffectType.Damage:
                ApplyDamage(action.EnemyTarget, Mathf.Max(1, card.Power), GetCardDisplayName(card));
                break;
            case CardEffectType.Repair:
                HealAlly(action.AllyTarget, Mathf.Max(1, card.Power));
                break;
            case CardEffectType.Guard:
                if (action.Actor != null)
                {
                    action.Actor.Status = "Guard +" + Mathf.Max(1, card.Power);
                }
                break;
            case CardEffectType.Charge:
                if (action.Actor != null)
                {
                    action.Actor.Status = "Charged +" + Mathf.Max(1, card.Power);
                }
                break;
            case CardEffectType.Freeze:
                if (action.EnemyTarget != null && action.EnemyTarget.IsAlive)
                {
                    action.EnemyTarget.NextAction = "Frozen";
                    action.EnemyTarget.NextReadyTick += 12;
                }
                break;
            default:
                if (card.TargetType == CardTargetType.Enemy)
                {
                    ApplyDamage(action.EnemyTarget, Mathf.Max(10, card.Power), GetCardDisplayName(card) + " fallback");
                }
                else
                {
                    if (action.Actor != null)
                    {
                        action.Actor.Status = "Used " + GetCardDisplayName(card);
                    }
                }
                break;
        }
    }

    private void ResolveWeapon(QueuedAction action)
    {
        ApplyDamage(action.EnemyTarget, WeaponPower, "Weapon");
    }

    private void ResolveSwap(PartyPosition a, PartyPosition b)
    {
        AllyUnit first = GetAllyAtPosition(a);
        AllyUnit second = GetAllyAtPosition(b);
        if (first == null || second == null)
        {
            return;
        }

        first.Position = b;
        second.Position = a;
    }

    private void ResolveEnemyTurn(EnemyUnit enemy)
    {
        if (enemy == null || !enemy.IsAlive)
        {
            return;
        }

        AllyUnit target = GetAllyAtPosition(PartyPosition.Front);
        if (target == null || !target.IsAlive)
        {
            target = GetFirstAliveAlly();
        }

        if (target == null)
        {
            return;
        }

        int damage = enemy.NextAction == "Guard Break" ? 20 : 16;
        target.Hp = Mathf.Max(0, target.Hp - damage);
        target.Status = "Hit -" + damage;
        enemy.NextAction = enemy.NextAction == "Claw" ? "Shot" : "Claw";
    }

    private void ApplyDamage(EnemyUnit target, int amount, string source)
    {
        EnemyUnit resolvedTarget = target != null && target.IsAlive ? target : GetFirstAliveEnemy();
        if (resolvedTarget == null)
        {
            return;
        }

        resolvedTarget.Hp = Mathf.Max(0, resolvedTarget.Hp - Mathf.Max(0, amount));
        if (!resolvedTarget.IsAlive)
        {
            resolvedTarget.NextAction = "KO";
            if (selectedEnemy == resolvedTarget)
            {
                selectedEnemy = GetFirstAliveEnemy();
            }
        }
    }

    private void HealAlly(AllyUnit target, int amount)
    {
        AllyUnit resolvedTarget = target != null && target.IsAlive ? target : activeUnit.Ally;
        if (resolvedTarget == null)
        {
            return;
        }

        resolvedTarget.Hp = Mathf.Min(resolvedTarget.MaxHp, resolvedTarget.Hp + Mathf.Max(0, amount));
        resolvedTarget.Status = "Healed +" + amount;
    }

    private void AdvanceActiveUnit(TimelineUnit unit, int actionLoad)
    {
        if (unit == null)
        {
            return;
        }

        int recovery = CalculateRecovery(unit.Speed, actionLoad);
        if (unit.IsAlly)
        {
            unit.Ally.NextReadyTick = currentTick + recovery;
        }
        else
        {
            unit.Enemy.NextReadyTick = currentTick + recovery;
        }

        activeUnitSequence++;
        activeUnit = GetCurrentActiveUnit();
        if (activeUnit != null && activeUnit.IsAlly)
        {
            selectedAlly = activeUnit.Ally;
        }
    }

    private int CalculateRecovery(int speed, int actionLoad)
    {
        return Mathf.Clamp(72 - Mathf.RoundToInt(speed * 0.58f) + (actionLoad - 1) * 8, 18, 80);
    }

    private void DiscardQueuedCards()
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < queuedActions.Count; i++)
        {
            if (queuedActions[i].Kind == ActionKind.Card && queuedActions[i].HandIndex >= 0)
            {
                indices.Add(queuedActions[i].HandIndex);
            }
        }

        indices.Sort();
        for (int i = indices.Count - 1; i >= 0; i--)
        {
            int handIndex = indices[i];
            if (handIndex >= 0 && handIndex < hand.Count)
            {
                discardPile.Add(hand[handIndex]);
                hand.RemoveAt(handIndex);
            }
        }
    }

    private void ResetSelection()
    {
        ClearQueuedActions();
        RefreshAll("Selection reset.");
    }

    private void ClearQueuedActions()
    {
        queuedActions.Clear();
        ClearQueuedHandSlots();
    }

    private void ClearQueuedHandSlots()
    {
        for (int i = 0; i < queuedHandSlots.Length; i++)
        {
            queuedHandSlots[i] = false;
        }
    }

    private void DrawToHand()
    {
        while (hand.Count < HandSize)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0)
                {
                    break;
                }

                drawPile.AddRange(discardPile);
                discardPile.Clear();
                Shuffle(drawPile);
            }

            CardData card = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(card);
        }
    }

    private void Shuffle(List<CardData> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int swapIndex = random.Next(i, cards.Count);
            CardData temp = cards[i];
            cards[i] = cards[swapIndex];
            cards[swapIndex] = temp;
        }
    }

    private void SelectAllyAtPosition(PartyPosition position)
    {
        AllyUnit ally = GetAllyAtPosition(position);
        if (ally != null)
        {
            selectedAlly = ally;
            RefreshAll("Selected ally target: " + ally.Name + ".");
        }
    }

    private void SelectEnemyAt(int row, int column)
    {
        EnemyUnit enemy = GetEnemyAt(row, column);
        if (enemy != null && enemy.IsAlive)
        {
            selectedEnemy = enemy;
            RefreshAll("Selected enemy target: " + enemy.Name + ".");
        }
    }

    private void RefreshAll(string message)
    {
        activeUnit = GetCurrentActiveUnit();
        RefreshTimeline();
        RefreshAllies();
        RefreshEnemies();
        RefreshHand();
        RefreshCommands();
        RefreshHeader();
        statusText.text = message;
    }

    private void RefreshHeader()
    {
        string activeName = activeUnit != null ? activeUnit.DisplayName : "None";
        string side = activeUnit != null && activeUnit.IsAlly ? "PLAYER TURN" : "ENEMY TURN";
        if (GetFirstAliveEnemy() == null)
        {
            side = "VICTORY";
            activeName = "All enemies defeated";
        }
        else if (GetFirstAliveAlly() == null)
        {
            side = "DEFEAT";
            activeName = "Party defeated";
        }

        turnText.text = side + " / TICK " + currentTick.ToString("000") + " / ACTIVE: " + activeName;
        selectedText.text = "Selected Ally: " + (selectedAlly != null ? selectedAlly.Name : "-") + "   Target: " + (selectedEnemy != null ? selectedEnemy.Name : "-");
    }

    private void RefreshTimeline()
    {
        List<TimelinePreview> previews = BuildTimelinePreview();
        for (int i = 0; i < timelineViews.Count; i++)
        {
            TimelineSlotView view = timelineViews[i];
            if (i >= previews.Count)
            {
                view.Panel.gameObject.SetActive(false);
                continue;
            }

            TimelinePreview preview = previews[i];
            view.Panel.gameObject.SetActive(true);
            bool active = i == 0;
            bool ally = preview.Unit.IsAlly;
            view.Panel.color = active
                ? new Color(0.09f, 0.15f, 0.11f, 0.98f)
                : new Color(0.035f, 0.07f, 0.09f, 0.96f);
            view.Accent.color = ally ? GetPositionColor(preview.Unit.Ally.Position) : new Color(1f, 0.32f, 0.42f, 1f);
            view.NameText.text = preview.Unit.DisplayName;
            view.DetailText.text = (ally ? preview.Unit.Ally.Position.ToString() : preview.Unit.Enemy.NextAction) + "\nT+" + preview.DeltaTick + " SPD " + preview.Unit.Speed;
        }
    }

    private List<TimelinePreview> BuildTimelinePreview()
    {
        List<TimelineUnit> simulatedUnits = new List<TimelineUnit>();
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].IsAlive)
            {
                simulatedUnits.Add(new TimelineUnit { IsAlly = true, Ally = allies[i], ReadyTick = allies[i].NextReadyTick, Sequence = i });
            }
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsAlive)
            {
                simulatedUnits.Add(new TimelineUnit { IsAlly = false, Enemy = enemies[i], ReadyTick = enemies[i].NextReadyTick, Sequence = allies.Count + i });
            }
        }

        List<TimelinePreview> previews = new List<TimelinePreview>();
        for (int i = 0; i < TimelinePreviewCount && simulatedUnits.Count > 0; i++)
        {
            TimelineUnit next = GetEarliestUnit(simulatedUnits);
            previews.Add(new TimelinePreview { Unit = next, DeltaTick = Mathf.Max(0, next.ReadyTick - currentTick) });
            next.ReadyTick += CalculateRecovery(next.Speed, 1);
            next.Sequence += 10;
        }

        return previews;
    }

    private void RefreshAllies()
    {
        foreach (KeyValuePair<PartyPosition, AllyView> pair in allyViews)
        {
            PartyPosition position = pair.Key;
            AllyView view = pair.Value;
            AllyUnit ally = GetAllyAtPosition(position);
            bool selected = ally != null && selectedAlly == ally;
            bool active = activeUnit != null && activeUnit.IsAlly && activeUnit.Ally == ally;
            view.Panel.color = active
                ? new Color(0.08f, 0.16f, 0.12f, 0.98f)
                : selected ? new Color(0.08f, 0.12f, 0.16f, 0.98f) : new Color(0.028f, 0.055f, 0.07f, 0.96f);
            view.Accent.color = GetPositionColor(position);
            view.PositionText.text = position.ToString().ToUpperInvariant();
            view.NameText.text = ally != null ? ally.Name : "-";
            view.DetailText.text = ally != null
                ? "HP " + ally.Hp + "/" + ally.MaxHp + "   SPD " + ally.Speed + "   " + ally.Status
                : "Empty";
        }
    }

    private void RefreshEnemies()
    {
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                EnemyCellView view = enemyCellViews[row, column];
                EnemyUnit enemy = GetEnemyAt(row, column);
                bool selected = enemy != null && selectedEnemy == enemy;
                if (enemy == null)
                {
                    view.Panel.color = new Color(0.025f, 0.052f, 0.065f, 0.98f);
                    view.Label.text = "EMPTY\n[" + row + "," + column + "]";
                    continue;
                }

                view.Panel.color = selected
                    ? new Color(0.18f, 0.08f, 0.12f, 0.98f)
                    : new Color(0.07f, 0.035f, 0.048f, 0.98f);
                view.Label.text = enemy.Name + "\nHP " + enemy.Hp + "/" + enemy.MaxHp + "\n" + enemy.Attribute + "\nNext: " + enemy.NextAction;
            }
        }
    }

    private void RefreshHand()
    {
        for (int i = 0; i < handViews.Count; i++)
        {
            CardButtonView view = handViews[i];
            if (i >= hand.Count || hand[i] == null)
            {
                view.Button.interactable = false;
                view.Panel.color = new Color(0.07f, 0.08f, 0.10f, 0.8f);
                view.NameText.text = "-";
                view.DetailText.text = string.Empty;
                continue;
            }

            CardData card = hand[i];
            bool queued = queuedHandSlots[i];
            view.Button.interactable = IsPlayerTurn() && !queued;
            view.Panel.color = queued ? new Color(0.35f, 0.30f, 0.12f, 1f) : GetCardColor(card);
            view.NameText.text = (i + 1) + ". " + GetCardDisplayName(card);
            view.DetailText.text = FormatCardMeta(card);
        }

        deckText.text = "Deck " + drawPile.Count + " / Discard " + discardPile.Count;
        queueText.text = FormatQueueText();
    }

    private void RefreshCommands()
    {
        bool canAct = IsPlayerTurn() && GetFirstAliveEnemy() != null && GetFirstAliveAlly() != null;
        weaponButton.interactable = canAct && GetQueuedActionCost() < MaxQueuedActions;
        swapFrontMiddleButton.interactable = canAct && GetQueuedActionCost() < MaxQueuedActions;
        swapMiddleBackButton.interactable = canAct && GetQueuedActionCost() < MaxQueuedActions;
        swapFrontBackButton.interactable = canAct && GetQueuedActionCost() < MaxQueuedActions;
        resetButton.interactable = queuedActions.Count > 0;
        confirmButton.interactable = activeUnit != null && GetFirstAliveEnemy() != null && GetFirstAliveAlly() != null;
        Text confirmLabel = confirmButton.GetComponentInChildren<Text>();
        if (confirmLabel != null)
        {
            confirmLabel.text = IsPlayerTurn() ? "Confirm" : "Enemy Act";
        }
    }

    private string FormatQueueText()
    {
        if (queuedActions.Count == 0)
        {
            return "Queue: empty / normal actions " + GetQueuedActionCost() + "/" + MaxQueuedActions;
        }

        string text = "Queue: ";
        for (int i = 0; i < queuedActions.Count; i++)
        {
            if (i > 0)
            {
                text += " > ";
            }

            text += queuedActions[i].Label + (queuedActions[i].ConsumesAction ? "" : " [CLEAR]");
        }

        text += " / normal actions " + GetQueuedActionCost() + "/" + MaxQueuedActions;
        return text;
    }

    private int GetQueuedActionCost()
    {
        int cost = 0;
        for (int i = 0; i < queuedActions.Count; i++)
        {
            if (queuedActions[i].ConsumesAction)
            {
                cost++;
            }
        }

        return cost;
    }

    private bool IsPlayerTurn()
    {
        return activeUnit != null && activeUnit.IsAlly && GetFirstAliveEnemy() != null && GetFirstAliveAlly() != null;
    }

    private TimelineUnit GetCurrentActiveUnit()
    {
        List<TimelineUnit> units = new List<TimelineUnit>();
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].IsAlive)
            {
                units.Add(new TimelineUnit { IsAlly = true, Ally = allies[i], ReadyTick = allies[i].NextReadyTick, Sequence = i });
            }
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsAlive)
            {
                units.Add(new TimelineUnit { IsAlly = false, Enemy = enemies[i], ReadyTick = enemies[i].NextReadyTick, Sequence = allies.Count + i });
            }
        }

        return GetEarliestUnit(units);
    }

    private TimelineUnit GetEarliestUnit(List<TimelineUnit> units)
    {
        TimelineUnit next = null;
        for (int i = 0; i < units.Count; i++)
        {
            TimelineUnit candidate = units[i];
            if (next == null
                || candidate.ReadyTick < next.ReadyTick
                || (candidate.ReadyTick == next.ReadyTick && candidate.Sequence < next.Sequence))
            {
                next = candidate;
            }
        }

        return next;
    }

    private AllyUnit GetAllyAtPosition(PartyPosition position)
    {
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].Position == position && allies[i].IsAlive)
            {
                return allies[i];
            }
        }

        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].Position == position)
            {
                return allies[i];
            }
        }

        return null;
    }

    private AllyUnit GetFirstAliveAlly()
    {
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].IsAlive)
            {
                return allies[i];
            }
        }

        return null;
    }

    private EnemyUnit GetEnemyAt(int row, int column)
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyUnit enemy = enemies[i];
            if (enemy.GridPosition.x == row && enemy.GridPosition.y == column)
            {
                return enemy;
            }
        }

        return null;
    }

    private EnemyUnit GetFirstAliveEnemy()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsAlive)
            {
                return enemies[i];
            }
        }

        return null;
    }

    private static string GetCardDisplayName(CardData card)
    {
        if (card == null)
        {
            return "-";
        }

        return string.IsNullOrEmpty(card.Name) ? card.CardId : card.Name;
    }

    private static string FormatCardMeta(CardData card)
    {
        if (card == null)
        {
            return string.Empty;
        }

        string clear = card.IsClearCard ? " CLEAR" : string.Empty;
        if (card.Effect == CardEffectType.Repair)
        {
            return card.DeckType + clear + "\nHeal " + card.Power;
        }

        if (card.Effect == CardEffectType.Damage)
        {
            return card.DeckType + clear + "\n" + card.Attribute + " " + card.Power;
        }

        return card.DeckType + clear + "\n" + card.Effect + " " + card.Power;
    }

    private static Color GetCardColor(CardData card)
    {
        if (card == null)
        {
            return new Color(0.10f, 0.12f, 0.15f, 1f);
        }

        if (card.IsClearCard)
        {
            return new Color(0.16f, 0.32f, 0.24f, 1f);
        }

        switch (card.DeckType)
        {
            case CardDeckType.HC:
                return new Color(0.14f, 0.25f, 0.42f, 1f);
            case CardDeckType.G:
                return new Color(0.38f, 0.16f, 0.18f, 1f);
            default:
                return new Color(0.13f, 0.17f, 0.22f, 1f);
        }
    }

    private static Color GetPositionColor(PartyPosition position)
    {
        switch (position)
        {
            case PartyPosition.Front:
                return new Color(0.18f, 0.95f, 1f, 1f);
            case PartyPosition.Middle:
                return new Color(0.68f, 1f, 0.28f, 1f);
            default:
                return new Color(1f, 0.78f, 0.24f, 1f);
        }
    }

    private static string ShortPosition(PartyPosition position)
    {
        switch (position)
        {
            case PartyPosition.Front:
                return "F";
            case PartyPosition.Middle:
                return "M";
            default:
                return "B";
        }
    }

    private RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        RectTransform rectTransform = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        Image image = rectTransform.gameObject.AddComponent<Image>();
        image.color = color;
        return rectTransform;
    }

    private RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
        return rectTransform;
    }

    private Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        RectTransform rectTransform = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Image image = rectTransform.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string text, int fontSize, TextAnchor alignment, Color color)
    {
        RectTransform rectTransform = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Text label = rectTransform.gameObject.AddComponent<Text>();
        label.text = text;
        label.font = uiFont;
        label.fontSize = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.alignment = alignment;
        label.color = color;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 9;
        label.resizeTextMaxSize = fontSize;

        Shadow shadow = rectTransform.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return label;
    }

    private Button CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string labelText, int fontSize, Color color)
    {
        Image image = CreateImage(name, parent, anchorMin, anchorMax, offsetMin, offsetMax, color);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.24f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.08f, 0.08f, 0.08f, 0.55f);
        button.colors = colors;

        CreateText(name + " Text", image.transform, Vector2.zero, Vector2.one, new Vector2(8f, 6f), new Vector2(-8f, -6f), labelText, fontSize, TextAnchor.MiddleCenter, Color.white);
        return button;
    }
}
