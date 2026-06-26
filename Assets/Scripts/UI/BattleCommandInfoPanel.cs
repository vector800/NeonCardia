using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class BattleCommandInfoPanel : MonoBehaviour
{
    private Image background;
    private Image frameImage;
    private Image dividerImage;
    private Outline outline;
    private TMP_Text titleText;
    private TMP_Text descriptionText;
    private RectTransform statsRoot;
    private TMP_Text powerValueText;
    private TMP_Text attributeValueText;
    private TMP_Text targetValueText;
    private TMP_Text delayValueText;

    public void BuildRuntimeUi()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = gameObject.AddComponent<RectTransform>();
        }

        background = GetComponent<Image>();
        if (background == null)
        {
            background = gameObject.AddComponent<Image>();
        }

        background.color = new Color(0.034f, 0.014f, 0.066f, 0.94f);
        background.raycastTarget = false;
        background.type = Image.Type.Sliced;

        outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }

        outline.effectColor = new Color(0.62f, 0.20f, 1f, 0.92f);
        outline.effectDistance = new Vector2(3f, -3f);

        frameImage = CreateImage("InfoPanelOuterFrameSprite", transform, Vector2.zero, Vector2.one, Color.white);
        frameImage.type = Image.Type.Sliced;
        frameImage.transform.SetAsLastSibling();
        CreateImage("InfoPanelInnerLine", transform, new Vector2(0.030f, 0.080f), new Vector2(0.970f, 0.920f), new Color(0.56f, 0.19f, 1f, 0.22f));
        CreateImage("InfoPanelTopRail", transform, new Vector2(0.055f, 0.815f), new Vector2(0.945f, 0.830f), new Color(0.76f, 0.33f, 1f, 0.72f));
        CreateImage("InfoPanelBottomRail", transform, new Vector2(0.055f, 0.405f), new Vector2(0.945f, 0.420f), new Color(0.55f, 0.18f, 1f, 0.48f));
        dividerImage = CreateImage("InfoPanelDividerSprite", transform, new Vector2(0.045f, 0.060f), new Vector2(0.955f, 0.390f), new Color(1f, 1f, 1f, 0.62f));

        titleText = CreateText("InfoPanelTitle", transform, new Vector2(0.075f, 0.670f), new Vector2(0.925f, 0.890f), string.Empty, 32f, TextAlignmentOptions.MidlineLeft, Color.white);
        titleText.enableAutoSizing = true;
        titleText.fontSizeMin = 19f;
        titleText.fontSizeMax = 32f;

        descriptionText = CreateText("InfoPanelDescription", transform, new Vector2(0.075f, 0.500f), new Vector2(0.925f, 0.645f), string.Empty, 18f, TextAlignmentOptions.MidlineLeft, new Color(0.90f, 0.86f, 1f, 1f));
        descriptionText.textWrappingMode = TextWrappingModes.NoWrap;
        descriptionText.overflowMode = TextOverflowModes.Ellipsis;

        statsRoot = CreateRect("InfoPanelStats", transform, new Vector2(0.065f, 0.105f), new Vector2(0.935f, 0.365f));
        CreateImage("StatsDividerVertical", statsRoot, new Vector2(0.50f, 0.06f), new Vector2(0.503f, 0.94f), new Color(0.67f, 0.25f, 1f, 0.62f));
        CreateImage("StatsDividerLeft", statsRoot, new Vector2(0.00f, 0.50f), new Vector2(0.49f, 0.515f), new Color(0.67f, 0.25f, 1f, 0.40f));
        CreateImage("StatsDividerRight", statsRoot, new Vector2(0.515f, 0.50f), new Vector2(1.00f, 0.515f), new Color(0.67f, 0.25f, 1f, 0.40f));

        CreateText("PowerLabel", statsRoot, new Vector2(0.04f, 0.52f), new Vector2(0.28f, 0.96f), "Power", 16f, TextAlignmentOptions.MidlineLeft, new Color(0.78f, 0.43f, 1f, 1f));
        powerValueText = CreateText("PowerValue", statsRoot, new Vector2(0.31f, 0.52f), new Vector2(0.46f, 0.96f), string.Empty, 20f, TextAlignmentOptions.MidlineRight, Color.white);
        CreateText("AttributeLabel", statsRoot, new Vector2(0.04f, 0.04f), new Vector2(0.30f, 0.48f), "Attribute", 16f, TextAlignmentOptions.MidlineLeft, new Color(0.78f, 0.43f, 1f, 1f));
        attributeValueText = CreateText("AttributeValue", statsRoot, new Vector2(0.31f, 0.04f), new Vector2(0.46f, 0.48f), string.Empty, 18f, TextAlignmentOptions.MidlineRight, Color.white);
        CreateText("TargetLabel", statsRoot, new Vector2(0.55f, 0.52f), new Vector2(0.74f, 0.96f), "Target", 16f, TextAlignmentOptions.MidlineLeft, new Color(0.78f, 0.43f, 1f, 1f));
        targetValueText = CreateText("TargetValue", statsRoot, new Vector2(0.77f, 0.52f), new Vector2(0.96f, 0.96f), string.Empty, 18f, TextAlignmentOptions.MidlineRight, Color.white);
        CreateText("DelayLabel", statsRoot, new Vector2(0.55f, 0.04f), new Vector2(0.74f, 0.48f), "Delay", 16f, TextAlignmentOptions.MidlineLeft, new Color(0.78f, 0.43f, 1f, 1f));
        delayValueText = CreateText("DelayValue", statsRoot, new Vector2(0.77f, 0.04f), new Vector2(0.96f, 0.48f), string.Empty, 20f, TextAlignmentOptions.MidlineRight, Color.white);

        frameImage.transform.SetAsLastSibling();
    }

    public void SetSprites(BattleCommandSpriteSet sprites)
    {
        if (sprites == null)
        {
            return;
        }

        if (sprites.InfoPanelInnerBackground != null)
        {
            background.sprite = sprites.InfoPanelInnerBackground;
            background.color = Color.white;
            ApplySpriteMode(background);
        }

        if (frameImage != null)
        {
            frameImage.sprite = sprites.InfoPanelOuterFrame;
            frameImage.color = sprites.InfoPanelOuterFrame != null ? Color.white : Color.clear;
            ApplySpriteMode(frameImage);
            frameImage.gameObject.SetActive(sprites.InfoPanelOuterFrame != null);
            frameImage.transform.SetAsLastSibling();
        }

        if (dividerImage != null)
        {
            dividerImage.sprite = sprites.InfoPanelDividers;
            dividerImage.color = sprites.InfoPanelDividers != null ? new Color(1f, 1f, 1f, 0.78f) : Color.clear;
            ApplySpriteMode(dividerImage);
            dividerImage.gameObject.SetActive(sprites.InfoPanelDividers != null);
            dividerImage.transform.SetAsFirstSibling();
            if (background != null)
            {
                background.transform.SetAsFirstSibling();
            }
        }
    }

    public void Bind(BattleCommandDisplayData data)
    {
        if (data == null)
        {
            titleText.text = string.Empty;
            descriptionText.text = string.Empty;
            statsRoot.gameObject.SetActive(false);
            return;
        }

        titleText.text = data.Title ?? string.Empty;
        descriptionText.text = data.Description ?? string.Empty;

        bool showStats = data.HasStats;
        statsRoot.gameObject.SetActive(showStats);
        if (!showStats)
        {
            return;
        }

        powerValueText.text = string.IsNullOrEmpty(data.PowerText) ? "--" : data.PowerText.Replace("POW ", string.Empty);
        attributeValueText.text = string.IsNullOrEmpty(data.AttributeText) ? "--" : data.AttributeText;
        targetValueText.text = string.IsNullOrEmpty(data.TargetText) ? "--" : data.TargetText;
        delayValueText.text = string.IsNullOrEmpty(data.DelayText) ? "--" : data.DelayText.Replace("D", string.Empty);
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
        Image image = rect.gameObject.AddComponent<Image>();
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
        image.preserveAspect = false;
    }

    private static TMP_Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string value, float fontSize, TextAlignmentOptions alignment, Color color)
    {
        RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
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
