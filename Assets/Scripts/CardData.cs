using System.Collections.Generic;
using UnityEngine;

public enum CardEffectType
{
    Damage,
    Guard,
    Move,
    Repair,
    Charge
}

public enum CardDeckType
{
    N,
    HC,
    G
}

public enum CardTargetPattern
{
    None,
    SameRowNearestEnemy,
    ForwardOnePanel,
    Row,
    SingleTarget,
    AroundSelf
}

public enum CardUsablePosition
{
    Any,
    PlayerPanel,
    EnemyPanel
}

public enum CardTargetType
{
    None,
    Enemy,
    Self,
    Panel
}

public enum CardAttribute
{
    Neutral,
    Slash,
    Shot,
    Fire,
    Ice,
    Electric
}

public enum MoveDirection
{
    None,
    Forward,
    Back,
    Up,
    Down
}

[CreateAssetMenu(menuName = "NeonCardia/Card Data", fileName = "NewCardData")]
public sealed class CardData : ScriptableObject
{
    [SerializeField] private string cardId;
    [SerializeField] private string cardName;
    [SerializeField] [TextArea(2, 4)] private string rulesText;
    [SerializeField] private CardDeckType deckType;
    [SerializeField] private bool isClearCard;
    [SerializeField] private CardEffectType effect;
    [SerializeField] private int power;
    [SerializeField] private int cost;
    [SerializeField] private CardAttribute attribute;
    [SerializeField] private CardUsablePosition usablePosition = CardUsablePosition.Any;
    [SerializeField] private CardTargetType targetType;
    [SerializeField] private CardTargetPattern targetPattern;
    [SerializeField] private MoveDirection moveDirection;

    public string CardId { get { return string.IsNullOrEmpty(cardId) ? name : cardId; } }
    public string Name { get { return cardName; } }
    public string RulesText { get { return rulesText; } }
    public CardDeckType DeckType { get { return deckType; } }
    public bool IsClearCard { get { return isClearCard; } }
    public CardEffectType Effect { get { return effect; } }
    public int Power { get { return power; } }
    public int Cost { get { return cost; } }
    public CardAttribute Attribute { get { return attribute; } }
    public CardUsablePosition UsablePosition { get { return usablePosition; } }
    public CardTargetType TargetType { get { return targetType; } }
    public CardTargetPattern TargetPattern { get { return targetPattern; } }
    public MoveDirection MoveDirection { get { return moveDirection; } }

    private void Initialize(string newCardId, string newName, string newRulesText, CardDeckType newDeckType, bool newIsClearCard, CardEffectType newEffect, int newPower, int newCost, CardAttribute newAttribute, CardUsablePosition newUsablePosition, CardTargetType newTargetType, CardTargetPattern newTargetPattern, MoveDirection newMoveDirection)
    {
        cardId = newCardId;
        cardName = newName;
        rulesText = newRulesText;
        deckType = newDeckType;
        isClearCard = newIsClearCard;
        effect = newEffect;
        power = newPower;
        cost = newCost;
        attribute = newAttribute;
        usablePosition = newUsablePosition;
        targetType = newTargetType;
        targetPattern = newTargetPattern;
        moveDirection = newMoveDirection;
    }

