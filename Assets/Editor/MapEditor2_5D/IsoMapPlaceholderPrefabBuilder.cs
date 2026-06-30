using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class IsoMapPlaceholderPrefabBuilder
{
    private const string RootFolder = "Assets/NeonCardia";
    private const string MapEditorFolder = RootFolder + "/MapEditor2_5D";
    private const string PrefabFolder = MapEditorFolder + "/PlaceholderPrefabs";
    private const string MaterialFolder = MapEditorFolder + "/Materials";
    private const string DataFolder = MapEditorFolder + "/Data";
    private const string CatalogPath = DataFolder + "/IsoMapPlaceholderPrefabCatalog.asset";

    [MenuItem("Tools/NeonCardia/Generate 2.5D Map Placeholder Prefabs and Catalog")]
    public static void GeneratePlaceholderPrefabsAndCatalog()
    {
        EnsureFolderPath(PrefabFolder);
        EnsureFolderPath(MaterialFolder);
        EnsureFolderPath(DataFolder);

        Dictionary<string, Material> materials = CreateMaterials();

        SavePrefab("PlatformTile_1x1", BuildPlatformTile(materials));
        SavePrefab("Bridge_1x1", BuildBridge(materials));
        SavePrefab("Stair_Up", BuildStair(materials));
        SavePrefab("Wall", BuildWall(materials));
        SavePrefab("Railing", BuildRailing(materials));
        SavePrefab("SpawnMarker", BuildSpawnMarker(materials));
        SavePrefab("EncounterAreaMarker", BuildEncounterAreaMarker(materials));
        SavePrefab("TransitionMarker", BuildTransitionMarker(materials));

        BuildCatalog();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Generated 2.5D map placeholder prefabs and catalog at " + MapEditorFolder + ".");
    }

    public static string CatalogAssetPath
    {
        get { return CatalogPath; }
    }

    private static Dictionary<string, Material> CreateMaterials()
    {
        Dictionary<string, Material> materials = new Dictionary<string, Material>();
        materials["TileTop"] = CreateMaterial("MIso_TileTop_Cyan", new Color(0.34f, 0.95f, 1f, 1f));
        materials["TileSide"] = CreateMaterial("MIso_TileSide_Blue", new Color(0.04f, 0.38f, 0.90f, 1f));
        materials["Bridge"] = CreateMaterial("MIso_Bridge_Mint", new Color(0.48f, 0.90f, 0.78f, 1f));
        materials["Step"] = CreateMaterial("MIso_Step_Warm", new Color(1f, 0.72f, 0.38f, 1f));
        materials["Wall"] = CreateMaterial("MIso_Wall_Gray", new Color(0.56f, 0.60f, 0.66f, 1f));
        materials["Railing"] = CreateMaterial("MIso_Railing_Dark", new Color(0.16f, 0.20f, 0.24f, 1f));
        materials["Spawn"] = CreateMaterial("MIso_Spawn_Green", new Color(0.18f, 1f, 0.45f, 1f));
        materials["Encounter"] = CreateMaterial("MIso_Encounter_Red", new Color(1f, 0.20f, 0.18f, 1f));
        materials["Transition"] = CreateMaterial("MIso_Transition_Blue", new Color(0.20f, 0.55f, 1f, 1f));
        materials["Accent"] = CreateMaterial("MIso_Accent_Yellow", new Color(1f, 0.82f, 0.22f, 1f));
        return materials;
    }

    private static Material CreateMaterial(string materialName, Color color)
    {
        string path = MaterialFolder + "/" + materialName + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static void SavePrefab(string prefabId, GameObject root)
    {
        string path = PrefabFolder + "/" + prefabId + ".prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static GameObject BuildPlatformTile(Dictionary<string, Material> materials)
    {
        GameObject root = new GameObject("PlatformTile_1x1");
        AddCube(root.transform, "TileTop", new Vector3(0f, -0.05f, 0f), new Vector3(0.96f, 0.10f, 0.96f), materials["TileTop"]);
        AddCube(root.transform, "FrontTrim", new Vector3(0f, -0.13f, -0.48f), new Vector3(0.98f, 0.10f, 0.06f), materials["TileSide"]);
        AddCube(root.transform, "RightTrim", new Vector3(0.48f, -0.13f, 0f), new Vector3(0.06f, 0.10f, 0.98f), materials["TileSide"]);
        AddCube(root.transform, "Underside", new Vector3(0f, -0.22f, 0f), new Vector3(0.78f, 0.14f, 0.78f), materials["Accent"]);
        AddDotGrid(root.transform, materials["Wall"]);
        return root;
    }

    private static GameObject BuildBridge(Dictionary<string, Material> materials)
    {
        GameObject root = new GameObject("Bridge_1x1");
        AddCube(root.transform, "Deck", new Vector3(0f, -0.04f, 0f), new Vector3(1.0f, 0.08f, 0.42f), materials["Bridge"]);
        AddCube(root.transform, "LeftEdge", new Vector3(0f, 0.02f, -0.23f), new Vector3(1.0f, 0.07f, 0.04f), materials["TileSide"]);
        AddCube(root.transform, "RightEdge", new Vector3(0f, 0.02f, 0.23f), new Vector3(1.0f, 0.07f, 0.04f), materials["TileSide"]);
        AddCylinder(root.transform, "SocketA", new Vector3(-0.30f, 0.03f, 0f), new Vector3(0.18f, 0.025f, 0.18f), materials["Wall"]);
        AddCylinder(root.transform, "SocketB", new Vector3(0.30f, 0.03f, 0f), new Vector3(0.18f, 0.025f, 0.18f), materials["Wall"]);
        return root;
    }

    private static GameObject BuildStair(Dictionary<string, Material> materials)
    {
        GameObject root = new GameObject("Stair_Up");
        AddCube(root.transform, "StepLow", new Vector3(0f, -0.03f, -0.28f), new Vector3(0.82f, 0.08f, 0.28f), materials["Step"]);
        AddCube(root.transform, "StepMid", new Vector3(0f, 0.06f, 0f), new Vector3(0.82f, 0.16f, 0.28f), materials["Step"]);
        AddCube(root.transform, "StepHigh", new Vector3(0f, 0.15f, 0.28f), new Vector3(0.82f, 0.24f, 0.28f), materials["Step"]);
        AddCube(root.transform, "SideA", new Vector3(-0.44f, 0.04f, 0f), new Vector3(0.06f, 0.22f, 0.88f), materials["TileSide"]);
        AddCube(root.transform, "SideB", new Vector3(0.44f, 0.04f, 0f), new Vector3(0.06f, 0.22f, 0.88f), materials["TileSide"]);
        return root;
    }

    private static GameObject BuildWall(Dictionary<string, Material> materials)
    {
        GameObject root = new GameObject("Wall");
        AddCube(root.transform, "WallBlock", new Vector3(0f, 0.42f, 0.40f), new Vector3(0.88f, 0.86f, 0.12f), materials["Wall"]);
        AddCube(root.transform, "TopCap", new Vector3(0f, 0.90f, 0.40f), new Vector3(0.96f, 0.10f, 0.18f), materials["TileSide"]);
        return root;
    }

    private static GameObject BuildRailing(Dictionary<string, Material> materials)
    {
        GameObject root = new GameObject("Railing");
        AddCube(root.transform, "PostA", new Vector3(-0.36f, 0.22f, 0.42f), new Vector3(0.08f, 0.44f, 0.08f), materials["Railing"]);
        AddCube(root.transform, "PostB", new Vector3(0.36f, 0.22f, 0.42f), new Vector3(0.08f, 0.44f, 0.08f), materials["Railing"]);
        AddCube(root.transform, "RailTop", new Vector3(0f, 0.44f, 0.42f), new Vector3(0.84f, 0.08f, 0.08f), materials["Railing"]);
        AddCube(root.transform, "RailMid", new Vector3(0f, 0.25f, 0.42f), new Vector3(0.80f, 0.05f, 0.06f), materials["TileSide"]);
        return root;
    }

    private static GameObject BuildSpawnMarker(Dictionary<string, Material> materials)
    {
        GameObject root = new GameObject("SpawnMarker");
        AddCylinder(root.transform, "Base", new Vector3(0f, 0.02f, 0f), new Vector3(0.34f, 0.025f, 0.34f), materials["Spawn"]);
        AddCube(root.transform, "ArrowStem", new Vector3(0f, 0.10f, -0.08f), new Vector3(0.10f, 0.10f, 0.32f), materials["Spawn"]);
        AddCube(root.transform, "ArrowHead", new Vector3(0f, 0.12f, 0.15f), new Vector3(0.28f, 0.10f, 0.18f), materials["Accent"]);
        return root;
    }

    private static GameObject BuildEncounterAreaMarker(Dictionary<string, Material> materials)
    {
        GameObject root = new GameObject("EncounterAreaMarker");
        AddCube(root.transform, "AreaPlate", new Vector3(0f, 0.01f, 0f), new Vector3(0.82f, 0.03f, 0.82f), materials["Encounter"]);
        AddCube(root.transform, "BorderA", new Vector3(0f, 0.04f, -0.41f), new Vector3(0.88f, 0.05f, 0.05f), materials["Accent"]);
        AddCube(root.transform, "BorderB", new Vector3(0f, 0.04f, 0.41f), new Vector3(0.88f, 0.05f, 0.05f), materials["Accent"]);
        return root;
    }

    private static GameObject BuildTransitionMarker(Dictionary<string, Material> materials)
    {
        GameObject root = new GameObject("TransitionMarker");
        AddCylinder(root.transform, "PortalBase", new Vector3(0f, 0.03f, 0f), new Vector3(0.36f, 0.025f, 0.36f), materials["Transition"]);
        AddCube(root.transform, "GateLeft", new Vector3(-0.30f, 0.30f, 0f), new Vector3(0.07f, 0.60f, 0.08f), materials["Transition"]);
        AddCube(root.transform, "GateRight", new Vector3(0.30f, 0.30f, 0f), new Vector3(0.07f, 0.60f, 0.08f), materials["Transition"]);
        AddCube(root.transform, "GateTop", new Vector3(0f, 0.60f, 0f), new Vector3(0.66f, 0.07f, 0.08f), materials["Accent"]);
        return root;
    }

    private static void AddDotGrid(Transform parent, Material material)
    {
        for (int z = -1; z <= 1; z++)
        {
            for (int x = -1; x <= 1; x++)
            {
                AddCube(parent, "Dot", new Vector3(x * 0.16f, 0.025f, z * 0.16f), new Vector3(0.035f, 0.035f, 0.035f), material);
            }
        }
    }

    private static GameObject AddCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPosition;
        cube.transform.localScale = localScale;
        MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        return cube;
    }

    private static GameObject AddCylinder(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = name;
        cylinder.transform.SetParent(parent, false);
        cylinder.transform.localPosition = localPosition;
        cylinder.transform.localScale = localScale;
        MeshRenderer renderer = cylinder.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        return cylinder;
    }

    private static void BuildCatalog()
    {
        IsoMapPrefabCatalog catalog = AssetDatabase.LoadAssetAtPath<IsoMapPrefabCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<IsoMapPrefabCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        catalog.Entries.Clear();
        AddCatalogEntry(catalog, "PlatformTile_1x1", "Floor 1x1", IsoMapPrefabCategory.Floor, true, false, 0, Vector3.zero, "Base walkable floor tile.");
        AddCatalogEntry(catalog, "Bridge_1x1", "Bridge 1x1", IsoMapPrefabCategory.Bridge, true, false, 0, Vector3.zero, "Walkable narrow connector tile.");
        AddCatalogEntry(catalog, "Stair_Up", "Stair Up", IsoMapPrefabCategory.Stair, true, false, 0, Vector3.zero, "Walkable level connector.");
        AddCatalogEntry(catalog, "Wall", "Wall", IsoMapPrefabCategory.Wall, false, true, 0, Vector3.zero, "Blocking vertical boundary prop.");
        AddCatalogEntry(catalog, "Railing", "Railing", IsoMapPrefabCategory.Railing, false, true, 0, Vector3.zero, "Blocking edge guard prop.");
        AddCatalogEntry(catalog, "SpawnMarker", "Spawn Marker", IsoMapPrefabCategory.Marker, true, false, 0, new Vector3(0f, 0.08f, 0f), "Editor/runtime marker. Not final map art.");
        AddCatalogEntry(catalog, "EncounterAreaMarker", "Encounter Area Marker", IsoMapPrefabCategory.Marker, true, false, 0, new Vector3(0f, 0.05f, 0f), "Editor preview marker for encounter cells.");
        AddCatalogEntry(catalog, "TransitionMarker", "Transition Marker", IsoMapPrefabCategory.Marker, true, false, 0, new Vector3(0f, 0.08f, 0f), "Editor/runtime marker for map exits.");
        EditorUtility.SetDirty(catalog);
    }

    private static void AddCatalogEntry(
        IsoMapPrefabCatalog catalog,
        string prefabId,
        string displayName,
        IsoMapPrefabCategory category,
        bool defaultWalkable,
        bool defaultBlocksMovement,
        int defaultRotationY,
        Vector3 defaultOffset,
        string notes)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/" + prefabId + ".prefab");
        catalog.Entries.Add(new IsoMapPrefabCatalogEntry(
            prefabId,
            displayName,
            prefab,
            category,
            defaultWalkable,
            defaultBlocksMovement,
            true,
            defaultRotationY,
            defaultOffset,
            notes));
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
