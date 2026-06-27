using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IsoMapData", menuName = "NeonCardia/Map Editor 2.5D/Iso Map Data")]
public sealed class IsoMapData : ScriptableObject
{
    [SerializeField] private string mapId = "new_iso_map";
    [SerializeField] private string displayName = "New Iso Map";
    [SerializeField] private int width = 12;
    [SerializeField] private int depth = 12;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private int maxLevel = 3;
    [SerializeField] private Vector3Int defaultSpawnCell = Vector3Int.zero;
    [SerializeField] private List<IsoMapTileInstanceData> tileInstances = new List<IsoMapTileInstanceData>();
    [SerializeField] private List<IsoMapPropInstanceData> propInstances = new List<IsoMapPropInstanceData>();
    [SerializeField] private List<IsoMapEncounterAreaData> encounterAreas = new List<IsoMapEncounterAreaData>();
    [SerializeField] private List<IsoMapTransitionData> transitions = new List<IsoMapTransitionData>();

    public string MapId
    {
        get { return mapId; }
        set { mapId = value; }
    }

    public string DisplayName
    {
        get { return displayName; }
        set { displayName = value; }
    }

    public int Width
    {
        get { return width; }
        set { width = Mathf.Max(1, value); }
    }

    public int Depth
    {
        get { return depth; }
        set { depth = Mathf.Max(1, value); }
    }

    public float CellSize
    {
        get { return cellSize; }
        set { cellSize = Mathf.Max(0.01f, value); }
    }

    public int MaxLevel
    {
        get { return maxLevel; }
        set { maxLevel = Mathf.Max(0, value); }
    }

    public Vector3Int DefaultSpawnCell
    {
        get { return defaultSpawnCell; }
        set { defaultSpawnCell = value; }
    }

    public List<IsoMapTileInstanceData> TileInstances
    {
        get
        {
            if (tileInstances == null)
            {
                tileInstances = new List<IsoMapTileInstanceData>();
            }

            return tileInstances;
        }
    }

    public List<IsoMapPropInstanceData> PropInstances
    {
        get
        {
            if (propInstances == null)
            {
                propInstances = new List<IsoMapPropInstanceData>();
            }

            return propInstances;
        }
    }

    public List<IsoMapEncounterAreaData> EncounterAreas
    {
        get
        {
            if (encounterAreas == null)
            {
                encounterAreas = new List<IsoMapEncounterAreaData>();
            }

            return encounterAreas;
        }
    }

    public List<IsoMapTransitionData> Transitions
    {
        get
        {
            if (transitions == null)
            {
                transitions = new List<IsoMapTransitionData>();
            }

            return transitions;
        }
    }

    public Vector3 CellToWorld(Vector3Int cell)
    {
        return new Vector3(cell.x * CellSize, cell.y * CellSize, cell.z * CellSize);
    }

    public bool IsInsideBounds(Vector3Int cell)
    {
        return cell.x >= 0
            && cell.x < Width
            && cell.z >= 0
            && cell.z < Depth
            && cell.y >= 0
            && cell.y <= MaxLevel;
    }

    public static int NormalizeRotationY(int value)
    {
        int rounded = Mathf.RoundToInt(value / 90f) * 90;
        rounded %= 360;
        if (rounded < 0)
        {
            rounded += 360;
        }

        return rounded;
    }

    private void OnValidate()
    {
        width = Mathf.Max(1, width);
        depth = Mathf.Max(1, depth);
        cellSize = Mathf.Max(0.01f, cellSize);
        maxLevel = Mathf.Max(0, maxLevel);

        if (tileInstances == null)
        {
            tileInstances = new List<IsoMapTileInstanceData>();
        }

        if (propInstances == null)
        {
            propInstances = new List<IsoMapPropInstanceData>();
        }

        if (encounterAreas == null)
        {
            encounterAreas = new List<IsoMapEncounterAreaData>();
        }

        if (transitions == null)
        {
            transitions = new List<IsoMapTransitionData>();
        }
    }
}

[Serializable]
public sealed class IsoMapTileInstanceData
{
    [SerializeField] private string instanceId;
    [SerializeField] private string prefabId;
    [SerializeField] private Vector3Int cell;
    [SerializeField] private int rotationY;
    [SerializeField] private bool walkable = true;
    [SerializeField] private string areaId;

    public IsoMapTileInstanceData(string instanceId, string prefabId, Vector3Int cell, int rotationY, bool walkable, string areaId)
    {
        this.instanceId = instanceId;
        this.prefabId = prefabId;
        this.cell = cell;
        this.rotationY = IsoMapData.NormalizeRotationY(rotationY);
        this.walkable = walkable;
        this.areaId = areaId;
    }

