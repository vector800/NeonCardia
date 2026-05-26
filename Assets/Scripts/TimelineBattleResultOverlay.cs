using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class TimelineBattleResultOverlay : MonoBehaviour
{
    private Font uiFont;
    private Action retryAction;
    private Action menuAction;
    private Action deckBuildAction;
    private CanvasGroup rootGroup;
    private CanvasGroup rankGroup;
    private CanvasGroup rewardGroup;
    private CanvasGroup buttonGroup;
    private Text rankText;
    private Text statsText;
    private Text rewardText;
    private Coroutine sequenceCoroutine;

    public void Build(Transform parent, Font font, Action retry, Action menu, Action deckBuild)
    {
        uiFont = font;
        retryAction = retry;
        menuAction = menu;
        deckBuildAction = deckBuild;

        RectTransform rootRect = GetOrAddRectTransform(gameObject);
        rootRect.SetParent(parent, false);
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        rootGroup = GetOrAddCanvasGroup(gameObject);
        Image dim = CreateImage("Timeline Result Dim", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0.01f, 0.02f, 0.72f));
        dim.raycastTarget = true;

        RectTransform panel = CreatePanel("Timeline Result Panel", transform, new Vector2(0.19f, 0.13f), new Vector2(0.81f, 0.86f), new Color(0.018f, 0.04f, 0.055f, 0.98f));
        CreateImage("Timeline Result Accent", panel, new Vector2(0f, 0.965f), Vector2.one, Vector2.zero, Vector2.zero, new Color(0.1f, 0.9f, 1f, 0.95f)).raycastTarget = false;
        CreateText("Timeline Result Title", panel, new Vector2(0.08f, 0.81f), new Vector2(0.92f, 0.93f), Vector2.zero, Vector2.zero, "RESULT", 54, TextAnchor.MiddleCenter, new Color(0.92f, 1f, 1f));
        CreateText("Timeline Hunting Label", panel, new Vector2(0.12f, 0.66f), new Vector2(0.55f, 0.75f), Vector2.zero, Vector2.zero, "HUNTING LEVEL", 25, TextAnchor.MiddleCenter, Color.white);

        RectTransform rankRoot = CreateRect("Timeline Rank Root", panel, new Vector2(0.58f, 0.56f), new Vector2(0.86f, 0.78f), Vector2.zero, Vector2.zero);
        rankGroup = rankRoot.gameObject.AddComponent<CanvasGroup>();
        rankText = CreateText("Timeline Rank Text", rankRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "B", 96, TextAnchor.MiddleCenter, Color.white);

        statsText = CreateText("Timeline Result Stats", panel, new Vector2(0.12f, 0.42f), new Vector2(0.88f, 0.56f), Vector2.zero, Vector2.zero, string.Empty, 22, TextAnchor.MiddleCenter, new Color(0.82f, 0.94f, 1f));

        RectTransform rewardRoot = CreatePanel("Timeline Reward Panel", panel, new Vector2(0.11f, 0.23f), new Vector2(0.89f, 0.37f), new Color(0.025f, 0.075f, 0.08f, 0.96f));
        rewardGroup = rewardRoot.gameObject.AddComponent<CanvasGroup>();
        rewardText = CreateText("Timeline Reward Text", rewardRoot, Vector2.zero, Vector2.one, new Vector2(16f, 4f), new Vector2(-16f, -4f), string.Empty, 24, TextAnchor.MiddleCenter, Color.white);
        rewardText.resizeTextForBestFit = true;
        rewardText.resizeTextMinSize = 12;
        rewardText.resizeTextMaxSize = 24;

        RectTransform buttons = CreateRect("Timeline Result Buttons", panel, new Vector2(0.08f, 0.06f), new Vector2(0.92f, 0.17f), Vector2.zero, Vector2.zero);
        buttonGroup = buttons.gameObject.AddComponent<CanvasGroup>();
        Button retryButton = CreateButton("Timeline Retry Button", buttons, new Vector2(0f, 0f), new Vector2(0.31f, 1f), Vector2.zero, Vector2.zero, "もう一度戦う", 20, new Color(0.03f, 0.18f, 0.30f));
        retryButton.onClick.AddListener(InvokeRetry);
        Button menuButton = CreateButton("Timeline Menu Button", buttons, new Vector2(0.345f, 0f), new Vector2(0.655f, 1f), Vector2.zero, Vector2.zero, "メニューへ", 20, new Color(0.05f, 0.05f, 0.18f));
        menuButton.onClick.AddListener(InvokeMenu);
        Button deckButton = CreateButton("Timeline Deck Button", buttons, new Vector2(0.69f, 0f), Vector2.one, Vector2.zero, Vector2.zero, "デッキ編集へ", 20, new Color(0.17f, 0.12f, 0.24f));
        deckButton.onClick.AddListener(InvokeDeckBuild);

        transform.SetAsLastSibling();
        HideImmediate();
    }

    public void Show(BattleResultData resultData, string rewardCardName)
    {
        if (resultData == null)
        {
            return;
        }

        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        rankText.text = resultData.HuntingLevel.ToString();
        rankText.color = GetLevelColor(resultData.HuntingLevel);
        statsText.text = "Victory Turn " + resultData.VictoryTurn
            + "   /   Damage Taken " + resultData.PlayerDamageTakenCount
            + "   /   Max Simul KO " + resultData.MaxSimultaneousDefeatCount;
        rewardText.text = "報酬カード: " + (string.IsNullOrEmpty(rewardCardName) ? "アクアショット" : rewardCardName);

        PrepareGroups();
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
        SetAlpha(rankGroup, 0f);
        SetAlpha(rewardGroup, 0f);
        SetAlpha(buttonGroup, 0f);
        if (rootGroup != null)
        {
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    private void PrepareGroups()
    {
        SetAlpha(rootGroup, 0f);
        SetAlpha(rankGroup, 0f);
        SetAlpha(rewardGroup, 0f);
        SetAlpha(buttonGroup, 0f);
        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = true;
        buttonGroup.interactable = false;
        buttonGroup.blocksRaycasts = false;
    }

    private IEnumerator PlaySequence()
    {
        yield return FadeCanvasGroup(rootGroup, 0f, 1f, 0.22f);
        yield return FadeCanvasGroup(rankGroup, 0f, 1f, 0.18f);
        yield return new WaitForSeconds(0.12f);
        yield return FadeCanvasGroup(rewardGroup, 0f, 1f, 0.24f);
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

    private void InvokeDeckBuild()
    {
        if (deckBuildAction != null)
        {
            deckBuildAction();
        }
    }

    private static Color GetLevelColor(HuntingLevel huntingLevel)
    {
        switch (huntingLevel)
        {
            case HuntingLevel.S:
                return new Color(0.78f, 1f, 0.12f);
            case HuntingLevel.A:
                return new Color(0.25f, 0.95f, 1f);
            default:
                return new Color(0.88f, 0.9f, 1f);
        }
    }

    private static void SetAlpha(CanvasGroup group, float alpha)
    {
        if (group != null)
        {
            group.alpha = alpha;
        }
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

    private RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        RectTransform rectTransform = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        Image image = rectTransform.gameObject.AddComponent<Image>();
        image.color = color;
        return rectTransform;
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
        shadow.effectColor = new Color(0f, 0f, 0f, 0.78f);
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
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.16f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.22f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.15f, 0.15f, 0.16f, 0.55f);
        button.colors = colors;

        CreateText(name + " Text", image.transform, Vector2.zero, Vector2.one, new Vector2(8f, 4f), new Vector2(-8f, -4f), labelText, fontSize, TextAnchor.MiddleCenter, Color.white);
        return button;
    }
}
