using UnityEditor;
using UnityEngine;

public static class IsoMapSampleMapBuilder
{
    public const string SampleMapAssetPath = "Assets/NeonCardia/MapEditor2_5D/Data/SF_Floating_Test.asset";

    private const string DataFolder = "Assets/NeonCardia/MapEditor2_5D/Data";

    [MenuItem("Tools/NeonCardia/Map Editor 2.5D/Create Sample SF Floating Map", false, 41082)]
    public static void CreateSampleSfFloatingMapFromMenu()
    {
        CreateOrUpdateSampleSfFloatingMap();
    }

    public static IsoMapData CreateOrUpdateSampleSfFloatingMap()
    {
        EnsureFolderPath(DataFolder);
        EnsurePlaceholderCatalog();

        IsoMapData mapData = AssetDatabase.LoadAssetAtPath<IsoMapData>(SampleMapAssetPath);
        if (mapData == null)
        {
            mapData = ScriptableObject.CreateInstance<IsoMapData>();
            AssetDatabase.CreateAsset(mapData, SampleMapAssetPath);
        }

        Undo.RecordObject(mapData, "Create Sample SF Floating Map");
        mapData.MapId = "SF_Floating_Test";
        mapData.DisplayName = "SF Floating Test";
        mapData.Width = 8;
        mapData.Depth = 8;
        mapData.CellSize = 1f;
        mapData.MaxLevel = 1;
        mapData.DefaultSpawnCell = new Vector3Int(3, 0, 3);
        mapData.TileInstances.Clear();
        mapData.PropInstances.Clear();
        mapData.EncounterAreas.Clear();
        mapData.Transitions.Clear();

        int index = 0;
        for (int z = 2; z <= 4; z++)
        {
            for (int x = 2; x <= 4; x++)
            {
                Vector3Int cell = new Vector3Int(x, 0, z);
                if (cell == new Vector3Int(4, 0, 4))
                {
                    AddTile(mapData, "Stair_Up", cell, 90, ref index);
                }
                else
                {
                    AddTile(mapData, "PlatformTile_1x1", cell, 0, ref index);
                }
            }
        }

        AddTile(mapData, "Bridge_1x1", new Vector3Int(1, 0, 3), 0, ref index);
        AddTile(mapData, "Bridge_1x1", new Vector3Int(5, 0, 3), 0, ref index);
        AddTile(mapData, "Bridge_1x1", new Vector3Int(6, 0, 3), 0, ref index);
        AddTile(mapData, "PlatformTile_1x1", new Vector3Int(5, 1, 4), 0, ref index);
        AddTile(mapData, "PlatformTile_1x1", new Vector3Int(6, 1, 4), 0, ref index);

        AddProp(mapData, "SpawnMarker", new Vector3Int(3, 0, 3), 0, false, ref index);
        AddProp(mapData, "TransitionMarker", new Vector3Int(6, 1, 4), 0, false, ref index);
        AddProp(mapData, "Railing", new Vector3Int(2, 0, 2), 0, true, ref index);
        AddProp(mapData, "Railing", new Vector3Int(4, 0, 2), 0, true, ref index);
        AddProp(mapData, "Wall", new Vector3Int(2, 0, 5), 0, true, ref index);
        AddProp(mapData, "Wall", new Vector3Int(5, 1, 5), 0, true, ref index);

        IsoMapEncounterAreaData encounterArea = new IsoMapEncounterAreaData();
        encounterArea.EncounterAreaId = "sf_float_patrol_01";
        encounterArea.EnemyGroupTableId = "sf_floating_test_group";
        encounterArea.BattleBackgroundId = "sf_floating_test_bg";
        encounterArea.BattleBgmId = "sf_floating_test_bgm";
        encounterArea.MinStepsBeforeEncounter = 2;
        encounterArea.EncounterChancePerStep = 0.35f;
        encounterArea.CooldownStepsAfterBattle = 5;
        encounterArea.Cells.Add(new Vector3Int(5, 0, 3));
        encounterArea.Cells.Add(new Vector3Int(6, 0, 3));
        encounterArea.Cells.Add(new Vector3Int(5, 1, 4));
        encounterArea.Cells.Add(new Vector3Int(6, 1, 4));
        mapData.EncounterAreas.Add(encounterArea);

        IsoMapTransitionData transition = new IsoMapTransitionData();
        transition.TransitionId = "sample_transition_stub";
        transition.Cell = new Vector3Int(6, 1, 4);
        transition.TargetMapId = "UNASSIGNED";
        transition.TargetSpawnId = "default";
        transition.RequiredFlag = string.Empty;
        mapData.Transitions.Add(transition);

        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = mapData;
        EditorGUIUtility.PingObject(mapData);
        Debug.Log("Created sample SF floating IsoMapData: " + SampleMapAssetPath);
        return mapData;
    }

    private static void AddTile(IsoMapData mapData, string prefabId, Vector3Int cell, int rotationY, ref int index)
    {
        mapData.TileInstances.Add(new IsoMapTileInstanceData(
            prefabId + "_sample_" + index.ToString("00"),
            prefabId,
            cell,
            rotationY,
            true,
            string.Empty));
        index++;
    }

    private static void AddProp(IsoMapData mapData, string prefabId, Vector3Int cell, int rotationY, bool blocksMovement, ref int index)
    {
        Vector3 offset = Vector3.zero;
        IsoMapPrefabCatalog catalog = AssetDatabase.LoadAssetAtPath<IsoMapPrefabCatalog>(IsoMapPlaceholderPrefabBuilder.CatalogAssetPath);
        if (catalog != null)
        {
            IsoMapPrefabCatalogEntry entry = catalog.FindEntry(prefabId);
            if (entry != null)
            {
                offset = entry.DefaultOffset;
                blocksMovement = blocksMovement || entry.BlocksMovement;
            }
        }

        mapData.PropInstances.Add(new IsoMapPropInstanceData(
            prefabId + "_sample_" + index.ToString("00"),
            prefabId,
            cell,
            offset,
            rotationY,
            blocksMovement));
        index++;
    }

    private static void EnsurePlaceholderCatalog()
    {
        IsoMapPrefabCatalog catalog = AssetDatabase.LoadAssetAtPath<IsoMapPrefabCatalog>(IsoMapPlaceholderPrefabBuilder.CatalogAssetPath);
        if (catalog == null
            || !catalog.ContainsPrefabId("PlatformTile_1x1")
            || !catalog.ContainsPrefabId("Bridge_1x1")
            || !catalog.ContainsPrefabId("Stair_Up")
            || !catalog.ContainsPrefabId("EncounterAreaMarker"))
        {
            IsoMapPlaceholderPrefabBuilder.GeneratePlaceholderPrefabsAndCatalog();
        }
    }

    private static void EnsureFolderPath(string path)
    {
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}
