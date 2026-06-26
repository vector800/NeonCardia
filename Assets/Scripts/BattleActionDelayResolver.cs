public enum BattleActionDelayKind
{
    Wait,
    Weapon,
    NormalCard,
    ClearCard,
    HighCapacityCard,
    GigaCard,
    HealCard,
    Swap,
    EnemyAttack,
    BossAction
}

public static class BattleActionDelayResolver
{
    public const int WeaponDelay = 80;
    public const int NormalCardDelay = 100;
    public const int ClearCardDelay = 60;
    public const int HighCapacityCardDelay = 130;
    public const int GigaCardDelay = 240;
    public const int HealCardDelay = 90;
    public const int SwapDelay = 70;
    public const int WaitDelay = 60;
    public const int EnemyAttackDelay = 100;
    public const int BossActionDelay = 130;

    public static int Resolve(BattleActionDelayKind kind)
    {
        switch (kind)
        {
            case BattleActionDelayKind.Weapon:
                return WeaponDelay;
            case BattleActionDelayKind.ClearCard:
                return ClearCardDelay;
            case BattleActionDelayKind.HighCapacityCard:
                return HighCapacityCardDelay;
            case BattleActionDelayKind.GigaCard:
                return GigaCardDelay;
            case BattleActionDelayKind.HealCard:
                return HealCardDelay;
            case BattleActionDelayKind.Swap:
                return SwapDelay;
            case BattleActionDelayKind.EnemyAttack:
                return EnemyAttackDelay;
            case BattleActionDelayKind.BossAction:
                return BossActionDelay;
            case BattleActionDelayKind.Wait:
                return WaitDelay;
            default:
                return NormalCardDelay;
        }
    }

    public static int ResolveCardDelay(TimelineCardAction action)
    {
        if (action == null)
        {
            return Resolve(BattleActionDelayKind.Wait);
        }

        return ResolveCardDelay(action.EffectKind, action.IsClearCard, action.DeckType);
    }

    public static int ResolveCardDelay(TimelineCardEffectKind effectKind, bool isClearCard, CardDeckType deckType)
    {
        if (effectKind == TimelineCardEffectKind.Heal)
        {
            return Resolve(BattleActionDelayKind.HealCard);
        }

        if (isClearCard)
        {
            return Resolve(BattleActionDelayKind.ClearCard);
        }

        switch (deckType)
        {
            case CardDeckType.HC:
                return Resolve(BattleActionDelayKind.HighCapacityCard);
            case CardDeckType.G:
                return Resolve(BattleActionDelayKind.GigaCard);
            default:
                return Resolve(BattleActionDelayKind.NormalCard);
        }
    }

    public static int ResolveEnemyActionDelay(bool isBoss)
    {
        return Resolve(isBoss ? BattleActionDelayKind.BossAction : BattleActionDelayKind.EnemyAttack);
    }
}
