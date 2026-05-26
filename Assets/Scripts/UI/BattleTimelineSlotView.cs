using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class BattleTimelineSlotView : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private Text indexText;
    [SerializeField] private Text stateText;

    public RectTransform Root { get { return root; } }
    public Image Background { get { return background; } }
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
        if (background != null)
        {
            background.color = isCurrent
                ? new Color(0.98f, 0.76f, 0.18f, 0.88f)
                : new Color(unitColor.r, unitColor.g, unitColor.b, 0.62f);
        }

        if (stateText != null)
        {
            stateText.color = isCurrent ? new Color(0.05f, 0.035f, 0.004f, 1f) : Color.white;
        }
    }

    public void Clear()
    {
        SetStateText(string.Empty);
        SetIcon(null, Color.clear);
        if (background != null)
        {
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
}
