using UnityEngine;

public enum ActionTimelineEntryType
{
    Ally,
    Enemy,
    Skill,
    Weapon,
    Support
}

public sealed class BattleTimelineEntry
{
    public string UnitId;
    public string UnitName;
    public ActionTimelineEntryType EntryType;
    public bool IsAlly;
    public bool IsEnemy;
    public bool IsSkill;
    public bool IsWeapon;
    public int NextActTick;
    public int Speed;
    public int Delay;
    public bool IsAlive;
    public bool IsActive;
    public Color DisplayColor;
    public string CurrentState;
    public object OwnerUnit;
    public object ActionData;
}
