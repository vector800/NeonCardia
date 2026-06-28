using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public sealed class IsoMapPlaytestSettings
{
    public const int InitialEncounterCooldownSteps = 3;

    public string MapDataGuid;
    public string PrefabCatalogGuid;
    public Vector3Int StartCell;
    public Vector2Int StartDirection = Vector2Int.down;
    public string ForceEncounterAreaId;
    public bool ReturnToPreviousScene;
    public string PreviousScenePath;
    public string PreviousSceneName;
    public string RuntimeScenePath;
    public string StartedAtUtc;

#if UNITY_EDITOR
    private const string EditorPrefsKey = "NeonCardia.MapEditor2_5D.IsoMapPlaytestSettings";

    public static void Save(IsoMapPlaytestSettings settings)
    {
        if (settings == null)
        {
            Clear();
            return;
        }

        EditorPrefs.SetString(EditorPrefsKey, JsonUtility.ToJson(settings));
    }

    public static bool TryLoad(out IsoMapPlaytestSettings settings)
    {
        settings = null;
        string json = EditorPrefs.GetString(EditorPrefsKey, string.Empty);
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        try
        {
            settings = JsonUtility.FromJson<IsoMapPlaytestSettings>(json);
        }
        catch (ArgumentException)
        {
            settings = null;
            return false;
        }

        return settings != null;
    }

    public static void Clear()
    {
        EditorPrefs.DeleteKey(EditorPrefsKey);
    }

    public bool TryResolveAssets(out IsoMapData resolvedMapData, out IsoMapPrefabCatalog resolvedPrefabCatalog)
    {
        resolvedMapData = LoadAssetByGuid<IsoMapData>(MapDataGuid);
        resolvedPrefabCatalog = LoadAssetByGuid<IsoMapPrefabCatalog>(PrefabCatalogGuid);
        return resolvedMapData != null && resolvedPrefabCatalog != null;
    }

    private static T LoadAssetByGuid<T>(string guid) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(guid))
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guid);
        return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<T>(path);
    }
#else
    public static bool TryLoad(out IsoMapPlaytestSettings settings)
    {
        settings = null;
        return false;
    }

    public static void Clear()
    {
    }

    public bool TryResolveAssets(out IsoMapData resolvedMapData, out IsoMapPrefabCatalog resolvedPrefabCatalog)
    {
        resolvedMapData = null;
        resolvedPrefabCatalog = null;
        return false;
    }
#endif
}