    public static List<CardData> CreateStarterDeck()
    {
        List<CardData> cards = new List<CardData>();
        AddCopies(cards, LoadOrCreate("Strike", "ストライク", "同じ行の一番近い敵に35ダメージを与える。", CardDeckType.N, false, CardEffectType.Damage, 35, 1, CardAttribute.Slash, CardUsablePosition.Any, CardTargetType.Enemy, CardTargetPattern.SameRowNearestEnemy, MoveDirection.None), 4);
        AddCopies(cards, LoadOrCreate("Guard", "ガード", "次に受けるダメージを40軽減する。", CardDeckType.N, true, CardEffectType.Guard, 40, 1, CardAttribute.Neutral, CardUsablePosition.Any, CardTargetType.Self, CardTargetPattern.None, MoveDirection.None), 4);
        AddCopies(cards, LoadOrCreate("Repair", "リペア", "HPを45回復する。", CardDeckType.N, true, CardEffectType.Repair, 45, 1, CardAttribute.Neutral, CardUsablePosition.Any, CardTargetType.Self, CardTargetPattern.None, MoveDirection.None), 4);
        AddCopies(cards, LoadOrCreate("WideShot", "ワイドショット", "横一列に25ダメージを与える。", CardDeckType.N, false, CardEffectType.Damage, 25, 1, CardAttribute.Shot, CardUsablePosition.Any, CardTargetType.Enemy, CardTargetPattern.Row, MoveDirection.None), 4);
        AddCopies(cards, LoadOrCreate("QuickShot", "クイックショット", "同じ行の一番近い敵に25ダメージを与える。", CardDeckType.N, false, CardEffectType.Damage, 25, 1, CardAttribute.Shot, CardUsablePosition.Any, CardTargetType.Enemy, CardTargetPattern.SameRowNearestEnemy, MoveDirection.None), 4);
        AddCopies(cards, LoadOrCreate("PierceShot", "ピアースショット", "同じ行の敵に30ダメージを与える。", CardDeckType.N, false, CardEffectType.Damage, 30, 1, CardAttribute.Electric, CardUsablePosition.Any, CardTargetType.Enemy, CardTargetPattern.Row, MoveDirection.None), 4);
        AddCopies(cards, LoadOrCreate("Charge", "チャージ", "次の攻撃カードのダメージを20増やす。", CardDeckType.N, true, CardEffectType.Charge, 20, 1, CardAttribute.Neutral, CardUsablePosition.Any, CardTargetType.Self, CardTargetPattern.None, MoveDirection.None), 1);
        AddCopies(cards, LoadOrCreate("HeavyShot", "ヘビーショット", "同じ行の敵に70ダメージを与える。", CardDeckType.HC, false, CardEffectType.Damage, 70, 2, CardAttribute.Shot, CardUsablePosition.Any, CardTargetType.Enemy, CardTargetPattern.SameRowNearestEnemy, MoveDirection.None), 1);
        AddCopies(cards, LoadOrCreate("HighBurst", "ハイバースト", "同じ行の敵に85ダメージを与える。", CardDeckType.HC, false, CardEffectType.Damage, 85, 2, CardAttribute.Fire, CardUsablePosition.Any, CardTargetType.Enemy, CardTargetPattern.SameRowNearestEnemy, MoveDirection.None), 1);
        AddCopies(cards, LoadOrCreate("BoostGuard", "ブーストガード", "ガードを60増やす。", CardDeckType.HC, false, CardEffectType.Guard, 60, 2, CardAttribute.Neutral, CardUsablePosition.Any, CardTargetType.Self, CardTargetPattern.None, MoveDirection.None), 1);
        AddCopies(cards, LoadOrCreate("HealField", "ヒールフィールド", "HPを65回復する。", CardDeckType.HC, false, CardEffectType.Repair, 65, 2, CardAttribute.Neutral, CardUsablePosition.Any, CardTargetType.Self, CardTargetPattern.None, MoveDirection.None), 1);
        AddCopies(cards, LoadOrCreate("RailCannon", "レールキャノン", "同じ行の敵に90ダメージを与える。", CardDeckType.HC, false, CardEffectType.Damage, 90, 2, CardAttribute.Electric, CardUsablePosition.Any, CardTargetType.Enemy, CardTargetPattern.Row, MoveDirection.None), 1);
        return cards;
    }

    private static void AddCopies(List<CardData> cards, CardData card, int count)
    {
        for (int i = 0; i < count; i++)
        {
            cards.Add(card);
        }
    }

    private static CardData LoadOrCreate(string resourceName, string name, string rulesText, CardDeckType deckType, bool isClearCard, CardEffectType effect, int power, int cost, CardAttribute attribute, CardUsablePosition usablePosition, CardTargetType targetType, CardTargetPattern targetPattern, MoveDirection moveDirection)
    {
        CardData card = Resources.Load<CardData>("Cards/" + resourceName);
        if (card != null)
        {
            return card;
        }

        CardData fallback = CreateInstance<CardData>();
        fallback.Initialize(resourceName, name, rulesText, deckType, isClearCard, effect, power, cost, attribute, usablePosition, targetType, targetPattern, moveDirection);
        return fallback;
    }
}
