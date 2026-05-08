using UnityEngine;

public enum BattlePosition
{
    Back = 0,
    Middle = 1,
    Front = 2
}

public sealed class CharacterUnit
{
    public CharacterUnit(string name, int maxHp)
    {
        Name = name;
        MaxHp = maxHp;
        Hp = maxHp;
    }

    public string Name { get; private set; }
    public int MaxHp { get; private set; }
    public int Hp { get; private set; }
    public int Guard { get; set; }
    public bool IsDefeated { get { return Hp <= 0; } }

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

    public static string FormatPosition(BattlePosition position)
    {
        switch (position)
        {
            case BattlePosition.Back:
                return "Back";
            case BattlePosition.Middle:
                return "Middle";
            case BattlePosition.Front:
                return "Front";
            default:
                return "Unknown";
        }
    }
}
