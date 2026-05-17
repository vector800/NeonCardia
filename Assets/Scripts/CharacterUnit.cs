using UnityEngine;

public enum GridSide
{
    Player,
    Enemy
}

public enum PanelType
{
    Normal,
    Cracked,
    Hole,
    Ice,
    Grass,
    Magma,
    Poison
}

public enum UnitElement
{
    Neutral,
    Fire,
    Grass,
    Ice
}

public enum AttackTravelType
{
    Ground,
    Air
}

public struct BattleGridPosition
{
    public const int GridSize = 3;

    public BattleGridPosition(GridSide side, int row, int column)
    {
        Side = side;
        Row = row;
        Column = column;
    }

    public GridSide Side { get; private set; }
    public int Row { get; private set; }
    public int Column { get; private set; }

    public bool IsValid
    {
        get
        {
            return Row >= 0 && Row < GridSize && Column >= 0 && Column < GridSize;
        }
    }

    public BattleGridPosition Offset(int rowDelta, int columnDelta)
    {
        return new BattleGridPosition(Side, Row + rowDelta, Column + columnDelta);
    }

    public static int ForwardColumnDelta(GridSide side)
    {
        return side == GridSide.Player ? 1 : -1;
    }

    public static int BackColumnDelta(GridSide side)
    {
        return side == GridSide.Player ? -1 : 1;
    }

    public override string ToString()
    {
        return BattleText.FormatPosition(this);
    }
}

public sealed class CharacterUnit
{
    public const int MaxGuard = 80;

    public CharacterUnit(string name, int maxHp, BattleGridPosition position)
    {
        Name = name;
        MaxHp = maxHp;
        Hp = maxHp;
        Position = position;
    }

    public string Name { get; private set; }
    public int MaxHp { get; private set; }
    public int Hp { get; private set; }
    public int Guard { get; set; }
    public BattleGridPosition Position { get; private set; }
    public UnitElement Element { get; set; }
    public bool HasFloatAbility { get; set; }
    public bool IsFrozen { get; private set; }
    public int FrozenTurnCount { get; private set; }
    public bool IsDefeated { get { return Hp <= 0; } }

    public void MoveTo(BattleGridPosition position)
    {
        Position = position;
    }

    public void AddGuard(int amount)
    {
        Guard = Mathf.Min(MaxGuard, Guard + Mathf.Max(0, amount));
    }

    public int TakeDamage(int damage, out int blocked)
    {
        int incoming = Mathf.Max(0, damage);
        blocked = Mathf.Min(Guard, incoming);
        Guard -= blocked;
        int actualDamage = incoming - blocked;
        Hp = Mathf.Max(0, Hp - actualDamage);
        return actualDamage;
    }

    public int TakeDirectDamage(int damage)
    {
        int incoming = Mathf.Max(0, damage);
        int before = Hp;
        Hp = Mathf.Max(0, Hp - incoming);
        return before - Hp;
    }

    public void Heal(int amount)
    {
        Hp = Mathf.Min(MaxHp, Hp + Mathf.Max(0, amount));
    }

    public bool ApplyFrozen()
    {
        if (Element == UnitElement.Fire)
        {
            return false;
        }

        IsFrozen = true;
        FrozenTurnCount = 0;
        return true;
    }

    public void ClearFrozen()
    {
        IsFrozen = false;
        FrozenTurnCount = 0;
    }

    public bool ConsumeFrozenTurn(out bool released)
    {
        released = false;
        if (!IsFrozen)
        {
            return false;
        }

        FrozenTurnCount++;
        if (FrozenTurnCount >= 2)
        {
            ClearFrozen();
            released = true;
        }

        return true;
    }
}
