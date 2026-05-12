public static class BattleText
{
    public const string PlayerName = "プレイヤー";
    public const string EnemyName = "エネミー";
    public const string PlayerTurn = "プレイヤーターン";
    public const string Victory = "勝利";
    public const string Defeat = "敗北";
    public const string EndTurn = "ターン終了";
    public const string PlayerPanel = "プレイヤーパネル";
    public const string EnemyPanel = "エネミーパネル";
    public const string Guard = "ガード";
    public const string Empty = "空き";
    public const string HoverPreview = "カードにカーソルを合わせると範囲を確認できます";

    public static string FormatPosition(BattleGridPosition position)
    {
        string side = position.Side == GridSide.Player ? "プレイヤー側" : "エネミー側";
        return side + " 行" + (position.Row + 1) + " 列" + (position.Column + 1);
    }

    public static string FormatMoveDirection(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Forward:
                return "前進";
            case MoveDirection.Back:
                return "後退";
            case MoveDirection.Up:
                return "上移動";
            case MoveDirection.Down:
                return "下移動";
            default:
                return "移動なし";
        }
    }

    public static string FormatAttribute(CardAttribute attribute)
    {
        switch (attribute)
        {
            case CardAttribute.Slash:
                return "斬撃";
            case CardAttribute.Shot:
                return "射撃";
            case CardAttribute.Fire:
                return "火";
            case CardAttribute.Ice:
                return "氷";
            case CardAttribute.Electric:
                return "電撃";
            case CardAttribute.Water:
                return "水";
            case CardAttribute.Grass:
                return "草";
            case CardAttribute.Break:
                return "ブレイク";
            default:
                return "無属性";
        }
    }

    public static string FormatDeckType(CardDeckType deckType)
    {
        switch (deckType)
        {
            case CardDeckType.HC:
                return "HC";
            case CardDeckType.G:
                return "G";
            default:
                return "N";
        }
    }

    public static string FormatCardTags(CardData card)
    {
        if (card == null)
        {
            return string.Empty;
        }

        return "[" + FormatDeckType(card.DeckType) + "]" + (card.IsClearCard ? "[CLEAR]" : string.Empty);
    }

    public static string DescribeRange(CardData card)
    {
        if (card.Effect == CardEffectType.Move)
        {
            return FormatMoveDirection(card.MoveDirection) + " 1マス";
        }

        switch (card.TargetPattern)
        {
            case CardTargetPattern.SameRowNearestEnemy:
                return "同じ行の一番近い敵";
            case CardTargetPattern.ForwardOnePanel:
                return "前方1マス";
            case CardTargetPattern.Row:
                return "同じ行";
            case CardTargetPattern.SingleTarget:
                return "単体";
            case CardTargetPattern.AroundSelf:
                return "自分の周囲1マス";
            default:
                return "自分";
        }
    }
}
