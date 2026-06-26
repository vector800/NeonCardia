using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class BattleTimelineHudView : MonoBehaviour
{
    [SerializeField] private RectTransform leftPanel;
    [SerializeField] private Text actionOrderText;
    [SerializeField] private Text currentHpLabel;
    [SerializeField] private Text currentHpValue;
    [SerializeField] private RectTransform slotsRoot;
    [SerializeField] private BattleTimelineSlotView[] slots = new BattleTimelineSlotView[0];
    [SerializeField] private RectTransform connectorLine;
    [SerializeField] private RectTransform currentMarker;
    [SerializeField] private RectTransform rightArrow;

    public RectTransform LeftPanel { get { return leftPanel; } }
    public Text ActionOrderText { get { return actionOrderText; } }
    public Text CurrentHpLabel { get { return currentHpLabel; } }
    public Text CurrentHpValue { get { return currentHpValue; } }
    public RectTransform SlotsRoot { get { return slotsRoot; } }
    public BattleTimelineSlotView[] Slots { get { return slots; } }
    public RectTransform ConnectorLine { get { return connectorLine; } }
    public RectTransform CurrentMarker { get { return currentMarker; } }
    public RectTransform RightArrow { get { return rightArrow; } }

    private void Reset()
    {
        CacheReferences();
    }

    private void OnValidate()
    {
        CacheReferences();
    }

    public void SetCurrentHp(int currentHp, int maxHp)
    {
        HideCurrentHpDisplay();
    }

    public void SetCurrentHpUnavailable()
    {
        HideCurrentHpDisplay();
    }

    public void HideCurrentHpDisplay()
    {
        SetObjectActive(actionOrderText, false);
        SetObjectActive(currentHpLabel, false);
        SetObjectActive(currentHpValue, false);

        SetChildActiveIfPresent(leftPanel, "CurrentHpGaugeBack", false);
        SetChildActiveIfPresent(leftPanel, "CurrentHpPanelImage", false);

        Image leftPanelImage = leftPanel != null ? leftPanel.GetComponent<Image>() : null;
        if (leftPanelImage != null)
        {
            leftPanelImage.enabled = false;
        }

        SetObjectActive(leftPanel, false);
    }

    public BattleTimelineSlotView GetSlot(int index)
    {
        return slots != null && index >= 0 && index < slots.Length ? slots[index] : null;
    }

    public void CacheReferences()
    {
        leftPanel = FindRect("LeftPanel", leftPanel);
        slotsRoot = FindRect("SlotsRoot", slotsRoot);
        connectorLine = FindRect("ConnectorLine", connectorLine);
        currentMarker = FindRect("CurrentMarker", currentMarker);
        rightArrow = FindRect("RightArrow", rightArrow);

        actionOrderText = FindText("LeftPanel/ActionOrderText", actionOrderText);
        currentHpLabel = FindText("LeftPanel/CurrentHpLabel", currentHpLabel);
        currentHpValue = FindText("LeftPanel/CurrentHpValue", currentHpValue);

        if (slotsRoot != null)
        {
            slots = slotsRoot.GetComponentsInChildren<BattleTimelineSlotView>(true);
        }
    }

    private RectTransform FindRect(string path, RectTransform fallback)
    {
        if (fallback != null)
        {
            return fallback;
        }

        Transform found = transform.Find(path);
        return found != null ? found as RectTransform : null;
    }

    private Text FindText(string path, Text fallback)
    {
        if (fallback != null)
        {
            return fallback;
        }

        Transform found = transform.Find(path);
        return found != null ? found.GetComponent<Text>() : null;
    }

    private static void SetObjectActive(Component component, bool active)
    {
        if (component != null)
        {
            component.gameObject.SetActive(active);
        }
    }

    private static void SetChildActiveIfPresent(Transform parent, string childName, bool active)
    {
        if (parent == null)
        {
            return;
        }

        Transform child = parent.Find(childName);
        if (child != null)
        {
            child.gameObject.SetActive(active);
        }
    }
}
