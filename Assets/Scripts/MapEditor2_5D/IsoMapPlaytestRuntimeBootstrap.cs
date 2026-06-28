using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DefaultExecutionOrder(-10000)]
public sealed class IsoMapPlaytestRuntimeBootstrap : MonoBehaviour
{
#if UNITY_EDITOR
    private const string BootstrapObjectName = "IsoMapPlaytestRuntimeBootstrap";
    private static readonly BindingFlags PrivateInstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;

    private IsoMapPlaytestSettings settings;
    private IsoMapData playtestMapInstance;
    private bool hasAppliedScene;
    private ulong lastAppliedSceneHandleRawData;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateForEditorPlaytest()
    {
        IsoMapPlaytestSettings loadedSettings;
        if (!IsoMapPlaytestSettings.TryLoad(out loadedSettings))
        {
            return;
        }

        if (GameObject.Find(BootstrapObjectName) != null)
        {
            return;
        }

        GameObject bootstrapObject = new GameObject(BootstrapObjectName);
        DontDestroyOnLoad(bootstrapObject);
        bootstrapObject.hideFlags = HideFlags.HideAndDontSave;
        bootstrapObject.AddComponent<IsoMapPlaytestRuntimeBootstrap>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void ApplyForEditorPlaytestAfterSceneLoad()
    {
        IsoMapPlaytestSettings loadedSettings;
        if (!IsoMapPlaytestSettings.TryLoad(out loadedSettings))
        {
            return;
        }

        IsoMapPlaytestRuntimeBootstrap bootstrap = GetOrCreateBootstrap();
        bootstrap.settings = loadedSettings;
        bootstrap.TryApplyToScene(SceneManager.GetActiveScene(), true);
    }

    private static IsoMapPlaytestRuntimeBootstrap GetOrCreateBootstrap()
    {
        GameObject existingObject = GameObject.Find(BootstrapObjectName);
        if (existingObject != null)
        {
            IsoMapPlaytestRuntimeBootstrap existingBootstrap = existingObject.GetComponent<IsoMapPlaytestRuntimeBootstrap>();
            if (existingBootstrap != null)
            {
                return existingBootstrap;
            }
        }

        GameObject bootstrapObject = new GameObject(BootstrapObjectName);
        DontDestroyOnLoad(bootstrapObject);
        bootstrapObject.hideFlags = HideFlags.HideAndDontSave;
        return bootstrapObject.AddComponent<IsoMapPlaytestRuntimeBootstrap>();
    }

    private void Awake()
    {
        if (!IsoMapPlaytestSettings.TryLoad(out settings))
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        TryApplyToScene(SceneManager.GetActiveScene(), false);
        StartCoroutine(TryApplyToActiveSceneNextFrame());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (playtestMapInstance != null)
        {
            Destroy(playtestMapInstance);
            playtestMapInstance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryApplyToScene(scene, true);
    }

    private IEnumerator TryApplyToActiveSceneNextFrame()
    {
        yield return null;
        TryApplyToScene(SceneManager.GetActiveScene(), true);
    }

    private void TryApplyToScene(Scene scene, bool logMissingController)
    {
        if (settings == null || !IsRuntimeTestScene(scene))
        {
            return;
        }

        if (!scene.isLoaded)
        {
            return;
        }

        ulong sceneHandleRawData = scene.handle.GetRawData();
        if (hasAppliedScene && lastAppliedSceneHandleRawData == sceneHandleRawData)
        {
            return;
        }

        IsoMapRuntimeController controller = FindControllerInScene(scene);
        if (controller == null)
        {
            if (logMissingController)
            {
                Debug.LogError("[IsoMapPlaytest] IsoMapRuntimeController was not found in runtime scene: "
                    + GetSceneLabel(scene) + " isLoaded=" + scene.isLoaded);
            }

            return;
        }

        IsoMapData sourceMapData;
        IsoMapPrefabCatalog sourcePrefabCatalog;
        if (!settings.TryResolveAssets(out sourceMapData, out sourcePrefabCatalog))
        {
            controller.enabled = false;
            Debug.LogError("[IsoMapPlaytest] Failed to resolve playtest assets. mapDataGuid="
                + settings.MapDataGuid + " prefabCatalogGuid=" + settings.PrefabCatalogGuid);
            return;
        }

        if (playtestMapInstance != null)
        {
            Destroy(playtestMapInstance);
        }

        playtestMapInstance = Instantiate(sourceMapData);
        playtestMapInstance.name = sourceMapData.name + "_Playtest";
        playtestMapInstance.DefaultSpawnCell = settings.StartCell;
        ApplyInitialEncounterCooldown(playtestMapInstance, settings.StartCell);

        SetPrivateField(controller, "mapData", playtestMapInstance);
        SetPrivateField(controller, "prefabCatalog", sourcePrefabCatalog);
        SetPrivateField(controller, "lastMoveDirection", settings.StartDirection == Vector2Int.zero ? Vector2Int.down : settings.StartDirection);
        hasAppliedScene = true;
        lastAppliedSceneHandleRawData = sceneHandleRawData;

        Debug.Log("[IsoMapPlaytest] Applied Play From Here settings. runtimeScene=" + scene.name
            + " controllerGameObject=" + controller.gameObject.name
            + " controllerPath=" + GetHierarchyPath(controller.transform)
            + " controllerScene=" + controller.gameObject.scene.name
            + " activeSelf=" + controller.gameObject.activeSelf
            + " activeInHierarchy=" + controller.gameObject.activeInHierarchy
            + " sourceMap=" + sourceMapData.MapId
            + " startCell=" + FormatCell(settings.StartCell)
            + " startDirection=" + (settings.StartDirection == Vector2Int.zero ? Vector2Int.down : settings.StartDirection)
            + " forceEncounterAreaId=" + (string.IsNullOrEmpty(settings.ForceEncounterAreaId) ? "(none)" : settings.ForceEncounterAreaId)
            + " previousScene=" + (string.IsNullOrEmpty(settings.PreviousSceneName) ? "(none)" : settings.PreviousSceneName)
            + " returnToPreviousScene=" + settings.ReturnToPreviousScene);
    }

    private bool IsRuntimeTestScene(Scene scene)
    {
        if (!string.IsNullOrEmpty(settings.RuntimeScenePath))
        {
            return string.Equals(scene.path, settings.RuntimeScenePath, StringComparison.Ordinal);
        }

        return string.Equals(scene.name, "IsoMapRuntimeTest", StringComparison.Ordinal);
    }

    private static IsoMapRuntimeController FindControllerInScene(Scene scene)
    {
        IsoMapRuntimeController[] controllers = FindObjectsByType<IsoMapRuntimeController>(
            FindObjectsInactive.Include);
        for (int i = 0; i < controllers.Length; i++)
        {
            IsoMapRuntimeController controller = controllers[i];
            if (controller != null && controller.gameObject.scene == scene)
            {
                return controller;
            }
        }

        return null;
    }

    private static void ApplyInitialEncounterCooldown(IsoMapData playtestMapData, Vector3Int startCell)
    {
        IsoMapEncounterAreaData startArea = FindEncounterAreaForCell(playtestMapData, startCell);
        if (startArea == null)
        {
            return;
        }

        int previousMinSteps = startArea.MinStepsBeforeEncounter;
        startArea.MinStepsBeforeEncounter = Mathf.Max(startArea.MinStepsBeforeEncounter, IsoMapPlaytestSettings.InitialEncounterCooldownSteps);
        if (startArea.MinStepsBeforeEncounter != previousMinSteps)
        {
            Debug.Log("[IsoMapPlaytest] Applied initial encounter cooldown. areaId="
                + startArea.EncounterAreaId
                + " minStepsBeforeEncounter=" + startArea.MinStepsBeforeEncounter);
        }
    }

    private static IsoMapEncounterAreaData FindEncounterAreaForCell(IsoMapData playtestMapData, Vector3Int cell)
    {
        if (playtestMapData == null)
        {
            return null;
        }

        for (int i = 0; i < playtestMapData.EncounterAreas.Count; i++)
        {
            IsoMapEncounterAreaData area = playtestMapData.EncounterAreas[i];
            if (area != null && area.Cells.Contains(cell))
            {
                return area;
            }
        }

        return null;
    }

    private static void SetPrivateField<T>(IsoMapRuntimeController controller, string fieldName, T value)
    {
        FieldInfo field = typeof(IsoMapRuntimeController).GetField(fieldName, PrivateInstanceFlags);
        if (field == null)
        {
            Debug.LogError("[IsoMapPlaytest] IsoMapRuntimeController field was not found: " + fieldName);
            return;
        }

        field.SetValue(controller, value);
    }

    private static string FormatCell(Vector3Int cell)
    {
        return "x:" + cell.x + " z:" + cell.z + " level:" + cell.y;
    }

    private static string GetHierarchyPath(Transform transform)
    {
        if (transform == null)
        {
            return "(none)";
        }

        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }

        return path;
    }

    private static string GetSceneLabel(Scene scene)
    {
        return string.IsNullOrEmpty(scene.path) ? scene.name : scene.path;
    }
#endif
}
