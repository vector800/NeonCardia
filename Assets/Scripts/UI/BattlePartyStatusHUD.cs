using System;
using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public sealed class BattlePartyStatusHUD : MonoBehaviour
{
    private const int MemberRowCount = 3;

    [Serializable]
    public sealed class BattlePartyHudMember
    {
        public string DisplayName;
        public Sprite Portrait;
        public int CurrentHp = 100;
        public int MaxHp = 100;
        public int CurrentMp = 40;
        public int MaxMp = 80;
    }

    [SerializeField] private RectTransform activeMemberPanel;
    [SerializeField] private RectTransform reserveMemberPanel;
    [FormerlySerializedAs("rows")]
    [SerializeField] private PartyMemberStatusRowView[] activeRows = new PartyMemberStatusRowView[MemberRowCount];
    [SerializeField] private PartyMemberStatusRowView[] reserveRows = new PartyMemberStatusRowView[MemberRowCount];
    [SerializeField] private int selectedIndex;
    [FormerlySerializedAs("previewMembers")]
    [SerializeField] private BattlePartyHudMember[] previewActiveMembers =
    {
        new BattlePartyHudMember { DisplayName = "Cyber Wolf", CurrentHp = 125, MaxHp = 125, CurrentMp = 58, MaxMp = 100 },
        new BattlePartyHudMember { DisplayName = "Armor Ally", CurrentHp = 150, MaxHp = 150, CurrentMp = 48, MaxMp = 100 },
        new BattlePartyHudMember { DisplayName = "Blue Girl", CurrentHp = 110, MaxHp = 110, CurrentMp = 68, MaxMp = 100 }
    };
    [SerializeField] private BattlePartyHudMember[] previewReserveMembers =
    {
        new BattlePartyHudMember { DisplayName = "Reserve Ally A", CurrentHp = 110, MaxHp = 110, CurrentMp = 32, MaxMp = 100 },
        new BattlePartyHudMember { DisplayName = "Reserve Ally B", CurrentHp = 104, MaxHp = 104, CurrentMp = 30, MaxMp = 100 },
        new BattlePartyHudMember { DisplayName = "Reserve Ally C", CurrentHp = 116, MaxHp = 116, CurrentMp = 34, MaxMp = 100 }
    };

    private void Reset()
    {
        CacheReferences();
    }

    private void Awake()
    {
        CacheReferences();
    }

    private void OnValidate()
    {
        CacheReferences();
    }

    public void SetMember(int index, string displayName, Sprite portrait, int currentHp, int maxHp, int currentMp, int maxMp, bool selected)
    {
        PartyMemberStatusRowView row = GetActiveRow(index);
        if (row == null)
        {
            return;
        }

        row.SetStatus(displayName, portrait, currentHp, maxHp, currentMp, maxMp, selected, false);
    }

    public void SetReserveMember(int index, string displayName, Sprite portrait, int currentHp, int maxHp, int currentMp, int maxMp)
    {
        PartyMemberStatusRowView row = GetReserveRow(index);
        if (row == null)
        {
            return;
        }

        if (reserveMemberPanel != null)
        {
            reserveMemberPanel.gameObject.SetActive(true);
        }

        row.SetStatus(displayName, portrait, currentHp, maxHp, currentMp, maxMp, false, true);
    }

    public void ClearReserveMember(int index)
    {
        PartyMemberStatusRowView row = GetReserveRow(index);
        if (row != null)
        {
            row.Clear();
        }
    }

    public void SetSelectedIndex(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, activeRows != null && activeRows.Length > 0 ? activeRows.Length - 1 : 0);
        RefreshSelection();
    }

    public void RefreshPreviewMembers()
    {
        CacheReferences();

        for (int i = 0; i < activeRows.Length; i++)
        {
            BattlePartyHudMember member = previewActiveMembers != null && i < previewActiveMembers.Length ? previewActiveMembers[i] : null;
            if (member == null)
            {
                ClearActiveMember(i);
                continue;
            }

            SetMember(i, member.DisplayName, member.Portrait, member.CurrentHp, member.MaxHp, member.CurrentMp, member.MaxMp, i == selectedIndex);
        }

        RefreshPreviewReserveMembers();
    }

    public void RefreshPreviewReserveMembers()
    {
        CacheReferences();

        bool hasVisibleReserve = false;
        for (int i = 0; i < reserveRows.Length; i++)
        {
            BattlePartyHudMember member = previewReserveMembers != null && i < previewReserveMembers.Length ? previewReserveMembers[i] : null;
            if (member == null)
            {
                ClearReserveMember(i);
                continue;
            }

            hasVisibleReserve = true;
            SetReserveMember(i, member.DisplayName, member.Portrait, member.CurrentHp, member.MaxHp, member.CurrentMp, member.MaxMp);
        }

        if (reserveMemberPanel != null)
        {
            reserveMemberPanel.gameObject.SetActive(hasVisibleReserve);
        }
    }

    public void CacheReferences()
    {
        if (activeRows == null || activeRows.Length != MemberRowCount)
        {
            activeRows = new PartyMemberStatusRowView[MemberRowCount];
        }

        if (reserveRows == null || reserveRows.Length != MemberRowCount)
        {
            reserveRows = new PartyMemberStatusRowView[MemberRowCount];
        }

        activeMemberPanel = FindRectTransform(new[] { "BattleHud/ActiveMemberPanel", "ActiveMemberPanel", "AllyHpHudRoot", "Rows" }, activeMemberPanel);
        reserveMemberPanel = FindRectTransform(new[] { "BattleHud/ReserveMemberPanel", "ReserveMemberPanel" }, reserveMemberPanel);

        Transform activeRoot = activeMemberPanel != null ? activeMemberPanel : transform;
        Transform reserveRoot = reserveMemberPanel != null ? reserveMemberPanel : null;

        CacheRows(activeRoot, activeRows);
        if (reserveRoot != null)
        {
            CacheRows(reserveRoot, reserveRows);
        }
    }

    private void ClearActiveMember(int index)
    {
        PartyMemberStatusRowView row = GetActiveRow(index);
        if (row != null)
        {
            row.Clear();
        }
    }

    private void RefreshSelection()
    {
        if (activeRows == null)
        {
            return;
        }

        for (int i = 0; i < activeRows.Length; i++)
        {
            if (activeRows[i] != null)
            {
                activeRows[i].SetSelected(i == selectedIndex);
            }
        }
    }

    private PartyMemberStatusRowView GetActiveRow(int index)
    {
        return activeRows != null && index >= 0 && index < activeRows.Length ? activeRows[index] : null;
    }

    private PartyMemberStatusRowView GetReserveRow(int index)
    {
        return reserveRows != null && index >= 0 && index < reserveRows.Length ? reserveRows[index] : null;
    }

    private static void CacheRows(Transform root, PartyMemberStatusRowView[] targetRows)
    {
        if (root == null || targetRows == null)
        {
            return;
        }

        for (int i = 0; i < targetRows.Length; i++)
        {
            if (targetRows[i] == null)
            {
                Transform namedRow = root.Find("MemberStatusRow_" + i);
                if (namedRow == null)
                {
                    namedRow = root.Find("AllyHpRow_" + i);
                }

                if (namedRow == null && i < root.childCount)
                {
                    namedRow = root.GetChild(i);
                }

                targetRows[i] = namedRow != null ? namedRow.GetComponent<PartyMemberStatusRowView>() : null;
            }

            if (targetRows[i] != null)
            {
                targetRows[i].CacheReferences();
            }
        }
    }

    private RectTransform FindRectTransform(string[] paths, RectTransform fallback)
    {
        if (fallback != null)
        {
            return fallback;
        }

        for (int i = 0; i < paths.Length; i++)
        {
            Transform child = transform.Find(paths[i]);
            if (child == null)
            {
                continue;
            }

            RectTransform rect = child as RectTransform;
            if (rect != null)
            {
                return rect;
            }
        }

        return null;
    }
}
