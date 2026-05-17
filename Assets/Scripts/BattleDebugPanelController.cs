using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum PanelDebugPreset
{
    AllNormal,
    CrackedTest,
    HoleTest,
    IceTest,
    GrassTest,
    MagmaTest,
    PoisonTest,
    AllTypes
}

public sealed class BattleDebugPanelController : MonoBehaviour
{
    private const float LeftColumnMin = 0.06f;
    private const float LeftColumnMax = 0.49f;
    private const float RightColumnMin = 0.51f;
    private const float RightColumnMax = 0.94f;

    private readonly List<Button> panelTypeButtons = new List<Button>();
    private RectTransform root;
    private RectTransform battleResetRoot;
    private RectTransform debugToggleRoot;
    private Text selectedText;
    private Text enemyCurrentText;
    private Text enemyCandidateText;
    private Text enemySpecText;
    private Font font;
    private BattleManager battleManager;
    private bool toolsEnabled = true;
    private bool visible = true;
    private PanelType selectedPanelType = PanelType.Normal;
    private EnemyType selectedEnemyType = EnemyType.NormalEnemy;

    public bool IsActive
    {
        get { return toolsEnabled && visible; }
    }

    public void Build(BattleManager manager, Transform parent, Font uiFont, bool showTools)
    {
        battleManager = manager;
        font = uiFont;
        toolsEnabled = showTools;
        visible = false;

        root = CreateRect("Debug Panel Root", parent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-330f, -540f), new Vector2(-16f, -16f));
        Image background = root.gameObject.AddComponent<Image>();
        background.color = new Color(0.005f, 0.012f, 0.022f, 0.97f);

        CreateText("Debug Title", root, new Vector2(0f, 0.92f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, "DEBUG PANEL", 24, TextAnchor.MiddleCenter, new Color(0.75f, 1f, 0.98f));
        Button closeButton = CreateButton("Debug Close Button", root, new Vector2(0.86f, 0.93f), new Vector2(0.96f, 0.985f), Vector2.zero, Vector2.zero, "X", 16, new Color(0.30f, 0.08f, 0.08f, 1f));
        closeButton.onClick.AddListener(ClosePanel);
        selectedText = CreateText("Debug Selected", root, new Vector2(0.06f, 0.835f), new Vector2(0.94f, 0.91f), Vector2.zero, Vector2.zero, string.Empty, 18, TextAnchor.MiddleCenter, Color.white);
        CreateText("Debug Hint", root, new Vector2(0.06f, 0.765f), new Vector2(0.94f, 0.825f), Vector2.zero, Vector2.zero, "Click panel / F1 Toggle / F2 Type", 13, TextAnchor.MiddleCenter, new Color(0.95f, 1f, 0.92f));
        CreateText("Panel Type Label", root, new Vector2(0.06f, 0.725f), new Vector2(0.94f, 0.765f), Vector2.zero, Vector2.zero, "PANEL TYPE", 14, TextAnchor.MiddleLeft, new Color(1f, 0.88f, 0.42f));
        CreateText("Preset Label", root, new Vector2(0.06f, 0.385f), new Vector2(0.94f, 0.425f), Vector2.zero, Vector2.zero, "PRESET", 14, TextAnchor.MiddleLeft, new Color(1f, 0.88f, 0.42f));

        BuildPanelTypeButtons();
        BuildPresetButtons();
        BuildEnemyDebugUi();
        BuildBattleResetButton(parent);
        BuildDebugToggleButton(parent);
        selectedEnemyType = battleManager != null ? battleManager.DebugGetEnemyType() : EnemyType.NormalEnemy;
        Refresh();
        ApplyVisibility();
    }

    public void HandlePanelClicked(BattleGridPosition position)
    {
        if (!IsActive || battleManager == null)
        {
            return;
        }

        battleManager.DebugSetPanelType(position, selectedPanelType);
    }

    public void SetToolsEnabled(bool enabled)
    {
        toolsEnabled = enabled;
        if (!toolsEnabled)
        {
            visible = false;
        }

        ApplyVisibility();
    }

    private void OnEnable()
    {
        ApplyVisibility();
    }

    private void OnDisable()
    {
        if (root != null)
        {
            root.gameObject.SetActive(false);
        }

        if (battleResetRoot != null)
        {
            battleResetRoot.gameObject.SetActive(false);
        }

        if (debugToggleRoot != null)
        {
            debugToggleRoot.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!toolsEnabled || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            TogglePanel();
        }

        if (visible && Keyboard.current.f2Key.wasPressedThisFrame)
        {
            CyclePanelType();
        }
    }

