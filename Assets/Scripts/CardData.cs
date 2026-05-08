using System.Collections.Generic;

public enum CardEffectType
{
    Damage,
    Guard,
    Move,
    Repair
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

public enum MoveDirection
{
    None,
    Forward,
    Back,
    Up,
    Down
}

public sealed class CardData
{
    public CardData(string name, string rulesText, CardEffectType effect, int power, CardTargetPattern targetPattern, MoveDirection moveDirection)
    {
        Name = name;
        RulesText = rulesText;
        Effect = effect;
        Power = power;
        TargetPattern = targetPattern;
        MoveDirection = moveDirection;
    }

    public string Name { get; private set; }
    public string RulesText { get; private set; }
    public CardEffectType Effect { get; private set; }
    public int Power { get; private set; }
    public CardTargetPattern TargetPattern { get; private set; }
    public MoveDirection MoveDirection { get; private set; }

    public static List<CardData> CreateStarterDeck()
    {
        return new List<CardData>
        {
            new CardData("ストライク", "同じ行の一番近い敵に6ダメージを与える。", CardEffectType.Damage, 6, CardTargetPattern.SameRowNearestEnemy, MoveDirection.None),
            new CardData("ストライク", "同じ行の一番近い敵に6ダメージを与える。", CardEffectType.Damage, 6, CardTargetPattern.SameRowNearestEnemy, MoveDirection.None),
            new CardData("ストライク", "同じ行の一番近い敵に6ダメージを与える。", CardEffectType.Damage, 6, CardTargetPattern.SameRowNearestEnemy, MoveDirection.None),
            new CardData("ストライク", "同じ行の一番近い敵に6ダメージを与える。", CardEffectType.Damage, 6, CardTargetPattern.SameRowNearestEnemy, MoveDirection.None),
            new CardData("ヘビーショット", "同じ行の敵に14ダメージを与える。", CardEffectType.Damage, 14, CardTargetPattern.SameRowNearestEnemy, MoveDirection.None),
            new CardData("ヘビーショット", "同じ行の敵に14ダメージを与える。", CardEffectType.Damage, 14, CardTargetPattern.SameRowNearestEnemy, MoveDirection.None),
            new CardData("ガード", "次に受けるダメージを8軽減する。", CardEffectType.Guard, 8, CardTargetPattern.None, MoveDirection.None),
            new CardData("ガード", "次に受けるダメージを8軽減する。", CardEffectType.Guard, 8, CardTargetPattern.None, MoveDirection.None),
            new CardData("リペア", "HPを7回復する。", CardEffectType.Repair, 7, CardTargetPattern.None, MoveDirection.None),
            new CardData("リペア", "HPを7回復する。", CardEffectType.Repair, 7, CardTargetPattern.None, MoveDirection.None),
            new CardData("前進", "敵側に近い方向へ1マス移動する。", CardEffectType.Move, 1, CardTargetPattern.None, MoveDirection.Forward),
            new CardData("前進", "敵側に近い方向へ1マス移動する。", CardEffectType.Move, 1, CardTargetPattern.None, MoveDirection.Forward),
            new CardData("後退", "自陣奥へ1マス移動する。", CardEffectType.Move, 1, CardTargetPattern.None, MoveDirection.Back),
            new CardData("後退", "自陣奥へ1マス移動する。", CardEffectType.Move, 1, CardTargetPattern.None, MoveDirection.Back),
            new CardData("上移動", "上に1マス移動する。", CardEffectType.Move, 1, CardTargetPattern.None, MoveDirection.Up),
            new CardData("下移動", "下に1マス移動する。", CardEffectType.Move, 1, CardTargetPattern.None, MoveDirection.Down)
        };
    }
}
