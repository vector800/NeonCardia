using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class BattleResultOverlay : MonoBehaviour
{
    private const string ResultPanelResourcePath = "UI/BattleResult/ResultPanel";
    private const string RewardCardResourcePath = "UI/BattleResult/RewardCard";
    private const float ResultPanelAspect = 1792f / 1024f;

    private readonly List<CanvasGroup> rewardLineGroups = new List<CanvasGroup>();

    [SerializeField] private bool showResultSafeAreaDebug = false;
    [SerializeField, Range(-0.05f, 0.05f)] private float huntingLevelVerticalOffset = 0.025f;
    [SerializeField, Range(-0.05f, 0.05f)] private float huntingRankVerticalOffset = 0.025f;
    [SerializeField, Range(-0.04f, 0.04f)] private float rewardTextVerticalOffset = 0.018f;
    [SerializeField, Range(0.75f, 1f)] private float rewardIconScale = 0.92f;
    [SerializeField] private Vector2 rewardIconOffset = new Vector2(0f, 0.01f);

    private Font uiFont;
    private Action retryAction;
    private Action menuAction;
    private CanvasGroup rootGroup;
    private CanvasGroup huntingLevelGroup;
    private CanvasGroup huntingRankGroup;
    private CanvasGroup rewardGroup;
    private CanvasGroup buttonGroup;
    private RectTransform rewardCardNameRoot;
    private Text huntingRankText;
    private Coroutine sequenceCoroutine;

    public void Build(Transform parent, Font font, Action retry, Action menu)
    {
        uiFont = font;
        retryAction = retry;
        menuAction = menu;

        RectTransform rootRect = GetOrAddRectTransform(gameObject);
        rootRect.SetParent(parent, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        rootGroup = GetOrAddCanvasGroup(gameObject);

        Image dim = CreateImage("Battle Result Dim", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0.01f, 0.02f, 0.68f));
        dim.raycastTarget = true;

        RectTransform panelFitArea = CreateRect("ResultPanelFitArea", transform, new Vector2(0.1f, 0.16f), new Vector2(0.9f, 0.86f), Vector2.zero, Vector2.zero);
        RectTransform panelRoot = CreateRect("ResultPanel", panelFitArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AspectRatioFitter aspectFitter = panelRoot.gameObject.AddComponent<AspectRatioFitter>();
        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        aspectFitter.aspectRatio = ResultPanelAspect;

        Image generatedPanel = CreateImage("ResultBackgroundImage", panelRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        generatedPanel.sprite = LoadSprite(ResultPanelResourcePath);
        generatedPanel.preserveAspect = true;
        generatedPanel.raycastTarget = true;

        RectTransform huntingLevelArea = CreateSafeArea("HuntingLevelArea", panelRoot, OffsetY(new Vector2(0.13f, 0.52f), huntingLevelVerticalOffset), OffsetY(new Vector2(0.63f, 0.68f), huntingLevelVerticalOffset), new Color(0.3f, 1f, 0.7f, 1f));
        huntingLevelGroup = huntingLevelArea.gameObject.AddComponent<CanvasGroup>();
        CreateText("HuntingLevelText", huntingLevelArea, Vector2.zero, Vector2.one, new Vector2(12f, 4f), new Vector2(-12f, -4f), "HUNTING LEVEL", 16, 32, TextAnchor.MiddleCenter, Color.white);

        RectTransform huntingRankArea = CreateSafeArea("HuntingRankArea", panelRoot, OffsetY(new Vector2(0.67f, 0.43f), huntingRankVerticalOffset), OffsetY(new Vector2(0.85f, 0.74f), huntingRankVerticalOffset), new Color(0.9f, 1f, 0.2f, 1f));
        huntingRankGroup = huntingRankArea.gameObject.AddComponent<CanvasGroup>();
        huntingRankText = CreateText("HuntingRankText", huntingRankArea, Vector2.zero, Vector2.one, new Vector2(4f, 0f), new Vector2(-4f, 0f), "B", 48, 112, TextAnchor.MiddleCenter, Color.white);

        RectTransform rewardCardNameArea = CreateSafeArea("RewardCardNameArea", panelRoot, OffsetY(new Vector2(0.13f, 0.18f), rewardTextVerticalOffset), OffsetY(new Vector2(0.68f, 0.29f), rewardTextVerticalOffset), new Color(1f, 1f, 1f, 1f));
        rewardGroup = rewardCardNameArea.gameObject.AddComponent<CanvasGroup>();
        rewardCardNameRoot = CreateRect("RewardCardNameMask", rewardCardNameArea, Vector2.zero, Vector2.one, new Vector2(10f, 2f), new Vector2(-10f, -2f));
        rewardCardNameRoot.gameObject.AddComponent<RectMask2D>();

        RectTransform rewardIconArea = CreateSafeArea("RewardIconArea", panelRoot, new Vector2(0.75f, 0.14f), new Vector2(0.89f, 0.34f), new Color(1f, 0.8f, 0.2f, 1f));
        Vector2 rewardIconAnchorMin;
        Vector2 rewardIconAnchorMax;
        GetCenteredAnchors(rewardIconScale, rewardIconOffset, out rewardIconAnchorMin, out rewardIconAnchorMax);
        Image rewardIcon = CreateImage("RewardIconImage", rewardIconArea, rewardIconAnchorMin, rewardIconAnchorMax, Vector2.zero, Vector2.zero, Color.white);
        rewardIcon.sprite = LoadSprite(RewardCardResourcePath);
        rewardIcon.preserveAspect = true;
        rewardIcon.raycastTarget = false;

        buttonGroup = CreateGroup("ResultButtonArea", panelRoot, new Vector2(0.24f, 0.02f), new Vector2(0.76f, 0.1f));
        Button retryButton = CreateButton("Retry Battle Button", buttonGroup.transform, Vector2.zero, new Vector2(0.46f, 1f), Vector2.zero, Vector2.zero, "もう一度戦う", 20, new Color(0.02f, 0.17f, 0.28f));
        retryButton.onClick.AddListener(InvokeRetry);
        Button menuButton = CreateButton("Return Menu Button", buttonGroup.transform, new Vector2(0.54f, 0f), Vector2.one, Vector2.zero, Vector2.zero, "メニューへ戻る", 20, new Color(0.05f, 0.04f, 0.17f));
        menuButton.onClick.AddListener(InvokeMenu);

        transform.SetAsLastSibling();
        HideImmediate();
    }

    public void Show(BattleResultData resultData, List<string> rewardLines)
    {
        if (resultData == null)
        {
            return;
        }

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        ApplyResultData(resultData);
        BuildRewardLines(rewardLines);
        PrepareSequenceGroups();

        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
        }

        sequenceCoroutine = StartCoroutine(PlaySequence());
    }

    public void HideImmediate()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        SetAlpha(rootGroup, 0f);
        SetAlpha(huntingLevelGroup, 0f);
        SetAlpha(huntingRankGroup, 0f);
        SetAlpha(rewardGroup, 0f);
        SetAlpha(buttonGroup, 0f);
        SetRewardLinesAlpha(0f);

        if (rootGroup != null)
        {
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    private void ApplyResultData(BattleResultData resultData)
    {
        if (huntingRankText != null)
        {
            huntingRankText.text = resultData.HuntingLevel.ToString();
            huntingRankText.color = GetLevelColor(resultData.HuntingLevel);
        }
    }

    private void BuildRewardLines(List<string> rewardLines)
    {
        for (int i = rewardCardNameRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(rewardCardNameRoot.GetChild(i).gameObject);
        }

        rewardLineGroups.Clear();
        if (rewardLines == null || rewardLines.Count == 0)
        {
            rewardLines = new List<string> { "仮カード" };
        }

        string rewardLine = rewardLines[0];
        Text rewardText = CreateText("RewardCardNameText", rewardCardNameRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, rewardLine, 14, 28, TextAnchor.MiddleCenter, Color.white);
        CanvasGroup lineGroup = rewardText.gameObject.AddComponent<CanvasGroup>();
        lineGroup.alpha = 0f;
        rewardLineGroups.Add(lineGroup);
    }

    private void PrepareSequenceGroups()
    {
        SetAlpha(rootGroup, 0f);
        SetAlpha(huntingLevelGroup, 0f);
        SetAlpha(huntingRankGroup, 0f);
        SetAlpha(rewardGroup, 0f);
        SetAlpha(buttonGroup, 0f);
        SetRewardLinesAlpha(0f);

        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = true;
        buttonGroup.interactable = false;
        buttonGroup.blocksRaycasts = false;
    }

    private IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(0.45f);
        yield return FadeCanvasGroup(rootGroup, 0f, 1f, 0.28f);
        yield return FadeCanvasGroup(huntingLevelGroup, 0f, 1f, 0.18f);
        yield return FadeCanvasGroup(huntingRankGroup, 0f, 1f, 0.18f);
        yield return new WaitForSeconds(0.12f);
        yield return FadeCanvasGroup(rewardGroup, 0f, 1f, 0.18f);

        for (int i = 0; i < rewardLineGroups.Count; i++)
        {
            yield return FadeCanvasGroup(rewardLineGroups[i], 0f, 1f, 0.14f);
            yield return new WaitForSeconds(0.04f);
        }

        yield return FadeCanvasGroup(buttonGroup, 0f, 1f, 0.16f);
        rootGroup.interactable = true;
        buttonGroup.interactable = true;
        buttonGroup.blocksRaycasts = true;
        sequenceCoroutine = null;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
        {
            yield break;
        }

        float elapsed = 0f;
        group.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(from, to, progress);
            yield return null;
        }

        group.alpha = to;
    }

    private void InvokeRetry()
    {
        if (retryAction != null)
        {
            retryAction();
        }
    }

    private void InvokeMenu()
    {
        if (menuAction != null)
        {
            menuAction();
        }
    }

    private static Color GetLevelColor(HuntingLevel huntingLevel)
    {
        switch (huntingLevel)
        {
            case HuntingLevel.S:
                return new Color(0.75f, 1f, 0.12f);
            case HuntingLevel.A:
                return new Color(0.3f, 0.95f, 1f);
            default:
                return new Color(0.88f, 0.9f, 1f);
        }
    }

    private static Sprite LoadSprite(string resourcePath)
    {
        Sprite importedSprite = Resources.Load<Sprite>(resourcePath);
        if (importedSprite != null)
        {
            return importedSprite;
        }

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            return null;
        }

        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private static RectTransform GetOrAddRectTransform(GameObject target)
    {
        RectTransform rectTransform = target.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = target.AddComponent<RectTransform>();
        }

        return rectTransform;
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }

    private CanvasGroup CreateGroup(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        return rect.gameObject.AddComponent<CanvasGroup>();
    }

    private RectTransform CreateSafeArea(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color debugColor)
    {
        RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        rect.gameObject.AddComponent<RectMask2D>();

        if (showResultSafeAreaDebug)
        {
            Image debugImage = rect.gameObject.AddComponent<Image>();
            debugImage.color = new Color(debugColor.r, debugColor.g, debugColor.b, 0.08f);
            debugImage.raycastTarget = false;

            Outline outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(debugColor.r, debugColor.g, debugColor.b, 0.6f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        return rect;
    }

    private static Vector2 OffsetY(Vector2 value, float offset)
    {
        return new Vector2(value.x, Mathf.Clamp01(value.y + offset));
    }

    private static void GetCenteredAnchors(float scale, Vector2 offset, out Vector2 anchorMin, out Vector2 anchorMax)
    {
        float halfSize = Mathf.Clamp(scale, 0.1f, 1f) * 0.5f;
        float centerX = Mathf.Clamp(0.5f + offset.x, halfSize, 1f - halfSize);
        float centerY = Mathf.Clamp(0.5f + offset.y, halfSize, 1f - halfSize);
        anchorMin = new Vector2(centerX - halfSize, centerY - halfSize);
        anchorMax = new Vector2(centerX + halfSize, centerY + halfSize);
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

    private Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string text, int fontSizeMin, int fontSizeMax, TextAnchor alignment, Color color)
    {
        RectTransform rectTransform = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Text label = rectTransform.gameObject.AddComponent<Text>();
        label.text = text;
        label.font = uiFont;
        label.fontSize = fontSizeMax;
        label.fontStyle = FontStyle.Bold;
        label.alignment = alignment;
        label.color = color;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = fontSizeMin;
        label.resizeTextMaxSize = fontSizeMax;
        label.raycastTarget = false;

        Shadow shadow = rectTransform.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.82f);
        shadow.effectDistance = new Vector2(2f, -2f);

        Outline outline = rectTransform.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.74f);
        outline.effectDistance = new Vector2(1f, -1f);

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
        colors.pressedColor = Color.Lerp(color, Color.black, 0.22f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.16f, 0.16f, 0.2f, 0.45f);
        button.colors = colors;

        Outline buttonOutline = image.gameObject.AddComponent<Outline>();
        buttonOutline.effectColor = new Color(0f, 0.95f, 1f, 0.42f);
        buttonOutline.effectDistance = new Vector2(1f, -1f);

        CreateText(name + " Text", image.transform, Vector2.zero, Vector2.one, new Vector2(8f, 6f), new Vector2(-8f, -6f), labelText, 12, fontSize, TextAnchor.MiddleCenter, Color.white);

        return button;
    }

    private static void SetAlpha(CanvasGroup group, float alpha)
    {
        if (group != null)
        {
            group.alpha = alpha;
        }
    }

    private void SetRewardLinesAlpha(float alpha)
    {
        for (int i = 0; i < rewardLineGroups.Count; i++)
        {
            rewardLineGroups[i].alpha = alpha;
        }
    }
}
