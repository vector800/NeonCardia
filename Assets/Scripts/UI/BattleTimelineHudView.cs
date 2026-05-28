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
        if (currentHpValue == null)
        {
            return;
        }

        int safeMaxHp = Mathf.Max(0, maxHp);
        int safeCurrentHp = Mathf.Clamp(currentHp, 0, safeMaxHp);
        currentHpValue.text = safeCurrentHp.ToString();
        currentHpValue.color = Color.white;
    }

    public void SetCurrentHpUnavailable()
    {
        if (currentHpValue == null)
        {
            return;
        }

        currentHpValue.text = "--";
        currentHpValue.color = new Color(0.58f, 0.72f, 0.78f, 0.86f);
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
}
