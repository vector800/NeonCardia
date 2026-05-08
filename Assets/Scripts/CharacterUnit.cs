using UnityEngine;

public enum GridSide
{
    Player,
    Enemy
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
    public bool IsDefeated { get { return Hp <= 0; } }

    public void MoveTo(BattleGridPosition position)
    {
        Position = position;
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

    public void Heal(int amount)
    {
        Hp = Mathf.Min(MaxHp, Hp + Mathf.Max(0, amount));
    }
}
