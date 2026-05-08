using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CardView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Button button;
    private Image image;
    private Text label;
    private int handIndex;
    private Action<int> onClicked;
    private Action<int> onHovered;
    private Action onHoverExited;

    public void Initialize(Button targetButton, int index, Action<int> clicked, Action<int> hovered, Action hoverExited)
    {
        button = targetButton;
        image = button.GetComponent<Image>();
        label = button.GetComponentInChildren<Text>();
        handIndex = index;
        onClicked = clicked;
        onHovered = hovered;
        onHoverExited = hoverExited;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClicked);
    }

    public void Refresh(CardData card, bool battleEnded)
    {
        bool hasCard = card != null;
        button.interactable = hasCard && !battleEnded;

        if (!hasCard)
        {
            label.text = BattleText.Empty;
            image.color = new Color(0.1f, 0.1f, 0.12f);
            return;
        }

        label.text = card.Name + "\n\n" + card.RulesText;
        image.color = new Color(0.18f, 0.23f, 0.32f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onHovered != null)
        {
            onHovered(handIndex);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onHoverExited != null)
        {
            onHoverExited();
        }
    }

    private void HandleClicked()
    {
        if (onClicked != null)
        {
            onClicked(handIndex);
        }
    }
}
