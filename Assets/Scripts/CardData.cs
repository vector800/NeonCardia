using System.Collections.Generic;

public enum CardEffectType
{
    Damage,
    FrontOnlyDamage,
    Guard,
    MoveForward,
    MoveBack,
    Repair
}

public sealed class CardData
{
    public CardData(string name, string rulesText, CardEffectType effect, int power)
    {
        Name = name;
        RulesText = rulesText;
        Effect = effect;
        Power = power;
    }

    public string Name { get; private set; }
    public string RulesText { get; private set; }
    public CardEffectType Effect { get; private set; }
    public int Power { get; private set; }

    public static List<CardData> CreateStarterDeck()
    {
        return new List<CardData>
        {
            new CardData("Strike", "Deal 6 damage.", CardEffectType.Damage, 6),
            new CardData("Heavy Shot", "Front only. Deal 14 damage.", CardEffectType.FrontOnlyDamage, 14),
            new CardData("Guard", "Reduce the next damage by 8.", CardEffectType.Guard, 8),
            new CardData("Step Forward", "Move one position forward.", CardEffectType.MoveForward, 1),
            new CardData("Step Back", "Move one position back.", CardEffectType.MoveBack, 1),
            new CardData("Repair", "Recover 7 HP.", CardEffectType.Repair, 7),
            new CardData("Strike", "Deal 6 damage.", CardEffectType.Damage, 6),
            new CardData("Guard", "Reduce the next damage by 8.", CardEffectType.Guard, 8),
            new CardData("Strike", "Deal 6 damage.", CardEffectType.Damage, 6),
            new CardData("Repair", "Recover 7 HP.", CardEffectType.Repair, 7)
        };
    }
}

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

public sealed class DeckRuntime
{
    private readonly List<CardInstance> drawPile = new List<CardInstance>();
    private readonly List<CardInstance> discardPile = new List<CardInstance>();
    private readonly System.Random random = new System.Random();

    public DeckRuntime(List<CardData> starterCards)
    {
        for (int i = 0; i < starterCards.Count; i++)
        {
            drawPile.Add(new CardInstance(starterCards[i], i));
        }
    }

    public List<CardInstance> Hand { get; private set; } = new List<CardInstance>();

    public void DrawUpTo(int handSize)
    {
        while (Hand.Count < handSize)
        {
            if (drawPile.Count == 0)
            {
                RecycleDiscardPile();
            }

            if (drawPile.Count == 0)
            {
                return;
            }

            CardInstance nextCard = drawPile[0];
            drawPile.RemoveAt(0);
            Hand.Add(nextCard);
        }
    }

    public void Discard(CardInstance card)
    {
        discardPile.Add(card);
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
            int swapIndex = random.Next(i + 1);
            CardInstance temp = cards[i];
            cards[i] = cards[swapIndex];
            cards[swapIndex] = temp;
        }
    }
}