    private void BuildPanelTypeButtons()
    {
        PanelType[] panelTypes = (PanelType[])Enum.GetValues(typeof(PanelType));
        for (int i = 0; i < panelTypes.Length; i++)
        {
            int row = i / 2;
            int column = i % 2;
            PanelType panelType = panelTypes[i];
            float minX = column == 0 ? LeftColumnMin : RightColumnMin;
            float maxX = column == 0 ? LeftColumnMax : RightColumnMax;
            float maxY = 0.715f - row * 0.075f;
            float minY = maxY - 0.06f;
            Button button = CreateButton("Panel Type " + panelType, root, new Vector2(minX, minY), new Vector2(maxX, maxY), Vector2.zero, Vector2.zero, panelType.ToString(), 15, GetPanelTypeButtonColor(panelType));
            PanelType capturedType = panelType;
            button.onClick.AddListener(() => SelectPanelType(capturedType));
            panelTypeButtons.Add(button);
        }
    }

    private void BuildPresetButtons()
    {
        AddPresetButton("All Normal", PanelDebugPreset.AllNormal, 0, 0);
        AddPresetButton("All Types", PanelDebugPreset.AllTypes, 0, 1);
        AddPresetButton("Crack", PanelDebugPreset.CrackedTest, 1, 0);
        AddPresetButton("Hole", PanelDebugPreset.HoleTest, 1, 1);
        AddPresetButton("Ice", PanelDebugPreset.IceTest, 2, 0);
        AddPresetButton("Grass", PanelDebugPreset.GrassTest, 2, 1);
        AddPresetButton("Magma", PanelDebugPreset.MagmaTest, 3, 0);
        AddPresetButton("Poison", PanelDebugPreset.PoisonTest, 3, 1);
    }

    private void AddPresetButton(string label, PanelDebugPreset preset, int row, int column)
    {
        float minX = column == 0 ? LeftColumnMin : RightColumnMin;
        float maxX = column == 0 ? LeftColumnMax : RightColumnMax;
        float maxY = 0.36f - row * 0.055f;
        float minY = maxY - 0.046f;
        Button button = CreateButton("Panel Preset " + preset, root, new Vector2(minX, minY), new Vector2(maxX, maxY), Vector2.zero, Vector2.zero, label, 13, new Color(0.15f, 0.22f, 0.31f, 1f));
        button.onClick.AddListener(() => battleManager.DebugApplyPanelPreset(preset));
    }

    private void BuildEnemyDebugUi()
    {
        CreateText("Enemy Debug Label", root, new Vector2(0.06f, 0.115f), new Vector2(0.94f, 0.145f), Vector2.zero, Vector2.zero, "ENEMY DEBUG", 13, TextAnchor.MiddleLeft, new Color(1f, 0.88f, 0.42f));
        enemyCurrentText = CreateText("Enemy Current", root, new Vector2(0.06f, 0.088f), new Vector2(0.94f, 0.116f), Vector2.zero, Vector2.zero, string.Empty, 12, TextAnchor.MiddleLeft, Color.white);
        enemySpecText = CreateText("Enemy Spec", root, new Vector2(0.06f, 0.062f), new Vector2(0.94f, 0.089f), Vector2.zero, Vector2.zero, string.Empty, 11, TextAnchor.MiddleLeft, new Color(0.86f, 1f, 0.92f));
        enemyCandidateText = CreateText("Enemy Candidate", root, new Vector2(0.06f, 0.037f), new Vector2(0.94f, 0.063f), Vector2.zero, Vector2.zero, string.Empty, 12, TextAnchor.MiddleCenter, new Color(0.92f, 1f, 1f));

        Button prevButton = CreateButton("Enemy Previous", root, new Vector2(0.06f, 0.004f), new Vector2(0.22f, 0.035f), Vector2.zero, Vector2.zero, "<", 12, new Color(0.16f, 0.22f, 0.30f, 1f));
        prevButton.onClick.AddListener(() => CycleEnemyType(-1));

        Button nextButton = CreateButton("Enemy Next", root, new Vector2(0.24f, 0.004f), new Vector2(0.40f, 0.035f), Vector2.zero, Vector2.zero, ">", 12, new Color(0.16f, 0.22f, 0.30f, 1f));
        nextButton.onClick.AddListener(() => CycleEnemyType(1));

        Button applyButton = CreateButton("Enemy Apply", root, new Vector2(0.43f, 0.004f), new Vector2(0.94f, 0.035f), Vector2.zero, Vector2.zero, "Apply Enemy", 12, new Color(0.10f, 0.30f, 0.20f, 1f));
        applyButton.onClick.AddListener(ApplySelectedEnemyType);
    }

