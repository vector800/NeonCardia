using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class IsoMapEditorWindow : EditorWindow
{
    private const string WindowTitle = "2.5D Map Editor";
    private const string DataFolder = "Assets/NeonCardia/MapEditor2_5D/Data";
    private const string PreviewRootName = "IsoMapPreviewRoot";

    private static readonly string[] RotationLabels = { "0", "90", "180", "270" };
    private static readonly int[] RotationValues = { 0, 90, 180, 270 };
    private static readonly string[] PlaytestDirectionLabels = { "Up", "Right", "Down", "Left" };
    private static readonly Vector2Int[] PlaytestDirectionValues = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

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
    private int selectedEncounterAreaIndex = -1;
    private int playtestDirectionIndex = 2;
    private bool playtestReturnToPreviousScene;
    private string playtestStatusMessage;
    private MessageType playtestStatusType = MessageType.None;
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
        DrawPlaytestSection();
        EditorGUILayout.Space(6f);
        DrawEncounterAreaSection();
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
            ClearPlaytestStatus();
            RepaintSceneViews();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("New IsoMapData", GUILayout.Height(24f)))
        {
            CreateNewMapData();
        }

        if (GUILayout.Button("Create Sample SF Floating Map", GUILayout.Height(24f)))
        {
            mapData = IsoMapSampleMapBuilder.CreateOrUpdateSampleSfFloatingMap();
            selectedInstanceId = null;
            selectedEncounterAreaIndex = mapData != null && mapData.EncounterAreas.Count > 0 ? 0 : -1;
            ClampPlacementCell();
            RepaintSceneViews();
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
            ClearPlaytestStatus();
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
            ClearPlaytestStatus();
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

    private void DrawPlaytestSection()
    {
        EditorGUILayout.LabelField("Play From Here", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        Vector3Int selectedCell = GetPlacementCell();
        SelectedCellStatus selectedCellStatus = GetSelectedCellStatus(selectedCell);
        EditorGUILayout.LabelField("Selected Cell", FormatCell(selectedCell));
        if (mapData != null)
        {
            EditorGUILayout.LabelField("Default Spawn", FormatCell(mapData.DefaultSpawnCell));
        }
        else
        {
            EditorGUILayout.HelpBox("Assign or create an IsoMapData asset before playtesting.", MessageType.Info);
        }

        EditorGUI.BeginDisabledGroup(mapData == null);
        if (GUILayout.Button("Frame Map In Scene View", GUILayout.Height(24f)))
        {
            FrameMapInSceneView();
        }

        if (GUILayout.Button("Select Cell From Fields", GUILayout.Height(24f)))
        {
            SelectCellFromFields();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select Default Spawn", GUILayout.Height(24f)))
        {
            SelectDefaultSpawnCell();
        }

        if (GUILayout.Button("Set Selected As Spawn", GUILayout.Height(24f)))
        {
            SetSelectedCellAsDefaultSpawn();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();

        playtestDirectionIndex = Mathf.Clamp(playtestDirectionIndex, 0, PlaytestDirectionValues.Length - 1);
        playtestDirectionIndex = EditorGUILayout.Popup("Start Direction", playtestDirectionIndex, PlaytestDirectionLabels);
        playtestReturnToPreviousScene = EditorGUILayout.Toggle("Return To Previous Scene", playtestReturnToPreviousScene);

        bool playtestDisabled = mapData == null || prefabCatalog == null || EditorApplication.isPlayingOrWillChangePlaymode;
        bool selectedCellPlaytestDisabled = playtestDisabled || selectedCellStatus != SelectedCellStatus.Walkable;
        EditorGUILayout.HelpBox("Selected Cell Status: " + GetSelectedCellStatusLabel(selectedCellStatus), GetSelectedCellStatusMessageType(selectedCellStatus));

        EditorGUI.BeginDisabledGroup(playtestDisabled);
        if (GUILayout.Button("Play From Default Spawn", GUILayout.Height(28f)))
        {
            StartMapPlaytest(mapData.DefaultSpawnCell, string.Empty, "Default Spawn");
        }
        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(selectedCellPlaytestDisabled);
        if (GUILayout.Button("Play From Selected Cell", GUILayout.Height(28f)))
        {
            StartMapPlaytest(selectedCell, string.Empty, "Selected Cell");
        }
        EditorGUI.EndDisabledGroup();

        IsoMapEncounterAreaData selectedArea = GetSelectedEncounterArea();
        EditorGUI.BeginDisabledGroup(playtestDisabled || selectedArea == null);
        if (GUILayout.Button("Play From Selected Encounter Area", GUILayout.Height(26f)))
        {
            PlayFromSelectedEncounterArea(selectedArea);
        }
        EditorGUI.EndDisabledGroup();

        if (!string.IsNullOrEmpty(playtestStatusMessage))
        {
            EditorGUILayout.HelpBox(playtestStatusMessage, playtestStatusType);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawEncounterAreaSection()
    {
        EditorGUILayout.LabelField("Encounter Areas", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        if (mapData == null)
        {
            EditorGUILayout.HelpBox("Assign or create an IsoMapData asset.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add EncounterArea", GUILayout.Height(24f)))
        {
            AddEncounterArea();
        }

        EditorGUI.BeginDisabledGroup(GetSelectedEncounterArea() == null);
        if (GUILayout.Button("Delete Selected Area", GUILayout.Height(24f)))
        {
            DeleteSelectedEncounterArea();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        if (mapData.EncounterAreas.Count == 0)
        {
            EditorGUILayout.HelpBox("No EncounterArea. Add one, then add selected cells.", MessageType.Info);
            EditorGUILayout.EndVertical();
            return;
        }

        selectedEncounterAreaIndex = Mathf.Clamp(selectedEncounterAreaIndex, 0, mapData.EncounterAreas.Count - 1);
        DrawEncounterAreaList();
        EditorGUILayout.Space(4f);
        DrawSelectedEncounterAreaEditor();
        EditorGUILayout.EndVertical();
    }

    private void DrawEncounterAreaList()
    {
        for (int i = 0; i < mapData.EncounterAreas.Count; i++)
        {
            IsoMapEncounterAreaData area = mapData.EncounterAreas[i];
            if (area == null)
            {
                continue;
            }

            EditorGUILayout.BeginHorizontal();
            Color previousColor = GUI.backgroundColor;
            if (i == selectedEncounterAreaIndex)
            {
                GUI.backgroundColor = new Color(1f, 0.58f, 0.36f, 1f);
            }

            string label = string.IsNullOrEmpty(area.EncounterAreaId) ? "(empty id)" : area.EncounterAreaId;
            if (GUILayout.Button(label, GUILayout.Height(22f)))
            {
                selectedEncounterAreaIndex = i;
                RepaintSceneViews();
            }

            GUI.backgroundColor = previousColor;
            EditorGUILayout.LabelField(area.Cells.Count + " cells", GUILayout.Width(72f));
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawSelectedEncounterAreaEditor()
    {
        IsoMapEncounterAreaData area = GetSelectedEncounterArea();
        if (area == null)
        {
            return;
        }

        EditorGUI.BeginChangeCheck();
        string nextAreaId = EditorGUILayout.TextField("Area Id", area.EncounterAreaId);
        string nextEnemyGroupTableId = EditorGUILayout.TextField("Enemy Group Table Id", area.EnemyGroupTableId);
        string nextBattleBackgroundId = EditorGUILayout.TextField("Battle Background Id", area.BattleBackgroundId);
        string nextBattleBgmId = EditorGUILayout.TextField("Battle BGM Id", area.BattleBgmId);
        int nextMinSteps = EditorGUILayout.IntField("Min Steps Before Encounter", area.MinStepsBeforeEncounter);
        float nextChance = EditorGUILayout.Slider("Encounter Chance Per Step", area.EncounterChancePerStep, 0f, 1f);
        int nextCooldownSteps = EditorGUILayout.IntField("Cooldown Steps After Battle", area.CooldownStepsAfterBattle);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(mapData, "Edit Encounter Area");
            area.EncounterAreaId = nextAreaId;
            area.EnemyGroupTableId = nextEnemyGroupTableId;
            area.BattleBackgroundId = nextBattleBackgroundId;
            area.BattleBgmId = nextBattleBgmId;
            area.MinStepsBeforeEncounter = nextMinSteps;
            area.EncounterChancePerStep = nextChance;
            area.CooldownStepsAfterBattle = nextCooldownSteps;
            EditorUtility.SetDirty(mapData);
            RepaintSceneViews();
        }

        Vector3Int cell = GetPlacementCell();
        EditorGUILayout.LabelField("Selected Cell", FormatCell(cell));
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Selected Cell", GUILayout.Height(24f)))
        {
            AddCellToSelectedEncounterArea(cell);
        }

        if (GUILayout.Button("Remove Selected Cell", GUILayout.Height(24f)))
        {
            RemoveCellFromSelectedEncounterArea(cell);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("Cells", EditorStyles.boldLabel);
        for (int i = 0; i < area.Cells.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(FormatCell(area.Cells[i]), GUILayout.MinWidth(180f));
            if (GUILayout.Button("Remove", GUILayout.Width(70f)))
            {
                RemoveCellFromSelectedEncounterAreaAtIndex(area, i);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }
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

    private enum SelectedCellStatus
    {
        Walkable,
        NotFound,
        Blocked,
        NoIsoMapData,
        NoPrefabCatalog
    }

    private void FrameMapInSceneView()
    {
        if (mapData == null)
        {
            SetPlaytestStatus("Cannot frame SceneView because IsoMapData is not assigned.", MessageType.Warning);
            return;
        }

        SceneView sceneView = GetTargetSceneView();
        if (sceneView == null)
        {
            SetPlaytestStatus("No SceneView is available to frame the map.", MessageType.Warning);
            return;
        }

        Bounds mapBounds = GetMapWorldBounds();
        Quaternion rotation = Quaternion.Euler(35f, 45f, 0f);
        float viewSize = Mathf.Max(4f, Mathf.Max(mapBounds.size.x, mapBounds.size.z) * 0.9f + mapBounds.size.y * 0.35f);

        sceneView.in2DMode = false;
        sceneView.LookAt(mapBounds.center, rotation, viewSize, false, true);
        sceneView.Repaint();

        SetPlaytestStatus("SceneView framed for map: " + mapData.MapId, MessageType.Info);
    }

    private void SelectCellFromFields()
    {
        Vector3Int requestedCell = new Vector3Int(placementX, placementLevel, placementZ);
        SetPlacementCell(requestedCell);
        SelectedCellStatus status = GetSelectedCellStatus(GetPlacementCell());
        SetPlaytestStatus("Selected cell set from fields: " + FormatCell(GetPlacementCell())
            + "\nSelected Cell Status: " + GetSelectedCellStatusLabel(status), GetSelectedCellStatusMessageType(status));
    }

    private void SelectDefaultSpawnCell()
    {
        if (mapData == null)
        {
            return;
        }

        SetPlacementCell(mapData.DefaultSpawnCell);
        FocusCellInSceneView(mapData.DefaultSpawnCell);

        SelectedCellStatus status = GetSelectedCellStatus(mapData.DefaultSpawnCell);
        SetPlaytestStatus("Selected cell moved to Default Spawn: " + FormatCell(mapData.DefaultSpawnCell)
            + "\nSelected Cell Status: " + GetSelectedCellStatusLabel(status), GetSelectedCellStatusMessageType(status));
    }

    private void SetSelectedCellAsDefaultSpawn()
    {
        if (mapData == null)
        {
            return;
        }

        Vector3Int selectedCell = GetPlacementCell();
        if (!mapData.IsInsideBounds(selectedCell))
        {
            SetPlaytestStatus("Cannot set Default Spawn outside map bounds: " + FormatCell(selectedCell), MessageType.Warning);
            return;
        }

        Undo.RecordObject(mapData, "Set Default Spawn Cell");
        mapData.DefaultSpawnCell = selectedCell;
        EditorUtility.SetDirty(mapData);
        RepaintSceneViews();

        MessageType messageType = IsRuntimeWalkableCell(selectedCell) ? MessageType.Info : MessageType.Warning;
        string suffix = messageType == MessageType.Info ? string.Empty : "\nWarning: selected cell is not runtime-walkable yet.";
        SetPlaytestStatus("Default Spawn set to " + FormatCell(selectedCell) + suffix, messageType);
    }

    private void PlayFromSelectedEncounterArea(IsoMapEncounterAreaData selectedArea)
    {
        if (selectedArea == null)
        {
            SetPlaytestStatus("No EncounterArea is selected.", MessageType.Warning);
            return;
        }

        Vector3Int startCell;
        if (!TryFindFirstWalkableCellInEncounterArea(selectedArea, out startCell))
        {
            SetPlaytestStatus("Selected EncounterArea has no runtime-walkable cell: " + selectedArea.EncounterAreaId, MessageType.Warning);
            return;
        }

        SetPlacementCell(startCell);
        StartMapPlaytest(startCell, selectedArea.EncounterAreaId, "Selected Encounter Area");
    }

    private void StartMapPlaytest(Vector3Int startCell, string forceEncounterAreaId, string modeLabel)
    {
        if (!ValidatePlaytestRequest(startCell, forceEncounterAreaId))
        {
            return;
        }

        if (!ConfirmAndSaveMapDataForPlaytest())
        {
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            SetPlaytestStatus("Play From Here canceled because the current Scene was not saved.", MessageType.Warning);
            return;
        }

        Scene previousScene = SceneManager.GetActiveScene();
        string mapDataPath = AssetDatabase.GetAssetPath(mapData);
        string prefabCatalogPath = AssetDatabase.GetAssetPath(prefabCatalog);
        IsoMapPlaytestSettings settings = new IsoMapPlaytestSettings
        {
            MapDataGuid = AssetDatabase.AssetPathToGUID(mapDataPath),
            PrefabCatalogGuid = AssetDatabase.AssetPathToGUID(prefabCatalogPath),
            StartCell = startCell,
            StartDirection = GetPlaytestStartDirection(),
            ForceEncounterAreaId = string.IsNullOrEmpty(forceEncounterAreaId) ? string.Empty : forceEncounterAreaId,
            ReturnToPreviousScene = playtestReturnToPreviousScene,
            PreviousScenePath = previousScene.path,
            PreviousSceneName = previousScene.name,
            RuntimeScenePath = IsoMapRuntimeTestSceneBuilder.ScenePath,
            StartedAtUtc = DateTime.UtcNow.ToString("o")
        };

        IsoMapPlaytestSettings.Save(settings);
        Debug.Log("[IsoMapPlaytest] Play From " + modeLabel + ". cell=" + FormatCell(startCell)
            + " mapId=" + mapData.MapId
            + " mapData=" + mapDataPath
            + " prefabCatalog=" + prefabCatalogPath
            + " startDirection=" + settings.StartDirection
            + " forceEncounterAreaId=" + (string.IsNullOrEmpty(settings.ForceEncounterAreaId) ? "(none)" : settings.ForceEncounterAreaId)
            + " runtimeScene=" + settings.RuntimeScenePath
            + " previousScene=" + (string.IsNullOrEmpty(settings.PreviousSceneName) ? "(none)" : settings.PreviousSceneName));

        try
        {
            EditorSceneManager.OpenScene(IsoMapRuntimeTestSceneBuilder.ScenePath, OpenSceneMode.Single);
        }
        catch (Exception exception)
        {
            IsoMapPlaytestSettings.Clear();
            SetPlaytestStatus("Failed to open RuntimeTest Scene: " + exception.Message, MessageType.Error);
            Debug.LogError("[IsoMapPlaytest] Failed to open RuntimeTest Scene. " + exception);
            return;
        }

        SetPlaytestStatus("PlayMode starting from " + FormatCell(startCell), MessageType.Info);
        EditorApplication.isPlaying = true;
    }

    private bool ConfirmAndSaveMapDataForPlaytest()
    {
        if (mapData == null)
        {
            return false;
        }

        if (EditorUtility.IsDirty(mapData)
            && !EditorUtility.DisplayDialog(
                "Save IsoMapData",
                "IsoMapData has unsaved changes. Save before Play From Here?",
                "Save and Play",
                "Cancel"))
        {
            SetPlaytestStatus("Play From Here canceled. IsoMapData was not saved.", MessageType.Warning);
            return false;
        }

        SaveMapData();
        return true;
    }

    private bool ValidatePlaytestRequest(Vector3Int startCell, string forceEncounterAreaId)
    {
        List<string> issues = new List<string>();
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            issues.Add("Unity is already entering or running PlayMode.");
        }

        if (mapData == null)
        {
            issues.Add("MapData is not assigned.");
        }

        if (prefabCatalog == null)
        {
            issues.Add("PrefabCatalog is not assigned.");
        }

        string mapDataPath = mapData != null ? AssetDatabase.GetAssetPath(mapData) : string.Empty;
        if (mapData != null && string.IsNullOrEmpty(mapDataPath))
        {
            issues.Add("MapData must be saved as an asset before playtesting.");
        }

        string prefabCatalogPath = prefabCatalog != null ? AssetDatabase.GetAssetPath(prefabCatalog) : string.Empty;
        if (prefabCatalog != null && string.IsNullOrEmpty(prefabCatalogPath))
        {
            issues.Add("PrefabCatalog must be saved as an asset before playtesting.");
        }

        SceneAsset runtimeScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(IsoMapRuntimeTestSceneBuilder.ScenePath);
        if (runtimeScene == null)
        {
            issues.Add("RuntimeTest Scene is missing: " + IsoMapRuntimeTestSceneBuilder.ScenePath);
        }

        if (!IsSceneEnabledInBuildSettings("BattleScene"))
        {
            issues.Add("BattleScene is not enabled in Build Settings.");
        }

        if (mapData != null)
        {
            if (!mapData.IsInsideBounds(startCell))
            {
                issues.Add("Start cell is outside map bounds: " + FormatCell(startCell));
            }
            else if (!IsRuntimeWalkableCell(startCell))
            {
                issues.Add("Start cell is not runtime-walkable: " + FormatCell(startCell));
            }
        }

        if (!string.IsNullOrEmpty(forceEncounterAreaId) && FindEncounterAreaById(forceEncounterAreaId) == null)
        {
            issues.Add("Selected EncounterArea was not found: " + forceEncounterAreaId);
        }

        if (issues.Count > 0)
        {
            string message = string.Join("\n", issues.ToArray());
            SetPlaytestStatus(message, MessageType.Warning);
            Debug.LogWarning("[IsoMapPlaytest] Play From Here validation failed:\n" + message);
            return false;
        }

        LogEncounterAreaPlaytestInfo(startCell, forceEncounterAreaId);
        return true;
    }

    private void LogEncounterAreaPlaytestInfo(Vector3Int startCell, string forceEncounterAreaId)
    {
        IsoMapEncounterAreaData startArea = FindEncounterAreaContainingCell(startCell);
        if (startArea != null && !string.IsNullOrWhiteSpace(startArea.EnemyGroupTableId))
        {
            Debug.Log("[IsoMapPlaytest] Start cell EncounterArea. areaId=" + startArea.EncounterAreaId
                + " enemyGroupTableId=" + startArea.EnemyGroupTableId);
        }

        IsoMapEncounterAreaData forcedArea = FindEncounterAreaById(forceEncounterAreaId);
        if (forcedArea != null && !string.IsNullOrWhiteSpace(forcedArea.EnemyGroupTableId))
        {
            Debug.Log("[IsoMapPlaytest] Selected EncounterArea. areaId=" + forcedArea.EncounterAreaId
                + " enemyGroupTableId=" + forcedArea.EnemyGroupTableId);
        }
    }

    private bool IsSceneEnabledInBuildSettings(string sceneName)
    {
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
            if (scene != null
                && scene.enabled
                && string.Equals(Path.GetFileNameWithoutExtension(scene.path), sceneName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryFindFirstWalkableCellInEncounterArea(IsoMapEncounterAreaData area, out Vector3Int startCell)
    {
        startCell = Vector3Int.zero;
        if (area == null)
        {
            return false;
        }

        for (int i = 0; i < area.Cells.Count; i++)
        {
            Vector3Int candidate = area.Cells[i];
            if (IsRuntimeWalkableCell(candidate))
            {
                startCell = candidate;
                return true;
            }
        }

        return false;
    }

    private bool IsRuntimeWalkableCell(Vector3Int cell)
    {
        if (mapData == null || !mapData.IsInsideBounds(cell))
        {
            return false;
        }

        bool hasWalkableTile = false;
        for (int i = 0; i < mapData.TileInstances.Count; i++)
        {
            IsoMapTileInstanceData tile = mapData.TileInstances[i];
            if (tile == null || tile.Cell != cell)
            {
                continue;
            }

            IsoMapPrefabCatalogEntry entry = prefabCatalog != null ? prefabCatalog.FindEntry(tile.PrefabId) : null;
            bool entryWalkable = entry == null || entry.Walkable;
            bool entryBlocksMovement = entry != null && entry.BlocksMovement;
            if (tile.Walkable && entryWalkable && !entryBlocksMovement)
            {
                hasWalkableTile = true;
            }
        }

        if (!hasWalkableTile)
        {
            return false;
        }

        for (int i = 0; i < mapData.PropInstances.Count; i++)
        {
            IsoMapPropInstanceData prop = mapData.PropInstances[i];
            if (prop == null || prop.Cell != cell)
            {
                continue;
            }

            IsoMapPrefabCatalogEntry entry = prefabCatalog != null ? prefabCatalog.FindEntry(prop.PrefabId) : null;
            if (prop.BlocksMovement || (entry != null && entry.BlocksMovement))
            {
                return false;
            }
        }

        return true;
    }

    private SelectedCellStatus GetSelectedCellStatus(Vector3Int cell)
    {
        if (mapData == null)
        {
            return SelectedCellStatus.NoIsoMapData;
        }

        if (prefabCatalog == null)
        {
            return SelectedCellStatus.NoPrefabCatalog;
        }

        if (!mapData.IsInsideBounds(cell))
        {
            return SelectedCellStatus.NotFound;
        }

        bool foundCellContent = false;
        bool hasWalkableTile = false;
        for (int i = 0; i < mapData.TileInstances.Count; i++)
        {
            IsoMapTileInstanceData tile = mapData.TileInstances[i];
            if (tile == null || tile.Cell != cell)
            {
                continue;
            }

            foundCellContent = true;
            IsoMapPrefabCatalogEntry entry = prefabCatalog.FindEntry(tile.PrefabId);
            bool entryWalkable = entry == null || entry.Walkable;
            bool entryBlocksMovement = entry != null && entry.BlocksMovement;
            if (tile.Walkable && entryWalkable && !entryBlocksMovement)
            {
                hasWalkableTile = true;
            }
        }

        bool blocked = false;
        for (int i = 0; i < mapData.PropInstances.Count; i++)
        {
            IsoMapPropInstanceData prop = mapData.PropInstances[i];
            if (prop == null || prop.Cell != cell)
            {
                continue;
            }

            foundCellContent = true;
            IsoMapPrefabCatalogEntry entry = prefabCatalog.FindEntry(prop.PrefabId);
            if (prop.BlocksMovement || (entry != null && entry.BlocksMovement))
            {
                blocked = true;
            }
        }

        if (!foundCellContent)
        {
            return SelectedCellStatus.NotFound;
        }

        if (!hasWalkableTile || blocked)
        {
            return SelectedCellStatus.Blocked;
        }

        return SelectedCellStatus.Walkable;
    }

    private static string GetSelectedCellStatusLabel(SelectedCellStatus status)
    {
        switch (status)
        {
            case SelectedCellStatus.Walkable:
                return "Walkable";
            case SelectedCellStatus.NotFound:
                return "Not found";
            case SelectedCellStatus.Blocked:
                return "Blocked";
            case SelectedCellStatus.NoIsoMapData:
                return "No IsoMapData";
            case SelectedCellStatus.NoPrefabCatalog:
                return "No PrefabCatalog";
            default:
                return "Unknown";
        }
    }

    private static MessageType GetSelectedCellStatusMessageType(SelectedCellStatus status)
    {
        return status == SelectedCellStatus.Walkable ? MessageType.Info : MessageType.Warning;
    }

    private SceneView GetTargetSceneView()
    {
        if (SceneView.lastActiveSceneView != null)
        {
            return SceneView.lastActiveSceneView;
        }

        if (SceneView.sceneViews != null && SceneView.sceneViews.Count > 0)
        {
            return SceneView.sceneViews[0] as SceneView;
        }

        return null;
    }

    private Bounds GetMapWorldBounds()
    {
        float cellSize = mapData != null ? mapData.CellSize : 1f;
        float width = Mathf.Max(1, mapData.Width) * cellSize;
        float depth = Mathf.Max(1, mapData.Depth) * cellSize;
        float height = Mathf.Max(1, mapData.MaxLevel + 1) * cellSize;
        Vector3 center = new Vector3(
            (mapData.Width - 1) * cellSize * 0.5f,
            mapData.MaxLevel * cellSize * 0.5f,
            (mapData.Depth - 1) * cellSize * 0.5f);

        return new Bounds(center, new Vector3(width, height, depth));
    }

    private void FocusCellInSceneView(Vector3Int cell)
    {
        if (mapData == null)
        {
            return;
        }

        SceneView sceneView = GetTargetSceneView();
        if (sceneView == null)
        {
            return;
        }

        Vector3 center = mapData.CellToWorld(cell);
        Quaternion rotation = Quaternion.Euler(35f, 45f, 0f);
        float viewSize = Mathf.Max(3f, mapData.CellSize * 4f);
        sceneView.in2DMode = false;
        sceneView.LookAt(center, rotation, viewSize, false, true);
        sceneView.Repaint();
    }

    private IsoMapEncounterAreaData FindEncounterAreaContainingCell(Vector3Int cell)
    {
        if (mapData == null)
        {
            return null;
        }

        for (int i = 0; i < mapData.EncounterAreas.Count; i++)
        {
            IsoMapEncounterAreaData area = mapData.EncounterAreas[i];
            if (area != null && area.Cells.Contains(cell))
            {
                return area;
            }
        }

        return null;
    }

    private IsoMapEncounterAreaData FindEncounterAreaById(string encounterAreaId)
    {
        if (mapData == null || string.IsNullOrEmpty(encounterAreaId))
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

    private void SetPlacementCell(Vector3Int cell)
    {
        placementX = cell.x;
        placementZ = cell.z;
        placementLevel = cell.y;
        ClampPlacementCell();
        Repaint();
        RepaintSceneViews();
    }

    private Vector2Int GetPlaytestStartDirection()
    {
        playtestDirectionIndex = Mathf.Clamp(playtestDirectionIndex, 0, PlaytestDirectionValues.Length - 1);
        return PlaytestDirectionValues[playtestDirectionIndex];
    }

    private void SetPlaytestStatus(string message, MessageType messageType)
    {
        playtestStatusMessage = message;
        playtestStatusType = messageType;
        Repaint();
    }

    private void ClearPlaytestStatus()
    {
        if (string.IsNullOrEmpty(playtestStatusMessage))
        {
            return;
        }

        playtestStatusMessage = string.Empty;
        playtestStatusType = MessageType.None;
        Repaint();
    }

    private void AddEncounterArea()
    {
        if (mapData == null)
        {
            return;
        }

        Undo.RecordObject(mapData, "Add Encounter Area");
        IsoMapEncounterAreaData area = new IsoMapEncounterAreaData();
        area.EncounterAreaId = NewEncounterAreaId();
        area.EnemyGroupTableId = "sf_floating_test_group";
        area.BattleBackgroundId = "sf_floating_test_bg";
        area.BattleBgmId = "sf_floating_test_bgm";
        area.MinStepsBeforeEncounter = 3;
        area.EncounterChancePerStep = 0.20f;
        area.CooldownStepsAfterBattle = 5;
        mapData.EncounterAreas.Add(area);
        selectedEncounterAreaIndex = mapData.EncounterAreas.Count - 1;
        EditorUtility.SetDirty(mapData);
        RefreshPreviewIfPresent();
        RepaintSceneViews();
    }

    private void DeleteSelectedEncounterArea()
    {
        IsoMapEncounterAreaData area = GetSelectedEncounterArea();
        if (area == null)
        {
            return;
        }

        Undo.RecordObject(mapData, "Delete Encounter Area");
        mapData.EncounterAreas.RemoveAt(selectedEncounterAreaIndex);
        selectedEncounterAreaIndex = Mathf.Clamp(selectedEncounterAreaIndex, -1, mapData.EncounterAreas.Count - 1);
        if (mapData.EncounterAreas.Count == 0)
        {
            selectedEncounterAreaIndex = -1;
        }

        EditorUtility.SetDirty(mapData);
        RefreshPreviewIfPresent();
        RepaintSceneViews();
    }

    private IsoMapEncounterAreaData GetSelectedEncounterArea()
    {
        if (mapData == null
            || selectedEncounterAreaIndex < 0
            || selectedEncounterAreaIndex >= mapData.EncounterAreas.Count)
        {
            return null;
        }

        return mapData.EncounterAreas[selectedEncounterAreaIndex];
    }

    private void AddCellToSelectedEncounterArea(Vector3Int cell)
    {
        IsoMapEncounterAreaData area = GetSelectedEncounterArea();
        if (area == null)
        {
            return;
        }

        if (!mapData.IsInsideBounds(cell))
        {
            Debug.LogWarning("EncounterArea cell is outside map bounds: " + FormatCell(cell));
            return;
        }

        if (area.Cells.Contains(cell))
        {
            return;
        }

        Undo.RecordObject(mapData, "Add Encounter Area Cell");
        area.Cells.Add(cell);
        EditorUtility.SetDirty(mapData);
        RefreshPreviewIfPresent();
        RepaintSceneViews();
    }

    private void RemoveCellFromSelectedEncounterArea(Vector3Int cell)
    {
        IsoMapEncounterAreaData area = GetSelectedEncounterArea();
        if (area == null)
        {
            return;
        }

        Undo.RecordObject(mapData, "Remove Encounter Area Cell");
        if (area.Cells.Remove(cell))
        {
            EditorUtility.SetDirty(mapData);
            RefreshPreviewIfPresent();
            RepaintSceneViews();
        }
    }

    private void RemoveCellFromSelectedEncounterAreaAtIndex(IsoMapEncounterAreaData area, int index)
    {
        if (area == null || index < 0 || index >= area.Cells.Count)
        {
            return;
        }

        Undo.RecordObject(mapData, "Remove Encounter Area Cell");
        area.Cells.RemoveAt(index);
        EditorUtility.SetDirty(mapData);
        RefreshPreviewIfPresent();
        RepaintSceneViews();
    }

    private string NewEncounterAreaId()
    {
        int nextIndex = mapData != null ? mapData.EncounterAreas.Count + 1 : 1;
        string candidate;
        do
        {
            candidate = "encounter_area_" + nextIndex.ToString("00");
            nextIndex++;
        }
        while (HasEncounterAreaId(candidate));

        return candidate;
    }

    private bool HasEncounterAreaId(string encounterAreaId)
    {
        if (mapData == null)
        {
            return false;
        }

        for (int i = 0; i < mapData.EncounterAreas.Count; i++)
        {
            IsoMapEncounterAreaData area = mapData.EncounterAreas[i];
            if (area != null && string.Equals(area.EncounterAreaId, encounterAreaId, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
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

        CreateEncounterAreaPreviewInstances(root.transform);

        Selection.activeGameObject = root;
        RepaintSceneViews();
    }

    private void CreateEncounterAreaPreviewInstances(Transform parent)
    {
        if (prefabCatalog == null || prefabCatalog.FindEntry("EncounterAreaMarker") == null)
        {
            return;
        }

        for (int areaIndex = 0; areaIndex < mapData.EncounterAreas.Count; areaIndex++)
        {
            IsoMapEncounterAreaData area = mapData.EncounterAreas[areaIndex];
            if (area == null)
            {
                continue;
            }

            for (int cellIndex = 0; cellIndex < area.Cells.Count; cellIndex++)
            {
                Vector3Int cell = area.Cells[cellIndex];
                CreatePreviewInstance(
                    parent,
                    "EncounterAreaMarker",
                    area.EncounterAreaId + "_" + cell.x + "_" + cell.y + "_" + cell.z,
                    cell,
                    prefabCatalog.FindEntry("EncounterAreaMarker").DefaultOffset,
                    0,
                    "Encounter");
            }
        }
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

        HashSet<string> encounterAreaIds = new HashSet<string>();
        for (int i = 0; i < mapData.EncounterAreas.Count; i++)
        {
            IsoMapEncounterAreaData area = mapData.EncounterAreas[i];
            if (area == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(area.EncounterAreaId))
            {
                AddValidationWarning("EncounterArea has an empty id.", ref warningCount);
            }
            else if (!encounterAreaIds.Add(area.EncounterAreaId))
            {
                AddValidationWarning("Duplicate EncounterArea id: " + area.EncounterAreaId, ref warningCount);
            }

            if (area.Cells.Count == 0)
            {
                AddValidationWarning("EncounterArea has no cells: " + area.EncounterAreaId, ref warningCount);
            }

            for (int cellIndex = 0; cellIndex < area.Cells.Count; cellIndex++)
            {
                Vector3Int cell = area.Cells[cellIndex];
                if (!mapData.IsInsideBounds(cell))
                {
                    AddValidationWarning("EncounterArea cell is outside map bounds: " + area.EncounterAreaId + " " + FormatCell(cell), ref warningCount);
                }
                else if (!HasAnyWalkableTileAtCell(cell))
                {
                    AddValidationWarning("EncounterArea cell has no walkable tile: " + area.EncounterAreaId + " " + FormatCell(cell), ref warningCount);
                }
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

    private bool HasAnyWalkableTileAtCell(Vector3Int cell)
    {
        if (mapData == null)
        {
            return false;
        }

        for (int i = 0; i < mapData.TileInstances.Count; i++)
        {
            IsoMapTileInstanceData instance = mapData.TileInstances[i];
            if (instance != null && instance.Cell == cell && instance.Walkable)
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
        DrawEncounterAreaHandles();
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

        DrawDefaultSpawnHandle(y);
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

    private void DrawDefaultSpawnHandle(float y)
    {
        Vector3Int spawnCell = mapData.DefaultSpawnCell;
        if (!mapData.IsInsideBounds(spawnCell))
        {
            return;
        }

        Vector3 center = mapData.CellToWorld(spawnCell);
        float half = mapData.CellSize * 0.5f;
        Vector3[] corners =
        {
            new Vector3(center.x - half, y + 0.006f, center.z - half),
            new Vector3(center.x - half, y + 0.006f, center.z + half),
            new Vector3(center.x + half, y + 0.006f, center.z + half),
            new Vector3(center.x + half, y + 0.006f, center.z - half)
        };

        Handles.DrawSolidRectangleWithOutline(corners, new Color(0.20f, 1f, 0.48f, 0.08f), new Color(0.20f, 1f, 0.48f, 0.85f));
        Handles.Label(center + new Vector3(0f, 0.44f, 0f), "Default Spawn");
    }

    private void DrawEncounterAreaHandles()
    {
        if (mapData == null || mapData.EncounterAreas.Count == 0)
        {
            return;
        }

        Color previousColor = Handles.color;
        float half = mapData.CellSize * 0.5f;
        for (int areaIndex = 0; areaIndex < mapData.EncounterAreas.Count; areaIndex++)
        {
            IsoMapEncounterAreaData area = mapData.EncounterAreas[areaIndex];
            if (area == null)
            {
                continue;
            }

            bool selected = areaIndex == selectedEncounterAreaIndex;
            Color fillColor = selected
                ? new Color(1f, 0.42f, 0.18f, 0.20f)
                : new Color(1f, 0.16f, 0.16f, 0.10f);
            Color outlineColor = selected
                ? new Color(1f, 0.70f, 0.26f, 0.95f)
                : new Color(1f, 0.22f, 0.22f, 0.65f);

            for (int i = 0; i < area.Cells.Count; i++)
            {
                Vector3Int cell = area.Cells[i];
                if (!mapData.IsInsideBounds(cell))
                {
                    continue;
                }

                Vector3 center = mapData.CellToWorld(cell);
                float y = center.y + 0.045f;
                Vector3[] corners =
                {
                    new Vector3(center.x - half, y, center.z - half),
                    new Vector3(center.x - half, y, center.z + half),
                    new Vector3(center.x + half, y, center.z + half),
                    new Vector3(center.x + half, y, center.z - half)
                };

                Handles.DrawSolidRectangleWithOutline(corners, fillColor, outlineColor);
                if (selected)
                {
                    Handles.Label(center + new Vector3(0f, 0.30f, 0f), area.EncounterAreaId);
                }
            }
        }

        Handles.color = previousColor;
    }

    private void HandleSceneViewInput(SceneView sceneView)
    {
        if (!IsSceneViewInputAllowed(sceneView))
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
            SetPlaytestStatus("SceneView click did not hit the map plane. Use Frame Map In Scene View, then click a visible floor face.", MessageType.Warning);
            return;
        }

        placementX = cell.x;
        placementZ = cell.z;
        placementLevel = cell.y;
        ClearPlaytestStatus();
        Repaint();
        sceneView.Repaint();

        if (current.shift)
        {
            PlaceSelectedPrefabAtCell(cell);
        }

        current.Use();
    }

    private bool IsSceneViewInputAllowed(SceneView sceneView)
    {
        EditorWindow focused = EditorWindow.focusedWindow;
        EditorWindow mouseOver = EditorWindow.mouseOverWindow;
        return focused == this
            || focused == sceneView
            || mouseOver == sceneView;
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
