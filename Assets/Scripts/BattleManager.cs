using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class BattleManager : MonoBehaviour
{
    private const int MaxHandSize = 5;
    private const int MaxPlayerActions = 3;
    private const int MaxAccelGauge = 100;
    private const int PlayerSideIndex = 0;
    private const int EnemySideIndex = 1;
    private static int previousBattleAccelGauge;

    [SerializeField] private List<CardData> starterDeck = new List<CardData>();
    [SerializeField] private EnemyType enemyType = EnemyType.NormalEnemy;

    private readonly IAttackPredictionChanceProvider predictionChanceProvider = new TestAttackPredictionChanceProvider();
    private readonly List<CardView> cardViews = new List<CardView>();
    private readonly List<Button> moveButtons = new List<Button>();
    private readonly List<QueuedBattleAction> actionQueue = new List<QueuedBattleAction>();
    private readonly Image[,,] gridSlots = new Image[2, BattleGridPosition.GridSize, BattleGridPosition.GridSize];
    private readonly Text[,,] gridLabels = new Text[2, BattleGridPosition.GridSize, BattleGridPosition.GridSize];
    private readonly List<BattleGridPosition> previewCells = new List<BattleGridPosition>();

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

    private Text playerHpText;
    private Text enemyHpText;
    private Text positionText;
    private Text statusText;
    private Text actionCountText;
    private Text actionQueueText;
    private Text enemyPlanText;
    private Text predictionText;
    private Text rangeText;
    private Text deckText;
    private Text logText;
    private AccelGaugeUI accelGaugeUI;
    private Button confirmButton;
    private Button resetSelectionButton;
    private readonly List<Image> actionPips = new List<Image>();

    private enum QueuedActionType
    {
        Card,
        Move
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
        string[] fontNames = { "Yu Gothic", "Meiryo", "MS Gothic", "Noto Sans CJK JP", "Arial" };
        return Font.CreateDynamicFontFromOSFont(fontNames, 20);
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

    private void StartBattle()
    {
        player = new CharacterUnit(BattleText.PlayerName, 180, new BattleGridPosition(GridSide.Player, 1, 1));
        enemy = new CharacterUnit(BattleText.EnemyName, EnemyAI.GetMaxHp(enemyType), new BattleGridPosition(GridSide.Enemy, 1, 1));
        battleEnded = false;
        predictionActive = false;
        currentRound = 1;
        remainingPlayerActions = MaxPlayerActions;
        accelGauge = previousBattleAccelGauge >= 50 ? 50 : 0;
        enemyActionsRemaining = 0;
        enemyActionIndex = 0;
        pendingPlayerAttackBonus = 0;
        pendingEnemyAttack = null;
        actionQueue.Clear();

        battleLog = new BattleLog(10);
        enemyAI = new EnemyAI(enemyType);
        deck = new DeckManager(GetStarterDeck());

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

    private bool TryResolveCard(CardData card, out string failureMessage, bool predictionAction = false)
    {
        switch (card.Effect)
        {
            case CardEffectType.Damage:
                CharacterUnit target;
                TryGetDamageTarget(card, out target);
                battleLog.Add(predictionAction ? "プレイヤーは予測行動で" + card.Name + "を使用。" : "プレイヤーは" + card.Name + "を使用。");
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

                player.MoveTo(destination);
                battleLog.Add(predictionAction ? "プレイヤーは予測行動で" + card.Name + "を使用。" : "プレイヤーは" + card.Name + "を使用。");
                battleLog.Add(destination + "へ移動しました。");
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
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DealDamageToEnemy(CharacterUnit target, int damage)
    {
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
            case CardTargetPattern.SingleTarget:
                target = enemy;
                break;
            case CardTargetPattern.AroundSelf:
                target = IsAroundSelf(enemy.Position) ? enemy : null;
                break;
        }

        return target != null;
    }

    private bool TryGetMoveDestination(MoveDirection direction, out BattleGridPosition destination, out string failureMessage)
    {
        int rowDelta = 0;
        int columnDelta = 0;

        switch (direction)
        {
            case MoveDirection.Forward:
                columnDelta = BattleGridPosition.ForwardColumnDelta(player.Position.Side);
                break;
            case MoveDirection.Back:
                columnDelta = BattleGridPosition.BackColumnDelta(player.Position.Side);
                break;
            case MoveDirection.Up:
                rowDelta = -1;
                break;
            case MoveDirection.Down:
                rowDelta = 1;
                break;
        }

        destination = player.Position.Offset(rowDelta, columnDelta);
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

        failureMessage = string.Empty;
        return true;
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

        player.MoveTo(destination);
        battleLog.Add(GetMoveCommandLog(direction));
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
            enemy.MoveTo(action.Destination);
            battleLog.Add(action.ActionText);
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

        DealDamageToPlayer(attackAction.Damage);
    }

    private void DealDamageToPlayer(int damage)
    {
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
    }

    private bool IsPlayerInEnemyAttackRange(EnemyBattleAction attackAction)
    {
        switch (attackAction.AttackPattern)
        {
            case EnemyAttackPattern.ForwardOnePanel:
                return enemy.Position.Column == 0 && player.Position.Column == BattleGridPosition.GridSize - 1 && enemy.Position.Row == player.Position.Row;
            case EnemyAttackPattern.Row:
            case EnemyAttackPattern.Strong:
            case EnemyAttackPattern.SameRowNearest:
                return enemy.Position.Row == player.Position.Row;
            default:
                return false;
        }
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

        player.MoveTo(destination);
        battleLog.Add(GetPredictionMoveLog(direction));

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
        }
        else if (player.IsDefeated)
        {
            battleEnded = true;
            previousBattleAccelGauge = accelGauge;
            battleLog.Add("プレイヤーは倒れた。");
            battleLog.Add("敗北。");
        }
    }

    private void RefreshUi()
    {
        playerHpText.text = BattleText.PlayerName + "\nHP " + player.Hp + "/" + player.MaxHp + "\nガード " + player.Guard;
        enemyHpText.text = BattleText.EnemyName + "\nタイプ " + EnemyAI.GetDisplayName(enemyType) + "\nHP " + enemy.Hp + "/" + enemy.MaxHp + "\nガード " + enemy.Guard;
        positionText.text = "プレイヤー：" + player.Position + "\nエネミー：" + enemy.Position;
        statusText.text = battleEnded ? (enemy.IsDefeated ? BattleText.Victory : BattleText.Defeat) : "ラウンド：" + currentRound + "\n" + BattleText.PlayerTurn;
        accelGaugeUI.SetValue(accelGauge);
        deckText.text = "山札：" + deck.DrawPileCount + "\n手札：" + deck.HandCount + " / " + MaxHandSize + "\n捨て札：" + deck.DiscardPileCount;
        actionQueueText.text = BuildActionQueueText();
        enemyPlanText.text = EnemyAI.GetPlanText(enemyType);
        predictionText.text = BuildPredictionText();
        logText.text = battleLog.DisplayText;
        confirmButton.interactable = !battleEnded && !predictionActive && actionQueue.Count > 0;
        resetSelectionButton.interactable = !battleEnded && !predictionActive && actionQueue.Count > 0;
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
        if (predictionActive)
        {
            actionCountText.text = "攻撃予測：1回だけ即時行動";
        }
        else
        {
            actionCountText.text = "残り行動権  " + remainingPlayerActions + " / " + MaxPlayerActions;
        }

        for (int i = 0; i < actionPips.Count; i++)
        {
            int activePips = predictionActive ? 1 : remainingPlayerActions;
            actionPips[i].color = i < activePips
                ? new Color(0.98f, 0.78f, 0.18f, 1f)
                : new Color(0.18f, 0.18f, 0.2f, 0.95f);
        }
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
    }

    private Color GetBaseCellColor(BattleGridPosition position)
    {
        if (position.Side == GridSide.Player)
        {
            return new Color(0.16f, 0.25f, 0.36f, 0.95f);
        }

        return new Color(0.35f, 0.18f, 0.22f, 0.95f);
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

        string suffix = string.IsNullOrEmpty(reason) ? string.Empty : "\n理由：" + reason;
        rangeText.text = card.Name + "\n範囲：" + DescribeRange(card) + suffix;
        RefreshGrid();
    }

    private void ClearPreview()
    {
        previewCells.Clear();
        if (rangeText != null)
        {
            rangeText.text = BattleText.HoverPreview;
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

        if (card.Effect != CardEffectType.Damage)
        {
            previewCells.Add(player.Position);
            return;
        }

        switch (card.TargetPattern)
        {
            case CardTargetPattern.SameRowNearestEnemy:
            case CardTargetPattern.Row:
                for (int column = 0; column < BattleGridPosition.GridSize; column++)
                {
                    previewCells.Add(new BattleGridPosition(GridSide.Enemy, player.Position.Row, column));
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

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image background = CreateImage("Background", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.04f, 0.05f, 0.07f));
        background.raycastTarget = false;

        CreateText("Title", canvasObject.transform, new Vector2(0.03f, 0.91f), new Vector2(0.97f, 0.98f), Vector2.zero, Vector2.zero, "NEON CARDIA - 3x3パネルバトルMVP", 32, TextAnchor.MiddleCenter, Color.white);
        BuildAccelGaugeUi(canvasObject.transform);

        playerHpText = CreateText("Player HP", canvasObject.transform, new Vector2(0.04f, 0.72f), new Vector2(0.2f, 0.88f), Vector2.zero, Vector2.zero, string.Empty, 25, TextAnchor.MiddleCenter, Color.white);
        enemyHpText = CreateText("Enemy HP", canvasObject.transform, new Vector2(0.8f, 0.72f), new Vector2(0.96f, 0.88f), Vector2.zero, Vector2.zero, string.Empty, 25, TextAnchor.MiddleCenter, Color.white);

        statusText = CreateText("Status Text", canvasObject.transform, new Vector2(0.36f, 0.82f), new Vector2(0.64f, 0.89f), Vector2.zero, Vector2.zero, string.Empty, 25, TextAnchor.MiddleCenter, new Color(0.95f, 0.95f, 0.72f));
        positionText = CreateText("Position Text", canvasObject.transform, new Vector2(0.35f, 0.72f), new Vector2(0.65f, 0.81f), Vector2.zero, Vector2.zero, string.Empty, 21, TextAnchor.MiddleCenter, Color.white);
        BuildActionCounter(canvasObject.transform);

        BuildBattleGrid(canvasObject.transform);

        rangeText = CreateText("Range Text", canvasObject.transform, new Vector2(0.05f, 0.32f), new Vector2(0.48f, 0.43f), Vector2.zero, Vector2.zero, BattleText.HoverPreview, 20, TextAnchor.UpperLeft, new Color(0.95f, 0.9f, 0.65f));

        BuildMoveCommands(canvasObject.transform);

        deckText = CreateText("Deck Text", canvasObject.transform, new Vector2(0.38f, 0.48f), new Vector2(0.62f, 0.66f), Vector2.zero, Vector2.zero, string.Empty, 22, TextAnchor.MiddleCenter, Color.white);
        actionQueueText = CreateText("Action Queue Text", canvasObject.transform, new Vector2(0.39f, 0.3f), new Vector2(0.51f, 0.45f), Vector2.zero, Vector2.zero, string.Empty, 18, TextAnchor.UpperLeft, new Color(0.96f, 0.96f, 0.9f));
        enemyPlanText = CreateText("Enemy Plan Text", canvasObject.transform, new Vector2(0.66f, 0.72f), new Vector2(0.78f, 0.88f), Vector2.zero, Vector2.zero, string.Empty, 18, TextAnchor.MiddleLeft, new Color(0.9f, 0.9f, 0.82f));
        predictionText = CreateText("Prediction Text", canvasObject.transform, new Vector2(0.82f, 0.3f), new Vector2(0.95f, 0.45f), Vector2.zero, Vector2.zero, string.Empty, 16, TextAnchor.UpperLeft, new Color(0.9f, 0.96f, 1f));

        Image logPanel = CreateImage("Log Panel", canvasObject.transform, new Vector2(0.52f, 0.3f), new Vector2(0.81f, 0.45f), Vector2.zero, Vector2.zero, new Color(0.09f, 0.1f, 0.13f, 0.92f));
        logPanel.raycastTarget = false;
        logText = CreateText("Log Text", logPanel.transform, Vector2.zero, Vector2.one, new Vector2(18f, 10f), new Vector2(-18f, -10f), string.Empty, 18, TextAnchor.UpperLeft, new Color(0.92f, 0.94f, 0.96f));

        RectTransform handRoot = CreateRect("Hand", canvasObject.transform, new Vector2(0.03f, 0.04f), new Vector2(0.78f, 0.25f), Vector2.zero, Vector2.zero);
        BuildCardViews(handRoot);

        confirmButton = CreateButton("Confirm Actions Button", canvasObject.transform, new Vector2(0.82f, 0.08f), new Vector2(0.95f, 0.22f), Vector2.zero, Vector2.zero, "決定", 26, new Color(0.14f, 0.48f, 0.28f));
        confirmButton.onClick.AddListener(ConfirmQueuedActions);

        resetSelectionButton = CreateButton("Reset Actions Button", canvasObject.transform, new Vector2(0.82f, 0.225f), new Vector2(0.95f, 0.285f), Vector2.zero, Vector2.zero, "選択リセット", 20, new Color(0.32f, 0.32f, 0.38f));
        resetSelectionButton.onClick.AddListener(ResetQueuedActions);
    }

    private void BuildAccelGaugeUi(Transform parent)
    {
        GameObject gaugeObject = new GameObject("AccelGaugeUI");
        accelGaugeUI = gaugeObject.AddComponent<AccelGaugeUI>();
        accelGaugeUI.Build(parent, uiFont);
    }

    private void BuildActionCounter(Transform parent)
    {
        Image panel = CreateImage("Action Counter Panel", parent, new Vector2(0.37f, 0.655f), new Vector2(0.63f, 0.725f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.12f, 0.16f, 0.96f));
        panel.raycastTarget = false;

        actionCountText = CreateText("Action Counter Text", panel.transform, new Vector2(0f, 0.35f), new Vector2(1f, 1f), new Vector2(8f, 0f), new Vector2(-8f, -2f), string.Empty, 30, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.35f));

        for (int i = 0; i < MaxPlayerActions; i++)
        {
            float minX = 0.18f + i * 0.23f;
            float maxX = minX + 0.18f;
            Image pip = CreateImage("Action Pip " + (i + 1), panel.transform, new Vector2(minX, 0.13f), new Vector2(maxX, 0.3f), Vector2.zero, Vector2.zero, new Color(0.98f, 0.78f, 0.18f, 1f));
            pip.raycastTarget = false;
            actionPips.Add(pip);
        }
    }

    private void BuildMoveCommands(Transform parent)
    {
        CreateText("Move Commands Label", parent, new Vector2(0.05f, 0.265f), new Vector2(0.13f, 0.31f), Vector2.zero, Vector2.zero, "移動", 20, TextAnchor.MiddleLeft, Color.white);
        AddMoveButton(parent, "Move Forward Button", "前進", MoveDirection.Forward, new Vector2(0.13f, 0.265f), new Vector2(0.22f, 0.31f));
        AddMoveButton(parent, "Move Back Button", "後退", MoveDirection.Back, new Vector2(0.23f, 0.265f), new Vector2(0.32f, 0.31f));
        AddMoveButton(parent, "Move Up Button", "上", MoveDirection.Up, new Vector2(0.33f, 0.265f), new Vector2(0.41f, 0.31f));
        AddMoveButton(parent, "Move Down Button", "下", MoveDirection.Down, new Vector2(0.42f, 0.265f), new Vector2(0.5f, 0.31f));
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
        CreateText("Player Panel Label", parent, new Vector2(0.08f, 0.66f), new Vector2(0.36f, 0.71f), Vector2.zero, Vector2.zero, BattleText.PlayerPanel, 20, TextAnchor.MiddleCenter, Color.white);
        CreateText("Enemy Panel Label", parent, new Vector2(0.64f, 0.66f), new Vector2(0.92f, 0.71f), Vector2.zero, Vector2.zero, BattleText.EnemyPanel, 20, TextAnchor.MiddleCenter, Color.white);

        BuildPanel(parent, GridSide.Player, new Vector2(0.08f, 0.46f), new Vector2(0.36f, 0.66f));
        BuildPanel(parent, GridSide.Enemy, new Vector2(0.64f, 0.46f), new Vector2(0.92f, 0.66f));
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

                Image slot = CreateImage(side + " Cell " + row + "-" + column, panelRoot, new Vector2(minX, minY), new Vector2(maxX, maxY), new Vector2(4f, 4f), new Vector2(-4f, -4f), side == GridSide.Player ? new Color(0.16f, 0.25f, 0.36f, 0.95f) : new Color(0.35f, 0.18f, 0.22f, 0.95f));
                Text label = CreateText(side + " Cell Label " + row + "-" + column, slot.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, string.Empty, 34, TextAnchor.MiddleCenter, Color.white);
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
        label.alignment = alignment;
        label.color = color;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
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