    private void BuildBattleResetButton(Transform parent)
    {
        battleResetRoot = CreateRect("Battle State Reset Root", parent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-570f, -86f), new Vector2(-344f, -28f));
        Button button = CreateButton("Battle State Reset Button", battleResetRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "Reset Battle State", 16, new Color(0.50f, 0.18f, 0.10f, 1f));
        button.onClick.AddListener(() =>
        {
            battleManager.DebugResetBattleToInitialState();
            selectedEnemyType = battleManager.DebugGetEnemyType();
            Refresh();
        });
    }

    private void BuildDebugToggleButton(Transform parent)
    {
        debugToggleRoot = CreateRect("Debug Toggle Root", parent, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-104f, -54f), new Vector2(-16f, -16f));
        Button button = CreateButton("Debug Toggle Button", debugToggleRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, "DEBUG", 15, new Color(0.08f, 0.24f, 0.30f, 1f));
        button.onClick.AddListener(TogglePanel);
    }

    private void TogglePanel()
    {
        visible = !visible;
        ApplyVisibility();
    }

    private void ClosePanel()
    {
        visible = false;
        ApplyVisibility();
    }

    private void SelectPanelType(PanelType panelType)
    {
        selectedPanelType = panelType;
        Refresh();
    }

    private void CyclePanelType()
    {
        PanelType[] panelTypes = (PanelType[])Enum.GetValues(typeof(PanelType));
        int index = Array.IndexOf(panelTypes, selectedPanelType);
        selectedPanelType = panelTypes[(index + 1) % panelTypes.Length];
        Refresh();
    }

    private void CycleEnemyType(int delta)
    {
        EnemyType[] enemyTypes = EnemyAI.GetDebugEnemyTypes();
        int index = Array.IndexOf(enemyTypes, selectedEnemyType);
        if (index < 0)
        {
            index = 0;
        }

        selectedEnemyType = enemyTypes[(index + delta + enemyTypes.Length) % enemyTypes.Length];
        Refresh();
    }

    private void ApplySelectedEnemyType()
    {
        if (battleManager == null)
        {
            return;
        }

        battleManager.DebugChangeEnemyType(selectedEnemyType);
        Refresh();
    }

    private void Refresh()
    {
        if (selectedText != null)
        {
            selectedText.text = "Selected: " + selectedPanelType;
        }

        PanelType[] panelTypes = (PanelType[])Enum.GetValues(typeof(PanelType));
        for (int i = 0; i < panelTypeButtons.Count && i < panelTypes.Length; i++)
        {
            ColorBlock colors = panelTypeButtons[i].colors;
            Color baseColor = GetPanelTypeButtonColor(panelTypes[i]);
            colors.normalColor = panelTypes[i] == selectedPanelType ? Color.Lerp(baseColor, Color.white, 0.35f) : baseColor;
            colors.highlightedColor = Color.Lerp(baseColor, Color.white, 0.28f);
            colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.25f);
            colors.selectedColor = colors.highlightedColor;
            panelTypeButtons[i].colors = colors;
        }

        if (battleManager == null)
        {
            return;
        }

        if (enemyCurrentText != null)
        {
            enemyCurrentText.text = "Current: " + EnemyAI.GetDisplayName(battleManager.DebugGetEnemyType());
        }

        if (enemySpecText != null)
        {
            enemySpecText.text = battleManager.DebugGetEnemySummary(selectedEnemyType);
        }

        if (enemyCandidateText != null)
        {
            enemyCandidateText.text = "Selected: " + EnemyAI.GetDisplayName(selectedEnemyType);
        }
    }

    private void ApplyVisibility()
    {
        if (root != null)
        {
            root.gameObject.SetActive(toolsEnabled && visible);
        }

        if (battleResetRoot != null)
        {
            battleResetRoot.gameObject.SetActive(toolsEnabled && visible);
        }

        if (debugToggleRoot != null)
        {
            debugToggleRoot.gameObject.SetActive(toolsEnabled);
        }
    }

    private static Color GetPanelTypeButtonColor(PanelType panelType)
    {
        switch (panelType)
        {
            case PanelType.Cracked:
                return new Color(0.62f, 0.43f, 0.12f, 1f);
            case PanelType.Hole:
                return new Color(0.08f, 0.09f, 0.13f, 1f);
            case PanelType.Ice:
                return new Color(0.16f, 0.58f, 0.76f, 1f);
            case PanelType.Grass:
                return new Color(0.12f, 0.50f, 0.18f, 1f);
            case PanelType.Magma:
                return new Color(0.66f, 0.18f, 0.04f, 1f);
            case PanelType.Poison:
                return new Color(0.43f, 0.16f, 0.58f, 1f);
            default:
                return new Color(0.22f, 0.29f, 0.36f, 1f);
        }
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

    private Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string text, int fontSize, TextAnchor alignment, Color color)
    {
        RectTransform rectTransform = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Text label = rectTransform.gameObject.AddComponent<Text>();
        label.text = text;
        label.font = font;
        label.fontSize = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.alignment = alignment;
        label.color = color;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.raycastTarget = false;

        Shadow shadow = rectTransform.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.92f);
        shadow.effectDistance = new Vector2(2f, -2f);

        Outline outline = rectTransform.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.86f);
        outline.effectDistance = new Vector2(1f, -1f);
        return label;
    }

    private Button CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string labelText, int fontSize, Color color)
    {
        RectTransform rectTransform = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Image image = rectTransform.gameObject.AddComponent<Image>();
        image.color = color;
        Button button = rectTransform.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.25f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.1f, 0.1f, 0.12f, 0.45f);
        button.colors = colors;

        Text label = CreateText(name + " Text", rectTransform, Vector2.zero, Vector2.one, new Vector2(6f, 3f), new Vector2(-6f, -3f), labelText, fontSize, TextAnchor.MiddleCenter, Color.white);
        label.resizeTextForBestFit = false;
        return button;
    }
}
