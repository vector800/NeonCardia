using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class BattleCommandSubmenuItem
{
    public BattleCommandDisplayData DisplayData;
    public object Payload;
}

[DisallowMultipleComponent]
public sealed class BattleCommandSubmenuUI : MonoBehaviour
{
    private const int RowCount = 6;
    private const int ReserveCardCount = 3;
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
    private const float ReserveCardHeight = 60f;
    private const float ReserveCardSpacing = 70f;
    private const float InfoPanelX = 340f;
    private const float InfoPanelWidth = 500f;
    private const float InfoPanelHeight = 214f;

    private readonly List<BattleCommandSubmenuItem> items = new List<BattleCommandSubmenuItem>(RowCount);
    private readonly BattleCommandOptionRow[] rows = new BattleCommandOptionRow[RowCount];
    private readonly BattleReserveMemberCardView[] reserveCards = new BattleReserveMemberCardView[ReserveCardCount];

    private CanvasGroup rootGroup;
    private RectTransform listRoot;
    private BattleCommandInfoPanel infoPanel;
    private BattleCommandSpriteSet spriteSet;
    private Coroutine fadeRoutine;
    private int selectedIndex;
    private bool visibleTarget;
    private bool inputLocked;
    private bool sBackKeyEnabled;
    private bool reserveCardLayout;
    private int suppressInputFrames;
    private bool waitingForSKeyRelease;
    private float nextStickMoveTime;

    public event Action<int, object> Submitted;
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

        listRoot = CreateFixedRect("SubmenuListColumn", transform, new Vector2(0f, 0f), new Vector2(ListWidth, ListHeight));

        for (int i = 0; i < RowCount; i++)
        {
            RectTransform rowRect = CreateFixedRect("SubmenuRow_" + i, listRoot, new Vector2(0f, -i * RowSpacing), new Vector2(ListWidth, RowHeight));
            Image rowImage = rowRect.gameObject.AddComponent<Image>();
            rowImage.color = Color.clear;

            BattleCommandOptionRow row = rowRect.gameObject.AddComponent<BattleCommandOptionRow>();
            row.BuildRuntimeUi(i);
            row.PointerEntered += SelectIndex;
            row.Clicked += SubmitIndex;
            rows[i] = row;
            rowRect.gameObject.SetActive(false);
        }

        for (int i = 0; i < ReserveCardCount; i++)
        {
            RectTransform cardRect = CreateFixedRect("ReserveMemberCard_" + i, listRoot, new Vector2(0f, -i * ReserveCardSpacing), new Vector2(ListWidth, ReserveCardHeight));
            Image cardImage = cardRect.gameObject.AddComponent<Image>();
            cardImage.color = Color.clear;

            BattleReserveMemberCardView card = cardRect.gameObject.AddComponent<BattleReserveMemberCardView>();
            card.BuildRuntimeUi(i);
            card.PointerEntered += SelectIndex;
            card.Clicked += SubmitIndex;
            reserveCards[i] = card;
            cardRect.gameObject.SetActive(false);
        }

        RectTransform infoRect = CreateFixedRect("SubmenuInfoPanel", transform, new Vector2(InfoPanelX, 0f), new Vector2(InfoPanelWidth, InfoPanelHeight));
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

    public void SetSBackKeyEnabled(bool enabled)
    {
        sBackKeyEnabled = enabled;
    }

    public void SetReserveCardLayout(bool enabled)
    {
        reserveCardLayout = enabled;
        RefreshRows();
    }

    public void SetItems(IList<BattleCommandSubmenuItem> submenuItems)
    {
        items.Clear();
        if (submenuItems != null)
        {
            int maxItems = reserveCardLayout ? ReserveCardCount : RowCount;
            for (int i = 0; i < submenuItems.Count && i < maxItems; i++)
            {
                items.Add(submenuItems[i]);
            }
        }

        selectedIndex = Mathf.Clamp(selectedIndex, 0, Mathf.Max(0, items.Count - 1));
        if (items.Count > 0 && !IsItemSelectable(selectedIndex))
        {
            selectedIndex = FindNextSelectable(0, 1);
        }

        RefreshRows();
    }

