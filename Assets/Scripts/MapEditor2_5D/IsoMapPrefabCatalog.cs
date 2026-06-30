using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum IsoMapPrefabCategory
{
    Floor = 0,
    Bridge = 3,
    Stair = 4,
    Wall = 5,
    Railing = 6,
    Prop = 1,
    Marker = 2
}

[CreateAssetMenu(fileName = "IsoMapPrefabCatalog", menuName = "NeonCardia/Map Editor 2.5D/Iso Map Prefab Catalog")]
public sealed class IsoMapPrefabCatalog : ScriptableObject
{
    [SerializeField] private List<IsoMapPrefabCatalogEntry> entries = new List<IsoMapPrefabCatalogEntry>();

    public List<IsoMapPrefabCatalogEntry> Entries
    {
        get
        {
            if (entries == null)
            {
                entries = new List<IsoMapPrefabCatalogEntry>();
            }

            return entries;
        }
    }

    public IsoMapPrefabCatalogEntry FindEntry(string prefabId)
    {
        if (string.IsNullOrEmpty(prefabId))
        {
            return null;
        }

        for (int i = 0; i < Entries.Count; i++)
        {
            IsoMapPrefabCatalogEntry entry = Entries[i];
            if (entry != null && string.Equals(entry.PrefabId, prefabId, StringComparison.Ordinal))
            {
                return entry;
            }
        }

        return null;
    }

    public bool ContainsPrefabId(string prefabId)
    {
        return FindEntry(prefabId) != null;
    }

    public bool TryGetPrefab(string prefabId, out GameObject prefab)
    {
        IsoMapPrefabCatalogEntry entry = FindEntry(prefabId);
        prefab = entry != null ? entry.Prefab : null;
        return prefab != null;
    }

    private void OnValidate()
    {
        for (int i = 0; i < Entries.Count; i++)
        {
            IsoMapPrefabCatalogEntry entry = Entries[i];
            if (entry != null)
            {
                entry.NormalizeSerializedValues();
            }
        }
    }
}

[Serializable]
public sealed class IsoMapPrefabCatalogEntry
{
    [SerializeField] private string prefabId;
    [SerializeField] private string displayName;
    [SerializeField] private GameObject prefab;
    [SerializeField] private IsoMapPrefabCategory category = IsoMapPrefabCategory.Prop;
    [FormerlySerializedAs("walkable")]
    [SerializeField] private bool defaultWalkable = true;
    [FormerlySerializedAs("blocksMovement")]
    [SerializeField] private bool defaultBlocksMovement;
    [SerializeField] private bool defaultBlocksMovementConfigured = true;
    [SerializeField] private int defaultRotationY;
    [SerializeField] private Vector3 defaultOffset;
    [SerializeField] private string notes;

    public IsoMapPrefabCatalogEntry(string prefabId, GameObject prefab, IsoMapPrefabCategory category, bool walkable, bool blocksMovement, Vector3 defaultOffset)
        : this(prefabId, prefabId, prefab, category, walkable, blocksMovement, true, 0, defaultOffset, string.Empty)
    {
    }

    public IsoMapPrefabCatalogEntry(
        string prefabId,
        string displayName,
        GameObject prefab,
        IsoMapPrefabCategory category,
        bool defaultWalkable,
        bool defaultBlocksMovement,
        bool defaultBlocksMovementConfigured,
        int defaultRotationY,
        Vector3 defaultOffset,
        string notes)
    {
        this.prefabId = prefabId;
        this.displayName = displayName;
        this.prefab = prefab;
        this.category = category;
        this.defaultWalkable = defaultWalkable;
        this.defaultBlocksMovement = defaultBlocksMovement;
        this.defaultBlocksMovementConfigured = defaultBlocksMovementConfigured;
        this.defaultRotationY = IsoMapData.NormalizeRotationY(defaultRotationY);
        this.defaultOffset = defaultOffset;
        this.notes = notes;
        NormalizeSerializedValues();
    }

    public string PrefabId
    {
        get { return prefabId; }
        set { prefabId = value; }
    }

    public string DisplayName
    {
        get { return string.IsNullOrWhiteSpace(displayName) ? prefabId : displayName; }
        set { displayName = value; }
    }

    public GameObject Prefab
    {
        get { return prefab; }
        set { prefab = value; }
    }

    public IsoMapPrefabCategory Category
    {
        get { return category; }
        set { category = value; }
    }

    public bool Walkable
    {
        get { return defaultWalkable; }
        set { defaultWalkable = value; }
    }

    public bool BlocksMovement
    {
        get { return defaultBlocksMovement; }
        set { defaultBlocksMovement = value; }
    }

    public bool DefaultWalkable
    {
        get { return defaultWalkable; }
        set { defaultWalkable = value; }
    }

    public bool DefaultBlocksMovement
    {
        get { return defaultBlocksMovement; }
        set { defaultBlocksMovement = value; }
    }

    public bool DefaultBlocksMovementConfigured
    {
        get { return defaultBlocksMovementConfigured; }
        set { defaultBlocksMovementConfigured = value; }
    }

    public int DefaultRotationY
    {
        get { return IsoMapData.NormalizeRotationY(defaultRotationY); }
        set { defaultRotationY = IsoMapData.NormalizeRotationY(value); }
    }

    public Vector3 DefaultOffset
    {
        get { return defaultOffset; }
        set { defaultOffset = value; }
    }

    public string Notes
    {
        get { return notes; }
        set { notes = value; }
    }

    public void NormalizeSerializedValues()
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = prefabId;
        }

        defaultRotationY = IsoMapData.NormalizeRotationY(defaultRotationY);
        if (notes == null)
        {
            notes = string.Empty;
        }
    }
}
