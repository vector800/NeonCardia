using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public static class IsoMapGeneratedMapAssetImporter
{
    private const string GeneratedMapAssetsRoot = "Assets/Generated/MapAssets";
    private const string MaterialsFolderName = "Materials";
    private const string PrefabsFolderName = "Prefabs";
    private const float SpritePixelsPerUnit = 100f;

    private static readonly string[] PlaceholderPrefabIds =
    {
        "PlatformTile_1x1",
        "Bridge_1x1",
        "Stair_Up",
        "Wall",
        "Railing",
        "SpawnMarker",
        "EncounterAreaMarker",
        "TransitionMarker"
    };

    [MenuItem("Tools/NeonCardia/Map Editor 2.5D/Import Generated Map Asset Pack", false, 41084)]
    public static void ImportPackFromMenu()
    {
        ImportPackWithFolderPanel(null);
    }

    public static IsoMapPrefabCatalog ImportPackWithFolderPanel(IsoMapPrefabCatalog targetCatalog)
    {
        EnsureFolderPath(GeneratedMapAssetsRoot);
        AssetDatabase.Refresh();

        string rootAbsolutePath = ToAbsolutePath(GeneratedMapAssetsRoot);
        string selectedAbsolutePath = EditorUtility.OpenFolderPanel("Select Generated Map Asset Pack", rootAbsolutePath, string.Empty);
        if (string.IsNullOrEmpty(selectedAbsolutePath))
        {
            return targetCatalog;
        }

        string selectedAssetPath;
        if (!TryToAssetPath(selectedAbsolutePath, out selectedAssetPath))
        {
            EditorUtility.DisplayDialog("Import Generated Map Asset Pack", "Select a folder under this Unity project.", "OK");
            return targetCatalog;
        }

        return ImportPack(selectedAssetPath, targetCatalog);
    }

    public static IsoMapPrefabCatalog ImportPack(string packFolderAssetPath, IsoMapPrefabCatalog targetCatalog)
    {
        string normalizedPackFolder = NormalizeAssetPath(packFolderAssetPath);
        if (!AssetDatabase.IsValidFolder(normalizedPackFolder))
        {
            Debug.LogWarning("[IsoMapGeneratedMapAssetImporter] Pack folder was not found: " + normalizedPackFolder);
            return targetCatalog;
        }

        if (!IsUnderGeneratedMapAssetsRoot(normalizedPackFolder))
        {
            EditorUtility.DisplayDialog(
                "Import Generated Map Asset Pack",
                "Pack folder must be under " + GeneratedMapAssetsRoot + ".\nSelected: " + normalizedPackFolder,
                "OK");
            return targetCatalog;
        }

        IsoMapPrefabCatalog catalog = ResolveCatalog(targetCatalog, normalizedPackFolder);
        if (catalog == null)
        {
            Debug.LogWarning("[IsoMapGeneratedMapAssetImporter] Import canceled because no IsoMapPrefabCatalog could be resolved.");
            return targetCatalog;
        }

        List<GeneratedMapAssetSource> sources = FindSourcePngs(normalizedPackFolder);
        if (sources.Count == 0)
        {
            Debug.LogWarning("[IsoMapGeneratedMapAssetImporter] No categorized PNG files were found in " + normalizedPackFolder + ".");
            return catalog;
        }

        string materialsFolder = normalizedPackFolder + "/" + MaterialsFolderName;
        string prefabsFolder = normalizedPackFolder + "/" + PrefabsFolderName;
        EnsureFolderPath(materialsFolder);
        EnsureFolderPath(prefabsFolder);

        int importedCount = 0;
        for (int i = 0; i < sources.Count; i++)
        {
            GeneratedMapAssetSource source = sources[i];
            ConfigureTextureImporter(source.TextureAssetPath);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(source.TextureAssetPath);
            if (texture == null)
            {
                Debug.LogWarning("[IsoMapGeneratedMapAssetImporter] Skipped missing texture asset: " + source.TextureAssetPath);
                continue;
            }

            Material material = CreateOrUpdateMaterial(materialsFolder, source.PrefabId, texture);
            GameObject prefab = CreateOrUpdatePrefab(prefabsFolder, source, material);
            if (prefab == null)
            {
                continue;
            }

            UpsertCatalogEntry(catalog, source, prefab);
            importedCount++;
        }

        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[IsoMapGeneratedMapAssetImporter] Imported " + importedCount + " generated map PNG(s) into " + catalog.name + ".");
        return catalog;
    }

    private static List<GeneratedMapAssetSource> FindSourcePngs(string packFolderAssetPath)
    {
        List<GeneratedMapAssetSource> sources = new List<GeneratedMapAssetSource>();
        string absolutePackFolder = ToAbsolutePath(packFolderAssetPath);
        string[] pngFiles = Directory.GetFiles(absolutePackFolder, "*.png", SearchOption.AllDirectories);
        Array.Sort(pngFiles, StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < pngFiles.Length; i++)
        {
            string assetPath;
            if (!TryToAssetPath(pngFiles[i], out assetPath))
            {
                continue;
            }

            assetPath = NormalizeAssetPath(assetPath);
            if (IsGeneratedOutputPath(assetPath))
            {
                continue;
            }

            IsoMapPrefabCategory category;
            if (!TryResolveCategory(packFolderAssetPath, assetPath, out category))
            {
                Debug.LogWarning("[IsoMapGeneratedMapAssetImporter] Skipped PNG with unknown category: " + assetPath);
                continue;
            }

            string prefabId = BuildPrefabId(category, Path.GetFileNameWithoutExtension(assetPath));
            sources.Add(new GeneratedMapAssetSource(assetPath, prefabId, ObjectNames.NicifyVariableName(prefabId), category));
        }

        return sources;
    }

    private static bool TryResolveCategory(string packFolderAssetPath, string pngAssetPath, out IsoMapPrefabCategory category)
    {
        string relativePath = pngAssetPath.Substring(packFolderAssetPath.Length).TrimStart('/');
        int slashIndex = relativePath.IndexOf('/');
        if (slashIndex > 0 && TryParseCategory(relativePath.Substring(0, slashIndex), out category))
        {
            return true;
        }

        string fileName = Path.GetFileNameWithoutExtension(relativePath);
        int separatorIndex = fileName.IndexOfAny(new[] { '_', '-', ' ' });
        string prefix = separatorIndex > 0 ? fileName.Substring(0, separatorIndex) : fileName;
        return TryParseCategory(prefix, out category);
    }

    private static bool TryParseCategory(string value, out IsoMapPrefabCategory category)
    {
        category = IsoMapPrefabCategory.Prop;
        string normalized = SanitizeToken(value).ToLowerInvariant();
        switch (normalized)
        {
            case "floor":
            case "floors":
                category = IsoMapPrefabCategory.Floor;
                return true;
            case "bridge":
            case "bridges":
                category = IsoMapPrefabCategory.Bridge;
                return true;
            case "stair":
            case "stairs":
                category = IsoMapPrefabCategory.Stair;
                return true;
            case "wall":
            case "walls":
                category = IsoMapPrefabCategory.Wall;
                return true;
            case "railing":
            case "railings":
                category = IsoMapPrefabCategory.Railing;
                return true;
            case "prop":
            case "props":
                category = IsoMapPrefabCategory.Prop;
                return true;
            case "marker":
            case "markers":
                category = IsoMapPrefabCategory.Marker;
                return true;
            default:
                return false;
        }
    }

    private static void ConfigureTextureImporter(string textureAssetPath)
    {
        AssetDatabase.ImportAsset(textureAssetPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(textureAssetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.spritePixelsPerUnit = SpritePixelsPerUnit;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        settings.spritePivot = new Vector2(0.5f, 0.5f);
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
    }

    private static Material CreateOrUpdateMaterial(string materialsFolder, string prefabId, Texture2D texture)
    {
        string materialPath = materialsFolder + "/" + prefabId + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(FindTransparentShader());
            AssetDatabase.CreateAsset(material, materialPath);
        }
        else if (material.shader == null)
        {
            material.shader = FindTransparentShader();
        }

        material.mainTexture = texture;
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
        }

        if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", texture);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", Color.white);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", Color.white);
        }

        ConfigureTransparentMaterial(material);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject CreateOrUpdatePrefab(string prefabsFolder, GeneratedMapAssetSource source, Material material)
    {
        string prefabPath = prefabsFolder + "/" + source.PrefabId + ".prefab";
        GameObject root = new GameObject(source.PrefabId);
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = "Visual_Quad";
        quad.transform.SetParent(root.transform, false);
        ApplyQuadTransform(quad.transform, source.Category);

        Collider collider = quad.GetComponent<Collider>();
        if (collider != null)
        {
            Object.DestroyImmediate(collider);
        }

        MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void ApplyQuadTransform(Transform quadTransform, IsoMapPrefabCategory category)
    {
        quadTransform.localScale = Vector3.one;
        switch (category)
        {
            case IsoMapPrefabCategory.Floor:
            case IsoMapPrefabCategory.Bridge:
            case IsoMapPrefabCategory.Stair:
                quadTransform.localPosition = new Vector3(0f, 0.01f, 0f);
                quadTransform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                break;
            case IsoMapPrefabCategory.Marker:
                quadTransform.localPosition = new Vector3(0f, 0.04f, 0f);
                quadTransform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                break;
            case IsoMapPrefabCategory.Wall:
            case IsoMapPrefabCategory.Railing:
                quadTransform.localPosition = new Vector3(0f, 0.5f, 0.45f);
                quadTransform.localRotation = Quaternion.identity;
                break;
            default:
                quadTransform.localPosition = new Vector3(0f, 0.5f, 0f);
                quadTransform.localRotation = Quaternion.identity;
                break;
        }
    }

    private static void UpsertCatalogEntry(IsoMapPrefabCatalog catalog, GeneratedMapAssetSource source, GameObject prefab)
    {
        IsoMapPrefabCatalogEntry entry = catalog.FindEntry(source.PrefabId);
        bool defaultWalkable;
        bool defaultBlocksMovement;
        Vector3 defaultOffset;
        GetCatalogDefaults(source.Category, out defaultWalkable, out defaultBlocksMovement, out defaultOffset);

        string notes = "Imported external PNG: " + source.TextureAssetPath;
        if (entry == null)
        {
            catalog.Entries.Add(new IsoMapPrefabCatalogEntry(
                source.PrefabId,
                source.DisplayName,
                prefab,
                source.Category,
                defaultWalkable,
                defaultBlocksMovement,
                true,
                0,
                defaultOffset,
                notes));
            return;
        }

        entry.DisplayName = source.DisplayName;
        entry.Prefab = prefab;
        entry.Category = source.Category;
        entry.DefaultWalkable = defaultWalkable;
        entry.DefaultBlocksMovement = defaultBlocksMovement;
        entry.DefaultBlocksMovementConfigured = true;
        entry.DefaultRotationY = 0;
        entry.DefaultOffset = defaultOffset;
        entry.Notes = notes;
        entry.NormalizeSerializedValues();
    }

    private static void GetCatalogDefaults(
        IsoMapPrefabCategory category,
        out bool defaultWalkable,
        out bool defaultBlocksMovement,
        out Vector3 defaultOffset)
    {
        defaultOffset = Vector3.zero;
        switch (category)
        {
            case IsoMapPrefabCategory.Floor:
            case IsoMapPrefabCategory.Bridge:
            case IsoMapPrefabCategory.Stair:
                defaultWalkable = true;
                defaultBlocksMovement = false;
                break;
            case IsoMapPrefabCategory.Marker:
                defaultWalkable = true;
                defaultBlocksMovement = false;
                defaultOffset = new Vector3(0f, 0.04f, 0f);
                break;
            case IsoMapPrefabCategory.Wall:
            case IsoMapPrefabCategory.Railing:
            case IsoMapPrefabCategory.Prop:
            default:
                defaultWalkable = false;
                defaultBlocksMovement = true;
                break;
        }
    }

    private static IsoMapPrefabCatalog ResolveCatalog(IsoMapPrefabCatalog targetCatalog, string packFolderAssetPath)
    {
        if (targetCatalog != null)
        {
            return targetCatalog;
        }

        IsoMapPrefabCatalog catalog = AssetDatabase.LoadAssetAtPath<IsoMapPrefabCatalog>(IsoMapPlaceholderPrefabBuilder.CatalogAssetPath);
        if (catalog != null)
        {
            return catalog;
        }

        string packId = Path.GetFileName(packFolderAssetPath.TrimEnd('/'));
        string catalogPath = packFolderAssetPath + "/" + SanitizeToken(packId) + "_IsoMapPrefabCatalog.asset";
        catalog = ScriptableObject.CreateInstance<IsoMapPrefabCatalog>();
        AssetDatabase.CreateAsset(catalog, catalogPath);
        return catalog;
    }

    private static Shader FindTransparentShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Transparent");
        }

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        return shader;
    }

    private static void ConfigureTransparentMaterial(Material material)
    {
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        if (material.HasProperty("_Blend"))
        {
            material.SetFloat("_Blend", 0f);
        }

        if (material.HasProperty("_AlphaClip"))
        {
            material.SetFloat("_AlphaClip", 0f);
        }

        if (material.HasProperty("_Cull"))
        {
            material.SetFloat("_Cull", (float)CullMode.Off);
        }

        if (material.HasProperty("_SrcBlend"))
        {
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        }

        if (material.HasProperty("_DstBlend"))
        {
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        }

        if (material.HasProperty("_ZWrite"))
        {
            material.SetFloat("_ZWrite", 0f);
        }

        material.renderQueue = (int)RenderQueue.Transparent;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
    }

    private static bool IsGeneratedOutputPath(string assetPath)
    {
        return assetPath.IndexOf("/" + MaterialsFolderName + "/", StringComparison.OrdinalIgnoreCase) >= 0
            || assetPath.IndexOf("/" + PrefabsFolderName + "/", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool IsUnderGeneratedMapAssetsRoot(string assetPath)
    {
        return string.Equals(assetPath, GeneratedMapAssetsRoot, StringComparison.Ordinal)
            || assetPath.StartsWith(GeneratedMapAssetsRoot + "/", StringComparison.Ordinal);
    }

    private static string BuildPrefabId(IsoMapPrefabCategory category, string fileNameWithoutExtension)
    {
        string baseId = SanitizeToken(fileNameWithoutExtension);
        if (string.IsNullOrEmpty(baseId))
        {
            baseId = category + "_ImportedAsset";
        }

        if (IsPlaceholderPrefabId(baseId) || HasCategoryPrefix(baseId))
        {
            return baseId;
        }

        return category + "_" + baseId;
    }

    private static bool IsPlaceholderPrefabId(string prefabId)
    {
        for (int i = 0; i < PlaceholderPrefabIds.Length; i++)
        {
            if (string.Equals(prefabId, PlaceholderPrefabIds[i], StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasCategoryPrefix(string prefabId)
    {
        IsoMapPrefabCategory ignored;
        int separatorIndex = prefabId.IndexOf('_');
        if (separatorIndex <= 0)
        {
            return false;
        }

        return TryParseCategory(prefabId.Substring(0, separatorIndex), out ignored);
    }

    private static string SanitizeToken(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        char[] buffer = new char[value.Length];
        int length = 0;
        bool previousWasUnderscore = false;
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            bool valid = IsAsciiLetterOrDigit(c);
            if (valid)
            {
                buffer[length++] = c;
                previousWasUnderscore = false;
            }
            else if ((c == '_' || c == '-' || char.IsWhiteSpace(c)) && !previousWasUnderscore && length > 0)
            {
                buffer[length++] = '_';
                previousWasUnderscore = true;
            }
        }

        while (length > 0 && buffer[length - 1] == '_')
        {
            length--;
        }

        string sanitized = new string(buffer, 0, length);
        if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
        {
            sanitized = "Asset_" + sanitized;
        }

        return sanitized;
    }

    private static bool IsAsciiLetterOrDigit(char value)
    {
        return (value >= 'A' && value <= 'Z')
            || (value >= 'a' && value <= 'z')
            || (value >= '0' && value <= '9');
    }

    private static string NormalizeAssetPath(string path)
    {
        return path.Replace('\\', '/').TrimEnd('/');
    }

    private static bool TryToAssetPath(string absolutePath, out string assetPath)
    {
        string normalizedAbsolutePath = Path.GetFullPath(absolutePath).Replace('\\', '/').TrimEnd('/');
        string normalizedDataPath = Application.dataPath.Replace('\\', '/').TrimEnd('/');
        if (string.Equals(normalizedAbsolutePath, normalizedDataPath, StringComparison.OrdinalIgnoreCase))
        {
            assetPath = "Assets";
            return true;
        }

        if (normalizedAbsolutePath.StartsWith(normalizedDataPath + "/", StringComparison.OrdinalIgnoreCase))
        {
            assetPath = "Assets/" + normalizedAbsolutePath.Substring(normalizedDataPath.Length + 1);
            return true;
        }

        assetPath = string.Empty;
        return false;
    }

    private static string ToAbsolutePath(string assetPath)
    {
        string normalizedAssetPath = NormalizeAssetPath(assetPath);
        if (string.Equals(normalizedAssetPath, "Assets", StringComparison.Ordinal))
        {
            return Application.dataPath;
        }

        return Path.Combine(Application.dataPath, normalizedAssetPath.Substring("Assets/".Length)).Replace('\\', '/');
    }

    private static void EnsureFolderPath(string path)
    {
        string[] parts = NormalizeAssetPath(path).Split('/');
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

    private struct GeneratedMapAssetSource
    {
        public readonly string TextureAssetPath;
        public readonly string PrefabId;
        public readonly string DisplayName;
        public readonly IsoMapPrefabCategory Category;

        public GeneratedMapAssetSource(string textureAssetPath, string prefabId, string displayName, IsoMapPrefabCategory category)
        {
            TextureAssetPath = textureAssetPath;
            PrefabId = prefabId;
            DisplayName = displayName;
            Category = category;
        }
    }
}
