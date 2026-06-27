using System;
using System.Collections.Generic;
using UnityEngine;

public enum IsoMapPrefabCategory
{
    Tile,
    Prop,
    Marker
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
}

[Serializable]
public sealed class IsoMapPrefabCatalogEntry
{
    [SerializeField] private string prefabId;
    [SerializeField] private GameObject prefab;
    [SerializeField] private IsoMapPrefabCategory category = IsoMapPrefabCategory.Prop;
    [SerializeField] private bool walkable = true;
    [SerializeField] private bool blocksMovement;
    [SerializeField] private Vector3 defaultOffset;

    public IsoMapPrefabCatalogEntry(string prefabId, GameObject prefab, IsoMapPrefabCategory category, bool walkable, bool blocksMovement, Vector3 defaultOffset)
    {
        this.prefabId = prefabId;
        this.prefab = prefab;
        this.category = category;
        this.walkable = walkable;
        this.blocksMovement = blocksMovement;
        this.defaultOffset = defaultOffset;
    }

    public string PrefabId
    {
        get { return prefabId; }
        set { prefabId = value; }
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
        get { return walkable; }
        set { walkable = value; }
    }

    public bool BlocksMovement
    {
        get { return blocksMovement; }
        set { blocksMovement = value; }
    }

    public Vector3 DefaultOffset
    {
        get { return defaultOffset; }
        set { defaultOffset = value; }
    }
}
