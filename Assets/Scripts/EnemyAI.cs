using System.Collections.Generic;

public enum EnemyType
{
    SmallEnemy = 0,
    NormalEnemy = 1,
    HeavyEnemy = 2,
    Stage1Boss = 3,
    FireEnemy = 4,
    GrassEnemy = 5,
    IceEnemy = 6,
    FloatingEnemy = 7
}

public enum EnemyCategory
{
    Normal,
    Boss
}

public enum EnemyActionKind
{
    Attack,
    Move,
    Guard
}

public enum EnemyAttackPattern
{
    SameRowNearest,
    ForwardOnePanel,
    Row,
    Strong
}

public sealed class EnemyBattleAction
{
    private EnemyBattleAction(EnemyActionKind kind)
    {
        Kind = kind;
    }

    public EnemyActionKind Kind { get; private set; }
    public BattleGridPosition Destination { get; private set; }
    public int GuardAmount { get; private set; }
    public int Damage { get; private set; }
    public string ActionText { get; private set; }
    public EnemyAttackPattern AttackPattern { get; private set; }

    public static EnemyBattleAction Move(BattleGridPosition destination, string actionText)
    {
        return new EnemyBattleAction(EnemyActionKind.Move)
        {
            Destination = destination,
            ActionText = actionText
        };
    }

    public static EnemyBattleAction Guard(int guardAmount, string actionText)
    {
        return new EnemyBattleAction(EnemyActionKind.Guard)
        {
            GuardAmount = guardAmount,
            ActionText = actionText
        };
    }

    public static EnemyBattleAction Attack(int damage, EnemyAttackPattern attackPattern, string actionText)
    {
        return new EnemyBattleAction(EnemyActionKind.Attack)
        {
            Damage = damage,
            AttackPattern = attackPattern,
            ActionText = actionText
        };
    }
}

public sealed class EnemyAI
{
    private readonly EnemyType enemyType;
    private int turnCount;
    private static readonly EnemyType[] DebugEnemyTypes =
    {
        EnemyType.NormalEnemy,
        EnemyType.FireEnemy,
        EnemyType.GrassEnemy,
        EnemyType.IceEnemy,
        EnemyType.HeavyEnemy,
        EnemyType.FloatingEnemy,
        EnemyType.Stage1Boss
    };

    public EnemyAI(EnemyType enemyType)
    {
        this.enemyType = enemyType;
    }

    public void BeginTurn()
    {
        turnCount++;
    }

