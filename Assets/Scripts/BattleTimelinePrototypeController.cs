using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class BattleTimelinePrototypeController : MonoBehaviour
{
    private const int TimelineWindowTicks = 120;
    private const int MaxPreviewEvents = 8;

    private readonly List<TimelineActor> actors = new List<TimelineActor>();
    private readonly List<TimelineEvent> previewEvents = new List<TimelineEvent>();
    private readonly List<TimelineEventView> eventViews = new List<TimelineEventView>();
    private readonly List<ActorPanelView> actorPanelViews = new List<ActorPanelView>();
    private readonly List<Text> tickMarkerLabels = new List<Text>();

    private Font uiFont;
    private RectTransform timelineTrackRoot;
    private Text tickText;
    private Text selectedActorText;
    private Text projectedOrderText;
    private Text detailText;
    private Text logText;
    private Button resolveButton;
    private Button advanceButton;
    private Button resetButton;
    private Button speedDownButton;
    private Button speedUpButton;

    private int currentTick;
    private int selectedActorIndex;
    private int logSerial;

    private sealed class TimelineActor
    {
        public string Name;
        public string Side;
        public Color Color;
        public int Speed;
        public int BaseRecovery;
        public int NextReadyTick;
        public int ActionIndex;
        public string[] Actions;
        public int[] Weights;
    }

    private struct TimelineEvent
    {
        public TimelineActor Actor;
        public int ReadyTick;
        public string ActionName;
        public int Recovery;
        public int Sequence;
    }

    private sealed class SimActorState
    {
        public TimelineActor Actor;
        public int ReadyTick;
        public int ActionIndex;
    }

    private sealed class TimelineEventView
    {
        public RectTransform Root;
        public Image Panel;
        public Image Accent;
        public Text Header;
        public Text Action;
        public Text Time;
    }

    private sealed class ActorPanelView
    {
        public Image Panel;
        public Image Accent;
        public Text Name;
        public Text Stat;
        public Button SelectButton;
    }

    private void Awake()
    {
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
        ResetPrototypeData();
        BuildUi();
        AddLog("Prototype ready. Resolve the next action or adjust an actor speed.");
        RefreshAll();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            ResolveNextEvent();
        }
        else if (keyboard.tKey.wasPressedThisFrame)
        {
            AdvanceTicks(10);
        }
        else if (keyboard.rKey.wasPressedThisFrame)
        {
            ResetPrototype();
        }
        else if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            SelectActor(selectedActorIndex - 1);
        }
        else if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            SelectActor(selectedActorIndex + 1);
        }
        else if (keyboard.minusKey.wasPressedThisFrame)
        {
            AdjustSelectedSpeed(-5);
        }
        else if (keyboard.equalsKey.wasPressedThisFrame)
        {
            AdjustSelectedSpeed(5);
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

    private void ResetPrototypeData()
    {
        actors.Clear();
        actors.Add(new TimelineActor
        {
            Name = "Player",
            Side = "PLAYER",
            Color = new Color(0.16f, 0.95f, 1f, 1f),
            Speed = 45,
            BaseRecovery = 42,
            NextReadyTick = 12,
            Actions = new[] { "Quick Shot", "Rail Cannon", "Guard" },
            Weights = new[] { 0, 12, -8 }
        });
        actors.Add(new TimelineActor
        {
            Name = "Enemy",
            Side = "ENEMY",
            Color = new Color(1f, 0.28f, 0.42f, 1f),
            Speed = 36,
            BaseRecovery = 48,
            NextReadyTick = 28,
            Actions = new[] { "Rush", "Wide Shot", "Charge" },
            Weights = new[] { 0, 8, -10 }
        });
        actors.Add(new TimelineActor
        {
            Name = "Support Bit",
            Side = "ALLY",
            Color = new Color(0.58f, 1f, 0.26f, 1f),
            Speed = 62,
            BaseRecovery = 35,
            NextReadyTick = 20,
            Actions = new[] { "Scan", "Heal Pulse", "Boost" },
            Weights = new[] { -6, 10, 4 }
        });
        actors.Add(new TimelineActor
        {
            Name = "Hazard Field",
            Side = "FIELD",
            Color = new Color(1f, 0.78f, 0.2f, 1f),
            Speed = 25,
            BaseRecovery = 55,
            NextReadyTick = 42,
            Actions = new[] { "Magma Tick", "Crack Panel", "Poison Vent" },
            Weights = new[] { 6, 12, 0 }
        });

        currentTick = 0;
        selectedActorIndex = 0;
        logSerial = 0;
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

        BuildGrid(canvasObject.transform);
        BuildHeader(canvasObject.transform);
        BuildTimeline(canvasObject.transform);
        BuildActorPanels(canvasObject.transform);
        BuildControlPanel(canvasObject.transform);
        BuildInfoPanel(canvasObject.transform);
    }

    private void BuildGrid(Transform parent)
    {
        Color verticalColor = new Color(0.10f, 0.70f, 0.85f, 0.08f);
        Color horizontalColor = new Color(0.80f, 0.95f, 0.24f, 0.06f);
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
        CreateImage("Header Backplate", parent, new Vector2(0.035f, 0.86f), new Vector2(0.965f, 0.965f), Vector2.zero, Vector2.zero, new Color(0.012f, 0.04f, 0.055f, 0.88f)).raycastTarget = false;
        CreateImage("Header Accent", parent, new Vector2(0.035f, 0.86f), new Vector2(0.965f, 0.868f), Vector2.zero, Vector2.zero, new Color(0.1f, 0.88f, 1f, 0.92f)).raycastTarget = false;
        CreateText("Title", parent, new Vector2(0.055f, 0.895f), new Vector2(0.55f, 0.955f), Vector2.zero, Vector2.zero, "BATTLE TIMELINE PROTOTYPE", 33, TextAnchor.MiddleLeft, new Color(0.9f, 1f, 1f));
        CreateText("Subtitle", parent, new Vector2(0.055f, 0.865f), new Vector2(0.55f, 0.905f), Vector2.zero, Vector2.zero, "Queue visibility / recovery preview / speed tuning", 17, TextAnchor.MiddleLeft, new Color(0.68f, 0.9f, 0.95f));
        tickText = CreateText("Tick Text", parent, new Vector2(0.68f, 0.89f), new Vector2(0.945f, 0.955f), Vector2.zero, Vector2.zero, string.Empty, 31, TextAnchor.MiddleRight, new Color(1f, 0.86f, 0.28f));
        CreateText("Keyboard Hint", parent, new Vector2(0.62f, 0.865f), new Vector2(0.945f, 0.905f), Vector2.zero, Vector2.zero, "Space Resolve / T +10 / R Reset / Arrows Select / +/- Speed", 15, TextAnchor.MiddleRight, new Color(0.72f, 0.85f, 0.9f));
    }

    private void BuildTimeline(Transform parent)
    {
        RectTransform panel = CreateRect("Timeline Panel", parent, new Vector2(0.035f, 0.46f), new Vector2(0.965f, 0.835f), Vector2.zero, Vector2.zero);
        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.018f, 0.035f, 0.045f, 0.94f);

        CreateText("Timeline Label", panel, new Vector2(0.02f, 0.82f), new Vector2(0.28f, 0.96f), Vector2.zero, Vector2.zero, "UPCOMING ACTIONS", 22, TextAnchor.MiddleLeft, new Color(0.88f, 1f, 1f));
        projectedOrderText = CreateText("Projected Order", panel, new Vector2(0.3f, 0.82f), new Vector2(0.98f, 0.96f), Vector2.zero, Vector2.zero, string.Empty, 18, TextAnchor.MiddleRight, new Color(0.84f, 0.92f, 0.95f));

        timelineTrackRoot = CreateRect("Timeline Track", panel, new Vector2(0.04f, 0.16f), new Vector2(0.96f, 0.78f), Vector2.zero, Vector2.zero);
        Image trackBack = timelineTrackRoot.gameObject.AddComponent<Image>();
        trackBack.color = new Color(0.004f, 0.012f, 0.018f, 0.96f);

        CreateImage("Timeline Center Line", timelineTrackRoot, new Vector2(0.04f, 0.49f), new Vector2(0.96f, 0.51f), Vector2.zero, Vector2.zero, new Color(0.28f, 0.56f, 0.62f, 0.55f)).raycastTarget = false;
        CreateImage("Now Line", timelineTrackRoot, new Vector2(0.04f, 0.02f), new Vector2(0.04f, 0.98f), new Vector2(-2f, 0f), new Vector2(2f, 0f), new Color(1f, 0.86f, 0.28f, 0.85f)).raycastTarget = false;
        CreateText("Now Label", timelineTrackRoot, new Vector2(0f, 0.86f), new Vector2(0.08f, 1f), Vector2.zero, Vector2.zero, "NOW", 14, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.28f));

        int[] markerTicks = { 0, 30, 60, 90, 120 };
        for (int i = 0; i < markerTicks.Length; i++)
        {
            float x = 0.04f + 0.92f * markerTicks[i] / TimelineWindowTicks;
            CreateImage("Tick Marker " + markerTicks[i], timelineTrackRoot, new Vector2(x, 0.08f), new Vector2(x, 0.18f), new Vector2(-1f, 0f), new Vector2(1f, 0f), new Color(0.68f, 0.88f, 0.92f, 0.4f)).raycastTarget = false;
            Text markerLabel = CreateText("Tick Marker Label " + markerTicks[i], timelineTrackRoot, new Vector2(x - 0.035f, 0f), new Vector2(x + 0.035f, 0.1f), Vector2.zero, Vector2.zero, string.Empty, 12, TextAnchor.MiddleCenter, new Color(0.66f, 0.78f, 0.82f));
            markerLabel.raycastTarget = false;
            tickMarkerLabels.Add(markerLabel);
        }

        for (int i = 0; i < MaxPreviewEvents; i++)
        {
            eventViews.Add(CreateTimelineEventView(timelineTrackRoot, i));
        }
    }

    private TimelineEventView CreateTimelineEventView(Transform parent, int index)
    {
        RectTransform root = CreateRect("Timeline Event " + index, parent, Vector2.zero, Vector2.zero, new Vector2(-62f, -42f), new Vector2(62f, 42f));
        root.pivot = new Vector2(0.5f, 0.5f);

        Image panel = root.gameObject.AddComponent<Image>();
        panel.color = new Color(0.035f, 0.07f, 0.09f, 0.98f);

        Image accent = CreateImage("Timeline Event Accent " + index, root, new Vector2(0f, 0.9f), Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        accent.raycastTarget = false;

        Text header = CreateText("Timeline Event Header " + index, root, new Vector2(0.06f, 0.58f), new Vector2(0.94f, 0.9f), Vector2.zero, Vector2.zero, string.Empty, 17, TextAnchor.MiddleCenter, Color.white);
        Text action = CreateText("Timeline Event Action " + index, root, new Vector2(0.06f, 0.26f), new Vector2(0.94f, 0.62f), Vector2.zero, Vector2.zero, string.Empty, 14, TextAnchor.MiddleCenter, new Color(0.92f, 1f, 0.94f));
        Text time = CreateText("Timeline Event Time " + index, root, new Vector2(0.06f, 0.02f), new Vector2(0.94f, 0.28f), Vector2.zero, Vector2.zero, string.Empty, 13, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.28f));

        return new TimelineEventView
        {
            Root = root,
            Panel = panel,
            Accent = accent,
            Header = header,
            Action = action,
            Time = time
        };
    }

    private void BuildActorPanels(Transform parent)
    {
        RectTransform panel = CreateRect("Actor Panel", parent, new Vector2(0.035f, 0.16f), new Vector2(0.63f, 0.425f), Vector2.zero, Vector2.zero);
        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.014f, 0.032f, 0.04f, 0.94f);

        CreateText("Actor Label", panel, new Vector2(0.03f, 0.76f), new Vector2(0.5f, 0.96f), Vector2.zero, Vector2.zero, "ACTORS", 20, TextAnchor.MiddleLeft, new Color(0.88f, 1f, 1f));
        selectedActorText = CreateText("Selected Actor Text", panel, new Vector2(0.42f, 0.76f), new Vector2(0.97f, 0.96f), Vector2.zero, Vector2.zero, string.Empty, 18, TextAnchor.MiddleRight, new Color(1f, 0.9f, 0.3f));

        for (int i = 0; i < actors.Count; i++)
        {
            int row = i / 2;
            int column = i % 2;
            float minX = 0.03f + column * 0.485f;
            float maxX = minX + 0.455f;
            float maxY = 0.70f - row * 0.31f;
            float minY = maxY - 0.24f;
            ActorPanelView view = CreateActorPanelView(panel, i, new Vector2(minX, minY), new Vector2(maxX, maxY));
            actorPanelViews.Add(view);
        }
    }

    private ActorPanelView CreateActorPanelView(Transform parent, int actorIndex, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform root = CreateRect("Actor View " + actorIndex, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        Image panel = root.gameObject.AddComponent<Image>();
        panel.color = new Color(0.028f, 0.055f, 0.07f, 0.96f);
        Button button = root.gameObject.AddComponent<Button>();
        button.targetGraphic = panel;

        Image accent = CreateImage("Actor Accent " + actorIndex, root, Vector2.zero, new Vector2(0.02f, 1f), Vector2.zero, Vector2.zero, Color.white);
        accent.raycastTarget = false;

        Text name = CreateText("Actor Name " + actorIndex, root, new Vector2(0.08f, 0.52f), new Vector2(0.96f, 0.96f), Vector2.zero, Vector2.zero, string.Empty, 20, TextAnchor.MiddleLeft, Color.white);
        Text stat = CreateText("Actor Stat " + actorIndex, root, new Vector2(0.08f, 0.06f), new Vector2(0.96f, 0.52f), Vector2.zero, Vector2.zero, string.Empty, 15, TextAnchor.MiddleLeft, new Color(0.78f, 0.9f, 0.94f));

        int capturedIndex = actorIndex;
        button.onClick.AddListener(() => SelectActor(capturedIndex));

        return new ActorPanelView
        {
            Panel = panel,
            Accent = accent,
            Name = name,
            Stat = stat,
            SelectButton = button
        };
    }

    private void BuildControlPanel(Transform parent)
    {
        RectTransform panel = CreateRect("Control Panel", parent, new Vector2(0.66f, 0.16f), new Vector2(0.965f, 0.425f), Vector2.zero, Vector2.zero);
        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.014f, 0.032f, 0.04f, 0.94f);

        CreateText("Control Label", panel, new Vector2(0.05f, 0.76f), new Vector2(0.95f, 0.96f), Vector2.zero, Vector2.zero, "CONTROLS", 20, TextAnchor.MiddleLeft, new Color(0.88f, 1f, 1f));

        resolveButton = CreateButton("Resolve Button", panel, new Vector2(0.05f, 0.54f), new Vector2(0.95f, 0.72f), Vector2.zero, Vector2.zero, "Resolve Next Action", 20, new Color(0.08f, 0.44f, 0.28f, 1f));
        resolveButton.onClick.AddListener(ResolveNextEvent);

        advanceButton = CreateButton("Advance Button", panel, new Vector2(0.05f, 0.35f), new Vector2(0.47f, 0.50f), Vector2.zero, Vector2.zero, "+10 Tick", 17, new Color(0.13f, 0.30f, 0.44f, 1f));
        advanceButton.onClick.AddListener(() => AdvanceTicks(10));

        resetButton = CreateButton("Reset Button", panel, new Vector2(0.53f, 0.35f), new Vector2(0.95f, 0.50f), Vector2.zero, Vector2.zero, "Reset", 17, new Color(0.42f, 0.16f, 0.12f, 1f));
        resetButton.onClick.AddListener(ResetPrototype);

        speedDownButton = CreateButton("Speed Down Button", panel, new Vector2(0.05f, 0.12f), new Vector2(0.29f, 0.28f), Vector2.zero, Vector2.zero, "SPD -", 16, new Color(0.18f, 0.20f, 0.28f, 1f));
        speedDownButton.onClick.AddListener(() => AdjustSelectedSpeed(-5));

        speedUpButton = CreateButton("Speed Up Button", panel, new Vector2(0.33f, 0.12f), new Vector2(0.57f, 0.28f), Vector2.zero, Vector2.zero, "SPD +", 16, new Color(0.18f, 0.20f, 0.28f, 1f));
        speedUpButton.onClick.AddListener(() => AdjustSelectedSpeed(5));

        Button primeButton = CreateButton("Prime Button", panel, new Vector2(0.61f, 0.12f), new Vector2(0.95f, 0.28f), Vector2.zero, Vector2.zero, "Prime Now", 16, new Color(0.24f, 0.24f, 0.10f, 1f));
        primeButton.onClick.AddListener(PrimeSelectedActor);
    }

    private void BuildInfoPanel(Transform parent)
    {
        RectTransform panel = CreateRect("Info Panel", parent, new Vector2(0.035f, 0.035f), new Vector2(0.965f, 0.13f), Vector2.zero, Vector2.zero);
        Image panelImage = panel.gameObject.AddComponent<Image>();
        panelImage.color = new Color(0.012f, 0.026f, 0.034f, 0.94f);

        detailText = CreateText("Detail Text", panel, new Vector2(0.02f, 0.18f), new Vector2(0.48f, 0.82f), Vector2.zero, Vector2.zero, string.Empty, 16, TextAnchor.MiddleLeft, new Color(0.86f, 1f, 0.95f));
        logText = CreateText("Log Text", panel, new Vector2(0.50f, 0.18f), new Vector2(0.98f, 0.82f), Vector2.zero, Vector2.zero, string.Empty, 16, TextAnchor.MiddleRight, new Color(0.96f, 0.96f, 0.82f));
    }

    private void RefreshAll()
    {
        BuildPreviewEvents();
        RefreshHeader();
        RefreshTimeline();
        RefreshActors();
        RefreshDetails();
    }

    private void RefreshHeader()
    {
        tickText.text = "TICK " + currentTick.ToString("000");
    }

    private void RefreshTimeline()
    {
        for (int i = 0; i < tickMarkerLabels.Count; i++)
        {
            int tickOffset = i * 30;
            tickMarkerLabels[i].text = "+" + tickOffset;
        }

        for (int i = 0; i < eventViews.Count; i++)
        {
            TimelineEventView view = eventViews[i];
            if (i >= previewEvents.Count)
            {
                view.Root.gameObject.SetActive(false);
                continue;
            }

            TimelineEvent timelineEvent = previewEvents[i];
            int delta = Mathf.Max(0, timelineEvent.ReadyTick - currentTick);
            float progress = Mathf.Clamp01(delta / (float)TimelineWindowTicks);
            float x = 0.04f + 0.92f * progress;
            float y = i % 2 == 0 ? 0.64f : 0.35f;

            view.Root.gameObject.SetActive(true);
            view.Root.anchorMin = new Vector2(x, y);
            view.Root.anchorMax = new Vector2(x, y);
            view.Root.anchoredPosition = Vector2.zero;
            view.Accent.color = timelineEvent.Actor.Color;
            view.Panel.color = i == 0
                ? new Color(0.08f, 0.14f, 0.10f, 0.98f)
                : new Color(0.032f, 0.062f, 0.078f, 0.96f);
            view.Header.text = timelineEvent.Actor.Name;
            view.Action.text = timelineEvent.ActionName;
            view.Time.text = "T+" + delta + " / rec " + timelineEvent.Recovery;
        }

        projectedOrderText.text = BuildProjectedOrderText();
    }

    private string BuildProjectedOrderText()
    {
        string orderText = "Order: ";
        int count = Mathf.Min(previewEvents.Count, 5);
        for (int i = 0; i < count; i++)
        {
            if (i > 0)
            {
                orderText += " > ";
            }

            orderText += previewEvents[i].Actor.Name;
        }

        return orderText;
    }

    private void RefreshActors()
    {
        selectedActorIndex = Mathf.Clamp(selectedActorIndex, 0, actors.Count - 1);
        for (int i = 0; i < actorPanelViews.Count; i++)
        {
            ActorPanelView view = actorPanelViews[i];
            TimelineActor actor = actors[i];
            bool selected = i == selectedActorIndex;
            int delta = Mathf.Max(0, actor.NextReadyTick - currentTick);

            view.Panel.color = selected
                ? new Color(0.08f, 0.14f, 0.16f, 0.98f)
                : new Color(0.028f, 0.055f, 0.07f, 0.96f);
            view.Accent.color = actor.Color;
            view.Name.text = actor.Name + "  [" + actor.Side + "]";
            view.Stat.text = "Speed " + actor.Speed + " / Ready T+" + delta + " / Next " + PeekAction(actor);
        }
    }

    private void RefreshDetails()
    {
        TimelineActor selectedActor = actors[selectedActorIndex];
        int selectedRecovery = CalculateRecovery(selectedActor, selectedActor.ActionIndex);
        selectedActorText.text = "Selected: " + selectedActor.Name;
        detailText.text = selectedActor.Name + " speed tuning changes future recovery. Current next action: " + PeekAction(selectedActor) + " / projected recovery " + selectedRecovery + ".";
    }

    private void BuildPreviewEvents()
    {
        previewEvents.Clear();
        List<SimActorState> simulatedActors = new List<SimActorState>();
        for (int i = 0; i < actors.Count; i++)
        {
            TimelineActor actor = actors[i];
            simulatedActors.Add(new SimActorState
            {
                Actor = actor,
                ReadyTick = actor.NextReadyTick,
                ActionIndex = actor.ActionIndex
            });
        }

        for (int i = 0; i < MaxPreviewEvents; i++)
        {
            SimActorState nextState = GetNextSimulatedActor(simulatedActors);
            if (nextState == null)
            {
                break;
            }

            int recovery = CalculateRecovery(nextState.Actor, nextState.ActionIndex);
            string actionName = GetActionName(nextState.Actor, nextState.ActionIndex);
            previewEvents.Add(new TimelineEvent
            {
                Actor = nextState.Actor,
                ReadyTick = nextState.ReadyTick,
                ActionName = actionName,
                Recovery = recovery,
                Sequence = i
            });

            nextState.ReadyTick += recovery;
            nextState.ActionIndex++;
        }
    }

    private SimActorState GetNextSimulatedActor(List<SimActorState> simulatedActors)
    {
        SimActorState next = null;
        for (int i = 0; i < simulatedActors.Count; i++)
        {
            SimActorState candidate = simulatedActors[i];
            if (next == null || candidate.ReadyTick < next.ReadyTick)
            {
                next = candidate;
            }
        }

        return next;
    }

    private void ResolveNextEvent()
    {
        TimelineActor actor = GetNextActor();
        if (actor == null)
        {
            return;
        }

        currentTick = Mathf.Max(currentTick, actor.NextReadyTick);
        string actionName = GetActionName(actor, actor.ActionIndex);
        int recovery = CalculateRecovery(actor, actor.ActionIndex);
        actor.NextReadyTick = currentTick + recovery;
        actor.ActionIndex++;
        AddLog(actor.Name + " resolved " + actionName + ". Next ready in " + recovery + " ticks.");
        RefreshAll();
    }

    private TimelineActor GetNextActor()
    {
        TimelineActor next = null;
        for (int i = 0; i < actors.Count; i++)
        {
            TimelineActor candidate = actors[i];
            if (next == null || candidate.NextReadyTick < next.NextReadyTick)
            {
                next = candidate;
            }
        }

        return next;
    }

    private void AdvanceTicks(int ticks)
    {
        currentTick += Mathf.Max(1, ticks);
        AddLog("Advanced timeline by " + ticks + " ticks.");
        RefreshAll();
    }

    private void ResetPrototype()
    {
        ResetPrototypeData();
        AddLog("Prototype reset.");
        RefreshAll();
    }

    private void SelectActor(int index)
    {
        if (actors.Count == 0)
        {
            selectedActorIndex = 0;
        }
        else
        {
            selectedActorIndex = (index + actors.Count) % actors.Count;
        }

        RefreshAll();
    }

    private void AdjustSelectedSpeed(int delta)
    {
        TimelineActor actor = actors[selectedActorIndex];
        actor.Speed = Mathf.Clamp(actor.Speed + delta, 5, 95);
        AddLog(actor.Name + " speed set to " + actor.Speed + ".");
        RefreshAll();
    }

    private void PrimeSelectedActor()
    {
        TimelineActor actor = actors[selectedActorIndex];
        actor.NextReadyTick = currentTick + 3;
        AddLog(actor.Name + " primed near the front of the timeline.");
        RefreshAll();
    }

    private string PeekAction(TimelineActor actor)
    {
        return GetActionName(actor, actor.ActionIndex);
    }

    private string GetActionName(TimelineActor actor, int actionIndex)
    {
        if (actor.Actions == null || actor.Actions.Length == 0)
        {
            return "Wait";
        }

        return actor.Actions[Mathf.Abs(actionIndex) % actor.Actions.Length];
    }

    private int CalculateRecovery(TimelineActor actor, int actionIndex)
    {
        int weight = 0;
        if (actor.Weights != null && actor.Weights.Length > 0)
        {
            weight = actor.Weights[Mathf.Abs(actionIndex) % actor.Weights.Length];
        }

        int speedDiscount = Mathf.RoundToInt(actor.Speed * 0.35f);
        return Mathf.Clamp(actor.BaseRecovery + weight - speedDiscount, 10, 90);
    }

    private void AddLog(string message)
    {
        logSerial++;
        if (logText != null)
        {
            logText.text = "[" + logSerial.ToString("00") + "] " + message;
        }
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
        label.resizeTextMinSize = 10;
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
        colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        button.colors = colors;

        CreateText(name + " Text", image.transform, Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f), labelText, fontSize, TextAnchor.MiddleCenter, Color.white);
        return button;
    }
}
