using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuController : MonoBehaviour
{
    private const int MenuBattle = 0;
    private const int MenuDeckBuild = 1;
    private const int MenuRpg = 2;

    private readonly string[] menuTexts = { "バトルへ", "デッキ編集へ", "RPGへ" };
    private readonly Button[] menuButtons = new Button[3];
    private readonly Text[] menuLabels = new Text[3];
    private readonly Image[] menuBackgrounds = new Image[3];

    private Font uiFont;
    private Text messageText;
    private int selectedIndex;

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
        BuildUi();
        SelectMenu(MenuBattle);
        SetMessage("↑ / ↓ で選択、Enterで決定");
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            MoveSelection(-1);
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            MoveSelection(1);
        }
        else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame)
        {
            ConfirmSelection();
        }
        else if (keyboard.escapeKey.wasPressedThisFrame)
        {
            SetMessage("Escape：プロトタイプでは終了処理は未実装です");
        }
    }

    private static Font CreateJapaneseFont()
    {
        string[] fontNames = { "Yu Gothic", "Meiryo", "MS Gothic", "Noto Sans CJK JP", "Arial" };
        return Font.CreateDynamicFontFromOSFont(fontNames, 20);
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
        camera.backgroundColor = new Color(0.025f, 0.035f, 0.055f);
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

    private void BuildUi()
    {
        GameObject canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image background = CreateImage("Background", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.025f, 0.035f, 0.055f));
        background.raycastTarget = false;

        BuildGrid(canvasObject.transform);
        BuildCircuitDecor(canvasObject.transform);
        BuildProgressMarkers(canvasObject.transform);
        BuildTitle(canvasObject.transform);
        BuildMenuButtons(canvasObject.transform);

        messageText = CreateText("Message Text", canvasObject.transform, new Vector2(0.32f, 0.14f), new Vector2(0.68f, 0.19f), Vector2.zero, Vector2.zero, string.Empty, 22, TextAnchor.MiddleCenter, new Color(0.95f, 1f, 0.78f));
        CreateText("Footer", canvasObject.transform, new Vector2(0.03f, 0.025f), new Vector2(0.97f, 0.065f), Vector2.zero, Vector2.zero, "© 2026 NEON CARDIA PROJECT  /  PROTOTYPE VERSION  /  MVP BUILD", 20, TextAnchor.MiddleCenter, new Color(0.75f, 0.9f, 0.92f));
    }

    private void BuildGrid(Transform parent)
    {
        Color gridColor = new Color(0.14f, 0.78f, 0.48f, 0.16f);
        for (int i = 0; i <= 24; i++)
        {
            float x = i / 24f;
            CreateImage("Grid Vertical " + i, parent, new Vector2(x, 0f), new Vector2(x, 1f), new Vector2(-1f, 0f), new Vector2(1f, 0f), gridColor).raycastTarget = false;
        }

        for (int i = 0; i <= 14; i++)
        {
            float y = i / 14f;
            CreateImage("Grid Horizontal " + i, parent, new Vector2(0f, y), new Vector2(1f, y), new Vector2(0f, -1f), new Vector2(0f, 1f), gridColor).raycastTarget = false;
        }
    }

    private void BuildCircuitDecor(Transform parent)
    {
        CreateText("Back Motif", parent, new Vector2(0.24f, 0.24f), new Vector2(0.76f, 0.84f), Vector2.zero, Vector2.zero, "◎", 360, TextAnchor.MiddleCenter, new Color(0.2f, 0.45f, 0.9f, 0.16f));
        CreateText("Back Symbol", parent, new Vector2(0.38f, 0.32f), new Vector2(0.62f, 0.7f), Vector2.zero, Vector2.zero, "◇", 220, TextAnchor.MiddleCenter, new Color(0.85f, 0.95f, 0.2f, 0.14f));

        Color cyan = new Color(0.12f, 0.95f, 1f, 0.45f);
        Color lime = new Color(0.58f, 1f, 0.18f, 0.45f);
        Color amber = new Color(1f, 0.76f, 0.2f, 0.36f);

        CreateCircuitLine(parent, "Circuit A", 0.08f, 0.78f, 0.22f, 3f, cyan);
        CreateCircuitLine(parent, "Circuit B", 0.08f, 0.78f, 0.055f, 3f, cyan);
        CreateCircuitLine(parent, "Circuit C", 0.74f, 0.82f, 0.18f, 3f, lime);
        CreateCircuitLine(parent, "Circuit D", 0.74f, 0.82f, 0.05f, 3f, lime);
        CreateCircuitLine(parent, "Circuit E", 0.12f, 0.28f, 0.26f, 4f, amber);
        CreateCircuitLine(parent, "Circuit F", 0.62f, 0.28f, 0.24f, 4f, amber);

        CreateNode(parent, "Node A", new Vector2(0.3f, 0.78f), cyan);
        CreateNode(parent, "Node B", new Vector2(0.69f, 0.78f), lime);
        CreateNode(parent, "Node C", new Vector2(0.42f, 0.29f), amber);
        CreateNode(parent, "Node D", new Vector2(0.86f, 0.29f), cyan);
    }

    private void CreateCircuitLine(Transform parent, string name, float x, float y, float width, float height, Color color)
    {
        CreateImage(name, parent, new Vector2(x, y), new Vector2(x + width, y), Vector2.zero, new Vector2(0f, height), color).raycastTarget = false;
    }

    private void CreateNode(Transform parent, string name, Vector2 center, Color color)
    {
        Image node = CreateImage(name, parent, center, center, new Vector2(-8f, -8f), new Vector2(8f, 8f), color);
        node.raycastTarget = false;
    }

    private void BuildProgressMarkers(Transform parent)
    {
        string[] markers = { "STAGE", "DECK", "BOSS", "CLEAR", "???" };
        for (int i = 0; i < markers.Length; i++)
        {
            float minX = 0.25f + i * 0.105f;
            float maxX = minX + 0.088f;
            Image panel = CreateImage("Progress Marker " + i, parent, new Vector2(minX, 0.9f), new Vector2(maxX, 0.96f), Vector2.zero, Vector2.zero, new Color(0.06f, 0.16f, 0.18f, 0.82f));
            panel.raycastTarget = false;
            CreateImage("Progress Marker Top " + i, panel.transform, new Vector2(0f, 0.85f), Vector2.one, Vector2.zero, Vector2.zero, i < 2 ? new Color(0.34f, 1f, 0.42f, 0.9f) : new Color(0.35f, 0.38f, 0.42f, 0.85f)).raycastTarget = false;
            CreateText("Progress Marker Text " + i, panel.transform, Vector2.zero, Vector2.one, new Vector2(6f, 0f), new Vector2(-6f, -2f), markers[i], 17, TextAnchor.MiddleCenter, i < 2 ? Color.white : new Color(0.58f, 0.68f, 0.7f));
        }
    }

    private void BuildTitle(Transform parent)
    {
        CreateText("Title Shadow", parent, new Vector2(0.12f, 0.62f), new Vector2(0.88f, 0.82f), new Vector2(9f, -9f), new Vector2(9f, -9f), "NEON CARDIA", 76, TextAnchor.MiddleCenter, new Color(0f, 0f, 0f, 0.82f));
        CreateText("Title Main", parent, new Vector2(0.12f, 0.62f), new Vector2(0.88f, 0.82f), Vector2.zero, Vector2.zero, "NEON CARDIA", 76, TextAnchor.MiddleCenter, new Color(0.7f, 1f, 0.18f));
        CreateText("Title Sub", parent, new Vector2(0.16f, 0.56f), new Vector2(0.84f, 0.63f), Vector2.zero, Vector2.zero, "ネオンカーディア  /  PANEL CARD BATTLE RPG", 28, TextAnchor.MiddleCenter, new Color(0.78f, 1f, 1f));
        CreateImage("Title Underline", parent, new Vector2(0.28f, 0.565f), new Vector2(0.72f, 0.565f), Vector2.zero, new Vector2(0f, 5f), new Color(0.12f, 0.95f, 1f, 0.8f)).raycastTarget = false;
    }

    private void BuildMenuButtons(Transform parent)
    {
        for (int i = 0; i < menuTexts.Length; i++)
        {
            float top = 0.49f - i * 0.09f;
            float bottom = top - 0.068f;
            Button button = CreateButton("Menu Button " + i, parent, new Vector2(0.35f, bottom), new Vector2(0.65f, top), Vector2.zero, Vector2.zero, menuTexts[i], 32, new Color(0.06f, 0.16f, 0.19f, 0.95f));
            int capturedIndex = i;
            button.onClick.AddListener(() => ActivateMenu(capturedIndex));
            AddPointerEnter(button.gameObject, () => SelectMenu(capturedIndex));

            menuButtons[i] = button;
            menuLabels[i] = button.GetComponentInChildren<Text>();
            menuBackgrounds[i] = button.GetComponent<Image>();
        }
    }

    private void AddPointerEnter(GameObject target, Action callback)
    {
        EventTrigger trigger = target.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener(_ => callback());
        trigger.triggers.Add(entry);
    }

    private void MoveSelection(int direction)
    {
        int next = selectedIndex;
        for (int i = 0; i < menuTexts.Length; i++)
        {
            next = (next + direction + menuTexts.Length) % menuTexts.Length;
            if (menuButtons[next] != null && menuButtons[next].interactable)
            {
                SelectMenu(next);
                return;
            }
        }
    }

    private void SelectMenu(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, menuTexts.Length - 1);
        for (int i = 0; i < menuTexts.Length; i++)
        {
            bool selected = i == selectedIndex;
            menuLabels[i].text = (selected ? "▶ " : "   ") + menuTexts[i];
            menuLabels[i].color = i == MenuRpg ? new Color(0.48f, 0.56f, 0.58f) : Color.white;
            menuLabels[i].fontSize = selected ? 36 : 32;
            menuBackgrounds[i].color = selected
                ? new Color(0.12f, 0.45f, 0.5f, 0.98f)
                : new Color(0.06f, 0.16f, 0.19f, 0.95f);
        }
    }

    private void ConfirmSelection()
    {
        ActivateMenu(selectedIndex);
    }

    private void ActivateMenu(int index)
    {
        SelectMenu(index);
        switch (index)
        {
            case MenuBattle:
                SceneManager.LoadScene("BattleScene");
                break;
            case MenuDeckBuild:
                SceneManager.LoadScene("DeckBuildScene");
                break;
            case MenuRpg:
                SetMessage("RPGモードはまだ未実装です");
                break;
        }
    }

    private void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
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
        label.alignment = alignment;
        label.color = color;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 12;
        label.resizeTextMaxSize = fontSize;
        return label;
    }

    private Button CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string labelText, int fontSize, Color color)
    {
        Image image = CreateImage(name, parent, anchorMin, anchorMax, offsetMin, offsetMax, color);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.16f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.18f, 0.18f, 0.2f, 0.55f);
        button.colors = colors;

        CreateText(name + " Text", image.transform, Vector2.zero, Vector2.one, new Vector2(18f, 0f), new Vector2(-18f, -2f), labelText, fontSize, TextAnchor.MiddleLeft, Color.white);
        return button;
    }
}
