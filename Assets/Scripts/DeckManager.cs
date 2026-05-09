using System.Collections.Generic;
using UnityEngine;

public sealed class CardInstance
{
    public CardInstance(CardData data, int id)
    {
        Data = data;
        Id = id;
    }

    public CardData Data { get; private set; }
    public int Id { get; private set; }
}

public sealed class DrawResult
{
    public int DrawnCount { get; set; }
    public bool Reshuffled { get; set; }
}

public sealed class DeckManager
{
    private readonly List<CardInstance> drawPile = new List<CardInstance>();
    private readonly List<CardInstance> discardPile = new List<CardInstance>();

    public DeckManager(List<CardData> starterCards)
    {
        for (int i = 0; i < starterCards.Count; i++)
        {
            drawPile.Add(new CardInstance(starterCards[i], i));
        }

        Shuffle(drawPile);
    }

    public List<CardInstance> Hand { get; private set; } = new List<CardInstance>();
    public int DrawPileCount { get { return drawPile.Count; } }
    public int HandCount { get { return Hand.Count; } }
    public int DiscardPileCount { get { return discardPile.Count; } }

    public DrawResult DrawUpTo(int handSize)
    {
        DrawResult result = new DrawResult();
        while (Hand.Count < handSize)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0)
                {
                    return result;
                }

                RecycleDiscardPile();
                result.Reshuffled = true;
            }

            if (drawPile.Count == 0)
            {
                return result;
            }

            CardInstance nextCard = drawPile[0];
            drawPile.RemoveAt(0);
            Hand.Add(nextCard);
            result.DrawnCount++;
        }

        return result;
    }

    public void DiscardFromHand(int handIndex)
    {
        if (handIndex < 0 || handIndex >= Hand.Count)
        {
            return;
        }

        CardInstance card = Hand[handIndex];
        Hand.RemoveAt(handIndex);
        discardPile.Add(card);
    }

    public bool DiscardFromHand(CardInstance card)
    {
        if (card == null)
        {
            return false;
        }

        bool removed = Hand.Remove(card);
        if (removed)
        {
            discardPile.Add(card);
        }

        return removed;
    }

    private void RecycleDiscardPile()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        Shuffle(drawPile);
    }

    private void Shuffle(List<CardInstance> cards)
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            CardInstance temp = cards[i];
            cards[i] = cards[swapIndex];
            cards[swapIndex] = temp;
        }
    }
}
