using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class IsoMapRuntimeTestSceneBuilder
{
    public const string ScenePath = "Assets/Scenes/IsoMapRuntimeTest.unity";

    [MenuItem("Tools/NeonCardia/Map Editor 2.5D/Create IsoMap Runtime Test Scene", false, 41083)]
    public static void CreateSceneAndRegister()
    {
        IsoMapData sampleMapData = IsoMapSampleMapBuilder.CreateOrUpdateSampleSfFloatingMap();
        IsoMapPrefabCatalog catalog = AssetDatabase.LoadAssetAtPath<IsoMapPrefabCatalog>(IsoMapPlaceholderPrefabBuilder.CatalogAssetPath);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "IsoMapRuntimeTest";

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        cameraObject.AddComponent<AudioListener>();

        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

        GameObject bootstrapObject = new GameObject("IsoMapRuntimeBootstrap");
        IsoMapRuntimeController controller = bootstrapObject.AddComponent<IsoMapRuntimeController>();
        SerializedObject serializedController = new SerializedObject(controller);
        serializedController.FindProperty("mapData").objectReferenceValue = sampleMapData;
        serializedController.FindProperty("prefabCatalog").objectReferenceValue = catalog;
        serializedController.FindProperty("battleSceneName").stringValue = "BattleScene";
        serializedController.FindProperty("fallbackEnemyGroupId").stringValue = "sf_floating_test_group";
        serializedController.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        AddSceneToBuildSettings("Assets/Scenes/BattleScene.unity");

        Debug.Log("Created IsoMapRuntimeTest scene and registered it in Build Settings.");
    }

    public static void CreateSceneAndRegisterFromCommandLine()
    {
        CreateSceneAndRegister();
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i] != null && scenes[i].path == scenePath)
            {
                scenes[i] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = scenes.ToArray();
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
