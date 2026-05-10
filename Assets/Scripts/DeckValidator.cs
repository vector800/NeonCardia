using System.Collections.Generic;

public static class DeckValidator
{
    public const int RequiredDeckCount = 30;
    public const int MaxNormalCopies = 4;
    public const int MaxHighClassCount = 5;
    public const int MaxGigantCount = 1;

    public static DeckValidationResult Validate(List<CardData> deck)
    {
        DeckValidationResult result = Count(deck);
        Dictionary<string, int> normalCounts = new Dictionary<string, int>();
        HashSet<string> highClassIds = new HashSet<string>();

        for (int i = 0; i < deck.Count; i++)
        {
            CardData card = deck[i];
            if (card == null)
            {
                result.Errors.Add("不明なカードが含まれています");
                continue;
            }

            if (card.Effect == CardEffectType.Move)
            {
                result.Errors.Add("移動カードはデッキに入れられません");
            }

            if (card.DeckType == CardDeckType.N)
            {
                string key = card.CardId;
                if (!normalCounts.ContainsKey(key))
                {
                    normalCounts[key] = 0;
                }

                normalCounts[key]++;
                if (normalCounts[key] > MaxNormalCopies)
                {
                    result.Errors.Add("Nカードは同名4枚までです");
                }
            }
            else if (card.DeckType == CardDeckType.HC)
            {
                if (highClassIds.Contains(card.CardId))
                {
                    result.Errors.Add("HCカードは同名カードを複数入れられません");
                }
                else
                {
                    highClassIds.Add(card.CardId);
                }
            }
        }

        if (result.TotalCount != RequiredDeckCount)
        {
            result.Errors.Add("デッキ枚数が30枚ではありません");
        }

        if (result.HighClassCount > MaxHighClassCount)
        {
            result.Errors.Add("HCカードが上限を超えています");
        }

        if (result.GigantCount > MaxGigantCount)
        {
            result.Errors.Add("Gカードが上限を超えています");
        }

        return result;
    }

    public static bool CanAddCard(List<CardData> deck, CardData card, out string message)
    {
        if (card == null)
        {
            message = "カードを追加できません";
            return false;
        }

        if (card.Effect == CardEffectType.Move)
        {
            message = "移動カードはデッキに入れられません";
            return false;
        }

        if (deck.Count >= RequiredDeckCount)
        {
            message = "デッキは30枚までです";
            return false;
        }

        DeckValidationResult count = Count(deck);
        int sameNameCount = CountCard(deck, card.CardId);

        if (card.DeckType == CardDeckType.N && sameNameCount >= MaxNormalCopies)
        {
            message = "Nカードは同名4枚までです";
            return false;
        }

        if (card.DeckType == CardDeckType.HC)
        {
            if (count.HighClassCount >= MaxHighClassCount)
            {
                message = "HCカードは5枚までです";
                return false;
            }

            if (sameNameCount > 0)
            {
                message = "HCカードは同名カードを複数入れられません";
                return false;
            }
        }

        if (card.DeckType == CardDeckType.G && count.GigantCount >= MaxGigantCount)
        {
            message = "Gカードは1枚までです";
            return false;
        }

        message = "カードを追加しました";
        return true;
    }

    public static DeckValidationResult Count(List<CardData> deck)
    {
        DeckValidationResult result = new DeckValidationResult();
        result.TotalCount = deck.Count;

        for (int i = 0; i < deck.Count; i++)
        {
            CardData card = deck[i];
            if (card == null)
            {
                continue;
            }

            switch (card.DeckType)
            {
                case CardDeckType.HC:
                    result.HighClassCount++;
                    break;
                case CardDeckType.G:
                    result.GigantCount++;
                    break;
                default:
                    result.NormalCount++;
                    break;
            }
        }

        return result;
    }

    private static int CountCard(List<CardData> deck, string cardId)
    {
        int count = 0;
        for (int i = 0; i < deck.Count; i++)
        {
            if (deck[i] != null && deck[i].CardId == cardId)
            {
                count++;
            }
        }

        return count;
    }
}
