using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class CardView : MonoBehaviour
{
    private Button button;
    private Image image;
    private Text label;
    private int handIndex;
    private Action<int> onClicked;

    public void Initialize(Button targetButton, int index, Action<int> clicked)
    {
        button = targetButton;
        image = button.GetComponent<Image>();
        label = button.GetComponentInChildren<Text>();
        handIndex = index;
        onClicked = clicked;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClicked);
    }

    public void Refresh(CardData card, bool playable, bool battleEnded)
    {
        bool hasCard = card != null;
        button.interactable = hasCard && !battleEnded;

        if (!hasCard)
        {
            label.text = "EMPTY";
            image.color = new Color(0.1f, 0.1f, 0.12f);
            return;
        }

        label.text = card.Name + "\n\n" + card.RulesText;
        image.color = playable ? new Color(0.18f, 0.23f, 0.32f) : new Color(0.28f, 0.16f, 0.17f);
    }

    private void HandleClicked()
    {
        if (onClicked != null)
        {
            onClicked(handIndex);
        }
    }
}
