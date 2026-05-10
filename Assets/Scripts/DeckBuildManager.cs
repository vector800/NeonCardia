using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class DeckBuildManager : MonoBehaviour
{
    private readonly List<CardData> ownedCards = new List<CardData>();
    private readonly List<CardData> editingDeck = new List<CardData>();
    private readonly List<Button> collectionButtons = new List<Button>();

    private Font uiFont;
    private RectTransform collectionListRoot;
    private RectTransform deckListRoot;
    private Text deckSummaryText;
    private Text validationText;
    private Text messageText;
    private Button saveButton;
    private Button battleButton;

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
        LoadData();
        BuildUi();
        RefreshUi("DeckBuildSceneを開始しました。");
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
        camera.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
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

    private void LoadData()
    {
        ownedCards.Clear();
        ownedCards.AddRange(PlayerCardCollection.LoadOwnedCards());

        List<CardData> savedDeck;
        if (DeckStorage.TryLoadDeck(out savedDeck))
        {
            editingDeck.AddRange(savedDeck);
        }
    }

    private void BuildUi()
    {
        GameObject canvasObject = new GameObject("Deck Build Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image background = CreateImage("Background", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.04f, 0.05f, 0.07f));
        background.raycastTarget = false;

        CreateText("Title", canvasObject.transform, new Vector2(0.03f, 0.92f), new Vector2(0.97f, 0.985f), Vector2.zero, Vector2.zero, "NEON CARDIA - デッキビルドMVP", 32, TextAnchor.MiddleCenter, Color.white);
        CreateText("Collection Title", canvasObject.transform, new Vector2(0.04f, 0.84f), new Vector2(0.45f, 0.9f), Vector2.zero, Vector2.zero, "所持カード一覧", 24, TextAnchor.MiddleLeft, Color.white);
        CreateText("Deck Title", canvasObject.transform, new Vector2(0.52f, 0.84f), new Vector2(0.94f, 0.9f), Vector2.zero, Vector2.zero, "現在のデッキ", 24, TextAnchor.MiddleLeft, Color.white);

        Image collectionPanel = CreateImage("Collection Panel", canvasObject.transform, new Vector2(0.04f, 0.18f), new Vector2(0.48f, 0.84f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.1f, 0.14f, 0.92f));
        Image deckPanel = CreateImage("Deck Panel", canvasObject.transform, new Vector2(0.52f, 0.18f), new Vector2(0.94f, 0.84f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.1f, 0.14f, 0.92f));

        collectionListRoot = CreateRect("Collection List", collectionPanel.transform, Vector2.zero, Vector2.one, new Vector2(14f, 12f), new Vector2(-14f, -12f));
        deckListRoot = CreateRect("Deck List", deckPanel.transform, Vector2.zero, Vector2.one, new Vector2(14f, 12f), new Vector2(-14f, -12f));

        deckSummaryText = CreateText("Deck Summary", canvasObject.transform, new Vector2(0.04f, 0.08f), new Vector2(0.28f, 0.17f), Vector2.zero, Vector2.zero, string.Empty, 20, TextAnchor.UpperLeft, Color.white);
        validationText = CreateText("Validation Text", canvasObject.transform, new Vector2(0.3f, 0.08f), new Vector2(0.56f, 0.17f), Vector2.zero, Vector2.zero, string.Empty, 20, TextAnchor.UpperLeft, new Color(1f, 0.9f, 0.55f));
        messageText = CreateText("Message Text", canvasObject.transform, new Vector2(0.58f, 0.08f), new Vector2(0.94f, 0.17f), Vector2.zero, Vector2.zero, string.Empty, 20, TextAnchor.UpperLeft, new Color(0.9f, 0.96f, 1f));

        Button defaultButton = CreateButton("Default Deck Button", canvasObject.transform, new Vector2(0.04f, 0.015f), new Vector2(0.2f, 0.065f), Vector2.zero, Vector2.zero, "デフォルトデッキ作成", 18, new Color(0.16f, 0.34f, 0.36f));
        defaultButton.onClick.AddListener(SetDefaultDeck);

        Button clearButton = CreateButton("Clear Deck Button", canvasObject.transform, new Vector2(0.22f, 0.015f), new Vector2(0.34f, 0.065f), Vector2.zero, Vector2.zero, "デッキ初期化", 18, new Color(0.34f, 0.22f, 0.24f));
        clearButton.onClick.AddListener(ClearDeck);

        saveButton = CreateButton("Save Deck Button", canvasObject.transform, new Vector2(0.58f, 0.015f), new Vector2(0.7f, 0.065f), Vector2.zero, Vector2.zero, "デッキ保存", 18, new Color(0.14f, 0.42f, 0.27f));
        saveButton.onClick.AddListener(SaveDeck);

        battleButton = CreateButton("Go Battle Button", canvasObject.transform, new Vector2(0.72f, 0.015f), new Vector2(0.94f, 0.065f), Vector2.zero, Vector2.zero, "BattleSceneへ進む", 18, new Color(0.58f, 0.36f, 0.08f));
        battleButton.onClick.AddListener(GoToBattleScene);
    }

    private void RefreshUi(string message)
    {
        DeckValidationResult result = DeckValidator.Validate(editingDeck);
        DeckValidationResult counts = DeckValidator.Count(editingDeck);

        deckSummaryText.text = "デッキ枚数：" + counts.TotalCount + " / " + DeckValidator.RequiredDeckCount
            + "\nN：" + counts.NormalCount
            + "\nHC：" + counts.HighClassCount + " / " + DeckValidator.MaxHighClassCount
            + "\nG：" + counts.GigantCount + " / " + DeckValidator.MaxGigantCount;
        validationText.text = result.Message;
        messageText.text = message;
        saveButton.interactable = true;
        battleButton.interactable = result.IsValid;

        RebuildCollectionList();
        RebuildDeckList();
    }

    private void RebuildCollectionList()
    {
        ClearChildren(collectionListRoot);
        collectionButtons.Clear();

        for (int i = 0; i < ownedCards.Count; i++)
        {
            CardData card = ownedCards[i];
            float maxY = 1f - i * 0.071f;
            float minY = maxY - 0.065f;
            Button button = CreateButton("Collection " + card.CardId, collectionListRoot, new Vector2(0f, minY), new Vector2(1f, maxY), Vector2.zero, Vector2.zero, FormatCollectionCard(card), 16, GetCardColor(card));
            CardData capturedCard = card;
            button.onClick.AddListener(() => AddCard(capturedCard));
            collectionButtons.Add(button);
        }
    }

    private void RebuildDeckList()
    {
        ClearChildren(deckListRoot);
        List<CardData> uniqueCards = GetUniqueDeckCards();

        for (int i = 0; i < uniqueCards.Count; i++)
        {
            CardData card = uniqueCards[i];
            float maxY = 1f - i * 0.071f;
            float minY = maxY - 0.065f;
            Button button = CreateButton("Deck " + card.CardId, deckListRoot, new Vector2(0f, minY), new Vector2(1f, maxY), Vector2.zero, Vector2.zero, FormatDeckCard(card), 16, GetCardColor(card));
            CardData capturedCard = card;
            button.onClick.AddListener(() => RemoveCard(capturedCard));
        }
    }

    private List<CardData> GetUniqueDeckCards()
    {
        List<CardData> uniqueCards = new List<CardData>();
        for (int i = 0; i < editingDeck.Count; i++)
        {
            CardData card = editingDeck[i];
            if (card == null)
            {
                continue;
            }

            bool exists = false;
            for (int j = 0; j < uniqueCards.Count; j++)
            {
                if (uniqueCards[j].CardId == card.CardId)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                uniqueCards.Add(card);
            }
        }

        uniqueCards.Sort((a, b) => a.DeckType == b.DeckType ? string.Compare(a.CardId, b.CardId, System.StringComparison.Ordinal) : a.DeckType.CompareTo(b.DeckType));
        return uniqueCards;
    }

    private string FormatCollectionCard(CardData card)
    {
        return BattleText.FormatCardTags(card) + " " + card.Name
            + "\n" + FormatPower(card)
            + "\nデッキ内：" + CountInDeck(card.CardId) + "枚";
    }

    private string FormatDeckCard(CardData card)
    {
        return BattleText.FormatCardTags(card) + " " + card.Name + " × " + CountInDeck(card.CardId)
            + "\nクリックで1枚削除";
    }

    private string FormatPower(CardData card)
    {
        switch (card.Effect)
        {
            case CardEffectType.Damage:
                return card.Power + "ダメージ / " + BattleText.FormatAttribute(card.Attribute);
            case CardEffectType.Guard:
                return "ガード +" + card.Power;
            case CardEffectType.Repair:
                return "HP +" + card.Power;
            case CardEffectType.Charge:
                return "次の攻撃 +" + card.Power;
            default:
                return card.RulesText;
        }
    }

    private int CountInDeck(string cardId)
    {
        int count = 0;
        for (int i = 0; i < editingDeck.Count; i++)
        {
            if (editingDeck[i] != null && editingDeck[i].CardId == cardId)
            {
                count++;
            }
        }

        return count;
    }

    private void AddCard(CardData card)
    {
        string message;
        if (!DeckValidator.CanAddCard(editingDeck, card, out message))
        {
            RefreshUi(message);
            return;
        }

        editingDeck.Add(card);
        RefreshUi(message);
    }

    private void RemoveCard(CardData card)
    {
        for (int i = 0; i < editingDeck.Count; i++)
        {
            if (editingDeck[i] != null && editingDeck[i].CardId == card.CardId)
            {
                editingDeck.RemoveAt(i);
                RefreshUi(card.Name + "を1枚削除しました");
                return;
            }
        }
    }

    private void SetDefaultDeck()
    {
        editingDeck.Clear();
        editingDeck.AddRange(CardData.CreateStarterDeck());
        RefreshUi("デフォルトデッキを作成しました");
    }

    private void ClearDeck()
    {
        editingDeck.Clear();
        RefreshUi("デッキを初期化しました");
    }

    private void SaveDeck()
    {
        DeckValidationResult result = DeckValidator.Validate(editingDeck);
        if (!result.IsValid)
        {
            RefreshUi("デッキが無効なため保存できません");
            return;
        }

        DeckStorage.SaveDeck(editingDeck);
        RefreshUi("デッキを保存しました");
    }

    private void GoToBattleScene()
    {
        DeckValidationResult result = DeckValidator.Validate(editingDeck);
        if (!result.IsValid)
        {
            RefreshUi("デッキが無効なためBattleSceneへ進めません");
            return;
        }

        DeckStorage.SaveDeck(editingDeck);
        SceneManager.LoadScene("BattleScene");
    }

    private static Color GetCardColor(CardData card)
    {
        switch (card.DeckType)
        {
            case CardDeckType.HC:
                return new Color(0.22f, 0.22f, 0.42f);
            case CardDeckType.G:
                return new Color(0.42f, 0.22f, 0.16f);
            default:
                return new Color(0.16f, 0.24f, 0.3f);
        }
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
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

        Text label = CreateText(name + " Text", image.transform, Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f), labelText, fontSize, TextAnchor.MiddleLeft, Color.white);
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 10;
        label.resizeTextMaxSize = fontSize;

        return button;
    }
}