    public static int GetMaxHp(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.SmallEnemy:
                return 40;
            case EnemyType.NormalEnemy:
                return 70;
            case EnemyType.FireEnemy:
                return 90;
            case EnemyType.GrassEnemy:
            case EnemyType.IceEnemy:
                return 80;
            case EnemyType.HeavyEnemy:
                return 150;
            case EnemyType.FloatingEnemy:
                return 70;
            case EnemyType.Stage1Boss:
                return 300;
            default:
                return 70;
        }
    }

    public static int GetAttackPower(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.SmallEnemy:
                return 15;
            case EnemyType.NormalEnemy:
                return 20;
            case EnemyType.FireEnemy:
                return 25;
            case EnemyType.GrassEnemy:
            case EnemyType.IceEnemy:
                return 20;
            case EnemyType.HeavyEnemy:
                return 30;
            case EnemyType.FloatingEnemy:
                return 18;
            case EnemyType.Stage1Boss:
                return 35;
            default:
                return 20;
        }
    }

    public static int GetActionCount(EnemyType enemyType)
    {
        return enemyType == EnemyType.Stage1Boss ? 2 : 1;
    }

    public static EnemyCategory GetCategory(EnemyType enemyType)
    {
        return enemyType == EnemyType.Stage1Boss ? EnemyCategory.Boss : EnemyCategory.Normal;
    }

    public static bool IsBoss(EnemyType enemyType)
    {
        return GetCategory(enemyType) == EnemyCategory.Boss;
    }

    public static CardAttribute GetWeakness(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.SmallEnemy:
                return CardAttribute.Slash;
            case EnemyType.NormalEnemy:
                return CardAttribute.Shot;
            case EnemyType.FireEnemy:
                return CardAttribute.Water;
            case EnemyType.GrassEnemy:
            case EnemyType.IceEnemy:
                return CardAttribute.Fire;
            case EnemyType.HeavyEnemy:
                return CardAttribute.Break;
            case EnemyType.FloatingEnemy:
                return CardAttribute.Electric;
            case EnemyType.Stage1Boss:
                return CardAttribute.Shot;
            default:
                return CardAttribute.Shot;
        }
    }

    public static string GetAttackRangeText(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.HeavyEnemy:
                return "前方1マス";
            case EnemyType.Stage1Boss:
                return "同じ行 / 横一列 / 強攻撃";
            default:
                return "同じ行の一番近い相手";
        }
    }

    public static string GetPlanText(EnemyType enemyType)
    {
        return "敵行動回数：" + GetActionCount(enemyType)
            + "\n攻撃範囲：" + GetAttackRangeText(enemyType)
            + "\n弱点：" + BattleText.FormatAttribute(GetWeakness(enemyType));
    }

    public static string GetDisplayName(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.SmallEnemy:
                return "SmallEnemy";
            case EnemyType.NormalEnemy:
                return "NormalEnemy";
            case EnemyType.FireEnemy:
                return "FireEnemy";
            case EnemyType.GrassEnemy:
                return "GrassEnemy";
            case EnemyType.IceEnemy:
                return "IceEnemy";
            case EnemyType.HeavyEnemy:
                return "HeavyEnemy";
            case EnemyType.FloatingEnemy:
                return "FloatingEnemy";
            case EnemyType.Stage1Boss:
                return "Stage1Boss";
            default:
                return "NormalEnemy";
        }
    }

    public static UnitElement GetElement(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.FireEnemy:
                return UnitElement.Fire;
            case EnemyType.GrassEnemy:
                return UnitElement.Grass;
            case EnemyType.IceEnemy:
                return UnitElement.Ice;
            default:
                return UnitElement.Neutral;
        }
    }

    public static bool HasFloatAbility(EnemyType enemyType)
    {
        return enemyType == EnemyType.FloatingEnemy;
    }

    public static EnemyType[] GetDebugEnemyTypes()
    {
        EnemyType[] result = new EnemyType[DebugEnemyTypes.Length];
        DebugEnemyTypes.CopyTo(result, 0);
        return result;
    }

    public static string GetDebugSummary(EnemyType enemyType)
    {
        return "HP:" + GetMaxHp(enemyType)
            + "  Element:" + GetElement(enemyType)
            + "  Weak:" + BattleText.FormatAttribute(GetWeakness(enemyType))
            + "  Actions:" + GetActionCount(enemyType)
            + (HasFloatAbility(enemyType) ? "  Float" : string.Empty)
            + (IsBoss(enemyType) ? "  Boss" : string.Empty);
    }

    public EnemyBattleAction CreateNextAction(CharacterUnit player, CharacterUnit enemy, IEnumerable<CharacterUnit> units, int actionIndex)
    {
        switch (enemyType)
        {
            case EnemyType.SmallEnemy:
                return CreateSmallAction(player, enemy, units);
            case EnemyType.NormalEnemy:
            case EnemyType.FireEnemy:
            case EnemyType.GrassEnemy:
            case EnemyType.IceEnemy:
            case EnemyType.FloatingEnemy:
                return CreateNormalAction(player, enemy, units);
            case EnemyType.HeavyEnemy:
                return CreateHeavyAction();
            case EnemyType.Stage1Boss:
                return CreateStage1BossAction(player, enemy, units, actionIndex);
            default:
                return CreateNormalAction(player, enemy, units);
        }
    }

    public static string FormatAttackPattern(EnemyAttackPattern attackPattern)
    {
        switch (attackPattern)
        {
            case EnemyAttackPattern.ForwardOnePanel:
                return "前方1マス";
            case EnemyAttackPattern.Row:
                return "横一列";
            case EnemyAttackPattern.Strong:
                return "同じ行の強攻撃";
            default:
                return "同じ行の一番近い相手";
        }
    }

    private EnemyBattleAction CreateSmallAction(CharacterUnit player, CharacterUnit enemy, IEnumerable<CharacterUnit> units)
    {
        if (enemy.Position.Row == player.Position.Row)
        {
            return EnemyBattleAction.Attack(GetAttackPower(enemyType), EnemyAttackPattern.SameRowNearest, "エネミーはクローを使用。");
        }

        BattleGridPosition nextPosition = enemy.Position.Offset(enemy.Position.Row < player.Position.Row ? 1 : -1, 0);
        if (CanMove(enemy, nextPosition, units))
        {
            return EnemyBattleAction.Move(nextPosition, enemy.Name + "は" + nextPosition + "へ移動しました。");
        }

        return EnemyBattleAction.Attack(GetAttackPower(enemyType), EnemyAttackPattern.SameRowNearest, "エネミーはクローを使用。");
    }

    private EnemyBattleAction CreateNormalAction(CharacterUnit player, CharacterUnit enemy, IEnumerable<CharacterUnit> units)
    {
        if (enemy.Hp <= 25)
        {
            BattleGridPosition retreat = enemy.Position.Offset(0, BattleGridPosition.BackColumnDelta(enemy.Position.Side));
            if (CanMove(enemy, retreat, units))
            {
                return EnemyBattleAction.Move(retreat, enemy.Name + "は" + retreat + "へ移動しました。");
            }
        }

        if (enemy.Position.Row == player.Position.Row)
        {
            return EnemyBattleAction.Attack(GetAttackPower(enemyType), EnemyAttackPattern.SameRowNearest, "エネミーはショットを使用。");
        }

        BattleGridPosition nextPosition = enemy.Position.Offset(enemy.Position.Row < player.Position.Row ? 1 : -1, 0);
        if (CanMove(enemy, nextPosition, units))
        {
            return EnemyBattleAction.Move(nextPosition, enemy.Name + "は" + nextPosition + "へ移動しました。");
        }

        return EnemyBattleAction.Attack(GetAttackPower(enemyType), EnemyAttackPattern.SameRowNearest, "エネミーはショットを使用。");
    }

    private EnemyBattleAction CreateHeavyAction()
    {
        if (turnCount % 2 == 0)
        {
            return EnemyBattleAction.Attack(GetAttackPower(enemyType), EnemyAttackPattern.ForwardOnePanel, "エネミーはヘビーショットを使用。");
        }

        return EnemyBattleAction.Guard(20, "エネミーは身構えた。");
    }

    private EnemyBattleAction CreateStage1BossAction(CharacterUnit player, CharacterUnit enemy, IEnumerable<CharacterUnit> units, int actionIndex)
    {
        int pattern = (turnCount + actionIndex - 1) % 3;
        if (pattern == 1)
        {
            return EnemyBattleAction.Attack(GetAttackPower(enemyType), EnemyAttackPattern.Row, "エネミーは横一列攻撃を使用。");
        }

        if (pattern == 2)
        {
            return EnemyBattleAction.Attack(GetAttackPower(enemyType) + 15, EnemyAttackPattern.Strong, "エネミーは強攻撃を使用。");
        }

        if (enemy.Position.Row == player.Position.Row)
        {
            return EnemyBattleAction.Attack(GetAttackPower(enemyType), EnemyAttackPattern.SameRowNearest, "エネミーはボスショットを使用。");
        }

        BattleGridPosition nextPosition = enemy.Position.Offset(enemy.Position.Row < player.Position.Row ? 1 : -1, 0);
        if (CanMove(enemy, nextPosition, units))
        {
            return EnemyBattleAction.Move(nextPosition, enemy.Name + "は" + nextPosition + "へ移動しました。");
        }

        return EnemyBattleAction.Attack(GetAttackPower(enemyType), EnemyAttackPattern.SameRowNearest, "エネミーはボスショットを使用。");
    }

    private static bool CanMove(CharacterUnit unit, BattleGridPosition destination, IEnumerable<CharacterUnit> units)
    {
        if (!destination.IsValid)
        {
            return false;
        }

        foreach (CharacterUnit other in units)
        {
            if (other != unit && other.Position.Side == destination.Side && other.Position.Row == destination.Row && other.Position.Column == destination.Column)
            {
                return false;
            }
        }

        return true;
    }
}
