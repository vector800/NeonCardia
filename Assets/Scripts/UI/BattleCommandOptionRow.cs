using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class BattleCommandOptionRow : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private static readonly Color NormalFallbackColor = new Color32(35, 72, 90, 255);
    private static readonly Color SelectedFallbackColor = new Color32(95, 199, 232, 255);
    private static readonly Color NormalTextColor = new Color32(242, 210, 139, 255);
    private static readonly Color SelectedTextColor = new Color32(28, 23, 16, 255);
    private static readonly Color DisabledTextColor = new Color32(112, 100, 78, 255);

    private Image background;
    private Image shadowImage;
    private Image buttonBackgroundImage;
    private Image designImage;
    private Image iconImage;
    private Image selectedGlow;
    private Image topRail;
    private Image bottomRail;
    private Image attributeIconImage;
    private Outline outline;
    private TMP_Text titleText;
    private TMP_Text powerText;
    private TMP_Text attributeText;
    private BattleCommandDisplayData data;
    private int index;
    private bool selected;

    public event Action<int> PointerEntered;
    public event Action<int> Clicked;

    public int Index { get { return index; } }

    public void BuildRuntimeUi(int rowIndex)
    {
        index = rowIndex;
        background = GetComponent<Image>();
        if (background == null)
        {
            background = gameObject.AddComponent<Image>();
        }

        background.raycastTarget = true;
        background.sprite = null;
        background.color = Color.clear;
        background.type = Image.Type.Simple;
        background.preserveAspect = false;

        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        outline.effectColor = Color.clear;
        outline.effectDistance = Vector2.zero;

        shadowImage = CreateImage("Shadow", transform, Vector2.zero, Vector2.one, Color.white);
        RectTransform shadowRect = shadowImage.rectTransform;
        shadowRect.offsetMin = new Vector2(0f, -5f);
        shadowRect.offsetMax = new Vector2(0f, -3f);
        shadowImage.type = Image.Type.Sliced;

        buttonBackgroundImage = CreateImage("Background", transform, Vector2.zero, Vector2.one, Color.white);
        buttonBackgroundImage.type = Image.Type.Sliced;

        selectedGlow = CreateImage("SelectedGlow", transform, new Vector2(-0.01f, -0.16f), new Vector2(1.01f, 1.16f), Color.clear);
        selectedGlow.type = Image.Type.Sliced;
        selectedGlow.gameObject.SetActive(false);

        designImage = CreateImage("CardDesign", transform, new Vector2(0.20f, 0.13f), new Vector2(0.93f, 0.87f), Color.clear);
        designImage.gameObject.SetActive(false);

        iconImage = CreateImage("CommandIcon", transform, new Vector2(0.035f, 0.11f), new Vector2(0.100f, 0.89f), Color.clear);
        iconImage.type = Image.Type.Sliced;
        iconImage.gameObject.SetActive(false);

        topRail = CreateImage("TopRail", transform, new Vector2(0.04f, 0.88f), new Vector2(0.96f, 0.94f), Color.clear);
        topRail.gameObject.SetActive(false);

        bottomRail = CreateImage("BottomRail", transform, new Vector2(0.04f, 0.06f), new Vector2(0.96f, 0.12f), Color.clear);
        bottomRail.gameObject.SetActive(false);

        titleText = CreateText("Title", transform, new Vector2(0.050f, 0.14f), new Vector2(0.640f, 0.86f), string.Empty, 22f, TextAlignmentOptions.MidlineLeft, NormalTextColor);
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 14f;
        titleText.fontSizeMax = 22f;

        powerText = CreateText("Power", transform, new Vector2(0.680f, 0.18f), new Vector2(0.820f, 0.84f), string.Empty, 18f, TextAlignmentOptions.MidlineRight, NormalTextColor);
        powerText.enableAutoSizing = true;
        powerText.fontSizeMin = 11f;
        powerText.fontSizeMax = 18f;

        attributeIconImage = CreateImage("AttributeIcon", transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Color.white);
        RectTransform attributeIconRect = attributeIconImage.rectTransform;
        attributeIconRect.pivot = new Vector2(1f, 0.5f);
        attributeIconRect.anchoredPosition = new Vector2(-16f, 0f);
        attributeIconRect.sizeDelta = new Vector2(24f, 24f);
        attributeIconImage.preserveAspect = true;
        attributeIconImage.gameObject.SetActive(false);

        attributeText = CreateText("Attribute", transform, new Vector2(0.840f, 0.18f), new Vector2(0.940f, 0.84f), string.Empty, 18f, TextAlignmentOptions.MidlineRight, NormalTextColor);
        attributeText.enableAutoSizing = true;
        attributeText.fontSizeMin = 10f;
        attributeText.fontSizeMax = 18f;
        attributeText.gameObject.SetActive(false);

        SetSelected(false);
    }

    public void Bind(BattleCommandDisplayData displayData, int rowIndex)
    {
        index = rowIndex;
        data = displayData;
        gameObject.SetActive(data != null);
        if (data == null)
        {
            return;
        }

        HideLegacyDecoration();

        shadowImage.sprite = data.BackgroundDesignSprite;
        ApplySpriteMode(shadowImage);
        shadowImage.gameObject.SetActive(data.BackgroundDesignSprite != null);
        shadowImage.color = data.Interactable ? Color.white : new Color(1f, 1f, 1f, 0.55f);

        titleText.text = data.Title ?? string.Empty;

        bool showCardFields = data.OptionType == BattleCommandOptionType.Card;
        powerText.gameObject.SetActive(showCardFields);
        attributeText.gameObject.SetActive(false);
        powerText.text = data.PowerText ?? string.Empty;
        attributeText.text = data.AttributeText ?? string.Empty;

        attributeIconImage.sprite = data.AttributeIcon;
        ApplySpriteMode(attributeIconImage);
        attributeIconImage.gameObject.SetActive(showCardFields && data.AttributeIcon != null);

        RectTransform titleRect = titleText.rectTransform;
        if (showCardFields)
        {
            titleRect.anchorMin = new Vector2(0.050f, 0.14f);
            titleRect.anchorMax = new Vector2(0.640f, 0.86f);
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
        }
        else
        {
            titleRect.anchorMin = new Vector2(0.050f, 0.10f);
            titleRect.anchorMax = new Vector2(0.950f, 0.90f);
            titleText.alignment = TextAlignmentOptions.Midline;
        }

        RectTransform powerRect = powerText.rectTransform;
        powerRect.anchorMin = new Vector2(0.680f, 0.18f);
        powerRect.anchorMax = new Vector2(0.820f, 0.84f);

        RectTransform attributeRect = attributeText.rectTransform;
        attributeRect.anchorMin = new Vector2(0.840f, 0.18f);
        attributeRect.anchorMax = new Vector2(0.940f, 0.84f);

        SetSelected(selected);
    }

    public void SetSelected(bool isSelected)
    {
        selected = isSelected;
        if (data == null)
        {
            return;
        }

        HideLegacyDecoration();
        outline.effectColor = Color.clear;
        outline.effectDistance = Vector2.zero;

        Sprite buttonSprite = isSelected && data.SelectedBackgroundSprite != null
            ? data.SelectedBackgroundSprite
            : data.NormalBackgroundSprite;
        buttonBackgroundImage.sprite = buttonSprite;
        ApplySpriteMode(buttonBackgroundImage);
        buttonBackgroundImage.color = GetBackgroundColor(isSelected);

        Color textColor = GetTextColor(isSelected);
        titleText.color = textColor;
        powerText.color = textColor;
        attributeText.color = textColor;

        if (attributeIconImage != null)
        {
            Color iconColor = Color.white;
            iconColor.a = data.Interactable ? 1f : 0.45f;
            attributeIconImage.color = iconColor;
        }
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

    private Color GetTextColor(bool isSelected)
    {
        if (data == null || !data.Interactable)
        {
            return DisabledTextColor;
        }

        return isSelected ? SelectedTextColor : NormalTextColor;
    }

    private Color GetBackgroundColor(bool isSelected)
    {
        if (data == null)
        {
            return isSelected ? SelectedFallbackColor : NormalFallbackColor;
        }

        Color color = isSelected ? data.SelectedBackgroundColor : data.NormalBackgroundColor;
        if (color.a <= 0.001f)
        {
            color = isSelected ? SelectedFallbackColor : NormalFallbackColor;
        }

        if (!data.Interactable)
        {
            color.a *= 0.55f;
        }

        return color;
    }

    private void HideLegacyDecoration()
    {
        if (selectedGlow != null)
        {
            selectedGlow.sprite = null;
            selectedGlow.color = Color.clear;
            selectedGlow.gameObject.SetActive(false);
        }

        if (designImage != null)
        {
            designImage.sprite = null;
            designImage.color = Color.clear;
            designImage.gameObject.SetActive(false);
        }

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = Color.clear;
            iconImage.gameObject.SetActive(false);
        }

        if (topRail != null)
        {
            topRail.sprite = null;
            topRail.color = Color.clear;
            topRail.gameObject.SetActive(false);
        }

        if (bottomRail != null)
        {
            bottomRail.sprite = null;
            bottomRail.color = Color.clear;
            bottomRail.gameObject.SetActive(false);
        }
    }

    private static Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = child.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        return image;
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
        image.preserveAspect = image.name == "AttributeIcon";
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
}
