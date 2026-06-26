using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BattlePartyStatusHUD : MonoBehaviour
{
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

    [SerializeField] private PartyMemberStatusRowView[] rows = new PartyMemberStatusRowView[3];
    [SerializeField] private int selectedIndex;
    [SerializeField] private BattlePartyHudMember[] previewMembers =
    {
        new BattlePartyHudMember { DisplayName = "Cyber Wolf", CurrentHp = 125, MaxHp = 125, CurrentMp = 58, MaxMp = 100 },
        new BattlePartyHudMember { DisplayName = "Armor Ally", CurrentHp = 150, MaxHp = 150, CurrentMp = 48, MaxMp = 100 },
        new BattlePartyHudMember { DisplayName = "Blue Girl", CurrentHp = 110, MaxHp = 110, CurrentMp = 68, MaxMp = 100 }
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
        PartyMemberStatusRowView row = GetRow(index);
        if (row == null)
        {
            return;
        }

        row.SetStatus(displayName, portrait, currentHp, maxHp, currentMp, maxMp, selected);
    }

    public void SetSelectedIndex(int index)
    {
        selectedIndex = Mathf.Clamp(index, 0, rows != null && rows.Length > 0 ? rows.Length - 1 : 0);
        RefreshSelection();
    }

    public void RefreshPreviewMembers()
    {
        CacheReferences();

        for (int i = 0; i < rows.Length; i++)
        {
            BattlePartyHudMember member = previewMembers != null && i < previewMembers.Length ? previewMembers[i] : null;
            if (member == null)
            {
                SetMember(i, "Empty", null, 0, 0, 0, 0, i == selectedIndex);
                continue;
            }

            SetMember(i, member.DisplayName, member.Portrait, member.CurrentHp, member.MaxHp, member.CurrentMp, member.MaxMp, i == selectedIndex);
        }
    }

    public void CacheReferences()
    {
        if (rows == null || rows.Length != 3)
        {
            rows = new PartyMemberStatusRowView[3];
        }

        Transform rowsRoot = transform.Find("Rows");
        if (rowsRoot == null)
        {
            rowsRoot = transform;
        }

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] == null && i < rowsRoot.childCount)
            {
                rows[i] = rowsRoot.GetChild(i).GetComponent<PartyMemberStatusRowView>();
            }

            if (rows[i] != null)
            {
                rows[i].CacheReferences();
            }
        }
    }

    private void RefreshSelection()
    {
        if (rows == null)
        {
            return;
        }

        for (int i = 0; i < rows.Length; i++)
        {
            if (rows[i] != null)
            {
                rows[i].SetSelected(i == selectedIndex);
            }
        }
    }

    private PartyMemberStatusRowView GetRow(int index)
    {
        return rows != null && index >= 0 && index < rows.Length ? rows[index] : null;
    }
}
