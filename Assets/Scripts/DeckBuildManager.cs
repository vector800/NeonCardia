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
    private readonly List<CardData> entryDeck = new List<CardData>();
    private readonly List<Button> collectionButtons = new List<Button>();

    private Font uiFont;
    private RectTransform collectionListRoot;
    private RectTransform deckListRoot;
    private Text deckSummaryText;
    private Text validationText;
    private Text messageText;
    private Button saveButton;
    private Button battleButton;
    private Button restoreButton;

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

        entryDeck.Clear();
        entryDeck.AddRange(editingDeck);
    }

    private void BuildUi()
    {
        GameObject canvasObject = new GameObject("Deck Build Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        Image background = CreateImage("Background", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.04f, 0.05f, 0.07f));
        background.raycastTarget = false;

        CreateText("Collection Title", canvasObject.transform, new Vector2(0.035f, 0.91f), new Vector2(0.48f, 0.97f), Vector2.zero, Vector2.zero, "所持カード一覧", 28, TextAnchor.MiddleLeft, Color.white);
        CreateText("Deck Title", canvasObject.transform, new Vector2(0.515f, 0.91f), new Vector2(0.65f, 0.97f), Vector2.zero, Vector2.zero, "現在のデッキ", 28, TextAnchor.MiddleLeft, Color.white);
        deckSummaryText = CreateText("Deck Summary", canvasObject.transform, new Vector2(0.64f, 0.91f), new Vector2(0.965f, 0.97f), Vector2.zero, Vector2.zero, string.Empty, 24, TextAnchor.MiddleRight, new Color(0.9f, 1f, 0.82f));

        Image collectionPanel = CreateImage("Collection Panel", canvasObject.transform, new Vector2(0.035f, 0.145f), new Vector2(0.485f, 0.905f), Vector2.zero, Vector2.zero, new Color(0.035f, 0.065f, 0.075f, 0.98f));
        Image deckPanel = CreateImage("Deck Panel", canvasObject.transform, new Vector2(0.515f, 0.145f), new Vector2(0.965f, 0.905f), Vector2.zero, Vector2.zero, new Color(0.035f, 0.065f, 0.075f, 0.98f));

        collectionListRoot = CreateRect("Collection List", collectionPanel.transform, Vector2.zero, Vector2.one, new Vector2(10f, 8f), new Vector2(-10f, -8f));
        deckListRoot = CreateRect("Deck List", deckPanel.transform, Vector2.zero, Vector2.one, new Vector2(10f, 8f), new Vector2(-10f, -8f));

        CreateImage("Status Panel", canvasObject.transform, new Vector2(0.035f, 0.098f), new Vector2(0.965f, 0.135f), Vector2.zero, Vector2.zero, new Color(0.015f, 0.025f, 0.032f, 0.9f)).raycastTarget = false;
        validationText = CreateText("Validation Text", canvasObject.transform, new Vector2(0.045f, 0.098f), new Vector2(0.485f, 0.135f), Vector2.zero, Vector2.zero, string.Empty, 20, TextAnchor.MiddleLeft, new Color(1f, 0.9f, 0.55f));
        messageText = CreateText("Message Text", canvasObject.transform, new Vector2(0.515f, 0.098f), new Vector2(0.955f, 0.135f), Vector2.zero, Vector2.zero, string.Empty, 20, TextAnchor.MiddleRight, new Color(0.92f, 1f, 1f));

        saveButton = CreateButton("Save Deck Button", canvasObject.transform, new Vector2(0.28f, 0.025f), new Vector2(0.43f, 0.08f), Vector2.zero, Vector2.zero, "デッキ保存", 22, new Color(0.08f, 0.34f, 0.18f));
        saveButton.onClick.AddListener(SaveDeck);

        battleButton = CreateButton("Go Battle Button", canvasObject.transform, new Vector2(0.45f, 0.025f), new Vector2(0.61f, 0.08f), Vector2.zero, Vector2.zero, "バトルへ進む", 22, new Color(0.5f, 0.27f, 0.05f));
        battleButton.onClick.AddListener(GoToBattleScene);

        restoreButton = CreateButton("Restore Deck Button", canvasObject.transform, new Vector2(0.63f, 0.025f), new Vector2(0.78f, 0.08f), Vector2.zero, Vector2.zero, "元に戻す", 22, new Color(0.22f, 0.24f, 0.32f));
        restoreButton.onClick.AddListener(RestoreEntryDeck);
    }

    private void RefreshUi(string message)
    {
        DeckValidationResult result = DeckValidator.Validate(editingDeck);
        DeckValidationResult counts = DeckValidator.Count(editingDeck);

        deckSummaryText.text = counts.TotalCount + "/" + DeckValidator.RequiredDeckCount
            + "   N:" + counts.NormalCount
            + "   HC:" + counts.HighClassCount
            + "   G:" + counts.GigantCount;
        validationText.text = FormatValidationMessage(result);
        validationText.color = result.IsValid ? new Color(0.65f, 1f, 0.48f) : new Color(1f, 0.84f, 0.38f);
        messageText.text = message;
        saveButton.interactable = true;
        battleButton.interactable = result.IsValid;
        restoreButton.interactable = true;

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
            float maxY = 1f - i * 0.074f;
            float minY = maxY - 0.067f;
            Button button = CreateButton("Collection " + card.CardId, collectionListRoot, new Vector2(0f, minY), new Vector2(1f, maxY), Vector2.zero, Vector2.zero, FormatCollectionCard(card), 18, GetCardColor(card));
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
            float maxY = 1f - i * 0.074f;
            float minY = maxY - 0.067f;
            Button button = CreateButton("Deck " + card.CardId, deckListRoot, new Vector2(0f, minY), new Vector2(1f, maxY), Vector2.zero, Vector2.zero, FormatDeckCard(card), 18, GetCardColor(card));
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
            + "\n" + FormatPower(card) + " / デッキ内:" + CountInDeck(card.CardId);
    }

    private string FormatDeckCard(CardData card)
    {
        return BattleText.FormatCardTags(card) + " " + card.Name + " ×" + CountInDeck(card.CardId)
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

    private void RestoreEntryDeck()
    {
        editingDeck.Clear();
        editingDeck.AddRange(entryDeck);
        RefreshUi("デッキを入場時の状態に戻しました");
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
                return card.IsClearCard ? new Color(0.24f, 0.32f, 0.48f) : new Color(0.12f, 0.16f, 0.42f);
            case CardDeckType.G:
                return card.IsClearCard ? new Color(0.48f, 0.28f, 0.14f) : new Color(0.42f, 0.08f, 0.06f);
            default:
                return card.IsClearCard ? new Color(0.12f, 0.32f, 0.28f) : new Color(0.08f, 0.2f, 0.19f);
        }
    }

    private static string FormatValidationMessage(DeckValidationResult result)
    {
        if (result.IsValid)
        {
            return "有効";
        }

        if (result.Errors.Count == 0)
        {
            return "無効";
        }

        return "無効：" + result.Errors[0];
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
        label.resizeTextForBestFit = false;
        label.raycastTarget = false;

        Outline outline = rectTransform.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.95f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        Shadow shadow = rectTransform.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        shadow.effectDistance = new Vector2(2.5f, -2.5f);
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

        Text label = CreateText(name + " Text", image.transform, Vector2.zero, Vector2.one, new Vector2(10f, 3f), new Vector2(-10f, -3f), labelText, fontSize, TextAnchor.MiddleLeft, Color.white);
        label.resizeTextForBestFit = false;

        return button;
    }
}
