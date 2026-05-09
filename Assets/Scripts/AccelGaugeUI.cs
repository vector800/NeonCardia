using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class AccelGaugeUI : MonoBehaviour
{
    private const int MaxGaugeValue = 100;

    private RectTransform root;
    private RectTransform fillRect;
    private Image fillImage;
    private Image frameImage;
    private Image flashImage;
    private Text percentText;
    private Text gainText;
    private Text maxText;

    private int currentValue;
    private Coroutine popRoutine;
    private Coroutine gainRoutine;
    private Coroutine flashRoutine;
    private Color baseFillColor = new Color(0.12f, 0.78f, 1f, 1f);
    private Color baseFrameColor = new Color(0.85f, 0.95f, 1f, 1f);

    public void Build(Transform parent, Font font)
    {
        root = CreateRect("AccelGaugeRoot", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 78f), new Vector2(24f, -18f));
        gameObject.transform.SetParent(root, false);

        Image panel = CreateImage("GaugePanel", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.04f, 0.07f, 0.1f, 0.94f));
        panel.raycastTarget = false;

        frameImage = CreateImage("GaugeFrame", root, new Vector2(0.03f, 0.18f), new Vector2(0.97f, 0.58f), Vector2.zero, Vector2.zero, baseFrameColor);
        frameImage.raycastTarget = false;

        Image background = CreateImage("GaugeBackground", frameImage.transform, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f), new Color(0.02f, 0.03f, 0.04f, 1f));
        background.raycastTarget = false;

        fillImage = CreateImage("GaugeFill", background.transform, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, Vector2.zero, baseFillColor);
        fillImage.raycastTarget = false;
        fillRect = fillImage.rectTransform;

        flashImage = CreateImage("GaugeFlashImage", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0f));
        flashImage.raycastTarget = false;

        Text label = CreateText("GaugeLabelText", root, new Vector2(0.04f, 0.58f), new Vector2(0.48f, 0.96f), Vector2.zero, Vector2.zero, "アクセル", 22, TextAnchor.MiddleLeft, baseFrameColor, font);
        label.raycastTarget = false;

        percentText = CreateText("GaugePercentText", root, new Vector2(0.42f, 0.58f), new Vector2(0.96f, 0.96f), Vector2.zero, Vector2.zero, "0%", 24, TextAnchor.MiddleRight, Color.white, font);
        percentText.raycastTarget = false;

        gainText = CreateText("GaugeGainText", root, new Vector2(0.52f, 0.03f), new Vector2(0.98f, 0.45f), Vector2.zero, Vector2.zero, string.Empty, 22, TextAnchor.MiddleRight, new Color(1f, 0.94f, 0.28f, 0f), font);
        gainText.raycastTarget = false;

        maxText = CreateText("GaugeMaxText", root, new Vector2(0.04f, 0.02f), new Vector2(0.44f, 0.46f), Vector2.zero, Vector2.zero, "MAX", 20, TextAnchor.MiddleLeft, new Color(1f, 0.86f, 0.2f, 0f), font);
        maxText.raycastTarget = false;

        SetValue(0);
    }

    public void SetValue(int value, int gain = 0, string effectLabel = null)
    {
        currentValue = Mathf.Clamp(value, 0, MaxGaugeValue);
        float fill = currentValue / (float)MaxGaugeValue;
        fillRect.anchorMax = new Vector2(fill, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        percentText.text = currentValue + "%";

        if (gain > 0)
        {
            PlayGainEffect(gain, effectLabel);
        }

        if (currentValue < MaxGaugeValue)
        {
            fillImage.color = baseFillColor;
            frameImage.color = baseFrameColor;
            SetAlpha(maxText, 0f);
        }
    }

    private void Update()
    {
        if (currentValue < MaxGaugeValue)
        {
            return;
        }

        float pulse = 0.55f + Mathf.Sin(Time.unscaledTime * 5f) * 0.25f;
        fillImage.color = Color.Lerp(baseFillColor, Color.white, pulse);
        frameImage.color = Color.Lerp(baseFrameColor, new Color(1f, 0.86f, 0.2f, 1f), pulse);
        SetAlpha(maxText, 0.55f + pulse * 0.45f);
    }

    private void PlayGainEffect(int gain, string effectLabel)
    {
        if (popRoutine != null)
        {
            StopCoroutine(popRoutine);
        }

        if (gainRoutine != null)
        {
            StopCoroutine(gainRoutine);
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        popRoutine = StartCoroutine(PopRoutine());
        gainRoutine = StartCoroutine(GainTextRoutine(gain, effectLabel));
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator PopRoutine()
    {
        float elapsed = 0f;
        while (elapsed < 0.18f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / 0.18f);
            root.localScale = Vector3.one * Mathf.Lerp(1.12f, 1f, t);
            yield return null;
        }

        root.localScale = Vector3.one;
    }

    private IEnumerator GainTextRoutine(int gain, string effectLabel)
    {
        string prefix = string.IsNullOrEmpty(effectLabel) ? string.Empty : effectLabel + " ";
        gainText.text = prefix + "+" + gain + "%";
        RectTransform rect = gainText.rectTransform;
        Vector2 start = Vector2.zero;
        Vector2 end = new Vector2(0f, 18f);

        float elapsed = 0f;
        while (elapsed < 0.85f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / 0.85f);
            rect.anchoredPosition = Vector2.Lerp(start, end, t);
            SetAlpha(gainText, Mathf.Sin(t * Mathf.PI));
            yield return null;
        }

        rect.anchoredPosition = start;
        gainText.text = string.Empty;
        SetAlpha(gainText, 0f);
    }

    private IEnumerator FlashRoutine()
    {
        float elapsed = 0f;
        while (elapsed < 0.35f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / 0.35f);
            SetAlpha(flashImage, Mathf.Lerp(0.38f, 0f, t));
            fillImage.color = Color.Lerp(Color.white, baseFillColor, t);
            yield return null;
        }

        SetAlpha(flashImage, 0f);
    }

    private static void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPosition)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.sizeDelta = sizeDelta;
        rectTransform.anchoredPosition = anchoredPosition;
        return rectTransform;
    }

    private static Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        RectTransform rectTransform = CreateRect(name, parent, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
        Image image = rectTransform.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string text, int fontSize, TextAnchor alignment, Color color, Font font)
    {
        RectTransform rectTransform = CreateRect(name, parent, anchorMin, anchorMax, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
        Text label = rectTransform.gameObject.AddComponent<Text>();
        label.text = text;
        label.font = font;
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
}
