using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MapBattleConnectionTestBootstrap : MonoBehaviour
{
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private string enemyGroupId = "test_area_normal_01";
    [SerializeField] private string encounterAreaId = "test_grid_area";
    [SerializeField] private int mapWidth = 10;
    [SerializeField] private int mapHeight = 6;
    [SerializeField] private int minStepsBeforeEncounter = 5;
    [SerializeField] private float encounterChancePerStep = 0.10f;
    [SerializeField] private int encounterCooldownStepsAfterReturn = 5;
    [SerializeField] private float encounterCooldownSecondsAfterReturn = 3f;
    [SerializeField] private float moveSeconds = 0.12f;

    private const float TileSize = 1f;
    private static readonly Vector2Int DefaultStartPosition = new Vector2Int(1, 1);

    private bool[,] blockedTiles;
    private Vector2Int playerGridPosition;
    private GameObject playerObject;
    private Text hudText;
    private bool moving;
    private bool encounterStarting;
    private int totalSteps;
    private int stepsSinceEncounterStart;
    private int cooldownStepsRemaining;
    private float cooldownUntilTime;
    private Sprite runtimeSquareSprite;

    private void Start()
    {
        mapWidth = Mathf.Max(3, mapWidth);
        mapHeight = Mathf.Max(3, mapHeight);
        blockedTiles = new bool[mapWidth, mapHeight];
        BuildBlockedTileMap();

        playerGridPosition = DefaultStartPosition;
        ApplyReturnResultIfNeeded();
        ClampPlayerPositionToWalkable();

        BuildRuntimeScene();
        RefreshHud();

        Debug.Log("[MapBattleConnection] MapBattleConnectionTest ready. Move with WASD/Arrow keys. TEST ONLY: press E to force an encounter.");
    }

    private void Update()
    {
        if (encounterStarting || moving)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        // TEST ONLY: force a random-battle transition without waiting for the step roll.
        if (keyboard.eKey.wasPressedThisFrame)
        {
            StartEncounter(true);
            return;
        }

        Vector2Int direction = Vector2Int.zero;
        if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
        {
            direction = Vector2Int.left;
        }
        else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
        {
            direction = Vector2Int.right;
        }
        else if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
        {
            direction = Vector2Int.up;
        }
        else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
        {
            direction = Vector2Int.down;
        }

        if (direction != Vector2Int.zero)
        {
            TryStartMove(direction);
        }
    }

    private void ApplyReturnResultIfNeeded()
    {
        BattleConnectionResultData resultData;
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (!BattleConnectionContext.TryConsumeResultForScene(activeSceneName, out resultData))
        {
            return;
        }

        playerGridPosition = resultData.ReturnPosition;
        totalSteps = resultData.StepCountAtEncounter;
        stepsSinceEncounterStart = 0;
        cooldownStepsRemaining = encounterCooldownStepsAfterReturn;
        cooldownUntilTime = Time.time + encounterCooldownSecondsAfterReturn;

        Debug.Log("[MapBattleConnection] Returned from BattleScene. result="
            + resultData.ResultType
            + " enemyGroupId=" + resultData.EnemyGroupId
            + " returnPosition=" + resultData.ReturnPosition
            + " cooldownSteps=" + cooldownStepsRemaining
            + " cooldownSeconds=" + encounterCooldownSecondsAfterReturn.ToString("0.0"));
    }

    private void BuildBlockedTileMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                blockedTiles[x, y] = x == 0 || y == 0 || x == mapWidth - 1 || y == mapHeight - 1;
            }
        }

        SetBlockedIfInside(4, 3, true);
        SetBlockedIfInside(5, 3, true);
    }

    private void SetBlockedIfInside(int x, int y, bool blocked)
    {
        if (x >= 0 && x < mapWidth && y >= 0 && y < mapHeight)
        {
            blockedTiles[x, y] = blocked;
        }
    }

    private void BuildRuntimeScene()
    {
        runtimeSquareSprite = CreateRuntimeSquareSprite();

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        mainCamera.orthographic = true;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color(0.025f, 0.028f, 0.035f, 1f);
        mainCamera.transform.position = new Vector3((mapWidth - 1) * 0.5f, (mapHeight - 1) * 0.5f, -10f);
        mainCamera.orthographicSize = Mathf.Max(3.8f, mapHeight * 0.65f);

        GameObject gridRoot = new GameObject("Runtime Test Grid");
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                CreateTile(gridRoot.transform, x, y, blockedTiles[x, y]);
            }
        }

        playerObject = new GameObject("Runtime Test Player");
        SpriteRenderer playerRenderer = playerObject.AddComponent<SpriteRenderer>();
        playerRenderer.sprite = runtimeSquareSprite;
        playerRenderer.color = new Color(0.20f, 0.86f, 1f, 1f);
        playerRenderer.sortingOrder = 10;
        playerObject.transform.localScale = new Vector3(0.58f, 0.58f, 1f);
        playerObject.transform.position = GridToWorld(playerGridPosition);

        BuildHud();
    }

    private void CreateTile(Transform parent, int x, int y, bool blocked)
    {
        GameObject tile = new GameObject(blocked ? "Wall Tile" : "Floor Tile");
        tile.transform.SetParent(parent, false);
        tile.transform.position = GridToWorld(new Vector2Int(x, y));
        tile.transform.localScale = new Vector3(0.92f, 0.92f, 1f);

        SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
        renderer.sprite = runtimeSquareSprite;
        renderer.color = blocked
            ? new Color(0.18f, 0.20f, 0.25f, 1f)
            : new Color(0.060f, 0.095f, 0.105f, 1f);
        renderer.sortingOrder = blocked ? 1 : 0;
    }

    private void BuildHud()
    {
        GameObject canvasObject = new GameObject("Runtime Test HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject panelObject = new GameObject("Status Panel");
        panelObject.transform.SetParent(canvasObject.transform, false);
        Image panel = panelObject.AddComponent<Image>();
        panel.color = new Color(0.015f, 0.018f, 0.024f, 0.86f);
        RectTransform panelRect = panel.rectTransform;
        panelRect.anchorMin = new Vector2(0.02f, 0.78f);
        panelRect.anchorMax = new Vector2(0.38f, 0.97f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        GameObject textObject = new GameObject("Status Text");
        textObject.transform.SetParent(panelObject.transform, false);
        hudText = textObject.AddComponent<Text>();
        hudText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (hudText.font == null)
        {
            hudText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        hudText.fontSize = 16;
        hudText.alignment = TextAnchor.MiddleLeft;
        hudText.color = new Color(0.88f, 0.96f, 1f, 1f);
        hudText.horizontalOverflow = HorizontalWrapMode.Wrap;
        hudText.verticalOverflow = VerticalWrapMode.Truncate;

        RectTransform textRect = hudText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(14f, 8f);
        textRect.offsetMax = new Vector2(-14f, -8f);
    }

    private Sprite CreateRuntimeSquareSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.name = "RuntimeSquareTexture";
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        sprite.name = "RuntimeSquareSprite";
        sprite.hideFlags = HideFlags.HideAndDontSave;
        return sprite;
    }

    private void TryStartMove(Vector2Int direction)
    {
        Vector2Int next = playerGridPosition + direction;
        if (!IsWalkable(next))
        {
            Debug.Log("[MapBattleConnection] Move blocked. current=" + playerGridPosition + " requested=" + next);
            return;
        }

        StartCoroutine(MovePlayer(next));
    }

    private IEnumerator MovePlayer(Vector2Int next)
    {
        moving = true;

        Vector3 from = playerObject.transform.position;
        Vector3 to = GridToWorld(next);
        float elapsed = 0f;
        while (elapsed < moveSeconds)
        {
            elapsed += Time.deltaTime;
            float t = moveSeconds <= 0f ? 1f : Mathf.Clamp01(elapsed / moveSeconds);
            playerObject.transform.position = Vector3.Lerp(from, to, SmoothStep(t));
            yield return null;
        }

        playerObject.transform.position = to;
        playerGridPosition = next;
        moving = false;

        OnStepCompleted();
    }

    private float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private void OnStepCompleted()
    {
        totalSteps++;
        RefreshHud();

        if (cooldownStepsRemaining > 0)
        {
            cooldownStepsRemaining--;
            Debug.Log("[MapBattleConnection] Step " + totalSteps + " completed. Encounter skipped by return cooldown steps. remaining=" + cooldownStepsRemaining);
            RefreshHud();
            return;
        }

        if (Time.time < cooldownUntilTime)
        {
            float remaining = cooldownUntilTime - Time.time;
            Debug.Log("[MapBattleConnection] Step " + totalSteps + " completed. Encounter skipped by return cooldown time. remainingSeconds=" + remaining.ToString("0.00"));
            return;
        }

        stepsSinceEncounterStart++;
        if (stepsSinceEncounterStart < minStepsBeforeEncounter)
        {
            Debug.Log("[MapBattleConnection] Step " + totalSteps + " completed. Encounter check not started. stepsSinceEncounterStart="
                + stepsSinceEncounterStart + "/" + minStepsBeforeEncounter);
            return;
        }

        float roll = Random.value;
        bool encounterHit = roll < encounterChancePerStep;
        Debug.Log("[MapBattleConnection] Step " + totalSteps + " encounter check. stepsSinceEncounterStart="
            + stepsSinceEncounterStart
            + " roll=" + roll.ToString("0.000")
            + " chance=" + encounterChancePerStep.ToString("0.000")
            + " result=" + (encounterHit ? "ENCOUNTER" : "none"));

        if (encounterHit)
        {
            StartEncounter(false);
        }
    }

    private void StartEncounter(bool forced)
    {
        if (encounterStarting)
        {
            return;
        }

        encounterStarting = true;
        string returnSceneName = SceneManager.GetActiveScene().name;
        BattleConnectionContext.BeginBattle(new BattleStartData(enemyGroupId, returnSceneName, playerGridPosition, encounterAreaId, totalSteps));

        Debug.Log("[MapBattleConnection] " + (forced ? "TEST ONLY forced encounter" : "Random encounter")
            + " started. battleScene=" + battleSceneName
            + " enemyGroupId=" + enemyGroupId
            + " returnScene=" + returnSceneName
            + " returnPosition=" + playerGridPosition
            + " totalSteps=" + totalSteps);

        SceneManager.LoadScene(battleSceneName);
    }

    private bool IsWalkable(Vector2Int position)
    {
        return position.x >= 0
            && position.x < mapWidth
            && position.y >= 0
            && position.y < mapHeight
            && !blockedTiles[position.x, position.y];
    }

    private void ClampPlayerPositionToWalkable()
    {
        if (IsWalkable(playerGridPosition))
        {
            return;
        }

        playerGridPosition = DefaultStartPosition;
        if (IsWalkable(playerGridPosition))
        {
            return;
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector2Int candidate = new Vector2Int(x, y);
                if (IsWalkable(candidate))
                {
                    playerGridPosition = candidate;
                    return;
                }
            }
        }
    }

    private Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * TileSize, gridPosition.y * TileSize, 0f);
    }

    private void RefreshHud()
    {
        if (hudText == null)
        {
            return;
        }

        int displayedCooldownSteps = Mathf.Max(0, cooldownStepsRemaining);
        float displayedCooldownSeconds = Mathf.Max(0f, cooldownUntilTime - Time.time);
        hudText.text = "MapBattleConnectionTest"
            + "\nPOS " + playerGridPosition.x + "," + playerGridPosition.y
            + "   STEPS " + totalSteps
            + "\nENCOUNTER " + Mathf.RoundToInt(encounterChancePerStep * 100f) + "% after " + minStepsBeforeEncounter + " steps"
            + "\nCOOLDOWN " + displayedCooldownSteps + " steps / " + displayedCooldownSeconds.ToString("0.0") + "s"
            + "\nE: force encounter";
    }
}
