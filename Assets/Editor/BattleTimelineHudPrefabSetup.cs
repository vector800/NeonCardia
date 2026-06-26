using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BattleTimelineHudPrefabSetup
{
    private const string PrefabPath = "Assets/Prefabs/UI/BattleTimelineHud.prefab";
    private const string BattleScenePath = "Assets/Scenes/BattleScene.unity";
    private const string TimelineSheetPath = "Assets/Art/UI/Battle/Timeline/TimelineIconSheet.png";

    private static readonly string[] FrameSpriteNames =
    {
        "Timeline_Ally_Active",
        "Timeline_Ally_Selected",
        "Timeline_Ally_Normal",
        "Timeline_Ally_Dim",
        "Timeline_Enemy_Active",
        "Timeline_Enemy_Selected",
        "Timeline_Enemy_Normal",
        "Timeline_Enemy_Dim"
    };

    private static readonly string[] FaceIconPaths =
    {
        "Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_01.png",
        "Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_02.png",
        "Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_03.png",
        "Assets/Art/UI/Battle/Timeline/FaceIcons/EnemyFaceIcon_01.png",
        "Assets/Art/UI/Battle/Timeline/FaceIcons/EnemyFaceIcon_02.png",
        "Assets/Art/UI/Battle/Timeline/FaceIcons/EnemyFaceIcon_03.png",
        "Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_01.png",
        "Assets/Art/UI/Battle/Timeline/FaceIcons/EnemyFaceIcon_01.png"
    };

    [MenuItem("Tools/NeonCardia/Apply Battle Timeline HUD Prefab Setup")]
    public static void Apply()
    {
        int assignedFrames = ApplyPrefabSprites();
        bool linkedScene = LinkBattleSceneManager();
        AssetDatabase.SaveAssets();
        Debug.Log("BattleTimelineHud prefab setup complete. Frame sprites: " + assignedFrames + "/8, BattleScene linked: " + linkedScene + ".");
    }

    private static int ApplyPrefabSprites()
    {
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
        int assignedFrames = 0;
        try
        {
            for (int i = 0; i < FrameSpriteNames.Length; i++)
            {
                Image background = FindImage(prefabRoot.transform, "SlotsRoot/Slot_" + (i + 1).ToString("00"));
                Sprite frameSprite = FindTimelineSprite(FrameSpriteNames[i]);
                if (background != null && frameSprite != null)
                {
                    background.sprite = frameSprite;
                    background.color = Color.white;
                    background.type = Image.Type.Simple;
                    background.preserveAspect = false;
                    EditorUtility.SetDirty(background);
                    assignedFrames++;
                }

                Image icon = FindImage(prefabRoot.transform, "SlotsRoot/Slot_" + (i + 1).ToString("00") + "/Icon");
                Sprite faceSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FaceIconPaths[i]);
                if (icon != null && faceSprite != null)
                {
                    icon.sprite = faceSprite;
                    icon.color = Color.white;
                    icon.preserveAspect = true;
                    EditorUtility.SetDirty(icon);
                }
            }

            BattleTimelineHudView hudView = prefabRoot.GetComponent<BattleTimelineHudView>();
            if (hudView != null)
            {
                hudView.CacheReferences();
                hudView.HideCurrentHpDisplay();
                EditorUtility.SetDirty(hudView);
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        return assignedFrames;
    }

    private static bool LinkBattleSceneManager()
    {
        Scene activeScene = EditorSceneManager.GetActiveScene();
        if (!string.Equals(activeScene.path, BattleScenePath, StringComparison.Ordinal))
        {
            activeScene = EditorSceneManager.OpenScene(BattleScenePath);
        }

        GameObject prefabObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        BattleTimelineHudView prefabView = prefabObject != null ? prefabObject.GetComponent<BattleTimelineHudView>() : null;
        BattleManager battleManager = UnityEngine.Object.FindFirstObjectByType<BattleManager>();
        if (prefabView == null || battleManager == null)
        {
            return false;
        }

        SerializedObject serializedManager = new SerializedObject(battleManager);
        serializedManager.FindProperty("usePrefabActionOrderHud").boolValue = true;
        serializedManager.FindProperty("battleTimelineHudPrefab").objectReferenceValue = prefabView;
        serializedManager.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(battleManager);
        EditorSceneManager.MarkSceneDirty(activeScene);
        return EditorSceneManager.SaveScene(activeScene);
    }

    private static Sprite FindTimelineSprite(string spriteName)
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(TimelineSheetPath);
        for (int i = 0; i < assets.Length; i++)
        {
            Sprite sprite = assets[i] as Sprite;
            if (sprite != null && string.Equals(sprite.name, spriteName, StringComparison.OrdinalIgnoreCase))
            {
                return sprite;
            }
        }

        return null;
    }

    private static Image FindImage(Transform root, string path)
    {
        Transform found = root.Find(path);
        return found != null ? found.GetComponent<Image>() : null;
    }
}
