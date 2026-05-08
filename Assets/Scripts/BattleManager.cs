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
    private const int PlayerSideIndex = 0;
    private const int EnemySideIndex = 1;

    private readonly List<CardView> cardViews = new List<CardView>();
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

    private Text playerHpText;
    private Text enemyHpText;
    private Text positionText;
    private Text statusText;
    private Text rangeText;
    private Text deckText;
    private Text logText;
    private Button endTurnButton;

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
        player = new CharacterUnit(BattleText.PlayerName, 32, new BattleGridPosition(GridSide.Player, 1, 1));
        enemy = new CharacterUnit(BattleText.EnemyName, 36, new BattleGridPosition(GridSide.Enemy, 1, 1));
        battleEnded = false;

        battleLog = new BattleLog(10);
        enemyAI = new EnemyAI(EnemyType.ShooterEnemy);
        deck = new DeckManager(CardData.CreateStarterDeck());

        battleLog.Add("バトル開始");
        battleLog.Add("初期デッキをシャッフルしました。");
        DrawToHandLimit("ターン開始");
        battleLog.Add("プレイヤーの初期位置：" + player.Position);
        battleLog.Add("エネミーの初期位置：" + enemy.Position);
        RefreshUi();
    }

    private void PlayCard(int handIndex)
    {
        if (battleEnded || handIndex < 0 || handIndex >= deck.Hand.Count)
        {
            return;
        }

        CardInstance card = deck.Hand[handIndex];
        string failureMessage;
        if (!TryResolveCard(card.Data, out failureMessage))
        {
            battleLog.Add(failureMessage);
            ShowCardPreview(card.Data, failureMessage);
            RefreshUi();
            return;
        }

        deck.DiscardFromHand(handIndex);
        battleLog.Add(card.Data.Name + "を捨て札に置きました。");

        ClearPreview();
        CheckBattleEnd();
        RefreshUi();
    }

    private bool TryResolveCard(CardData card, out string failureMessage)
    {
        switch (card.Effect)
        {
            case CardEffectType.Damage:
                CharacterUnit target;
                TryGetDamageTarget(card, out target);
                battleLog.Add("プレイヤーは" + card.Name + "を使用。");
                if (target == null)
                {
                    battleLog.Add("しかし攻撃範囲内にエネミーはいなかった。");
                    battleLog.Add("攻撃は空振りした。");
                    failureMessage = string.Empty;
                    return true;
                }

                DealDamageToEnemy(target, card.Power);
                failureMessage = string.Empty;
                return true;
            case CardEffectType.Guard:
                player.Guard += card.Power;
                battleLog.Add("プレイヤーは" + card.Name + "を使用。");
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
                battleLog.Add("プレイヤーは" + card.Name + "を使用。");
                battleLog.Add(destination + "へ移動しました。");
                failureMessage = string.Empty;
                return true;
            case CardEffectType.Repair:
                int before = player.Hp;
                player.Heal(card.Power);
                int recovered = player.Hp - before;
                battleLog.Add("プレイヤーは" + card.Name + "を使用。");
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

        battleLog.Add("プレイヤーはターンを終了。");
        enemyAI.Act(player, enemy, GetUnits(), battleLog);
        CheckBattleEnd();

        if (!battleEnded)
        {
            DrawToHandLimit("ターン開始");
        }

        ClearPreview();
        RefreshUi();
    }

    private IEnumerable<CharacterUnit> GetUnits()
    {
        yield return player;
        yield return enemy;
    }

    private void CheckBattleEnd()
    {
        if (enemy.IsDefeated)
        {
            battleEnded = true;
            battleLog.Add("エネミーを撃破。");
            battleLog.Add("プレイヤーの勝利。");
        }
        else if (player.IsDefeated)
        {
            battleEnded = true;
            battleLog.Add("プレイヤーは倒れた。");
            battleLog.Add("敗北。");
        }
    }

    private void RefreshUi()
    {
        playerHpText.text = BattleText.PlayerName + "\nHP " + player.Hp + "/" + player.MaxHp + "\nガード " + player.Guard;
        enemyHpText.text = BattleText.EnemyName + "\nHP " + enemy.Hp + "/" + enemy.MaxHp + "\nガード " + enemy.Guard;
        positionText.text = "プレイヤー：" + player.Position + "\nエネミー：" + enemy.Position;
        statusText.text = battleEnded ? (enemy.IsDefeated ? BattleText.Victory : BattleText.Defeat) : BattleText.PlayerTurn;
        deckText.text = "山札：" + deck.DrawPileCount + "\n手札：" + deck.HandCount + " / " + MaxHandSize + "\n捨て札：" + deck.DiscardPileCount;
        logText.text = battleLog.DisplayText;
        endTurnButton.interactable = !battleEnded;

        RefreshGrid();

        for (int i = 0; i < cardViews.Count; i++)
        {
            CardData card = i < deck.Hand.Count ? deck.Hand[i].Data : null;
            cardViews[i].Refresh(card, battleEnded);
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

        playerHpText = CreateText("Player HP", canvasObject.transform, new Vector2(0.04f, 0.72f), new Vector2(0.2f, 0.88f), Vector2.zero, Vector2.zero, string.Empty, 25, TextAnchor.MiddleCenter, Color.white);
        enemyHpText = CreateText("Enemy HP", canvasObject.transform, new Vector2(0.8f, 0.72f), new Vector2(0.96f, 0.88f), Vector2.zero, Vector2.zero, string.Empty, 25, TextAnchor.MiddleCenter, Color.white);

        statusText = CreateText("Status Text", canvasObject.transform, new Vector2(0.36f, 0.82f), new Vector2(0.64f, 0.89f), Vector2.zero, Vector2.zero, string.Empty, 25, TextAnchor.MiddleCenter, new Color(0.95f, 0.95f, 0.72f));
        positionText = CreateText("Position Text", canvasObject.transform, new Vector2(0.35f, 0.72f), new Vector2(0.65f, 0.81f), Vector2.zero, Vector2.zero, string.Empty, 21, TextAnchor.MiddleCenter, Color.white);

        BuildBattleGrid(canvasObject.transform);

        rangeText = CreateText("Range Text", canvasObject.transform, new Vector2(0.05f, 0.32f), new Vector2(0.48f, 0.43f), Vector2.zero, Vector2.zero, BattleText.HoverPreview, 20, TextAnchor.UpperLeft, new Color(0.95f, 0.9f, 0.65f));

        deckText = CreateText("Deck Text", canvasObject.transform, new Vector2(0.38f, 0.48f), new Vector2(0.62f, 0.66f), Vector2.zero, Vector2.zero, string.Empty, 22, TextAnchor.MiddleCenter, Color.white);

        Image logPanel = CreateImage("Log Panel", canvasObject.transform, new Vector2(0.52f, 0.3f), new Vector2(0.95f, 0.45f), Vector2.zero, Vector2.zero, new Color(0.09f, 0.1f, 0.13f, 0.92f));
        logPanel.raycastTarget = false;
        logText = CreateText("Log Text", logPanel.transform, Vector2.zero, Vector2.one, new Vector2(18f, 10f), new Vector2(-18f, -10f), string.Empty, 18, TextAnchor.UpperLeft, new Color(0.92f, 0.94f, 0.96f));

        RectTransform handRoot = CreateRect("Hand", canvasObject.transform, new Vector2(0.03f, 0.04f), new Vector2(0.78f, 0.25f), Vector2.zero, Vector2.zero);
        BuildCardViews(handRoot);

        endTurnButton = CreateButton("End Turn Button", canvasObject.transform, new Vector2(0.82f, 0.08f), new Vector2(0.95f, 0.22f), Vector2.zero, Vector2.zero, BattleText.EndTurn, 26, new Color(0.78f, 0.56f, 0.16f));
        endTurnButton.onClick.AddListener(EndPlayerTurn);
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