    public void SetVisible(bool visible)
    {
        if (!visible)
        {
            sBackKeyEnabled = false;
            reserveCardLayout = false;
        }

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

    public void ShowPassiveItems(IList<BattleCommandSubmenuItem> submenuItems)
    {
        visibleTarget = true;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        SetItems(submenuItems);
        inputLocked = true;
        if (rootGroup != null)
        {
            rootGroup.alpha = 1f;
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = false;
        }
    }

    public void SetPassiveInfo(BattleCommandDisplayData displayData)
    {
        visibleTarget = true;
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        inputLocked = true;
        if (rootGroup != null)
        {
            rootGroup.alpha = 1f;
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = false;
        }

        if (infoPanel != null)
        {
            infoPanel.Bind(displayData);
        }
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

        if (!IsAcceptingInput)
        {
            return;
        }

        if (WasCancelPressed())
        {
            RequestCancel();
            return;
        }

        if (items.Count == 0)
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
        if (items.Count == 0)
        {
            return;
        }

        int next = FindNextSelectable(selectedIndex + direction, direction);
        SelectIndex(next);
    }

    private int FindNextSelectable(int startIndex, int direction)
    {
        if (items.Count == 0)
        {
            return 0;
        }

        int safeDirection = direction >= 0 ? 1 : -1;
        int index = Mod(startIndex, items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            if (IsItemSelectable(index))
            {
                return index;
            }

            index = Mod(index + safeDirection, items.Count);
        }

        return Mathf.Clamp(selectedIndex, 0, items.Count - 1);
    }

    private void SelectIndex(int index)
    {
        if (!IsValidItemIndex(index))
        {
            return;
        }

        selectedIndex = index;
        RefreshRows();
    }

    public void RestoreSelection(int index)
    {
        if (!IsValidItemIndex(index))
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
        if (!IsItemSelectable(index))
        {
            return;
        }

        selectedIndex = index;
        RefreshRows();
        inputLocked = true;
        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = false;

        if (Submitted != null)
        {
            Submitted(index, items[index].Payload);
        }
    }

    private void RequestCancel()
    {
        if (!IsAcceptingInput || CancelRequested == null)
        {
            return;
        }

        inputLocked = true;
        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = false;
        CancelRequested();
    }

    private void RefreshRows()
    {
        if (infoPanel == null)
        {
            return;
        }

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null)
            {
                continue;
            }

            if (reserveCardLayout)
            {
                rows[i].Bind(null, i);
                continue;
            }

            BattleCommandSubmenuItem item = i < items.Count ? items[i] : null;
            BattleCommandDisplayData data = item != null ? item.DisplayData : null;
            rows[i].Bind(data, i);
            rows[i].SetSelected(i == selectedIndex);
        }

        for (int i = 0; i < reserveCards.Length; i++)
        {
            if (reserveCards[i] == null)
            {
                continue;
            }

            if (!reserveCardLayout)
            {
                reserveCards[i].Bind(null, i);
                continue;
            }

            BattleCommandSubmenuItem item = i < items.Count ? items[i] : null;
            BattleCommandDisplayData data = item != null ? item.DisplayData : null;
            reserveCards[i].Bind(data, i);
            reserveCards[i].SetSelected(i == selectedIndex);
        }

        BattleCommandSubmenuItem selected = IsValidItemIndex(selectedIndex) ? items[selectedIndex] : null;
        infoPanel.Bind(selected != null ? selected.DisplayData : null);
    }

    private bool IsValidItemIndex(int index)
    {
        return index >= 0 && index < items.Count && items[index] != null && items[index].DisplayData != null;
    }

    private bool IsItemSelectable(int index)
    {
        return IsValidItemIndex(index) && items[index].DisplayData.Interactable;
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
        if (keyboard != null && (keyboard.downArrowKey.wasPressedThisFrame || (!sBackKeyEnabled && keyboard.sKey.wasPressedThisFrame)))
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

    private bool WasCancelPressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame || (sBackKeyEnabled && keyboard.sKey.wasPressedThisFrame)))
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
