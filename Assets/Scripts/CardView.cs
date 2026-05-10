using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    private Image background;
    private Image deckTypeStrip;
    private Image clearBand;
    private Image attributeIcon;
    private Text nameLabel;
    private Text valueLabel;
    private int handIndex;
    private Action<int> onClicked;
    private Action<int> onHovered;
    private Action onHoverExited;

    public void Initialize(Button targetButton, int index, Action<int> clicked, Action<int> hovered, Action hoverExited)
    {
        button = targetButton;
        background = button.GetComponent<Image>();
        nameLabel = button.GetComponentInChildren<Text>();
        handIndex = index;
        onClicked = clicked;
        onHovered = hovered;
        onHoverExited = hoverExited;

        Font font = nameLabel.font;
        ConfigureText(nameLabel, 28, TextAnchor.MiddleCenter, CardVisualStyleResolver.TextColor, 22);
        RectTransform nameRect = nameLabel.rectTransform;
        nameRect.anchorMin = new Vector2(0.08f, 0.58f);
        nameRect.anchorMax = new Vector2(0.94f, 0.93f);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;

        deckTypeStrip = CreateImage("Deck Type Strip", transform, new Vector2(0f, 0f), new Vector2(0.045f, 1f), Vector2.zero, Vector2.zero, Color.white);
        clearBand = CreateImage("Clear Card Band", transform, new Vector2(0.045f, 0.9f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, CardVisualStyleResolver.ClearAccentColor);
        valueLabel = CreateText("Card Value", transform, new Vector2(0.08f, 0.12f), new Vector2(0.64f, 0.55f), Vector2.zero, Vector2.zero, string.Empty, 48, TextAnchor.MiddleCenter, CardVisualStyleResolver.TextColor, font, 32);
        attributeIcon = CreateImage("Attribute Icon", transform, new Vector2(0.68f, 0.16f), new Vector2(0.92f, 0.46f), Vector2.zero, Vector2.zero, Color.white);
        attributeIcon.preserveAspect = true;
        attributeIcon.raycastTarget = false;

        deckTypeStrip.transform.SetAsFirstSibling();
        clearBand.transform.SetSiblingIndex(1);
        nameLabel.transform.SetAsLastSibling();
        valueLabel.transform.SetAsLastSibling();
        attributeIcon.transform.SetAsLastSibling();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClicked);
    }

    public void Refresh(CardData card, bool battleEnded)
    {
        bool hasCard = card != null;
        button.interactable = hasCard && !battleEnded;

        if (!hasCard)
        {
            nameLabel.text = string.Empty;
            valueLabel.text = string.Empty;
            attributeIcon.enabled = false;
            deckTypeStrip.enabled = false;
            clearBand.enabled = false;
            background.color = new Color(0.08f, 0.09f, 0.11f, 0.96f);
            return;
        }

        nameLabel.text = card.Name;
        valueLabel.text = CardVisualStyleResolver.GetValueText(card);
        attributeIcon.sprite = CardAttributeIconResolver.GetIcon(card.Attribute);
        attributeIcon.enabled = attributeIcon.sprite != null;
        deckTypeStrip.enabled = true;
        deckTypeStrip.color = CardVisualStyleResolver.GetDeckTypeAccentColor(card.DeckType);
        clearBand.enabled = card.IsClearCard;
        background.color = CardVisualStyleResolver.GetCardBackgroundColor(card);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onHovered != null)
        {
            onHovered(handIndex);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onHoverExited != null)
        {
            onHoverExited();
        }
    }

    private void HandleClicked()
    {
        if (onClicked != null)
        {
            onClicked(handIndex);
        }
    }

    private static Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string text, int fontSize, TextAnchor alignment, Color color, Font font, int minFontSize)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Text label = go.GetComponent<Text>();
        label.text = text;
        label.font = font;
        ConfigureText(label, fontSize, alignment, color, minFontSize);
        return label;
    }

    private static void ConfigureText(Text label, int fontSize, TextAnchor alignment, Color color, int minFontSize)
    {
        label.fontSize = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.alignment = alignment;
        label.color = color;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.resizeTextForBestFit = false;
        label.resizeTextMinSize = minFontSize;
        label.resizeTextMaxSize = fontSize;
        label.raycastTarget = false;

        Shadow shadow = label.GetComponent<Shadow>();
        if (shadow == null)
        {
            shadow = label.gameObject.AddComponent<Shadow>();
        }

        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(1f, -1f);

        Outline outline = label.GetComponent<Outline>();
        if (outline == null)
        {
            outline = label.gameObject.AddComponent<Outline>();
        }

        outline.enabled = false;
    }
}

