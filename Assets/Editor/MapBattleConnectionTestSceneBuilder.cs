using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MapBattleConnectionTestSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/MapBattleConnectionTest.unity";

    [MenuItem("Tools/NeonCardia/Create Map Battle Connection Test Scene")]
    public static void CreateSceneAndRegister()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "MapBattleConnectionTest";

        GameObject bootstrapObject = new GameObject("MapBattleConnectionTestBootstrap");
        bootstrapObject.AddComponent<MapBattleConnectionTestBootstrap>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);

        Debug.Log("Created MapBattleConnectionTest scene and registered it in Build Settings.");
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
