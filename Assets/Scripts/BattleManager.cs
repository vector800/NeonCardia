using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class BattleManager : MonoBehaviour
{
    [SerializeField] private bool showDebugLabels;
    [SerializeField] private bool usePrefabActionOrderHud = true;
    [SerializeField] private BattleTimelineHudView battleTimelineHudPrefab;
    [SerializeField] private BattlePartyStatusHUD battlePartyStatusHudPrefab;
    [SerializeField] private BattlePartyStatusPanelController battlePartyStatusPanelPrefab;

    private EnemyType debugEnemyType = EnemyType.NormalEnemy;

    private void Awake()
    {
        BattleSceneTimelineController controller = GetComponent<BattleSceneTimelineController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<BattleSceneTimelineController>();
        }

        controller.InitializeFromBattleManager(showDebugLabels, usePrefabActionOrderHud, battleTimelineHudPrefab, battlePartyStatusHudPrefab, battlePartyStatusPanelPrefab);
    }

    public void DebugSetPanelType(BattleGridPosition position, PanelType panelType)
    {
        Debug.Log("BattleScene timeline mode does not edit legacy panels. Requested " + panelType + " at " + position + ".");
    }

    public void DebugApplyPanelPreset(PanelDebugPreset preset)
    {
        Debug.Log("BattleScene timeline mode keeps the enemy grid separate from legacy panel presets. Requested preset: " + preset + ".");
    }

    public void DebugResetBattleToInitialState()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public EnemyType DebugGetEnemyType()
    {
        return debugEnemyType;
    }

    public void DebugChangeEnemyType(EnemyType enemyType)
    {
        debugEnemyType = enemyType;
        Debug.Log("BattleScene timeline mode currently uses its fixed 1-3 enemy grid. Debug enemy type set for legacy tooling only: " + enemyType + ".");
    }

    public string DebugGetEnemySummary(EnemyType enemyType)
    {
        return EnemyAI.GetDebugSummary(enemyType);
    }
}

internal sealed class LegacyBattleManager : MonoBehaviour
{
    private const int MaxHandSize = 5;
    private const int MaxPlayerActions = 3;
    private const int MaxAccelGauge = 100;
    private const int WeaponPower = 10;
    private const int PlayerSideIndex = 0;
    private const int EnemySideIndex = 1;
    private const string WeaponDisplayName = "ウエポン";
    private const CardAttribute WeaponAttribute = CardAttribute.Neutral;
    private static int previousBattleAccelGauge;

    [SerializeField] private List<CardData> starterDeck = new List<CardData>();
    [SerializeField] private EnemyType enemyType = EnemyType.NormalEnemy;
    [SerializeField] private UnitElement playerElement = UnitElement.Neutral;
    [SerializeField] private UnitElement enemyElement = UnitElement.Neutral;
    [SerializeField] private bool playerHasFloatAbility;
    [SerializeField] private bool enemyHasFloatAbility;
    [SerializeField] private bool showDebugPanelTools = true;

    private readonly IAttackPredictionChanceProvider predictionChanceProvider = new TestAttackPredictionChanceProvider();
    private readonly List<CardView> cardViews = new List<CardView>();
    private readonly List<Button> moveButtons = new List<Button>();
    private readonly List<QueuedBattleAction> actionQueue = new List<QueuedBattleAction>();
    private readonly List<GameObject> actionQueueItemObjects = new List<GameObject>();
    private readonly Image[,,] gridSlots = new Image[2, BattleGridPosition.GridSize, BattleGridPosition.GridSize];
    private readonly Text[,,] gridLabels = new Text[2, BattleGridPosition.GridSize, BattleGridPosition.GridSize];
    private readonly PanelType[,,] panelTypes = new PanelType[2, BattleGridPosition.GridSize, BattleGridPosition.GridSize];
    private readonly List<BattleGridPosition> previewCells = new List<BattleGridPosition>();
    private readonly BattleStatsTracker battleStatsTracker = new BattleStatsTracker();

    private Font uiFont;
    private CharacterUnit player;
    private CharacterUnit enemy;
    private DeckManager deck;
    private BattleLog battleLog;
    private EnemyAI enemyAI;
    private bool battleEnded;
    private bool predictionActive;
    private int currentRound;
    private int remainingPlayerActions;
    private int accelGauge;
    private int enemyActionsRemaining;
    private int enemyActionIndex;
    private int pendingPlayerAttackBonus;
    private EnemyBattleAction pendingEnemyAttack;
    private CardAttribute currentPlayerAttackAttribute = CardAttribute.Neutral;
    private bool resolvingAttackPathEffects;
    private int battleStartAccelGauge;
    private CharacterUnit currentPlayerAttackTarget;
    private PanelType currentPlayerAttackTargetPanelBeforePath;

    private Text playerHpText;
    private Text enemyHpText;
    private Text enemyNameText;
    private Text positionText;
    private Text statusText;
    private Text actionCountText;
    private Text actionQueueText;
    private RectTransform actionQueueRoot;
    private Text enemyPlanText;
    private Text predictionText;
    private Text rangeText;
    private Text deckText;
    private Text logText;
    private CardHoverDetailView cardHoverDetailView;
    private AttackEffectPlayer attackEffectPlayer;
    private AccelGaugeUI accelGaugeUI;
    private BattleDebugPanelController debugPanelController;
    private BattleResultOverlay battleResultOverlay;
    private Transform battleCanvasRoot;
    private Button confirmButton;
    private Button resetSelectionButton;
    private Button weaponButton;
    private readonly List<Image> actionPips = new List<Image>();

    private enum QueuedActionType
    {
        Card,
        Move,
        Weapon
    }

    private sealed class QueuedBattleAction
    {
        public QueuedBattleAction(CardInstance card)
        {
            Type = QueuedActionType.Card;
            Card = card;
            DisplayName = card.Data.Name;
            ConsumesAction = !card.Data.IsClearCard;
        }

        public QueuedBattleAction(MoveDirection direction, string displayName)
        {
            Type = QueuedActionType.Move;
            MoveDirection = direction;
            DisplayName = displayName;
            ConsumesAction = true;
        }

        public QueuedBattleAction(string displayName)
        {
            Type = QueuedActionType.Weapon;
            DisplayName = displayName;
            ConsumesAction = false;
        }

        public QueuedActionType Type { get; private set; }
        public CardInstance Card { get; private set; }
        public MoveDirection MoveDirection { get; private set; }
        public string DisplayName { get; private set; }
        public bool ConsumesAction { get; private set; }
    }

