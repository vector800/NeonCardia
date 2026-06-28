using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class IsoMapPlaytestEditorLifecycle
{
    static IsoMapPlaytestEditorLifecycle()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode)
        {
            return;
        }

        IsoMapPlaytestSettings settings;
        if (!IsoMapPlaytestSettings.TryLoad(out settings))
        {
            return;
        }

        try
        {
            RestorePreviousSceneIfRequested(settings);
        }
        finally
        {
            IsoMapPlaytestSettings.Clear();
            Debug.Log("[IsoMapPlaytest] Cleared Play From Here settings after PlayMode.");
        }
    }

    private static void RestorePreviousSceneIfRequested(IsoMapPlaytestSettings settings)
    {
        if (!settings.ReturnToPreviousScene || string.IsNullOrEmpty(settings.PreviousScenePath))
        {
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        if (string.Equals(currentScene.path, settings.PreviousScenePath, System.StringComparison.Ordinal))
        {
            return;
        }

        SceneAsset previousScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(settings.PreviousScenePath);
        if (previousScene == null)
        {
            Debug.LogWarning("[IsoMapPlaytest] Previous Scene could not be restored because it no longer exists: " + settings.PreviousScenePath);
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            Debug.LogWarning("[IsoMapPlaytest] Previous Scene restore skipped because the current Scene was not saved.");
            return;
        }

        EditorSceneManager.OpenScene(settings.PreviousScenePath, OpenSceneMode.Single);
        Debug.Log("[IsoMapPlaytest] Restored previous Scene after PlayMode: " + settings.PreviousScenePath);
    }
}
