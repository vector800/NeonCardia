using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class BattleReserveMemberCardView : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private static readonly Color NormalCardColor = new Color32(42, 54, 75, 235);
    private static readonly Color SelectedCardColor = new Color32(171, 137, 61, 245);
    private static readonly Color NormalTextColor = new Color32(232, 246, 255, 255);
    private static readonly Color SelectedTextColor = new Color32(30, 24, 14, 255);
    private static readonly Color HpTextColor = new Color32(255, 231, 151, 255);
    private static readonly Color DisabledOverlayColor = new Color(0f, 0f, 0f, 0.32f);
    private const float FaceGhostNormalAlpha = 0.18f;
    private const float FaceGhostSelectedAlpha = 0.25f;
    private const float FaceGhostDisabledAlpha = 0.11f;
    private const float ReadabilityNormalAlpha = 0.30f;
    private const float ReadabilitySelectedAlpha = 0.20f;
    private const float ReadabilityDisabledAlpha = 0.34f;
    private const float FaceGhostWidthScale = 1.08f;
    private const float FaceGhostHeightScale = 3.05f;
    private const float FaceGhostOffsetX = 36f;

    private Image raycastImage;
    private Image shadowImage;
    private Image backgroundImage;
    private RectTransform faceClipRoot;
    private Image faceGhostImage;
    private Image readabilityOverlayImage;
    private Image attributeIconImage;
    private Image disabledOverlayImage;
    private Outline outline;
    private TMP_Text nameText;
    private TMP_Text hpText;
    private TMP_Text attributeText;
    private BattleCommandDisplayData data;
    private int index;
    private bool selected;

    public event Action<int> PointerEntered;
    public event Action<int> Clicked;

    public int Index { get { return index; } }

    public void BuildRuntimeUi(int cardIndex)
    {
        index = cardIndex;
        raycastImage = GetComponent<Image>();
        if (raycastImage == null)
        {
            raycastImage = gameObject.AddComponent<Image>();
        }

        raycastImage.sprite = null;
        raycastImage.color = Color.clear;
        raycastImage.raycastTarget = true;

        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        outline.effectColor = Color.clear;
        outline.effectDistance = Vector2.zero;

        shadowImage = CreateImage("Shadow", transform, Vector2.zero, Vector2.one, Color.white);
        shadowImage.rectTransform.offsetMin = new Vector2(0f, -6f);
        shadowImage.rectTransform.offsetMax = new Vector2(0f, -4f);
        shadowImage.type = Image.Type.Sliced;

        backgroundImage = CreateImage("Background", transform, Vector2.zero, Vector2.one, NormalCardColor);
        backgroundImage.type = Image.Type.Sliced;

        faceClipRoot = CreateRect("FaceClipRoot", transform, Vector2.zero, Vector2.one);
        faceClipRoot.gameObject.AddComponent<RectMask2D>();

        faceGhostImage = CreateImage("FaceGhostImage", faceClipRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Color.clear);
        faceGhostImage.preserveAspect = true;
        ConfigureFaceGhostRect();

        readabilityOverlayImage = CreateImage("ReadabilityOverlay", transform, Vector2.zero, Vector2.one, new Color(0.02f, 0.05f, 0.08f, ReadabilityNormalAlpha));

        nameText = CreateText("NameText", transform, new Vector2(0.055f, 0.54f), new Vector2(0.660f, 0.92f), string.Empty, 20f, TextAlignmentOptions.MidlineLeft, NormalTextColor);
        nameText.enableAutoSizing = true;
        nameText.fontSizeMin = 13f;
        nameText.fontSizeMax = 20f;

        hpText = CreateText("HpText", transform, new Vector2(0.055f, 0.10f), new Vector2(0.380f, 0.45f), string.Empty, 17f, TextAlignmentOptions.MidlineLeft, HpTextColor);
        hpText.enableAutoSizing = true;
        hpText.fontSizeMin = 12f;
        hpText.fontSizeMax = 17f;

        attributeIconImage = CreateImage("AttributeIcon", transform, new Vector2(0.430f, 0.14f), new Vector2(0.510f, 0.43f), Color.white);
        attributeIconImage.preserveAspect = true;

        attributeText = CreateText("AttributeText", transform, new Vector2(0.525f, 0.10f), new Vector2(0.840f, 0.45f), string.Empty, 15f, TextAlignmentOptions.MidlineLeft, NormalTextColor);
        attributeText.enableAutoSizing = true;
        attributeText.fontSizeMin = 11f;
        attributeText.fontSizeMax = 15f;

        disabledOverlayImage = CreateImage("DisabledOverlay", transform, Vector2.zero, Vector2.one, DisabledOverlayColor);
        disabledOverlayImage.gameObject.SetActive(false);

        SetSelected(false);
    }

    public void Bind(BattleCommandDisplayData displayData, int cardIndex)
    {
        index = cardIndex;
        data = displayData;
        gameObject.SetActive(data != null);
        if (data == null)
        {
            return;
        }

        shadowImage.sprite = data.BackgroundDesignSprite;
        ApplySpriteMode(shadowImage);
        shadowImage.gameObject.SetActive(data.BackgroundDesignSprite != null);
        shadowImage.color = data.Interactable ? Color.white : new Color(1f, 1f, 1f, 0.48f);

        backgroundImage.sprite = data.NormalBackgroundSprite;
        ApplySpriteMode(backgroundImage);

        faceGhostImage.sprite = data.FaceIcon;
        faceClipRoot.gameObject.SetActive(data.FaceIcon != null);
        faceGhostImage.gameObject.SetActive(data.FaceIcon != null);
        ConfigureFaceGhostRect();

        nameText.text = data.Title ?? string.Empty;
        hpText.text = string.IsNullOrEmpty(data.HpText) ? "HP --" : data.HpText;

        attributeIconImage.sprite = data.AttributeIcon;
        ApplySpriteMode(attributeIconImage);
        attributeIconImage.gameObject.SetActive(data.AttributeIcon != null);
        attributeText.text = string.IsNullOrEmpty(data.AttributeText) ? "--" : data.AttributeText;

        disabledOverlayImage.gameObject.SetActive(!data.Interactable);
        SetSelected(selected);
    }

    public void SetSelected(bool isSelected)
    {
        selected = isSelected;
        if (data == null)
        {
            return;
        }

        Sprite sprite = selected && data.SelectedBackgroundSprite != null
            ? data.SelectedBackgroundSprite
            : data.NormalBackgroundSprite;
        backgroundImage.sprite = sprite;
        ApplySpriteMode(backgroundImage);

        Color backgroundColor = selected ? data.SelectedBackgroundColor : data.NormalBackgroundColor;
        if (backgroundColor.a <= 0.001f)
        {
            backgroundColor = selected ? SelectedCardColor : NormalCardColor;
        }

        if (!data.Interactable)
        {
            backgroundColor.a *= 0.68f;
        }

        backgroundImage.color = backgroundColor;
        float faceAlpha = FaceGhostNormalAlpha;
        float overlayAlpha = ReadabilityNormalAlpha;
        if (!data.Interactable)
        {
            faceAlpha = FaceGhostDisabledAlpha;
            overlayAlpha = ReadabilityDisabledAlpha;
        }
        else if (selected)
        {
            faceAlpha = FaceGhostSelectedAlpha;
            overlayAlpha = ReadabilitySelectedAlpha;
        }

        faceGhostImage.color = data.FaceIcon != null ? new Color(1f, 1f, 1f, faceAlpha) : Color.clear;
        readabilityOverlayImage.color = new Color(0.02f, 0.05f, 0.08f, overlayAlpha);
        Color textColor = selected && data.Interactable ? SelectedTextColor : NormalTextColor;
        nameText.color = textColor;
        attributeText.color = textColor;
        hpText.color = selected && data.Interactable ? SelectedTextColor : HpTextColor;
        outline.effectColor = selected ? new Color(1f, 0.78f, 0.28f, 0.60f) : Color.clear;
        outline.effectDistance = selected ? new Vector2(2f, -2f) : Vector2.zero;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data != null && data.Interactable && PointerEntered != null)
        {
            PointerEntered(index);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (data != null && data.Interactable && Clicked != null)
        {
            Clicked(index);
        }
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private static Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax);
        rect.gameObject.AddComponent<CanvasRenderer>();
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        return image;
    }

    private static TMP_Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string value, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        GameObject child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = child.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.font = BattleCommandFontProvider.GetFontAsset();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    private void ConfigureFaceGhostRect()
    {
        if (faceGhostImage == null)
        {
            return;
        }

        RectTransform parentRect = transform as RectTransform;
        RectTransform rect = faceGhostImage.rectTransform;
        if (faceClipRoot != null)
        {
            faceClipRoot.anchorMin = Vector2.zero;
            faceClipRoot.anchorMax = Vector2.one;
            faceClipRoot.offsetMin = Vector2.zero;
            faceClipRoot.offsetMax = Vector2.zero;
        }

        float cardWidth = parentRect != null && parentRect.rect.width > 1f ? parentRect.rect.width : 320f;
        float cardHeight = parentRect != null && parentRect.rect.height > 1f ? parentRect.rect.height : 60f;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(FaceGhostOffsetX, 0f);
        rect.sizeDelta = new Vector2(cardWidth * FaceGhostWidthScale, cardHeight * FaceGhostHeightScale);
    }

    private static void ApplySpriteMode(Image image)
    {
        if (image == null)
        {
            return;
        }

        Sprite sprite = image.sprite;
        image.type = sprite != null && sprite.border.sqrMagnitude > 0.001f
            ? Image.Type.Sliced
            : Image.Type.Simple;
        image.preserveAspect = image.name == "AttributeIcon" || image.name == "FaceGhostImage";
    }
}