    private void Awake()
    {
        uiFont = CreateJapaneseFont();
        if (uiFont == null)
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        if (uiFont == null)
        {
            uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        EnsureCamera();
        EnsureEventSystem();
        BuildUi();
        StartBattle();
    }

    private static Font CreateJapaneseFont()
    {
        string[] fontNames = { "Meiryo UI", "Yu Gothic UI", "Meiryo", "Yu Gothic", "Noto Sans CJK JP", "Arial" };
        return Font.CreateDynamicFontFromOSFont(fontNames, 28);
    }

    private void EnsureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.05f, 0.06f, 0.08f);
    }

    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
            InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            ConfigureInputModule(inputModule);
            return;
        }

        InputSystemUIInputModule existingInputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (existingInputModule == null)
        {
            BaseInputModule legacyModule = eventSystem.GetComponent<BaseInputModule>();
            if (legacyModule != null)
            {
                Destroy(legacyModule);
            }

            existingInputModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        ConfigureInputModule(existingInputModule);
    }

    private static void ConfigureInputModule(InputSystemUIInputModule inputModule)
    {
        if (inputModule.point == null)
        {
            inputModule.point = CreateInputActionReference("Point", InputActionType.PassThrough, "<Pointer>/position");
        }

        if (inputModule.leftClick == null)
        {
            inputModule.leftClick = CreateInputActionReference("Click", InputActionType.Button, "<Pointer>/press");
        }

        if (inputModule.rightClick == null)
        {
            inputModule.rightClick = CreateInputActionReference("Right Click", InputActionType.Button, "<Mouse>/rightButton");
        }

        if (inputModule.middleClick == null)
        {
            inputModule.middleClick = CreateInputActionReference("Middle Click", InputActionType.Button, "<Mouse>/middleButton");
        }

        if (inputModule.scrollWheel == null)
        {
            inputModule.scrollWheel = CreateInputActionReference("Scroll Wheel", InputActionType.PassThrough, "<Mouse>/scroll");
        }
    }

    private static InputActionReference CreateInputActionReference(string name, InputActionType type, string binding)
    {
        InputAction action = new InputAction(name, type, binding);
        return InputActionReference.Create(action);
    }

    private void StartBattle(int? overrideAccelGauge = null, bool captureInitialState = true)
    {
        if (battleResultOverlay != null)
        {
            battleResultOverlay.HideImmediate();
        }

        battleStatsTracker.Reset();
        player = new CharacterUnit(BattleText.PlayerName, 180, new BattleGridPosition(GridSide.Player, 1, 1));
        enemy = new CharacterUnit(BattleText.EnemyName, EnemyAI.GetMaxHp(enemyType), new BattleGridPosition(GridSide.Enemy, 1, 1));
        enemyElement = EnemyAI.GetElement(enemyType);
        enemyHasFloatAbility = EnemyAI.HasFloatAbility(enemyType);
        player.Element = playerElement;
        enemy.Element = enemyElement;
        player.HasFloatAbility = playerHasFloatAbility;
        enemy.HasFloatAbility = enemyHasFloatAbility;
        battleEnded = false;
        predictionActive = false;
        currentRound = 1;
        remainingPlayerActions = MaxPlayerActions;
        accelGauge = overrideAccelGauge.HasValue ? Mathf.Clamp(overrideAccelGauge.Value, 0, MaxAccelGauge) : (previousBattleAccelGauge >= 50 ? 50 : 0);
        if (captureInitialState)
        {
            battleStartAccelGauge = accelGauge;
        }

        enemyActionsRemaining = 0;
        enemyActionIndex = 0;
        pendingPlayerAttackBonus = 0;
        pendingEnemyAttack = null;
        currentPlayerAttackAttribute = CardAttribute.Neutral;
        resolvingAttackPathEffects = false;
        currentPlayerAttackTarget = null;
        currentPlayerAttackTargetPanelBeforePath = PanelType.Normal;
        actionQueue.Clear();
        previewCells.Clear();
        if (cardHoverDetailView != null)
        {
            cardHoverDetailView.Hide();
        }

        battleLog = new BattleLog(10);
        enemyAI = new EnemyAI(enemyType);
        deck = new DeckManager(GetStarterDeck());
        InitializePanels();

        battleLog.Add("バトル開始");
        battleLog.Add("アクセルゲージ：" + accelGauge + "％");
        battleLog.Add("エネミータイプ：" + EnemyAI.GetDisplayName(enemyType));
        battleLog.Add("初期デッキをシャッフルしました。");
        DrawToHandLimit("ターン開始");
        battleLog.Add("プレイヤーの初期位置：" + player.Position);
        battleLog.Add("エネミーの初期位置：" + enemy.Position);
        RefreshUi();
    }

    private List<CardData> GetStarterDeck()
    {
        List<CardData> savedDeck;
        if (DeckStorage.TryLoadDeck(out savedDeck))
        {
            DeckValidationResult savedResult = DeckValidator.Validate(savedDeck);
            if (savedResult.IsValid)
            {
                battleLog.Add("保存済みデッキを読み込みました。");
                return savedDeck;
            }

            battleLog.Add("保存済みデッキが無効なためデフォルトデッキを使用します。");
        }

        List<CardData> cards = new List<CardData>();
        for (int i = 0; i < starterDeck.Count; i++)
        {
            if (starterDeck[i] != null && starterDeck[i].Effect != CardEffectType.Move)
            {
                cards.Add(starterDeck[i]);
            }
        }

        if (cards.Count > 0 && DeckValidator.Validate(cards).IsValid)
        {
            return cards;
        }

        return CardData.CreateStarterDeck();
    }

    private void InitializePanels()
    {
        for (int side = 0; side < 2; side++)
        {
            for (int row = 0; row < BattleGridPosition.GridSize; row++)
            {
                for (int column = 0; column < BattleGridPosition.GridSize; column++)
                {
                    panelTypes[side, row, column] = PanelType.Normal;
                }
            }
        }
    }

    private PanelType GetPanelType(BattleGridPosition position)
    {
        return panelTypes[GetSideIndex(position.Side), position.Row, position.Column];
    }

    private void SetPanelType(BattleGridPosition position, PanelType panelType)
    {
        if (!position.IsValid)
        {
            return;
        }

        panelTypes[GetSideIndex(position.Side), position.Row, position.Column] = panelType;
    }

    private void ApplyStagePanelChange(PanelType panelType)
    {
        if (attackEffectPlayer != null)
        {
            attackEffectPlayer.PlayStageEffect(EffectAssetResolver.GetStageEffect(panelType));
        }

        for (int side = 0; side < 2; side++)
        {
            for (int row = 0; row < BattleGridPosition.GridSize; row++)
            {
                for (int column = 0; column < BattleGridPosition.GridSize; column++)
                {
                    SetPanelType(new BattleGridPosition((GridSide)side, row, column), panelType);
                }
            }
        }

        battleLog.Add("すべてのパネルを" + GetPanelTypeDisplayName(panelType) + "に変更しました。");
        Debug.Log("Debug: Stage card changed all panels to " + panelType + ".");

        if (panelType == PanelType.Magma)
        {
            ApplyImmediateMagmaStageEffects();
        }

        RefreshGrid();
    }

    private void ApplyImmediateMagmaStageEffects()
    {
        ApplyMagmaPanelEffectToCurrentPosition(player);
        ApplyMagmaPanelEffectToCurrentPosition(enemy);
        CheckBattleEnd();
    }

    private void ApplyMagmaPanelEffectToCurrentPosition(CharacterUnit unit)
    {
        if (unit == null || unit.IsDefeated || GetPanelType(unit.Position) != PanelType.Magma)
        {
            return;
        }

        ApplyArrivalPanelEffect(unit, unit.Position);
    }

    public void DebugSetPanelType(BattleGridPosition position, PanelType panelType)
    {
        SetPanelType(position, panelType);
        Debug.Log("Debug: Panel (" + position.Side + ", row " + position.Row + ", col " + position.Column + ") changed to " + panelType);
        RefreshGrid();
    }

    public void DebugApplyPanelPreset(PanelDebugPreset preset)
    {
        ResetAllPanelsToNormal();

        switch (preset)
        {
            case PanelDebugPreset.CrackedTest:
                SetPanelType(new BattleGridPosition(GridSide.Player, 1, 0), PanelType.Cracked);
                SetPanelType(new BattleGridPosition(GridSide.Player, 1, 2), PanelType.Cracked);
                SetPanelType(new BattleGridPosition(GridSide.Enemy, 1, 0), PanelType.Cracked);
                break;
            case PanelDebugPreset.HoleTest:
                SetPanelType(new BattleGridPosition(GridSide.Player, 1, 2), PanelType.Hole);
                SetPanelType(new BattleGridPosition(GridSide.Enemy, 1, 0), PanelType.Hole);
                SetPanelType(new BattleGridPosition(GridSide.Enemy, 0, 0), PanelType.Hole);
                break;
            case PanelDebugPreset.IceTest:
                SetPanelType(new BattleGridPosition(GridSide.Enemy, enemy.Position.Row, enemy.Position.Column), PanelType.Ice);
                SetPanelType(new BattleGridPosition(GridSide.Player, 0, 2), PanelType.Ice);
                break;
            case PanelDebugPreset.GrassTest:
                SetPanelType(new BattleGridPosition(GridSide.Enemy, 1, 0), PanelType.Grass);
                SetPanelType(new BattleGridPosition(GridSide.Enemy, 1, 1), PanelType.Grass);
                SetPanelType(new BattleGridPosition(GridSide.Enemy, 1, 2), PanelType.Grass);
                break;
            case PanelDebugPreset.MagmaTest:
                SetPanelType(new BattleGridPosition(GridSide.Player, 2, 1), PanelType.Magma);
                SetPanelType(new BattleGridPosition(GridSide.Enemy, 1, 0), PanelType.Magma);
                SetPanelType(new BattleGridPosition(GridSide.Enemy, 2, 1), PanelType.Magma);
                break;
            case PanelDebugPreset.PoisonTest:
                SetPanelType(player.Position, PanelType.Poison);
                SetPanelType(new BattleGridPosition(GridSide.Enemy, 1, 0), PanelType.Poison);
                SetPanelType(new BattleGridPosition(GridSide.Player, 0, 1), PanelType.Poison);
                break;
            case PanelDebugPreset.AllTypes:
                ApplyAllTypesPanelLayout();
                break;
        }

        Debug.Log("Debug: Applied panel preset " + preset);
        RefreshGrid();
    }

    public void DebugResetBattleToInitialState()
    {
        StartBattle(battleStartAccelGauge, false);
        ResetAllPanelsToNormal();
        RefreshGrid();
        Debug.Log("Debug: Battle state reset to initial state.");
    }

    public void DebugChangeEnemyType(EnemyType newEnemyType)
    {
        enemyType = newEnemyType;
        StartBattle(battleStartAccelGauge, false);
        Debug.Log("Debug: Enemy type changed to " + EnemyAI.GetDisplayName(enemyType) + ".");
    }

    public EnemyType DebugGetEnemyType()
    {
        return enemyType;
    }

    public string DebugGetEnemySummary(EnemyType targetEnemyType)
    {
        return EnemyAI.GetDebugSummary(targetEnemyType);
    }

    private void ResetAllPanelsToNormal()
    {
        for (int side = 0; side < 2; side++)
        {
            for (int row = 0; row < BattleGridPosition.GridSize; row++)
            {
                for (int column = 0; column < BattleGridPosition.GridSize; column++)
                {
                    panelTypes[side, row, column] = PanelType.Normal;
                }
            }
        }

        Debug.Log("Debug: All panels reset to Normal");
    }

    private void ApplyAllTypesPanelLayout()
    {
        SetPanelType(new BattleGridPosition(GridSide.Player, 0, 0), PanelType.Cracked);
        SetPanelType(new BattleGridPosition(GridSide.Player, 0, 1), PanelType.Hole);
        SetPanelType(new BattleGridPosition(GridSide.Player, 0, 2), PanelType.Ice);
        SetPanelType(new BattleGridPosition(GridSide.Player, 2, 0), PanelType.Grass);
        SetPanelType(new BattleGridPosition(GridSide.Player, 2, 1), PanelType.Magma);
        SetPanelType(new BattleGridPosition(GridSide.Player, 2, 2), PanelType.Poison);

        SetPanelType(new BattleGridPosition(GridSide.Enemy, 0, 0), PanelType.Hole);
        SetPanelType(new BattleGridPosition(GridSide.Enemy, 0, 1), PanelType.Ice);
        SetPanelType(new BattleGridPosition(GridSide.Enemy, 0, 2), PanelType.Cracked);
        SetPanelType(new BattleGridPosition(GridSide.Enemy, 2, 0), PanelType.Grass);
        SetPanelType(new BattleGridPosition(GridSide.Enemy, 2, 1), PanelType.Magma);
        SetPanelType(new BattleGridPosition(GridSide.Enemy, 2, 2), PanelType.Poison);
    }

    private void HandleDebugPanelCellClicked(BattleGridPosition position)
    {
        if (debugPanelController != null)
        {
            debugPanelController.HandlePanelClicked(position);
        }
    }

    private void PlayCard(int handIndex)
    {
        if (battleEnded || handIndex < 0 || handIndex >= deck.Hand.Count)
        {
            return;
        }

        if (predictionActive)
        {
            ResolvePredictionCard(handIndex);
            return;
        }

        CardInstance card = deck.Hand[handIndex];
        if (IsCardQueued(card))
        {
            battleLog.Add("そのカードはすでに選択中です。");
            RefreshUi();
            return;
        }

        if (!card.Data.IsClearCard && GetQueuedActionCost() >= MaxPlayerActions)
        {
            battleLog.Add("これ以上アクションを選択できません。");
            battleLog.Add("行動権を消費するアクションは最大3回まで選択できます。");
            RefreshUi();
            return;
        }

        actionQueue.Add(new QueuedBattleAction(card));
        remainingPlayerActions = GetRemainingPlayerActions();
        battleLog.Add(card.Data.Name + (card.Data.IsClearCard ? "（CLEAR）" : string.Empty) + "を行動キューに追加しました。");
        ShowCardPreview(card.Data, string.Empty);
        RefreshUi();
    }

    private int GetQueuedActionCost()
    {
        int cost = 0;
        for (int i = 0; i < actionQueue.Count; i++)
        {
            if (actionQueue[i].ConsumesAction)
            {
                cost++;
            }
        }

        return cost;
    }

    private int GetRemainingPlayerActions()
    {
        return Mathf.Max(0, MaxPlayerActions - GetQueuedActionCost());
    }

    private bool IsCardQueued(CardInstance card)
    {
        for (int i = 0; i < actionQueue.Count; i++)
        {
            if (actionQueue[i].Type == QueuedActionType.Card && actionQueue[i].Card == card)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsWeaponQueued()
    {
        for (int i = 0; i < actionQueue.Count; i++)
        {
            if (actionQueue[i].Type == QueuedActionType.Weapon)
            {
                return true;
            }
        }

        return false;
    }

    private void HandleWeaponCommand()
    {
        if (battleEnded || predictionActive)
        {
            return;
        }

        if (IsWeaponQueued())
        {
            battleLog.Add("ウエポンは1ターンに1回までです。");
            Debug.Log("Debug: Weapon command can only be queued once per player turn.");
            RefreshUi();
            return;
        }

        actionQueue.Add(new QueuedBattleAction(WeaponDisplayName));
        remainingPlayerActions = GetRemainingPlayerActions();
        battleLog.Add("ウエポンを行動キューに追加しました。");
        Debug.Log("Debug: Weapon command queued.");
        ClearPreview();
        RefreshUi();
    }

    private bool TryResolveCard(CardData card, out string failureMessage, bool predictionAction = false)
    {
        switch (card.Effect)
        {
            case CardEffectType.Damage:
                CharacterUnit target;
                currentPlayerAttackAttribute = card.Attribute;
                resolvingAttackPathEffects = true;
                TryGetDamageTarget(card, out target);
                resolvingAttackPathEffects = false;
                battleLog.Add(predictionAction ? "プレイヤーは予測行動で" + card.Name + "を使用。" : "プレイヤーは" + card.Name + "を使用。");
                PlayCardAttackPresentation(card, target);
                int attackBonus = pendingPlayerAttackBonus;
                int damage = card.Power + attackBonus;
                if (attackBonus > 0)
                {
                    battleLog.Add("チャージ効果で攻撃ダメージ +" + attackBonus + "。");
                    pendingPlayerAttackBonus = 0;
                }

                if (target == null)
                {
                    battleLog.Add("しかし攻撃範囲内にエネミーはいなかった。");
                    battleLog.Add("攻撃は空振りした。");
                    failureMessage = string.Empty;
                    return true;
                }

                DealDamageToEnemy(target, damage);
                failureMessage = string.Empty;
                return true;
            case CardEffectType.Guard:
                player.AddGuard(card.Power);
                battleLog.Add(predictionAction ? "プレイヤーは予測行動で" + card.Name + "を使用。" : "プレイヤーは" + card.Name + "を使用。");
                battleLog.Add("ガード +" + card.Power + "。");
                failureMessage = string.Empty;
                return true;
            case CardEffectType.Move:
                BattleGridPosition destination;
                if (!TryGetMoveDestination(card.MoveDirection, out destination, out failureMessage))
                {
                    return false;
                }

                battleLog.Add(predictionAction ? "プレイヤーは予測行動で" + card.Name + "を使用。" : "プレイヤーは" + card.Name + "を使用。");
                TryMoveUnitTo(player, destination, destination + "へ移動しました。");
                failureMessage = string.Empty;
                return true;
            case CardEffectType.Repair:
                int before = player.Hp;
                player.Heal(card.Power);
                int recovered = player.Hp - before;
                battleLog.Add(predictionAction ? "プレイヤーは予測行動で" + card.Name + "を使用。" : "プレイヤーは" + card.Name + "を使用。");
                if (recovered == 0)
                {
                    battleLog.Add("HPはすでに最大だった。");
                }
                else
                {
                    battleLog.Add("HPを" + recovered + "回復。");
                }
                failureMessage = string.Empty;
                return true;
            case CardEffectType.Charge:
                pendingPlayerAttackBonus += card.Power;
                battleLog.Add(predictionAction ? "プレイヤーは予測行動で" + card.Name + "を使用。" : "プレイヤーは" + card.Name + "を使用。");
                battleLog.Add("次の攻撃カードのダメージ +" + card.Power + "。");
                failureMessage = string.Empty;
                return true;
            case CardEffectType.Freeze:
                currentPlayerAttackAttribute = card.Attribute;
                resolvingAttackPathEffects = true;
                CharacterUnit freezeTarget;
                TryGetDamageTarget(card, out freezeTarget);
                resolvingAttackPathEffects = false;
                battleLog.Add(predictionAction ? "プレイヤーは予測行動で" + card.Name + "を使用。" : "プレイヤーは" + card.Name + "を使用。");
                PlayCardAttackPresentation(card, freezeTarget);
                if (freezeTarget == null)
                {
                    battleLog.Add("しかし攻撃範囲内に敵はいなかった。");
                    battleLog.Add(card.Name + "は空振りした。");
                    failureMessage = string.Empty;
                    return true;
                }

                if (freezeTarget.ApplyFrozen())
                {
                    battleLog.Add(freezeTarget.Name + "を凍結状態にした。");
                    Debug.Log("Debug: " + card.Name + " applied Frozen to " + freezeTarget.Name + ".");
                }
                else
                {
                    battleLog.Add(freezeTarget.Name + "は炎属性のため凍結しなかった。");
                    Debug.Log("Debug: " + card.Name + " did not freeze " + freezeTarget.Name + " because the target is Fire element.");
                }

                currentPlayerAttackTarget = null;
                currentPlayerAttackTargetPanelBeforePath = PanelType.Normal;
                failureMessage = string.Empty;
                return true;
            case CardEffectType.StageChange:
                battleLog.Add(predictionAction ? "プレイヤーは予測行動で" + card.Name + "を使用。" : "プレイヤーは" + card.Name + "を使用。");
                if (attackEffectPlayer != null)
                {
                    attackEffectPlayer.ShowCardName(card.Name);
                }

                ApplyStagePanelChange(card.TargetPanelType);
                failureMessage = string.Empty;
                return true;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DealDamageToEnemy(CharacterUnit target, int damage)
    {
        damage = CalculatePanelAdjustedDamage(target, damage, currentPlayerAttackAttribute);
        bool wasDefeated = target.IsDefeated;
        int blocked;
        int actualDamage = target.TakeDamage(damage, out blocked);
        if (blocked > 0)
        {
            battleLog.Add("エネミーは" + blocked + "ダメージをガードし、" + actualDamage + "ダメージを受けた。");
        }
        else
        {
            battleLog.Add("エネミーに" + actualDamage + "ダメージ。");
        }

        if (attackEffectPlayer != null)
        {
            attackEffectPlayer.PlayDamagePopup(target.Position, actualDamage);
        }

        RecordDamageResult(target, wasDefeated, actualDamage);
        currentPlayerAttackTarget = null;
        currentPlayerAttackTargetPanelBeforePath = PanelType.Normal;
    }

    private void PlayCardAttackPresentation(CardData card, CharacterUnit target)
    {
        if (attackEffectPlayer == null || card == null)
        {
            return;
        }

        attackEffectPlayer.ShowCardName(card.Name);
        if (target != null)
        {
            attackEffectPlayer.PlayEffectAtPanel(EffectAssetResolver.GetHitEffect(card.Attribute), target.Position);
        }
    }

    private int CalculatePanelAdjustedDamage(CharacterUnit target, int baseDamage, CardAttribute attribute)
    {
        int damage = Mathf.Max(0, baseDamage);
        PanelType targetPanel = target == currentPlayerAttackTarget
            ? currentPlayerAttackTargetPanelBeforePath
            : GetPanelType(target.Position);

        if (targetPanel == PanelType.Ice && attribute == CardAttribute.Electric)
        {
            damage *= 2;
            battleLog.Add("氷パネル効果で電気属性ダメージが2倍になりました。");
        }

        if (targetPanel == PanelType.Grass && attribute == CardAttribute.Fire)
        {
            damage *= 2;
            battleLog.Add("草パネル効果で炎属性ダメージが2倍になりました。");
        }

        if (targetPanel == PanelType.Ice && attribute == CardAttribute.Water)
        {
            if (target.ApplyFrozen())
            {
                battleLog.Add(target.Name + "は凍結しました。");
            }
            else
            {
                battleLog.Add(target.Name + "は炎属性のため凍結しません。");
            }
        }

        if (target.IsFrozen && attribute == CardAttribute.Break)
        {
            damage *= 2;
            target.ClearFrozen();
            battleLog.Add("ブレイク属性で凍結中の対象に2倍ダメージ。凍結を解除しました。");
        }

        return damage;
    }

    private bool TryGetDamageTarget(CardData card, out CharacterUnit target)
    {
        target = null;

        switch (card.TargetPattern)
        {
            case CardTargetPattern.SameRowNearestEnemy:
                if (enemy.Position.Row == player.Position.Row)
                {
                    target = enemy;
                }
                break;
            case CardTargetPattern.ForwardOnePanel:
                target = IsForwardOnePanelTarget(enemy) ? enemy : null;
                break;
            case CardTargetPattern.Row:
                target = enemy.Position.Row == player.Position.Row ? enemy : null;
                break;
            case CardTargetPattern.ForwardSingle:
                target = IsEnemyInSameForwardRow(enemy) ? enemy : null;
                break;
            case CardTargetPattern.ForwardLine3:
                target = IsEnemyWithinForwardDistance(enemy, 3) ? enemy : null;
                break;
            case CardTargetPattern.ForwardExactly3:
                target = IsEnemyAtForwardDistance(enemy, 3) ? enemy : null;
                break;
            case CardTargetPattern.SingleTarget:
                target = enemy;
                break;
            case CardTargetPattern.AroundSelf:
                target = IsAroundSelf(enemy.Position) ? enemy : null;
                break;
        }

        if (resolvingAttackPathEffects)
        {
            currentPlayerAttackTarget = target;
            currentPlayerAttackTargetPanelBeforePath = target != null ? GetPanelType(target.Position) : PanelType.Normal;
            List<BattleGridPosition> attackPath = BuildPlayerAttackPath(card, target);
            if (ApplyAttackPathPanelEffects(card.Attribute, AttackTravelType.Ground, attackPath))
            {
                target = null;
                currentPlayerAttackTarget = null;
            }
        }

        return target != null;
    }

    private bool TryGetWeaponTarget(out CharacterUnit target)
    {
        target = enemy.Position.Row == player.Position.Row ? enemy : null;
        currentPlayerAttackTarget = target;
        currentPlayerAttackTargetPanelBeforePath = target != null ? GetPanelType(target.Position) : PanelType.Normal;

        List<BattleGridPosition> attackPath = BuildPlayerSameRowAttackPath(target);
        if (ApplyAttackPathPanelEffects(WeaponAttribute, AttackTravelType.Ground, attackPath))
        {
            target = null;
            currentPlayerAttackTarget = null;
        }

        return target != null;
    }

    private List<BattleGridPosition> BuildPlayerAttackPath(CardData card, CharacterUnit target)
    {
        List<BattleGridPosition> path = new List<BattleGridPosition>();

        switch (card.TargetPattern)
        {
            case CardTargetPattern.SameRowNearestEnemy:
            case CardTargetPattern.Row:
            case CardTargetPattern.ForwardSingle:
                path.AddRange(BuildPlayerSameRowAttackPath(target));
                break;
            case CardTargetPattern.ForwardLine3:
            case CardTargetPattern.ForwardExactly3:
                path.AddRange(BuildPlayerForwardPath(3));
                break;
            case CardTargetPattern.ForwardOnePanel:
                if (player.Position.Column < BattleGridPosition.GridSize - 1)
                {
                    path.Add(new BattleGridPosition(GridSide.Player, player.Position.Row, player.Position.Column + 1));
                }
                else
                {
                    path.Add(new BattleGridPosition(GridSide.Enemy, player.Position.Row, 0));
                }
                break;
            case CardTargetPattern.SingleTarget:
                if (target != null)
                {
                    path.Add(target.Position);
                }
                else
                {
                    for (int column = 0; column < BattleGridPosition.GridSize; column++)
                    {
                        path.Add(new BattleGridPosition(GridSide.Enemy, player.Position.Row, column));
                    }
                }
                break;
            case CardTargetPattern.AroundSelf:
                if (target != null)
                {
                    path.Add(target.Position);
                }
                break;
        }

        return path;
    }

    private List<BattleGridPosition> BuildPlayerSameRowAttackPath(CharacterUnit target)
    {
        List<BattleGridPosition> path = new List<BattleGridPosition>();
        for (int column = player.Position.Column + 1; column < BattleGridPosition.GridSize; column++)
        {
            path.Add(new BattleGridPosition(GridSide.Player, player.Position.Row, column));
        }

        int maxColumn = target != null && target.Position.Side == GridSide.Enemy ? target.Position.Column : BattleGridPosition.GridSize - 1;
        for (int column = 0; column <= maxColumn; column++)
        {
            path.Add(new BattleGridPosition(GridSide.Enemy, player.Position.Row, column));
        }

        return path;
    }

    private List<BattleGridPosition> BuildPlayerForwardPath(int distance)
    {
        List<BattleGridPosition> path = new List<BattleGridPosition>();
        for (int i = 1; i <= distance; i++)
        {
            BattleGridPosition position;
            if (TryGetPlayerForwardPosition(i, out position))
            {
                path.Add(position);
            }
        }

        return path;
    }

    private List<BattleGridPosition> BuildEnemyAttackPath(EnemyBattleAction attackAction)
    {
        List<BattleGridPosition> path = new List<BattleGridPosition>();
        if (attackAction == null)
        {
            return path;
        }

        switch (attackAction.AttackPattern)
        {
            case EnemyAttackPattern.ForwardOnePanel:
                path.Add(new BattleGridPosition(GridSide.Player, enemy.Position.Row, BattleGridPosition.GridSize - 1));
                break;
            case EnemyAttackPattern.Row:
            case EnemyAttackPattern.Strong:
            case EnemyAttackPattern.SameRowNearest:
                for (int column = enemy.Position.Column - 1; column >= 0; column--)
                {
                    path.Add(new BattleGridPosition(GridSide.Enemy, enemy.Position.Row, column));
                }

                int minColumn = player.Position.Row == enemy.Position.Row ? player.Position.Column : 0;
                for (int column = BattleGridPosition.GridSize - 1; column >= minColumn; column--)
                {
                    path.Add(new BattleGridPosition(GridSide.Player, enemy.Position.Row, column));
                }
                break;
        }

        return path;
    }

    private bool ApplyAttackPathPanelEffects(CardAttribute attribute, AttackTravelType travelType, List<BattleGridPosition> path)
    {
        bool blockedByHole = false;
        for (int i = 0; i < path.Count; i++)
        {
            BattleGridPosition position = path[i];
            if (!position.IsValid)
            {
                continue;
            }

            PanelType panelType = GetPanelType(position);
            if (attribute == CardAttribute.Fire && panelType == PanelType.Grass)
            {
                SetPanelType(position, PanelType.Normal);
                battleLog.Add("炎属性攻撃が草パネルをノーマルパネルに変化させました。");
                Debug.Log("Fire attack changed Grass panel to Normal: " + position);
                panelType = PanelType.Normal;
            }
            else if (attribute == CardAttribute.Water && panelType == PanelType.Magma)
            {
                SetPanelType(position, PanelType.Normal);
                battleLog.Add("水属性攻撃がマグマパネルをノーマルパネルに変化させました。");
                Debug.Log("Water attack changed Magma panel to Normal: " + position);
                panelType = PanelType.Normal;
            }

            if (travelType == AttackTravelType.Ground && panelType == PanelType.Hole)
            {
                battleLog.Add("地上判定攻撃は穴パネルで止まりました。");
                blockedByHole = true;
                break;
            }
        }

        return blockedByHole;
    }

    private bool TryGetMoveDestination(MoveDirection direction, out BattleGridPosition destination, out string failureMessage)
    {
        return TryGetMoveDestination(player, direction, out destination, out failureMessage);
    }

    private bool TryGetMoveDestination(CharacterUnit unit, MoveDirection direction, out BattleGridPosition destination, out string failureMessage)
    {
        int rowDelta = 0;
        int columnDelta = 0;

        switch (direction)
        {
            case MoveDirection.Forward:
                columnDelta = BattleGridPosition.ForwardColumnDelta(unit.Position.Side);
                break;
            case MoveDirection.Back:
                columnDelta = BattleGridPosition.BackColumnDelta(unit.Position.Side);
                break;
            case MoveDirection.Up:
                rowDelta = -1;
                break;
            case MoveDirection.Down:
                rowDelta = 1;
                break;
        }

        destination = unit.Position.Offset(rowDelta, columnDelta);
        if (!destination.IsValid)
        {
            failureMessage = "移動先がパネル外です。";
            return false;
        }

        if (IsOccupied(destination))
        {
            failureMessage = "移動先にユニットがいます。";
            return false;
        }

        if (!CanUnitEnterPanel(unit, destination, out failureMessage))
        {
            return false;
        }

        failureMessage = string.Empty;
        return true;
    }

    private bool CanUnitEnterPanel(CharacterUnit unit, BattleGridPosition destination, out string failureMessage)
    {
        if (GetPanelType(destination) == PanelType.Hole && !unit.HasFloatAbility)
        {
            failureMessage = "穴パネルには移動できません。";
            return false;
        }

        failureMessage = string.Empty;
        return true;
    }

    private bool TryMoveUnitTo(CharacterUnit unit, BattleGridPosition destination, string successLog)
    {
        if (!destination.IsValid)
        {
            battleLog.Add("移動先がパネル外です。");
            return false;
        }

        if (IsOccupied(destination))
        {
            battleLog.Add("移動先にユニットがいます。");
            return false;
        }

        string failureMessage;
        if (!CanUnitEnterPanel(unit, destination, out failureMessage))
        {
            battleLog.Add(failureMessage);
            return false;
        }

        BattleGridPosition origin = unit.Position;
        unit.MoveTo(destination);
        battleLog.Add(successLog);
        ApplyDeparturePanelEffect(origin);
        ApplyArrivalPanelEffect(unit, destination);
        return true;
    }

    private void ApplyDeparturePanelEffect(BattleGridPosition origin)
    {
        if (GetPanelType(origin) != PanelType.Cracked)
        {
            return;
        }

        SetPanelType(origin, PanelType.Hole);
        battleLog.Add("ヒビパネルが穴パネルに変化しました。");
        Debug.Log("Cracked panel changed to Hole: " + origin);
    }

    private void ApplyArrivalPanelEffect(CharacterUnit unit, BattleGridPosition destination)
    {
        if (GetPanelType(destination) != PanelType.Magma)
        {
            return;
        }

        if (unit.Element == UnitElement.Fire)
        {
            int before = unit.Hp;
            unit.Heal(50);
            battleLog.Add(unit.Name + "はマグマパネルで" + (unit.Hp - before) + "回復しました。");
            return;
        }

        bool wasDefeated = unit.IsDefeated;
        int actualDamage = unit.TakeDirectDamage(50);
        battleLog.Add(unit.Name + "はマグマパネルで" + actualDamage + "ダメージを受けました。");
        if (attackEffectPlayer != null)
        {
            attackEffectPlayer.PlayDamagePopup(unit.Position, actualDamage);
        }

        RecordDamageResult(unit, wasDefeated, actualDamage);
    }

    private void HandleMoveCommand(MoveDirection direction)
    {
        if (battleEnded)
        {
            return;
        }

        if (predictionActive)
        {
            ResolvePredictionMove(direction);
            return;
        }

        if (GetQueuedActionCost() >= MaxPlayerActions)
        {
            battleLog.Add("これ以上アクションを選択できません。");
            battleLog.Add("行動権を消費するアクションは最大3回まで選択できます。");
            RefreshUi();
            return;
        }

        actionQueue.Add(new QueuedBattleAction(direction, GetMoveQueueName(direction)));
        remainingPlayerActions = GetRemainingPlayerActions();
        battleLog.Add(GetMoveQueueName(direction) + "を行動キューに追加しました。");
        ClearPreview();
        RefreshUi();
    }

    private static string GetMoveQueueName(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Forward:
                return "前進";
            case MoveDirection.Back:
                return "後退";
            case MoveDirection.Up:
                return "上移動";
            case MoveDirection.Down:
                return "下移動";
            default:
                return "移動";
        }
    }

    private static string GetMoveCommandLog(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Forward:
                return "プレイヤーは前進した。";
            case MoveDirection.Back:
                return "プレイヤーは後退した。";
            case MoveDirection.Up:
                return "プレイヤーは上へ移動した。";
            case MoveDirection.Down:
                return "プレイヤーは下へ移動した。";
            default:
                return "プレイヤーは移動した。";
        }
    }

    private bool IsOccupied(BattleGridPosition position)
    {
        return MatchesPosition(player, position) || MatchesPosition(enemy, position);
    }

    private static bool MatchesPosition(CharacterUnit unit, BattleGridPosition position)
    {
        return unit.Position.Side == position.Side && unit.Position.Row == position.Row && unit.Position.Column == position.Column;
    }

    private bool IsForwardOnePanelTarget(CharacterUnit target)
    {
        if (target.Position.Side == player.Position.Side || target.Position.Row != player.Position.Row)
        {
            return false;
        }

        return player.Position.Column == BattleGridPosition.GridSize - 1 && target.Position.Column == 0;
    }

    private bool IsEnemyInSameForwardRow(CharacterUnit target)
    {
        return target.Position.Side == GridSide.Enemy && target.Position.Row == player.Position.Row && GetGlobalColumn(target.Position) > GetGlobalColumn(player.Position);
    }

    private bool IsEnemyWithinForwardDistance(CharacterUnit target, int distance)
    {
        if (!IsEnemyInSameForwardRow(target))
        {
            return false;
        }

        int delta = GetGlobalColumn(target.Position) - GetGlobalColumn(player.Position);
        return delta >= 1 && delta <= distance;
    }

    private bool IsEnemyAtForwardDistance(CharacterUnit target, int distance)
    {
        return IsEnemyInSameForwardRow(target) && GetGlobalColumn(target.Position) - GetGlobalColumn(player.Position) == distance;
    }

    private static int GetGlobalColumn(BattleGridPosition position)
    {
        return position.Side == GridSide.Player ? position.Column : BattleGridPosition.GridSize + position.Column;
    }

    private bool TryGetPlayerForwardPosition(int distance, out BattleGridPosition position)
    {
        int globalColumn = GetGlobalColumn(player.Position) + distance;
        int maxGlobalColumn = BattleGridPosition.GridSize * 2 - 1;
        if (globalColumn < 0 || globalColumn > maxGlobalColumn)
        {
            position = new BattleGridPosition(GridSide.Player, -1, -1);
            return false;
        }

        if (globalColumn < BattleGridPosition.GridSize)
        {
            position = new BattleGridPosition(GridSide.Player, player.Position.Row, globalColumn);
        }
        else
        {
            position = new BattleGridPosition(GridSide.Enemy, player.Position.Row, globalColumn - BattleGridPosition.GridSize);
        }

        return position.IsValid;
    }

    private bool IsAroundSelf(BattleGridPosition targetPosition)
    {
        if (targetPosition.Side != player.Position.Side)
        {
            return false;
        }

        int rowDistance = Mathf.Abs(targetPosition.Row - player.Position.Row);
        int columnDistance = Mathf.Abs(targetPosition.Column - player.Position.Column);
        return rowDistance <= 1 && columnDistance <= 1;
    }

    private void EndPlayerTurn()
    {
        if (battleEnded)
        {
            return;
        }

        actionQueue.Clear();
        remainingPlayerActions = 0;
        battleLog.Add("プレイヤーはターンを終了。");
        ResolveEnemyTurn();
    }

    private void ConfirmQueuedActions()
    {
        if (battleEnded)
        {
            return;
        }

        if (actionQueue.Count == 0)
        {
            battleLog.Add("選択中の行動がありません。");
            RefreshUi();
            return;
        }

        battleLog.Add("選択した行動を解決します。");
        List<QueuedBattleAction> actionsToResolve = new List<QueuedBattleAction>(actionQueue);
        actionQueue.Clear();
        remainingPlayerActions = 0;

        for (int i = 0; i < actionsToResolve.Count; i++)
        {
            ResolveQueuedAction(actionsToResolve[i]);
            CheckBattleEnd();
            if (battleEnded)
            {
                ClearPreview();
                RefreshUi();
                return;
            }
        }

        ResolveEnemyTurn();
    }

    private void ResolveQueuedAction(QueuedBattleAction action)
    {
        if (action.Type == QueuedActionType.Move)
        {
            ResolveMoveCommand(action.MoveDirection);
            return;
        }

        if (action.Type == QueuedActionType.Weapon)
        {
            ResolveWeaponCommand();
            return;
        }

        if (action.Card == null || action.Card.Data == null)
        {
            battleLog.Add("カードを解決できませんでした。");
            return;
        }

        string failureMessage;
        if (!TryResolveCard(action.Card.Data, out failureMessage))
        {
            battleLog.Add(failureMessage);
            return;
        }

        if (deck.DiscardFromHand(action.Card))
        {
            battleLog.Add(action.Card.Data.Name + "を捨て札に置きました。");
        }
    }

    private void ResolveMoveCommand(MoveDirection direction)
    {
        BattleGridPosition destination;
        string failureMessage;
        if (!TryGetMoveDestination(direction, out destination, out failureMessage))
        {
            battleLog.Add(failureMessage);
            return;
        }

        TryMoveUnitTo(player, destination, GetMoveCommandLog(direction));
    }

    private void ResolveWeaponCommand()
    {
        battleLog.Add("プレイヤーはウエポンを使用。");
        currentPlayerAttackAttribute = WeaponAttribute;

        CharacterUnit target;
        if (!TryGetWeaponTarget(out target))
        {
            battleLog.Add("しかし攻撃範囲内に敵はいなかった。");
            battleLog.Add("ウエポンは空振りした。");
            currentPlayerAttackTarget = null;
            currentPlayerAttackTargetPanelBeforePath = PanelType.Normal;
            Debug.Log("Debug: Weapon command missed.");
            return;
        }

        if (attackEffectPlayer != null)
        {
            attackEffectPlayer.PlayEffectAtPanel(BattleEffectType.WeaponHit, target.Position);
        }

        DealDamageToEnemy(target, WeaponPower);
        Debug.Log("Debug: Weapon command dealt " + WeaponPower + " neutral damage.");
    }

    private void ResetQueuedActions()
    {
        if (battleEnded)
        {
            return;
        }

        actionQueue.Clear();
        remainingPlayerActions = MaxPlayerActions;
        battleLog.Add("選択中の行動をリセットしました。");
        ClearPreview();
        RefreshUi();
    }

    private void ResolveEnemyTurn()
    {
        if (battleEnded)
        {
            RefreshUi();
            return;
        }

        predictionActive = false;
        pendingEnemyAttack = null;
        ApplyTurnStartPanelEffects(enemy);
        CheckBattleEnd();
        if (battleEnded)
        {
            RefreshUi();
            return;
        }

        if (TrySkipFrozenTurn(enemy))
        {
            StartNextPlayerTurn();
            return;
        }

        enemyAI.BeginTurn();
        enemyActionsRemaining = EnemyAI.GetActionCount(enemyType);
        enemyActionIndex = 0;
        ContinueEnemyTurn();
    }

    private void ContinueEnemyTurn()
    {
        if (battleEnded)
        {
            RefreshUi();
            return;
        }

        while (enemyActionsRemaining > 0 && !battleEnded && !predictionActive)
        {
            EnemyBattleAction action = enemyAI.CreateNextAction(player, enemy, GetUnits(), enemyActionIndex);
            enemyActionIndex++;

            if (action.Kind == EnemyActionKind.Attack)
            {
                if (ShouldTriggerPrediction())
                {
                    StartAttackPrediction(action);
                    return;
                }

                ResolveEnemyAttack(action);
                enemyActionsRemaining--;
                CheckBattleEnd();
                continue;
            }

            ResolveEnemyNonAttackAction(action);
            enemyActionsRemaining--;
            CheckBattleEnd();
        }

        if (battleEnded || predictionActive)
        {
            RefreshUi();
            return;
        }

        StartNextPlayerTurn();
    }

    private bool ShouldTriggerPrediction()
    {
        float chance = Mathf.Clamp01(predictionChanceProvider.GetPredictionChance(enemyType));
        return UnityEngine.Random.value <= chance;
    }

    private void StartAttackPrediction(EnemyBattleAction attackAction)
    {
        predictionActive = true;
        pendingEnemyAttack = attackAction;
        battleLog.Add("攻撃予測が発生した。");
        battleLog.Add("予測された攻撃範囲：" + EnemyAI.FormatAttackPattern(attackAction.AttackPattern));
        RefreshPredictionPreview();
        RefreshUi();
    }

    private void ResolveEnemyNonAttackAction(EnemyBattleAction action)
    {
        if (action.Kind == EnemyActionKind.Move)
        {
            TryMoveUnitTo(enemy, action.Destination, action.ActionText);
            return;
        }

        if (action.Kind == EnemyActionKind.Guard)
        {
            enemy.AddGuard(action.GuardAmount);
            battleLog.Add(action.ActionText);
            battleLog.Add("エネミーのガード +" + action.GuardAmount + "。");
        }
    }

    private void ResolveEnemyAttack(EnemyBattleAction attackAction)
    {
        battleLog.Add(attackAction.ActionText);
        if (!IsPlayerInEnemyAttackRange(attackAction))
        {
            battleLog.Add("しかし攻撃範囲内にプレイヤーはいなかった。");
            battleLog.Add("敵の攻撃は外れた。");
            return;
        }

        if (attackEffectPlayer != null)
        {
            attackEffectPlayer.PlayEffectAtPanel(BattleEffectType.EnemyHit, player.Position);
        }

        DealDamageToPlayer(attackAction.Damage);
    }

    private void DealDamageToPlayer(int damage)
    {
        damage = CalculatePanelAdjustedDamage(player, damage, CardAttribute.Neutral);
        bool wasDefeated = player.IsDefeated;
        int blocked;
        int actualDamage = player.TakeDamage(damage, out blocked);
        if (blocked > 0)
        {
            battleLog.Add("プレイヤーは" + blocked + "ダメージをガードし、" + actualDamage + "ダメージを受けた。");
        }
        else
        {
            battleLog.Add("プレイヤーに" + actualDamage + "ダメージ。");
        }

        if (attackEffectPlayer != null)
        {
            attackEffectPlayer.PlayDamagePopup(player.Position, actualDamage);
        }

        RecordDamageResult(player, wasDefeated, actualDamage);
    }

    private void RecordDamageResult(CharacterUnit unit, bool wasDefeated, int actualDamage)
    {
        if (actualDamage <= 0)
        {
            return;
        }

        if (unit == player)
        {
            battleStatsTracker.RecordPlayerDamageTaken(actualDamage);
        }

        if (unit == enemy && !wasDefeated && unit.IsDefeated)
        {
            battleStatsTracker.RecordEnemyDefeatBatch(1);
        }
    }

    private bool IsPlayerInEnemyAttackRange(EnemyBattleAction attackAction)
    {
        bool inRange;
        switch (attackAction.AttackPattern)
        {
            case EnemyAttackPattern.ForwardOnePanel:
                inRange = enemy.Position.Column == 0 && player.Position.Column == BattleGridPosition.GridSize - 1 && enemy.Position.Row == player.Position.Row;
                break;
            case EnemyAttackPattern.Row:
            case EnemyAttackPattern.Strong:
            case EnemyAttackPattern.SameRowNearest:
                inRange = enemy.Position.Row == player.Position.Row;
                break;
            default:
                return false;
        }

        return inRange && !IsEnemyAttackBlockedByHole(attackAction);
    }

    private bool IsEnemyAttackBlockedByHole(EnemyBattleAction attackAction)
    {
        List<BattleGridPosition> path = BuildEnemyAttackPath(attackAction);
        for (int i = 0; i < path.Count; i++)
        {
            if (path[i].IsValid && GetPanelType(path[i]) == PanelType.Hole)
            {
                return true;
            }
        }

        return false;
    }

    private void ApplyTurnStartPanelEffects(CharacterUnit unit)
    {
        PanelType panelType = GetPanelType(unit.Position);
        if (panelType == PanelType.Poison)
        {
            int damage = Mathf.Max(1, Mathf.CeilToInt(unit.MaxHp * 0.2f));
            bool wasDefeated = unit.IsDefeated;
            int actualDamage = unit.TakeDirectDamage(damage);
            battleLog.Add(unit.Name + "は毒パネルで" + actualDamage + "ダメージを受けました。");
            RecordDamageResult(unit, wasDefeated, actualDamage);
        }

        if (panelType == PanelType.Grass && unit.Element == UnitElement.Grass)
        {
            int before = unit.Hp;
            unit.Heal(Mathf.Max(1, Mathf.CeilToInt(unit.MaxHp * 0.2f)));
            battleLog.Add(unit.Name + "は草パネルで" + (unit.Hp - before) + "回復しました。");
        }
    }

    private bool TrySkipFrozenTurn(CharacterUnit unit)
    {
        bool released;
        if (!unit.ConsumeFrozenTurn(out released))
        {
            return false;
        }

        battleLog.Add(unit.Name + "は凍結状態で行動できません。");
        if (released)
        {
            battleLog.Add(unit.Name + "の凍結が解除されました。");
        }

        return true;
    }

    private void StartNextPlayerTurn()
    {
        if (battleEnded)
        {
            RefreshUi();
            return;
        }

        currentRound++;
        remainingPlayerActions = MaxPlayerActions;
        actionQueue.Clear();
        ApplyTurnStartPanelEffects(player);
        CheckBattleEnd();
        if (battleEnded)
        {
            RefreshUi();
            return;
        }

        if (TrySkipFrozenTurn(player))
        {
            RefreshUi();
            ResolveEnemyTurn();
            return;
        }

        DrawToHandLimit("ターン開始");
        ClearPreview();
        RefreshUi();
    }

    private void FinishPrediction(bool skipAttack, bool logCancel)
    {
        EnemyBattleAction attackAction = pendingEnemyAttack;
        predictionActive = false;
        pendingEnemyAttack = null;
        ClearPreview();

        CheckBattleEnd();
        if (!battleEnded)
        {
            if (skipAttack)
            {
                if (logCancel)
                {
                    battleLog.Add("エネミーの行動をキャンセルした。");
                }
            }
            else
            {
                ResolveEnemyAttack(attackAction);
                CheckBattleEnd();
            }
        }

        enemyActionsRemaining--;

        if (battleEnded)
        {
            RefreshUi();
            return;
        }

        ContinueEnemyTurn();
    }

    private void ResolvePredictionMove(MoveDirection direction)
    {
        BattleGridPosition destination;
        string failureMessage;
        if (!TryGetMoveDestination(direction, out destination, out failureMessage))
        {
            battleLog.Add(failureMessage);
            FinishPrediction(false, false);
            return;
        }

        TryMoveUnitTo(player, destination, GetPredictionMoveLog(direction));
        CheckBattleEnd();
        if (battleEnded)
        {
            RefreshUi();
            return;
        }

        if (!IsPlayerInEnemyAttackRange(pendingEnemyAttack))
        {
            battleLog.Add("プレイヤーは攻撃範囲から離脱した。");
            AddAccelGauge(20, "回避成功");
            battleLog.Add("敵の攻撃は外れた。");
            FinishPrediction(true, false);
            return;
        }

        FinishPrediction(false, false);
    }

    private void ResolvePredictionCard(int handIndex)
    {
        CardInstance card = deck.Hand[handIndex];
        int enemyHpBefore = enemy.Hp;
        string failureMessage;
        if (!TryResolveCard(card.Data, out failureMessage, true))
        {
            battleLog.Add(failureMessage);
            FinishPrediction(false, false);
            return;
        }

        if (deck.DiscardFromHand(card))
        {
            battleLog.Add(card.Data.Name + "を捨て札へ送った。");
        }

        bool defeatedByCard = enemyHpBefore > 0 && enemy.IsDefeated;
        bool weaknessHit = card.Data.Attribute != CardAttribute.Neutral && card.Data.Attribute == EnemyAI.GetWeakness(enemyType);
        bool isBoss = EnemyAI.IsBoss(enemyType);
        int accelGain = 0;
        bool cancelAttack = false;

        if (!isBoss && defeatedByCard)
        {
            battleLog.Add("予測行動でエネミーを撃破。");
            accelGain += 50;
            cancelAttack = true;
        }

        if (weaknessHit)
        {
            battleLog.Add(isBoss ? "ボスの弱点属性を突いた。" : "弱点属性を突いた。");
            accelGain += 50;
            cancelAttack = true;
        }

        if (accelGain > 0)
        {
            AddAccelGauge(accelGain, weaknessHit ? "弱点ヒット" : "予測成功");
        }

        if (defeatedByCard)
        {
            cancelAttack = true;
        }

        FinishPrediction(cancelAttack, cancelAttack && !defeatedByCard);
    }

    private static string GetPredictionMoveLog(MoveDirection direction)
    {
        switch (direction)
        {
            case MoveDirection.Forward:
                return "プレイヤーは回避行動として前進した。";
            case MoveDirection.Back:
                return "プレイヤーは回避行動として後退した。";
            case MoveDirection.Up:
                return "プレイヤーは回避行動として上へ移動した。";
            case MoveDirection.Down:
                return "プレイヤーは回避行動として下へ移動した。";
            default:
                return "プレイヤーは回避行動として移動した。";
        }
    }

    private void AddAccelGauge(int amount, string effectLabel = "アクセル上昇")
    {
        int before = accelGauge;
        accelGauge = Mathf.Clamp(accelGauge + amount, 0, MaxAccelGauge);
        int gained = accelGauge - before;
        if (gained > 0)
        {
            battleLog.Add("アクセルゲージが" + gained + "％上昇。");
            if (accelGaugeUI != null)
            {
                accelGaugeUI.SetValue(accelGauge, gained, effectLabel);
            }
            return;
        }

        battleLog.Add("アクセルゲージは最大です。");
        if (accelGaugeUI != null)
        {
            accelGaugeUI.SetValue(accelGauge);
        }
    }

    private IEnumerable<CharacterUnit> GetUnits()
    {
        yield return player;
        yield return enemy;
    }

    private void CheckBattleEnd()
    {
        if (battleEnded)
        {
            return;
        }

        if (enemy.IsDefeated)
        {
            battleEnded = true;
            previousBattleAccelGauge = accelGauge;
            battleLog.Add("エネミーを撃破。");
            battleLog.Add("プレイヤーの勝利。");
            ShowBattleResult();
        }
        else if (player.IsDefeated)
        {
            battleEnded = true;
            previousBattleAccelGauge = accelGauge;
            battleLog.Add("プレイヤーは倒れた。");
            battleLog.Add("敗北。");
        }
    }

    private void ShowBattleResult()
    {
        BattleResultData resultData = battleStatsTracker.CreateResultData(EnemyAI.IsBoss(enemyType), currentRound);
        resultData.HuntingLevel = HuntingLevelEvaluator.Evaluate(resultData);

        if (battleResultOverlay == null && battleCanvasRoot != null)
        {
            BuildBattleResultOverlay(battleCanvasRoot);
        }

        if (battleResultOverlay != null)
        {
            battleResultOverlay.Show(resultData, CreateTemporaryRewardLines(resultData.HuntingLevel));
        }
        else
        {
            Debug.LogError("Battle result overlay could not be created.");
        }
    }

    private List<string> CreateTemporaryRewardLines(HuntingLevel huntingLevel)
    {
        List<string> rewardCandidates = new List<string>();
        switch (huntingLevel)
        {
            case HuntingLevel.S:
                rewardCandidates.Add("フリーズ");
                rewardCandidates.Add("バーナーブレス");
                rewardCandidates.Add("テッキュウナゲ");
                break;
            case HuntingLevel.A:
                rewardCandidates.Add("仮カード");
                rewardCandidates.Add("アクアショット");
                rewardCandidates.Add("フリーズ");
                break;
            default:
                rewardCandidates.Add("仮カード");
                rewardCandidates.Add("アクアショット");
                rewardCandidates.Add("テッキュウナゲ");
                break;
        }

        int rewardIndex = UnityEngine.Random.Range(0, rewardCandidates.Count);
        return new List<string> { rewardCandidates[rewardIndex] };
    }

    private void RetryBattleFromResult()
    {
        StartBattle(battleStartAccelGauge, false);
    }

    private void ReturnToMenuFromResult()
    {
        SceneManager.LoadScene("MenuScene");
    }

    private void RefreshUi()
    {
        playerHpText.text = player.Hp.ToString();
        statusText.text = BuildBattleStatusText();
        accelGaugeUI.SetValue(accelGauge);
        if (positionText != null)
        {
            positionText.text = string.Empty;
        }

        if (deckText != null)
        {
            deckText.text = string.Empty;
        }

        if (actionQueueText != null)
        {
            RefreshActionQueueView();
        }

        if (enemyPlanText != null)
        {
            enemyPlanText.text = string.Empty;
        }

        if (enemyNameText != null)
        {
            enemyNameText.text = enemy != null ? EnemyAI.GetDisplayName(enemyType) : string.Empty;
        }

        if (predictionText != null)
        {
            predictionText.text = string.Empty;
        }

        if (logText != null)
        {
            logText.text = string.Empty;
        }

        confirmButton.interactable = !battleEnded && !predictionActive && actionQueue.Count > 0;
        resetSelectionButton.interactable = !battleEnded && !predictionActive && actionQueue.Count > 0;
        if (weaponButton != null)
        {
            weaponButton.interactable = !battleEnded && !predictionActive && !IsWeaponQueued();
        }
        int queuedActionCost = GetQueuedActionCost();
        SetMoveButtonsInteractable(!battleEnded && (predictionActive || queuedActionCost < MaxPlayerActions));
        RefreshActionCounter();

        RefreshGrid();

        for (int i = 0; i < cardViews.Count; i++)
        {
            CardData card = i < deck.Hand.Count ? deck.Hand[i].Data : null;
            bool cardQueued = i < deck.Hand.Count && IsCardQueued(deck.Hand[i]);
            bool actionLimitReached = !predictionActive && card != null && !card.IsClearCard && queuedActionCost >= MaxPlayerActions;
            cardViews[i].Refresh(card, battleEnded || (!predictionActive && (cardQueued || actionLimitReached)));
        }
    }

    private void RefreshActionCounter()
    {
        remainingPlayerActions = battleEnded ? 0 : GetRemainingPlayerActions();
        if (actionCountText != null)
        {
            actionCountText.text = string.Empty;
        }

        for (int i = 0; i < actionPips.Count; i++)
        {
            int activePips = predictionActive ? 1 : remainingPlayerActions;
            actionPips[i].color = i < activePips
                ? new Color(0.98f, 0.78f, 0.18f, 1f)
                : new Color(0.18f, 0.18f, 0.2f, 0.95f);
        }
    }

    private string BuildBattleStatusText()
    {
        if (battleEnded)
        {
            return enemy.IsDefeated ? "VICTORY" : "DEFEAT";
        }

        string turnText = predictionActive || pendingEnemyAttack != null ? "ENEMY TURN" : "PLAYER TURN";
        return "ROUND：" + currentRound + "\n" + turnText;
    }

    private string BuildPredictionText()
    {
        string text = "アクセルゲージ：" + accelGauge + "％"
            + "\n敵弱点：" + BattleText.FormatAttribute(EnemyAI.GetWeakness(enemyType));

        if (!predictionActive || pendingEnemyAttack == null)
        {
            return text + "\n攻撃予測：待機";
        }

        text += "\n攻撃予測発生中"
            + "\n予測範囲：" + EnemyAI.FormatAttackPattern(pendingEnemyAttack.AttackPattern)
            + "\nプレイヤー：" + (IsPlayerInEnemyAttackRange(pendingEnemyAttack) ? "攻撃範囲内" : "攻撃範囲外")
            + "\n移動またはカード1枚を選択";
        return text;
    }

    private string BuildActionQueueText()
    {
        if (actionQueue.Count == 0)
        {
            return "選択中アクション：なし";
        }

        string text = "選択中アクション：\n消費行動権：" + GetQueuedActionCost() + " / " + MaxPlayerActions;
        for (int i = 0; i < actionQueue.Count; i++)
        {
            text += "\n" + (i + 1) + ". " + actionQueue[i].DisplayName;
            if (!actionQueue[i].ConsumesAction)
            {
                text += " [CLEAR / 行動権消費なし]";
            }
        }

        return text;
    }

    private string BuildCompactActionQueueText()
    {
        if (actionQueue.Count == 0)
        {
            return "選択中アクション\nなし\n消費 0 / " + MaxPlayerActions;
        }

        string text = "選択中アクション\n消費 " + GetQueuedActionCost() + " / " + MaxPlayerActions;
        for (int i = 0; i < actionQueue.Count; i++)
        {
            text += "\n" + (i + 1) + " " + actionQueue[i].DisplayName;
            if (!actionQueue[i].ConsumesAction)
            {
                text += " 無消費";
            }
        }

        return text;
    }

    private void RefreshActionQueueView()
    {
        if (actionQueueRoot == null || actionQueueText == null)
        {
            return;
        }

        for (int i = 0; i < actionQueueItemObjects.Count; i++)
        {
            if (actionQueueItemObjects[i] != null)
            {
                Destroy(actionQueueItemObjects[i]);
            }
        }

        actionQueueItemObjects.Clear();
        bool hasActions = actionQueue.Count > 0;
        actionQueueText.gameObject.SetActive(!hasActions);
        if (!hasActions)
        {
            actionQueueText.text = "選択中アクション：なし";
            return;
        }

        int count = actionQueue.Count;
        float slotWidth = 1f / count;
        for (int i = 0; i < count; i++)
        {
            QueuedBattleAction action = actionQueue[i];
            float minX = i * slotWidth;
            float maxX = (i + 1) * slotWidth;
            Image slot = CreateImage("Queued Action " + (i + 1), actionQueueRoot, new Vector2(minX, 0f), new Vector2(maxX, 1f), new Vector2(4f, 3f), new Vector2(-4f, -3f), GetQueuedActionColor(action));
            slot.raycastTarget = false;
            actionQueueItemObjects.Add(slot.gameObject);

            CreateText("Queued Action Number " + (i + 1), slot.transform, new Vector2(0.03f, 0.56f), new Vector2(0.22f, 0.96f), Vector2.zero, Vector2.zero, (i + 1).ToString(), 18, TextAnchor.MiddleCenter, Color.white);
            Text nameText = CreateText("Queued Action Name " + (i + 1), slot.transform, new Vector2(0.18f, 0.28f), new Vector2(0.98f, 0.96f), Vector2.zero, Vector2.zero, action.DisplayName, 17, TextAnchor.MiddleCenter, Color.white);
            nameText.resizeTextForBestFit = true;
            nameText.resizeTextMinSize = 10;
            nameText.resizeTextMaxSize = 17;

            Text badgeText = CreateText("Queued Action Badge " + (i + 1), slot.transform, new Vector2(0.18f, 0.02f), new Vector2(0.98f, 0.32f), Vector2.zero, Vector2.zero, GetQueuedActionBadge(action), 12, TextAnchor.MiddleRight, new Color(1f, 0.96f, 0.72f));
            badgeText.resizeTextForBestFit = true;
            badgeText.resizeTextMinSize = 8;
            badgeText.resizeTextMaxSize = 12;
        }
    }

    private Color GetQueuedActionColor(QueuedBattleAction action)
    {
        if (action.Type == QueuedActionType.Weapon)
        {
            return new Color(0.68f, 0.72f, 0.76f, 0.96f);
        }

        if (action.Type == QueuedActionType.Move)
        {
            return new Color(0.72f, 0.56f, 0.16f, 0.96f);
        }

        if (action.Type == QueuedActionType.Card && action.Card != null && action.Card.Data != null)
        {
            if (action.Card.Data.IsClearCard)
            {
                return new Color(0.42f, 0.72f, 0.48f, 0.96f);
            }

            if (action.Card.Data.Effect == CardEffectType.Damage)
            {
                return new Color(0.18f, 0.43f, 0.78f, 0.96f);
            }
        }

        return new Color(0.20f, 0.24f, 0.32f, 0.96f);
    }

    private string GetQueuedActionBadge(QueuedBattleAction action)
    {
        if (action.Type == QueuedActionType.Card && action.Card != null && action.Card.Data != null && action.Card.Data.IsClearCard)
        {
            return "CLEAR";
        }

        return action.ConsumesAction ? "ACT" : "FREE";
    }

    private void SetMoveButtonsInteractable(bool interactable)
    {
        for (int i = 0; i < moveButtons.Count; i++)
        {
            moveButtons[i].interactable = interactable;
        }
    }

    private void DrawToHandLimit(string context)
    {
        DrawResult result = deck.DrawUpTo(MaxHandSize);
        if (result.Reshuffled)
        {
            battleLog.Add("捨て札をシャッフルして山札に戻しました。");
        }

        battleLog.Add(context + "：カードを" + result.DrawnCount + "枚引きました。手札 " + deck.HandCount + " / " + MaxHandSize + "。");
    }

    private void RefreshGrid()
    {
        for (int side = 0; side < 2; side++)
        {
            for (int row = 0; row < BattleGridPosition.GridSize; row++)
            {
                for (int column = 0; column < BattleGridPosition.GridSize; column++)
                {
                    BattleGridPosition position = new BattleGridPosition(side == PlayerSideIndex ? GridSide.Player : GridSide.Enemy, row, column);
                    gridSlots[side, row, column].color = GetBaseCellColor(position);
                    gridLabels[side, row, column].text = string.Empty;
                }
            }
        }

        foreach (BattleGridPosition position in previewCells)
        {
            if (position.IsValid)
            {
                gridSlots[GetSideIndex(position.Side), position.Row, position.Column].color = new Color(0.82f, 0.72f, 0.18f, 0.98f);
            }
        }

        SetUnitCell(player, "プ", new Color(0.12f, 0.64f, 0.74f, 0.98f));
        SetUnitCell(enemy, "敵", new Color(0.84f, 0.24f, 0.28f, 0.98f));
        RefreshEnemyHpText();
    }

    private void RefreshEnemyHpText()
    {
        if (enemyHpText == null)
        {
            return;
        }

        bool visible = enemy != null && !enemy.IsDefeated;
        enemyHpText.gameObject.SetActive(visible);
        if (!visible)
        {
            return;
        }

        int globalColumn = BattleGridPosition.GridSize + enemy.Position.Column;
        float xCenter = (globalColumn + 0.5f) / (BattleGridPosition.GridSize * 2f);
        float yTop = 1f - enemy.Position.Row / (float)BattleGridPosition.GridSize;
        RectTransform rect = enemyHpText.rectTransform;
        rect.anchorMin = new Vector2(xCenter, yTop);
        rect.anchorMax = new Vector2(xCenter, yTop);
        rect.offsetMin = new Vector2(-42f, -6f);
        rect.offsetMax = new Vector2(42f, 28f);
        enemyHpText.text = enemy.Hp.ToString();
    }

    private Color GetBaseCellColor(BattleGridPosition position)
    {
        switch (GetPanelType(position))
        {
            case PanelType.Cracked:
                return new Color(0.72f, 0.55f, 0.22f, 0.98f);
            case PanelType.Hole:
                return new Color(0.015f, 0.018f, 0.026f, 1f);
            case PanelType.Ice:
                return new Color(0.42f, 0.82f, 0.95f, 0.98f);
            case PanelType.Grass:
                return new Color(0.22f, 0.62f, 0.28f, 0.98f);
            case PanelType.Magma:
                return new Color(0.88f, 0.28f, 0.08f, 0.98f);
            case PanelType.Poison:
                return new Color(0.52f, 0.2f, 0.68f, 0.98f);
        }

        if (position.Side == GridSide.Player)
        {
            return new Color(0.16f, 0.28f, 0.42f, 0.96f);
        }

        return new Color(0.48f, 0.19f, 0.24f, 0.96f);
    }

    private static string GetPanelTypeDisplayName(PanelType panelType)
    {
        switch (panelType)
        {
            case PanelType.Cracked:
                return "ヒビパネル";
            case PanelType.Hole:
                return "穴パネル";
            case PanelType.Ice:
                return "氷パネル";
            case PanelType.Grass:
                return "草パネル";
            case PanelType.Magma:
                return "マグマパネル";
            case PanelType.Poison:
                return "毒パネル";
            default:
                return "ノーマルパネル";
        }
    }

    private void SetUnitCell(CharacterUnit unit, string label, Color color)
    {
        int sideIndex = GetSideIndex(unit.Position.Side);
        gridSlots[sideIndex, unit.Position.Row, unit.Position.Column].color = color;
        gridLabels[sideIndex, unit.Position.Row, unit.Position.Column].text = label;
    }

    private static int GetSideIndex(GridSide side)
    {
        return side == GridSide.Player ? PlayerSideIndex : EnemySideIndex;
    }

    private void ShowCardPreview(int handIndex)
    {
        if (handIndex < 0 || handIndex >= deck.Hand.Count)
        {
            ClearPreview();
            return;
        }

        string failureMessage;
        ShowCardPreview(deck.Hand[handIndex].Data, GetPreviewFailure(deck.Hand[handIndex].Data, out failureMessage) ? string.Empty : failureMessage);
    }

    private bool GetPreviewFailure(CardData card, out string failureMessage)
    {
        CharacterUnit target;
        BattleGridPosition destination;

        switch (card.Effect)
        {
            case CardEffectType.Damage:
            case CardEffectType.Freeze:
                TryGetDamageTarget(card, out target);
                failureMessage = target == null ? "攻撃範囲内にエネミーがいないため空振りします。" : string.Empty;
                return true;
            case CardEffectType.Move:
                return TryGetMoveDestination(card.MoveDirection, out destination, out failureMessage);
            default:
                failureMessage = string.Empty;
                return true;
        }
    }

    private void ShowCardPreview(CardData card, string reason)
    {
        previewCells.Clear();
        AddPreviewCells(card);

        if (rangeText != null)
        {
            rangeText.text = string.Empty;
        }

        if (cardHoverDetailView != null)
        {
            cardHoverDetailView.Show(card, DescribeRange(card), string.IsNullOrEmpty(reason) ? string.Empty : "理由：" + reason);
        }

        RefreshGrid();
    }

    private void ClearPreview()
    {
        previewCells.Clear();
        if (rangeText != null)
        {
            rangeText.text = string.Empty;
        }

        if (cardHoverDetailView != null)
        {
            cardHoverDetailView.Hide();
        }

        if (player != null && enemy != null)
        {
            RefreshGrid();
        }
    }

    private void RefreshPredictionPreview()
    {
        previewCells.Clear();
        AddEnemyAttackPreviewCells(pendingEnemyAttack);
        RefreshGrid();
    }

    private void AddEnemyAttackPreviewCells(EnemyBattleAction attackAction)
    {
        if (attackAction == null)
        {
            return;
        }

        switch (attackAction.AttackPattern)
        {
            case EnemyAttackPattern.ForwardOnePanel:
                previewCells.Add(new BattleGridPosition(GridSide.Player, enemy.Position.Row, BattleGridPosition.GridSize - 1));
                break;
            case EnemyAttackPattern.Row:
            case EnemyAttackPattern.Strong:
            case EnemyAttackPattern.SameRowNearest:
                for (int column = 0; column < BattleGridPosition.GridSize; column++)
                {
                    previewCells.Add(new BattleGridPosition(GridSide.Player, enemy.Position.Row, column));
                }
                break;
        }
    }

    private void AddPreviewCells(CardData card)
    {
        if (card.Effect == CardEffectType.Move)
        {
            BattleGridPosition destination;
            string failureMessage;
            TryGetMoveDestination(card.MoveDirection, out destination, out failureMessage);
            previewCells.Add(destination);
            return;
        }

        if (card.Effect == CardEffectType.StageChange)
        {
            for (int side = 0; side < 2; side++)
            {
                for (int row = 0; row < BattleGridPosition.GridSize; row++)
                {
                    for (int column = 0; column < BattleGridPosition.GridSize; column++)
                    {
                        previewCells.Add(new BattleGridPosition((GridSide)side, row, column));
                    }
                }
            }

            return;
        }

        if (card.Effect != CardEffectType.Damage && card.Effect != CardEffectType.Freeze)
        {
            previewCells.Add(player.Position);
            return;
        }

        switch (card.TargetPattern)
        {
            case CardTargetPattern.SameRowNearestEnemy:
            case CardTargetPattern.Row:
            case CardTargetPattern.ForwardSingle:
                for (int column = 0; column < BattleGridPosition.GridSize; column++)
                {
                    previewCells.Add(new BattleGridPosition(GridSide.Enemy, player.Position.Row, column));
                }
                break;
            case CardTargetPattern.ForwardLine3:
            case CardTargetPattern.ForwardExactly3:
                List<BattleGridPosition> path = BuildPlayerForwardPath(3);
                for (int i = 0; i < path.Count; i++)
                {
                    previewCells.Add(path[i]);
                }
                break;
            case CardTargetPattern.ForwardOnePanel:
                previewCells.Add(new BattleGridPosition(GridSide.Enemy, player.Position.Row, 0));
                break;
            case CardTargetPattern.SingleTarget:
                previewCells.Add(enemy.Position);
                break;
            case CardTargetPattern.AroundSelf:
                for (int row = player.Position.Row - 1; row <= player.Position.Row + 1; row++)
                {
                    for (int column = player.Position.Column - 1; column <= player.Position.Column + 1; column++)
                    {
                        BattleGridPosition position = new BattleGridPosition(player.Position.Side, row, column);
                        if (position.IsValid)
                        {
                            previewCells.Add(position);
                        }
                    }
                }
                break;
        }
    }

    private static string DescribeRange(CardData card)
    {
        if (card.Effect == CardEffectType.Move)
        {
            return BattleText.DescribeRange(card);
        }

        return BattleText.DescribeRange(card);
    }

    private void BuildUi()
    {
        GameObject canvasObject = new GameObject("Battle MVP Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        battleCanvasRoot = canvasObject.transform;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        Image background = CreateImage("Background", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.035f, 0.04f, 0.065f));
        background.raycastTarget = false;

        BuildAccelGaugeUi(canvasObject.transform);

        playerHpText = CreateText("Player HP", canvasObject.transform, new Vector2(0.012f, 0.905f), new Vector2(0.085f, 0.985f), Vector2.zero, Vector2.zero, string.Empty, 46, TextAnchor.MiddleCenter, Color.white);

        statusText = CreateText("Status Text", canvasObject.transform, new Vector2(0.39f, 0.86f), new Vector2(0.61f, 0.975f), Vector2.zero, Vector2.zero, string.Empty, 30, TextAnchor.MiddleCenter, new Color(0.95f, 1f, 0.78f));
        BuildActionCounter(canvasObject.transform);
        BuildEnemyNameView(canvasObject.transform);

        BuildBattleGrid(canvasObject.transform);
        BuildCardHoverDetail(canvasObject.transform);
        BuildActionQueueView(canvasObject.transform);

        BuildMoveCommands(canvasObject.transform);

        RectTransform handRoot = CreateRect("Hand", canvasObject.transform, new Vector2(0.03f, 0.04f), new Vector2(0.78f, 0.25f), Vector2.zero, Vector2.zero);
        BuildCardViews(handRoot);

        BuildBottomCommandButtons(canvasObject.transform);
        BuildEffectPresentation(canvasObject.transform);

        BuildDebugPanel(canvasObject.transform);
        BuildBattleResultOverlay(canvasObject.transform);
    }

    private void BuildBattleResultOverlay(Transform parent)
    {
        GameObject overlayObject = new GameObject("Battle Result Overlay");
        try
        {
            battleResultOverlay = overlayObject.AddComponent<BattleResultOverlay>();
            battleResultOverlay.Build(parent, uiFont, RetryBattleFromResult, ReturnToMenuFromResult);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            battleResultOverlay = null;
            Destroy(overlayObject);
        }
    }

    private void BuildEffectPresentation(Transform parent)
    {
        attackEffectPlayer = new AttackEffectPlayer();
        attackEffectPlayer.Build(parent, uiFont, this);
    }

    private void BuildEnemyNameView(Transform parent)
    {
        Image panel = CreateImage("Enemy Name Panel", parent, new Vector2(0.805f, 0.82f), new Vector2(0.995f, 0.91f), Vector2.zero, Vector2.zero, new Color(0.015f, 0.02f, 0.035f, 0.92f));
        panel.raycastTarget = false;
        enemyNameText = CreateText("Enemy Name Text", panel.transform, Vector2.zero, Vector2.one, new Vector2(12f, 6f), new Vector2(-16f, -6f), string.Empty, 32, TextAnchor.MiddleRight, new Color(1f, 0.97f, 0.86f));
        enemyNameText.resizeTextForBestFit = true;
        enemyNameText.resizeTextMinSize = 18;
        enemyNameText.resizeTextMaxSize = 32;
    }

    private void BuildDebugPanel(Transform parent)
    {
        GameObject debugObject = new GameObject("BattleDebugPanelController");
        debugObject.transform.SetParent(parent, false);
        debugPanelController = debugObject.AddComponent<BattleDebugPanelController>();
        debugPanelController.Build(null, parent, uiFont, false);
    }

    private bool ShouldShowDebugPanelTools()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        return showDebugPanelTools;
#else
        return false;
#endif
    }

    private void BuildAccelGaugeUi(Transform parent)
    {
        GameObject gaugeObject = new GameObject("AccelGaugeUI");
        accelGaugeUI = gaugeObject.AddComponent<AccelGaugeUI>();
        accelGaugeUI.Build(parent, uiFont);
    }

    private void BuildCardHoverDetail(Transform parent)
    {
        cardHoverDetailView = new CardHoverDetailView();
        cardHoverDetailView.Build(parent, uiFont);
    }

    private void BuildActionQueueView(Transform parent)
    {
        Image panel = CreateImage("Selected Action Queue Panel", parent, new Vector2(0.03f, 0.325f), new Vector2(0.78f, 0.415f), Vector2.zero, Vector2.zero, new Color(0.012f, 0.018f, 0.032f, 0.94f));
        panel.raycastTarget = false;
        actionQueueRoot = CreateRect("Selected Action Queue Items", panel.transform, Vector2.zero, Vector2.one, new Vector2(8f, 5f), new Vector2(-8f, -5f));
        actionQueueText = CreateText("Selected Action Queue Empty Text", panel.transform, Vector2.zero, Vector2.one, new Vector2(12f, 8f), new Vector2(-12f, -8f), "選択中アクション：なし", 20, TextAnchor.MiddleCenter, new Color(1f, 0.96f, 0.72f));
        actionQueueText.resizeTextForBestFit = true;
        actionQueueText.resizeTextMinSize = 12;
        actionQueueText.resizeTextMaxSize = 20;
    }

    private void BuildBottomCommandButtons(Transform parent)
    {
        weaponButton = CreateButton("Weapon Command Button", parent, new Vector2(0.80f, 0.18f), new Vector2(0.96f, 0.25f), Vector2.zero, Vector2.zero, WeaponDisplayName, 22, new Color(0.2f, 0.34f, 0.46f));
        weaponButton.onClick.AddListener(HandleWeaponCommand);

        resetSelectionButton = CreateButton("Reset Actions Button", parent, new Vector2(0.80f, 0.11f), new Vector2(0.96f, 0.175f), Vector2.zero, Vector2.zero, "選択リセット", 19, new Color(0.32f, 0.32f, 0.38f));
        resetSelectionButton.onClick.AddListener(ResetQueuedActions);

        confirmButton = CreateButton("Confirm Actions Button", parent, new Vector2(0.80f, 0.04f), new Vector2(0.96f, 0.105f), Vector2.zero, Vector2.zero, "決定", 24, new Color(0.14f, 0.48f, 0.28f));
        confirmButton.onClick.AddListener(ConfirmQueuedActions);
    }

    private void BuildActionCounter(Transform parent)
    {
        Image panel = CreateImage("Action Gauge Panel", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(128f, -112f), new Vector2(452f, -88f), new Color(0.015f, 0.018f, 0.025f, 0.96f));
        panel.raycastTarget = false;

        for (int i = 0; i < MaxPlayerActions; i++)
        {
            float minX = i / (float)MaxPlayerActions;
            float maxX = (i + 1) / (float)MaxPlayerActions;
            Image pip = CreateImage("Action Pip " + (i + 1), panel.transform, new Vector2(minX, 0f), new Vector2(maxX, 1f), new Vector2(4f, 4f), new Vector2(-4f, -4f), new Color(0.98f, 0.78f, 0.18f, 1f));
            pip.raycastTarget = false;
            actionPips.Add(pip);
        }
    }

    private void BuildMoveCommands(Transform parent)
    {
        AddMoveButton(parent, "Move Forward Button", "前進", MoveDirection.Forward, new Vector2(0.04f, 0.265f), new Vector2(0.14f, 0.31f));
        AddMoveButton(parent, "Move Back Button", "後退", MoveDirection.Back, new Vector2(0.15f, 0.265f), new Vector2(0.25f, 0.31f));
        AddMoveButton(parent, "Move Up Button", "上", MoveDirection.Up, new Vector2(0.26f, 0.265f), new Vector2(0.36f, 0.31f));
        AddMoveButton(parent, "Move Down Button", "下", MoveDirection.Down, new Vector2(0.37f, 0.265f), new Vector2(0.47f, 0.31f));
    }

    private void AddMoveButton(Transform parent, string objectName, string label, MoveDirection direction, Vector2 anchorMin, Vector2 anchorMax)
    {
        Button button = CreateButton(objectName, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero, label, 20, new Color(0.16f, 0.34f, 0.36f));
        MoveDirection capturedDirection = direction;
        button.onClick.AddListener(() => HandleMoveCommand(capturedDirection));
        moveButtons.Add(button);
    }

    private void BuildBattleGrid(Transform parent)
    {
        Image field = CreateImage("Battle Field", parent, new Vector2(0.19f, 0.43f), new Vector2(0.81f, 0.72f), Vector2.zero, Vector2.zero, new Color(0.015f, 0.02f, 0.035f, 0.96f));
        field.raycastTarget = false;

        BuildPanel(field.transform, GridSide.Player, new Vector2(0f, 0f), new Vector2(0.5f, 1f));
        BuildPanel(field.transform, GridSide.Enemy, new Vector2(0.5f, 0f), new Vector2(1f, 1f));

        Image divider = CreateImage("Battle Field Divider", field.transform, new Vector2(0.497f, 0f), new Vector2(0.503f, 1f), Vector2.zero, Vector2.zero, new Color(0.85f, 0.95f, 1f, 0.7f));
        divider.raycastTarget = false;

        enemyHpText = CreateText("Enemy Floating HP", field.transform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, string.Empty, 26, TextAnchor.MiddleCenter, Color.white);
    }

    private void BuildPanel(Transform parent, GridSide side, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform panelRoot = CreateRect(side + " Panel", parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        int sideIndex = GetSideIndex(side);

        for (int row = 0; row < BattleGridPosition.GridSize; row++)
        {
            for (int column = 0; column < BattleGridPosition.GridSize; column++)
            {
                float minX = column / (float)BattleGridPosition.GridSize;
                float maxX = (column + 1) / (float)BattleGridPosition.GridSize;
                float maxY = 1f - row / (float)BattleGridPosition.GridSize;
                float minY = 1f - (row + 1) / (float)BattleGridPosition.GridSize;

                Image slot = CreateImage(side + " Cell " + row + "-" + column, panelRoot, new Vector2(minX, minY), new Vector2(maxX, maxY), new Vector2(2f, 2f), new Vector2(-2f, -2f), side == GridSide.Player ? new Color(0.16f, 0.28f, 0.42f, 0.96f) : new Color(0.48f, 0.19f, 0.24f, 0.96f));
                BattleGridPosition capturedPosition = new BattleGridPosition(side, row, column);
                EventTrigger trigger = slot.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry clickEntry = new EventTrigger.Entry();
                clickEntry.eventID = EventTriggerType.PointerClick;
                clickEntry.callback.AddListener(_ => HandleDebugPanelCellClicked(capturedPosition));
                trigger.triggers.Add(clickEntry);
                Text label = CreateText(side + " Cell Label " + row + "-" + column, slot.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, string.Empty, 40, TextAnchor.MiddleCenter, Color.white);
                gridSlots[sideIndex, row, column] = slot;
                gridLabels[sideIndex, row, column] = label;
            }
        }
    }

    private void BuildCardViews(RectTransform handRoot)
    {
        for (int i = 0; i < MaxHandSize; i++)
        {
            float min = i / (float)MaxHandSize;
            float max = (i + 1) / (float)MaxHandSize;
            Button button = CreateButton("Card " + (i + 1), handRoot, new Vector2(min, 0f), new Vector2(max, 1f), new Vector2(8f, 0f), new Vector2(-8f, 0f), string.Empty, 20, new Color(0.18f, 0.23f, 0.32f));
            CardView cardView = button.gameObject.AddComponent<CardView>();
            cardView.Initialize(button, i, PlayCard, ShowCardPreview, ClearPreview);
            cardViews.Add(cardView);
        }
    }

    private RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        RectTransform rectTransform = go.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = offsetMin;
        rectTransform.offsetMax = offsetMax;
        return rectTransform;
    }

    private Image CreateImage(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        RectTransform rectTransform = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Image image = rectTransform.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private Text CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string text, int fontSize, TextAnchor alignment, Color color)
    {
        RectTransform rectTransform = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Text label = rectTransform.gameObject.AddComponent<Text>();
        label.text = text;
        label.font = uiFont;
        label.fontSize = fontSize;
        label.fontStyle = FontStyle.Bold;
        label.alignment = alignment;
        label.color = color;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        label.raycastTarget = false;

        Shadow shadow = rectTransform.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.78f);
        shadow.effectDistance = new Vector2(2f, -2f);

        Outline outline = rectTransform.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.72f);
        outline.effectDistance = new Vector2(1f, -1f);

        return label;
    }

    private Button CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string labelText, int fontSize, Color color)
    {
        Image image = CreateImage(name, parent, anchorMin, anchorMax, offsetMin, offsetMax, color);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.16f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.18f, 0.18f, 0.2f, 0.55f);
        button.colors = colors;

        Text label = CreateText(name + " Text", image.transform, Vector2.zero, Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -8f), labelText, fontSize, TextAnchor.MiddleCenter, Color.white);
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 12;
        label.resizeTextMaxSize = fontSize;

        return button;
    }
}
