using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class BattleCommandSelectionUI : MonoBehaviour
{
    private const int RowCount = 6;
    private const float FadeSeconds = 0.22f;
    private const float StickRepeatSeconds = 0.18f;
    private const float RootX = 420f;
    private const float RootY = -128f;
    private const float RootWidth = 840f;
    private const float RootHeight = 228f;
    private const float ListWidth = 320f;
    private const float ListHeight = 210f;
    private const float RowHeight = 30f;
    private const float RowSpacing = 36f;
    private const float InfoPanelX = 340f;
    private const float InfoPanelWidth = 500f;
    private const float InfoPanelHeight = 214f;

    private readonly List<BattleCommandDisplayData> options = new List<BattleCommandDisplayData>(RowCount);
    private readonly BattleCommandOptionRow[] rows = new BattleCommandOptionRow[RowCount];

    private CanvasGroup rootGroup;
    private RectTransform commandListRoot;
    private BattleCommandInfoPanel infoPanel;
    private BattleCommandSpriteSet spriteSet;
    private Coroutine fadeRoutine;
    private int selectedIndex;
    private bool visibleTarget;
    private bool inputLocked;
    private float nextStickMoveTime;
    private int suppressInputFrames;
    private bool waitingForSKeyRelease;

    public event Action<int> CardSubmitted;
    public event Action SkillsRequested;
    public event Action ChangeRequested;
    public event Action RunRequested;
    public event Action CancelRequested;

    public bool IsAcceptingInput
    {
        get { return visibleTarget && rootGroup != null && rootGroup.interactable && !inputLocked; }
    }

    public bool IsOpenOrFading
    {
        get { return visibleTarget || (rootGroup != null && rootGroup.alpha > 0.01f); }
    }

    public void BuildRuntimeUi()
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null)
        {
            rect = gameObject.AddComponent<RectTransform>();
        }

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(RootX, RootY);
        rect.sizeDelta = new Vector2(RootWidth, RootHeight);

        rootGroup = GetComponent<CanvasGroup>();
        if (rootGroup == null)
        {
            rootGroup = gameObject.AddComponent<CanvasGroup>();
        }

        rootGroup.alpha = 0f;
        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = false;

        commandListRoot = CreateFixedRect("CommandListColumn", transform, new Vector2(0f, 0f), new Vector2(ListWidth, ListHeight));

        for (int i = 0; i < RowCount; i++)
        {
            RectTransform rowRect = CreateFixedRect(GetRowObjectName(i), commandListRoot, new Vector2(0f, -i * RowSpacing), new Vector2(ListWidth, RowHeight));
            Image rowImage = rowRect.gameObject.AddComponent<Image>();
            rowImage.color = Color.clear;
            BattleCommandOptionRow row = rowRect.gameObject.AddComponent<BattleCommandOptionRow>();
            row.BuildRuntimeUi(i);
            row.PointerEntered += SelectIndex;
            row.Clicked += SubmitIndex;
            rows[i] = row;
        }

        RectTransform infoRect = CreateFixedRect("CommandInfoPanel", transform, new Vector2(InfoPanelX, 0f), new Vector2(InfoPanelWidth, InfoPanelHeight));
        infoPanel = infoRect.gameObject.AddComponent<BattleCommandInfoPanel>();
        infoPanel.BuildRuntimeUi();
    }

    public void SetSpriteSet(BattleCommandSpriteSet sprites)
    {
        spriteSet = sprites;
        if (infoPanel != null)
        {
            infoPanel.SetSprites(spriteSet);
        }
    }

    public void SetOptions(IList<BattleCommandDisplayData> displayOptions)
    {
        options.Clear();
        if (displayOptions != null)
        {
            for (int i = 0; i < displayOptions.Count && i < RowCount; i++)
            {
                options.Add(displayOptions[i]);
            }
        }

        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, options.Count - 1));
        if (options.Count > 0 && !IsOptionSelectable(selectedIndex))
        {
            selectedIndex = FindNextSelectable(0, 1);
        }

        RefreshRows();
    }

    public void SetVisible(bool visible)
    {
        if (visibleTarget == visible && fadeRoutine == null)
        {
            if (visible)
            {
                inputLocked = false;
            }

            return;
        }

        visibleTarget = visible;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeTo(visible ? 1f : 0f, visible, null));
    }

    public void FadeOutThen(Action afterFade)
    {
        visibleTarget = false;
        inputLocked = true;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeTo(0f, false, afterFade));
    }

    public void SetInputLocked(bool locked)
    {
        inputLocked = locked;
        if (rootGroup != null && visibleTarget)
        {
            rootGroup.interactable = !locked;
            rootGroup.blocksRaycasts = !locked;
        }
    }

    private void Update()
    {
        if (ShouldSuppressInput())
        {
            return;
        }

        if (!IsAcceptingInput || options.Count == 0)
        {
            return;
        }

        if (WasUpPressed())
        {
            MoveSelection(-1);
        }
        else if (WasDownPressed())
        {
            MoveSelection(1);
        }
        else if (WasSubmitPressed())
        {
            SubmitIndex(selectedIndex);
        }
        else if (WasCancelPressed() && CancelRequested != null)
        {
            CancelRequested();
        }
    }

    private IEnumerator FadeTo(float targetAlpha, bool interactiveAfterFade, Action afterFade)
    {
        float startAlpha = rootGroup != null ? rootGroup.alpha : 0f;
        float elapsed = 0f;
        inputLocked = true;
        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = false;

        while (elapsed < FadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / FadeSeconds);
            rootGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, SmoothStep(t));
            yield return null;
        }

        rootGroup.alpha = targetAlpha;
        inputLocked = false;
        rootGroup.interactable = interactiveAfterFade;
        rootGroup.blocksRaycasts = interactiveAfterFade;
        fadeRoutine = null;

        if (afterFade != null)
        {
            afterFade();
        }
    }

    private void MoveSelection(int direction)
    {
        if (options.Count == 0)
        {
            return;
        }

        int next = FindNextSelectable(selectedIndex + direction, direction);
        SelectIndex(next);
    }

    private int FindNextSelectable(int startIndex, int direction)
    {
        if (options.Count == 0)
        {
            return 0;
        }

        int safeDirection = direction >= 0 ? 1 : -1;
        int index = Mod(startIndex, options.Count);
        for (int i = 0; i < options.Count; i++)
        {
            if (IsOptionSelectable(index))
            {
                return index;
            }

            index = Mod(index + safeDirection, options.Count);
        }

        return Mathf.Clamp(selectedIndex, 0, options.Count - 1);
    }

    private void SelectIndex(int index)
    {
        if (!IsOptionSelectable(index))
        {
            return;
        }

        selectedIndex = index;
        RefreshRows();
    }

    public void RestoreSelection(int index)
    {
        if (!IsOptionSelectable(index))
        {
            return;
        }

        selectedIndex = index;
        RefreshRows();
    }

    public void SuppressInputUntilSKeyRelease(int minimumFrames)
    {
        suppressInputFrames = Mathf.Max(suppressInputFrames, Mathf.Max(0, minimumFrames));
        waitingForSKeyRelease = true;
        nextStickMoveTime = Time.unscaledTime + StickRepeatSeconds;
    }

    private void SubmitIndex(int index)
    {
        if (!IsOptionSelectable(index))
        {
            return;
        }

        selectedIndex = index;
        RefreshRows();
        BattleCommandDisplayData selected = options[index];
        switch (selected.OptionType)
        {
            case BattleCommandOptionType.Card:
                if (CardSubmitted != null)
                {
                    CardSubmitted(selected.SourceHandIndex);
                }
                break;
            case BattleCommandOptionType.Skills:
                if (SkillsRequested != null)
                {
                    SkillsRequested();
                }
                break;
            case BattleCommandOptionType.Change:
                if (ChangeRequested != null)
                {
                    ChangeRequested();
                }
                break;
            case BattleCommandOptionType.Run:
                if (RunRequested != null)
                {
                    RunRequested();
                }
                break;
        }
    }

    private void RefreshRows()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            BattleCommandDisplayData data = i < options.Count ? options[i] : null;
            rows[i].Bind(data, i);
            rows[i].SetSelected(i == selectedIndex);
        }

        infoPanel.Bind(selectedIndex >= 0 && selectedIndex < options.Count ? options[selectedIndex] : null);
    }

    private bool IsOptionSelectable(int index)
    {
        return index >= 0 && index < options.Count && options[index] != null && options[index].Interactable;
    }

    private bool WasUpPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame))
        {
            return true;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad == null)
        {
            return false;
        }

        if (gamepad.dpad.up.wasPressedThisFrame)
        {
            return true;
        }

        float y = gamepad.leftStick.ReadValue().y;
        if (y > 0.55f && Time.unscaledTime >= nextStickMoveTime)
        {
            nextStickMoveTime = Time.unscaledTime + StickRepeatSeconds;
            return true;
        }

        return false;
    }

    private bool WasDownPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame))
        {
            return true;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad == null)
        {
            return false;
        }

        if (gamepad.dpad.down.wasPressedThisFrame)
        {
            return true;
        }

        float y = gamepad.leftStick.ReadValue().y;
        if (y < -0.55f && Time.unscaledTime >= nextStickMoveTime)
        {
            nextStickMoveTime = Time.unscaledTime + StickRepeatSeconds;
            return true;
        }

        return false;
    }

    private static bool WasSubmitPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
        {
            return true;
        }

        Gamepad gamepad = Gamepad.current;
        return gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;
    }

    private static bool WasCancelPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            return true;
        }

        Gamepad gamepad = Gamepad.current;
        return gamepad != null && gamepad.buttonEast.wasPressedThisFrame;
    }

    private bool ShouldSuppressInput()
    {
        bool suppressed = false;
        if (suppressInputFrames > 0)
        {
            suppressInputFrames--;
            suppressed = true;
        }

        if (waitingForSKeyRelease)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.sKey.isPressed)
            {
                return true;
            }

            waitingForSKeyRelease = false;
            return suppressed;
        }

        return suppressed;
    }

    private static int Mod(int value, int count)
    {
        return (value % count + count) % count;
    }

    private static float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private static string GetRowObjectName(int index)
    {
        switch (index)
        {
            case 0:
            case 1:
            case 2:
                return "CardRow_" + index;
            case 3:
                return "OptionRow_SKILL";
            case 4:
                return "OptionRow_CHANGE";
            case 5:
                return "OptionRow_RUN";
            default:
                return "CommandRow_" + index;
        }
    }

    private static RectTransform CreateFixedRect(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        child.transform.SetParent(parent, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return rect;
    }
}
