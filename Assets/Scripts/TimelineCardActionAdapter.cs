using System;
using UnityEngine;

public enum TimelineCardEffectKind
{
    SingleDamage,
    RowDamage,
    PushDamage,
    DelayDamage,
    Heal,
    Unsupported
}

public enum TimelineCardTargetKind
{
    Enemy,
    Ally,
    None
}

public sealed class TimelineCardAction
{
    public CardData SourceCard;
    public string CardId;
    public string DisplayName;
    public TimelineCardEffectKind EffectKind;
    public TimelineCardTargetKind TargetKind;
    public int Power;
    public bool IsClearCard;
    public CardDeckType DeckType;
    public CardAttribute Attribute;
    public int ActionDelay;
    public bool IsUnsupported;
    public string UnsupportedReason;
}

public static class TimelineCardActionAdapter
{
    public static TimelineCardAction Resolve(CardData card)
    {
        TimelineCardAction action = new TimelineCardAction();
        if (card == null)
        {
            action.CardId = "NullCard";
            action.DisplayName = "Null Card";
            action.EffectKind = TimelineCardEffectKind.Unsupported;
            action.TargetKind = TimelineCardTargetKind.None;
            action.Power = 0;
            action.DeckType = CardDeckType.N;
            action.Attribute = CardAttribute.Neutral;
            action.ActionDelay = BattleActionDelayResolver.Resolve(BattleActionDelayKind.Wait);
            action.IsUnsupported = true;
            action.UnsupportedReason = "CardData is null.";
            return action;
        }

        action.SourceCard = card;
        action.CardId = card.CardId;
        action.DisplayName = string.IsNullOrEmpty(card.Name) ? card.CardId : card.Name;
        action.Power = Mathf.Max(0, card.Power);
        action.IsClearCard = card.IsClearCard;
        action.DeckType = card.DeckType;
        action.Attribute = card.Attribute;

        switch (card.Effect)
        {
            case CardEffectType.Damage:
                ResolveDamageCard(card, action);
                break;
            case CardEffectType.Repair:
                action.EffectKind = TimelineCardEffectKind.Heal;
                action.TargetKind = TimelineCardTargetKind.Ally;
                break;
            case CardEffectType.Freeze:
                action.EffectKind = TimelineCardEffectKind.DelayDamage;
                action.TargetKind = TimelineCardTargetKind.Enemy;
                action.UnsupportedReason = "Freeze is mapped to timeline delay for the MVP.";
                break;
            case CardEffectType.Move:
                action.EffectKind = TimelineCardEffectKind.PushDamage;
                action.TargetKind = TimelineCardTargetKind.Enemy;
                action.UnsupportedReason = "Move is mapped to a one-cell enemy push for the timeline BattleScene MVP.";
                break;
            default:
                action.EffectKind = TimelineCardEffectKind.Unsupported;
                action.TargetKind = ResolveTargetKind(card.TargetType);
                action.IsUnsupported = true;
                action.UnsupportedReason = card.Effect + " is not implemented in the timeline battle adapter.";
                break;
        }

        action.ActionDelay = BattleActionDelayResolver.ResolveCardDelay(action);
        return action;
    }

    private static void ResolveDamageCard(CardData card, TimelineCardAction action)
    {
        action.TargetKind = TimelineCardTargetKind.Enemy;

        if (LooksLikePush(card))
        {
            action.EffectKind = TimelineCardEffectKind.PushDamage;
            return;
        }

        if (LooksLikeDelay(card))
        {
            action.EffectKind = TimelineCardEffectKind.DelayDamage;
            return;
        }

        switch (card.TargetPattern)
        {
            case CardTargetPattern.Row:
            case CardTargetPattern.ForwardLine3:
                action.EffectKind = TimelineCardEffectKind.RowDamage;
                break;
            default:
                action.EffectKind = TimelineCardEffectKind.SingleDamage;
                break;
        }
    }

    private static TimelineCardTargetKind ResolveTargetKind(CardTargetType targetType)
    {
        switch (targetType)
        {
            case CardTargetType.Enemy:
                return TimelineCardTargetKind.Enemy;
            case CardTargetType.Self:
                return TimelineCardTargetKind.Ally;
            default:
                return TimelineCardTargetKind.None;
        }
    }

    private static bool LooksLikePush(CardData card)
    {
        return ContainsToken(card, "Push") || ContainsToken(card, "押");
    }

    private static bool LooksLikeDelay(CardData card)
    {
        return ContainsToken(card, "Delay") || ContainsToken(card, "ディレイ");
    }

    private static bool ContainsToken(CardData card, string token)
    {
        if (card == null || string.IsNullOrEmpty(token))
        {
            return false;
        }

        return Contains(card.CardId, token) || Contains(card.Name, token) || Contains(card.RulesText, token);
    }

    private static bool Contains(string value, string token)
    {
        return !string.IsNullOrEmpty(value) && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
