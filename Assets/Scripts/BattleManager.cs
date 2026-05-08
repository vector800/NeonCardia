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

    private readonly List<CardView> cardViews = new List<CardView>();
    private readonly List<Image> positionSlots = new List<Image>();

    private Font uiFont;
    private CharacterUnit player;
    private CharacterUnit enemy;
    private DeckRuntime deck;
    private BattleLog battleLog;
    private EnemyAI enemyAI;
    private BattlePosition playerPosition;
    private bool battleEnded;

    private Text playerHpText;
    private Text enemyHpText;
    private Text positionText;
    private Text statusText;
    private Text logText;
    private Button endTurnButton;

    private void Awake()
    {
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiFont == null)
        {
            uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        EnsureCamera();
        EnsureEventSystem();
        BuildUi();
        StartBattle();
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
        player = new CharacterUnit("Player", 32);
        enemy = new CharacterUnit("Enemy Drone", 36);
        playerPosition = BattlePosition.Middle;
        battleEnded = false;

        battleLog = new BattleLog(10);
        enemyAI = new EnemyAI();
        deck = new DeckRuntime(CardData.CreateStarterDeck());
        deck.DrawUpTo(MaxHandSize);

        battleLog.Add("Battle start.");
        battleLog.Add("Player begins at Middle position.");
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
        if (!CanPlay(card.Data, out failureMessage))
        {
            battleLog.Add(failureMessage);
            RefreshUi();
            return;
        }

        deck.Hand.RemoveAt(handIndex);
        ResolveCard(card.Data);
        deck.Discard(card);

        CheckBattleEnd();
        RefreshUi();
    }

    private bool CanPlay(CardData card, out string failureMessage)
    {
        if (card.Effect == CardEffectType.FrontOnlyDamage && playerPosition != BattlePosition.Front)
        {
            failureMessage = "Heavy Shot can only be used from Front.";
            return false;
        }

        failureMessage = string.Empty;
        return true;
    }

    private void ResolveCard(CardData card)
    {
        switch (card.Effect)
        {
            case CardEffectType.Damage:
                DealDamageToEnemy(card.Name, card.Power);
                break;
            case CardEffectType.FrontOnlyDamage:
                DealDamageToEnemy(card.Name, card.Power);
                break;
            case CardEffectType.Guard:
                player.Guard += card.Power;
                battleLog.Add("Player uses Guard. Guard +" + card.Power + ".");
                break;
            case CardEffectType.MoveForward:
                MovePlayer(1);
                break;
            case CardEffectType.MoveBack:
                MovePlayer(-1);
                break;
            case CardEffectType.Repair:
                int before = player.Hp;
                player.Heal(card.Power);
                battleLog.Add("Player uses Repair. HP +" + (player.Hp - before) + ".");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DealDamageToEnemy(string sourceName, int damage)
    {
        int blocked;
        int actualDamage = enemy.TakeDamage(damage, out blocked);
        if (blocked > 0)
        {
            battleLog.Add("Player uses " + sourceName + ". Enemy blocks " + blocked + " and takes " + actualDamage + ".");
        }
        else
        {
            battleLog.Add("Player uses " + sourceName + ". Enemy takes " + actualDamage + ".");
        }
    }

    private void MovePlayer(int direction)
    {
        BattlePosition previous = playerPosition;
        int nextValue = Mathf.Clamp((int)playerPosition + direction, (int)BattlePosition.Back, (int)BattlePosition.Front);
        playerPosition = (BattlePosition)nextValue;

        if (previous == playerPosition)
        {
            battleLog.Add("Player is already at " + CharacterUnit.FormatPosition(playerPosition) + ".");
        }
        else
        {
            battleLog.Add("Player moves to " + CharacterUnit.FormatPosition(playerPosition) + ".");
        }
    }

    private void EndPlayerTurn()
    {
        if (battleEnded)
        {
            return;
        }

        battleLog.Add("Player ends turn.");
        enemyAI.Act(player, enemy, playerPosition, battleLog);
        CheckBattleEnd();

        if (!battleEnded)
        {
            deck.DrawUpTo(MaxHandSize);
            battleLog.Add("Player draws to " + deck.Hand.Count + " cards.");
        }

        RefreshUi();
    }

    private void CheckBattleEnd()
    {
        if (enemy.IsDefeated)
        {
            battleEnded = true;
            battleLog.Add("Victory! Enemy defeated.");
        }
        else if (player.IsDefeated)
        {
            battleEnded = true;
            battleLog.Add("Defeat. Player was defeated.");
        }
    }

    private void RefreshUi()
    {
        playerHpText.text = "PLAYER\nHP " + player.Hp + "/" + player.MaxHp + "\nGuard " + player.Guard;
        enemyHpText.text = "ENEMY\nHP " + enemy.Hp + "/" + enemy.MaxHp + "\nGuard " + enemy.Guard;
        positionText.text = "Position: " + CharacterUnit.FormatPosition(playerPosition);
        statusText.text = battleEnded ? (enemy.IsDefeated ? "VICTORY" : "DEFEAT") : "PLAYER TURN";
        logText.text = battleLog.DisplayText;
        endTurnButton.interactable = !battleEnded;

        for (int i = 0; i < positionSlots.Count; i++)
        {
            bool active = i == (int)playerPosition;
            positionSlots[i].color = active ? new Color(0.12f, 0.64f, 0.74f, 0.95f) : new Color(0.13f, 0.15f, 0.19f, 0.95f);
        }

        for (int i = 0; i < cardViews.Count; i++)
        {
            CardData card = i < deck.Hand.Count ? deck.Hand[i].Data : null;
            bool playable = false;
            if (card != null)
            {
                string failureMessage;
                playable = CanPlay(card, out failureMessage);
            }

            cardViews[i].Refresh(card, playable, battleEnded);
        }
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

        CreateText("Title", canvasObject.transform, new Vector2(0.03f, 0.9f), new Vector2(0.97f, 0.98f), Vector2.zero, Vector2.zero, "NEON CARDIA - BATTLE MVP", 34, TextAnchor.MiddleCenter, Color.white);

        playerHpText = CreateText("Player HP", canvasObject.transform, new Vector2(0.05f, 0.68f), new Vector2(0.27f, 0.86f), Vector2.zero, Vector2.zero, string.Empty, 28, TextAnchor.MiddleCenter, Color.white);
        enemyHpText = CreateText("Enemy HP", canvasObject.transform, new Vector2(0.73f, 0.68f), new Vector2(0.95f, 0.86f), Vector2.zero, Vector2.zero, string.Empty, 28, TextAnchor.MiddleCenter, Color.white);

        CreateImage("Player Body", canvasObject.transform, new Vector2(0.1f, 0.46f), new Vector2(0.2f, 0.64f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.54f, 0.72f));
        CreateImage("Enemy Body", canvasObject.transform, new Vector2(0.8f, 0.46f), new Vector2(0.9f, 0.64f), Vector2.zero, Vector2.zero, new Color(0.8f, 0.22f, 0.26f));

        positionText = CreateText("Position Text", canvasObject.transform, new Vector2(0.33f, 0.72f), new Vector2(0.67f, 0.8f), Vector2.zero, Vector2.zero, string.Empty, 30, TextAnchor.MiddleCenter, Color.white);
        statusText = CreateText("Status Text", canvasObject.transform, new Vector2(0.33f, 0.82f), new Vector2(0.67f, 0.89f), Vector2.zero, Vector2.zero, string.Empty, 26, TextAnchor.MiddleCenter, new Color(0.95f, 0.95f, 0.72f));

        BuildPositionSlots(canvasObject.transform);

        Image logPanel = CreateImage("Log Panel", canvasObject.transform, new Vector2(0.56f, 0.33f), new Vector2(0.95f, 0.62f), Vector2.zero, Vector2.zero, new Color(0.09f, 0.1f, 0.13f, 0.92f));
        logPanel.raycastTarget = false;
        logText = CreateText("Log Text", logPanel.transform, Vector2.zero, Vector2.one, new Vector2(18f, 12f), new Vector2(-18f, -12f), string.Empty, 22, TextAnchor.UpperLeft, new Color(0.92f, 0.94f, 0.96f));

        RectTransform handRoot = CreateRect("Hand", canvasObject.transform, new Vector2(0.03f, 0.04f), new Vector2(0.78f, 0.28f), Vector2.zero, Vector2.zero);
        BuildCardViews(handRoot);

        endTurnButton = CreateButton("End Turn Button", canvasObject.transform, new Vector2(0.82f, 0.08f), new Vector2(0.95f, 0.22f), Vector2.zero, Vector2.zero, "END TURN", 26, new Color(0.78f, 0.56f, 0.16f));
        endTurnButton.onClick.AddListener(EndPlayerTurn);
    }

    private void BuildPositionSlots(Transform parent)
    {
        string[] names = { "BACK", "MIDDLE", "FRONT" };
        for (int i = 0; i < names.Length; i++)
        {
            float xMin = 0.32f + i * 0.12f;
            float xMax = xMin + 0.1f;
            Image slot = CreateImage("Position " + names[i], parent, new Vector2(xMin, 0.48f), new Vector2(xMax, 0.63f), Vector2.zero, Vector2.zero, new Color(0.13f, 0.15f, 0.19f, 0.95f));
            positionSlots.Add(slot);
            CreateText(names[i] + " Label", slot.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, names[i], 20, TextAnchor.MiddleCenter, Color.white);
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
            cardView.Initialize(button, i, PlayCard);
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
