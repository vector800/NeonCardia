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
