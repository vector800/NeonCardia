using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerCardCollection
{
    public static List<CardData> LoadOwnedCards()
    {
        CardData[] loadedCards = Resources.LoadAll<CardData>("Cards");
        List<CardData> cards = new List<CardData>();

        for (int i = 0; i < loadedCards.Length; i++)
        {
            if (loadedCards[i] != null && loadedCards[i].Effect != CardEffectType.Move)
            {
                cards.Add(loadedCards[i]);
            }
        }

        cards.Sort(CompareCards);
        return cards;
    }

    public static Dictionary<string, CardData> CreateCardMap()
    {
        List<CardData> cards = LoadOwnedCards();
        Dictionary<string, CardData> map = new Dictionary<string, CardData>();
        for (int i = 0; i < cards.Count; i++)
        {
            if (!map.ContainsKey(cards[i].CardId))
            {
                map.Add(cards[i].CardId, cards[i]);
            }
        }

        return map;
    }

    private static int CompareCards(CardData a, CardData b)
    {
        int typeCompare = a.DeckType.CompareTo(b.DeckType);
        if (typeCompare != 0)
        {
            return typeCompare;
        }

        return string.Compare(a.CardId, b.CardId, StringComparison.Ordinal);
    }
}