public sealed class CardHoverDetailView
{
    private Image root;
    private Image deckTypeStrip;
    private Image clearBand;
    private Image artworkImage;
    private Image attributeIcon;
    private Text nameText;
    private Text valueText;
    private Text rulesText;

    public void Build(Transform parent, Font font)
    {
        root = CreateImage("Card Hover Detail", parent, new Vector2(0.035f, 0.42f), new Vector2(0.18f, 0.72f), Vector2.zero, Vector2.zero, new Color(0.82f, 0.84f, 0.78f, 0.97f));
        root.raycastTarget = false;

        deckTypeStrip = CreateImage("Detail Deck Type Strip", root.transform, new Vector2(0f, 0f), new Vector2(0.055f, 1f), Vector2.zero, Vector2.zero, Color.white);
        clearBand = CreateImage("Detail Clear Band", root.transform, new Vector2(0.055f, 0.925f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, CardVisualStyleResolver.ClearAccentColor);
        nameText = CreateText("Detail Name", root.transform, new Vector2(0.09f, 0.83f), new Vector2(0.95f, 0.98f), Vector2.zero, Vector2.zero, string.Empty, 22, TextAnchor.MiddleCenter, CardVisualStyleResolver.TextColor, font);
        artworkImage = CreateImage("Detail Artwork", root.transform, new Vector2(0.1f, 0.45f), new Vector2(0.9f, 0.82f), Vector2.zero, Vector2.zero, Color.white);
        artworkImage.preserveAspect = true;
        valueText = CreateText("Detail Value", root.transform, new Vector2(0.1f, 0.31f), new Vector2(0.62f, 0.43f), Vector2.zero, Vector2.zero, string.Empty, 27, TextAnchor.MiddleCenter, CardVisualStyleResolver.TextColor, font);
        attributeIcon = CreateImage("Detail Attribute Icon", root.transform, new Vector2(0.67f, 0.31f), new Vector2(0.9f, 0.43f), Vector2.zero, Vector2.zero, Color.white);
        attributeIcon.preserveAspect = true;
        rulesText = CreateText("Detail Rules", root.transform, new Vector2(0.09f, 0.055f), new Vector2(0.94f, 0.3f), Vector2.zero, Vector2.zero, string.Empty, 15, TextAnchor.UpperLeft, CardVisualStyleResolver.TextColor, font);

        Hide();
    }

    public void Show(CardData card, string rangeDescription, string reason)
    {
        if (root == null || card == null)
        {
            Hide();
            return;
        }

        root.gameObject.SetActive(true);
        root.color = CardVisualStyleResolver.GetCardBackgroundColor(card);
        deckTypeStrip.color = CardVisualStyleResolver.GetDeckTypeAccentColor(card.DeckType);
        clearBand.enabled = card.IsClearCard;
        nameText.text = card.Name;
        valueText.text = CardVisualStyleResolver.GetValueText(card);
        attributeIcon.sprite = CardAttributeIconResolver.GetIcon(card.Attribute);
        attributeIcon.enabled = attributeIcon.sprite != null;
        artworkImage.sprite = CardArtworkResolver.GetArtwork(card);
        artworkImage.enabled = artworkImage.sprite != null;

        string text = card.RulesText;
        if (!string.IsNullOrEmpty(rangeDescription))
        {
            text += "\n範囲：" + rangeDescription;
        }

        if (!string.IsNullOrEmpty(reason))
        {
            text += "\n" + reason;
        }

        rulesText.text = text;
    }

    public void Hide()
    {
        if (root != null)
        {
            root.gameObject.SetActive(false);
        }
    }

    private static Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Image image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string text, int fontSize, TextAnchor alignment, Color color, Font font)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;

