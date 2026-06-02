using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class BattleTimelineSlotView : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private Image background;
    [SerializeField] private Sprite currentBackgroundSprite;
    [SerializeField] private Sprite allyBackgroundSprite;
    [SerializeField] private Sprite enemyBackgroundSprite;
    [SerializeField] private Sprite blankBackgroundSprite;
    [SerializeField] private Image icon;
    [SerializeField] private Text indexText;
    [SerializeField] private Text stateText;
    private CanvasGroup canvasGroup;

    public RectTransform Root { get { return root; } }
    public Image Background { get { return background; } }
    public Sprite CurrentBackgroundSprite { get { return currentBackgroundSprite; } }
    public Sprite AllyBackgroundSprite { get { return allyBackgroundSprite; } }
    public Sprite EnemyBackgroundSprite { get { return enemyBackgroundSprite; } }
    public Sprite BlankBackgroundSprite { get { return blankBackgroundSprite; } }
    public Image Icon { get { return icon; } }
    public Text IndexText { get { return indexText; } }
    public Text StateText { get { return stateText; } }

    private void Reset()
    {
        CacheReferences();
    }

    private void OnValidate()
    {
        CacheReferences();
    }

    public void SetIndex(int index)
    {
        if (indexText != null)
        {
            indexText.text = index.ToString("00");
        }
    }

    public void SetStateText(string value)
    {
        if (stateText != null)
        {
            stateText.text = value ?? string.Empty;
        }
    }

    public void SetTimelineLabelsVisible(bool visible)
    {
        if (indexText != null)
        {
            indexText.text = string.Empty;
            indexText.gameObject.SetActive(visible);
        }

        if (stateText != null)
        {
            stateText.text = string.Empty;
            stateText.gameObject.SetActive(visible);
        }
    }

    public void SetIcon(Sprite sprite, Color fallbackColor)
    {
        if (icon == null)
        {
            return;
        }

        icon.sprite = sprite;
        icon.color = sprite != null ? Color.white : fallbackColor;
        icon.enabled = sprite != null || fallbackColor.a > 0f;
    }

    public void SetActiveVisual(bool isCurrent, Color unitColor)
    {
        SetActiveVisual(isCurrent, unitColor, null);
    }

    public void SetActiveVisual(bool isCurrent, bool isAlly, Color unitColor)
    {
        SetActiveVisual(isCurrent, unitColor, ResolveBackgroundSprite(isCurrent, isAlly));
    }

    public void SetActiveVisual(bool isCurrent, Color unitColor, Sprite backgroundSprite)
    {
        if (background != null)
        {
            background.sprite = backgroundSprite;
            background.type = Image.Type.Sliced;
            background.color = backgroundSprite != null
                ? Color.white
                : isCurrent
                    ? new Color(0.98f, 0.76f, 0.18f, 0.88f)
                    : new Color(unitColor.r, unitColor.g, unitColor.b, 0.62f);
        }

        if (stateText != null)
        {
            stateText.color = isCurrent
                ? new Color(0.06f, 0.04f, 0f, 1f)
                : new Color(
                    Mathf.Lerp(unitColor.r, 1f, 0.72f),
                    Mathf.Lerp(unitColor.g, 1f, 0.72f),
                    Mathf.Lerp(unitColor.b, 1f, 0.72f),
                    0.96f);
        }

        if (indexText != null)
        {
            indexText.color = isCurrent ? new Color(0.06f, 0.045f, 0f, 1f) : Color.white;
        }
    }

    public void SetBlankVisual()
    {
        if (background != null)
        {
            background.sprite = blankBackgroundSprite;
            background.type = Image.Type.Sliced;
            background.color = blankBackgroundSprite != null
                ? Color.white
                : new Color(0.72f, 0.70f, 0.86f, 0.72f);
        }

        SetIcon(null, Color.clear);
        SetTimelineLabelsVisible(false);
    }

    public void SetBackgroundSprites(Sprite currentSprite, Sprite allySprite, Sprite enemySprite)
    {
        currentBackgroundSprite = currentSprite;
        allyBackgroundSprite = allySprite;
        enemyBackgroundSprite = enemySprite;
    }

    public void SetBackgroundSprites(Sprite currentSprite, Sprite allySprite, Sprite enemySprite, Sprite blankSprite)
    {
        SetBackgroundSprites(currentSprite, allySprite, enemySprite);
        blankBackgroundSprite = blankSprite;
    }

    public CanvasGroup EnsureCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        return canvasGroup;
    }

    public void SetAlpha(float alpha)
    {
        CanvasGroup group = EnsureCanvasGroup();
        group.alpha = Mathf.Clamp01(alpha);
    }

    public void Clear()
    {
        SetStateText(string.Empty);
        SetTimelineLabelsVisible(false);
        SetIcon(null, Color.clear);
        if (background != null)
        {
            background.sprite = null;
            background.color = new Color(0.035f, 0.07f, 0.09f, 0.48f);
        }
    }

    public void CacheReferences()
    {
        if (root == null)
        {
            root = transform as RectTransform;
        }

        if (background == null)
        {
            background = GetComponent<Image>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (indexText == null)
        {
            Transform found = transform.Find("IndexText");
            indexText = found != null ? found.GetComponent<Text>() : null;
        }

        if (stateText == null)
        {
            Transform found = transform.Find("StateText");
            stateText = found != null ? found.GetComponent<Text>() : null;
        }

        if (icon == null)
        {
            Transform found = transform.Find("Icon");
            icon = found != null ? found.GetComponent<Image>() : null;
        }
    }

    private Sprite ResolveBackgroundSprite(bool isCurrent, bool isAlly)
    {
        if (isCurrent && currentBackgroundSprite != null)
        {
            return currentBackgroundSprite;
        }

        return isAlly ? allyBackgroundSprite : enemyBackgroundSprite;
    }
}
