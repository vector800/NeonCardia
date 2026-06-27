using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class IsoMapEditorWindow : EditorWindow
{
    private const string WindowTitle = "2.5D Map Editor";
    private const string DataFolder = "Assets/NeonCardia/MapEditor2_5D/Data";
    private const string PreviewRootName = "IsoMapPreviewRoot";

    private static readonly string[] RotationLabels = { "0", "90", "180", "270" };
    private static readonly int[] RotationValues = { 0, 90, 180, 270 };

    private IsoMapData mapData;
    private IsoMapPrefabCatalog prefabCatalog;
    private Vector2 scrollPosition;
    private string selectedPrefabId;
    private string selectedInstanceId;
    private bool selectedInstanceIsTile;
    private int placementX;
    private int placementZ;
    private int placementLevel;
    private int rotationY;
    private List<string> validationMessages = new List<string>();

    [MenuItem("Tools/NeonCardia/2.5D Map Editor", false, 41080)]
    public static void Open()
    {
        IsoMapEditorWindow window = GetWindow<IsoMapEditorWindow>();
        window.titleContent = new GUIContent(WindowTitle);
        window.minSize = new Vector2(420f, 520f);
        window.Show();
    }

    [MenuItem("Tools/NeonCardia/Open 2.5D Map Editor", false, 41081)]
    public static void OpenFromAlternateMenu()
    {
        Open();
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        if (prefabCatalog == null)
        {
            prefabCatalog = AssetDatabase.LoadAssetAtPath<IsoMapPrefabCatalog>(IsoMapPlaceholderPrefabBuilder.CatalogAssetPath);
        }
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        DrawMapDataSection();
        EditorGUILayout.Space(6f);
        DrawCatalogSection();
        EditorGUILayout.Space(6f);
        DrawPlacementSection();
        EditorGUILayout.Space(6f);
        DrawPreviewSection();
        EditorGUILayout.Space(6f);
        DrawInstanceSection();
        EditorGUILayout.Space(6f);
        DrawValidationSection();
        EditorGUILayout.EndScrollView();
    }

    private void DrawMapDataSection()
    {
        EditorGUILayout.LabelField("Map Data", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        IsoMapData nextMapData = (IsoMapData)EditorGUILayout.ObjectField("IsoMapData", mapData, typeof(IsoMapData), false);
        if (nextMapData != mapData)
        {
            mapData = nextMapData;
            selectedInstanceId = null;
            ClampPlacementCell();
            RepaintSceneViews();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("New IsoMapData", GUILayout.Height(24f)))
        {
            CreateNewMapData();
        }

        EditorGUI.BeginDisabledGroup(mapData == null);
        if (GUILayout.Button("Save", GUILayout.Height(24f)))
        {
            SaveMapData();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        if (mapData != null)
        {
            DrawMapSettings();
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawMapSettings()
    {
        EditorGUI.BeginChangeCheck();
        string nextMapId = EditorGUILayout.TextField("Map Id", mapData.MapId);
        string nextDisplayName = EditorGUILayout.TextField("Display Name", mapData.DisplayName);
        int nextWidth = EditorGUILayout.IntField("Width", mapData.Width);
        int nextDepth = EditorGUILayout.IntField("Depth", mapData.Depth);
        float nextCellSize = EditorGUILayout.FloatField("Cell Size", mapData.CellSize);
        int nextMaxLevel = EditorGUILayout.IntField("Max Level", mapData.MaxLevel);
        Vector3Int currentSpawn = mapData.DefaultSpawnCell;
        int spawnX = EditorGUILayout.IntField("Default Spawn X", currentSpawn.x);
        int spawnZ = EditorGUILayout.IntField("Default Spawn Z", currentSpawn.z);
        int spawnLevel = EditorGUILayout.IntField("Default Spawn Level", currentSpawn.y);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(mapData, "Edit Iso Map Settings");
            mapData.MapId = nextMapId;
            mapData.DisplayName = nextDisplayName;
            mapData.Width = nextWidth;
            mapData.Depth = nextDepth;
            mapData.CellSize = nextCellSize;
            mapData.MaxLevel = nextMaxLevel;
            mapData.DefaultSpawnCell = new Vector3Int(spawnX, spawnLevel, spawnZ);
            EditorUtility.SetDirty(mapData);
            ClampPlacementCell();
            RepaintSceneViews();
        }
    }

    private void DrawCatalogSection()
    {
        EditorGUILayout.LabelField("Prefab Catalog", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        IsoMapPrefabCatalog nextCatalog = (IsoMapPrefabCatalog)EditorGUILayout.ObjectField("IsoMapPrefabCatalog", prefabCatalog, typeof(IsoMapPrefabCatalog), false);
        if (nextCatalog != prefabCatalog)
        {
            prefabCatalog = nextCatalog;
            if (prefabCatalog == null || prefabCatalog.FindEntry(selectedPrefabId) == null)
            {
                selectedPrefabId = null;
            }
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate Placeholder Prefabs/Catalog", GUILayout.Height(24f)))
        {
            IsoMapPlaceholderPrefabBuilder.GeneratePlaceholderPrefabsAndCatalog();
            prefabCatalog = AssetDatabase.LoadAssetAtPath<IsoMapPrefabCatalog>(IsoMapPlaceholderPrefabBuilder.CatalogAssetPath);
        }

        EditorGUI.BeginDisabledGroup(prefabCatalog == null);
        if (GUILayout.Button("Ping Catalog", GUILayout.Width(92f), GUILayout.Height(24f)))
        {
            EditorGUIUtility.PingObject(prefabCatalog);
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        DrawPrefabPalette();
        EditorGUILayout.EndVertical();
    }

    private void DrawPrefabPalette()
    {
        if (prefabCatalog == null)
        {
            EditorGUILayout.HelpBox("Assign an IsoMapPrefabCatalog.", MessageType.Info);
            return;
        }

        if (prefabCatalog.Entries.Count == 0)
        {
            EditorGUILayout.HelpBox("The catalog has no prefab entries.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("Prefab Palette", EditorStyles.boldLabel);
        for (int i = 0; i < prefabCatalog.Entries.Count; i++)
        {
            IsoMapPrefabCatalogEntry entry = prefabCatalog.Entries[i];
            if (entry == null)
            {
                continue;
            }

            EditorGUILayout.BeginHorizontal();
            Color previousColor = GUI.backgroundColor;
            if (string.Equals(selectedPrefabId, entry.PrefabId, StringComparison.Ordinal))
            {
                GUI.backgroundColor = new Color(0.55f, 0.85f, 1f, 1f);
            }

            if (GUILayout.Button(entry.PrefabId, GUILayout.Height(22f)))
            {
                selectedPrefabId = entry.PrefabId;
            }

            GUI.backgroundColor = previousColor;
            EditorGUILayout.LabelField(entry.Category.ToString(), GUILayout.Width(62f));
            EditorGUILayout.ObjectField(entry.Prefab, typeof(GameObject), false, GUILayout.Width(120f));
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.LabelField("Selected PrefabId", string.IsNullOrEmpty(selectedPrefabId) ? "(none)" : selectedPrefabId);
    }

    private void DrawPlacementSection()
    {
        EditorGUILayout.LabelField("Placement", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUI.BeginDisabledGroup(mapData == null);
        EditorGUI.BeginChangeCheck();
        placementX = EditorGUILayout.IntField("Cell X", placementX);
        placementZ = EditorGUILayout.IntField("Cell Z", placementZ);
        placementLevel = EditorGUILayout.IntField("Level", placementLevel);
        rotationY = EditorGUILayout.IntPopup("Rotation Y", rotationY, RotationLabels, RotationValues);
        if (EditorGUI.EndChangeCheck())
        {
            ClampPlacementCell();
            RepaintSceneViews();
        }

        Vector3Int cell = GetPlacementCell();
        EditorGUILayout.LabelField("Current Cell", FormatCell(cell));
        EditorGUILayout.HelpBox("SceneView: click selects a cell. Shift + click places the selected prefab.", MessageType.None);

        EditorGUI.BeginDisabledGroup(mapData == null || prefabCatalog == null || string.IsNullOrEmpty(selectedPrefabId));
        if (GUILayout.Button("Place Selected Prefab", GUILayout.Height(26f)))
        {
            PlaceSelectedPrefabAtCell(cell);
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(mapData == null);
        if (GUILayout.Button("Delete All Instances At Cell", GUILayout.Height(24f)))
        {
            DeleteInstancesAtCell(cell);
        }
        EditorGUI.EndDisabledGroup();
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();
    }

    private void DrawPreviewSection()
    {
        EditorGUILayout.LabelField("Scene Preview", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginDisabledGroup(mapData == null || prefabCatalog == null);
        if (GUILayout.Button("Refresh Preview", GUILayout.Height(26f)))
        {
            RefreshPreview();
        }
        EditorGUI.EndDisabledGroup();

        if (GUILayout.Button("Clear Preview", GUILayout.Height(26f)))
        {
            ClearPreview();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Preview Root", PreviewRootName);
        EditorGUILayout.EndVertical();
    }

    private void DrawInstanceSection()
    {
        EditorGUILayout.LabelField("Placed Instances", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if (mapData == null)
        {
            EditorGUILayout.HelpBox("Assign or create an IsoMapData asset.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        DrawSelectedInstanceActions();
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Tiles (" + mapData.TileInstances.Count + ")", EditorStyles.boldLabel);
        for (int i = 0; i < mapData.TileInstances.Count; i++)
        {
            DrawTileInstanceRow(mapData.TileInstances[i]);
        }

        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Props / Markers (" + mapData.PropInstances.Count + ")", EditorStyles.boldLabel);
        for (int i = 0; i < mapData.PropInstances.Count; i++)
        {
            DrawPropInstanceRow(mapData.PropInstances[i]);
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawSelectedInstanceActions()
    {
        if (string.IsNullOrEmpty(selectedInstanceId))
        {
            EditorGUILayout.LabelField("Selected Instance", "(none)");
            return;
        }

        EditorGUILayout.LabelField("Selected Instance", selectedInstanceId);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Rotate +90", GUILayout.Height(22f)))
        {
            RotateSelectedInstance();
        }

        if (GUILayout.Button("Delete Selected", GUILayout.Height(22f)))
        {
            DeleteSelectedInstance();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawTileInstanceRow(IsoMapTileInstanceData instance)
    {
        if (instance == null)
        {
            return;
        }

        EditorGUILayout.BeginHorizontal();
        DrawInstanceSelectButton(instance.InstanceId, true);
        EditorGUILayout.LabelField(instance.PrefabId + "  " + FormatCell(instance.Cell) + "  R" + instance.RotationY, GUILayout.MinWidth(220f));
        if (GUILayout.Button("R+90", GUILayout.Width(48f)))
        {
            RotateTileInstance(instance);
        }

        if (GUILayout.Button("Delete", GUILayout.Width(58f)))
        {
            DeleteTileInstance(instance);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPropInstanceRow(IsoMapPropInstanceData instance)
    {
        if (instance == null)
        {
            return;
        }

        EditorGUILayout.BeginHorizontal();
        DrawInstanceSelectButton(instance.InstanceId, false);
        EditorGUILayout.LabelField(instance.PrefabId + "  " + FormatCell(instance.Cell) + "  R" + instance.RotationY, GUILayout.MinWidth(220f));
        if (GUILayout.Button("R+90", GUILayout.Width(48f)))
        {
            RotatePropInstance(instance);
        }

        if (GUILayout.Button("Delete", GUILayout.Width(58f)))
        {
            DeletePropInstance(instance);
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawInstanceSelectButton(string instanceId, bool isTile)
    {
        Color previousColor = GUI.backgroundColor;
        if (string.Equals(selectedInstanceId, instanceId, StringComparison.Ordinal))
        {
            GUI.backgroundColor = new Color(0.55f, 0.85f, 1f, 1f);
        }

        if (GUILayout.Button("Select", GUILayout.Width(58f)))
        {
            selectedInstanceId = instanceId;
            selectedInstanceIsTile = isTile;
        }

        GUI.backgroundColor = previousColor;
    }

    private void DrawValidationSection()
    {
        EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUI.BeginDisabledGroup(mapData == null);
        if (GUILayout.Button("Validate", GUILayout.Height(26f)))
        {
            ValidateMapData();
        }
        EditorGUI.EndDisabledGroup();

        for (int i = 0; i < validationMessages.Count; i++)
        {
            EditorGUILayout.HelpBox(validationMessages[i], validationMessages[i].StartsWith("OK", StringComparison.Ordinal) ? MessageType.Info : MessageType.Warning);
        }
        EditorGUILayout.EndVertical();
    }

    private void CreateNewMapData()
    {
        EnsureFolderPath(DataFolder);
        string path = EditorUtility.SaveFilePanelInProject("Create IsoMapData", "IsoMapData.asset", "asset", "Create a new IsoMapData asset.", DataFolder);
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        path = AssetDatabase.GenerateUniqueAssetPath(path);
        IsoMapData asset = CreateInstance<IsoMapData>();
        string assetName = Path.GetFileNameWithoutExtension(path);
        asset.MapId = assetName;
        asset.DisplayName = assetName;
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        mapData = asset;
        EditorGUIUtility.PingObject(mapData);
        RepaintSceneViews();
    }

    private void SaveMapData()
    {
        if (mapData == null)
        {
            return;
        }

        EditorUtility.SetDirty(mapData);
        AssetDatabase.SaveAssets();
        Debug.Log("Saved IsoMapData: " + AssetDatabase.GetAssetPath(mapData));
    }

    private void PlaceSelectedPrefabAtCell(Vector3Int cell)
    {
        if (mapData == null || prefabCatalog == null)
        {
            return;
        }

        IsoMapPrefabCatalogEntry entry = prefabCatalog.FindEntry(selectedPrefabId);
        if (entry == null || entry.Prefab == null)
        {
            Debug.LogWarning("Selected prefabId is missing from IsoMapPrefabCatalog: " + selectedPrefabId);
            return;
        }

        if (!mapData.IsInsideBounds(cell))
        {
            Debug.LogWarning("Cell is outside map bounds: " + FormatCell(cell));
            return;
        }

        if (entry.Category == IsoMapPrefabCategory.Tile && HasSameTileAtCell(cell, entry.PrefabId))
        {
            Debug.LogWarning("Skipped duplicate tile placement at " + FormatCell(cell) + ": " + entry.PrefabId);
            return;
        }

        Undo.RecordObject(mapData, "Place Iso Map Prefab");
        if (entry.Category == IsoMapPrefabCategory.Tile)
        {
            mapData.TileInstances.Add(new IsoMapTileInstanceData(NewInstanceId(entry.PrefabId), entry.PrefabId, cell, rotationY, entry.Walkable, string.Empty));
        }
        else
        {
            mapData.PropInstances.Add(new IsoMapPropInstanceData(NewInstanceId(entry.PrefabId), entry.PrefabId, cell, entry.DefaultOffset, rotationY, entry.BlocksMovement));
        }

        EditorUtility.SetDirty(mapData);
        RefreshPreviewIfPresent();
    }

    private void DeleteInstancesAtCell(Vector3Int cell)
    {
        if (mapData == null)
        {
            return;
        }

        Undo.RecordObject(mapData, "Delete Iso Map Cell Instances");
        int removed = mapData.TileInstances.RemoveAll(instance => instance != null && instance.Cell == cell);
        removed += mapData.PropInstances.RemoveAll(instance => instance != null && instance.Cell == cell);
        if (removed > 0)
        {
            selectedInstanceId = null;
            EditorUtility.SetDirty(mapData);
            RefreshPreviewIfPresent();
        }
    }

    private void RotateSelectedInstance()
    {
        if (mapData == null || string.IsNullOrEmpty(selectedInstanceId))
        {
            return;
        }

        if (selectedInstanceIsTile)
        {
            IsoMapTileInstanceData tile = FindTileInstance(selectedInstanceId);
            if (tile != null)
            {
                RotateTileInstance(tile);
            }
        }
        else
        {
            IsoMapPropInstanceData prop = FindPropInstance(selectedInstanceId);
            if (prop != null)
            {
                RotatePropInstance(prop);
            }
        }
    }

    private void DeleteSelectedInstance()
    {
        if (mapData == null || string.IsNullOrEmpty(selectedInstanceId))
        {
            return;
        }

        if (selectedInstanceIsTile)
        {
            IsoMapTileInstanceData tile = FindTileInstance(selectedInstanceId);
            if (tile != null)
            {
                DeleteTileInstance(tile);
            }
        }
        else
        {
            IsoMapPropInstanceData prop = FindPropInstance(selectedInstanceId);
            if (prop != null)
            {
                DeletePropInstance(prop);
            }
        }
    }

    private void RotateTileInstance(IsoMapTileInstanceData instance)
    {
        Undo.RecordObject(mapData, "Rotate Iso Map Tile");
        instance.RotationY = instance.RotationY + 90;
        EditorUtility.SetDirty(mapData);
        RefreshPreviewIfPresent();
    }

    private void RotatePropInstance(IsoMapPropInstanceData instance)
    {
        Undo.RecordObject(mapData, "Rotate Iso Map Prop");
        instance.RotationY = instance.RotationY + 90;
        EditorUtility.SetDirty(mapData);
        RefreshPreviewIfPresent();
    }

    private void DeleteTileInstance(IsoMapTileInstanceData instance)
    {
        Undo.RecordObject(mapData, "Delete Iso Map Tile");
        mapData.TileInstances.Remove(instance);
        if (string.Equals(selectedInstanceId, instance.InstanceId, StringComparison.Ordinal))
        {
            selectedInstanceId = null;
        }

        EditorUtility.SetDirty(mapData);
        RefreshPreviewIfPresent();
    }

    private void DeletePropInstance(IsoMapPropInstanceData instance)
    {
        Undo.RecordObject(mapData, "Delete Iso Map Prop");
        mapData.PropInstances.Remove(instance);
        if (string.Equals(selectedInstanceId, instance.InstanceId, StringComparison.Ordinal))
        {
            selectedInstanceId = null;
        }

        EditorUtility.SetDirty(mapData);
        RefreshPreviewIfPresent();
    }

    private void RefreshPreview()
    {
        if (mapData == null || prefabCatalog == null)
        {
            return;
        }

        GameObject root = FindOrCreatePreviewRoot();
        ClearPreviewChildren(root);
        for (int i = 0; i < mapData.TileInstances.Count; i++)
        {
            IsoMapTileInstanceData instance = mapData.TileInstances[i];
            if (instance != null)
            {
                CreatePreviewInstance(root.transform, instance.PrefabId, instance.InstanceId, instance.Cell, Vector3.zero, instance.RotationY, "Tile");
            }
        }

        for (int i = 0; i < mapData.PropInstances.Count; i++)
        {
            IsoMapPropInstanceData instance = mapData.PropInstances[i];
            if (instance != null)
            {
                CreatePreviewInstance(root.transform, instance.PrefabId, instance.InstanceId, instance.Cell, instance.Offset, instance.RotationY, "Prop");
            }
        }

        Selection.activeGameObject = root;
        RepaintSceneViews();
    }

    private void ClearPreview()
    {
        GameObject root = GameObject.Find(PreviewRootName);
        if (root != null)
        {
            Undo.DestroyObjectImmediate(root);
            RepaintSceneViews();
        }
    }

    private void RefreshPreviewIfPresent()
    {
        if (GameObject.Find(PreviewRootName) != null)
        {
            RefreshPreview();
        }
    }

    private GameObject FindOrCreatePreviewRoot()
    {
        GameObject root = GameObject.Find(PreviewRootName);
        if (root == null)
        {
            root = new GameObject(PreviewRootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Iso Map Preview Root");
        }

        return root;
    }

    private void ClearPreviewChildren(GameObject root)
    {
        for (int i = root.transform.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.DestroyImmediate(root.transform.GetChild(i).gameObject);
        }
    }

    private void CreatePreviewInstance(Transform parent, string prefabId, string instanceId, Vector3Int cell, Vector3 offset, int instanceRotationY, string labelPrefix)
    {
        GameObject prefab;
        if (!prefabCatalog.TryGetPrefab(prefabId, out prefab))
        {
            Debug.LogWarning("Preview skipped missing prefabId: " + prefabId);
            return;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (instance == null)
        {
            instance = Instantiate(prefab);
        }

        instance.name = labelPrefix + "_" + prefabId + "_" + instanceId;
        instance.transform.SetParent(parent, false);
        instance.transform.position = mapData.CellToWorld(cell) + offset;
        instance.transform.rotation = Quaternion.Euler(0f, IsoMapData.NormalizeRotationY(instanceRotationY), 0f);
    }

    private void ValidateMapData()
    {
        validationMessages.Clear();
        int warningCount = 0;

        if (mapData == null)
        {
            AddValidationWarning("IsoMapData is not assigned.", ref warningCount);
            return;
        }

        if (string.IsNullOrWhiteSpace(mapData.MapId))
        {
            AddValidationWarning("mapId is empty.", ref warningCount);
        }

        if (mapData.Width <= 0 || mapData.Depth <= 0 || mapData.CellSize <= 0f || mapData.MaxLevel < 0)
        {
            AddValidationWarning("width/depth/cellSize/maxLevel contains an invalid value.", ref warningCount);
        }

        HashSet<string> tileKeys = new HashSet<string>();
        for (int i = 0; i < mapData.TileInstances.Count; i++)
        {
            IsoMapTileInstanceData tile = mapData.TileInstances[i];
            if (tile == null)
            {
                continue;
            }

            ValidatePrefabId(tile.PrefabId, ref warningCount);
            if (!mapData.IsInsideBounds(tile.Cell))
            {
                AddValidationWarning("Tile is outside map bounds: " + tile.PrefabId + " " + FormatCell(tile.Cell), ref warningCount);
            }

            string key = FormatCell(tile.Cell) + "|" + tile.PrefabId;
            if (!tileKeys.Add(key))
            {
                AddValidationWarning("Duplicate tile with the same prefabId at the same cell: " + key, ref warningCount);
            }
        }

        for (int i = 0; i < mapData.PropInstances.Count; i++)
        {
            IsoMapPropInstanceData prop = mapData.PropInstances[i];
            if (prop == null)
            {
                continue;
            }

            ValidatePrefabId(prop.PrefabId, ref warningCount);
            if (!mapData.IsInsideBounds(prop.Cell))
            {
                AddValidationWarning("Prop/Marker is outside map bounds: " + prop.PrefabId + " " + FormatCell(prop.Cell), ref warningCount);
            }
        }

        if (warningCount == 0)
        {
            validationMessages.Add("OK: validation passed.");
            Debug.Log("IsoMapData validation passed: " + mapData.MapId);
        }
        else
        {
            Debug.LogWarning("IsoMapData validation found " + warningCount + " issue(s): " + mapData.MapId);
        }
    }

    private void ValidatePrefabId(string prefabId, ref int warningCount)
    {
        if (prefabCatalog == null || !prefabCatalog.ContainsPrefabId(prefabId))
        {
            AddValidationWarning("prefabId is missing from the catalog: " + prefabId, ref warningCount);
        }
    }

    private void AddValidationWarning(string message, ref int warningCount)
    {
        warningCount++;
        validationMessages.Add(message);
    }

    private bool HasSameTileAtCell(Vector3Int cell, string prefabId)
    {
        for (int i = 0; i < mapData.TileInstances.Count; i++)
        {
            IsoMapTileInstanceData instance = mapData.TileInstances[i];
            if (instance != null && instance.Cell == cell && string.Equals(instance.PrefabId, prefabId, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private IsoMapTileInstanceData FindTileInstance(string instanceId)
    {
        for (int i = 0; i < mapData.TileInstances.Count; i++)
        {
            IsoMapTileInstanceData instance = mapData.TileInstances[i];
            if (instance != null && string.Equals(instance.InstanceId, instanceId, StringComparison.Ordinal))
            {
                return instance;
            }
        }

        return null;
    }

    private IsoMapPropInstanceData FindPropInstance(string instanceId)
    {
        for (int i = 0; i < mapData.PropInstances.Count; i++)
        {
            IsoMapPropInstanceData instance = mapData.PropInstances[i];
            if (instance != null && string.Equals(instance.InstanceId, instanceId, StringComparison.Ordinal))
            {
                return instance;
            }
        }

        return null;
    }

    private Vector3Int GetPlacementCell()
    {
        return new Vector3Int(placementX, placementLevel, placementZ);
    }

    private void ClampPlacementCell()
    {
        if (mapData == null)
        {
            placementX = Mathf.Max(0, placementX);
            placementZ = Mathf.Max(0, placementZ);
            placementLevel = Mathf.Max(0, placementLevel);
            return;
        }

        placementX = Mathf.Clamp(placementX, 0, Mathf.Max(0, mapData.Width - 1));
        placementZ = Mathf.Clamp(placementZ, 0, Mathf.Max(0, mapData.Depth - 1));
        placementLevel = Mathf.Clamp(placementLevel, 0, mapData.MaxLevel);
    }

    private string NewInstanceId(string prefabId)
    {
        return prefabId + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    private static string FormatCell(Vector3Int cell)
    {
        return "x:" + cell.x + " z:" + cell.z + " level:" + cell.y;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (mapData == null)
        {
            return;
        }

        DrawSceneGrid();
        HandleSceneViewInput(sceneView);
    }

    private void DrawSceneGrid()
    {
        float size = mapData.CellSize;
        float y = placementLevel * size + 0.012f;
        float xMin = -0.5f * size;
        float zMin = -0.5f * size;
        float xMax = (mapData.Width - 0.5f) * size;
        float zMax = (mapData.Depth - 0.5f) * size;

        Color previousColor = Handles.color;
        Handles.color = new Color(0.18f, 0.80f, 1f, 0.36f);
        for (int x = 0; x <= mapData.Width; x++)
        {
            float worldX = (x - 0.5f) * size;
            Handles.DrawLine(new Vector3(worldX, y, zMin), new Vector3(worldX, y, zMax));
        }

        for (int z = 0; z <= mapData.Depth; z++)
        {
            float worldZ = (z - 0.5f) * size;
            Handles.DrawLine(new Vector3(xMin, y, worldZ), new Vector3(xMax, y, worldZ));
        }

        DrawSelectedCellHandle(y);
        Handles.color = previousColor;
    }

    private void DrawSelectedCellHandle(float y)
    {
        Vector3Int cell = GetPlacementCell();
        Vector3 center = mapData.CellToWorld(cell);
        float half = mapData.CellSize * 0.5f;
        Vector3[] corners =
        {
            new Vector3(center.x - half, y, center.z - half),
            new Vector3(center.x - half, y, center.z + half),
            new Vector3(center.x + half, y, center.z + half),
            new Vector3(center.x + half, y, center.z - half)
        };

        Handles.DrawSolidRectangleWithOutline(corners, new Color(1f, 0.86f, 0.18f, 0.12f), new Color(1f, 0.86f, 0.18f, 0.95f));
        Handles.Label(center + new Vector3(0f, 0.22f, 0f), FormatCell(cell));
    }

    private void HandleSceneViewInput(SceneView sceneView)
    {
        if (focusedWindow != this)
        {
            return;
        }

        Event current = Event.current;
        if (current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            return;
        }

        if (current.type != EventType.MouseDown || current.button != 0 || current.alt)
        {
            return;
        }

        Vector3Int cell;
        if (!TryGetCellFromSceneMouse(current.mousePosition, out cell))
        {
            return;
        }

        placementX = cell.x;
        placementZ = cell.z;
        placementLevel = cell.y;
        Repaint();
        sceneView.Repaint();

        if (current.shift)
        {
            PlaceSelectedPrefabAtCell(cell);
        }

        current.Use();
    }

    private bool TryGetCellFromSceneMouse(Vector2 mousePosition, out Vector3Int cell)
    {
        cell = Vector3Int.zero;
        float levelY = placementLevel * mapData.CellSize;
        Plane plane = new Plane(Vector3.up, new Vector3(0f, levelY, 0f));
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        float enter;
        if (!plane.Raycast(ray, out enter))
        {
            return false;
        }

        Vector3 point = ray.GetPoint(enter);
        int x = Mathf.FloorToInt(point.x / mapData.CellSize + 0.5f);
        int z = Mathf.FloorToInt(point.z / mapData.CellSize + 0.5f);
        cell = new Vector3Int(x, placementLevel, z);
        return mapData.IsInsideBounds(cell);
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

    private static void RepaintSceneViews()
    {
        SceneView.RepaintAll();
    }
}
