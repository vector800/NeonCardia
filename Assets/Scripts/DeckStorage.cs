using System.Collections.Generic;
using UnityEngine;

public static class DeckStorage
{
    private const string SavedDeckKey = "NeonCardia.SavedDeck.CardIds";

    public static bool HasSavedDeck()
    {
        return PlayerPrefs.HasKey(SavedDeckKey);
    }

    public static void SaveDeck(List<CardData> deck)
    {
        List<string> ids = new List<string>();
        for (int i = 0; i < deck.Count; i++)
        {
            if (deck[i] != null)
            {
                ids.Add(deck[i].CardId);
            }
        }

        PlayerPrefs.SetString(SavedDeckKey, string.Join(",", ids.ToArray()));
        PlayerPrefs.Save();
    }

    public static bool TryLoadDeck(out List<CardData> deck)
    {
        deck = new List<CardData>();
        if (!HasSavedDeck())
        {
            return false;
        }

        Dictionary<string, CardData> cardMap = PlayerCardCollection.CreateCardMap();
        string raw = PlayerPrefs.GetString(SavedDeckKey, string.Empty);
        string[] ids = raw.Split(',');
        for (int i = 0; i < ids.Length; i++)
        {
            string id = ids[i].Trim();
            if (string.IsNullOrEmpty(id))
            {
                continue;
            }

            CardData card;
            if (cardMap.TryGetValue(id, out card))
            {
                deck.Add(card);
            }
        }

        return deck.Count > 0;
    }

    public static void ClearSavedDeck()
    {
        PlayerPrefs.DeleteKey(SavedDeckKey);
        PlayerPrefs.Save();
    }
}