        Text label = go.GetComponent<Text>();
        label.text = text;
        label.font = font;
        label.fontSize = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.alignment = alignment;
        label.color = color;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.resizeTextForBestFit = false;
        label.resizeTextMinSize = 12;
        label.resizeTextMaxSize = fontSize;
        label.raycastTarget = false;

        Shadow shadow = label.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.55f);
        shadow.effectDistance = new Vector2(1f, -1f);

        Outline outline = label.gameObject.AddComponent<Outline>();
        outline.enabled = false;

        return label;
    }
}

public static class CardVisualStyleResolver
{
    public static readonly Color TextColor = new Color(0.93f, 1f, 1f, 1f);
    public static readonly Color ClearAccentColor = new Color(0.42f, 1f, 0.58f, 1f);

    public static Color GetCardBackgroundColor(CardData card)
    {
        if (card != null && card.IsClearCard)
        {
            return new Color(0.06f, 0.2f, 0.13f, 0.98f);
        }

        return card == null ? new Color(0.1f, 0.1f, 0.12f, 0.96f) : GetDeckTypeBackgroundColor(card.DeckType);
    }

    public static Color GetDeckTypeBackgroundColor(CardDeckType deckType)
    {
        switch (deckType)
        {
            case CardDeckType.HC:
                return new Color(0.05f, 0.16f, 0.24f, 0.98f);
            case CardDeckType.G:
                return new Color(0.24f, 0.06f, 0.09f, 0.98f);
            case CardDeckType.N:
            default:
                return new Color(0.1f, 0.12f, 0.14f, 0.98f);
        }
    }

    public static Color GetDeckTypeAccentColor(CardDeckType deckType)
    {
        switch (deckType)
        {
            case CardDeckType.HC:
                return new Color(0.42f, 0.86f, 1f, 1f);
            case CardDeckType.G:
                return new Color(1f, 0.42f, 0.46f, 1f);
            case CardDeckType.N:
            default:
                return new Color(0.78f, 0.82f, 0.86f, 1f);
        }
    }

    public static string GetValueText(CardData card)
    {
        if (card == null || card.Power <= 0)
        {
            return string.Empty;
        }

        return card.Effect == CardEffectType.Charge ? "+" + card.Power : card.Power.ToString();
    }
}

public static class CardAttributeIconResolver
{
    private static readonly Dictionary<CardAttribute, Sprite> Icons = new Dictionary<CardAttribute, Sprite>();

    public static Sprite GetIcon(CardAttribute attribute)
    {
        Sprite icon;
        if (Icons.TryGetValue(attribute, out icon))
        {
            return icon;
        }

        icon = LoadSprite("UI/AttributeIcons/" + attribute);
        Icons[attribute] = icon;
        return icon;
    }

    private static Sprite LoadSprite(string resourcePath)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            return null;
        }

        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }
}

public static class CardArtworkResolver
{
    private static readonly Dictionary<string, Sprite> Artwork = new Dictionary<string, Sprite>();

    public static Sprite GetArtwork(CardData card)
    {
        if (card == null)
        {
            return null;
        }

        Sprite sprite = LoadCached("Cards/Placeholders/" + card.CardId);
        if (sprite != null)
        {
            return sprite;
        }

        sprite = LoadCached("Cards/Placeholders/" + card.Effect);
        if (sprite != null)
        {
            return sprite;
        }

        sprite = LoadCached("Cards/Placeholders/" + card.Attribute);
        return sprite;
    }

    private static Sprite LoadCached(string resourcePath)
    {
        Sprite sprite;
        if (Artwork.TryGetValue(resourcePath, out sprite))
        {
            return sprite;
        }

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            Artwork[resourcePath] = null;
            return null;
        }

        sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        Artwork[resourcePath] = sprite;
        return sprite;
    }
}
