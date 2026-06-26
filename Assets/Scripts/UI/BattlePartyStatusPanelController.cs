using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BattlePartyStatusPanelController : MonoBehaviour
{
    [Serializable]
    public sealed class BattlePartyStatusMember
    {
        public string DisplayName;
        public Sprite FaceIcon;
        public int CurrentHp;
        public int MaxHp;
    }

    [SerializeField] private BattlePartyStatusEntryView[] activeEntries = new BattlePartyStatusEntryView[3];
    [SerializeField] private BattlePartyStatusEntryView[] reserveEntries = new BattlePartyStatusEntryView[3];
    [SerializeField] private BattlePartyStatusMember[] reserveMembers =
    {
        new BattlePartyStatusMember { DisplayName = "Reserve Ally A", CurrentHp = 110, MaxHp = 110 },
        new BattlePartyStatusMember { DisplayName = "Reserve Ally B", CurrentHp = 104, MaxHp = 104 },
        new BattlePartyStatusMember { DisplayName = "Reserve Ally C", CurrentHp = 116, MaxHp = 116 }
    };

    private void Reset()
    {
        CacheReferences();
    }

    private void Awake()
    {
        CacheReferences();
        RefreshReserveMembers();
    }

    private void OnValidate()
    {
        CacheReferences();
    }

    public void SetActiveMember(int index, string displayName, Sprite faceIcon, int currentHp, int maxHp)
    {
        BattlePartyStatusEntryView entry = GetEntry(activeEntries, index);
        if (entry == null)
        {
            return;
        }

        entry.SetStatus(displayName, faceIcon, currentHp, maxHp, false);
    }

    public void SetReserveMember(int index, string displayName, Sprite faceIcon, int currentHp, int maxHp)
    {
        if (reserveMembers == null || reserveMembers.Length < reserveEntries.Length)
        {
            Array.Resize(ref reserveMembers, reserveEntries.Length);
        }

        if (index < 0 || index >= reserveMembers.Length)
        {
            return;
        }

        if (reserveMembers[index] == null)
        {
            reserveMembers[index] = new BattlePartyStatusMember();
        }

        reserveMembers[index].DisplayName = displayName;
        reserveMembers[index].FaceIcon = faceIcon;
        reserveMembers[index].CurrentHp = currentHp;
        reserveMembers[index].MaxHp = maxHp;
        RefreshReserveMember(index);
    }

    public bool TryGetReserveMember(int index, out string displayName, out Sprite faceIcon, out int currentHp, out int maxHp)
    {
        displayName = string.Empty;
        faceIcon = null;
        currentHp = 0;
        maxHp = 0;

        BattlePartyStatusMember member = reserveMembers != null && index >= 0 && index < reserveMembers.Length
            ? reserveMembers[index]
            : null;
        if (member == null)
        {
            return false;
        }

        displayName = member.DisplayName;
        faceIcon = member.FaceIcon;
        currentHp = member.CurrentHp;
        maxHp = member.MaxHp;
        return !string.IsNullOrWhiteSpace(displayName) || faceIcon != null || currentHp > 0 || maxHp > 0;
    }

    public void RefreshReserveMembers()
    {
        for (int i = 0; i < reserveEntries.Length; i++)
        {
            RefreshReserveMember(i);
        }
    }

    public void CacheReferences()
    {
        if (activeEntries == null || activeEntries.Length != 3)
        {
            activeEntries = new BattlePartyStatusEntryView[3];
        }

        if (reserveEntries == null || reserveEntries.Length != 3)
        {
            reserveEntries = new BattlePartyStatusEntryView[3];
        }

        CacheEntryGroup("ActiveEntries", activeEntries);
        CacheEntryGroup("ReserveEntries", reserveEntries);
    }

    private void RefreshReserveMember(int index)
    {
        BattlePartyStatusEntryView entry = GetEntry(reserveEntries, index);
        if (entry == null)
        {
            return;
        }

        BattlePartyStatusMember member = reserveMembers != null && index >= 0 && index < reserveMembers.Length
            ? reserveMembers[index]
            : null;

        string displayName = member != null ? member.DisplayName : "Reserve Ally";
        Sprite faceIcon = member != null ? member.FaceIcon : null;
        int currentHp = member != null ? member.CurrentHp : 0;
        int maxHp = member != null ? member.MaxHp : 0;
        entry.SetStatus(displayName, faceIcon, currentHp, maxHp, true);
    }

    private void CacheEntryGroup(string groupName, BattlePartyStatusEntryView[] entries)
    {
        Transform group = transform.Find(groupName);
        if (group == null)
        {
            return;
        }

        for (int i = 0; i < entries.Length && i < group.childCount; i++)
        {
            BattlePartyStatusEntryView view = group.GetChild(i).GetComponent<BattlePartyStatusEntryView>();
            if (view != null)
            {
                entries[i] = view;
                view.CacheReferences();
            }
        }
    }

    private static BattlePartyStatusEntryView GetEntry(BattlePartyStatusEntryView[] entries, int index)
    {
        return entries != null && index >= 0 && index < entries.Length ? entries[index] : null;
    }
}
