using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class IsoMapRuntimeController : MonoBehaviour
{
    [SerializeField] private IsoMapData mapData;
    [SerializeField] private IsoMapPrefabCatalog prefabCatalog;
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private string fallbackEnemyGroupId = "sf_floating_test_group";
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private float moveSeconds = 0.14f;
    [SerializeField] private Vector3 playerOffset = new Vector3(0f, 0.48f, 0f);
    [SerializeField] private bool showEncounterAreaMarkers = true;
    [SerializeField] private Vector3 cameraOffset = new Vector3(4.8f, 7.2f, -7.2f);

    private const string RuntimeRootName = "IsoMapRuntimeRoot";
    private const string TileRootName = "Tiles";
    private const string PropRootName = "Props";
    private const string EncounterMarkerRootName = "EncounterAreaRuntimeMarkers";
    private const string EncounterMarkerPrefabId = "EncounterAreaMarker";
    private const string StairUpPrefabId = "Stair_Up";

    private readonly Dictionary<Vector3Int, RuntimeCell> cells = new Dictionary<Vector3Int, RuntimeCell>();
    private readonly Dictionary<Vector3Int, IsoMapEncounterAreaData> encounterCells = new Dictionary<Vector3Int, IsoMapEncounterAreaData>();
    private readonly HashSet<Vector3Int> stairCells = new HashSet<Vector3Int>();

    private GameObject runtimeRoot;
    private GameObject playerObject;
    private Vector3Int playerCell;
    private Vector2Int lastMoveDirection = Vector2Int.down;
    private bool moving;
    private bool encounterStarting;
    private int totalSteps;
    private int stepsInCurrentEncounterArea;
    private int cooldownStepsRemaining;
    private string activeEncounterAreaId;

    private sealed class RuntimeCell
    {
        public bool Walkable;
        public bool BlocksMovement;
    }

    private void Start()
    {
        if (mapData == null)
        {
            Debug.LogError("[IsoMapRuntime] IsoMapData is not assigned.");
            return;
        }

        if (prefabCatalog == null)
        {
            Debug.LogError("[IsoMapRuntime] IsoMapPrefabCatalog is not assigned.");
            return;
        }

        BuildRuntimeScene();

        Vector3Int spawnCell = mapData.DefaultSpawnCell;
        BattleConnectionResultData resultData;
        if (BattleConnectionContext.TryConsumeResultForScene(SceneManager.GetActiveScene().name, out resultData))
        {
            spawnCell = resultData.ReturnCell;
            totalSteps = resultData.StepCountAtEncounter;
            stepsInCurrentEncounterArea = 0;
            activeEncounterAreaId = string.Empty;
            lastMoveDirection = resultData.PlayerDirection == Vector2Int.zero ? Vector2Int.down : resultData.PlayerDirection;
            cooldownStepsRemaining = resultData.EncounterCooldownStepsAfterBattle;
            if (cooldownStepsRemaining <= 0)
            {
                IsoMapEncounterAreaData returnedArea = FindEncounterAreaById(resultData.EncounterAreaId);
                if (returnedArea != null)
                {
                    cooldownStepsRemaining = returnedArea.CooldownStepsAfterBattle;
                }
            }

            if (!string.IsNullOrEmpty(resultData.ReturnMapId)
                && !string.Equals(resultData.ReturnMapId, mapData.MapId, StringComparison.Ordinal))
            {
                Debug.LogWarning("[IsoMapRuntime] Battle return mapId does not match assigned map. returnMapId="
                    + resultData.ReturnMapId + " assignedMapId=" + mapData.MapId);
            }

            Debug.Log("[IsoMapRuntime] Returned from BattleScene. result="
                + resultData.ResultType
                + " mapId=" + resultData.ReturnMapId
                + " returnCell=" + FormatCell(spawnCell)
                + " cooldownSteps=" + cooldownStepsRemaining
                + " encounterAreaId=" + resultData.EncounterAreaId);
        }

        playerCell = FindNearestWalkableCell(spawnCell);
        CreateOrPlacePlayer();
        ConfigureCamera();

        Debug.Log("[IsoMapRuntime] Map ready. mapId=" + mapData.MapId
            + " spawnCell=" + FormatCell(playerCell)
            + " walkableCells=" + CountWalkableCells()
            + ". Move with WASD/Arrow keys. TEST ONLY: press E to force an encounter.");
    }

    private void Update()
    {
        if (encounterStarting || moving || playerObject == null)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        // TEST ONLY: force the BattleScene transition without waiting for the random encounter roll.
        if (keyboard.eKey.wasPressedThisFrame)
        {
            TryStartEncounter(true);
            return;
        }

        Vector2Int direction = ReadMoveDirection(keyboard);
        if (direction != Vector2Int.zero)
        {
            TryStartMove(direction);
        }
    }

    private void BuildRuntimeScene()
    {
        cells.Clear();
        encounterCells.Clear();
        stairCells.Clear();

        ClearRuntimeRoot();
        runtimeRoot = new GameObject(RuntimeRootName);
        Transform tileRoot = CreateChildRoot(runtimeRoot.transform, TileRootName);
        Transform propRoot = CreateChildRoot(runtimeRoot.transform, PropRootName);

        for (int i = 0; i < mapData.TileInstances.Count; i++)
        {
            IsoMapTileInstanceData tile = mapData.TileInstances[i];
            if (tile == null)
            {
                continue;
            }

            IsoMapPrefabCatalogEntry entry = prefabCatalog.FindEntry(tile.PrefabId);
            InstantiateCatalogPrefab(tileRoot, entry, tile.PrefabId, tile.InstanceId, tile.Cell, Vector3.zero, tile.RotationY);
            RegisterTile(tile, entry);
        }

        for (int i = 0; i < mapData.PropInstances.Count; i++)
        {
            IsoMapPropInstanceData prop = mapData.PropInstances[i];
            if (prop == null)
            {
                continue;
            }

            IsoMapPrefabCatalogEntry entry = prefabCatalog.FindEntry(prop.PrefabId);
            InstantiateCatalogPrefab(propRoot, entry, prop.PrefabId, prop.InstanceId, prop.Cell, prop.Offset, prop.RotationY);
            RegisterProp(prop, entry);
        }

        BuildEncounterLookup();
        if (showEncounterAreaMarkers)
        {
            CreateEncounterAreaMarkers(runtimeRoot.transform);
        }
    }

    private void ClearRuntimeRoot()
    {
        GameObject existing = GameObject.Find(RuntimeRootName);
        if (existing != null)
        {
            Destroy(existing);
        }
    }

    private Transform CreateChildRoot(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    private void InstantiateCatalogPrefab(Transform parent, IsoMapPrefabCatalogEntry entry, string prefabId, string instanceId, Vector3Int cell, Vector3 offset, int rotationY)
    {
        if (entry == null || entry.Prefab == null)
        {
            Debug.LogWarning("[IsoMapRuntime] Missing prefabId in catalog: " + prefabId);
            return;
        }

        GameObject instance = Instantiate(entry.Prefab, parent);
        instance.name = prefabId + "_" + instanceId;
        instance.transform.position = CellToWorld(cell) + offset;
        instance.transform.rotation = Quaternion.Euler(0f, IsoMapData.NormalizeRotationY(rotationY), 0f);
    }

    private void RegisterTile(IsoMapTileInstanceData tile, IsoMapPrefabCatalogEntry entry)
    {
        RuntimeCell runtimeCell = GetOrCreateCell(tile.Cell);
        bool entryWalkable = entry == null || entry.Walkable;
        bool entryBlocksMovement = entry != null && entry.BlocksMovement;
        runtimeCell.Walkable = runtimeCell.Walkable || (tile.Walkable && entryWalkable && !entryBlocksMovement);
        runtimeCell.BlocksMovement = runtimeCell.BlocksMovement || entryBlocksMovement;

        if (string.Equals(tile.PrefabId, StairUpPrefabId, StringComparison.Ordinal))
        {
            stairCells.Add(tile.Cell);
        }
    }

    private void RegisterProp(IsoMapPropInstanceData prop, IsoMapPrefabCatalogEntry entry)
    {
        bool blocksMovement = prop.BlocksMovement || (entry != null && entry.BlocksMovement);
        if (!blocksMovement)
        {
            return;
        }

        RuntimeCell runtimeCell = GetOrCreateCell(prop.Cell);
        runtimeCell.BlocksMovement = true;
    }

    private RuntimeCell GetOrCreateCell(Vector3Int cell)
    {
        RuntimeCell runtimeCell;
        if (!cells.TryGetValue(cell, out runtimeCell))
        {
            runtimeCell = new RuntimeCell();
            cells.Add(cell, runtimeCell);
        }

        return runtimeCell;
    }

    private void BuildEncounterLookup()
    {
        for (int i = 0; i < mapData.EncounterAreas.Count; i++)
        {
            IsoMapEncounterAreaData area = mapData.EncounterAreas[i];
            if (area == null)
            {
                continue;
            }

            for (int cellIndex = 0; cellIndex < area.Cells.Count; cellIndex++)
            {
                Vector3Int cell = area.Cells[cellIndex];
                if (!mapData.IsInsideBounds(cell))
                {
                    Debug.LogWarning("[IsoMapRuntime] EncounterArea cell outside bounds. areaId="
                        + area.EncounterAreaId + " cell=" + FormatCell(cell));
                    continue;
                }

                encounterCells[cell] = area;
            }
        }
    }

    private void CreateEncounterAreaMarkers(Transform parent)
    {
        IsoMapPrefabCatalogEntry entry = prefabCatalog.FindEntry(EncounterMarkerPrefabId);
        if (entry == null || entry.Prefab == null)
        {
            return;
        }

        Transform markerRoot = CreateChildRoot(parent, EncounterMarkerRootName);
        foreach (KeyValuePair<Vector3Int, IsoMapEncounterAreaData> pair in encounterCells)
        {
            GameObject marker = Instantiate(entry.Prefab, markerRoot);
            marker.name = EncounterMarkerPrefabId + "_" + pair.Value.EncounterAreaId + "_" + FormatCellForName(pair.Key);
            marker.transform.position = CellToWorld(pair.Key) + entry.DefaultOffset;
            marker.transform.rotation = Quaternion.identity;
        }
    }

    private void CreateOrPlacePlayer()
    {
        if (playerObject == null)
        {
            playerObject = playerPrefab != null ? Instantiate(playerPrefab) : CreateFallbackPlayer();
            playerObject.name = "IsoMapRuntimePlayer";
        }

        playerObject.transform.position = CellToWorld(playerCell) + playerOffset;
    }

    private GameObject CreateFallbackPlayer()
    {
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.transform.localScale = new Vector3(0.42f, 0.62f, 0.42f);
        MeshRenderer renderer = capsule.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader != null)
            {
                Material material = new Material(shader);
                material.name = "MIso_RuntimePlayer_Cyan";
                material.color = new Color(0.16f, 0.88f, 1f, 1f);
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", material.color);
                }

                renderer.sharedMaterial = material;
            }
        }

        return capsule;
    }

    private void ConfigureCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
            if (FindAnyObjectByType<AudioListener>() == null)
            {
                cameraObject.AddComponent<AudioListener>();
            }
        }

        Vector3 center = new Vector3(
            (mapData.Width - 1) * mapData.CellSize * 0.5f,
            mapData.MaxLevel * mapData.CellSize * 0.35f,
            (mapData.Depth - 1) * mapData.CellSize * 0.5f);
        mainCamera.orthographic = true;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.015f, 0.018f, 0.026f, 1f);
        mainCamera.transform.position = center + cameraOffset;
        mainCamera.transform.LookAt(center);
        mainCamera.orthographicSize = Mathf.Max(4.2f, Mathf.Max(mapData.Width, mapData.Depth) * mapData.CellSize * 0.58f);
    }

    private Vector2Int ReadMoveDirection(Keyboard keyboard)
    {
        if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
        {
            return Vector2Int.left;
        }

        if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
        {
            return Vector2Int.right;
        }

        if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
        {
            return Vector2Int.up;
        }

        if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
        {
            return Vector2Int.down;
        }

        return Vector2Int.zero;
    }

    private void TryStartMove(Vector2Int direction)
    {
        Vector3Int nextCell;
        if (!TryResolveMoveTarget(playerCell, direction, out nextCell))
        {
            Debug.Log("[IsoMapRuntime] Move blocked. current=" + FormatCell(playerCell)
                + " direction=" + direction);
            return;
        }

        lastMoveDirection = direction;
        StartCoroutine(MovePlayer(nextCell));
    }

    private bool TryResolveMoveTarget(Vector3Int currentCell, Vector2Int direction, out Vector3Int nextCell)
    {
        nextCell = new Vector3Int(currentCell.x + direction.x, currentCell.y, currentCell.z + direction.y);
        if (IsWalkableCell(nextCell))
        {
            return true;
        }

        // Temporary stair support for Step 4-7:
        // Stair_Up acts as a connector around the current/target x,z pair and allows
        // one-level up/down movement. Directional stair facing and slope occupancy
        // are intentionally deferred to the later full traversal rules.
        if (HasStairConnector(currentCell) || HasStairConnector(nextCell))
        {
            Vector3Int upperCell = new Vector3Int(nextCell.x, currentCell.y + 1, nextCell.z);
            if (IsWalkableCell(upperCell))
            {
                nextCell = upperCell;
                return true;
            }

            Vector3Int lowerCell = new Vector3Int(nextCell.x, currentCell.y - 1, nextCell.z);
            if (IsWalkableCell(lowerCell))
            {
                nextCell = lowerCell;
                return true;
            }
        }

        return false;
    }

    private bool HasStairConnector(Vector3Int cell)
    {
        if (stairCells.Contains(cell))
        {
            return true;
        }

        Vector3Int lower = new Vector3Int(cell.x, cell.y - 1, cell.z);
        if (stairCells.Contains(lower))
        {
            return true;
        }

        Vector3Int upper = new Vector3Int(cell.x, cell.y + 1, cell.z);
        return stairCells.Contains(upper);
    }

    private IEnumerator MovePlayer(Vector3Int nextCell)
    {
        moving = true;
        Vector3 from = playerObject.transform.position;
        Vector3 to = CellToWorld(nextCell) + playerOffset;
        float elapsed = 0f;
        while (elapsed < moveSeconds)
        {
            elapsed += Time.deltaTime;
            float t = moveSeconds <= 0f ? 1f : Mathf.Clamp01(elapsed / moveSeconds);
            playerObject.transform.position = Vector3.Lerp(from, to, SmoothStep(t));
            yield return null;
        }

        playerObject.transform.position = to;
        playerCell = nextCell;
        moving = false;

        OnStepCompleted();
    }

    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private void OnStepCompleted()
    {
        totalSteps++;

        if (cooldownStepsRemaining > 0)
        {
            cooldownStepsRemaining--;
            stepsInCurrentEncounterArea = 0;
            activeEncounterAreaId = string.Empty;
            Debug.Log("[IsoMapRuntime] Step " + totalSteps + " completed at " + FormatCell(playerCell)
                + ". Encounter skipped by cooldown. remainingSteps=" + cooldownStepsRemaining);
            return;
        }

        IsoMapEncounterAreaData area = FindEncounterAreaForCell(playerCell);
        if (area == null)
        {
            stepsInCurrentEncounterArea = 0;
            activeEncounterAreaId = string.Empty;
            Debug.Log("[IsoMapRuntime] Step " + totalSteps + " completed at " + FormatCell(playerCell)
                + ". No encounter area.");
            return;
        }

        if (!string.Equals(activeEncounterAreaId, area.EncounterAreaId, StringComparison.Ordinal))
        {
            activeEncounterAreaId = area.EncounterAreaId;
            stepsInCurrentEncounterArea = 0;
        }

        stepsInCurrentEncounterArea++;
        if (stepsInCurrentEncounterArea < area.MinStepsBeforeEncounter)
        {
            Debug.Log("[IsoMapRuntime] Step " + totalSteps + " completed at " + FormatCell(playerCell)
                + ". Encounter check not started. areaId=" + area.EncounterAreaId
                + " stepsInArea=" + stepsInCurrentEncounterArea + "/" + area.MinStepsBeforeEncounter);
            return;
        }

        float roll = UnityEngine.Random.value;
        bool encounterHit = roll < area.EncounterChancePerStep;
        Debug.Log("[IsoMapRuntime] Step " + totalSteps + " encounter check. areaId="
            + area.EncounterAreaId
            + " roll=" + roll.ToString("0.000")
            + " chance=" + area.EncounterChancePerStep.ToString("0.000")
            + " result=" + (encounterHit ? "ENCOUNTER" : "none"));

        if (encounterHit)
        {
            StartEncounter(area, false);
        }
    }

    private void TryStartEncounter(bool forced)
    {
        IsoMapEncounterAreaData area = FindEncounterAreaForCell(playerCell);
        if (area == null)
        {
            area = GetFirstEncounterArea();
        }

        if (area == null)
        {
            Debug.LogWarning("[IsoMapRuntime] Cannot start encounter because this map has no EncounterArea.");
            return;
        }

        StartEncounter(area, forced);
    }

    private void StartEncounter(IsoMapEncounterAreaData area, bool forced)
    {
        if (encounterStarting)
        {
            return;
        }

        encounterStarting = true;
        string enemyGroupId = string.IsNullOrEmpty(area.EnemyGroupTableId) ? fallbackEnemyGroupId : area.EnemyGroupTableId;
        string returnSceneName = SceneManager.GetActiveScene().name;
        BattleConnectionContext.BeginBattle(new BattleStartData(
            enemyGroupId,
            returnSceneName,
            mapData.MapId,
            playerCell,
            area.EncounterAreaId,
            totalSteps,
            area.BattleBackgroundId,
            area.BattleBgmId,
            area.CooldownStepsAfterBattle,
            lastMoveDirection));

        Debug.Log("[IsoMapRuntime] " + (forced ? "TEST ONLY forced encounter" : "Random encounter")
            + " started. battleScene=" + battleSceneName
            + " mapId=" + mapData.MapId
            + " enemyGroupId=" + enemyGroupId
            + " encounterAreaId=" + area.EncounterAreaId
            + " returnScene=" + returnSceneName
            + " returnCell=" + FormatCell(playerCell)
            + " backgroundId=" + area.BattleBackgroundId
            + " bgmId=" + area.BattleBgmId
            + " totalSteps=" + totalSteps);

        SceneManager.LoadScene(battleSceneName);
    }

    private bool IsWalkableCell(Vector3Int cell)
    {
        if (!mapData.IsInsideBounds(cell))
        {
            return false;
        }

        RuntimeCell runtimeCell;
        return cells.TryGetValue(cell, out runtimeCell)
            && runtimeCell.Walkable
            && !runtimeCell.BlocksMovement;
    }

    private Vector3Int FindNearestWalkableCell(Vector3Int preferredCell)
    {
        if (IsWalkableCell(preferredCell))
        {
            return preferredCell;
        }

        if (IsWalkableCell(mapData.DefaultSpawnCell))
        {
            Debug.LogWarning("[IsoMapRuntime] Preferred spawn is not walkable. Using defaultSpawnCell. preferred="
                + FormatCell(preferredCell) + " default=" + FormatCell(mapData.DefaultSpawnCell));
            return mapData.DefaultSpawnCell;
        }

        foreach (KeyValuePair<Vector3Int, RuntimeCell> pair in cells)
        {
            if (pair.Value.Walkable && !pair.Value.BlocksMovement)
            {
                Debug.LogWarning("[IsoMapRuntime] No configured spawn cell is walkable. Using first walkable cell: "
                    + FormatCell(pair.Key));
                return pair.Key;
            }
        }

        Debug.LogError("[IsoMapRuntime] Map has no walkable cells. Falling back to defaultSpawnCell.");
        return mapData.DefaultSpawnCell;
    }

    private IsoMapEncounterAreaData FindEncounterAreaForCell(Vector3Int cell)
    {
        IsoMapEncounterAreaData area;
        return encounterCells.TryGetValue(cell, out area) ? area : null;
    }

    private IsoMapEncounterAreaData FindEncounterAreaById(string encounterAreaId)
    {
        if (string.IsNullOrEmpty(encounterAreaId))
        {
            return null;
        }

        for (int i = 0; i < mapData.EncounterAreas.Count; i++)
        {
            IsoMapEncounterAreaData area = mapData.EncounterAreas[i];
            if (area != null && string.Equals(area.EncounterAreaId, encounterAreaId, StringComparison.Ordinal))
            {
                return area;
            }
        }

        return null;
    }

    private IsoMapEncounterAreaData GetFirstEncounterArea()
    {
        for (int i = 0; i < mapData.EncounterAreas.Count; i++)
        {
            if (mapData.EncounterAreas[i] != null)
            {
                return mapData.EncounterAreas[i];
            }
        }

        return null;
    }

    private int CountWalkableCells()
    {
        int count = 0;
        foreach (KeyValuePair<Vector3Int, RuntimeCell> pair in cells)
        {
            if (pair.Value.Walkable && !pair.Value.BlocksMovement)
            {
                count++;
            }
        }

        return count;
    }

    private Vector3 CellToWorld(Vector3Int cell)
    {
        return mapData.CellToWorld(cell);
    }

    private static string FormatCell(Vector3Int cell)
    {
        return "x:" + cell.x + " z:" + cell.z + " level:" + cell.y;
    }

    private static string FormatCellForName(Vector3Int cell)
    {
        return cell.x + "_" + cell.y + "_" + cell.z;
    }
}