    public string InstanceId
    {
        get { return instanceId; }
        set { instanceId = value; }
    }

    public string PrefabId
    {
        get { return prefabId; }
        set { prefabId = value; }
    }

    public Vector3Int Cell
    {
        get { return cell; }
        set { cell = value; }
    }

    public int RotationY
    {
        get { return rotationY; }
        set { rotationY = IsoMapData.NormalizeRotationY(value); }
    }

    public bool Walkable
    {
        get { return walkable; }
        set { walkable = value; }
    }

    public string AreaId
    {
        get { return areaId; }
        set { areaId = value; }
    }
}

[Serializable]
public sealed class IsoMapPropInstanceData
{
    [SerializeField] private string instanceId;
    [SerializeField] private string prefabId;
    [SerializeField] private Vector3Int cell;
    [SerializeField] private Vector3 offset;
    [SerializeField] private int rotationY;
    [SerializeField] private bool blocksMovement;

    public IsoMapPropInstanceData(string instanceId, string prefabId, Vector3Int cell, Vector3 offset, int rotationY, bool blocksMovement)
    {
        this.instanceId = instanceId;
        this.prefabId = prefabId;
        this.cell = cell;
        this.offset = offset;
        this.rotationY = IsoMapData.NormalizeRotationY(rotationY);
        this.blocksMovement = blocksMovement;
    }

    public string InstanceId
    {
        get { return instanceId; }
        set { instanceId = value; }
    }

    public string PrefabId
    {
        get { return prefabId; }
        set { prefabId = value; }
    }

    public Vector3Int Cell
    {
        get { return cell; }
        set { cell = value; }
    }

    public Vector3 Offset
    {
        get { return offset; }
        set { offset = value; }
    }

    public int RotationY
    {
        get { return rotationY; }
        set { rotationY = IsoMapData.NormalizeRotationY(value); }
    }

    public bool BlocksMovement
    {
        get { return blocksMovement; }
        set { blocksMovement = value; }
    }
}

[Serializable]
public sealed class IsoMapEncounterAreaData
{
    [SerializeField] private string encounterAreaId;
    [SerializeField] private List<Vector3Int> cells = new List<Vector3Int>();
    [SerializeField] private string enemyGroupTableId;
    [SerializeField] private string battleBackgroundId;
    [SerializeField] private string battleBgmId;
    [SerializeField] private int minStepsBeforeEncounter = 5;
    [SerializeField] private float encounterChancePerStep = 0.10f;
    [SerializeField] private int cooldownStepsAfterBattle = 5;

    public string EncounterAreaId
    {
        get { return encounterAreaId; }
        set { encounterAreaId = value; }
    }

    public List<Vector3Int> Cells
    {
        get
        {
            if (cells == null)
            {
                cells = new List<Vector3Int>();
            }

            return cells;
        }
    }

    public string EnemyGroupTableId
    {
        get { return enemyGroupTableId; }
        set { enemyGroupTableId = value; }
    }

    public string BattleBackgroundId
    {
        get { return battleBackgroundId; }
        set { battleBackgroundId = value; }
    }

    public string BattleBgmId
    {
        get { return battleBgmId; }
        set { battleBgmId = value; }
    }

    public int MinStepsBeforeEncounter
    {
        get { return minStepsBeforeEncounter; }
        set { minStepsBeforeEncounter = Mathf.Max(0, value); }
    }

    public float EncounterChancePerStep
    {
        get { return encounterChancePerStep; }
        set { encounterChancePerStep = Mathf.Clamp01(value); }
    }

    public int CooldownStepsAfterBattle
    {
        get { return cooldownStepsAfterBattle; }
        set { cooldownStepsAfterBattle = Mathf.Max(0, value); }
    }
}

[Serializable]
public sealed class IsoMapTransitionData
{
    [SerializeField] private string transitionId;
    [SerializeField] private Vector3Int cell;
    [SerializeField] private string targetMapId;
    [SerializeField] private string targetSpawnId;
    [SerializeField] private string requiredFlag;

    public string TransitionId
    {
        get { return transitionId; }
        set { transitionId = value; }
    }

    public Vector3Int Cell
    {
        get { return cell; }
        set { cell = value; }
    }

    public string TargetMapId
    {
        get { return targetMapId; }
        set { targetMapId = value; }
    }

    public string TargetSpawnId
    {
        get { return targetSpawnId; }
        set { targetSpawnId = value; }
    }

    public string RequiredFlag
    {
        get { return requiredFlag; }
        set { requiredFlag = value; }
    }
}
