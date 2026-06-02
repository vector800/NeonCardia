using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleTimelinePrototypeController : MonoBehaviour
{
    private const int HandSize = 5;
    private const int MaxQueuedActions = 3;
    private const int TimelinePreviewCount = 8;
    private const float PrefabTimelineMoveSeconds = 0.22f;
    private const float PrefabTimelineFadeShift = 0.020f;
    private const int BattleScenePrototypeAttackCount = 3;
    private const int BattleScenePrototypeAttackRangeStartColumn = 0;
    private const int WeaponPower = 10;
    private const int EchoShotPower = 20;
    private const int EchoSkillPower = 10;
    private const int EchoSkillInsertDelay = 45;
    private const float FrontOutgoingDamageMultiplier = 1.2f;
    private const float FrontIncomingDamageMultiplier = 1.2f;
    private const float MiddleModifierMultiplier = 1f;
    private const float BackIncomingDamageMultiplier = 0.8f;
    private const float BackHealingReceivedMultiplier = 1.2f;
    private const int PushDistance = 1;
    private const int DelayTicks = 24;
    private const string BattleBackgroundAssetPath = "Assets/Art/Backgrounds/Battle/CyberBattleBackground.png";
    private const string BattleUiFrameAssetPath = "Assets/Art/UI/Battle/BattleUIFrame.png";
    private const string CardFrameAssetPath = "Assets/Art/Cards/Frames/CardFrame_Base.png";
    private const string TimelineIconsAssetPath = "Assets/Art/UI/Battle/Timeline/TimelineIconSheet.png";
    private static readonly string[] TimelineFaceIconAssetPaths =
    {
        "Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_01.png",
        "Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_02.png",
        "Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_03.png",
        "Assets/Art/UI/Battle/Timeline/FaceIcons/EnemyFaceIcon_01.png",
        "Assets/Art/UI/Battle/Timeline/FaceIcons/EnemyFaceIcon_02.png",
        "Assets/Art/UI/Battle/Timeline/FaceIcons/EnemyFaceIcon_03.png"
    };
    private const string AllyPortraitFramesAssetPath = "Assets/Art/UI/Party/AllyPortraitFramesSheet.png";
    private const string EnemyGridPanelsAssetPath = "Assets/Art/UI/Grid/EnemyGridPanelsSheet.png";
    private const string EnemySpritesAssetPath = "Assets/Art/Enemies/Prototype/EnemySpritesSheet.png";
    private const string BattlePanelVisualSetAssetPath = "Assets/ScriptableObjects/Visual/BattlePanelVisualSet.asset";
    private const string BattleGridBottomPrefabName = "PF_BattleGrid_Full_3x6_Image2_Bottom";
    private const string BattleFieldFloorAssetPath = "Assets/Art/BattleField/NeonGrid/Textures/CyberFloor_Base.png";
    private const string BattleGridFullImage2AssetPath = "Assets/Art/BattleField/NeonGrid/Textures/BattleGrid_Full_Image2.png";
    private const string PanelPlayerNormalAssetPath = "Assets/Art/BattleField/NeonGrid/Sprites/Panel_RedOrange_Base.png";
    private const string PanelPlayerSelectedAssetPath = "Assets/Art/Runtime/Panels/Panel_Player_Selected.png";
    private const string PanelEnemyNormalAssetPath = "Assets/Art/BattleField/NeonGrid/Sprites/Panel_CyanBlue_Base.png";
    private const string PanelEnemySelectedAssetPath = "Assets/Art/Runtime/Panels/Panel_Enemy_Selected.png";
    private const string PanelTargetableOverlayAssetPath = "Assets/Art/Runtime/Panels/Perspective/Panel_Targetable_Perspective_Overlay.png";
    private const string BattleSceneAttackRangePanelAssetFolder = "Assets/Art/BattleField/NeonGrid/RangePanels";
    private const string BattleSceneAttackRangeAlignedPanelAssetFolder = "Assets/Art/BattleField/NeonGrid/RangePanelsAligned";
    private const string BattleSceneAttackRangeRowAssetFolder = "Assets/Art/BattleField/NeonGrid/RangeRows";
    private const string BattleSceneUnifiedPanelAssetFolder = "Assets/Art/BattleField/NeonGrid/UnifiedPanels";
    private const string BattleSceneUnifiedBoardFrameAssetPath = "Assets/Art/BattleField/NeonGrid/UnifiedPanels/UnifiedBoardFrame.png";
    private const string BattleSceneUnifiedTilePanelAllyAssetPath = "Assets/Art/BattleField/NeonGrid/UnifiedPanels/TilePanel_Ally.png";
    private const string BattleSceneUnifiedTilePanelEnemyAssetPath = "Assets/Art/BattleField/NeonGrid/UnifiedPanels/TilePanel_Enemy.png";
    private const string BattleSceneUnifiedPanelRootName = "UnifiedPanelVisuals";
    private const bool BattleSceneUseUnifiedPanelCells = true;
    private const float BattleSceneUnifiedTextureWidthPixels = 1672f;
    private const float BattleSceneUnifiedTextureHeightPixels = 941f;
    private const float BattleSceneUnifiedPixelsPerUnit = 100f;
    private const float BattleSceneUnifiedColliderTrim = 0.10f;
    private const float BattleSceneVisualPanelBaseWidthPixels = 205f;
    private const float BattleSceneVisualPanelBaseHeightPixels = 108f;
    private const int BattleSceneUnifiedPanelSortingOrder = -1;
    // RowSprites uses full-size transparent row masks for the highlighted row.
    // The masks share the BattleGrid_Full_Image2 coordinate space and are
    // clipped to each panel top surface, so side walls and board frame glow stay untouched.
    private static readonly BattleSceneAttackRangeOverlayMode BattleSceneAttackRangeMode = BattleSceneAttackRangeOverlayMode.RowSprites;
    private const float BattleSceneAttackRangeOverlayExpand = 0.085f;
    private const float BattleSceneAttackRangeOverlayZ = -0.024f;
    private const float BattleSceneAttackRangeOutlineWidth = 0.055f;
    private const float BattleSceneAttackRangeGlowWidth = 0.18f;
    private const int BattleSceneAttackRangeFillSortingOrder = 26;
    private const int BattleSceneAttackRangeGlowSortingOrder = 27;
    private const int BattleSceneAttackRangeOutlineSortingOrder = 28;
    private const string PanelDangerOverlayAssetPath = "Assets/Art/Runtime/Panels/Perspective/Panel_Danger_Perspective_Overlay.png";
    private const string PanelHoverAssetPath = "Assets/Art/Runtime/Panels/Perspective/Panel_Hover_Perspective_Overlay.png";
    private const string PanelDisabledAssetPath = "Assets/Art/Runtime/Panels/Perspective/Panel_Disabled_Perspective_Overlay.png";
    private const string PanelBreakHintAssetPath = "Assets/Art/Runtime/Panels/Panel_BreakHint.png";
    private const string PanelHealHintAssetPath = "Assets/Art/Runtime/Panels/Panel_HealHint.png";
    private const string AllyAIdleSheetAssetPath = "Assets/Art/Runtime/Characters/Allies/AllyA_Protagonist/AllyA_Protagonist_IdleSheet.png";
    private const string AllyAIdleTransparentSheetAssetPath = "Assets/Art/Runtime/Characters/Allies/AllyA_Protagonist/AllyA_Protagonist_IdleSheet_Transparent.png";
    private const string AllyBIdleSheetAssetPath = "Assets/Art/Runtime/Characters/Allies/AllyB_CyberWolf/AllyB_CyberWolf_IdleSheet.png";
    private const string AllyBIdleTransparentSheetAssetPath = "Assets/Art/Runtime/Characters/Allies/AllyB_CyberWolf/AllyB_CyberWolf_IdleSheet_Transparent.png";
    private const string AllyCIdleSheetAssetPath = "Assets/Art/Runtime/Characters/Allies/AllyC_CyberFairy/AllyC_CyberFairy_IdleSheet.png";
    private const string AllyCIdleTransparentSheetAssetPath = "Assets/Art/Runtime/Characters/Allies/AllyC_CyberFairy/AllyC_CyberFairy_IdleSheet_Transparent.png";
    private const string CyberKnightIdleSheetAssetPath = "Assets/Art/Characters/CyberKnight/CyberKnight_idle_1x4_128.png";
    private const string CyberWolfIdleSheetAssetPath = "Assets/Art/Enemies/CyberWolf/CyberWolf_idle_1x4_128.png";
    private const string DigitalFairyIdleSheetAssetPath = "Assets/Art/Characters/DigitalFairy/DigitalFairy_idle_1x4_128.png";
    private const int AllyIdleFrameCount = 4;
    private const string EnemyDrillMoleIdleSheetAssetPath = "Assets/Art/Enemies/DrillMole/Enemy_DrillMole_Idle_Sheet.png";
    private const string EnemyElecGeckoIdleSheetAssetPath = "Assets/Art/Enemies/ElecGecko/Enemy_ElecGecko_Idle_Sheet.png";
    private const string EnemyBladeBugIdleSheetAssetPath = "Assets/Art/Enemies/BladeBug/Enemy_BladeBug_Idle_Sheet.png";
    private const int EnemyIdleFrameCount = 6;
    private const int BattleGridRows = 3;
    private const int BattleGridAllyCols = 3;
    private const int BattleGridEnemyCols = 3;
    private const int BattleGridTotalCols = BattleGridAllyCols + BattleGridEnemyCols;
    private const float BattleGridTileSize = 216f;
    private const float BattleGridCenterYOffset = -132f;
    private const float BattleFieldTiltDegrees = 62f;
    private const float BattleSceneHudMinX = 0.012f;
    private const float BattleSceneHudMaxX = 0.865f;
    private const float BattleSceneHudMinY = 0.728f;
    private const float BattleSceneHudMaxY = 0.982f;
    private static readonly Vector2 BattleSceneCommandPanelMin = new Vector2(0.012f, 0.225f);
    private static readonly Vector2 BattleSceneCommandPanelMax = new Vector2(0.265f, 0.695f);
    private const float BattleSpriteScaleRowDelta = 0.13f;
    private const float BattleSpriteYOffsetRowDelta = -0.035f;
    private const string BattleSceneEnemyHpTextName = "HPText";
    private const string BattleSceneEnemyHpRootName = "EnemyHpUI";
    private const string BattleSceneEnemyHpObjectPrefix = "EnemyHp_";
    private const string BattleSceneEnemyHpAnchorName = "HpAnchor";
    private const float BattleSceneEnemyHpPanelYRatioFromBottom = 0.20716f;
    private static readonly Vector3 BattleSceneEnemyHpFallbackLocalPosition = new Vector3(0f, -0.38f, -0.08f);
    private static readonly Vector3 BattleSceneEnemyHpLocalScale = new Vector3(0.94f, 0.94f, 1f);
    private static readonly Vector2 BattleGridAnchor = new Vector2(0.5f, 0.5f);
    private static readonly Vector2 BattleSceneEnemyHpTextSize = new Vector2(92f, 38f);
    private static readonly Vector2 BattleSceneEnemyHpWorldTextSize = new Vector2(2.8f, 1.05f);
    private static readonly Vector2 BattleFieldSize = new Vector2(
        BattleGridTileSize * BattleGridTotalCols,
        BattleGridTileSize * BattleGridRows);
    private static readonly Vector2 BattleFieldOffsetMin = new Vector2(
        -BattleFieldSize.x * 0.5f,
        BattleGridCenterYOffset - BattleFieldSize.y * 0.5f);
    private static readonly Vector2 BattleFieldOffsetMax = BattleFieldOffsetMin + BattleFieldSize;
    private static readonly Vector2 BattleGridOrigin = new Vector2(
        -BattleGridTileSize * BattleGridTotalCols * 0.5f,
        -BattleGridTileSize * BattleGridRows * 0.5f);

    private static readonly Vector2[][] BattleSceneUnifiedPanelCornerPixels =
    {
        new[] { new Vector2(161.0f, 204.0f), new Vector2(366.0f, 204.0f), new Vector2(366.0f, 312.0f), new Vector2(161.0f, 312.0f) },
        new[] { new Vector2(390.0f, 204.0f), new Vector2(595.0f, 204.0f), new Vector2(595.0f, 312.0f), new Vector2(390.0f, 312.0f) },
        new[] { new Vector2(619.0f, 204.0f), new Vector2(824.0f, 204.0f), new Vector2(824.0f, 312.0f), new Vector2(619.0f, 312.0f) },
        new[] { new Vector2(848.0f, 204.0f), new Vector2(1053.0f, 204.0f), new Vector2(1053.0f, 312.0f), new Vector2(848.0f, 312.0f) },
        new[] { new Vector2(1077.0f, 204.0f), new Vector2(1282.0f, 204.0f), new Vector2(1282.0f, 312.0f), new Vector2(1077.0f, 312.0f) },
        new[] { new Vector2(1306.0f, 204.0f), new Vector2(1511.0f, 204.0f), new Vector2(1511.0f, 312.0f), new Vector2(1306.0f, 312.0f) },
        new[] { new Vector2(161.0f, 336.0f), new Vector2(366.0f, 336.0f), new Vector2(366.0f, 444.0f), new Vector2(161.0f, 444.0f) },
        new[] { new Vector2(390.0f, 336.0f), new Vector2(595.0f, 336.0f), new Vector2(595.0f, 444.0f), new Vector2(390.0f, 444.0f) },
        new[] { new Vector2(619.0f, 336.0f), new Vector2(824.0f, 336.0f), new Vector2(824.0f, 444.0f), new Vector2(619.0f, 444.0f) },
        new[] { new Vector2(848.0f, 336.0f), new Vector2(1053.0f, 336.0f), new Vector2(1053.0f, 444.0f), new Vector2(848.0f, 444.0f) },
        new[] { new Vector2(1077.0f, 336.0f), new Vector2(1282.0f, 336.0f), new Vector2(1282.0f, 444.0f), new Vector2(1077.0f, 444.0f) },
        new[] { new Vector2(1306.0f, 336.0f), new Vector2(1511.0f, 336.0f), new Vector2(1511.0f, 444.0f), new Vector2(1306.0f, 444.0f) },
        new[] { new Vector2(161.0f, 468.0f), new Vector2(366.0f, 468.0f), new Vector2(366.0f, 576.0f), new Vector2(161.0f, 576.0f) },
        new[] { new Vector2(390.0f, 468.0f), new Vector2(595.0f, 468.0f), new Vector2(595.0f, 576.0f), new Vector2(390.0f, 576.0f) },
        new[] { new Vector2(619.0f, 468.0f), new Vector2(824.0f, 468.0f), new Vector2(824.0f, 576.0f), new Vector2(619.0f, 576.0f) },
        new[] { new Vector2(848.0f, 468.0f), new Vector2(1053.0f, 468.0f), new Vector2(1053.0f, 576.0f), new Vector2(848.0f, 576.0f) },
        new[] { new Vector2(1077.0f, 468.0f), new Vector2(1282.0f, 468.0f), new Vector2(1282.0f, 576.0f), new Vector2(1077.0f, 576.0f) },
        new[] { new Vector2(1306.0f, 468.0f), new Vector2(1511.0f, 468.0f), new Vector2(1511.0f, 576.0f), new Vector2(1306.0f, 576.0f) }
    };

    private static readonly Vector2[][] BattleSceneVisualPanelCornerPixels =
    {
        new[] { new Vector2(161.0f, 204.0f), new Vector2(366.0f, 204.0f), new Vector2(366.0f, 312.0f), new Vector2(161.0f, 312.0f) },
        new[] { new Vector2(390.0f, 204.0f), new Vector2(595.0f, 204.0f), new Vector2(595.0f, 312.0f), new Vector2(390.0f, 312.0f) },
        new[] { new Vector2(619.0f, 204.0f), new Vector2(824.0f, 204.0f), new Vector2(824.0f, 312.0f), new Vector2(619.0f, 312.0f) },
        new[] { new Vector2(848.0f, 204.0f), new Vector2(1053.0f, 204.0f), new Vector2(1053.0f, 312.0f), new Vector2(848.0f, 312.0f) },
        new[] { new Vector2(1077.0f, 204.0f), new Vector2(1282.0f, 204.0f), new Vector2(1282.0f, 312.0f), new Vector2(1077.0f, 312.0f) },
        new[] { new Vector2(1306.0f, 204.0f), new Vector2(1511.0f, 204.0f), new Vector2(1511.0f, 312.0f), new Vector2(1306.0f, 312.0f) },
        new[] { new Vector2(161.0f, 336.0f), new Vector2(366.0f, 336.0f), new Vector2(366.0f, 444.0f), new Vector2(161.0f, 444.0f) },
        new[] { new Vector2(390.0f, 336.0f), new Vector2(595.0f, 336.0f), new Vector2(595.0f, 444.0f), new Vector2(390.0f, 444.0f) },
        new[] { new Vector2(619.0f, 336.0f), new Vector2(824.0f, 336.0f), new Vector2(824.0f, 444.0f), new Vector2(619.0f, 444.0f) },
        new[] { new Vector2(848.0f, 336.0f), new Vector2(1053.0f, 336.0f), new Vector2(1053.0f, 444.0f), new Vector2(848.0f, 444.0f) },
        new[] { new Vector2(1077.0f, 336.0f), new Vector2(1282.0f, 336.0f), new Vector2(1282.0f, 444.0f), new Vector2(1077.0f, 444.0f) },
        new[] { new Vector2(1306.0f, 336.0f), new Vector2(1511.0f, 336.0f), new Vector2(1511.0f, 444.0f), new Vector2(1306.0f, 444.0f) },
        new[] { new Vector2(161.0f, 468.0f), new Vector2(366.0f, 468.0f), new Vector2(366.0f, 576.0f), new Vector2(161.0f, 576.0f) },
        new[] { new Vector2(390.0f, 468.0f), new Vector2(595.0f, 468.0f), new Vector2(595.0f, 576.0f), new Vector2(390.0f, 576.0f) },
        new[] { new Vector2(619.0f, 468.0f), new Vector2(824.0f, 468.0f), new Vector2(824.0f, 576.0f), new Vector2(619.0f, 576.0f) },
        new[] { new Vector2(848.0f, 468.0f), new Vector2(1053.0f, 468.0f), new Vector2(1053.0f, 576.0f), new Vector2(848.0f, 576.0f) },
        new[] { new Vector2(1077.0f, 468.0f), new Vector2(1282.0f, 468.0f), new Vector2(1282.0f, 576.0f), new Vector2(1077.0f, 576.0f) },
        new[] { new Vector2(1306.0f, 468.0f), new Vector2(1511.0f, 468.0f), new Vector2(1511.0f, 576.0f), new Vector2(1306.0f, 576.0f) }
    };

    private readonly List<AllyUnit> allies = new List<AllyUnit>();
    private readonly List<EnemyUnit> enemies = new List<EnemyUnit>();
    private readonly List<SkillTimelineAction> skillTimelineActions = new List<SkillTimelineAction>();
    private readonly List<PrototypeCard> drawPile = new List<PrototypeCard>();
    private readonly List<PrototypeCard> hand = new List<PrototypeCard>();
    private readonly List<PrototypeCard> discardPile = new List<PrototypeCard>();
    private readonly List<QueuedAction> queuedActions = new List<QueuedAction>();
    private readonly bool[] queuedHandSlots = new bool[HandSize];
    private readonly Dictionary<string, AllySpriteDefinition> allySpriteDefinitions = new Dictionary<string, AllySpriteDefinition>();
    private readonly Dictionary<string, EnemySpriteDefinition> enemySpriteDefinitions = new Dictionary<string, EnemySpriteDefinition>();
    private readonly Dictionary<string, SpriteRenderer> sceneBattleGridUnitRenderers = new Dictionary<string, SpriteRenderer>();
    private readonly Dictionary<string, TextMeshPro> battleSceneEnemyHpTexts = new Dictionary<string, TextMeshPro>();
    private readonly Dictionary<string, SceneUnitSpriteAnimation> sceneBattleGridUnitAnimations = new Dictionary<string, SceneUnitSpriteAnimation>();
    private readonly Dictionary<PartyPosition, bool> allyPanelHoverStates = new Dictionary<PartyPosition, bool>();
    private readonly bool[,] enemyPanelHoverStates = new bool[3, 3];
    private readonly List<GameObject> debugGridLines = new List<GameObject>();

    private readonly List<TimelineSlotView> timelineViews = new List<TimelineSlotView>();
    private readonly List<PrefabTimelineDisplayState> prefabTimelineSlotStates = new List<PrefabTimelineDisplayState>();
    private readonly List<GameObject> prefabTimelineGhostObjects = new List<GameObject>();
    private readonly Dictionary<PartyPosition, AllyView> allyViews = new Dictionary<PartyPosition, AllyView>();
    private readonly EnemyCellView[,] enemyCellViews = new EnemyCellView[3, 3];
    private readonly Image[,] battleSceneAttackRangeCells = new Image[BattleGridRows, BattleGridTotalCols];
    private readonly SpriteRenderer[,] battleSceneAttackRangeSceneSpriteRenderers = new SpriteRenderer[BattleGridRows, BattleGridTotalCols];
    private readonly SpriteRenderer[] battleSceneAttackRangeSceneRowRenderers = new SpriteRenderer[BattleGridRows];
    private readonly GameObject[,] battleSceneAttackRangeColliderOverlayRoots = new GameObject[BattleGridRows, BattleGridTotalCols];
    private readonly List<CardButtonView> handViews = new List<CardButtonView>();
    private readonly List<Text> chipQueueSlotTexts = new List<Text>();
    private readonly List<PrototypeAttackButtonView> prototypeAttackViews = new List<PrototypeAttackButtonView>();

    private Font uiFont;
    private Text turnText;
    private Text statusText;
    private Text queueText;
    private Text deckText;
    private Text selectedText;
    private Text selectedCommandNameText;
    private Image selectedCommandActorIcon;
    private Text purposeText;
    private Text timelineHintText;
    private Text allyHintText;
    private Text enemyHintText;
    private Text timelineLabelText;
    private Text allyLabelText;
    private Text enemyLabelText;
    private Text handLabelText;
    private Text playerHpText;
    private Text currentHpValueText;
    private RectTransform battleSceneTimelineRoot;
    private RectTransform battleSceneEnemyHpOverlayRoot;
    private RectTransform battleSceneAttackRangeOverlayRoot;
    private RectTransform selectedCommandNameRoot;
    private BattleTimelineHudView battleTimelineHudView;
    private string prefabTimelineSignature = string.Empty;
    private Coroutine prefabTimelineAnimationRoutine;
    private Material battleSceneAttackRangeFillMaterial;
    private Material battleSceneAttackRangeGlowMaterial;
    private Material battleSceneAttackRangeOutlineMaterial;
    private Button weaponButton;
    private Button confirmButton;
    private Button resetButton;
    private Button debugButton;
    private Button swapFrontMiddleButton;
    private Button swapMiddleBackButton;
    private GameObject battleSceneCommandRoot;
    private Text battleSceneCommandActorText;
    private Text battleSceneCommandTargetText;
    private Text battleSceneCommandSelectedText;
    private Button battleSceneCommandOkButton;
    private Image battleSceneCommandOkImage;
    private GameObject cardSelectRoot;
    private Text chipDetailNameText;
    private Text chipDetailPowerText;
    private Text chipDetailMetaText;
    private Text chipSelectTitleText;
    private Image chipDetailArtwork;
    private Image chipDetailAttributeIcon;
    private Image chipDetailRankBox;
    private TimelineBattleResultOverlay resultOverlay;
    private BattleResultOverlay battleSceneResultOverlay;
    private Sprite battleBackgroundSprite;
    private Sprite battleUiFrameSprite;
    private Sprite cardFrameSprite;
    private TimelineSpriteSet timelineSprites;
    private Sprite[] timelineFaceIconSprites = new Sprite[0];
    private AllyUiSpriteSet allySprites;
    private EnemyGridSpriteSet enemyGridSprites;
    private EnemySpriteSet enemySprites;
    private BattlePanelSpriteSet battlePanelSprites;
    private Sprite battleFieldFloorSprite;
    private Sprite battleGridFullImage2Sprite;
    [SerializeField] private bool showDebugLabels;
    [SerializeField] private bool mainBattleSceneMode;
    [SerializeField] private bool usePrefabActionOrderHud;
    [SerializeField] private BattleTimelineHudView battleTimelineHudPrefab;
    [SerializeField] private BattlePanelVisualSet battlePanelVisualSet;
    [SerializeField] private float allyIdleFrameSeconds = 0.20f;
    [SerializeField] private float enemyIdleFrameSeconds = 0.14f;
    [SerializeField] private Vector2 allyAScale = new Vector2(0.78f, 0.82f);
    [SerializeField] private Vector2 allyBScale = new Vector2(0.92f, 0.68f);
    [SerializeField] private Vector2 allyCScale = new Vector2(0.74f, 0.78f);
    [SerializeField] private Vector2 allyAOffset = new Vector2(0.00f, 0.02f);
    [SerializeField] private Vector2 allyBOffset = new Vector2(0.00f, 0.02f);
    [SerializeField] private Vector2 allyCOffset = new Vector2(0.00f, 0.03f);

    private bool useSceneBattleGridPrefabVisuals;
    private bool initialized;
    private int currentTick;
    private int activeUnitSequence;
    private int playerActionTurnCount;
    private int playerDamageTakenCount;
    private int maxSimultaneousDefeatCount;
    private int allyIdleFrameIndex;
    private int enemyIdleFrameIndex;
    private float allyIdleTimer;
    private float enemyIdleTimer;
    private AllyUnit selectedAlly;
    private EnemyUnit selectedEnemy;
    private TimelineUnit activeUnit;
    private System.Random random;
    private bool loadedSavedDeck;
    private bool battleEnded;
    private bool cardSelectOpen;
    private int selectedHandIndex;
    private int selectedPrototypeAttackIndex = 1;
    private int hoveredPrototypeAttackIndex = -1;

    private enum PartyPosition
    {
        Front,
        Middle,
        Back
    }

    private enum ActionKind
    {
        Card,
        Weapon,
        Swap
    }

    private enum PrototypeCardEffect
    {
        SingleDamage,
        RowDamage,
        PushDamage,
        DelayDamage,
        Heal,
        EchoShot,
        Unsupported
    }

    private enum PrototypeTargetKind
    {
        Enemy,
        Ally,
        None
    }

    private enum PrototypeAttackRangePattern
    {
        RowToEnemyEdge
    }

    private enum BattleSceneAttackRangeOverlayMode
    {
        AlignedPanelSprites,
        ColliderPolygons,
        RowSprites
    }

    private sealed class PrototypeCard
    {
        public string CardId;
        public string Name;
        public PrototypeCardEffect Effect;
        public PrototypeTargetKind TargetKind;
        public int Power;
        public bool IsClearCard;
        public CardDeckType DeckType;
        public CardAttribute Attribute;
        public int ActionDelay;
        public CardData SourceCard;
        public bool IsUnsupported;
        public string UnsupportedReason;
        public bool AddsEchoSkillEntry;
    }

    private sealed class AllyUnit
    {
        public string Name;
        public int Hp;
        public int MaxHp;
        public PartyPosition Position;
        public int Speed;
        public string Status;
        public int NextReadyTick;

        public bool IsAlive
        {
            get { return Hp > 0; }
        }
    }

    private sealed class EnemyUnit
    {
        public string Name;
        public int Hp;
        public int MaxHp;
        public CardAttribute Attribute;
        public CardAttribute Weakness;
        public Vector2Int GridPosition;
        public string SpriteKey;
        public string Status;
        public int Speed;
        public int NextReadyTick;
        public bool IsBoss;

        public bool IsAlive
        {
            get { return Hp > 0; }
        }
    }

    private sealed class SkillTimelineAction
    {
        public string Id;
        public string DisplayName;
        public AllyUnit Owner;
        public EnemyUnit Target;
        public int Power;
        public int NextReadyTick;
        public int Delay;
        public bool IsAlive = true;
        public Color DisplayColor;
        public string Status;
    }

    private sealed class TimelineUnit
    {
        public AllyUnit Ally;
        public EnemyUnit Enemy;
        public SkillTimelineAction SkillAction;
        public bool IsAlly;
        public bool IsSkill;
        public int ReadyTick;
        public int Sequence;
        public BattleTimelineEntry Entry;

        public string DisplayName
        {
            get { return IsSkill ? SkillAction != null ? SkillAction.DisplayName : "Skill" : IsAlly ? Ally.Name : Enemy.Name; }
        }

        public int Speed
        {
            get { return IsSkill ? 0 : IsAlly ? Ally.Speed : Enemy.Speed; }
        }
    }

    private sealed class QueuedAction
    {
        public ActionKind Kind;
        public PrototypeCard Card;
        public int HandIndex = -1;
        public AllyUnit Actor;
        public AllyUnit AllyTarget;
        public EnemyUnit EnemyTarget;
        public PartyPosition SwapA;
        public PartyPosition SwapB;
        public bool ConsumesAction;
        public string Label;
    }

    private sealed class PrototypeAttackDefinition
    {
        public string Name;
        public int Damage;
        public int Delay;
        public CardAttribute Attribute;
        public PrototypeAttackRangePattern RangePattern;
    }

    private sealed class PrototypeAttackButtonView
    {
        public Button Button;
        public Image Panel;
        public Text Label;
    }

    private sealed class PrototypeDamageRequest
    {
        public AllyUnit Attacker;
        public EnemyUnit Target;
        public PrototypeAttackDefinition Attack;
    }

    private sealed class PrototypeDamageResult
    {
        public int BaseDamage;
        public int FinalDamage;
        public CardAttribute Attribute;
        public float Multiplier;
        public bool WeaknessHit;
        public string Reason;
    }

    private sealed class TimelineSlotView
    {
        public Image Panel;
        public Image Glow;
        public Image InnerPanel;
        public Image IconPlate;
        public Image NumberPlate;
        public Image TopEdge;
        public Image BottomEdge;
        public Image LeftEdge;
        public Image RightEdge;
        public Image TopCut;
        public Image BottomCut;
        public Image Accent;
        public Image ProgressSegment;
        public Image UnitIcon;
        public Image Cursor;
        public Text ProgressDot;
        public Text CurrentMarker;
        public Text NameText;
        public Text DetailText;
    }

    private sealed class AllyView
    {
        public Image Panel;
        public Image Accent;
        public Image Portrait;
        public Image SelectedHighlight;
        public Image ActiveHighlight;
        public Image TargetableOverlay;
        public Image DangerOverlay;
        public Image HoverOverlay;
        public Image DisabledOverlay;
        public RectTransform UnitAnchor;
        public Image HpBack;
        public Image HpFill;
        public RectTransform HpFillRect;
        public Text PositionText;
        public Text NameText;
        public Text DetailText;
        public Button Button;
    }

    private sealed class AllySpriteDefinition
    {
        public string AllyName;
        public string CharacterLabel;
        public Sprite[] IdleFrames = new Sprite[0];
        public Vector2 Scale = Vector2.one;
        public Vector2 Offset;
        public Color FallbackColor = Color.white;
    }

    private sealed class EnemySpriteDefinition
    {
        public string EnemyKey;
        public string CharacterLabel;
        public Sprite[] IdleFrames = new Sprite[0];
        public Vector2 Scale = Vector2.one;
        public Vector2 Offset;
        public bool FlipX;
        public Color FallbackColor = Color.white;
    }

    private sealed class SceneUnitSpriteAnimation
    {
        public Sprite[] IdleFrames = new Sprite[0];
        public bool FlipX;
        public bool IsEnemy;
    }

    private sealed class EnemyCellView
    {
        public Image Panel;
        public Image Highlight;
        public Image TargetableOverlay;
        public Image DangerOverlay;
        public Image HoverOverlay;
        public Image DisabledOverlay;
        public RectTransform UnitAnchor;
        public GameObject EnemyRoot;
        public Image EnemySprite;
        public Image HpBack;
        public Image HpFill;
        public RectTransform HpFillRect;
        public Text NameText;
        public Text Label;
        public Button Button;
    }

    private sealed class TimelineSpriteSet
    {
        public Sprite AllyNormal;
        public Sprite AllySelected;
        public Sprite AllyActive;
        public Sprite AllyDone;
        public Sprite EnemyNormal;
        public Sprite EnemySelected;
        public Sprite EnemyActive;
        public Sprite EnemyDone;
        public Sprite TargetHighlight;
        public Sprite StatusBadge;
        public Sprite SlotBase;
        public Sprite Cursor;
    }

    private sealed class AllyUiSpriteSet
    {
        public Sprite FrontFrame;
        public Sprite MiddleFrame;
        public Sprite BackFrame;
        public Sprite SelectedFrame;
        public Sprite ActiveFrame;
        public Sprite HpBarBack;
        public Sprite HpBarFill;
        public Sprite SmallIconFrame;
    }

    private sealed class EnemyGridSpriteSet
    {
        public Sprite Normal;
        public Sprite Empty;
        public Sprite Selected;
        public Sprite Targetable;
        public Sprite Danger;
        public Sprite Cracked;
        public Sprite Hole;
        public Sprite Ice;
        public Sprite Grass;
        public Sprite Magma;
        public Sprite Poison;
        public Sprite HighlightOverlay;
    }

    private sealed class EnemySpriteSet
    {
        public Sprite NormalEnemy;
        public Sprite FireEnemy;
        public Sprite IceEnemy;
    }

    private sealed class BattlePanelSpriteSet
    {
        public Sprite PlayerNormal;
        public Sprite PlayerSelected;
        public Sprite EnemyNormal;
        public Sprite EnemySelected;
        public Sprite TargetableOverlay;
        public Sprite DangerOverlay;
        public Sprite HoverOverlay;
        public Sprite DisabledOverlay;
        public Sprite BreakHintOverlay;
        public Sprite HealHintOverlay;
    }

    private sealed class CardButtonView
    {
        public Image Panel;
        public Image Artwork;
        public Image AttributeIcon;
        public Image RankBox;
        public Text NameText;
        public Text DetailText;
        public Text RankText;
        public Text PowerText;
        public Button Button;
    }

    private struct TimelinePreview
    {
        public TimelineUnit Unit;
        public int DeltaTick;
    }

    private struct PrefabTimelineSlotLayout
    {
        public Vector2 AnchorMin;
        public Vector2 AnchorMax;
    }

    private sealed class PrefabTimelineDisplayState
    {
        public string Key;
        public int LogicalIndex;
        public int DisplayIndex;
        public TimelinePreview Preview;
        public BattleTimelineSlotView Slot;
        public bool MatchedPrevious;
    }

    private sealed class PrefabTimelineSlotMotion
    {
        public BattleTimelineSlotView Slot;
        public PrefabTimelineSlotLayout StartLayout;
        public PrefabTimelineSlotLayout EndLayout;
        public float StartAlpha;
        public float EndAlpha;
    }

    private sealed class PrefabTimelineGhostMotion
    {
        public RectTransform Root;
        public CanvasGroup Group;
        public PrefabTimelineSlotLayout StartLayout;
        public PrefabTimelineSlotLayout EndLayout;
    }

    protected virtual void Awake()
    {
        InitializeController();
    }

    protected void InitializeController()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        mainBattleSceneMode = mainBattleSceneMode || SceneManager.GetActiveScene().name == "BattleScene";
        useSceneBattleGridPrefabVisuals = mainBattleSceneMode && FindBattleSceneGridRoot() != null;
        random = new System.Random(17);
        uiFont = CreateJapaneseFont();
        if (uiFont == null)
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        if (uiFont == null)
        {
            uiFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        if (mainBattleSceneMode)
        {
            Application.runInBackground = true;
        }

        if (mainBattleSceneMode)
        {
            ClearPreviewArtSprites();
            if (usePrefabActionOrderHud)
            {
                timelineSprites = LoadTimelineSprites();
            }
        }
        else
        {
            LoadPreviewArtSprites();
        }

        LoadTimelineFaceIconSprites();
        LoadAllyCharacterSprites();
        LoadEnemyCharacterSprites();
        LoadSceneBattleGridUnitAnimations();
        LoadBattlePanelSprites();
        ApplyBattleSceneUnifiedPanelGrid();
        EnsureCamera();
        EnsureEventSystem();
        InitializeBattle();
        BuildUi();
        CacheSceneBattleGridUnitRenderers();
        RefreshAll("Ready.");
    }

    protected void ConfigureActionOrderHud(bool usePrefabHud, BattleTimelineHudView prefab)
    {
        usePrefabActionOrderHud = usePrefabHud;
        if (prefab != null)
        {
            battleTimelineHudPrefab = prefab;
        }
    }

    protected virtual void Update()
    {
        UpdateAllyIdleAnimation();
        UpdateEnemyIdleAnimation();

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
        {
            Confirm();
        }
        else if (keyboard.rKey.wasPressedThisFrame)
        {
            ResetSelection();
        }
        else if (keyboard.digit1Key.wasPressedThisFrame)
        {
            if (mainBattleSceneMode && IsPlayerTurn())
            {
                SelectPrototypeAttack(0);
            }
            else
            {
                QueueCardFromHand(0);
            }
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            if (mainBattleSceneMode && IsPlayerTurn())
            {
                SelectPrototypeAttack(1);
            }
            else
            {
                QueueCardFromHand(1);
            }
        }
        else if (keyboard.digit3Key.wasPressedThisFrame)
        {
            if (mainBattleSceneMode && IsPlayerTurn())
            {
                SelectPrototypeAttack(2);
            }
            else
            {
                QueueCardFromHand(2);
            }
        }
        else if (keyboard.digit4Key.wasPressedThisFrame)
        {
            if (!mainBattleSceneMode)
            {
                QueueCardFromHand(3);
            }
        }
        else if (keyboard.digit5Key.wasPressedThisFrame)
        {
            if (!mainBattleSceneMode)
            {
                QueueCardFromHand(4);
            }
        }
    }

    protected virtual void LateUpdate()
    {
        UpdateSceneEnemyHpNumbers();
        RefreshCurrentHpPanel();
    }

    public void SetInitialDebugLabels(bool enabled)
    {
        showDebugLabels = enabled;
        if (statusText != null)
        {
            RefreshAll(showDebugLabels ? "Debug labels on." : "Ready.");
        }
    }

    private static Font CreateJapaneseFont()
    {
        string[] fontNames = { "Meiryo UI", "Yu Gothic UI", "Meiryo", "Yu Gothic", "Noto Sans CJK JP", "Arial" };
        return Font.CreateDynamicFontFromOSFont(fontNames, 24);
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

        if (mainBattleSceneMode)
        {
            camera.orthographic = false;
            camera.fieldOfView = 34f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.transform.rotation = Quaternion.identity;
        }
        else
        {
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.018f, 0.022f, 0.032f);
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

        InputSystemUIInputModule inputModuleOnObject = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputModuleOnObject == null)
        {
            BaseInputModule legacyModule = eventSystem.GetComponent<BaseInputModule>();
            if (legacyModule != null)
            {
                Destroy(legacyModule);
            }

            inputModuleOnObject = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }

        ConfigureInputModule(inputModuleOnObject);
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

    private void InitializeBattle()
    {
        allies.Clear();
        enemies.Clear();
        skillTimelineActions.Clear();
        drawPile.Clear();
        hand.Clear();
        discardPile.Clear();
        queuedActions.Clear();
        ClearQueuedHandSlots();

        currentTick = 0;
        activeUnitSequence = 0;
        playerActionTurnCount = 1;
        playerDamageTakenCount = 0;
        maxSimultaneousDefeatCount = 0;
        loadedSavedDeck = false;
        battleEnded = false;
        cardSelectOpen = false;
        selectedHandIndex = 0;
        selectedPrototypeAttackIndex = 1;
        hoveredPrototypeAttackIndex = -1;

        allies.Add(new AllyUnit { Name = "AllyFront", Hp = 150, MaxHp = 150, Position = PartyPosition.Front, Speed = 48, Status = "Normal", NextReadyTick = 0 });
        allies.Add(new AllyUnit { Name = "AllyMiddle", Hp = 125, MaxHp = 125, Position = PartyPosition.Middle, Speed = 58, Status = "Normal", NextReadyTick = 8 });
        allies.Add(new AllyUnit { Name = "AllyBack", Hp = 110, MaxHp = 110, Position = PartyPosition.Back, Speed = 68, Status = "Normal", NextReadyTick = 16 });
        EnsureUniquePartyPositions();

        enemies.Add(new EnemyUnit { Name = "Enemy1", Hp = 90, MaxHp = 90, Attribute = CardAttribute.Neutral, Weakness = CardAttribute.Shot, GridPosition = new Vector2Int(1, 1), SpriteKey = "DrillMole", Status = "Ready", Speed = 42, NextReadyTick = 12, IsBoss = false });
        enemies.Add(new EnemyUnit { Name = "Enemy2", Hp = 70, MaxHp = 70, Attribute = CardAttribute.Fire, Weakness = CardAttribute.Water, GridPosition = new Vector2Int(0, 2), SpriteKey = "ElecGecko", Status = "Ready", Speed = 36, NextReadyTick = 24, IsBoss = false });
        enemies.Add(new EnemyUnit { Name = "Enemy3", Hp = 75, MaxHp = 75, Attribute = CardAttribute.Ice, Weakness = CardAttribute.Fire, GridPosition = new Vector2Int(2, 2), SpriteKey = "BladeBug", Status = "Ready", Speed = 32, NextReadyTick = 32, IsBoss = false });

        selectedAlly = allies[0];
        selectedEnemy = enemies[0];

        BuildTimelineDeck();
        DrawToHand();
        activeUnit = GetCurrentActiveUnit();
    }

    private void BuildTimelineDeck()
    {
        List<CardData> savedDeck;
        if (DeckStorage.TryLoadDeck(out savedDeck))
        {
            DeckValidationResult validation = DeckValidator.Validate(savedDeck);
            if (validation.IsValid)
            {
                for (int i = 0; i < savedDeck.Count; i++)
                {
                    drawPile.Add(CreatePrototypeCard(TimelineCardActionAdapter.Resolve(savedDeck[i])));
                }

                drawPile.Add(CreateEchoShotCard());
                loadedSavedDeck = true;
                Shuffle(drawPile);
                Debug.Log((mainBattleSceneMode ? "BattleScene" : "BattleTimelinePrototypeScene") + " loaded saved deck: " + drawPile.Count + " cards.");
                return;
            }

            Debug.Log((mainBattleSceneMode ? "BattleScene" : "BattleTimelinePrototypeScene") + " saved deck is invalid, using fallback deck. First error: "
                + (validation.Errors.Count > 0 ? validation.Errors[0] : "unknown"));
        }
        else
        {
            Debug.Log((mainBattleSceneMode ? "BattleScene" : "BattleTimelinePrototypeScene") + " found no saved deck, using fallback deck.");
        }

        if (mainBattleSceneMode)
        {
            BuildStarterDeck();
        }
        else
        {
            BuildPrototypeDeck();
        }

        ExpandFallbackDeckToThirtyCards();
        Shuffle(drawPile);
    }

    private void BuildStarterDeck()
    {
        List<CardData> starterCards = CardData.CreateStarterDeck();
        for (int i = 0; i < starterCards.Count; i++)
        {
            drawPile.Add(CreatePrototypeCard(TimelineCardActionAdapter.Resolve(starterCards[i])));
        }

        drawPile.Add(CreateEchoShotCard());
    }

    private void ExpandFallbackDeckToThirtyCards()
    {
        int seedCount = drawPile.Count;
        if (seedCount == 0)
        {
            return;
        }

        while (drawPile.Count < DeckValidator.RequiredDeckCount)
        {
            drawPile.Add(drawPile[drawPile.Count % seedCount]);
        }
    }

    private void BuildPrototypeDeck()
    {
        drawPile.Add(CreateEchoShotCard());
        drawPile.Add(CreatePrototypeCard("AquaShot", "アクアショット", PrototypeCardEffect.SingleDamage, PrototypeTargetKind.Enemy, 40, CardAttribute.Water));
        drawPile.Add(CreatePrototypeCard("WideSlash", "ワイドスラッシュ", PrototypeCardEffect.RowDamage, PrototypeTargetKind.Enemy, 35, CardAttribute.Slash));
        drawPile.Add(CreatePrototypeCard("PushShot", "プッシュショット", PrototypeCardEffect.PushDamage, PrototypeTargetKind.Enemy, 20, CardAttribute.Shot));
        drawPile.Add(CreatePrototypeCard("DelayBullet", "ディレイバレット", PrototypeCardEffect.DelayDamage, PrototypeTargetKind.Enemy, 20, CardAttribute.Electric));
        drawPile.Add(CreatePrototypeCard("Repair", "リペア", PrototypeCardEffect.Heal, PrototypeTargetKind.Ally, 50, CardAttribute.Neutral, true));
        drawPile.Add(CreatePrototypeCard("AquaShot2", "アクアショット", PrototypeCardEffect.SingleDamage, PrototypeTargetKind.Enemy, 40, CardAttribute.Water));
        drawPile.Add(CreatePrototypeCard("WideSlash2", "ワイドスラッシュ", PrototypeCardEffect.RowDamage, PrototypeTargetKind.Enemy, 35, CardAttribute.Slash));
        drawPile.Add(CreatePrototypeCard("PushShot2", "プッシュショット", PrototypeCardEffect.PushDamage, PrototypeTargetKind.Enemy, 20, CardAttribute.Shot));
        drawPile.Add(CreatePrototypeCard("DelayBullet2", "ディレイバレット", PrototypeCardEffect.DelayDamage, PrototypeTargetKind.Enemy, 20, CardAttribute.Electric));
        drawPile.Add(CreatePrototypeCard("Repair2", "リペア", PrototypeCardEffect.Heal, PrototypeTargetKind.Ally, 50, CardAttribute.Neutral, true));
    }

    private PrototypeCard CreatePrototypeCard(TimelineCardAction action)
    {
        PrototypeCardEffect effect = PrototypeCardEffect.Unsupported;
        PrototypeTargetKind targetKind = PrototypeTargetKind.None;
        if (action != null)
        {
            effect = ConvertEffect(action.EffectKind);
            targetKind = ConvertTargetKind(action.TargetKind);
        }

        return new PrototypeCard
        {
            CardId = action != null ? action.CardId : "NullCard",
            Name = action != null ? action.DisplayName : "Null Card",
            Effect = effect,
            TargetKind = targetKind,
            Power = action != null ? action.Power : 0,
            IsClearCard = action != null && action.IsClearCard,
            DeckType = action != null ? action.DeckType : CardDeckType.N,
            Attribute = action != null ? action.Attribute : CardAttribute.Neutral,
            ActionDelay = action != null ? action.ActionDelay : BattleActionDelayResolver.Resolve(BattleActionDelayKind.Wait),
            SourceCard = action != null ? action.SourceCard : null,
            IsUnsupported = action != null && action.IsUnsupported,
            UnsupportedReason = action != null ? action.UnsupportedReason : "No timeline card action was provided.",
            AddsEchoSkillEntry = action != null && LooksLikeEchoCard(action.CardId, action.DisplayName)
        };
    }

    private PrototypeCard CreateEchoShotCard()
    {
        return new PrototypeCard
        {
            CardId = "EchoShot",
            Name = "Echo Shot",
            Effect = PrototypeCardEffect.EchoShot,
            TargetKind = PrototypeTargetKind.Enemy,
            Power = EchoShotPower,
            IsClearCard = false,
            DeckType = CardDeckType.N,
            Attribute = CardAttribute.Electric,
            ActionDelay = BattleActionDelayResolver.Resolve(BattleActionDelayKind.NormalCard),
            IsUnsupported = false,
            AddsEchoSkillEntry = true
        };
    }

    private static bool LooksLikeEchoCard(string cardId, string displayName)
    {
        return ContainsText(cardId, "Echo") || ContainsText(displayName, "Echo");
    }

    private static bool ContainsText(string value, string token)
    {
        return !string.IsNullOrEmpty(value) && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static PrototypeCardEffect ConvertEffect(TimelineCardEffectKind effectKind)
    {
        switch (effectKind)
        {
            case TimelineCardEffectKind.SingleDamage:
                return PrototypeCardEffect.SingleDamage;
            case TimelineCardEffectKind.RowDamage:
                return PrototypeCardEffect.RowDamage;
            case TimelineCardEffectKind.PushDamage:
                return PrototypeCardEffect.PushDamage;
            case TimelineCardEffectKind.DelayDamage:
                return PrototypeCardEffect.DelayDamage;
            case TimelineCardEffectKind.Heal:
                return PrototypeCardEffect.Heal;
            default:
                return PrototypeCardEffect.Unsupported;
        }
    }

    private static PrototypeTargetKind ConvertTargetKind(TimelineCardTargetKind targetKind)
    {
        switch (targetKind)
        {
            case TimelineCardTargetKind.Enemy:
                return PrototypeTargetKind.Enemy;
            case TimelineCardTargetKind.Ally:
                return PrototypeTargetKind.Ally;
            default:
                return PrototypeTargetKind.None;
        }
    }

    private PrototypeCard CreatePrototypeCard(string cardId, string cardName, PrototypeCardEffect effect, PrototypeTargetKind targetKind, int power, CardAttribute attribute, bool isClearCard = false)
    {
        return new PrototypeCard
        {
            CardId = cardId,
            Name = cardName,
            Effect = effect,
            TargetKind = targetKind,
            Power = power,
            IsClearCard = isClearCard,
            DeckType = CardDeckType.N,
            Attribute = attribute,
            ActionDelay = BattleActionDelayResolver.ResolveCardDelay(effect == PrototypeCardEffect.Heal ? TimelineCardEffectKind.Heal : TimelineCardEffectKind.SingleDamage, isClearCard, CardDeckType.N),
            IsUnsupported = false,
            AddsEchoSkillEntry = effect == PrototypeCardEffect.EchoShot || LooksLikeEchoCard(cardId, cardName)
        };
    }

    private void BuildUi()
    {
        GameObject canvasObject = new GameObject(mainBattleSceneMode ? "Battle Scene Timeline Canvas" : "Battle Timeline Prototype Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform root = CreateRect(mainBattleSceneMode ? "BattleSceneTimelineRoot" : "BattleTimelinePrototypeRoot", canvasObject.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        battleSceneTimelineRoot = root;
        battleSceneEnemyHpOverlayRoot = null;
        battleSceneAttackRangeOverlayRoot = null;
        ClearBattleSceneAttackRangeCells();

        BuildPreviewBackground(root);
        BuildBackgroundGrid(root);
        if (!mainBattleSceneMode)
        {
            BuildVisualFrame(root);
        }
        BuildHeader(root);
        BuildTimeline(root);
        BuildSelectedCommandName(root);
        BuildImage2BattleFieldArt(root);
        RectTransform battleFieldRoot = mainBattleSceneMode ? CreateBattleFieldRoot(root) : root;
        BuildAllies(battleFieldRoot);
        BuildEnemyGrid(battleFieldRoot);
        BuildBattleSceneAttackRangeOverlay(root);
        BuildHandAndCommands(root);
        if (!useSceneBattleGridPrefabVisuals)
        {
            EnsureBattleSceneEnemyHpOverlay();
        }
        BuildResultOverlay(root);
    }

    private void LoadPreviewArtSprites()
    {
        battleBackgroundSprite = LoadPreviewSprite(BattleBackgroundAssetPath);
        battleUiFrameSprite = LoadPreviewSprite(BattleUiFrameAssetPath);
        cardFrameSprite = LoadPreviewSprite(CardFrameAssetPath);
        timelineSprites = LoadTimelineSprites();
        allySprites = LoadAllyUiSprites();
        enemyGridSprites = LoadEnemyGridSprites();
        enemySprites = LoadEnemySprites();
    }

    private void ClearPreviewArtSprites()
    {
        battleBackgroundSprite = null;
        battleUiFrameSprite = null;
        cardFrameSprite = null;
        timelineSprites = null;
        allySprites = null;
        enemyGridSprites = null;
        enemySprites = null;
    }

    private void LoadTimelineFaceIconSprites()
    {
        timelineFaceIconSprites = new Sprite[TimelineFaceIconAssetPaths.Length];
        for (int i = 0; i < TimelineFaceIconAssetPaths.Length; i++)
        {
            timelineFaceIconSprites[i] = LoadOptionalSprite(TimelineFaceIconAssetPaths[i]);
        }
    }

    private void LoadAllyCharacterSprites()
    {
        allySpriteDefinitions.Clear();
        allyIdleFrameIndex = 0;
        allyIdleTimer = 0f;

        RegisterAllySpriteDefinition(
            "AllyFront",
            "Protagonist",
            allyAScale,
            allyAOffset,
            new Color(0.18f, 0.62f, 1f, 1f),
            new[] { AllyAIdleTransparentSheetAssetPath, AllyAIdleSheetAssetPath },
            new[]
            {
                "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_00.png",
                "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_01.png",
                "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_02.png",
                "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_03.png"
            },
            CyberKnightIdleSheetAssetPath);

        RegisterAllySpriteDefinition(
            "AllyMiddle",
            "Cyber Wolf",
            allyBScale,
            allyBOffset,
            new Color(0.26f, 1f, 0.45f, 1f),
            new[] { AllyBIdleTransparentSheetAssetPath, AllyBIdleSheetAssetPath },
            new[]
            {
                "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_00.png",
                "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_01.png",
                "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_02.png",
                "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_03.png"
            },
            CyberWolfIdleSheetAssetPath);

        RegisterAllySpriteDefinition(
            "AllyBack",
            "Cyber Fairy",
            allyCScale,
            allyCOffset,
            new Color(0.38f, 0.92f, 1f, 1f),
            new[] { AllyCIdleTransparentSheetAssetPath, AllyCIdleSheetAssetPath },
            new[]
            {
                "Assets/Art/Characters/DigitalFairy/Frames/DigitalFairy_idle_00.png",
                "Assets/Art/Characters/DigitalFairy/Frames/DigitalFairy_idle_01.png",
                "Assets/Art/Characters/DigitalFairy/Frames/DigitalFairy_idle_02.png",
                "Assets/Art/Characters/DigitalFairy/Frames/DigitalFairy_idle_03.png"
            },
            DigitalFairyIdleSheetAssetPath);
    }

    private void LoadEnemyCharacterSprites()
    {
        enemySpriteDefinitions.Clear();
        enemyIdleFrameIndex = 0;
        enemyIdleTimer = 0f;

        RegisterEnemySpriteDefinition(
            "DrillMole",
            "Drill Mole",
            EnemyDrillMoleIdleSheetAssetPath,
            new Vector2(0.92f, 0.78f),
            new Vector2(0.00f, 0.03f),
            true,
            new Color(1f, 0.70f, 0.26f, 1f));

        RegisterEnemySpriteDefinition(
            "ElecGecko",
            "Elec Gecko",
            EnemyElecGeckoIdleSheetAssetPath,
            new Vector2(0.94f, 0.72f),
            new Vector2(0.00f, 0.02f),
            false,
            new Color(0.34f, 1f, 0.86f, 1f));

        RegisterEnemySpriteDefinition(
            "BladeBug",
            "Blade Bug",
            EnemyBladeBugIdleSheetAssetPath,
            new Vector2(0.86f, 0.82f),
            new Vector2(0.00f, 0.03f),
            false,
            new Color(1f, 0.40f, 0.78f, 1f));
    }

    private void LoadSceneBattleGridUnitAnimations()
    {
        sceneBattleGridUnitAnimations.Clear();
        if (!mainBattleSceneMode)
        {
            return;
        }

        RegisterSceneBattleGridUnitAnimation("Ally_Front_CyberKnight", LoadAllyIdleFrames(new string[0], new[]
        {
            "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_00.png",
            "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_01.png",
            "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_02.png",
            "Assets/Art/Characters/CyberKnight/Frames/CyberKnight_idle_03.png"
        }, CyberKnightIdleSheetAssetPath), false, false);

        RegisterSceneBattleGridUnitAnimation("Ally_Middle_CyberWolf", LoadAllyIdleFrames(new string[0], new[]
        {
            "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_00.png",
            "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_01.png",
            "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_02.png",
            "Assets/Art/Enemies/CyberWolf/Frames/CyberWolf_idle_03.png"
        }, CyberWolfIdleSheetAssetPath), false, false);

        RegisterSceneBattleGridUnitAnimation("Ally_Back_DigitalFairy", LoadAllyIdleFrames(new string[0], new[]
        {
            "Assets/Art/Characters/DigitalFairy/Frames/DigitalFairy_idle_00.png",
            "Assets/Art/Characters/DigitalFairy/Frames/DigitalFairy_idle_01.png",
            "Assets/Art/Characters/DigitalFairy/Frames/DigitalFairy_idle_02.png",
            "Assets/Art/Characters/DigitalFairy/Frames/DigitalFairy_idle_03.png"
        }, DigitalFairyIdleSheetAssetPath), false, false);

        RegisterSceneBattleGridUnitAnimation("Enemy_DrillMole", LoadEnemyIdleFrames(EnemyDrillMoleIdleSheetAssetPath), true, true);
        RegisterSceneBattleGridUnitAnimation("Enemy_ElecGecko", LoadEnemyIdleFrames(EnemyElecGeckoIdleSheetAssetPath), false, true);
        RegisterSceneBattleGridUnitAnimation("Enemy_BladeBug", LoadEnemyIdleFrames(EnemyBladeBugIdleSheetAssetPath), false, true);
    }

    private void RegisterSceneBattleGridUnitAnimation(string unitName, Sprite[] idleFrames, bool flipX, bool isEnemy)
    {
        sceneBattleGridUnitAnimations[unitName] = new SceneUnitSpriteAnimation
        {
            IdleFrames = idleFrames != null ? idleFrames : new Sprite[0],
            FlipX = flipX,
            IsEnemy = isEnemy
        };
    }

    private void LoadBattlePanelSprites()
    {
        battleFieldFloorSprite = mainBattleSceneMode ? LoadOptionalSprite(BattleFieldFloorAssetPath) : null;
        battleGridFullImage2Sprite = mainBattleSceneMode ? LoadOptionalSprite(BattleGridFullImage2AssetPath) : null;
        BattlePanelVisualSet visualSet = battlePanelVisualSet;
#if UNITY_EDITOR
        if (visualSet == null)
        {
            visualSet = UnityEditor.AssetDatabase.LoadAssetAtPath<BattlePanelVisualSet>(BattlePanelVisualSetAssetPath);
        }
#endif

        battlePanelSprites = new BattlePanelSpriteSet
        {
            PlayerNormal = mainBattleSceneMode ? LoadOptionalSprite(PanelPlayerNormalAssetPath) : visualSet != null && visualSet.PlayerNormal != null ? visualSet.PlayerNormal : LoadOptionalSprite(PanelPlayerNormalAssetPath),
            PlayerSelected = visualSet != null && visualSet.PlayerSelected != null ? visualSet.PlayerSelected : LoadOptionalSprite(PanelPlayerSelectedAssetPath),
            EnemyNormal = mainBattleSceneMode ? LoadOptionalSprite(PanelEnemyNormalAssetPath) : visualSet != null && visualSet.EnemyNormal != null ? visualSet.EnemyNormal : LoadOptionalSprite(PanelEnemyNormalAssetPath),
            EnemySelected = visualSet != null && visualSet.EnemySelected != null ? visualSet.EnemySelected : LoadOptionalSprite(PanelEnemySelectedAssetPath),
            TargetableOverlay = visualSet != null && visualSet.TargetableOverlay != null ? visualSet.TargetableOverlay : LoadOptionalSprite(PanelTargetableOverlayAssetPath),
            DangerOverlay = visualSet != null && visualSet.DangerOverlay != null ? visualSet.DangerOverlay : LoadOptionalSprite(PanelDangerOverlayAssetPath),
            HoverOverlay = visualSet != null && visualSet.HoverOverlay != null ? visualSet.HoverOverlay : LoadOptionalSprite(PanelHoverAssetPath),
            DisabledOverlay = visualSet != null && visualSet.DisabledOverlay != null ? visualSet.DisabledOverlay : LoadOptionalSprite(PanelDisabledAssetPath),
            BreakHintOverlay = visualSet != null && visualSet.BreakHintOverlay != null ? visualSet.BreakHintOverlay : LoadOptionalSprite(PanelBreakHintAssetPath),
            HealHintOverlay = visualSet != null && visualSet.HealHintOverlay != null ? visualSet.HealHintOverlay : LoadOptionalSprite(PanelHealHintAssetPath)
        };
    }

    private void RegisterAllySpriteDefinition(string allyName, string characterLabel, Vector2 scale, Vector2 offset, Color fallbackColor, string[] preferredSheets, string[] fallbackFramePaths, string fallbackSheetPath)
    {
        AllySpriteDefinition definition = new AllySpriteDefinition
        {
            AllyName = allyName,
            CharacterLabel = characterLabel,
            Scale = ClampSpriteScale(scale),
            Offset = offset,
            FallbackColor = fallbackColor,
            IdleFrames = LoadAllyIdleFrames(preferredSheets, fallbackFramePaths, fallbackSheetPath)
        };

        if (definition.IdleFrames.Length == 0)
        {
            Debug.LogWarning("BattleScene ally sprite missing for " + characterLabel + ". A fallback color icon will be used.");
        }

        allySpriteDefinitions[allyName] = definition;
    }

    private void RegisterEnemySpriteDefinition(string enemyKey, string characterLabel, string sheetPath, Vector2 scale, Vector2 offset, bool flipX, Color fallbackColor)
    {
        EnemySpriteDefinition definition = new EnemySpriteDefinition
        {
            EnemyKey = enemyKey,
            CharacterLabel = characterLabel,
            Scale = ClampSpriteScale(scale),
            Offset = offset,
            FlipX = flipX,
            FallbackColor = fallbackColor,
            IdleFrames = LoadEnemyIdleFrames(sheetPath)
        };

        if (definition.IdleFrames.Length == 0)
        {
            Debug.LogWarning("BattleScene enemy sprite missing for " + characterLabel + ". A fallback color token will be used.");
        }

        enemySpriteDefinitions[enemyKey] = definition;
    }

    private static Vector2 ClampSpriteScale(Vector2 scale)
    {
        return new Vector2(Mathf.Clamp(scale.x, 0.18f, 0.99f), Mathf.Clamp(scale.y, 0.18f, 0.97f));
    }

    private static Sprite[] LoadAllyIdleFrames(string[] preferredSheets, string[] fallbackFramePaths, string fallbackSheetPath)
    {
        for (int i = 0; i < preferredSheets.Length; i++)
        {
            Sprite[] frames = LoadSpritesFromImportedSheet(preferredSheets[i], AllyIdleFrameCount);
            if (frames.Length >= AllyIdleFrameCount)
            {
                return TrimFrames(frames, AllyIdleFrameCount);
            }

            frames = SliceSpriteSheetFromFile(preferredSheets[i], 2, 2);
            if (frames.Length >= AllyIdleFrameCount)
            {
                return TrimFrames(frames, AllyIdleFrameCount);
            }
        }

        Sprite[] fallbackFrames = LoadIndividualSpriteFiles(fallbackFramePaths);
        if (fallbackFrames.Length >= AllyIdleFrameCount)
        {
            return TrimFrames(fallbackFrames, AllyIdleFrameCount);
        }

        Sprite[] importedFallback = LoadSpritesFromImportedSheet(fallbackSheetPath, AllyIdleFrameCount);
        if (importedFallback.Length >= AllyIdleFrameCount)
        {
            return TrimFrames(importedFallback, AllyIdleFrameCount);
        }

        Sprite[] slicedFallback = SliceSpriteSheetFromFile(fallbackSheetPath, 4, 1);
        if (slicedFallback.Length >= AllyIdleFrameCount)
        {
            return TrimFrames(slicedFallback, AllyIdleFrameCount);
        }

        return new Sprite[0];
    }

    private static Sprite[] LoadEnemyIdleFrames(string sheetPath)
    {
        Sprite[] importedFrames = LoadSpritesFromImportedSheet(sheetPath, EnemyIdleFrameCount);
        if (importedFrames.Length >= EnemyIdleFrameCount)
        {
            return TrimFrames(importedFrames, EnemyIdleFrameCount);
        }

        Sprite[] slicedFrames = SliceSpriteSheetFromFile(sheetPath, EnemyIdleFrameCount, 1);
        if (slicedFrames.Length >= EnemyIdleFrameCount)
        {
            return TrimFrames(slicedFrames, EnemyIdleFrameCount);
        }

        return new Sprite[0];
    }

    private static Sprite[] TrimFrames(Sprite[] frames, int count)
    {
        if (frames.Length <= count)
        {
            return frames;
        }

        Sprite[] trimmed = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            trimmed[i] = frames[i];
        }

        return trimmed;
    }

    private TimelineSpriteSet LoadTimelineSprites()
    {
        return new TimelineSpriteSet
        {
            AllyNormal = LoadNamedPreviewSprite(TimelineIconsAssetPath, "Timeline_Ally_Normal", "Ally_Normal"),
            AllySelected = LoadNamedPreviewSprite(TimelineIconsAssetPath, "Timeline_Ally_Selected", "Ally_Selected"),
            AllyActive = LoadNamedPreviewSprite(TimelineIconsAssetPath, "Timeline_Ally_Active", "Ally_Active"),
            AllyDone = LoadNamedPreviewSprite(TimelineIconsAssetPath, "Timeline_Ally_Dim", "Ally_Done"),
            EnemyNormal = LoadNamedPreviewSprite(TimelineIconsAssetPath, "Timeline_Enemy_Normal", "Enemy_Normal"),
            EnemySelected = LoadNamedPreviewSprite(TimelineIconsAssetPath, "Timeline_Enemy_Selected", "Enemy_Selected"),
            EnemyActive = LoadNamedPreviewSprite(TimelineIconsAssetPath, "Timeline_Enemy_Active", "Enemy_Active"),
            EnemyDone = LoadNamedPreviewSprite(TimelineIconsAssetPath, "Timeline_Enemy_Dim", "Enemy_Done"),
            TargetHighlight = LoadNamedPreviewSprite(TimelineIconsAssetPath, "Timeline_Target_Highlight", "Target_Highlight"),
            StatusBadge = LoadNamedPreviewSprite(TimelineIconsAssetPath, "Timeline_Status_Badge", "Status_Badge"),
            SlotBase = LoadNamedPreviewSprite(TimelineIconsAssetPath, "Timeline_Slot_Base", "Timeline_SlotBase"),
            Cursor = LoadNamedPreviewSprite(TimelineIconsAssetPath, "Timeline_Current_Cursor", "Timeline_Cursor")
        };
    }

    private AllyUiSpriteSet LoadAllyUiSprites()
    {
        return new AllyUiSpriteSet
        {
            FrontFrame = LoadNamedPreviewSprite(AllyPortraitFramesAssetPath, "Party_Front_PortraitFrame", "Front_Frame"),
            MiddleFrame = LoadNamedPreviewSprite(AllyPortraitFramesAssetPath, "Party_Middle_PortraitFrame", "Middle_Frame"),
            BackFrame = LoadNamedPreviewSprite(AllyPortraitFramesAssetPath, "Party_Back_PortraitFrame", "Back_Frame"),
            SelectedFrame = LoadNamedPreviewSprite(AllyPortraitFramesAssetPath, "Party_Selected_Frame", "Selected_Frame"),
            ActiveFrame = LoadNamedPreviewSprite(AllyPortraitFramesAssetPath, "Party_Active_Frame", "Active_Frame"),
            HpBarBack = LoadNamedPreviewSprite(AllyPortraitFramesAssetPath, "Party_HpBar_Base", "HpBar_Back"),
            HpBarFill = LoadNamedPreviewSprite(AllyPortraitFramesAssetPath, "Party_HpBar_Fill", "HpBar_Fill"),
            SmallIconFrame = LoadNamedPreviewSprite(AllyPortraitFramesAssetPath, "Party_Status_Badge", "SmallIcon_Frame")
        };
    }

    private EnemyGridSpriteSet LoadEnemyGridSprites()
    {
        return new EnemyGridSpriteSet
        {
            Normal = LoadNamedPreviewSprite(EnemyGridPanelsAssetPath, "EnemyGrid_Normal", "Normal"),
            Empty = LoadNamedPreviewSprite(EnemyGridPanelsAssetPath, "EnemyGrid_Empty", "Empty"),
            Selected = LoadNamedPreviewSprite(EnemyGridPanelsAssetPath, "EnemyGrid_Selected", "Selected"),
            Targetable = LoadNamedPreviewSprite(EnemyGridPanelsAssetPath, "EnemyGrid_Targetable", "Targetable"),
            Danger = LoadNamedPreviewSprite(EnemyGridPanelsAssetPath, "EnemyGrid_Danger", "Danger"),
            Cracked = LoadNamedPreviewSprite(EnemyGridPanelsAssetPath, "EnemyGrid_Cracked", "Cracked"),
            Hole = LoadNamedPreviewSprite(EnemyGridPanelsAssetPath, "EnemyGrid_Hole", "Hole"),
            Ice = LoadNamedPreviewSprite(EnemyGridPanelsAssetPath, "EnemyGrid_Ice", "Ice"),
            Grass = LoadNamedPreviewSprite(EnemyGridPanelsAssetPath, "EnemyGrid_Grass", "Grass"),
            Magma = LoadNamedPreviewSprite(EnemyGridPanelsAssetPath, "EnemyGrid_Magma", "Magma"),
            Poison = LoadNamedPreviewSprite(EnemyGridPanelsAssetPath, "EnemyGrid_Poison", "Poison"),
            HighlightOverlay = LoadNamedPreviewSprite(EnemyGridPanelsAssetPath, "EnemyGrid_HighlightOverlay", "HighlightOverlay")
        };
    }

    private EnemySpriteSet LoadEnemySprites()
    {
        return new EnemySpriteSet
        {
            NormalEnemy = LoadNamedPreviewSprite(EnemySpritesAssetPath, "Enemy_Normal", "NormalEnemy"),
            FireEnemy = LoadNamedPreviewSprite(EnemySpritesAssetPath, "Enemy_Fire", "FireEnemy"),
            IceEnemy = LoadNamedPreviewSprite(EnemySpritesAssetPath, "Enemy_Ice", "IceEnemy")
        };
    }

    private void BuildPreviewBackground(Transform parent)
    {
        RectTransform backgroundRoot = CreateRect("BackgroundRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Image background = CreateImage(mainBattleSceneMode ? "FlatBattleBackground" : "CyberBattleBackground", backgroundRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.016f, 0.02f, 0.03f, 1f));
        background.raycastTarget = false;

        if (useSceneBattleGridPrefabVisuals)
        {
            background.color = Color.clear;
            return;
        }

        if (!mainBattleSceneMode && battleBackgroundSprite != null)
        {
            background.sprite = battleBackgroundSprite;
            background.type = Image.Type.Simple;
            background.preserveAspect = false;
            background.color = Color.white;
        }
        else if (mainBattleSceneMode && battleFieldFloorSprite != null)
        {
            background.sprite = battleFieldFloorSprite;
            background.type = Image.Type.Simple;
            background.preserveAspect = false;
            background.color = new Color(1f, 1f, 1f, 0.86f);
        }

        CreateImage("Background Readability Tint", backgroundRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, mainBattleSceneMode ? new Color(0.01f, 0.018f, 0.032f, 0.35f) : new Color(0f, 0.006f, 0.014f, 0.16f)).raycastTarget = false;
    }

    private void BuildImage2BattleFieldArt(Transform parent)
    {
        if (!mainBattleSceneMode || battleGridFullImage2Sprite == null || useSceneBattleGridPrefabVisuals)
        {
            return;
        }

        Image image = CreateImage("Image2 Battle Field Art", parent, BattleGridAnchor, BattleGridAnchor, new Vector2(-670f, -365f), new Vector2(670f, 318f), Color.white);
        image.sprite = battleGridFullImage2Sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private void BuildVisualFrame(Transform parent)
    {
        if (mainBattleSceneMode)
        {
            return;
        }

        RectTransform frameRoot = CreateRect("VisualFrameRoot", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        if (battleUiFrameSprite == null)
        {
            return;
        }

        Image frame = CreateImage("BattleUIFrame", frameRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.86f));
        frame.sprite = battleUiFrameSprite;
        frame.type = Image.Type.Simple;
        frame.preserveAspect = false;
        frame.raycastTarget = false;
    }

    private void BuildResultOverlay(Transform parent)
    {
        if (mainBattleSceneMode)
        {
            GameObject overlayObject = new GameObject("Battle Result Overlay");
            battleSceneResultOverlay = overlayObject.AddComponent<BattleResultOverlay>();
            battleSceneResultOverlay.Build(parent, uiFont, RetryBattle, ReturnToMenu, ReturnToDeckBuild);
            return;
        }

        GameObject timelineOverlayObject = new GameObject("Timeline Battle Result Overlay");
        resultOverlay = timelineOverlayObject.AddComponent<TimelineBattleResultOverlay>();
        resultOverlay.Build(parent, uiFont, RetryBattle, ReturnToMenu, ReturnToDeckBuild);
    }

    private void BuildBackgroundGrid(Transform parent)
    {
        Color verticalColor = new Color(0.10f, 0.72f, 0.86f, 0.075f);
        Color horizontalColor = new Color(0.86f, 0.95f, 0.24f, 0.055f);
        bool visible = !mainBattleSceneMode || showDebugLabels;
        debugGridLines.Clear();
        for (int i = 0; i <= 24; i++)
        {
            float x = i / 24f;
            Image line = CreateImage("Grid Vertical " + i, parent, new Vector2(x, 0f), new Vector2(x, 1f), new Vector2(-1f, 0f), new Vector2(1f, 0f), verticalColor);
            line.raycastTarget = false;
            line.gameObject.SetActive(visible);
            if (mainBattleSceneMode)
            {
                debugGridLines.Add(line.gameObject);
            }
        }

        for (int i = 0; i <= 12; i++)
        {
            float y = i / 12f;
            Image line = CreateImage("Grid Horizontal " + i, parent, new Vector2(0f, y), new Vector2(1f, y), new Vector2(0f, -1f), new Vector2(0f, 1f), horizontalColor);
            line.raycastTarget = false;
            line.gameObject.SetActive(visible);
            if (mainBattleSceneMode)
            {
                debugGridLines.Add(line.gameObject);
            }
        }
    }

    private void BuildHeader(Transform parent)
    {
        if (mainBattleSceneMode)
        {
            return;
        }

        CreateImage("Header Panel", parent, new Vector2(0.035f, 0.865f), new Vector2(0.965f, 0.965f), Vector2.zero, Vector2.zero, new Color(0.012f, 0.04f, 0.055f, 0.92f)).raycastTarget = false;
        CreateImage("Header Accent", parent, new Vector2(0.035f, 0.865f), new Vector2(0.965f, 0.873f), Vector2.zero, Vector2.zero, new Color(0.1f, 0.88f, 1f, 0.92f)).raycastTarget = false;
        CreateText("Title", parent, new Vector2(0.055f, 0.91f), new Vector2(0.5f, 0.955f), Vector2.zero, Vector2.zero, "TIMELINE BATTLE", 30, TextAnchor.MiddleLeft, new Color(0.9f, 1f, 1f));
        purposeText = CreateText("Purpose", parent, new Vector2(0.055f, 0.873f), new Vector2(0.58f, 0.915f), Vector2.zero, Vector2.zero, "Separate MVP scene. Existing BattleScene is not used by this prototype.", 16, TextAnchor.MiddleLeft, new Color(0.72f, 0.9f, 0.96f));
        turnText = CreateText("Turn Text", parent, new Vector2(0.58f, 0.91f), new Vector2(0.945f, 0.955f), Vector2.zero, Vector2.zero, string.Empty, 24, TextAnchor.MiddleRight, new Color(1f, 0.88f, 0.28f));
        selectedText = CreateText("Selected Text", parent, new Vector2(0.58f, 0.873f), new Vector2(0.945f, 0.915f), Vector2.zero, Vector2.zero, string.Empty, 15, TextAnchor.MiddleRight, new Color(0.9f, 1f, 0.92f));
    }

    private void BuildSelectedCommandName(Transform parent)
    {
        if (!mainBattleSceneMode)
        {
            return;
        }

        selectedCommandNameRoot = CreateRect(
            "Selected Command Name Row",
            parent,
            new Vector2(0.500f, 0.732f),
            new Vector2(0.500f, 0.732f),
            new Vector2(-230f, -22f),
            new Vector2(230f, 22f));

        HorizontalLayoutGroup layout = selectedCommandNameRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = false;
        layout.spacing = 8f;
        layout.padding = new RectOffset(0, 0, 0, 0);

        selectedCommandActorIcon = CreateImage(
            "Selected Command Actor Icon",
            selectedCommandNameRoot,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero,
            Color.clear);
        selectedCommandActorIcon.preserveAspect = true;
        selectedCommandActorIcon.raycastTarget = false;
        LayoutElement iconLayout = selectedCommandActorIcon.gameObject.AddComponent<LayoutElement>();
        iconLayout.minWidth = 32f;
        iconLayout.minHeight = 32f;
        iconLayout.preferredWidth = 32f;
        iconLayout.preferredHeight = 32f;

        selectedCommandNameText = CreateText(
            "Selected Command Name Text",
            selectedCommandNameRoot,
            Vector2.zero,
            Vector2.one,
            Vector2.zero,
            Vector2.zero,
            string.Empty,
            24,
            TextAnchor.MiddleLeft,
            new Color(0.88f, 0.98f, 1f, 1f));
        selectedCommandNameText.fontStyle = FontStyle.Bold;
        selectedCommandNameText.resizeTextForBestFit = true;
        selectedCommandNameText.resizeTextMinSize = 16;
        selectedCommandNameText.resizeTextMaxSize = 24;
        selectedCommandNameText.raycastTarget = false;

        Outline outline = selectedCommandNameText.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.01f, 0.06f, 0.16f, 0.98f);
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = true;

        selectedCommandNameRoot.gameObject.SetActive(false);
    }

    private bool BuildPrefabTimeline(Transform parent)
    {
        if (!mainBattleSceneMode || !usePrefabActionOrderHud || battleTimelineHudPrefab == null)
        {
            return false;
        }

        battleTimelineHudView = Instantiate(battleTimelineHudPrefab, parent, false);
        battleTimelineHudView.name = "BattleTimelineHud";
        RectTransform hudRect = battleTimelineHudView.transform as RectTransform;
        if (hudRect != null)
        {
            hudRect.anchorMin = new Vector2(BattleSceneHudMinX, BattleSceneHudMinY);
            hudRect.anchorMax = new Vector2(BattleSceneHudMaxX, BattleSceneHudMaxY);
            hudRect.offsetMin = Vector2.zero;
            hudRect.offsetMax = Vector2.zero;
            hudRect.localScale = Vector3.one;
        }

        battleTimelineHudView.CacheReferences();
        ApplyPrefabTimelineHudLayout(battleTimelineHudView);
        if (battleTimelineHudView.ActionOrderText != null)
        {
            HideText(battleTimelineHudView.ActionOrderText);
        }

        if (battleTimelineHudView.CurrentHpLabel != null)
        {
            HideText(battleTimelineHudView.CurrentHpLabel);
        }

        return true;
    }

    private static void ApplyPrefabTimelineHudLayout(BattleTimelineHudView hudView)
    {
        if (hudView == null)
        {
            return;
        }

        RectTransform hudRect = hudView.transform as RectTransform;
        SetAnchors(hudRect, BattleSceneHudMinX, BattleSceneHudMinY, BattleSceneHudMaxX, BattleSceneHudMaxY);
        SetImageType(hudRect, Image.Type.Sliced);

        SetAnchors(hudView.LeftPanel, 0.047f, 0.257f, 0.194f, 0.743f);
        SetImageEnabled(hudView.LeftPanel, false);
        if (hudView.ActionOrderText != null)
        {
            HideText(hudView.ActionOrderText);
        }

        if (hudView.CurrentHpLabel != null)
        {
            HideText(hudView.CurrentHpLabel);
        }

        if (hudView.CurrentHpValue != null)
        {
            SetAnchors(hudView.CurrentHpValue.rectTransform, 0.185735f, 0.240f, 0.770735f, 0.820f);
        }

        SetTextStyle(hudView.CurrentHpValue, 30, TextAnchor.MiddleCenter, Color.white);
        SetChildActiveIfPresent(hudView.LeftPanel, "CurrentHpGaugeBack", false);

        SetAnchors(hudView.SlotsRoot, 0.165f, 0.105f, 0.930f, 0.925f);

        BattleTimelineSlotView[] slots = hudView.Slots;
        if (slots != null)
        {
            int slotCount = slots.Length;
            for (int i = 0; i < slotCount; i++)
            {
                BattleTimelineSlotView slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                PrefabTimelineSlotLayout layout = GetPrefabTimelineSlotLayout(i, slotCount);
                bool current = i == 0;
                SetAnchors(slot.Root, layout.AnchorMin.x, layout.AnchorMin.y, layout.AnchorMax.x, layout.AnchorMax.y);
                SetImageType(slot.Root, Image.Type.Sliced);
                ApplyPrefabTimelineSlotInnerLayout(slot, current);
                SetAnchors(
                    slot.IndexText != null ? slot.IndexText.rectTransform : null,
                    0.500f,
                    0.500f,
                    0.500f,
                    0.500f);
                SetAnchors(
                    slot.StateText != null ? slot.StateText.rectTransform : null,
                    0.500f,
                    0.500f,
                    0.500f,
                    0.500f);

                if (slot.Background != null)
                {
                    slot.Background.enabled = true;
                    slot.Background.color = Color.white;
                    slot.Background.raycastTarget = false;
                }

                slot.SetTimelineLabelsVisible(false);
            }
        }

        SetAnchors(hudView.ConnectorLine, 0.055f, 0.070f, 0.930f, 0.105f);
        SetImageType(hudView.ConnectorLine, Image.Type.Sliced);

        if (hudView.CurrentMarker != null)
        {
            hudView.CurrentMarker.gameObject.SetActive(false);
        }

        SetImageEnabled(hudView.RightArrow, false);
        if (hudView.RightArrow != null)
        {
            hudView.RightArrow.gameObject.SetActive(false);
        }
    }

    private static PrefabTimelineSlotLayout GetPrefabTimelineSlotLayout(int displayIndex, int slotCount)
    {
        int safeSlotCount = Mathf.Max(1, slotCount);
        int safeDisplayIndex = Mathf.Clamp(displayIndex, 0, safeSlotCount - 1);
        const float totalWidth = 0.996f;
        const float gap = 0.004f;
        float currentWidth = safeSlotCount > 1 ? 0.142f : totalWidth;
        float normalWidth = safeSlotCount > 1
            ? Mathf.Max(0.050f, (totalWidth - currentWidth - gap * (safeSlotCount - 1)) / (safeSlotCount - 1))
            : totalWidth;

        float x = 0f;
        for (int i = 0; i < safeDisplayIndex; i++)
        {
            x += (i == 0 ? currentWidth : normalWidth) + gap;
        }

        bool currentGate = safeDisplayIndex == 0;
        float width = currentGate ? currentWidth : normalWidth;
        return new PrefabTimelineSlotLayout
        {
            AnchorMin = new Vector2(x, currentGate ? 0.005f : 0.100f),
            AnchorMax = new Vector2(Mathf.Min(totalWidth, x + width), currentGate ? 0.985f : 0.900f)
        };
    }

    private static PrefabTimelineSlotLayout GetPrefabTimelineOffscreenRightLayout(int slotCount)
    {
        int safeSlotCount = Mathf.Max(1, slotCount);
        PrefabTimelineSlotLayout lastLayout = GetPrefabTimelineSlotLayout(safeSlotCount - 1, safeSlotCount);
        float shift = lastLayout.AnchorMax.x - lastLayout.AnchorMin.x + 0.040f;
        lastLayout.AnchorMin.x += shift;
        lastLayout.AnchorMax.x += shift;
        return lastLayout;
    }

    private static PrefabTimelineSlotLayout ShiftPrefabTimelineLayout(PrefabTimelineSlotLayout layout, float x)
    {
        layout.AnchorMin.x += x;
        layout.AnchorMax.x += x;
        return layout;
    }

    private static void ApplyPrefabTimelineSlotLayout(BattleTimelineSlotView slot, PrefabTimelineSlotLayout layout)
    {
        if (slot == null)
        {
            return;
        }

        SetAnchors(slot.Root, layout.AnchorMin.x, layout.AnchorMin.y, layout.AnchorMax.x, layout.AnchorMax.y);
    }

    private static void ApplyPrefabTimelineSlotInnerLayout(BattleTimelineSlotView slot, bool current)
    {
        if (slot == null)
        {
            return;
        }

        SetAnchors(
            slot.Icon != null ? slot.Icon.rectTransform : null,
            current ? 0.090f : 0.075f,
            current ? 0.110f : 0.095f,
            current ? 0.910f : 0.925f,
            current ? 0.900f : 0.905f);
    }

    private static void SetAnchors(RectTransform rectTransform, float minX, float minY, float maxX, float maxY)
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.anchorMin = new Vector2(minX, minY);
        rectTransform.anchorMax = new Vector2(maxX, maxY);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    private static void SetImageType(RectTransform rectTransform, Image.Type imageType)
    {
        if (rectTransform == null)
        {
            return;
        }

        Image image = rectTransform.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        image.color = Color.white;
        image.type = imageType;
        image.preserveAspect = imageType == Image.Type.Simple;
    }

    private static void SetImageEnabled(RectTransform rectTransform, bool enabled)
    {
        if (rectTransform == null)
        {
            return;
        }

        Image image = rectTransform.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        image.enabled = enabled;
        image.raycastTarget = false;
        if (!enabled)
        {
            image.color = Color.clear;
        }
    }

    private static void SetTextFontSize(Text text, int fontSize)
    {
        if (text == null)
        {
            return;
        }

        text.fontSize = fontSize;
        text.resizeTextForBestFit = true;
        text.resizeTextMaxSize = fontSize;
        text.resizeTextMinSize = Mathf.Min(text.resizeTextMinSize, fontSize);
    }

    private static void SetTextStyle(Text text, int fontSize, TextAnchor alignment, Color color)
    {
        if (text == null)
        {
            return;
        }

        SetTextFontSize(text, fontSize);
        text.fontStyle = FontStyle.Bold;
        text.alignment = alignment;
        text.color = color;
    }

    private static void HideText(Text text)
    {
        if (text == null)
        {
            return;
        }

        text.text = string.Empty;
        text.gameObject.SetActive(false);
    }

    private static void SetChildActiveIfPresent(Transform parent, string childName, bool active)
    {
        if (parent == null)
        {
            return;
        }

        Transform child = parent.Find(childName);
        if (child != null)
        {
            child.gameObject.SetActive(active);
        }
    }

    private void BuildTimeline(Transform parent)
    {
        if (BuildPrefabTimeline(parent))
        {
            return;
        }

        RectTransform panel = CreatePanel(
            "Action Bar Panel",
            parent,
            mainBattleSceneMode ? new Vector2(0.035f, 0.795f) : new Vector2(0.035f, 0.70f),
            mainBattleSceneMode ? new Vector2(BattleSceneHudMaxX, 0.985f) : new Vector2(0.965f, 0.85f),
            new Color(0.006f, 0.014f, 0.026f, mainBattleSceneMode ? 0.92f : 0.94f));

        if (mainBattleSceneMode)
        {
            Shadow panelGlow = panel.gameObject.AddComponent<Shadow>();
            panelGlow.effectColor = new Color(0.04f, 0.60f, 1f, 0.58f);
            panelGlow.effectDistance = new Vector2(0f, -5f);

            CreateImage("Action Order Outer Top Line", panel, new Vector2(0.205f, 0.965f), new Vector2(0.992f, 0.976f), Vector2.zero, Vector2.zero, new Color(0.14f, 0.94f, 1f, 0.48f)).raycastTarget = false;
            CreateImage("Action Order Outer Bottom Line", panel, new Vector2(0.205f, 0.021f), new Vector2(0.988f, 0.032f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.70f, 1f, 0.38f)).raycastTarget = false;
            CreateImage("Action Order Outer Left Line", panel, new Vector2(0.008f, 0.060f), new Vector2(0.016f, 0.960f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.88f, 1f, 0.34f)).raycastTarget = false;
            CreateImage("Action Order Outer Right Line", panel, new Vector2(0.984f, 0.060f), new Vector2(0.992f, 0.960f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.88f, 1f, 0.34f)).raycastTarget = false;
            CreateImage("Action Order Top Neon", panel, new Vector2(0.205f, 0.91f), new Vector2(0.986f, 0.942f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.88f, 1f, 0.78f)).raycastTarget = false;
            CreateImage("Action Order Bottom Neon", panel, new Vector2(0.205f, 0.055f), new Vector2(0.982f, 0.085f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.58f, 1f, 0.50f)).raycastTarget = false;
            CreateImage("Action Order Card Rail", panel, new Vector2(0.165f, 0.455f), new Vector2(0.935f, 0.492f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.86f, 1f, 0.30f)).raycastTarget = false;
            CreateImage("Action Order Progress Line Glow", panel, new Vector2(0.212f, 0.041f), new Vector2(0.908f, 0.064f), Vector2.zero, Vector2.zero, new Color(0.04f, 0.72f, 1f, 0.18f)).raycastTarget = false;
            CreateImage("Action Order Progress Line Core", panel, new Vector2(0.216f, 0.050f), new Vector2(0.904f, 0.056f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.92f, 1f, 0.72f)).raycastTarget = false;
            CreateImage("Action Order Left Brace", panel, new Vector2(0.010f, 0.16f), new Vector2(0.026f, 0.88f), Vector2.zero, Vector2.zero, new Color(0.95f, 0.38f, 0.10f, 0.72f)).raycastTarget = false;
            CreateImage("Action Order Right Brace", panel, new Vector2(0.975f, 0.18f), new Vector2(0.990f, 0.86f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.88f, 1f, 0.66f)).raycastTarget = false;

            RectTransform infoPanel = CreatePanel("Action Order Info Panel", panel, new Vector2(0.024f, 0.22f), new Vector2(0.176f, 0.800f), new Color(0.012f, 0.032f, 0.048f, 0.96f));
            Outline infoOutline = infoPanel.gameObject.AddComponent<Outline>();
            infoOutline.effectColor = new Color(0.12f, 0.88f, 1f, 0.62f);
            infoOutline.effectDistance = new Vector2(2f, -2f);
            CreateImage("Action Order Info Hot Edge", infoPanel, new Vector2(0f, 0f), new Vector2(0.020f, 1f), Vector2.zero, Vector2.zero, new Color(1f, 0.58f, 0.14f, 0.84f)).raycastTarget = false;
            CreateImage("Action Order Info Top Edge", infoPanel, new Vector2(0.060f, 0.805f), new Vector2(0.940f, 0.850f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.88f, 1f, 0.56f)).raycastTarget = false;
            CreateImage("Action Order Info Bottom Trace", infoPanel, new Vector2(0.145f, 0.140f), new Vector2(0.820f, 0.158f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.88f, 1f, 0.28f)).raycastTarget = false;

            RectTransform hpReserve = CreatePanel("Current HP Reserved Area", infoPanel, new Vector2(0.080f, 0.260f), new Vector2(0.920f, 0.740f), new Color(0.004f, 0.012f, 0.020f, 0.84f));
            Outline hpOutline = hpReserve.gameObject.AddComponent<Outline>();
            hpOutline.effectColor = new Color(0.12f, 0.88f, 1f, 0.32f);
            hpOutline.effectDistance = new Vector2(1f, -1f);

            currentHpValueText = CreateText("Current HP Value", hpReserve, new Vector2(0.040f, 0.050f), new Vector2(0.960f, 0.950f), Vector2.zero, Vector2.zero, "--", 24, TextAnchor.MiddleCenter, Color.white);
            currentHpValueText.resizeTextMinSize = 16;
            currentHpValueText.resizeTextMaxSize = 24;
            currentHpValueText.horizontalOverflow = HorizontalWrapMode.Overflow;
            currentHpValueText.verticalOverflow = VerticalWrapMode.Overflow;
            Outline hpValueOutline = currentHpValueText.gameObject.AddComponent<Outline>();
            hpValueOutline.effectColor = new Color(0f, 0f, 0f, 0.82f);
            hpValueOutline.effectDistance = new Vector2(1.5f, -1.5f);

            timelineHintText = CreateText("Timeline Hint", panel, new Vector2(0.165f, 0.865f), new Vector2(0.935f, 0.975f), Vector2.zero, Vector2.zero, "Left card acts now. Cards loop back by Delay.", 11, TextAnchor.MiddleRight, new Color(0.68f, 0.84f, 0.9f));
            Text directionText = CreateText("Action Order Direction", panel, new Vector2(0.928f, 0.024f), new Vector2(0.980f, 0.100f), Vector2.zero, Vector2.zero, ">>>", 13, TextAnchor.MiddleRight, new Color(0.46f, 0.96f, 1f, 0.92f));
            directionText.resizeTextMinSize = 9;
            directionText.resizeTextMaxSize = 13;
            directionText.horizontalOverflow = HorizontalWrapMode.Overflow;
            directionText.raycastTarget = false;
        }
        else
        {
            CreateImage("Action Bar Rail", panel, new Vector2(0.06f, 0.47f), new Vector2(0.96f, 0.53f), Vector2.zero, Vector2.zero, new Color(0.10f, 0.86f, 1f, 0.22f)).raycastTarget = false;
            CreateImage("Action Bar Return Arrow", panel, new Vector2(0.955f, 0.43f), new Vector2(0.985f, 0.57f), Vector2.zero, Vector2.zero, new Color(0.95f, 1f, 0.42f, 0.75f)).raycastTarget = false;
            timelineLabelText = CreateText("Timeline Label", panel, new Vector2(0.018f, 0.76f), new Vector2(0.24f, 0.98f), Vector2.zero, Vector2.zero, "TIMELINE", 18, TextAnchor.MiddleLeft, new Color(0.88f, 1f, 1f, 1f));
            timelineHintText = CreateText("Timeline Hint", panel, new Vector2(0.24f, 0.77f), new Vector2(0.98f, 0.98f), Vector2.zero, Vector2.zero, "Speed controls how quickly each unit returns after acting. Leftmost unit is active.", 14, TextAnchor.MiddleRight, new Color(0.68f, 0.84f, 0.9f));
        }

        float cardCursorX = mainBattleSceneMode ? 0.165f : 0f;
        float battleSceneCardGap = 0.007f;
        for (int i = 0; i < TimelinePreviewCount; i++)
        {
            bool currentSlot = mainBattleSceneMode && i == 0;
            float minX;
            float maxX;
            float minY;
            float maxY;
            if (mainBattleSceneMode)
            {
                float slotWidth = currentSlot ? 0.118f : 0.080f;
                minX = cardCursorX;
                maxX = minX + slotWidth;
                minY = currentSlot ? 0.065f : 0.135f;
                maxY = currentSlot ? 0.905f : 0.835f;
                cardCursorX = maxX + battleSceneCardGap;
            }
            else
            {
                float slotStart = 0.025f;
                float slotStep = 0.12f;
                float slotWidth = 0.102f;
                minX = slotStart + i * slotStep;
                maxX = minX + slotWidth;
                minY = 0.10f;
                maxY = 0.60f;
            }

            RectTransform slot = CreateRect("Timeline Slot " + i, panel, new Vector2(minX, minY), new Vector2(maxX, maxY), Vector2.zero, Vector2.zero);
            Image glow = null;
            Image innerPanel = null;
            Image iconPlate = null;
            Image numberPlate = null;
            Image topEdge = null;
            Image bottomEdge = null;
            Image leftEdge = null;
            Image rightEdge = null;
            Image topCut = null;
            Image bottomCut = null;
            Image progressSegment = null;
            Text progressDot = null;
            Text currentMarker = null;
            if (mainBattleSceneMode)
            {
                glow = CreateImage("Timeline Slot Glow " + i, slot, new Vector2(-0.145f, -0.120f), new Vector2(1.145f, 1.120f), Vector2.zero, Vector2.zero, Color.clear);
                glow.raycastTarget = false;
            }

            Image slotPanel = slot.gameObject.AddComponent<Image>();
            slotPanel.sprite = mainBattleSceneMode ? null : GetSpriteOrNull(timelineSprites, s => s.SlotBase);
            slotPanel.color = slotPanel.sprite != null ? Color.white : new Color(0.035f, 0.07f, 0.09f, 0.96f);
            slotPanel.raycastTarget = false;
            if (mainBattleSceneMode)
            {
                Outline slotOutline = slot.gameObject.AddComponent<Outline>();
                slotOutline.effectColor = currentSlot ? new Color(1f, 0.88f, 0.22f, 0.95f) : new Color(0.62f, 0.95f, 1f, 0.55f);
                slotOutline.effectDistance = currentSlot ? new Vector2(3f, -3f) : new Vector2(2f, -2f);
                Shadow slotGlow = slot.gameObject.AddComponent<Shadow>();
                slotGlow.effectColor = currentSlot ? new Color(1f, 0.82f, 0.16f, 0.62f) : new Color(0.05f, 0.76f, 1f, 0.24f);
                slotGlow.effectDistance = currentSlot ? new Vector2(0f, -5f) : new Vector2(0f, -3f);
            }

            if (mainBattleSceneMode)
            {
                innerPanel = CreateImage("Timeline Slot Inner Core " + i, slot, new Vector2(0.055f, 0.095f), new Vector2(0.945f, 0.890f), Vector2.zero, Vector2.zero, new Color(0.002f, 0.010f, 0.018f, 0.74f));
                innerPanel.raycastTarget = false;

                iconPlate = CreateImage("Timeline Slot Icon Plate " + i, slot, new Vector2(currentSlot ? 0.085f : 0.100f, currentSlot ? 0.280f : 0.305f), new Vector2(currentSlot ? 0.915f : 0.900f, currentSlot ? 0.770f : 0.760f), Vector2.zero, Vector2.zero, new Color(0f, 0.018f, 0.030f, 0.88f));
                iconPlate.raycastTarget = false;

                topEdge = CreateImage("Timeline Slot Top Edge " + i, slot, new Vector2(0.085f, 0.905f), new Vector2(0.915f, 0.936f), Vector2.zero, Vector2.zero, Color.clear);
                bottomEdge = CreateImage("Timeline Slot Bottom Edge " + i, slot, new Vector2(0.085f, 0.050f), new Vector2(0.915f, 0.078f), Vector2.zero, Vector2.zero, Color.clear);
                leftEdge = CreateImage("Timeline Slot Left Edge " + i, slot, new Vector2(0.042f, 0.165f), new Vector2(0.066f, 0.840f), Vector2.zero, Vector2.zero, Color.clear);
                rightEdge = CreateImage("Timeline Slot Right Edge " + i, slot, new Vector2(0.934f, 0.165f), new Vector2(0.958f, 0.840f), Vector2.zero, Vector2.zero, Color.clear);
                topCut = CreateImage("Timeline Slot Top Cut " + i, slot, new Vector2(-0.020f, 0.745f), new Vector2(0.205f, 0.790f), Vector2.zero, Vector2.zero, Color.clear);
                bottomCut = CreateImage("Timeline Slot Bottom Cut " + i, slot, new Vector2(0.795f, 0.200f), new Vector2(1.020f, 0.245f), Vector2.zero, Vector2.zero, Color.clear);
                topCut.rectTransform.localEulerAngles = new Vector3(0f, 0f, -24f);
                bottomCut.rectTransform.localEulerAngles = new Vector3(0f, 0f, -24f);

                topEdge.raycastTarget = false;
                bottomEdge.raycastTarget = false;
                leftEdge.raycastTarget = false;
                rightEdge.raycastTarget = false;
                topCut.raycastTarget = false;
                bottomCut.raycastTarget = false;
            }

            Vector2 accentMin = mainBattleSceneMode ? new Vector2(0.050f, currentSlot ? 0.785f : 0.780f) : new Vector2(0f, 0.82f);
            Vector2 accentMax = mainBattleSceneMode ? new Vector2(0.950f, 0.875f) : Vector2.one;
            Image accent = CreateImage("Timeline Slot Accent " + i, slot, accentMin, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            if (mainBattleSceneMode)
            {
                accent.rectTransform.anchorMax = accentMax;
            }
            accent.raycastTarget = false;

            if (mainBattleSceneMode)
            {
                numberPlate = CreateImage("Timeline Slot Number Plate " + i, slot, new Vector2(currentSlot ? 0.085f : 0.080f, 0.085f), new Vector2(currentSlot ? 0.915f : 0.920f, currentSlot ? 0.255f : 0.275f), Vector2.zero, Vector2.zero, new Color(0f, 0.018f, 0.030f, 0.86f));
                numberPlate.raycastTarget = false;
            }

            Vector2 iconMin = mainBattleSceneMode ? new Vector2(currentSlot ? 0.115f : 0.135f, currentSlot ? 0.300f : 0.325f) : new Vector2(0.19f, 0.24f);
            Vector2 iconMax = mainBattleSceneMode ? new Vector2(currentSlot ? 0.885f : 0.865f, currentSlot ? 0.765f : 0.745f) : new Vector2(0.81f, 0.84f);
            Image unitIcon = CreateImage("Timeline Unit Icon " + i, slot, iconMin, iconMax, Vector2.zero, Vector2.zero, new Color(0.12f, 0.22f, 0.28f, 0.92f));
            unitIcon.raycastTarget = false;

            Image cursor = CreateImage("Timeline Cursor " + i, slot, mainBattleSceneMode ? new Vector2(0.28f, -0.095f) : new Vector2(0.28f, 0.02f), mainBattleSceneMode ? new Vector2(0.72f, 0.085f) : new Vector2(0.72f, 0.22f), Vector2.zero, Vector2.zero, new Color(1f, 0.9f, 0.2f, 0.95f));
            cursor.sprite = mainBattleSceneMode ? null : GetSpriteOrNull(timelineSprites, s => s.Cursor);
            cursor.raycastTarget = false;

            Text name = CreateText("Timeline Slot Name " + i, slot, mainBattleSceneMode ? new Vector2(0.085f, 0.075f) : new Vector2(0.06f, 0.05f), mainBattleSceneMode ? new Vector2(0.915f, currentSlot ? 0.265f : 0.280f) : new Vector2(0.94f, 0.30f), Vector2.zero, Vector2.zero, string.Empty, currentSlot ? 22 : 16, TextAnchor.MiddleCenter, Color.white);
            Text detail = CreateText("Timeline Slot Detail " + i, slot, mainBattleSceneMode ? new Vector2(0.060f, currentSlot ? 0.785f : 0.780f) : new Vector2(0.06f, 0.30f), mainBattleSceneMode ? new Vector2(0.940f, 0.875f) : new Vector2(0.94f, 0.54f), Vector2.zero, Vector2.zero, string.Empty, currentSlot ? 13 : 10, TextAnchor.MiddleCenter, new Color(1f, 0.88f, 0.35f));
            if (mainBattleSceneMode)
            {
                name.resizeTextMinSize = currentSlot ? 16 : 12;
                name.resizeTextMaxSize = currentSlot ? 22 : 16;
                detail.resizeTextMinSize = currentSlot ? 9 : 8;
                detail.resizeTextMaxSize = currentSlot ? 13 : 10;
            }

            if (mainBattleSceneMode)
            {
                float dotCenterX = (minX + maxX) * 0.5f;
                progressSegment = CreateImage("Timeline Slot Progress Segment " + i, panel, new Vector2(minX + 0.012f, 0.050f), new Vector2(maxX - 0.012f, 0.058f), Vector2.zero, Vector2.zero, Color.clear);
                progressSegment.raycastTarget = false;

                progressDot = CreateText("Timeline Slot Progress Dot " + i, panel, new Vector2(dotCenterX - 0.014f, 0.012f), new Vector2(dotCenterX + 0.014f, 0.078f), Vector2.zero, Vector2.zero, "●", currentSlot ? 14 : 11, TextAnchor.MiddleCenter, Color.clear);
                progressDot.resizeTextMinSize = currentSlot ? 11 : 9;
                progressDot.resizeTextMaxSize = currentSlot ? 16 : 12;
                progressDot.raycastTarget = false;

                currentMarker = CreateText("Timeline Current Marker " + i, panel, new Vector2(dotCenterX - 0.022f, 0.070f), new Vector2(dotCenterX + 0.022f, 0.134f), Vector2.zero, Vector2.zero, "▲", 18, TextAnchor.MiddleCenter, new Color(1f, 0.86f, 0.20f, 0.96f));
                currentMarker.resizeTextMinSize = 14;
                currentMarker.resizeTextMaxSize = 18;
                currentMarker.raycastTarget = false;
                currentMarker.gameObject.SetActive(false);
            }

            timelineViews.Add(new TimelineSlotView
            {
                Panel = slotPanel,
                Glow = glow,
                InnerPanel = innerPanel,
                IconPlate = iconPlate,
                NumberPlate = numberPlate,
                TopEdge = topEdge,
                BottomEdge = bottomEdge,
                LeftEdge = leftEdge,
                RightEdge = rightEdge,
                TopCut = topCut,
                BottomCut = bottomCut,
                Accent = accent,
                ProgressSegment = progressSegment,
                UnitIcon = unitIcon,
                Cursor = cursor,
                ProgressDot = progressDot,
                CurrentMarker = currentMarker,
                NameText = name,
                DetailText = detail
            });
        }
    }

    private void BuildAllies(Transform parent)
    {
        RectTransform panel = mainBattleSceneMode
            ? CreateBattleGridSidePanel("Ally Panel", parent, true)
            : CreatePanel("Ally Panel", parent, new Vector2(0.035f, 0.305f), new Vector2(0.45f, 0.675f), new Color(0.014f, 0.032f, 0.04f, 0.88f));
        allyLabelText = CreateText("Ally Label", panel, new Vector2(0.02f, 0.90f), new Vector2(0.44f, 1.00f), Vector2.zero, Vector2.zero, "ALLY PARTY", 18, TextAnchor.MiddleLeft, new Color(0.88f, 1f, 1f));
        allyHintText = CreateText("Ally Hint", panel, new Vector2(0.44f, 0.90f), new Vector2(0.98f, 1.00f), Vector2.zero, Vector2.zero, "Click an ally to open chips", 13, TextAnchor.MiddleRight, new Color(0.78f, 0.92f, 0.96f));
        if (mainBattleSceneMode)
        {
            allyLabelText.gameObject.SetActive(false);
            allyHintText.gameObject.SetActive(false);
        }

        if (mainBattleSceneMode)
        {
            if (!useSceneBattleGridPrefabVisuals)
            {
                BuildBattleGridCells(panel, true);
            }

            CreateAllyGridView(panel, PartyPosition.Middle, 0, 1);
            CreateAllyGridView(panel, PartyPosition.Front, 1, 1);
            CreateAllyGridView(panel, PartyPosition.Back, 2, 1);
            return;
        }

        CreateAllyView(panel, PartyPosition.Front, 0.62f);
        CreateAllyView(panel, PartyPosition.Middle, 0.36f);
        CreateAllyView(panel, PartyPosition.Back, 0.10f);
    }

    private void BuildBattleGridCells(Transform parent, bool allySide)
    {
        if (useSceneBattleGridPrefabVisuals)
        {
            return;
        }

        bool generatedGridArt = HasImage2BattleGridArt();
        Color baseColor = generatedGridArt ? Color.clear : allySide ? new Color(0.82f, 0.08f, 0.06f, 0.92f) : new Color(0.06f, 0.33f, 0.82f, 0.92f);
        Color innerColor = generatedGridArt ? Color.clear : allySide ? new Color(0.95f, 0.70f, 0.62f, 0.94f) : new Color(0.68f, 0.84f, 1f, 0.94f);
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                Vector2 min;
                Vector2 max;
                GetGridCellAnchors(row, column, allySide, out min, out max);
                Sprite frameSprite = GetBattlePanelBaseSprite(allySide);
                Image frame = CreateImage((allySide ? "Ally" : "Enemy") + " Battle Panel " + row + "-" + column, parent, min, max, Vector2.zero, Vector2.zero, baseColor);
                frame.sprite = frameSprite;
                frame.type = Image.Type.Simple;
                frame.preserveAspect = false;
                frame.color = frameSprite != null ? Color.white : baseColor;
                frame.raycastTarget = false;

                Image inner = CreateImage("Panel Inner", frame.transform, new Vector2(0.08f, 0.16f), new Vector2(0.92f, 0.84f), Vector2.zero, Vector2.zero, frameSprite != null ? Color.clear : innerColor);
                inner.raycastTarget = false;
            }
        }
    }

    private void GetGridCellAnchors(int row, int column, bool allySide, out Vector2 anchorMin, out Vector2 anchorMax)
    {
        if (mainBattleSceneMode)
        {
            GetContiguousGridCellAnchors(row, column, out anchorMin, out anchorMax);
            return;
        }

        const float left = -0.012f;
        const float right = 1.012f;
        const float bottom = -0.006f;
        const float top = 1.006f;
        const float gapX = -0.040f;
        const float gapY = -0.052f;
        float width = (right - left - gapX * 2f) / 3f;
        float height = (top - bottom - gapY * 2f) / 3f;
        float minX = left + column * (width + gapX);
        float maxX = minX + width;
        float maxY = top - row * (height + gapY);
        float minY = maxY - height;
        anchorMin = new Vector2(minX, minY);
        anchorMax = new Vector2(maxX, maxY);
    }

    private static void GetContiguousGridCellAnchors(int row, int column, out Vector2 anchorMin, out Vector2 anchorMax)
    {
        int clampedRow = Mathf.Clamp(row, 0, BattleGridRows - 1);
        int clampedColumn = Mathf.Clamp(column, 0, BattleGridAllyCols - 1);
        float tileWidth = 1f / BattleGridAllyCols;
        float tileHeight = 1f / BattleGridRows;
        float minX = clampedColumn * tileWidth;
        float maxX = minX + tileWidth;
        float minY = (BattleGridRows - clampedRow - 1) * tileHeight;
        float maxY = minY + tileHeight;
        anchorMin = new Vector2(minX, minY);
        anchorMax = new Vector2(maxX, maxY);
    }

    private static void GetBattleFieldGridCellAnchors(int row, int column, out Vector2 anchorMin, out Vector2 anchorMax)
    {
        int clampedRow = Mathf.Clamp(row, 0, BattleGridRows - 1);
        int clampedColumn = Mathf.Clamp(column, 0, BattleGridTotalCols - 1);
        float tileWidth = 1f / BattleGridTotalCols;
        float tileHeight = 1f / BattleGridRows;
        float minX = clampedColumn * tileWidth;
        float maxX = minX + tileWidth;
        float minY = (BattleGridRows - clampedRow - 1) * tileHeight;
        float maxY = minY + tileHeight;
        anchorMin = new Vector2(minX, minY);
        anchorMax = new Vector2(maxX, maxY);
    }

    private void CreateAllyGridView(Transform parent, PartyPosition position, int row, int column)
    {
        Vector2 cellMin;
        Vector2 cellMax;
        GetGridCellAnchors(row, column, true, out cellMin, out cellMax);
        RectTransform root = CreateRect("Ally Grid " + position, parent, cellMin, cellMax, Vector2.zero, Vector2.zero);
        Image panel = root.gameObject.AddComponent<Image>();
        panel.color = new Color(0.03f, 0.08f, 0.10f, 0.08f);
        Button button = root.gameObject.AddComponent<Button>();
        button.targetGraphic = panel;
        PartyPosition capturedPosition = position;
        button.onClick.AddListener(() => SelectAllyAtPosition(capturedPosition));
        RegisterHoverEvents(root.gameObject, isHovering =>
        {
            allyPanelHoverStates[capturedPosition] = isHovering;
            RefreshAllies();
        });

        Image hoverOverlay = CreateImage("Hover Overlay " + position, root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear);
        hoverOverlay.raycastTarget = false;
        Image selectedHighlight = CreateImage("Selected Highlight " + position, root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.35f, 0.95f, 1f, 0.36f));
        selectedHighlight.raycastTarget = false;
        Image targetableOverlay = CreateImage("Targetable Overlay " + position, root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear);
        targetableOverlay.raycastTarget = false;
        Image activeHighlight = CreateImage("Active Highlight " + position, root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1f, 0.86f, 0.22f, 0.34f));
        activeHighlight.raycastTarget = false;
        Image dangerOverlay = CreateImage("Danger Overlay " + position, root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear);
        dangerOverlay.raycastTarget = false;
        Image disabledOverlay = CreateImage("Disabled Overlay " + position, root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear);
        disabledOverlay.raycastTarget = false;

        RectTransform unitAnchor = CreateRect("UnitAnchor " + position, root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        OrientBattleBillboard(unitAnchor);
        Image portrait = CreateImage("Ally Token " + position, root, new Vector2(0.10f, 0.18f), new Vector2(0.90f, 0.92f), Vector2.zero, Vector2.zero, GetPositionColor(position));
        portrait.rectTransform.SetParent(unitAnchor, false);
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;
        Image accent = CreateImage("Ally Token Accent " + position, portrait.transform, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.22f), Vector2.zero, Vector2.zero, new Color(0.01f, 0.02f, 0.04f, 0.75f));
        accent.raycastTarget = false;
        accent.gameObject.SetActive(!mainBattleSceneMode);
        Text positionText = CreateText("Ally Position " + position, root, new Vector2(0.04f, 0.68f), new Vector2(0.20f, 0.94f), Vector2.zero, Vector2.zero, ShortPosition(position), 20, TextAnchor.MiddleCenter, Color.white);
        Text nameText = CreateText("Ally HP Number " + position, root, new Vector2(0.23f, 0.015f), new Vector2(0.77f, 0.18f), Vector2.zero, Vector2.zero, string.Empty, 22, TextAnchor.MiddleCenter, Color.white);
        ConfigureHpNumberText(nameText);
        Image hpBack = CreateImage("Ally Hp Back " + position, root, new Vector2(0.16f, 0.145f), new Vector2(0.84f, 0.215f), Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.04f, 0.96f));
        hpBack.raycastTarget = false;
        Image hpFill = CreateImage("Ally Hp Fill " + position, hpBack.transform, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f), new Color(0.28f, 1f, 0.45f, 0.95f));
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        hpFill.fillOrigin = 0;
        hpFill.raycastTarget = false;
        hpBack.gameObject.SetActive(!mainBattleSceneMode);
        Text detailText = CreateText("Ally Detail " + position, root, new Vector2(0.02f, 0.80f), new Vector2(0.98f, 1.02f), Vector2.zero, Vector2.zero, string.Empty, 11, TextAnchor.MiddleCenter, new Color(0.82f, 0.94f, 0.96f));
        if (useSceneBattleGridPrefabVisuals)
        {
            portrait.gameObject.SetActive(false);
        }

        allyViews[position] = new AllyView { Panel = panel, Accent = accent, Portrait = portrait, SelectedHighlight = selectedHighlight, ActiveHighlight = activeHighlight, TargetableOverlay = targetableOverlay, DangerOverlay = dangerOverlay, HoverOverlay = hoverOverlay, DisabledOverlay = disabledOverlay, UnitAnchor = unitAnchor, HpBack = hpBack, HpFill = hpFill, HpFillRect = hpFill.rectTransform, PositionText = positionText, NameText = nameText, DetailText = detailText, Button = button };
    }

    private void CreateAllyView(Transform parent, PartyPosition position, float y)
    {
        RectTransform root = CreateRect("Ally " + position, parent, new Vector2(0.04f, y), new Vector2(0.96f, y + 0.20f), Vector2.zero, Vector2.zero);
        Image panel = root.gameObject.AddComponent<Image>();
        panel.sprite = mainBattleSceneMode ? null : GetAllyFrameSprite(position);
        panel.color = panel.sprite != null ? Color.white : new Color(0.028f, 0.055f, 0.07f, 0.96f);
        Button button = root.gameObject.AddComponent<Button>();
        button.targetGraphic = panel;
        PartyPosition capturedPosition = position;
        button.onClick.AddListener(() => SelectAllyAtPosition(capturedPosition));

        Image selectedHighlight = CreateImage("Selected Highlight " + position, root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.35f, 0.95f, 1f, 0.95f));
        selectedHighlight.sprite = mainBattleSceneMode ? null : GetSpriteOrNull(allySprites, s => s.SelectedFrame);
        selectedHighlight.raycastTarget = false;
        Image activeHighlight = CreateImage("Active Highlight " + position, root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1f, 0.86f, 0.22f, 0.95f));
        activeHighlight.sprite = mainBattleSceneMode ? null : GetSpriteOrNull(allySprites, s => s.ActiveFrame);
        activeHighlight.raycastTarget = false;

        Image portrait = CreateImage("Portrait " + position, root, new Vector2(0.055f, 0.18f), new Vector2(0.23f, 0.82f), Vector2.zero, Vector2.zero, GetPositionColor(position));
        portrait.sprite = mainBattleSceneMode ? null : GetSpriteOrNull(allySprites, s => s.SmallIconFrame);
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;

        Image accent = CreateImage("Ally Accent " + position, root, Vector2.zero, new Vector2(0.018f, 1f), Vector2.zero, Vector2.zero, GetPositionColor(position));
        accent.raycastTarget = false;
        Text positionText = CreateText("Ally Position " + position, root, new Vector2(0.26f, 0.58f), new Vector2(0.43f, 0.95f), Vector2.zero, Vector2.zero, ShortPosition(position), 21, TextAnchor.MiddleLeft, GetPositionColor(position));
        Text nameText = CreateText("Ally Name " + position, root, new Vector2(0.40f, 0.55f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero, string.Empty, 20, TextAnchor.MiddleLeft, Color.white);
        Image hpBack = CreateImage("Ally Hp Back " + position, root, new Vector2(0.27f, 0.22f), new Vector2(0.92f, 0.40f), Vector2.zero, Vector2.zero, new Color(0.05f, 0.08f, 0.09f, 0.96f));
        hpBack.sprite = mainBattleSceneMode ? null : GetSpriteOrNull(allySprites, s => s.HpBarBack);
        hpBack.raycastTarget = false;
        Image hpFill = CreateImage("Ally Hp Fill " + position, hpBack.transform, Vector2.zero, Vector2.one, new Vector2(3f, 3f), new Vector2(-3f, -3f), new Color(0.28f, 1f, 0.45f, 0.95f));
        hpFill.sprite = mainBattleSceneMode ? null : GetSpriteOrNull(allySprites, s => s.HpBarFill);
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        hpFill.fillOrigin = 0;
        hpFill.raycastTarget = false;
        Text detailText = CreateText("Ally Detail " + position, root, new Vector2(0.27f, 0.02f), new Vector2(0.95f, 0.22f), Vector2.zero, Vector2.zero, string.Empty, 12, TextAnchor.MiddleLeft, new Color(0.82f, 0.94f, 0.96f));

        allyViews[position] = new AllyView { Panel = panel, Accent = accent, Portrait = portrait, SelectedHighlight = selectedHighlight, ActiveHighlight = activeHighlight, HpBack = hpBack, HpFill = hpFill, HpFillRect = hpFill.rectTransform, PositionText = positionText, NameText = nameText, DetailText = detailText, Button = button };
    }

    private void BuildEnemyGrid(Transform parent)
    {
        RectTransform panel = mainBattleSceneMode
            ? CreateBattleGridSidePanel("Enemy Grid Panel", parent, false)
            : CreatePanel("Enemy Grid Panel", parent, new Vector2(0.485f, 0.305f), new Vector2(0.965f, 0.675f), new Color(0.014f, 0.032f, 0.04f, 0.88f));
        enemyLabelText = CreateText("Enemy Label", panel, new Vector2(0.04f, 0.90f), new Vector2(0.50f, 1.00f), Vector2.zero, Vector2.zero, "ENEMY 3x3 GRID", 18, TextAnchor.MiddleLeft, new Color(0.88f, 1f, 1f));
        enemyHintText = CreateText("Enemy Hint", panel, new Vector2(0.50f, 0.90f), new Vector2(0.98f, 1.00f), Vector2.zero, Vector2.zero, "Select target", 13, TextAnchor.MiddleRight, new Color(0.78f, 0.92f, 0.96f));
        if (mainBattleSceneMode)
        {
            enemyLabelText.gameObject.SetActive(false);
            enemyHintText.gameObject.SetActive(false);
        }

        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                Vector2 cellMin;
                Vector2 cellMax;
                if (mainBattleSceneMode)
                {
                    GetGridCellAnchors(row, column, false, out cellMin, out cellMax);
                }
                else
                {
                    float minX = 0.09f + column * 0.28f;
                    float maxX = minX + 0.22f;
                    float maxY = 0.82f - row * 0.25f;
                    float minY = maxY - 0.18f;
                    cellMin = new Vector2(minX, minY);
                    cellMax = new Vector2(maxX, maxY);
                }

                RectTransform cell = CreateRect("Enemy Cell " + row + "-" + column, panel, cellMin, cellMax, Vector2.zero, Vector2.zero);
                Image image = cell.gameObject.AddComponent<Image>();
                image.sprite = mainBattleSceneMode ? null : GetSpriteOrNull(enemyGridSprites, s => s.Empty);
                image.color = image.sprite != null ? Color.white : mainBattleSceneMode ? new Color(0.02f, 0.04f, 0.05f, 0.02f) : new Color(0.025f, 0.052f, 0.065f, 0.98f);

                Button button = cell.gameObject.AddComponent<Button>();
                button.targetGraphic = image;
                int capturedRow = row;
                int capturedColumn = column;
                button.onClick.AddListener(() => SelectEnemyAt(capturedRow, capturedColumn));
                RegisterHoverEvents(cell.gameObject, isHovering =>
                {
                    enemyPanelHoverStates[capturedRow, capturedColumn] = isHovering;
                    RefreshEnemies();
                });
                Image hoverOverlay = CreateImage("Enemy Hover Overlay " + row + "-" + column, cell, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear);
                hoverOverlay.raycastTarget = false;
                Image highlight = CreateImage("Enemy Highlight " + row + "-" + column, cell, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.2f, 0.95f, 1f, 0.55f));
                highlight.sprite = mainBattleSceneMode ? null : GetSpriteOrNull(enemyGridSprites, s => s.HighlightOverlay);
                highlight.raycastTarget = false;
                Image targetableOverlay = CreateImage("Enemy Targetable Overlay " + row + "-" + column, cell, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear);
                targetableOverlay.raycastTarget = false;
                Image dangerOverlay = CreateImage("Enemy Danger Overlay " + row + "-" + column, cell, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear);
                dangerOverlay.raycastTarget = false;
                Image disabledOverlay = CreateImage("Enemy Disabled Overlay " + row + "-" + column, cell, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear);
                disabledOverlay.raycastTarget = false;
                GameObject enemyRoot = CreateRect("EnemyUnitRoot", cell, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject;
                OrientBattleBillboard(enemyRoot.GetComponent<RectTransform>());
                Image enemySprite = CreateImage("Enemy Sprite", enemyRoot.transform, mainBattleSceneMode ? new Vector2(0.08f, 0.22f) : new Vector2(0.16f, 0.21f), mainBattleSceneMode ? new Vector2(0.92f, 0.92f) : new Vector2(0.84f, 0.88f), Vector2.zero, Vector2.zero, Color.white);
                enemySprite.preserveAspect = true;
                enemySprite.raycastTarget = false;
                if (useSceneBattleGridPrefabVisuals)
                {
                    enemySprite.gameObject.SetActive(false);
                }

                Image hpBack = CreateImage("Enemy Hp Back", enemyRoot.transform, new Vector2(0.15f, 0.12f), new Vector2(0.85f, 0.19f), Vector2.zero, Vector2.zero, new Color(0.04f, 0.04f, 0.05f, 0.95f));
                hpBack.raycastTarget = false;
                Image hpFill = CreateImage("Enemy Hp Fill", hpBack.transform, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f), new Color(1f, 0.26f, 0.32f, 0.95f));
                hpFill.type = Image.Type.Filled;
                hpFill.fillMethod = Image.FillMethod.Horizontal;
                hpFill.fillOrigin = 0;
                hpFill.raycastTarget = false;
                hpBack.gameObject.SetActive(!mainBattleSceneMode);
                Text nameText = CreateText("Enemy HP Number", enemyRoot.transform, mainBattleSceneMode ? new Vector2(0.20f, 0.015f) : new Vector2(0.08f, 0.78f), mainBattleSceneMode ? new Vector2(0.80f, 0.18f) : new Vector2(0.92f, 0.95f), Vector2.zero, Vector2.zero, string.Empty, mainBattleSceneMode ? 22 : 11, TextAnchor.MiddleCenter, Color.white);
                if (mainBattleSceneMode)
                {
                    ConfigureBattleSceneEnemyHpNumberText(nameText);
                }
                Text label = CreateText("Enemy Cell Label " + row + "-" + column, cell, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f), string.Empty, 12, TextAnchor.MiddleCenter, Color.white);
                enemyCellViews[row, column] = new EnemyCellView { Panel = image, Highlight = highlight, TargetableOverlay = targetableOverlay, DangerOverlay = dangerOverlay, HoverOverlay = hoverOverlay, DisabledOverlay = disabledOverlay, UnitAnchor = enemyRoot.GetComponent<RectTransform>(), EnemyRoot = enemyRoot, EnemySprite = enemySprite, HpBack = hpBack, HpFill = hpFill, HpFillRect = hpFill.rectTransform, NameText = nameText, Label = label, Button = button };
            }
        }
    }

    private void BuildBattleSceneAttackRangeOverlay(Transform parent)
    {
        if (!mainBattleSceneMode || parent == null)
        {
            return;
        }

        ClearBattleSceneAttackRangeCells();
        if (useSceneBattleGridPrefabVisuals)
        {
            BuildSceneBattleGridAttackRangeOverlays();
            return;
        }

        battleSceneAttackRangeOverlayRoot = CreateRect("BattleSceneAttackRangeOverlayRoot", parent, BattleGridAnchor, BattleGridAnchor, BattleFieldOffsetMin, BattleFieldOffsetMax);
        battleSceneAttackRangeOverlayRoot.localEulerAngles = new Vector3(BattleFieldTiltDegrees, 0f, 0f);
        battleSceneAttackRangeOverlayRoot.localScale = Vector3.one;

        Sprite rangeSprite = battlePanelSprites != null ? battlePanelSprites.TargetableOverlay : null;
        for (int row = 0; row < BattleGridRows; row++)
        {
            for (int column = 0; column < BattleGridTotalCols; column++)
            {
                Vector2 min;
                Vector2 max;
                GetBattleFieldGridCellAnchors(row, column, out min, out max);
                Image cell = CreateImage("Attack Range Cell " + row + "-" + column, battleSceneAttackRangeOverlayRoot, min, max, Vector2.zero, Vector2.zero, Color.clear);
                cell.sprite = rangeSprite;
                cell.type = Image.Type.Simple;
                cell.preserveAspect = false;
                cell.raycastTarget = false;
                cell.gameObject.SetActive(false);
                battleSceneAttackRangeCells[row, column] = cell;
            }
        }

        battleSceneAttackRangeOverlayRoot.gameObject.SetActive(false);
    }

    private void BuildSceneBattleGridAttackRangeOverlays()
    {
        GameObject grid = FindBattleSceneGridRoot();
        if (grid == null)
        {
            return;
        }

        switch (BattleSceneAttackRangeMode)
        {
            case BattleSceneAttackRangeOverlayMode.AlignedPanelSprites:
                if (BuildSceneBattleGridAttackRangeAlignedPanelOverlays(grid))
                {
                    return;
                }
                break;
            case BattleSceneAttackRangeOverlayMode.ColliderPolygons:
                if (BuildSceneBattleGridAttackRangeColliderOverlays(grid))
                {
                    return;
                }
                break;
            case BattleSceneAttackRangeOverlayMode.RowSprites:
                if (BuildSceneBattleGridAttackRangeRowOverlays(grid))
                {
                    return;
                }
                break;
        }

        for (int row = 0; row < BattleGridRows; row++)
        {
            for (int column = 0; column < BattleGridTotalCols; column++)
            {
                Sprite panelSprite = LoadOptionalSprite(GetBattleSceneAttackRangePanelSpriteAssetPath(row, column));
                if (panelSprite == null)
                {
                    continue;
                }

                GameObject spriteOverlay = new GameObject("AttackRangePanelSprite_R" + row + "_C" + column, typeof(SpriteRenderer));
                spriteOverlay.transform.SetParent(grid.transform, false);
                spriteOverlay.transform.localPosition = new Vector3(0f, 0f, -0.025f);
                spriteOverlay.transform.localRotation = Quaternion.identity;
                spriteOverlay.transform.localScale = Vector3.one;

                SpriteRenderer spriteRenderer = spriteOverlay.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = panelSprite;
                spriteRenderer.sortingOrder = 28;
                spriteRenderer.enabled = false;
                battleSceneAttackRangeSceneSpriteRenderers[row, column] = spriteRenderer;
            }
        }
    }

    private bool BuildSceneBattleGridAttackRangeAlignedPanelOverlays(GameObject grid)
    {
        bool builtAny = false;
        for (int row = 0; row < BattleGridRows; row++)
        {
            for (int column = 0; column < BattleGridTotalCols; column++)
            {
                Sprite panelSprite = LoadBattleSceneAttackRangeAlignedPanelSprite(row, column);
                if (panelSprite == null)
                {
                    continue;
                }

                GameObject spriteOverlay = new GameObject("AttackRangeAlignedPanelSprite_R" + row + "_C" + column, typeof(SpriteRenderer));
                spriteOverlay.transform.SetParent(grid.transform, false);
                spriteOverlay.transform.localPosition = new Vector3(0f, 0f, BattleSceneAttackRangeOverlayZ);
                spriteOverlay.transform.localRotation = Quaternion.identity;
                spriteOverlay.transform.localScale = Vector3.one;

                SpriteRenderer spriteRenderer = spriteOverlay.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = panelSprite;
                spriteRenderer.sortingOrder = BattleSceneAttackRangeOutlineSortingOrder;
                spriteRenderer.enabled = false;
                battleSceneAttackRangeSceneSpriteRenderers[row, column] = spriteRenderer;
                builtAny = true;
            }
        }

        return builtAny;
    }

    private bool BuildSceneBattleGridAttackRangeColliderOverlays(GameObject grid)
    {
        Transform collidersRoot = grid != null ? grid.transform.Find("GridColliders") : null;
        if (collidersRoot == null)
        {
            return false;
        }

        bool builtAny = false;
        for (int row = 0; row < BattleGridRows; row++)
        {
            for (int column = 0; column < BattleGridTotalCols; column++)
            {
                Transform cell = collidersRoot.Find(GetBattleScenePanelCellName(row, column));
                PolygonCollider2D collider = cell != null ? cell.GetComponent<PolygonCollider2D>() : null;
                if (collider == null || collider.pathCount == 0)
                {
                    continue;
                }

                Vector2[] points = BuildBattleSceneAttackRangeColliderPoints(grid.transform, collider, BattleSceneAttackRangeOverlayExpand);
                if (points == null || points.Length < 3)
                {
                    continue;
                }

                GameObject overlayRoot = new GameObject("AttackRangeColliderOverlay_R" + row + "_C" + column);
                overlayRoot.transform.SetParent(grid.transform, false);
                overlayRoot.transform.localPosition = new Vector3(0f, 0f, BattleSceneAttackRangeOverlayZ);
                overlayRoot.transform.localRotation = Quaternion.identity;
                overlayRoot.transform.localScale = Vector3.one;

                CreateBattleSceneAttackRangeFill(overlayRoot.transform, points, row, column);
                CreateBattleSceneAttackRangeLine(
                    overlayRoot.transform,
                    "Glow",
                    points,
                    new Color(1f, 0.73f, 0.05f, 0.34f),
                    BattleSceneAttackRangeGlowWidth,
                    BattleSceneAttackRangeGlowSortingOrder,
                    GetBattleSceneAttackRangeGlowMaterial());
                CreateBattleSceneAttackRangeLine(
                    overlayRoot.transform,
                    "Outline",
                    points,
                    new Color(1f, 0.92f, 0.28f, 0.92f),
                    BattleSceneAttackRangeOutlineWidth,
                    BattleSceneAttackRangeOutlineSortingOrder,
                    GetBattleSceneAttackRangeOutlineMaterial());

                overlayRoot.SetActive(false);
                battleSceneAttackRangeColliderOverlayRoots[row, column] = overlayRoot;
                builtAny = true;
            }
        }

        return builtAny;
    }

    private static Vector2[] BuildBattleSceneAttackRangeColliderPoints(Transform grid, PolygonCollider2D collider, float expand)
    {
        if (grid == null || collider == null || collider.pathCount == 0)
        {
            return null;
        }

        Vector2[] sourcePoints = collider.GetPath(0);
        if (sourcePoints == null || sourcePoints.Length < 3)
        {
            return null;
        }

        Vector2[] points = new Vector2[sourcePoints.Length];
        Vector2 center = Vector2.zero;
        for (int i = 0; i < sourcePoints.Length; i++)
        {
            Vector3 worldPoint = collider.transform.TransformPoint(sourcePoints[i]);
            Vector3 gridLocalPoint = grid.InverseTransformPoint(worldPoint);
            points[i] = new Vector2(gridLocalPoint.x, gridLocalPoint.y);
            center += points[i];
        }

        center /= points.Length;
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 direction = points[i] - center;
            if (direction.sqrMagnitude > 0.0001f)
            {
                points[i] += direction.normalized * expand;
            }
        }

        return points;
    }

    private void CreateBattleSceneAttackRangeFill(Transform parent, Vector2[] points, int row, int column)
    {
        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(parent, false);
        fillObject.transform.localPosition = Vector3.zero;
        fillObject.transform.localRotation = Quaternion.identity;
        fillObject.transform.localScale = Vector3.one;

        Mesh mesh = new Mesh();
        mesh.name = "AttackRangeFill_R" + row + "_C" + column;

        Vector3[] vertices = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            vertices[i] = new Vector3(points[i].x, points[i].y, 0f);
        }

        List<int> triangles = new List<int>();
        for (int i = 1; i < points.Length - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(i);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();

        MeshFilter meshFilter = fillObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = fillObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = GetBattleSceneAttackRangeFillMaterial();
        meshRenderer.sortingOrder = BattleSceneAttackRangeFillSortingOrder;
    }

    private void CreateBattleSceneAttackRangeLine(Transform parent, string name, Vector2[] points, Color color, float width, int sortingOrder, Material material)
    {
        GameObject lineObject = new GameObject(name);
        lineObject.transform.SetParent(parent, false);
        lineObject.transform.localPosition = Vector3.zero;
        lineObject.transform.localRotation = Quaternion.identity;
        lineObject.transform.localScale = Vector3.one;

        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = points.Length;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.numCapVertices = 4;
        lineRenderer.numCornerVertices = 4;
        lineRenderer.alignment = LineAlignment.View;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.sortingOrder = sortingOrder;
        lineRenderer.sharedMaterial = material;

        for (int i = 0; i < points.Length; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(points[i].x, points[i].y, 0f));
        }
    }

    private Material GetBattleSceneAttackRangeFillMaterial()
    {
        return GetBattleSceneAttackRangeMaterial(
            ref battleSceneAttackRangeFillMaterial,
            "BattleSceneAttackRangeFill",
            new Color(1f, 0.72f, 0.04f, 0.28f));
    }

    private Material GetBattleSceneAttackRangeGlowMaterial()
    {
        return GetBattleSceneAttackRangeMaterial(
            ref battleSceneAttackRangeGlowMaterial,
            "BattleSceneAttackRangeGlow",
            new Color(1f, 0.68f, 0.02f, 0.34f));
    }

    private Material GetBattleSceneAttackRangeOutlineMaterial()
    {
        return GetBattleSceneAttackRangeMaterial(
            ref battleSceneAttackRangeOutlineMaterial,
            "BattleSceneAttackRangeOutline",
            new Color(1f, 0.92f, 0.28f, 0.92f));
    }

    private static Material GetBattleSceneAttackRangeMaterial(ref Material material, string name, Color color)
    {
        if (material != null)
        {
            return material;
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Transparent");
        }

        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        material = new Material(shader);
        material.name = name;
        material.hideFlags = HideFlags.DontSave;
        material.renderQueue = 3000;
        if (material.HasProperty("_Color"))
        {
            material.color = color;
        }

        if (material.HasProperty("_MainTex"))
        {
            material.mainTexture = Texture2D.whiteTexture;
        }

        return material;
    }

    private bool BuildSceneBattleGridAttackRangeRowOverlays(GameObject grid)
    {
        bool builtAny = false;
        for (int row = 0; row < BattleGridRows; row++)
        {
            Sprite rowSprite = LoadBattleSceneAttackRangeRowSprite(row);
            if (rowSprite == null)
            {
                continue;
            }

            GameObject rowOverlay = new GameObject("AttackRangeRowSprite_R" + row, typeof(SpriteRenderer));
            rowOverlay.transform.SetParent(grid.transform, false);
            rowOverlay.transform.localPosition = new Vector3(0f, 0f, -0.024f);
            rowOverlay.transform.localRotation = Quaternion.identity;
            rowOverlay.transform.localScale = Vector3.one;

            SpriteRenderer spriteRenderer = rowOverlay.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = rowSprite;
            spriteRenderer.sortingOrder = 28;
            spriteRenderer.enabled = false;
            battleSceneAttackRangeSceneRowRenderers[row] = spriteRenderer;
            builtAny = true;
        }

        return builtAny;
    }

    private static Sprite LoadBattleSceneAttackRangeRowSprite(int row)
    {
        return LoadFullRectSpriteFromFile(GetBattleSceneAttackRangeRowSpriteAssetPath(row), FilterMode.Bilinear);
    }

    private static Sprite LoadBattleSceneAttackRangeAlignedPanelSprite(int row, int column)
    {
        return LoadFullRectSpriteFromFile(GetBattleSceneAttackRangeAlignedPanelSpriteAssetPath(row, column), FilterMode.Bilinear);
    }

    private static string GetBattleSceneAttackRangeAlignedPanelSpriteAssetPath(int row, int column)
    {
        return BattleSceneAttackRangeAlignedPanelAssetFolder + "/AttackRangePanel_R" + row + "_C" + column + ".png";
    }

    private static string GetBattleSceneAttackRangePanelSpriteAssetPath(int row, int column)
    {
        return BattleSceneAttackRangePanelAssetFolder + "/AttackRangePanel_R" + row + "_C" + column + ".png";
    }

    private static string GetBattleSceneAttackRangeRowSpriteAssetPath(int row)
    {
        return BattleSceneAttackRangeRowAssetFolder + "/AttackRangeRow_R" + row + ".png";
    }

    private void ClearBattleSceneAttackRangeCells()
    {
        GameObject grid = FindBattleSceneGridRoot();
        Transform collidersRoot = grid != null ? grid.transform.Find("GridColliders") : null;
        for (int row = 0; row < BattleGridRows; row++)
        {
            SpriteRenderer rowRenderer = battleSceneAttackRangeSceneRowRenderers[row];
            if (rowRenderer != null)
            {
                Destroy(rowRenderer.gameObject);
            }

            Transform staleRowOverlay = grid != null ? grid.transform.Find("AttackRangeRowSprite_R" + row) : null;
            if (staleRowOverlay != null)
            {
                Destroy(staleRowOverlay.gameObject);
            }

            battleSceneAttackRangeSceneRowRenderers[row] = null;
        }

        for (int row = 0; row < BattleGridRows; row++)
        {
            for (int column = 0; column < BattleGridTotalCols; column++)
            {
                SpriteRenderer spriteRenderer = battleSceneAttackRangeSceneSpriteRenderers[row, column];
                if (spriteRenderer != null)
                {
                    Destroy(spriteRenderer.gameObject);
                }

                Transform cell = collidersRoot != null ? collidersRoot.Find(GetBattleScenePanelCellName(row, column)) : null;
                Transform staleOverlay = cell != null ? cell.Find("AttackRangeGlow_R" + row + "_C" + column) : null;
                if (staleOverlay != null)
                {
                    Destroy(staleOverlay.gameObject);
                }

                Transform staleSpriteOverlay = grid != null ? grid.transform.Find("AttackRangePanelSprite_R" + row + "_C" + column) : null;
                if (staleSpriteOverlay != null)
                {
                    Destroy(staleSpriteOverlay.gameObject);
                }

                Transform staleAlignedSpriteOverlay = grid != null ? grid.transform.Find("AttackRangeAlignedPanelSprite_R" + row + "_C" + column) : null;
                if (staleAlignedSpriteOverlay != null)
                {
                    Destroy(staleAlignedSpriteOverlay.gameObject);
                }

                GameObject colliderOverlayRoot = battleSceneAttackRangeColliderOverlayRoots[row, column];
                if (colliderOverlayRoot != null)
                {
                    Destroy(colliderOverlayRoot);
                }

                Transform staleColliderOverlay = grid != null ? grid.transform.Find("AttackRangeColliderOverlay_R" + row + "_C" + column) : null;
                if (staleColliderOverlay != null)
                {
                    Destroy(staleColliderOverlay.gameObject);
                }

                battleSceneAttackRangeCells[row, column] = null;
                battleSceneAttackRangeSceneSpriteRenderers[row, column] = null;
                battleSceneAttackRangeColliderOverlayRoots[row, column] = null;
            }
        }
    }

    private void BuildHandAndCommands(Transform parent)
    {
        if (!mainBattleSceneMode)
        {
            BuildPrototypeHandAndCommands(parent);
            return;
        }

        BuildBattleScenePrototypeCommandPanel(parent);
    }

    private void BuildBattleScenePrototypeCommandPanel(Transform parent)
    {
        prototypeAttackViews.Clear();
        RectTransform panel = CreatePanel("Battle Command Panel", parent, BattleSceneCommandPanelMin, BattleSceneCommandPanelMax, new Color(0.018f, 0.032f, 0.044f, 0.96f));
        battleSceneCommandRoot = panel.gameObject;
        CreateImage("Battle Command Inner", panel, new Vector2(0.035f, 0.030f), new Vector2(0.965f, 0.970f), Vector2.zero, Vector2.zero, new Color(0.030f, 0.050f, 0.064f, 0.96f)).raycastTarget = false;
        CreateImage("Battle Command Accent", panel, new Vector2(0.035f, 0.965f), new Vector2(0.965f, 0.985f), Vector2.zero, Vector2.zero, new Color(1f, 0.78f, 0.20f, 0.95f)).raycastTarget = false;

        CreateText("Battle Command Title", panel, new Vector2(0.080f, 0.880f), new Vector2(0.920f, 0.960f), Vector2.zero, Vector2.zero, "COMMAND", 20, TextAnchor.MiddleLeft, new Color(0.90f, 0.98f, 1f, 1f));
        battleSceneCommandActorText = CreateText("Battle Command Actor", panel, new Vector2(0.080f, 0.785f), new Vector2(0.920f, 0.860f), Vector2.zero, Vector2.zero, string.Empty, 13, TextAnchor.MiddleLeft, new Color(1f, 0.91f, 0.42f, 1f));
        battleSceneCommandTargetText = CreateText("Battle Command Target", panel, new Vector2(0.080f, 0.708f), new Vector2(0.920f, 0.775f), Vector2.zero, Vector2.zero, string.Empty, 12, TextAnchor.MiddleLeft, new Color(0.72f, 0.92f, 1f, 1f));
        battleSceneCommandSelectedText = CreateText("Battle Command Selected", panel, new Vector2(0.080f, 0.625f), new Vector2(0.920f, 0.700f), Vector2.zero, Vector2.zero, string.Empty, 12, TextAnchor.MiddleLeft, new Color(0.86f, 1f, 0.84f, 1f));

        for (int i = 0; i < BattleScenePrototypeAttackCount; i++)
        {
            float maxY = 0.590f - i * 0.150f;
            float minY = maxY - 0.115f;
            int capturedIndex = i;
            Button button = CreateButton("Prototype Attack " + (i + 1), panel, new Vector2(0.080f, minY), new Vector2(0.920f, maxY), Vector2.zero, Vector2.zero, string.Empty, 13, new Color(0.065f, 0.092f, 0.110f, 0.96f));
            button.onClick.AddListener(() => SelectPrototypeAttack(capturedIndex));
            RegisterHoverEvents(button.gameObject, isHovering => SetPrototypeAttackHover(capturedIndex, isHovering));
            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.alignment = TextAnchor.MiddleLeft;
                label.resizeTextMinSize = 9;
                label.resizeTextMaxSize = 13;
            }

            prototypeAttackViews.Add(new PrototypeAttackButtonView
            {
                Button = button,
                Panel = button.GetComponent<Image>(),
                Label = label
            });
        }

        battleSceneCommandOkButton = CreateButton("Prototype Attack OK", panel, new Vector2(0.080f, 0.070f), new Vector2(0.920f, 0.155f), Vector2.zero, Vector2.zero, "OK", 18, new Color(0.12f, 0.42f, 0.25f, 0.96f));
        battleSceneCommandOkButton.onClick.AddListener(Confirm);
        battleSceneCommandOkImage = battleSceneCommandOkButton.GetComponent<Image>();
        statusText = CreateText("Battle Command Status", panel, new Vector2(0.080f, 0.020f), new Vector2(0.920f, 0.062f), Vector2.zero, Vector2.zero, string.Empty, 10, TextAnchor.MiddleLeft, new Color(0.78f, 0.94f, 0.88f, 1f));
        battleSceneCommandRoot.SetActive(false);
    }

    private void BuildPrototypeHandAndCommands(Transform parent)
    {
        RectTransform panel = CreatePanel("Command Panel", parent, new Vector2(0.035f, 0.025f), new Vector2(0.965f, 0.285f), new Color(0.012f, 0.026f, 0.034f, 0.82f));
        handLabelText = CreateText("Hand Label", panel, new Vector2(0.02f, 0.78f), new Vector2(0.22f, 0.96f), Vector2.zero, Vector2.zero, "COMMON HAND", 18, TextAnchor.MiddleLeft, new Color(0.88f, 1f, 1f));
        queueText = CreateText("Queue Text", panel, new Vector2(0.22f, 0.78f), new Vector2(0.70f, 0.96f), Vector2.zero, Vector2.zero, string.Empty, 14, TextAnchor.MiddleLeft, new Color(1f, 0.92f, 0.55f));
        deckText = CreateText("Deck Text", panel, new Vector2(0.70f, 0.78f), new Vector2(0.98f, 0.96f), Vector2.zero, Vector2.zero, string.Empty, 13, TextAnchor.MiddleRight, new Color(0.74f, 0.88f, 0.94f));

        RectTransform cardPreviewRoot = CreateRect("CardPreviewRoot", panel, new Vector2(0.02f, 0.16f), new Vector2(0.70f, 0.74f), Vector2.zero, Vector2.zero);
        for (int i = 0; i < HandSize; i++)
        {
            float slotWidth = 1f / HandSize;
            float minX = i * slotWidth + 0.012f;
            float maxX = (i + 1) * slotWidth - 0.012f;
            RectTransform slot = CreateRect("CardSlot" + (i + 1), cardPreviewRoot, new Vector2(minX, 0f), new Vector2(maxX, 1f), Vector2.zero, Vector2.zero);
            Button button = CreateButton("Hand Card " + i, slot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, string.Empty, 13, new Color(0.08f, 0.12f, 0.18f, 0.55f));
            if (!mainBattleSceneMode)
            {
                AddCardFrameImage(button.transform);
            }
            int capturedIndex = i;
            button.onClick.AddListener(() => QueueCardFromHand(capturedIndex));
            Text[] labels = button.GetComponentsInChildren<Text>();
            ConfigureCardNameText(labels[0]);
            handViews.Add(new CardButtonView { Panel = button.GetComponent<Image>(), NameText = labels[0], DetailText = CreateText("Hand Detail " + i, button.transform, new Vector2(0.12f, 0.08f), new Vector2(0.88f, 0.26f), Vector2.zero, Vector2.zero, string.Empty, 11, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.62f)), Button = button });
        }

        weaponButton = CreateButton("Weapon Button", panel, new Vector2(0.73f, 0.54f), new Vector2(0.845f, 0.72f), Vector2.zero, Vector2.zero, "Weapon", 15, new Color(0.23f, 0.30f, 0.38f, 0.92f));
        weaponButton.onClick.AddListener(QueueWeapon);

        swapFrontMiddleButton = CreateButton("Swap Front Middle", panel, new Vector2(0.865f, 0.54f), new Vector2(0.98f, 0.72f), Vector2.zero, Vector2.zero, "Swap F/M", 14, new Color(0.22f, 0.26f, 0.42f, 0.92f));
        swapFrontMiddleButton.onClick.AddListener(() => QueueSwap(PartyPosition.Front, PartyPosition.Middle));

        swapMiddleBackButton = CreateButton("Swap Middle Back", panel, new Vector2(0.865f, 0.34f), new Vector2(0.98f, 0.50f), Vector2.zero, Vector2.zero, "Swap M/B", 13, new Color(0.22f, 0.26f, 0.42f, 0.92f));
        swapMiddleBackButton.onClick.AddListener(() => QueueSwap(PartyPosition.Middle, PartyPosition.Back));

        resetButton = CreateButton("Reset Selection", panel, new Vector2(0.73f, 0.14f), new Vector2(0.845f, 0.50f), Vector2.zero, Vector2.zero, "Reset", 15, new Color(0.36f, 0.24f, 0.22f, 0.92f));
        resetButton.onClick.AddListener(ResetSelection);

        confirmButton = CreateButton("Confirm Button", panel, new Vector2(0.865f, 0.14f), new Vector2(0.98f, 0.30f), Vector2.zero, Vector2.zero, "Confirm", 16, new Color(0.10f, 0.38f, 0.24f, 0.92f));
        confirmButton.onClick.AddListener(Confirm);

        debugButton = CreateButton("Debug Labels Button", panel, new Vector2(0.02f, 0.02f), new Vector2(0.14f, 0.12f), Vector2.zero, Vector2.zero, "DEBUG", 13, new Color(0.10f, 0.12f, 0.16f, 0.70f));
        debugButton.onClick.AddListener(ToggleDebugLabels);

        statusText = CreateText("Status Text", panel, new Vector2(0.16f, 0.02f), new Vector2(0.70f, 0.12f), Vector2.zero, Vector2.zero, string.Empty, 13, TextAnchor.MiddleLeft, new Color(0.92f, 1f, 0.86f));
    }

    private void BuildChipSelectPanel(Transform parent)
    {
        RectTransform panel = CreatePanel("Chip Select Panel", parent, new Vector2(0.055f, 0.12f), new Vector2(0.44f, 0.86f), new Color(0.82f, 0.88f, 0.94f, 0.98f));
        cardSelectRoot = panel.gameObject;
        CreateImage("Chip Select Inner", panel, new Vector2(0.025f, 0.025f), new Vector2(0.975f, 0.975f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.16f, 0.20f, 1f)).raycastTarget = false;
        chipSelectTitleText = CreateText("Chip Select Title", panel, new Vector2(0.06f, 0.89f), new Vector2(0.92f, 0.97f), Vector2.zero, Vector2.zero, "BATTLE CHIP", 23, TextAnchor.MiddleLeft, new Color(0.86f, 0.92f, 1f));

        RectTransform detail = CreatePanel("Chip Detail Card", panel, new Vector2(0.06f, 0.45f), new Vector2(0.58f, 0.88f), new Color(0.10f, 0.10f, 0.11f, 1f));
        chipDetailNameText = CreateText("Chip Detail Name", detail, new Vector2(0.08f, 0.78f), new Vector2(0.94f, 0.96f), Vector2.zero, Vector2.zero, string.Empty, 20, TextAnchor.MiddleLeft, Color.white);
        chipDetailArtwork = CreateImage("Chip Detail Artwork", detail, new Vector2(0.10f, 0.28f), new Vector2(0.90f, 0.76f), Vector2.zero, Vector2.zero, new Color(0.18f, 0.48f, 0.72f, 1f));
        chipDetailArtwork.raycastTarget = false;
        chipDetailRankBox = CreateImage("Chip Detail Rank Box", detail, new Vector2(0.08f, 0.06f), new Vector2(0.27f, 0.24f), Vector2.zero, Vector2.zero, new Color(0.05f, 0.05f, 0.06f, 1f));
        chipDetailRankBox.raycastTarget = false;
        chipDetailMetaText = CreateText("Chip Detail Rank", chipDetailRankBox.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, string.Empty, 24, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.18f));
        chipDetailAttributeIcon = CreateImage("Chip Detail Attribute", detail, new Vector2(0.30f, 0.06f), new Vector2(0.46f, 0.24f), Vector2.zero, Vector2.zero, new Color(0.15f, 0.16f, 0.18f, 1f));
        chipDetailAttributeIcon.raycastTarget = false;
        chipDetailPowerText = CreateText("Chip Detail Power", detail, new Vector2(0.62f, 0.03f), new Vector2(0.94f, 0.24f), Vector2.zero, Vector2.zero, string.Empty, 30, TextAnchor.MiddleRight, Color.white);

        RectTransform queueColumn = CreatePanel("Chip Queue Column", panel, new Vector2(0.62f, 0.23f), new Vector2(0.76f, 0.88f), new Color(0.05f, 0.07f, 0.08f, 1f));
        chipQueueSlotTexts.Clear();
        for (int i = 0; i < MaxQueuedActions + 2; i++)
        {
            float top = 0.94f - i * 0.18f;
            Image slot = CreateImage("Chip Queue Slot " + i, queueColumn, new Vector2(0.17f, top - 0.13f), new Vector2(0.83f, top), Vector2.zero, Vector2.zero, new Color(0.02f, 0.025f, 0.03f, 1f));
            slot.raycastTarget = false;
            Text slotText = CreateText("Chip Queue Slot Text " + i, slot.transform, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f), string.Empty, 10, TextAnchor.MiddleCenter, new Color(0.86f, 1f, 0.92f));
            slotText.resizeTextMinSize = 7;
            chipQueueSlotTexts.Add(slotText);
        }

        Button okButton = CreateButton("Chip OK Button", panel, new Vector2(0.62f, 0.10f), new Vector2(0.76f, 0.20f), Vector2.zero, Vector2.zero, "OK", 24, new Color(0.82f, 0.08f, 0.05f, 1f));
        okButton.onClick.AddListener(Confirm);
        Button closeButton = CreateButton("Chip Close Button", panel, new Vector2(0.80f, 0.10f), new Vector2(0.94f, 0.20f), Vector2.zero, Vector2.zero, "CLOSE", 14, new Color(0.18f, 0.20f, 0.24f, 1f));
        closeButton.onClick.AddListener(CloseCardSelect);

        RectTransform chipList = CreatePanel("Chip Select List", panel, new Vector2(0.06f, 0.06f), new Vector2(0.56f, 0.42f), new Color(0.84f, 0.91f, 0.98f, 1f));
        for (int i = 0; i < HandSize; i++)
        {
            int row = i / 3;
            int column = i % 3;
            float width = 0.30f;
            float height = 0.42f;
            float minX = 0.04f + column * 0.32f;
            float maxX = minX + width;
            float maxY = 0.92f - row * 0.46f;
            float minY = maxY - height;
            Button button = CreateButton("Chip Card " + i, chipList, new Vector2(minX, minY), new Vector2(maxX, maxY), Vector2.zero, Vector2.zero, string.Empty, 10, new Color(0.10f, 0.10f, 0.11f, 1f));
            int capturedIndex = i;
            button.onClick.AddListener(() => QueueCardFromHand(capturedIndex));
            CardButtonView view = BuildChipCardView(button, i);
            handViews.Add(view);
        }

        cardSelectRoot.SetActive(false);
    }

    private CardButtonView BuildChipCardView(Button button, int index)
    {
        Image panel = button.GetComponent<Image>();
        Text name = button.GetComponentInChildren<Text>();
        ConfigureChipCardNameText(name);
        Image artwork = CreateImage("Chip Artwork " + index, button.transform, new Vector2(0.12f, 0.38f), new Vector2(0.88f, 0.73f), Vector2.zero, Vector2.zero, new Color(0.20f, 0.40f, 0.62f, 1f));
        artwork.raycastTarget = false;
        Image rankBox = CreateImage("Chip Rank Box " + index, button.transform, new Vector2(0.09f, 0.08f), new Vector2(0.28f, 0.28f), Vector2.zero, Vector2.zero, new Color(0.05f, 0.05f, 0.06f, 1f));
        rankBox.raycastTarget = false;
        Text rankText = CreateText("Chip Rank " + index, rankBox.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, string.Empty, 15, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.18f));
        Image attributeIcon = CreateImage("Chip Attribute " + index, button.transform, new Vector2(0.32f, 0.08f), new Vector2(0.50f, 0.28f), Vector2.zero, Vector2.zero, new Color(0.15f, 0.16f, 0.18f, 1f));
        attributeIcon.raycastTarget = false;
        Text powerText = CreateText("Chip Power " + index, button.transform, new Vector2(0.54f, 0.04f), new Vector2(0.92f, 0.29f), Vector2.zero, Vector2.zero, string.Empty, 18, TextAnchor.MiddleRight, Color.white);
        Text detail = CreateText("Chip Detail " + index, button.transform, new Vector2(0.06f, 0.28f), new Vector2(0.94f, 0.37f), Vector2.zero, Vector2.zero, string.Empty, 8, TextAnchor.MiddleCenter, new Color(0.72f, 0.9f, 1f));
        return new CardButtonView { Panel = panel, Artwork = artwork, AttributeIcon = attributeIcon, RankBox = rankBox, NameText = name, DetailText = detail, RankText = rankText, PowerText = powerText, Button = button };
    }

    private void AddCardFrameImage(Transform parent)
    {
        if (cardFrameSprite == null)
        {
            return;
        }

        Image cardFrame = CreateImage("CardFrame", parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
        cardFrame.sprite = cardFrameSprite;
        cardFrame.type = Image.Type.Simple;
        cardFrame.preserveAspect = true;
        cardFrame.raycastTarget = false;
        cardFrame.transform.SetAsFirstSibling();
    }

    private static void ConfigureCardNameText(Text text)
    {
        if (text == null)
        {
            return;
        }

        RectTransform rectTransform = text.rectTransform;
        rectTransform.anchorMin = new Vector2(0.16f, 0.78f);
        rectTransform.anchorMax = new Vector2(0.90f, 0.94f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        text.fontSize = 12;
        text.resizeTextMinSize = 7;
        text.resizeTextMaxSize = 12;
    }

    private static void ConfigureChipCardNameText(Text text)
    {
        if (text == null)
        {
            return;
        }

        RectTransform rectTransform = text.rectTransform;
        rectTransform.anchorMin = new Vector2(0.08f, 0.74f);
        rectTransform.anchorMax = new Vector2(0.94f, 0.96f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        text.fontSize = 15;
        text.alignment = TextAnchor.MiddleLeft;
        text.resizeTextMinSize = 8;
        text.resizeTextMaxSize = 15;
    }

    private static PrototypeAttackDefinition GetPrototypeAttackDefinition(int index)
    {
        switch (index)
        {
            case 0:
                return new PrototypeAttackDefinition { Name = "Quick Attack", Damage = 20, Delay = 60, Attribute = CardAttribute.Neutral, RangePattern = PrototypeAttackRangePattern.RowToEnemyEdge };
            case 2:
                return new PrototypeAttackDefinition { Name = "Heavy Attack", Damage = 55, Delay = 150, Attribute = CardAttribute.Neutral, RangePattern = PrototypeAttackRangePattern.RowToEnemyEdge };
            default:
                return new PrototypeAttackDefinition { Name = "Standard Attack", Damage = 35, Delay = 100, Attribute = CardAttribute.Neutral, RangePattern = PrototypeAttackRangePattern.RowToEnemyEdge };
        }
    }

    private PrototypeAttackDefinition GetSelectedPrototypeAttack()
    {
        selectedPrototypeAttackIndex = Mathf.Clamp(selectedPrototypeAttackIndex, 0, BattleScenePrototypeAttackCount - 1);
        return GetPrototypeAttackDefinition(selectedPrototypeAttackIndex);
    }

    private void SelectPrototypeAttack(int index)
    {
        if (index < 0 || index >= BattleScenePrototypeAttackCount)
        {
            return;
        }

        selectedPrototypeAttackIndex = index;
        PrototypeAttackDefinition attack = GetSelectedPrototypeAttack();
        RefreshAll("Selected " + attack.Name + ". Power " + attack.Damage + " / Delay " + attack.Delay + ".");
    }

    private void SetPrototypeAttackHover(int index, bool hovering)
    {
        if (!mainBattleSceneMode)
        {
            return;
        }

        if (hovering)
        {
            if (index < 0 || index >= BattleScenePrototypeAttackCount)
            {
                return;
            }

            hoveredPrototypeAttackIndex = index;
        }
        else if (hoveredPrototypeAttackIndex == index)
        {
            hoveredPrototypeAttackIndex = -1;
        }

        RefreshBattleSceneCommandPanel();
    }

    private void ConfirmBattleScenePrototypeAttack()
    {
        if (!IsPlayerTurn())
        {
            RefreshAll("No player command is available.");
            return;
        }

        PrototypeAttackDefinition attack = GetSelectedPrototypeAttack();
        AllyUnit actor = activeUnit != null ? activeUnit.Ally : null;
        EnemyUnit target = ResolvePrototypeAttackTarget(actor, attack);
        if (actor == null || target == null)
        {
            RefreshAll("No enemy is inside " + attack.Name + "'s range.");
            return;
        }

        int resolvedPlayerTurn = playerActionTurnCount;
        int defeatedCount = ApplyPrototypeAttackDamage(actor, target, attack);
        if (defeatedCount > maxSimultaneousDefeatCount)
        {
            maxSimultaneousDefeatCount = defeatedCount;
        }

        ClearQueuedActions();
        CloseCardSelect();
        AdvanceActiveUnit(activeUnit, attack.Delay);
        if (TryShowVictory(resolvedPlayerTurn))
        {
            return;
        }

        string enemySummary = ResolveEnemyTurnsUntilPlayerTurn();
        if (battleEnded)
        {
            return;
        }

        playerActionTurnCount++;
        RefreshAll(actor.Name + " used " + attack.Name + ". Delay " + attack.Delay + "." + enemySummary);
    }

    private EnemyUnit ResolvePrototypeAttackTarget(AllyUnit actor, PrototypeAttackDefinition attack)
    {
        if (actor == null || attack == null)
        {
            return null;
        }

        if (selectedEnemy != null && selectedEnemy.IsAlive && IsEnemyInPrototypeAttackRange(actor, selectedEnemy, attack))
        {
            return selectedEnemy;
        }

        int actorRow = GetAllyGridRow(actor.Position);
        EnemyUnit bestTarget = null;
        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyUnit enemy = enemies[i];
            if (enemy == null || !enemy.IsAlive || enemy.GridPosition.x != actorRow)
            {
                continue;
            }

            if (bestTarget == null || enemy.GridPosition.y < bestTarget.GridPosition.y)
            {
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    private bool IsEnemyInCurrentPrototypeAttackRange(EnemyUnit enemy)
    {
        if (!mainBattleSceneMode || activeUnit == null || !activeUnit.IsAlly)
        {
            return false;
        }

        return IsEnemyInPrototypeAttackRange(activeUnit.Ally, enemy, GetSelectedPrototypeAttack());
    }

    private bool IsEnemyInPrototypeAttackRange(AllyUnit actor, EnemyUnit enemy, PrototypeAttackDefinition attack)
    {
        if (actor == null || enemy == null || !enemy.IsAlive || attack == null)
        {
            return false;
        }

        switch (attack.RangePattern)
        {
            case PrototypeAttackRangePattern.RowToEnemyEdge:
                return enemy.GridPosition.x == GetAllyGridRow(actor.Position);
            default:
                return false;
        }
    }

    private int ApplyPrototypeAttackDamage(AllyUnit attacker, EnemyUnit target, PrototypeAttackDefinition attack)
    {
        EnemyUnit resolvedTarget = target != null && target.IsAlive ? target : null;
        if (resolvedTarget == null || attack == null)
        {
            return 0;
        }

        bool wasAlive = resolvedTarget.IsAlive;
        PrototypeDamageResult damageResult = CalculatePrototypeAttackDamage(new PrototypeDamageRequest
        {
            Attacker = attacker,
            Target = resolvedTarget,
            Attack = attack
        });
        int damage = Mathf.Max(0, damageResult.FinalDamage);
        resolvedTarget.Hp = Mathf.Max(0, resolvedTarget.Hp - damage);
        if (attacker != null)
        {
            attacker.Status = attack.Name + " -" + damage + damageResult.Reason;
        }

        if (!resolvedTarget.IsAlive)
        {
            resolvedTarget.Status = "KO";
            if (selectedEnemy == resolvedTarget)
            {
                selectedEnemy = GetFirstAliveEnemy();
            }
        }

        return wasAlive && !resolvedTarget.IsAlive ? 1 : 0;
    }

    private PrototypeDamageResult CalculatePrototypeAttackDamage(PrototypeDamageRequest request)
    {
        PrototypeAttackDefinition attack = request != null ? request.Attack : null;
        EnemyUnit target = request != null ? request.Target : null;
        int baseDamage = attack != null ? Mathf.Max(0, attack.Damage) : 0;
        CardAttribute attribute = attack != null ? attack.Attribute : CardAttribute.Neutral;
        bool weaknessHit = target != null
            && attribute != CardAttribute.Neutral
            && attribute == target.Weakness;
        float multiplier = weaknessHit ? 2f : 1f;
        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(baseDamage * multiplier));
        return new PrototypeDamageResult
        {
            BaseDamage = baseDamage,
            FinalDamage = finalDamage,
            Attribute = attribute,
            Multiplier = multiplier,
            WeaknessHit = weaknessHit,
            Reason = weaknessHit ? " Weakness x2" : string.Empty
        };
    }

    private void QueueCardFromHand(int handIndex)
    {
        if (mainBattleSceneMode)
        {
            SelectPrototypeAttack(handIndex);
            return;
        }

        if (!IsPlayerTurn())
        {
            RefreshAll("Enemy turn. Press Confirm to resolve the enemy action.");
            return;
        }

        if (handIndex < 0 || handIndex >= hand.Count || hand[handIndex] == null)
        {
            return;
        }

        if (queuedHandSlots[handIndex])
        {
            RefreshAll("That card is already queued. Use Reset to change the plan.");
            return;
        }

        PrototypeCard card = hand[handIndex];
        selectedHandIndex = handIndex;
        bool consumesAction = !card.IsClearCard;
        if (consumesAction && GetQueuedActionCost() >= MaxQueuedActions)
        {
            RefreshAll("Action queue is full. Clear cards are still free, but normal actions are capped at 3.");
            return;
        }

        QueuedAction action = new QueuedAction
        {
            Kind = ActionKind.Card,
            Card = card,
            HandIndex = handIndex,
            Actor = activeUnit.Ally,
            AllyTarget = selectedAlly != null && selectedAlly.IsAlive ? selectedAlly : activeUnit.Ally,
            EnemyTarget = selectedEnemy != null && selectedEnemy.IsAlive ? selectedEnemy : GetFirstAliveEnemy(),
            ConsumesAction = consumesAction,
            Label = GetCardDisplayName(card)
        };
        queuedActions.Add(action);
        queuedHandSlots[handIndex] = true;
        RefreshAll("Queued card: " + action.Label + ". Delay " + ResolveQueuedActionDelay(action) + ".");
    }

    private void QueueWeapon()
    {
        if (!IsPlayerTurn())
        {
            RefreshAll("Enemy turn. Press Confirm to resolve the enemy action.");
            return;
        }

        if (GetQueuedActionCost() >= MaxQueuedActions)
        {
            RefreshAll("Action queue is full.");
            return;
        }

        queuedActions.Add(new QueuedAction
        {
            Kind = ActionKind.Weapon,
            Actor = activeUnit.Ally,
            EnemyTarget = selectedEnemy != null && selectedEnemy.IsAlive ? selectedEnemy : GetFirstAliveEnemy(),
            ConsumesAction = true,
            Label = "Weapon"
        });
        RefreshAll("Queued Weapon. Delay " + BattleActionDelayResolver.Resolve(BattleActionDelayKind.Weapon) + ".");
    }

    private void QueueSwap(PartyPosition a, PartyPosition b)
    {
        if (!IsPlayerTurn())
        {
            RefreshAll("Enemy turn. Press Confirm to resolve the enemy action.");
            return;
        }

        if (GetQueuedActionCost() >= MaxQueuedActions)
        {
            RefreshAll("Action queue is full.");
            return;
        }

        if (a == b)
        {
            RefreshAll("Swap requires two different positions.");
            return;
        }

        if (GetAllyAtPosition(a) == null || GetAllyAtPosition(b) == null)
        {
            RefreshAll("Swap failed because one position is empty.");
            return;
        }

        queuedActions.Add(new QueuedAction
        {
            Kind = ActionKind.Swap,
            Actor = activeUnit.Ally,
            SwapA = a,
            SwapB = b,
            ConsumesAction = true,
            Label = "Swap " + ShortPosition(a) + "/" + ShortPosition(b)
        });
        RefreshAll("Queued swap: " + a + " <-> " + b + ". Delay " + BattleActionDelayResolver.Resolve(BattleActionDelayKind.Swap) + ".");
    }

    private void Confirm()
    {
        if (battleEnded)
        {
            RefreshAll("Battle is already finished. Use the result buttons.");
            return;
        }

        activeUnit = GetCurrentActiveUnit();
        if (activeUnit == null)
        {
            RefreshAll("Battle is over.");
            return;
        }

        currentTick = Mathf.Max(currentTick, activeUnit.ReadyTick);

        if (activeUnit.IsSkill)
        {
            string skillName = activeUnit.SkillAction != null ? activeUnit.SkillAction.DisplayName : "Skill";
            int defeatedCount = ResolveSkillTurn(activeUnit.SkillAction);
            if (defeatedCount > maxSimultaneousDefeatCount)
            {
                maxSimultaneousDefeatCount = defeatedCount;
            }

            ConsumeSkillTimelineAction(activeUnit.SkillAction);
            ClearQueuedActions();
            if (TryShowVictory(playerActionTurnCount))
            {
                return;
            }

            string skillChainSummary = ResolveEnemyTurnsUntilPlayerTurn();
            if (battleEnded)
            {
                return;
            }

            RefreshAll(skillName + " resolved and left the Action Bar." + skillChainSummary);
            return;
        }

        if (!activeUnit.IsAlly)
        {
            int enemyDelay = BattleActionDelayResolver.ResolveEnemyActionDelay(activeUnit.Enemy != null && activeUnit.Enemy.IsBoss);
            string actedEnemyName = activeUnit.Enemy != null ? activeUnit.Enemy.Name : "Enemy";
            ResolveEnemyTurn(activeUnit.Enemy);
            ClearQueuedActions();
            AdvanceActiveUnit(activeUnit, enemyDelay);
            if (TryShowDefeat())
            {
                return;
            }

            string enemyChainSummary = ResolveEnemyTurnsUntilPlayerTurn();
            if (battleEnded)
            {
                return;
            }

            RefreshAll(actedEnemyName + " acted and returned to the timeline. Delay " + enemyDelay + "." + enemyChainSummary);
            return;
        }

        if (mainBattleSceneMode)
        {
            ConfirmBattleScenePrototypeAttack();
            return;
        }

        if (queuedActions.Count == 0)
        {
            int waitDelay = BattleActionDelayResolver.Resolve(BattleActionDelayKind.Wait);
            string waitedName = activeUnit.Ally != null ? activeUnit.Ally.Name : "Ally";
            CloseCardSelect();
            AdvanceActiveUnit(activeUnit, waitDelay);
            string waitEnemySummary = ResolveEnemyTurnsUntilPlayerTurn();
            if (battleEnded)
            {
                return;
            }

            playerActionTurnCount++;
            RefreshAll(waitedName + " waited. Delay " + waitDelay + "." + waitEnemySummary);
            return;
        }

        string summary = activeUnit.Ally.Name + " resolved " + queuedActions.Count + " action(s).";
        int actionDelay = ResolveQueuedActionDelay();
        int resolvedPlayerTurn = playerActionTurnCount;
        ResolveQueuedActions();
        DiscardQueuedCards();
        ClearQueuedActions();
        CloseCardSelect();
        DrawToHand();
        AdvanceActiveUnit(activeUnit, actionDelay);
        if (TryShowVictory(resolvedPlayerTurn))
        {
            return;
        }

        string enemySummary = ResolveEnemyTurnsUntilPlayerTurn();
        if (battleEnded)
        {
            return;
        }

        playerActionTurnCount++;
        RefreshAll(summary + " Delay " + actionDelay + "." + enemySummary);
    }

    private void RetryBattle()
    {
        SceneManager.LoadScene(mainBattleSceneMode ? "BattleScene" : "BattleTimelinePrototypeScene");
    }

    private void ReturnToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    private void ReturnToDeckBuild()
    {
        SceneManager.LoadScene("DeckBuildScene");
    }

    private bool TryShowVictory(int victoryTurn)
    {
        if (GetFirstAliveEnemy() != null)
        {
            return false;
        }

        battleEnded = true;
        BattleResultData resultData = new BattleResultData(WasBossBattle(), victoryTurn, playerDamageTakenCount, maxSimultaneousDefeatCount);
        resultData.HuntingLevel = HuntingLevelEvaluator.Evaluate(resultData);
        RefreshAll(mainBattleSceneMode ? "Victory. Battle result is displayed." : "Victory. Timeline result is displayed.");

        if (battleSceneResultOverlay != null)
        {
            battleSceneResultOverlay.Show(resultData, new List<string> { SelectRewardCardName() });
        }
        else if (resultOverlay != null)
        {
            resultOverlay.Show(resultData, SelectRewardCardName());
        }

        return true;
    }

    private bool WasBossBattle()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsBoss)
            {
                return true;
            }
        }

        return false;
    }

    private bool TryShowDefeat()
    {
        if (GetFirstAliveAlly() != null)
        {
            return false;
        }

        battleEnded = true;
        RefreshAll("Defeat. Return to menu or retry from the scene.");
        return true;
    }

    private string ResolveEnemyTurnsUntilPlayerTurn()
    {
        string summary = string.Empty;
        int safety = 0;
        activeUnit = GetCurrentActiveUnit();

        while (!battleEnded
            && activeUnit != null
            && !activeUnit.IsAlly
            && GetFirstAliveEnemy() != null
            && GetFirstAliveAlly() != null
            && safety < 16)
        {
            currentTick = Mathf.Max(currentTick, activeUnit.ReadyTick);
            if (activeUnit.IsSkill)
            {
                string skillName = activeUnit.SkillAction != null ? activeUnit.SkillAction.DisplayName : "Skill";
                int defeatedCount = ResolveSkillTurn(activeUnit.SkillAction);
                if (defeatedCount > maxSimultaneousDefeatCount)
                {
                    maxSimultaneousDefeatCount = defeatedCount;
                }

                ConsumeSkillTimelineAction(activeUnit.SkillAction);
                summary += " " + skillName + " auto resolved.";
                if (TryShowVictory(playerActionTurnCount))
                {
                    break;
                }
            }
            else
            {
                int enemyDelay = BattleActionDelayResolver.ResolveEnemyActionDelay(activeUnit.Enemy != null && activeUnit.Enemy.IsBoss);
                string actedEnemyName = activeUnit.Enemy != null ? activeUnit.Enemy.Name : "Enemy";
                ResolveEnemyTurn(activeUnit.Enemy);
                AdvanceActiveUnit(activeUnit, enemyDelay);
                summary += " " + actedEnemyName + " auto acted D" + enemyDelay + ".";

                if (TryShowDefeat())
                {
                    break;
                }
            }

            safety++;

            activeUnit = GetCurrentActiveUnit();
        }

        return summary;
    }

    private string SelectRewardCardName()
    {
        string[] fallbackRewards = { "アクアショット", "フリーズ", "テッキュウナゲ" };
        if (discardPile.Count > 0)
        {
            PrototypeCard card = discardPile[random.Next(0, discardPile.Count)];
            string displayName = GetCardDisplayName(card);
            if (!string.IsNullOrEmpty(displayName) && displayName != "-")
            {
                return displayName;
            }
        }

        return fallbackRewards[random.Next(0, fallbackRewards.Length)];
    }

    private void ResolveQueuedActions()
    {
        for (int i = 0; i < queuedActions.Count; i++)
        {
            QueuedAction action = queuedActions[i];
            int defeatedCount = 0;
            if (action.Kind == ActionKind.Card)
            {
                defeatedCount = ResolveCard(action);
            }
            else if (action.Kind == ActionKind.Weapon)
            {
                defeatedCount = ResolveWeapon(action);
            }
            else if (action.Kind == ActionKind.Swap)
            {
                ResolveSwap(action.SwapA, action.SwapB);
            }

            if (defeatedCount > maxSimultaneousDefeatCount)
            {
                maxSimultaneousDefeatCount = defeatedCount;
            }
        }
    }

    private int ResolveCard(QueuedAction action)
    {
        PrototypeCard card = action.Card;
        if (card == null)
        {
            return 0;
        }

        if (card.IsUnsupported)
        {
            Debug.LogWarning("Unsupported timeline card: " + GetCardDisplayName(card) + " / " + card.UnsupportedReason);
        }

        switch (card.Effect)
        {
            case PrototypeCardEffect.SingleDamage:
                int singleDefeatCount = ApplyDamage(action.Actor, action.EnemyTarget, Mathf.Max(1, card.Power), GetCardDisplayName(card));
                if (card.AddsEchoSkillEntry)
                {
                    AddSkillEntry("Echo", action.Actor, EchoSkillInsertDelay, EchoSkillPower, action.EnemyTarget);
                }

                return singleDefeatCount;
            case PrototypeCardEffect.RowDamage:
                return ApplyRowDamage(action.Actor, action.EnemyTarget, Mathf.Max(1, card.Power), GetCardDisplayName(card));
            case PrototypeCardEffect.PushDamage:
                int pushDefeatCount = ApplyDamage(action.Actor, action.EnemyTarget, Mathf.Max(0, card.Power), GetCardDisplayName(card));
                if (action.EnemyTarget != null && action.EnemyTarget.IsAlive)
                {
                    PushEnemy(action.EnemyTarget, PushDistance);
                }

                return pushDefeatCount;
            case PrototypeCardEffect.DelayDamage:
                int delayDefeatCount = ApplyDamage(action.Actor, action.EnemyTarget, Mathf.Max(0, card.Power), GetCardDisplayName(card));
                if (action.EnemyTarget != null && action.EnemyTarget.IsAlive)
                {
                    DelayEnemy(action.EnemyTarget, DelayTicks);
                }

                return delayDefeatCount;
            case PrototypeCardEffect.Heal:
                HealAlly(action.AllyTarget, Mathf.Max(1, card.Power));
                return 0;
            case PrototypeCardEffect.EchoShot:
                int echoDefeatCount = ApplyDamage(action.Actor, action.EnemyTarget, Mathf.Max(1, card.Power), GetCardDisplayName(card));
                AddSkillEntry("Echo", action.Actor, EchoSkillInsertDelay, EchoSkillPower, action.EnemyTarget);
                return echoDefeatCount;
            default:
                return 0;
        }
    }

    private int ResolveWeapon(QueuedAction action)
    {
        return ApplyDamage(action.Actor, action.EnemyTarget, WeaponPower, "Weapon");
    }

    private void ResolveSwap(PartyPosition a, PartyPosition b)
    {
        AllyUnit first = GetAllyAtPosition(a);
        AllyUnit second = GetAllyAtPosition(b);
        if (first == null || second == null)
        {
            return;
        }

        first.Position = b;
        second.Position = a;
        EnsureUniquePartyPositions();
        if (activeUnit != null && activeUnit.IsAlly)
        {
            activeUnit.Ally.Status = "Swapped " + ShortPosition(a) + "/" + ShortPosition(b);
        }
    }

    private void ResolveEnemyTurn(EnemyUnit enemy)
    {
        if (enemy == null || !enemy.IsAlive)
        {
            return;
        }

        AllyUnit target = GetPreferredEnemyAttackTarget();

        if (target == null)
        {
            return;
        }

        const int baseDamage = 20;
        int damage = ApplyIncomingDamageModifier(target, baseDamage);
        target.Hp = Mathf.Max(0, target.Hp - damage);
        if (damage > 0)
        {
            playerDamageTakenCount++;
        }

        target.Status = "Hit -" + damage + " (" + FormatPositionModifier(target.Position) + ")";
        enemy.Status = "Attacked " + ShortPosition(target.Position);
    }

    private SkillTimelineAction AddSkillEntry(string actionName, AllyUnit owner, int delay, int power, EnemyUnit target)
    {
        EnemyUnit resolvedTarget = target != null && target.IsAlive ? target : GetFirstAliveEnemy();
        if (resolvedTarget == null)
        {
            return null;
        }

        int safeDelay = Mathf.Max(1, delay);
        SkillTimelineAction skillAction = new SkillTimelineAction
        {
            Id = "Skill_" + skillTimelineActions.Count + "_" + activeUnitSequence,
            DisplayName = string.IsNullOrEmpty(actionName) ? "Skill" : actionName,
            Owner = owner,
            Target = resolvedTarget,
            Power = Mathf.Max(1, power),
            NextReadyTick = currentTick + safeDelay,
            Delay = safeDelay,
            DisplayColor = new Color(0.70f, 1f, 0.36f, 1f),
            Status = "Queued D" + safeDelay
        };

        skillTimelineActions.Add(skillAction);
        if (owner != null)
        {
            owner.Status = skillAction.DisplayName + " queued D" + safeDelay;
        }

        return skillAction;
    }

    private int ResolveSkillTurn(SkillTimelineAction skillAction)
    {
        if (skillAction == null || !skillAction.IsAlive)
        {
            return 0;
        }

        EnemyUnit resolvedTarget = skillAction.Target != null && skillAction.Target.IsAlive ? skillAction.Target : GetFirstAliveEnemy();
        if (resolvedTarget == null)
        {
            skillAction.Status = "No target";
            return 0;
        }

        int defeatedCount = ApplyDamage(skillAction.Owner, resolvedTarget, Mathf.Max(1, skillAction.Power), skillAction.DisplayName);
        skillAction.Status = "Resolved";
        return defeatedCount;
    }

    private void ConsumeSkillTimelineAction(SkillTimelineAction skillAction)
    {
        if (skillAction == null)
        {
            return;
        }

        skillAction.IsAlive = false;
        skillTimelineActions.Remove(skillAction);
        activeUnitSequence++;
        activeUnit = GetCurrentActiveUnit();
        if (activeUnit != null && activeUnit.IsAlly)
        {
            selectedAlly = activeUnit.Ally;
        }
    }

    private int ApplyDamage(AllyUnit attacker, EnemyUnit target, int amount, string source)
    {
        EnemyUnit resolvedTarget = target != null && target.IsAlive ? target : GetFirstAliveEnemy();
        if (resolvedTarget == null)
        {
            return 0;
        }

        bool wasAlive = resolvedTarget.IsAlive;
        int modifiedDamage = ApplyOutgoingDamageModifier(attacker, amount);
        resolvedTarget.Hp = Mathf.Max(0, resolvedTarget.Hp - modifiedDamage);
        if (attacker != null)
        {
            attacker.Status = source + " -" + modifiedDamage + " (" + FormatPositionModifier(attacker.Position) + ")";
        }

        if (!resolvedTarget.IsAlive)
        {
            resolvedTarget.Status = "KO";
            if (selectedEnemy == resolvedTarget)
            {
                selectedEnemy = GetFirstAliveEnemy();
            }
        }

        return wasAlive && !resolvedTarget.IsAlive ? 1 : 0;
    }

    private int ApplyRowDamage(AllyUnit attacker, EnemyUnit target, int amount, string source)
    {
        EnemyUnit rowTarget = target != null && target.IsAlive ? target : GetFirstAliveEnemy();
        if (rowTarget == null)
        {
            return 0;
        }

        int row = rowTarget.GridPosition.x;
        int hitCount = 0;
        int defeatedCount = 0;
        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyUnit enemy = enemies[i];
            if (enemy.IsAlive && enemy.GridPosition.x == row)
            {
                defeatedCount += ApplyDamage(attacker, enemy, amount, source);
                hitCount++;
            }
        }

        if (attacker != null)
        {
            attacker.Status = source + " row hit x" + hitCount + " (" + FormatPositionModifier(attacker.Position) + ")";
        }

        return defeatedCount;
    }

    private void PushEnemy(EnemyUnit target, int distance)
    {
        EnemyUnit resolvedTarget = target != null && target.IsAlive ? target : GetFirstAliveEnemy();
        if (resolvedTarget == null)
        {
            return;
        }

        Vector2Int from = resolvedTarget.GridPosition;
        Vector2Int to = new Vector2Int(from.x, Mathf.Clamp(from.y + Mathf.Max(1, distance), 0, 2));
        if (to == from)
        {
            resolvedTarget.Status = "Push blocked";
            return;
        }

        if (GetEnemyAt(to.x, to.y) != null)
        {
            resolvedTarget.Status = "Push blocked";
            return;
        }

        resolvedTarget.GridPosition = to;
        resolvedTarget.Status = "Pushed";
    }

    private void DelayEnemy(EnemyUnit target, int ticks)
    {
        EnemyUnit resolvedTarget = target != null && target.IsAlive ? target : GetFirstAliveEnemy();
        if (resolvedTarget == null)
        {
            return;
        }

        resolvedTarget.NextReadyTick += Mathf.Max(1, ticks);
        resolvedTarget.Status = "Delayed +" + Mathf.Max(1, ticks);
    }

    private void HealAlly(AllyUnit target, int amount)
    {
        AllyUnit resolvedTarget = target != null && target.IsAlive ? target : activeUnit.Ally;
        if (resolvedTarget == null)
        {
            return;
        }

        int modifiedHealing = ApplyHealingModifier(resolvedTarget, amount);
        resolvedTarget.Hp = Mathf.Min(resolvedTarget.MaxHp, resolvedTarget.Hp + modifiedHealing);
        resolvedTarget.Status = "Healed +" + modifiedHealing + " (" + FormatPositionModifier(resolvedTarget.Position) + ")";
    }

    private void AdvanceActiveUnit(TimelineUnit unit, int actionDelay)
    {
        if (unit == null)
        {
            return;
        }

        int delay = Mathf.Max(1, actionDelay);
        if (unit.IsAlly)
        {
            unit.Ally.NextReadyTick = currentTick + delay;
        }
        else
        {
            unit.Enemy.NextReadyTick = currentTick + delay;
        }

        activeUnitSequence++;
        activeUnit = GetCurrentActiveUnit();
        if (activeUnit != null && activeUnit.IsAlly)
        {
            selectedAlly = activeUnit.Ally;
        }
    }

    private void EnsureUniquePartyPositions()
    {
        if (allies.Count < 3)
        {
            return;
        }

        bool hasFront = HasAllyAtPosition(PartyPosition.Front);
        bool hasMiddle = HasAllyAtPosition(PartyPosition.Middle);
        bool hasBack = HasAllyAtPosition(PartyPosition.Back);
        if (hasFront && hasMiddle && hasBack)
        {
            return;
        }

        PartyPosition[] positions = { PartyPosition.Front, PartyPosition.Middle, PartyPosition.Back };
        for (int i = 0; i < allies.Count && i < positions.Length; i++)
        {
            allies[i].Position = positions[i];
        }
    }

    private bool HasAllyAtPosition(PartyPosition position)
    {
        int count = 0;
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].Position == position)
            {
                count++;
            }
        }

        return count == 1;
    }

    private int ApplyOutgoingDamageModifier(AllyUnit attacker, int amount)
    {
        if (attacker == null)
        {
            return Mathf.Max(0, amount);
        }

        return Mathf.Max(0, Mathf.RoundToInt(amount * GetOutgoingDamageMultiplier(attacker.Position)));
    }

    private int ApplyIncomingDamageModifier(AllyUnit target, int amount)
    {
        if (target == null)
        {
            return Mathf.Max(0, amount);
        }

        return Mathf.Max(0, Mathf.RoundToInt(amount * GetIncomingDamageMultiplier(target.Position)));
    }

    private int ApplyHealingModifier(AllyUnit target, int amount)
    {
        if (target == null)
        {
            return Mathf.Max(0, amount);
        }

        return Mathf.Max(0, Mathf.RoundToInt(amount * GetHealingReceivedMultiplier(target.Position)));
    }

    private float GetOutgoingDamageMultiplier(PartyPosition position)
    {
        return position == PartyPosition.Front ? FrontOutgoingDamageMultiplier : MiddleModifierMultiplier;
    }

    private float GetIncomingDamageMultiplier(PartyPosition position)
    {
        if (position == PartyPosition.Front)
        {
            return FrontIncomingDamageMultiplier;
        }

        if (position == PartyPosition.Back)
        {
            return BackIncomingDamageMultiplier;
        }

        return MiddleModifierMultiplier;
    }

    private float GetHealingReceivedMultiplier(PartyPosition position)
    {
        return position == PartyPosition.Back ? BackHealingReceivedMultiplier : MiddleModifierMultiplier;
    }

    private string FormatPositionModifier(PartyPosition position)
    {
        switch (position)
        {
            case PartyPosition.Front:
                return "DMG +20% / TAKEN +20%";
            case PartyPosition.Back:
                return "TAKEN -20% / HEAL +20%";
            default:
                return "No modifier";
        }
    }

    private void DiscardQueuedCards()
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < queuedActions.Count; i++)
        {
            if (queuedActions[i].Kind == ActionKind.Card && queuedActions[i].HandIndex >= 0)
            {
                indices.Add(queuedActions[i].HandIndex);
            }
        }

        indices.Sort();
        for (int i = indices.Count - 1; i >= 0; i--)
        {
            int handIndex = indices[i];
            if (handIndex >= 0 && handIndex < hand.Count)
            {
                discardPile.Add(hand[handIndex]);
                hand.RemoveAt(handIndex);
            }
        }
    }

    private void ResetSelection()
    {
        ClearQueuedActions();
        CloseCardSelect();
        RefreshAll("Selection reset.");
    }

    private void ToggleDebugLabels()
    {
        showDebugLabels = !showDebugLabels;
        RefreshAll(showDebugLabels ? "Debug labels on." : "Debug labels off.");
    }

    private void ClearQueuedActions()
    {
        queuedActions.Clear();
        ClearQueuedHandSlots();
    }

    private void ClearQueuedHandSlots()
    {
        for (int i = 0; i < queuedHandSlots.Length; i++)
        {
            queuedHandSlots[i] = false;
        }
    }

    private void DrawToHand()
    {
        while (hand.Count < HandSize)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0)
                {
                    break;
                }

                drawPile.AddRange(discardPile);
                discardPile.Clear();
                Shuffle(drawPile);
            }

            PrototypeCard card = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(card);
        }
    }

    private void Shuffle(List<PrototypeCard> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int swapIndex = random.Next(i, cards.Count);
            PrototypeCard temp = cards[i];
            cards[i] = cards[swapIndex];
            cards[swapIndex] = temp;
        }
    }

    private void SelectAllyAtPosition(PartyPosition position)
    {
        AllyUnit ally = GetAllyAtPosition(position);
        if (ally != null)
        {
            selectedAlly = ally;
            if (mainBattleSceneMode && IsPlayerTurn())
            {
                RefreshAll("Selected ally: " + ally.Name + ".");
            }
            else
            {
                RefreshAll("Selected ally target: " + ally.Name + ".");
            }
        }
    }

    private void OpenCardSelect()
    {
        cardSelectOpen = true;
        selectedHandIndex = Mathf.Clamp(selectedHandIndex, 0, Mathf.Max(0, hand.Count - 1));
    }

    private void CloseCardSelect()
    {
        cardSelectOpen = false;
        if (cardSelectRoot != null)
        {
            cardSelectRoot.SetActive(false);
        }

        RefreshSelectedCommandName();
    }

    private void SelectEnemyAt(int row, int column)
    {
        EnemyUnit enemy = GetEnemyAt(row, column);
        if (enemy != null && enemy.IsAlive)
        {
            selectedEnemy = enemy;
            RefreshAll("Selected enemy target: " + enemy.Name + ".");
        }
    }

    private void RefreshAll(string message)
    {
        activeUnit = GetCurrentActiveUnit();
        RefreshTimeline();
        RefreshAllies();
        RefreshEnemies();
        RefreshHand();
        RefreshCommands();
        RefreshHeader();
        RefreshDebugVisibility();
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private void RefreshHeader()
    {
        string activeName = activeUnit != null
            ? activeUnit.IsSkill ? ShortSkillName(activeUnit.SkillAction) : activeUnit.IsAlly ? GetAllyDisplayName(activeUnit.Ally) : ShortEnemyName(activeUnit.Enemy)
            : "None";
        string side = activeUnit != null && activeUnit.IsAlly ? "PLAYER TURN" : activeUnit != null && activeUnit.IsSkill ? "SKILL TURN" : "ENEMY TURN";
        if (battleEnded && GetFirstAliveEnemy() == null)
        {
            side = "RESULT";
            activeName = "Timeline result";
        }
        else if (GetFirstAliveEnemy() == null)
        {
            side = "VICTORY";
            activeName = "All enemies defeated";
        }
        else if (GetFirstAliveAlly() == null)
        {
            side = "DEFEAT";
            activeName = "Party defeated";
        }

        if (turnText != null)
        {
            turnText.text = showDebugLabels
                ? side + " / TICK " + currentTick.ToString("000") + " / ACTIVE: " + activeName
                : side + "  " + activeName;
        }

        if (selectedText != null)
        {
            selectedText.gameObject.SetActive(showDebugLabels);
            selectedText.text = "Selected Ally: " + (selectedAlly != null ? selectedAlly.Name : "-") + "   Target: " + (selectedEnemy != null ? selectedEnemy.Name : "-");
        }

        if (purposeText != null)
        {
            purposeText.gameObject.SetActive(showDebugLabels);
        }

        if (playerHpText != null)
        {
            AllyUnit hpSource = selectedAlly != null && selectedAlly.IsAlive ? selectedAlly : GetFirstAliveAlly();
            playerHpText.text = hpSource != null ? hpSource.Hp.ToString("000") : "000";
        }

        SetOptionalDebugText(timelineHintText);
        SetOptionalDebugText(allyHintText);
        SetOptionalDebugText(enemyHintText);
    }

    private void RefreshDebugVisibility()
    {
        bool showGameDebug = !mainBattleSceneMode || showDebugLabels;
        SetActiveIfPresent(timelineLabelText, mainBattleSceneMode || showGameDebug);
        SetActiveIfPresent(allyLabelText, showGameDebug);
        SetActiveIfPresent(enemyLabelText, showGameDebug);
        SetActiveIfPresent(handLabelText, showGameDebug);

        for (int i = 0; i < debugGridLines.Count; i++)
        {
            if (debugGridLines[i] != null)
            {
                debugGridLines[i].SetActive(showDebugLabels);
            }
        }
    }

    private void RefreshPrefabTimeline(List<TimelinePreview> previews)
    {
        if (battleTimelineHudView == null)
        {
            return;
        }

        BattleTimelineSlotView[] slots = battleTimelineHudView.Slots;
        int slotCount = slots != null ? slots.Length : 0;
        if (slotCount <= 0)
        {
            return;
        }

        RefreshPrefabTimelineGate();

        List<PrefabTimelineDisplayState> desiredStates = BuildPrefabTimelineDisplayStates(previews, slotCount);
        string signature = BuildPrefabTimelineSignature(desiredStates);
        if (desiredStates.Count == 0)
        {
            StopPrefabTimelineAnimation();
            ClearPrefabTimelineSlots(slots);
            prefabTimelineSignature = signature;
            prefabTimelineSlotStates.Clear();
            return;
        }

        AssignPrefabTimelineSlots(desiredStates, slots);
        if (string.Equals(signature, prefabTimelineSignature, StringComparison.Ordinal)
            && prefabTimelineSlotStates.Count == desiredStates.Count)
        {
            for (int i = 0; i < desiredStates.Count; i++)
            {
                ApplyPrefabTimelineSlotVisual(desiredStates[i]);
            }

            if (prefabTimelineAnimationRoutine == null)
            {
                SnapPrefabTimelineSlots(desiredStates, slots);
            }

            UpdatePrefabTimelineSlotStates(desiredStates);
            return;
        }

        StopPrefabTimelineAnimation();
        if (string.IsNullOrEmpty(prefabTimelineSignature) || prefabTimelineSlotStates.Count == 0)
        {
            prefabTimelineSignature = signature;
            SnapPrefabTimelineSlots(desiredStates, slots);
            UpdatePrefabTimelineSlotStates(desiredStates);
            return;
        }

        List<PrefabTimelineGhostMotion> ghosts = CreatePrefabTimelineGhosts(desiredStates);
        prefabTimelineSignature = signature;
        prefabTimelineAnimationRoutine = StartCoroutine(AnimatePrefabTimelineSlots(desiredStates, ghosts, slots));
        UpdatePrefabTimelineSlotStates(desiredStates);
    }

    private void RefreshPrefabTimelineGate()
    {
        if (battleTimelineHudView.CurrentMarker != null)
        {
            battleTimelineHudView.CurrentMarker.gameObject.SetActive(false);
        }

        if (battleTimelineHudView.RightArrow != null)
        {
            battleTimelineHudView.RightArrow.gameObject.SetActive(false);
            Image arrowImage = battleTimelineHudView.RightArrow.GetComponent<Image>();
            if (arrowImage != null)
            {
                arrowImage.enabled = false;
                arrowImage.raycastTarget = false;
            }
        }
    }

    private List<PrefabTimelineDisplayState> BuildPrefabTimelineDisplayStates(List<TimelinePreview> previews, int slotCount)
    {
        List<PrefabTimelineDisplayState> states = new List<PrefabTimelineDisplayState>();
        if (previews == null || slotCount <= 0)
        {
            return states;
        }

        Dictionary<string, int> occurrences = new Dictionary<string, int>();
        int count = Mathf.Min(previews.Count, slotCount);
        for (int i = 0; i < count; i++)
        {
            TimelinePreview preview = previews[i];
            if (preview.Unit == null)
            {
                continue;
            }

            string baseKey = BuildPrefabTimelineEntryBaseKey(preview);
            int occurrence;
            occurrences.TryGetValue(baseKey, out occurrence);
            occurrences[baseKey] = occurrence + 1;
            states.Add(new PrefabTimelineDisplayState
            {
                Key = baseKey + "#" + occurrence,
                LogicalIndex = i,
                DisplayIndex = i,
                Preview = preview
            });
        }

        return states;
    }

    private string BuildPrefabTimelineEntryBaseKey(TimelinePreview preview)
    {
        TimelineUnit unit = preview.Unit;
        BattleTimelineEntry entry = unit != null ? unit.Entry : null;
        string unitId = entry != null && !string.IsNullOrEmpty(entry.UnitId) ? entry.UnitId : unit != null ? unit.DisplayName : "Unknown";
        ActionTimelineEntryType entryType = entry != null
            ? entry.EntryType
            : unit != null && unit.IsSkill ? ActionTimelineEntryType.Skill : unit != null && unit.IsAlly ? ActionTimelineEntryType.Ally : ActionTimelineEntryType.Enemy;
        int nextActTick = currentTick + Mathf.Max(0, preview.DeltaTick);
        return entryType + ":" + unitId + ":" + nextActTick;
    }

    private static string BuildPrefabTimelineSignature(List<PrefabTimelineDisplayState> states)
    {
        if (states == null || states.Count == 0)
        {
            return string.Empty;
        }

        string signature = string.Empty;
        for (int i = 0; i < states.Count; i++)
        {
            signature += states[i].Key + "|";
        }

        return signature;
    }

    private void AssignPrefabTimelineSlots(List<PrefabTimelineDisplayState> desiredStates, BattleTimelineSlotView[] slots)
    {
        Dictionary<string, Queue<PrefabTimelineDisplayState>> oldStatesByKey = new Dictionary<string, Queue<PrefabTimelineDisplayState>>();
        for (int i = 0; i < prefabTimelineSlotStates.Count; i++)
        {
            PrefabTimelineDisplayState state = prefabTimelineSlotStates[i];
            if (state == null || state.Slot == null || string.IsNullOrEmpty(state.Key))
            {
                continue;
            }

            Queue<PrefabTimelineDisplayState> queue;
            if (!oldStatesByKey.TryGetValue(state.Key, out queue))
            {
                queue = new Queue<PrefabTimelineDisplayState>();
                oldStatesByKey.Add(state.Key, queue);
            }

            queue.Enqueue(state);
        }

        HashSet<BattleTimelineSlotView> usedSlots = new HashSet<BattleTimelineSlotView>();
        for (int i = 0; i < desiredStates.Count; i++)
        {
            Queue<PrefabTimelineDisplayState> queue;
            if (oldStatesByKey.TryGetValue(desiredStates[i].Key, out queue) && queue.Count > 0)
            {
                PrefabTimelineDisplayState oldState = queue.Dequeue();
                desiredStates[i].Slot = oldState.Slot;
                desiredStates[i].MatchedPrevious = true;
                usedSlots.Add(oldState.Slot);
            }
        }

        for (int i = 0; i < desiredStates.Count; i++)
        {
            if (desiredStates[i].Slot != null)
            {
                continue;
            }

            for (int slotIndex = 0; slotIndex < slots.Length; slotIndex++)
            {
                BattleTimelineSlotView slot = slots[slotIndex];
                if (slot == null || usedSlots.Contains(slot))
                {
                    continue;
                }

                desiredStates[i].Slot = slot;
                usedSlots.Add(slot);
                break;
            }
        }
    }

    private void SnapPrefabTimelineSlots(List<PrefabTimelineDisplayState> desiredStates, BattleTimelineSlotView[] slots)
    {
        HashSet<BattleTimelineSlotView> usedSlots = new HashSet<BattleTimelineSlotView>();
        for (int i = 0; i < desiredStates.Count; i++)
        {
            PrefabTimelineDisplayState state = desiredStates[i];
            if (state.Slot == null)
            {
                continue;
            }

            ApplyPrefabTimelineSlotVisual(state);
            ApplyPrefabTimelineSlotLayout(state.Slot, GetPrefabTimelineSlotLayout(state.DisplayIndex, slots.Length));
            state.Slot.SetAlpha(1f);
            usedSlots.Add(state.Slot);
        }

        HideUnusedPrefabTimelineSlots(slots, usedSlots);
    }

    private IEnumerator AnimatePrefabTimelineSlots(List<PrefabTimelineDisplayState> desiredStates, List<PrefabTimelineGhostMotion> ghosts, BattleTimelineSlotView[] slots)
    {
        List<PrefabTimelineSlotMotion> motions = new List<PrefabTimelineSlotMotion>();
        HashSet<BattleTimelineSlotView> usedSlots = new HashSet<BattleTimelineSlotView>();
        for (int i = 0; i < desiredStates.Count; i++)
        {
            PrefabTimelineDisplayState state = desiredStates[i];
            if (state.Slot == null)
            {
                continue;
            }

            bool existed = state.MatchedPrevious;
            PrefabTimelineSlotLayout startLayout = existed
                ? GetCurrentPrefabTimelineSlotLayout(state.Slot)
                : GetPrefabTimelineOffscreenRightLayout(slots.Length);
            PrefabTimelineSlotLayout endLayout = GetPrefabTimelineSlotLayout(state.DisplayIndex, slots.Length);
            ApplyPrefabTimelineSlotVisual(state);
            if (!existed)
            {
                ApplyPrefabTimelineSlotLayout(state.Slot, startLayout);
                state.Slot.SetAlpha(0f);
            }

            motions.Add(new PrefabTimelineSlotMotion
            {
                Slot = state.Slot,
                StartLayout = startLayout,
                EndLayout = endLayout,
                StartAlpha = existed ? GetPrefabTimelineSlotAlpha(state.Slot) : 0f,
                EndAlpha = 1f
            });
            usedSlots.Add(state.Slot);
        }

        HideUnusedPrefabTimelineSlots(slots, usedSlots);

        float elapsed = 0f;
        while (elapsed < PrefabTimelineMoveSeconds)
        {
            float rawT = Mathf.Clamp01(elapsed / PrefabTimelineMoveSeconds);
            float t = Mathf.SmoothStep(0f, 1f, rawT);
            ApplyPrefabTimelineMotions(motions, ghosts, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        ApplyPrefabTimelineMotions(motions, ghosts, 1f);
        ClearPrefabTimelineGhosts();
        prefabTimelineAnimationRoutine = null;
    }

    private void ApplyPrefabTimelineMotions(List<PrefabTimelineSlotMotion> motions, List<PrefabTimelineGhostMotion> ghosts, float t)
    {
        for (int i = 0; i < motions.Count; i++)
        {
            PrefabTimelineSlotMotion motion = motions[i];
            if (motion == null || motion.Slot == null)
            {
                continue;
            }

            ApplyPrefabTimelineSlotLayout(motion.Slot, LerpPrefabTimelineSlotLayout(motion.StartLayout, motion.EndLayout, t));
            motion.Slot.SetAlpha(Mathf.Lerp(motion.StartAlpha, motion.EndAlpha, t));
        }

        for (int i = 0; i < ghosts.Count; i++)
        {
            PrefabTimelineGhostMotion ghost = ghosts[i];
            if (ghost == null || ghost.Root == null)
            {
                continue;
            }

            PrefabTimelineSlotLayout layout = LerpPrefabTimelineSlotLayout(ghost.StartLayout, ghost.EndLayout, t);
            SetAnchors(ghost.Root, layout.AnchorMin.x, layout.AnchorMin.y, layout.AnchorMax.x, layout.AnchorMax.y);
            if (ghost.Group != null)
            {
                ghost.Group.alpha = 1f - t;
            }
        }
    }

    private List<PrefabTimelineGhostMotion> CreatePrefabTimelineGhosts(List<PrefabTimelineDisplayState> desiredStates)
    {
        List<PrefabTimelineGhostMotion> ghosts = new List<PrefabTimelineGhostMotion>();
        Dictionary<string, int> desiredCounts = new Dictionary<string, int>();
        for (int i = 0; i < desiredStates.Count; i++)
        {
            int count;
            desiredCounts.TryGetValue(desiredStates[i].Key, out count);
            desiredCounts[desiredStates[i].Key] = count + 1;
        }

        for (int i = 0; i < prefabTimelineSlotStates.Count; i++)
        {
            PrefabTimelineDisplayState oldState = prefabTimelineSlotStates[i];
            if (oldState == null || oldState.Slot == null || oldState.Slot.Root == null)
            {
                continue;
            }

            int remainingCount;
            if (desiredCounts.TryGetValue(oldState.Key, out remainingCount) && remainingCount > 0)
            {
                desiredCounts[oldState.Key] = remainingCount - 1;
                continue;
            }

            RectTransform oldRoot = oldState.Slot.Root;
            GameObject ghostObject = Instantiate(oldRoot.gameObject, oldRoot.parent, false);
            ghostObject.name = oldRoot.gameObject.name + " Fading Out";
            RectTransform ghostRoot = ghostObject.transform as RectTransform;
            if (ghostRoot == null)
            {
                Destroy(ghostObject);
                continue;
            }

            PrefabTimelineSlotLayout startLayout = GetCurrentPrefabTimelineSlotLayout(ghostRoot);
            CanvasGroup group = ghostObject.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = ghostObject.AddComponent<CanvasGroup>();
            }

            group.alpha = GetPrefabTimelineSlotAlpha(oldState.Slot);
            prefabTimelineGhostObjects.Add(ghostObject);
            ghosts.Add(new PrefabTimelineGhostMotion
            {
                Root = ghostRoot,
                Group = group,
                StartLayout = startLayout,
                EndLayout = ShiftPrefabTimelineLayout(startLayout, PrefabTimelineFadeShift)
            });
        }

        return ghosts;
    }

    private void ApplyPrefabTimelineSlotVisual(PrefabTimelineDisplayState state)
    {
        if (state == null || state.Slot == null || state.Preview.Unit == null)
        {
            return;
        }

        BattleTimelineSlotView slot = state.Slot;
        if (slot.Root != null)
        {
            slot.Root.gameObject.SetActive(true);
        }

        TimelinePreview preview = state.Preview;
        bool active = state.LogicalIndex == 0;
        bool skill = preview.Unit.IsSkill;
        bool ally = preview.Unit.IsAlly;
        bool selected = skill ? false : ally
            ? selectedAlly != null && preview.Unit.Ally == selectedAlly
            : selectedEnemy != null && preview.Unit.Enemy == selectedEnemy;
        Color entryColor = !skill
            ? ally ? new Color(0.12f, 0.88f, 1f, 1f) : new Color(1f, 0.34f, 0.14f, 1f)
            : GetTimelineEntryColor(preview.Unit);

        SetImageType(slot.Root, Image.Type.Sliced);
        ApplyPrefabTimelineSlotInnerLayout(slot, active);
        slot.SetTimelineLabelsVisible(false);
        slot.SetIcon(GetTimelineUnitIconSprite(preview.Unit, ally, active, selected), active ? Color.Lerp(entryColor, Color.white, 0.18f) : entryColor);
        slot.SetActiveVisual(active, ally, entryColor);
    }

    private void UpdatePrefabTimelineSlotStates(List<PrefabTimelineDisplayState> desiredStates)
    {
        prefabTimelineSlotStates.Clear();
        for (int i = 0; i < desiredStates.Count; i++)
        {
            prefabTimelineSlotStates.Add(desiredStates[i]);
        }
    }

    private void ClearPrefabTimelineSlots(BattleTimelineSlotView[] slots)
    {
        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            BattleTimelineSlotView slot = slots[i];
            if (slot == null)
            {
                continue;
            }

            if (slot.Root != null)
            {
                slot.Root.gameObject.SetActive(false);
            }

            slot.SetAlpha(0f);
            slot.Clear();
        }
    }

    private void HideUnusedPrefabTimelineSlots(BattleTimelineSlotView[] slots, HashSet<BattleTimelineSlotView> usedSlots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            BattleTimelineSlotView slot = slots[i];
            if (slot == null || usedSlots.Contains(slot))
            {
                continue;
            }

            if (slot.Root != null)
            {
                slot.Root.gameObject.SetActive(false);
            }

            slot.SetAlpha(0f);
            slot.Clear();
        }
    }

    private void StopPrefabTimelineAnimation()
    {
        if (prefabTimelineAnimationRoutine != null)
        {
            StopCoroutine(prefabTimelineAnimationRoutine);
            prefabTimelineAnimationRoutine = null;
        }

        ClearPrefabTimelineGhosts();
    }

    private void ClearPrefabTimelineGhosts()
    {
        for (int i = 0; i < prefabTimelineGhostObjects.Count; i++)
        {
            if (prefabTimelineGhostObjects[i] != null)
            {
                Destroy(prefabTimelineGhostObjects[i]);
            }
        }

        prefabTimelineGhostObjects.Clear();
    }

    private static float GetPrefabTimelineSlotAlpha(BattleTimelineSlotView slot)
    {
        if (slot == null)
        {
            return 0f;
        }

        return slot.EnsureCanvasGroup().alpha;
    }

    private static PrefabTimelineSlotLayout GetCurrentPrefabTimelineSlotLayout(BattleTimelineSlotView slot)
    {
        return slot != null ? GetCurrentPrefabTimelineSlotLayout(slot.Root) : new PrefabTimelineSlotLayout();
    }

    private static PrefabTimelineSlotLayout GetCurrentPrefabTimelineSlotLayout(RectTransform root)
    {
        if (root == null)
        {
            return new PrefabTimelineSlotLayout();
        }

        return new PrefabTimelineSlotLayout
        {
            AnchorMin = root.anchorMin,
            AnchorMax = root.anchorMax
        };
    }

    private static PrefabTimelineSlotLayout LerpPrefabTimelineSlotLayout(PrefabTimelineSlotLayout start, PrefabTimelineSlotLayout end, float t)
    {
        return new PrefabTimelineSlotLayout
        {
            AnchorMin = Vector2.Lerp(start.AnchorMin, end.AnchorMin, t),
            AnchorMax = Vector2.Lerp(start.AnchorMax, end.AnchorMax, t)
        };
    }

    private void RefreshTimeline()
    {
        List<TimelinePreview> previews = BuildTimelinePreview();
        RefreshPrefabTimeline(previews);
        for (int i = 0; i < timelineViews.Count; i++)
        {
            TimelineSlotView view = timelineViews[i];
            if (i >= previews.Count)
            {
                view.Panel.gameObject.SetActive(false);
                if (view.ProgressSegment != null)
                {
                    view.ProgressSegment.gameObject.SetActive(false);
                }

                SetActiveIfPresent(view.ProgressDot, false);
                SetActiveIfPresent(view.CurrentMarker, false);
                continue;
            }

            TimelinePreview preview = previews[i];
            view.Panel.gameObject.SetActive(true);
            bool active = i == 0;
            bool ally = preview.Unit.IsAlly;
            bool skill = preview.Unit.IsSkill;
            bool selected = skill ? false : ally
                ? selectedAlly != null && preview.Unit.Ally == selectedAlly
                : selectedEnemy != null && preview.Unit.Enemy == selectedEnemy;
            Sprite frameSprite = mainBattleSceneMode || skill ? null : GetTimelineFrameSprite(ally, active, selected, i > 0);
            view.Panel.sprite = mainBattleSceneMode ? null : (frameSprite != null ? frameSprite : GetSpriteOrNull(timelineSprites, s => s.SlotBase));
            Color entryColor = mainBattleSceneMode && !skill
                ? ally ? new Color(0.12f, 0.88f, 1f, 1f) : new Color(1f, 0.34f, 0.14f, 1f)
                : GetTimelineEntryColor(preview.Unit);
            Color frameColor = active ? new Color(1f, 0.86f, 0.18f, 1f) : entryColor;
            if (mainBattleSceneMode)
            {
                Color progressColor = active
                    ? new Color(1f, 0.82f, 0.12f, 0.72f)
                    : new Color(entryColor.r, entryColor.g, entryColor.b, skill ? 0.46f : ally ? 0.54f : 0.64f);
                Color dotColor = active
                    ? new Color(1f, 0.88f, 0.18f, 1f)
                    : skill ? new Color(0.34f, 1f, 0.62f, 0.90f)
                        : ally ? new Color(0.36f, 0.96f, 1f, 0.94f) : new Color(1f, 0.46f, 0.16f, 0.94f);

                if (view.ProgressSegment != null)
                {
                    view.ProgressSegment.gameObject.SetActive(true);
                    view.ProgressSegment.color = progressColor;
                }

                if (view.ProgressDot != null)
                {
                    view.ProgressDot.gameObject.SetActive(true);
                    view.ProgressDot.fontSize = active ? 16 : 12;
                    view.ProgressDot.color = dotColor;
                }

                if (view.CurrentMarker != null)
                {
                    view.CurrentMarker.gameObject.SetActive(active);
                    view.CurrentMarker.color = new Color(1f, 0.86f, 0.20f, 0.98f);
                }
            }

            view.Panel.color = view.Panel.sprite != null
                ? Color.white
                : mainBattleSceneMode
                    ? active
                        ? new Color(0.080f, 0.052f, 0.010f, 0.99f)
                        : skill ? new Color(0.025f, 0.075f, 0.040f, 0.96f)
                            : ally ? new Color(0.003f, 0.022f, 0.042f, 0.97f) : new Color(0.060f, 0.014f, 0.008f, 0.97f)
                    : active ? new Color(0.14f, 0.16f, 0.09f, 0.98f) : skill ? new Color(0.055f, 0.12f, 0.075f, 0.96f) : new Color(0.03f, 0.055f, 0.075f, 0.96f);
            if (view.Glow != null)
            {
                view.Glow.color = active ? new Color(1f, 0.82f, 0.10f, 0.34f) : new Color(entryColor.r, entryColor.g, entryColor.b, 0.10f);
            }

            if (mainBattleSceneMode)
            {
                Color innerColor = active
                    ? new Color(0.030f, 0.022f, 0.006f, 0.88f)
                    : skill ? new Color(0.004f, 0.030f, 0.020f, 0.82f)
                        : ally ? new Color(0f, 0.018f, 0.038f, 0.84f) : new Color(0.036f, 0.006f, 0.004f, 0.84f);
                Color plateColor = active
                    ? new Color(0.050f, 0.036f, 0.004f, 0.90f)
                    : skill ? new Color(0.002f, 0.035f, 0.020f, 0.84f)
                        : ally ? new Color(0f, 0.025f, 0.046f, 0.88f) : new Color(0.046f, 0.006f, 0.004f, 0.88f);
                Color strongLine = new Color(frameColor.r, frameColor.g, frameColor.b, active ? 0.96f : 0.76f);
                Color softLine = new Color(frameColor.r, frameColor.g, frameColor.b, active ? 0.66f : 0.42f);
                Color slashLine = new Color(frameColor.r, frameColor.g, frameColor.b, active ? 0.88f : 0.60f);

                SetImageColorIfPresent(view.InnerPanel, innerColor);
                SetImageColorIfPresent(view.IconPlate, plateColor);
                SetImageColorIfPresent(view.NumberPlate, active ? new Color(0.018f, 0.013f, 0.002f, 0.92f) : plateColor);
                SetImageColorIfPresent(view.TopEdge, strongLine);
                SetImageColorIfPresent(view.BottomEdge, softLine);
                SetImageColorIfPresent(view.LeftEdge, softLine);
                SetImageColorIfPresent(view.RightEdge, softLine);
                SetImageColorIfPresent(view.TopCut, slashLine);
                SetImageColorIfPresent(view.BottomCut, slashLine);
            }

            Outline outline = mainBattleSceneMode ? view.Panel.GetComponent<Outline>() : null;
            if (outline != null)
            {
                outline.effectColor = active ? new Color(1f, 0.88f, 0.22f, 0.98f) : Color.Lerp(entryColor, Color.white, 0.12f);
                outline.effectDistance = active ? new Vector2(3f, -3f) : new Vector2(2f, -2f);
            }

            if (mainBattleSceneMode)
            {
                Shadow[] shadows = view.Panel.GetComponents<Shadow>();
                for (int shadowIndex = 0; shadowIndex < shadows.Length; shadowIndex++)
                {
                    Shadow shadow = shadows[shadowIndex];
                    if (shadow == null || shadow.GetType() != typeof(Shadow))
                    {
                        continue;
                    }

                    shadow.effectColor = active ? new Color(1f, 0.82f, 0.16f, 0.72f) : new Color(entryColor.r, entryColor.g, entryColor.b, 0.26f);
                    shadow.effectDistance = active ? new Vector2(0f, -5f) : new Vector2(0f, -3f);
                    break;
                }
            }

            view.Accent.color = active ? new Color(1f, 0.88f, 0.22f, 0.96f) : entryColor;
            view.UnitIcon.sprite = GetTimelineUnitIconSprite(preview.Unit, ally, active, selected);
            view.UnitIcon.preserveAspect = true;
            view.UnitIcon.color = view.UnitIcon.sprite != null ? Color.white : active ? Color.Lerp(entryColor, Color.white, 0.18f) : entryColor;
            view.UnitIcon.rectTransform.localScale = GetTimelineUnitIconScale(preview.Unit);
            view.Cursor.gameObject.SetActive(active);
            view.NameText.text = mainBattleSceneMode ? (i + 1).ToString("00") : GetTimelineDisplayName(preview.Unit);
            view.NameText.color = active ? new Color(1f, 0.96f, 0.58f, 1f) : Color.white;
            bool showTimelineDetail = mainBattleSceneMode || showDebugLabels;
            view.DetailText.gameObject.SetActive(showTimelineDetail);
            BattleTimelineEntry entry = preview.Unit.Entry;
            int nextActTick = entry != null ? entry.NextActTick : preview.Unit.ReadyTick;
            if (showTimelineDetail)
            {
                view.DetailText.text = mainBattleSceneMode
                    ? active ? "CURRENT" : i == 1 ? "NEXT" : GetTimelineCardTypeLabel(preview.Unit)
                    : GetTimelineDebugLine(preview.Unit)
                        + "\nT+" + preview.DeltaTick + " next " + nextActTick + " delay " + GetPreviewLoopDelay(preview.Unit);
                view.DetailText.color = mainBattleSceneMode
                    ? active ? new Color(0.052f, 0.035f, 0.004f, 1f) : Color.white
                    : active ? new Color(0.05f, 0.035f, 0.01f, 1f) : entryColor;
            }
            else
            {
                view.DetailText.text = string.Empty;
            }
        }

        RefreshCurrentHpPanel();
    }

    private void RefreshCurrentHpPanel()
    {
        int hp;
        int maxHp;
        bool hasHp = TryGetCurrentUnitHp(out hp, out maxHp);
        int safeMaxHp = Mathf.Max(0, maxHp);
        int safeHp = hasHp ? Mathf.Clamp(hp, 0, safeMaxHp) : 0;
        if (currentHpValueText != null)
        {
            currentHpValueText.text = hasHp ? safeHp.ToString() : "--";
            currentHpValueText.color = hasHp ? Color.white : new Color(0.58f, 0.72f, 0.78f, 0.86f);
        }

        if (battleTimelineHudView != null)
        {
            if (hasHp)
            {
                battleTimelineHudView.SetCurrentHp(safeHp, safeMaxHp);
            }
            else
            {
                battleTimelineHudView.SetCurrentHpUnavailable();
            }
        }
    }

    private bool TryGetCurrentUnitHp(out int hp, out int maxHp)
    {
        hp = 0;
        maxHp = 0;
        TimelineUnit unit = GetCurrentHpDisplayUnit();
        if (unit == null)
        {
            return false;
        }

        if (unit.IsSkill)
        {
            AllyUnit owner = unit.SkillAction != null ? unit.SkillAction.Owner : null;
            if (owner != null)
            {
                hp = owner.Hp;
                maxHp = owner.MaxHp;
                return maxHp > 0;
            }

            return false;
        }

        if (unit.IsAlly && unit.Ally != null)
        {
            hp = unit.Ally.Hp;
            maxHp = unit.Ally.MaxHp;
            return maxHp > 0;
        }

        if (!unit.IsAlly && unit.Enemy != null)
        {
            hp = unit.Enemy.Hp;
            maxHp = unit.Enemy.MaxHp;
            return maxHp > 0;
        }

        return false;
    }

    private TimelineUnit GetCurrentHpDisplayUnit()
    {
        if (mainBattleSceneMode)
        {
            List<TimelinePreview> previews = BuildTimelinePreview();
            if (previews.Count > 0 && previews[0].Unit != null)
            {
                return previews[0].Unit;
            }
        }

        return activeUnit != null ? activeUnit : GetCurrentActiveUnit();
    }

    private List<TimelinePreview> BuildTimelinePreview()
    {
        List<TimelineUnit> simulatedUnits = new List<TimelineUnit>();
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].IsAlive)
            {
                simulatedUnits.Add(CreateTimelineUnit(allies[i], null, allies[i].NextReadyTick, i));
            }
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsAlive)
            {
                simulatedUnits.Add(CreateTimelineUnit(null, enemies[i], enemies[i].NextReadyTick, allies.Count + i));
            }
        }

        for (int i = 0; i < skillTimelineActions.Count; i++)
        {
            SkillTimelineAction skillAction = skillTimelineActions[i];
            if (skillAction.IsAlive)
            {
                simulatedUnits.Add(CreateSkillTimelineUnit(skillAction, allies.Count + enemies.Count + i));
            }
        }

        List<TimelinePreview> previews = new List<TimelinePreview>();
        for (int i = 0; i < TimelinePreviewCount && simulatedUnits.Count > 0; i++)
        {
            TimelineUnit next = GetEarliestUnit(simulatedUnits);
            previews.Add(new TimelinePreview { Unit = next, DeltaTick = Mathf.Max(0, next.ReadyTick - currentTick) });
            if (next.IsSkill)
            {
                simulatedUnits.Remove(next);
            }
            else
            {
                next.ReadyTick += GetPreviewLoopDelay(next);
                next.Sequence += 10;
            }
        }

        return previews;
    }

    private int GetPreviewLoopDelay(TimelineUnit unit)
    {
        if (unit == null)
        {
            return BattleActionDelayResolver.Resolve(BattleActionDelayKind.Wait);
        }

        if (unit.IsSkill)
        {
            return unit.SkillAction != null ? unit.SkillAction.Delay : 0;
        }

        if (!unit.IsAlly)
        {
            return BattleActionDelayResolver.ResolveEnemyActionDelay(unit.Enemy != null && unit.Enemy.IsBoss);
        }

        return BattleActionDelayResolver.Resolve(BattleActionDelayKind.NormalCard);
    }

    private Color GetTimelineEntryColor(TimelineUnit unit)
    {
        if (unit == null)
        {
            return Color.gray;
        }

        if (unit.IsSkill)
        {
            return unit.SkillAction != null ? unit.SkillAction.DisplayColor : new Color(0.70f, 1f, 0.36f, 1f);
        }

        return unit.IsAlly ? GetPositionColor(unit.Ally.Position) : GetAttributeColor(unit.Enemy.Attribute);
    }

    private string GetTimelineDisplayName(TimelineUnit unit)
    {
        if (unit == null)
        {
            return "-";
        }

        if (unit.IsSkill)
        {
            return ShortSkillName(unit.SkillAction);
        }

        return unit.IsAlly ? ShortAllyTimelineName(unit.Ally) : ShortEnemyTimelineName(unit.Enemy);
    }

    private string GetTimelineDebugLine(TimelineUnit unit)
    {
        if (unit == null)
        {
            return "-";
        }

        if (unit.IsSkill)
        {
            string owner = unit.SkillAction != null && unit.SkillAction.Owner != null ? ShortPosition(unit.SkillAction.Owner.Position) : "-";
            return "Skill from " + owner;
        }

        return unit.IsAlly ? unit.Ally.Position.ToString() : "Grid " + unit.Enemy.GridPosition.x + "," + unit.Enemy.GridPosition.y;
    }

    private static string GetTimelineCardTypeLabel(TimelineUnit unit)
    {
        if (unit == null)
        {
            return "-";
        }

        if (unit.IsSkill)
        {
            return "SKILL";
        }

        return unit.IsAlly ? "ALLY" : "ENEMY";
    }

    private void RefreshAllies()
    {
        foreach (KeyValuePair<PartyPosition, AllyView> pair in allyViews)
        {
            PartyPosition position = pair.Key;
            AllyView view = pair.Value;
            AllyUnit ally = GetAllyAtPosition(position);
            bool selected = ally != null && selectedAlly == ally;
            bool active = activeUnit != null && activeUnit.IsAlly && activeUnit.Ally == ally;
            view.Panel.sprite = mainBattleSceneMode ? null : GetAllyFrameSprite(position);
            view.Panel.color = view.Panel.sprite != null ? Color.white
                : mainBattleSceneMode ? active
                    ? new Color(1f, 0.88f, 0.22f, 0.16f)
                    : selected ? new Color(0.20f, 0.95f, 1f, 0.14f) : new Color(0.02f, 0.04f, 0.05f, 0.02f)
                : active ? new Color(0.08f, 0.16f, 0.12f, 0.98f)
                : selected ? new Color(0.08f, 0.12f, 0.16f, 0.98f) : new Color(0.028f, 0.055f, 0.07f, 0.96f);
            view.Accent.color = GetPositionColor(position);
            view.SelectedHighlight.gameObject.SetActive(selected);
            view.ActiveHighlight.gameObject.SetActive(active);
            bool targetable = IsAllyPanelTargetable(ally);
            bool danger = ally != null && activeUnit != null && !activeUnit.IsAlly && GetPreferredEnemyAttackTarget() == ally;
            bool hover = allyPanelHoverStates.ContainsKey(position) && allyPanelHoverStates[position];
            bool disabled = ally == null || !ally.IsAlive;
            RefreshAllyPanelOverlays(view, selected, active, targetable, danger, hover, disabled);
            if (useSceneBattleGridPrefabVisuals)
            {
                HideSceneGridOverlay(view);
            }

            if (mainBattleSceneMode)
            {
                view.PositionText.gameObject.SetActive(showDebugLabels);
                view.NameText.gameObject.SetActive(false);
                view.NameText.text = string.Empty;
            }
            else
            {
                view.PositionText.gameObject.SetActive(true);
                view.PositionText.text = ShortPosition(position);
                view.NameText.gameObject.SetActive(true);
                view.NameText.text = ally != null ? GetAllyDisplayName(ally) : "-";
            }
            float hpRate = ally != null && ally.MaxHp > 0 ? Mathf.Clamp01((float)ally.Hp / ally.MaxHp) : 0f;
            view.HpBack.gameObject.SetActive(!mainBattleSceneMode);
            view.HpFill.fillAmount = hpRate;
            view.HpFill.color = hpRate > 0.45f ? new Color(0.28f, 1f, 0.45f, 0.95f) : hpRate > 0.2f ? new Color(1f, 0.75f, 0.22f, 0.95f) : new Color(1f, 0.24f, 0.28f, 0.95f);
            view.Portrait.color = ally != null ? GetPositionColor(position) : new Color(0.2f, 0.2f, 0.2f, 0.7f);
            view.DetailText.gameObject.SetActive(showDebugLabels);
            view.DetailText.text = ally != null
                ? "HP " + ally.Hp + "/" + ally.MaxHp + "   SPD " + ally.Speed + "   " + FormatPositionModifier(position) + "   " + ally.Status
                : "Empty";
            ApplyAllySprite(view, ally, position);
        }
    }

    private void UpdateAllyIdleAnimation()
    {
        if (!mainBattleSceneMode || allySpriteDefinitions.Count == 0)
        {
            return;
        }

        float frameSeconds = Mathf.Max(0.05f, allyIdleFrameSeconds);
        allyIdleTimer += Time.unscaledDeltaTime;
        if (allyIdleTimer < frameSeconds)
        {
            return;
        }

        allyIdleTimer -= frameSeconds;
        allyIdleFrameIndex = (allyIdleFrameIndex + 1) % AllyIdleFrameCount;
        foreach (KeyValuePair<PartyPosition, AllyView> pair in allyViews)
        {
            ApplyAllySprite(pair.Value, GetAllyAtPosition(pair.Key), pair.Key);
        }
    }

    private void UpdateEnemyIdleAnimation()
    {
        if (!mainBattleSceneMode || enemySpriteDefinitions.Count == 0)
        {
            return;
        }

        float frameSeconds = Mathf.Max(0.05f, enemyIdleFrameSeconds);
        enemyIdleTimer += Time.unscaledDeltaTime;
        if (enemyIdleTimer < frameSeconds)
        {
            return;
        }

        enemyIdleTimer -= frameSeconds;
        enemyIdleFrameIndex = (enemyIdleFrameIndex + 1) % EnemyIdleFrameCount;
        RefreshEnemySprites();
    }

    private void ApplyBattleSceneUnifiedPanelGrid()
    {
        if (!BattleSceneUseUnifiedPanelCells || !mainBattleSceneMode || !useSceneBattleGridPrefabVisuals)
        {
            return;
        }

        GameObject grid = FindBattleSceneGridRoot();
        if (grid == null)
        {
            return;
        }

        SpriteRenderer sourceRenderer = grid.GetComponent<SpriteRenderer>();
        if (sourceRenderer != null)
        {
            Sprite frameSprite = LoadBattleSceneUnifiedBoardFrameSprite();
            if (frameSprite != null)
            {
                sourceRenderer.sprite = frameSprite;
                sourceRenderer.enabled = true;
            }
            else
            {
                sourceRenderer.enabled = false;
            }
        }

        Transform visualRoot = grid.transform.Find(BattleSceneUnifiedPanelRootName);
        if (visualRoot == null)
        {
            GameObject visualRootObject = new GameObject(BattleSceneUnifiedPanelRootName);
            visualRoot = visualRootObject.transform;
            visualRoot.SetParent(grid.transform, false);
        }

        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;

        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            DestroyBattleSceneRuntimeObject(visualRoot.GetChild(i).gameObject);
        }

        int sortingLayerId = sourceRenderer != null ? sourceRenderer.sortingLayerID : 0;
        Sprite allyPanelSprite = LoadBattleSceneUnifiedTilePanelSprite(true);
        Sprite enemyPanelSprite = LoadBattleSceneUnifiedTilePanelSprite(false);
        for (int row = 0; row < BattleGridRows; row++)
        {
            for (int column = 0; column < BattleGridTotalCols; column++)
            {
                Sprite panelSprite = column < BattleGridAllyCols ? allyPanelSprite : enemyPanelSprite;
                if (panelSprite == null)
                {
                    panelSprite = LoadBattleSceneUnifiedPanelSprite(row, column);
                }

                if (panelSprite == null)
                {
                    continue;
                }

                GameObject panelObject = new GameObject("UnifiedPanel_R" + row + "_C" + column, typeof(SpriteRenderer));
                panelObject.transform.SetParent(visualRoot, false);
                Vector2 center = GetBattleSceneVisualPanelLocalCenter(row, column);
                panelObject.transform.localPosition = new Vector3(center.x, center.y, 0f);
                panelObject.transform.localRotation = GetBattleSceneVisualPanelLocalRotation(row, column);
                panelObject.transform.localScale = GetBattleSceneVisualPanelSpriteScale(row, column);

                SpriteRenderer renderer = panelObject.GetComponent<SpriteRenderer>();
                renderer.sprite = panelSprite;
                renderer.sortingLayerID = sortingLayerId;
                renderer.sortingOrder = BattleSceneUnifiedPanelSortingOrder + row;
            }
        }

        ApplyBattleSceneUnifiedPanelColliders(grid.transform);
        RepositionBattleSceneUnifiedUnits(grid.transform);
    }

    private static void ApplyBattleSceneUnifiedPanelColliders(Transform grid)
    {
        if (grid == null)
        {
            return;
        }

        Transform collidersRoot = grid.Find("GridColliders");
        if (collidersRoot == null)
        {
            return;
        }

        for (int row = 0; row < BattleGridRows; row++)
        {
            for (int column = 0; column < BattleGridTotalCols; column++)
            {
                Transform cell = collidersRoot.Find(GetBattleScenePanelCellName(row, column));
                if (cell == null)
                {
                    continue;
                }

                Vector2 center;
                Vector2[] colliderPoints = BuildBattleSceneUnifiedPanelLocalPoints(row, column, out center);
                cell.localPosition = new Vector3(center.x, center.y, cell.localPosition.z);
                cell.localRotation = Quaternion.identity;
                cell.localScale = Vector3.one;

                PolygonCollider2D polygon = cell.GetComponent<PolygonCollider2D>();
                if (polygon == null)
                {
                    polygon = cell.gameObject.AddComponent<PolygonCollider2D>();
                }

                polygon.isTrigger = true;
                polygon.pathCount = 1;
                polygon.SetPath(0, colliderPoints);

                Transform anchor = cell.Find("UnitAnchor");
                if (anchor != null)
                {
                    anchor.localPosition = new Vector3(0f, 0.30f, -0.05f);
                    anchor.localRotation = Quaternion.identity;
                    anchor.localScale = Vector3.one;
                }
            }
        }
    }

    private void RepositionBattleSceneUnifiedUnits(Transform grid)
    {
        if (grid == null)
        {
            return;
        }

        Transform units = grid.Find("Units");
        if (units == null)
        {
            return;
        }

        RepositionBattleSceneUnifiedRootUnit(grid, units, "Ally_Front_CyberKnight", 1, 1, new Vector3(0f, 0.27f, 0f));
        RepositionBattleSceneUnifiedRootUnit(grid, units, "Ally_Middle_CyberWolf", 0, 1, new Vector3(0f, 0.19f, 0f));
        RepositionBattleSceneUnifiedRootUnit(grid, units, "Ally_Back_DigitalFairy", 2, 1, new Vector3(0f, 0.22f, 0f));
        RepositionBattleSceneUnifiedVisualUnit(grid, units, "Enemy_DrillMole", 1, BattleGridAllyCols + 1, new Vector3(0f, 0.28f, 0f));
        RepositionBattleSceneUnifiedVisualUnit(grid, units, "Enemy_ElecGecko", 0, BattleGridAllyCols + 2, new Vector3(-0.06f, 0.25f, 0f));
        RepositionBattleSceneUnifiedVisualUnit(grid, units, "Enemy_BladeBug", 2, BattleGridAllyCols + 2, new Vector3(0f, 0.42f, 0f));
    }

    private static void RepositionBattleSceneUnifiedRootUnit(Transform grid, Transform units, string unitName, int row, int globalColumn, Vector3 displayOffset)
    {
        Transform unit = units.Find(unitName);
        Transform anchor = FindBattleSceneUnifiedUnitAnchor(grid, row, globalColumn);
        if (unit == null || anchor == null)
        {
            return;
        }

        unit.localPosition = units.InverseTransformPoint(anchor.position) + displayOffset;
        unit.localRotation = Quaternion.identity;
    }

    private static void RepositionBattleSceneUnifiedVisualUnit(Transform grid, Transform units, string unitName, int row, int globalColumn, Vector3 displayOffset)
    {
        Transform unit = units.Find(unitName);
        Transform anchor = FindBattleSceneUnifiedUnitAnchor(grid, row, globalColumn);
        if (unit == null || anchor == null)
        {
            return;
        }

        unit.localPosition = units.InverseTransformPoint(anchor.position);
        unit.localRotation = Quaternion.identity;
        Transform visual = unit.Find("Visual");
        if (visual != null)
        {
            visual.localPosition = displayOffset;
            visual.localRotation = Quaternion.identity;
        }
        else
        {
            unit.localPosition += displayOffset;
        }
    }

    private static Transform FindBattleSceneUnifiedUnitAnchor(Transform grid, int row, int globalColumn)
    {
        Transform collidersRoot = grid != null ? grid.Find("GridColliders") : null;
        Transform cell = collidersRoot != null ? collidersRoot.Find(GetBattleScenePanelCellName(row, globalColumn)) : null;
        return cell != null ? cell.Find("UnitAnchor") : null;
    }

    private static Vector2[] BuildBattleSceneUnifiedPanelLocalPoints(int row, int globalColumn, out Vector2 center)
    {
        Vector2[] corners = GetBattleSceneUnifiedPanelPixelCorners(row, globalColumn);
        Vector2[] polygonPixels = BuildBattleSceneUnifiedBeveledPixelPolygon(corners, BattleSceneUnifiedColliderTrim);
        Vector2[] gridLocalPoints = new Vector2[polygonPixels.Length];
        center = Vector2.zero;
        for (int i = 0; i < polygonPixels.Length; i++)
        {
            gridLocalPoints[i] = ConvertBattleSceneUnifiedPixelToLocal(polygonPixels[i]);
            center += gridLocalPoints[i];
        }

        center /= gridLocalPoints.Length;
        Vector2[] localPoints = new Vector2[gridLocalPoints.Length];
        for (int i = 0; i < gridLocalPoints.Length; i++)
        {
            localPoints[i] = gridLocalPoints[i] - center;
        }

        return localPoints;
    }

    private static Vector2[] GetBattleSceneUnifiedPanelPixelCorners(int row, int globalColumn)
    {
        int clampedRow = Mathf.Clamp(row, 0, BattleGridRows - 1);
        int clampedColumn = Mathf.Clamp(globalColumn, 0, BattleGridTotalCols - 1);
        return BattleSceneUnifiedPanelCornerPixels[clampedRow * BattleGridTotalCols + clampedColumn];
    }

    private static Vector2[] GetBattleSceneVisualPanelPixelCorners(int row, int globalColumn)
    {
        int clampedRow = Mathf.Clamp(row, 0, BattleGridRows - 1);
        int clampedColumn = Mathf.Clamp(globalColumn, 0, BattleGridTotalCols - 1);
        return BattleSceneVisualPanelCornerPixels[clampedRow * BattleGridTotalCols + clampedColumn];
    }

    private static Vector2 GetBattleSceneUnifiedPanelLocalCenter(int row, int globalColumn)
    {
        return GetBattleScenePanelLocalCenter(GetBattleSceneUnifiedPanelPixelCorners(row, globalColumn));
    }

    private static Vector2 GetBattleSceneVisualPanelLocalCenter(int row, int globalColumn)
    {
        return GetBattleScenePanelLocalCenter(GetBattleSceneVisualPanelPixelCorners(row, globalColumn));
    }

    private static Vector2 GetBattleScenePanelLocalCenter(Vector2[] corners)
    {
        Vector2 center = Vector2.zero;
        for (int i = 0; i < corners.Length; i++)
        {
            center += corners[i];
        }

        center /= corners.Length;
        return ConvertBattleSceneUnifiedPixelToLocal(center);
    }

    private static Quaternion GetBattleSceneVisualPanelLocalRotation(int row, int globalColumn)
    {
        Vector2[] corners = GetBattleSceneVisualPanelPixelCorners(row, globalColumn);
        Vector2 topLeft = corners[0];
        Vector2 topRight = corners[1];
        float pixelAngle = Mathf.Atan2(topRight.y - topLeft.y, topRight.x - topLeft.x) * Mathf.Rad2Deg;
        return Quaternion.Euler(0f, 0f, -pixelAngle);
    }

    private static Vector3 GetBattleSceneVisualPanelSpriteScale(int row, int globalColumn)
    {
        Vector2[] corners = GetBattleSceneVisualPanelPixelCorners(row, globalColumn);
        float topWidth = Vector2.Distance(corners[0], corners[1]);
        float bottomWidth = Vector2.Distance(corners[3], corners[2]);
        float leftHeight = Vector2.Distance(corners[0], corners[3]);
        float rightHeight = Vector2.Distance(corners[1], corners[2]);
        float width = Mathf.Max(1f, (topWidth + bottomWidth) * 0.5f);
        float height = Mathf.Max(1f, (leftHeight + rightHeight) * 0.5f);
        return new Vector3(
            width / BattleSceneVisualPanelBaseWidthPixels,
            height / BattleSceneVisualPanelBaseHeightPixels,
            1f);
    }

    private static Vector2[] BuildBattleSceneUnifiedBeveledPixelPolygon(Vector2[] corners, float trim)
    {
        Vector2 topLeft = corners[0];
        Vector2 topRight = corners[1];
        Vector2 bottomRight = corners[2];
        Vector2 bottomLeft = corners[3];
        return new[]
        {
            Vector2.Lerp(topLeft, topRight, trim),
            Vector2.Lerp(topLeft, topRight, 1f - trim),
            Vector2.Lerp(topRight, bottomRight, trim),
            Vector2.Lerp(topRight, bottomRight, 1f - trim),
            Vector2.Lerp(bottomRight, bottomLeft, trim),
            Vector2.Lerp(bottomRight, bottomLeft, 1f - trim),
            Vector2.Lerp(bottomLeft, topLeft, trim),
            Vector2.Lerp(bottomLeft, topLeft, 1f - trim)
        };
    }

    private static Vector2 ConvertBattleSceneUnifiedPixelToLocal(Vector2 pixel)
    {
        return new Vector2(
            (pixel.x - BattleSceneUnifiedTextureWidthPixels * 0.5f) / BattleSceneUnifiedPixelsPerUnit,
            (BattleSceneUnifiedTextureHeightPixels * 0.5f - pixel.y) / BattleSceneUnifiedPixelsPerUnit);
    }

    private static Sprite LoadBattleSceneUnifiedPanelSprite(int row, int column)
    {
        return LoadFullRectSpriteFromFile(GetBattleSceneUnifiedPanelSpriteAssetPath(row, column), FilterMode.Bilinear);
    }

    private static Sprite LoadBattleSceneUnifiedBoardFrameSprite()
    {
        return LoadFullRectSpriteFromFile(BattleSceneUnifiedBoardFrameAssetPath, FilterMode.Bilinear);
    }

    private static Sprite LoadBattleSceneUnifiedTilePanelSprite(bool allyPanel)
    {
        return LoadFullRectSpriteFromFile(
            allyPanel ? BattleSceneUnifiedTilePanelAllyAssetPath : BattleSceneUnifiedTilePanelEnemyAssetPath,
            FilterMode.Bilinear);
    }

    private static string GetBattleSceneUnifiedPanelSpriteAssetPath(int row, int column)
    {
        return BattleSceneUnifiedPanelAssetFolder + "/UnifiedPanel_R" + row + "_C" + column + ".png";
    }

    private static void DestroyBattleSceneRuntimeObject(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }

    private void CacheSceneBattleGridUnitRenderers()
    {
        sceneBattleGridUnitRenderers.Clear();
        battleSceneEnemyHpTexts.Clear();
        if (!useSceneBattleGridPrefabVisuals)
        {
            return;
        }

        GameObject grid = FindBattleSceneGridRoot();
        Transform units = grid != null ? grid.transform.Find("Units") : null;
        if (units == null)
        {
            return;
        }

        CacheSceneBattleGridUnitRenderer(units, "Ally_Front_CyberKnight");
        CacheSceneBattleGridUnitRenderer(units, "Ally_Middle_CyberWolf");
        CacheSceneBattleGridUnitRenderer(units, "Ally_Back_DigitalFairy");
        CacheSceneBattleGridUnitRenderer(units, "Enemy_DrillMole");
        CacheSceneBattleGridUnitRenderer(units, "Enemy_ElecGecko");
        CacheSceneBattleGridUnitRenderer(units, "Enemy_BladeBug");
        DisableLegacyBattleSceneEnemyHpTexts(units);
    }

    private void CacheSceneBattleGridUnitRenderer(Transform units, string unitName)
    {
        Transform unit = units.Find(unitName);
        DisableLegacyBattleSceneEnemyHpTexts(unit);
        SpriteRenderer renderer = unit != null ? unit.GetComponentInChildren<SpriteRenderer>(true) : null;
        if (renderer != null)
        {
            sceneBattleGridUnitRenderers[unitName] = renderer;
        }
    }

    private TextMeshPro EnsureBattleSceneEnemyHpText(Transform hpRoot, EnemyUnit enemy)
    {
        if (hpRoot == null || enemy == null)
        {
            return null;
        }

        DisableRetiredBattleSceneEnemyHpTexts(hpRoot);
        string hpObjectName = GetBattleSceneEnemyHpObjectName(enemy);
        Transform hpTransform = hpRoot.Find(hpObjectName);
        if (hpTransform == null)
        {
            GameObject hpObject = new GameObject(hpObjectName);
            hpTransform = hpObject.transform;
            hpTransform.SetParent(hpRoot, false);
        }

        TextMeshPro hpText = hpTransform.GetComponent<TextMeshPro>();
        if (hpText == null)
        {
            hpText = hpTransform.gameObject.AddComponent<TextMeshPro>();
        }

        ConfigureBattleSceneEnemyWorldHpText(hpText);
        return hpText;
    }

    private static Transform EnsureBattleSceneEnemyHpRoot()
    {
        GameObject grid = FindBattleSceneGridRoot();
        if (grid == null)
        {
            return null;
        }

        Transform hpRoot = grid.transform.Find(BattleSceneEnemyHpRootName);
        if (hpRoot == null)
        {
            GameObject hpRootObject = new GameObject(BattleSceneEnemyHpRootName);
            hpRoot = hpRootObject.transform;
            hpRoot.SetParent(grid.transform, false);
        }

        hpRoot.localPosition = Vector3.zero;
        hpRoot.localRotation = Quaternion.identity;
        hpRoot.localScale = Vector3.one;
        return hpRoot;
    }

    private static void DisableLegacyBattleSceneEnemyHpTexts(Transform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child == null)
            {
                continue;
            }

            string childName = child.name;
            bool isLegacyHpText =
                childName == BattleSceneEnemyHpTextName ||
                (childName.StartsWith("Enemy_", StringComparison.Ordinal) &&
                childName.EndsWith("_" + BattleSceneEnemyHpTextName, StringComparison.Ordinal));
            if (isLegacyHpText)
            {
                child.gameObject.SetActive(false);
            }

            DisableLegacyBattleSceneEnemyHpTexts(child);
        }
    }

    private static void DisableRetiredBattleSceneEnemyHpTexts(Transform hpRoot)
    {
        if (hpRoot == null)
        {
            return;
        }

        for (int i = 0; i < hpRoot.childCount; i++)
        {
            Transform child = hpRoot.GetChild(i);
            if (child == null)
            {
                continue;
            }

            string childName = child.name;
            bool isRetiredGeneratedHp =
                childName.StartsWith("Enemy_", StringComparison.Ordinal) &&
                childName.EndsWith("_" + BattleSceneEnemyHpTextName, StringComparison.Ordinal);
            bool isUnexpectedHp =
                childName == BattleSceneEnemyHpTextName ||
                (childName.StartsWith(BattleSceneEnemyHpObjectPrefix, StringComparison.Ordinal) &&
                childName != "EnemyHp_70" &&
                childName != "EnemyHp_90" &&
                childName != "EnemyHp_75");
            if (isRetiredGeneratedHp || isUnexpectedHp)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private static string GetBattleSceneEnemyHpObjectName(EnemyUnit enemy)
    {
        if (enemy == null)
        {
            return BattleSceneEnemyHpObjectPrefix + "Unknown";
        }

        int labelNumber = enemy.MaxHp > 0 ? enemy.MaxHp : enemy.Hp;
        return BattleSceneEnemyHpObjectPrefix + Mathf.Max(0, labelNumber);
    }

    private void ApplySceneBattleGridUnitSprites()
    {
        if (!useSceneBattleGridPrefabVisuals || sceneBattleGridUnitRenderers.Count == 0)
        {
            return;
        }

        ApplySceneBattleGridUnitSprite("Ally_Front_CyberKnight");
        ApplySceneBattleGridUnitSprite("Ally_Middle_CyberWolf");
        ApplySceneBattleGridUnitSprite("Ally_Back_DigitalFairy");
        ApplySceneBattleGridUnitSprite("Enemy_DrillMole");
        ApplySceneBattleGridUnitSprite("Enemy_ElecGecko");
        ApplySceneBattleGridUnitSprite("Enemy_BladeBug");
    }

    private void ApplySceneBattleGridUnitSprite(string unitName)
    {
        SpriteRenderer renderer;
        SceneUnitSpriteAnimation animation;
        if (!sceneBattleGridUnitRenderers.TryGetValue(unitName, out renderer)
            || !sceneBattleGridUnitAnimations.TryGetValue(unitName, out animation)
            || animation.IdleFrames == null
            || animation.IdleFrames.Length == 0)
        {
            return;
        }

        int frameIndex = animation.IsEnemy ? enemyIdleFrameIndex : allyIdleFrameIndex;
        Sprite sprite = animation.IdleFrames[Mathf.Abs(frameIndex) % animation.IdleFrames.Length];
        if (sprite != null)
        {
            renderer.sprite = sprite;
        }

        renderer.flipX = animation.FlipX;
    }

    private void RefreshEnemySprites()
    {
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                EnemyCellView view = enemyCellViews[row, column];
                EnemyUnit enemy = GetEnemyAt(row, column);
                if (view != null && view.EnemyRoot != null && view.EnemyRoot.activeSelf && enemy != null && enemy.IsAlive)
                {
                    ApplyEnemySprite(view, enemy);
                }
            }
        }
    }

    private void ApplyAllySprite(AllyView view, AllyUnit ally, PartyPosition slotPosition)
    {
        if (view == null || view.Portrait == null || !mainBattleSceneMode)
        {
            return;
        }

        AllySpriteDefinition definition = GetAllySpriteDefinition(ally);
        if (definition == null)
        {
            view.Portrait.sprite = null;
            view.Portrait.color = ally != null ? GetPositionColor(slotPosition) : new Color(0.2f, 0.2f, 0.2f, 0.7f);
            int fallbackRow = GetAllyGridRow(slotPosition);
            SetAllySpriteRect(view.Portrait.rectTransform, ScaleSpriteForBattleRow(new Vector2(0.72f, 0.76f), fallbackRow), OffsetSpriteForBattleRow(Vector2.zero, fallbackRow));
            return;
        }

        int row = GetAllyGridRow(slotPosition);
        SetAllySpriteRect(view.Portrait.rectTransform, ScaleSpriteForBattleRow(definition.Scale, row), OffsetSpriteForBattleRow(definition.Offset, row));
        Sprite sprite = GetAllyIdleSprite(definition);
        view.Portrait.sprite = sprite;
        view.Portrait.preserveAspect = true;
        view.Portrait.color = sprite != null ? Color.white : definition.FallbackColor;
    }

    private void ApplyEnemySprite(EnemyCellView view, EnemyUnit enemy)
    {
        if (view == null || view.EnemySprite == null || !mainBattleSceneMode)
        {
            return;
        }

        EnemySpriteDefinition definition = GetEnemySpriteDefinition(enemy);
        if (definition == null)
        {
            view.EnemySprite.sprite = null;
            view.EnemySprite.rectTransform.localScale = Vector3.one;
            view.EnemySprite.color = enemy != null ? GetAttributeColor(enemy.Attribute) : new Color(0.2f, 0.2f, 0.2f, 0.7f);
            int fallbackRow = enemy != null ? enemy.GridPosition.x : 1;
            SetEnemySpriteRect(view.EnemySprite.rectTransform, ScaleSpriteForBattleRow(new Vector2(0.72f, 0.70f), fallbackRow), OffsetSpriteForBattleRow(Vector2.zero, fallbackRow));
            return;
        }

        int row = enemy != null ? enemy.GridPosition.x : 1;
        SetEnemySpriteRect(view.EnemySprite.rectTransform, ScaleSpriteForBattleRow(definition.Scale, row), OffsetSpriteForBattleRow(definition.Offset, row));
        Sprite sprite = GetEnemyIdleSprite(definition);
        view.EnemySprite.sprite = sprite;
        view.EnemySprite.preserveAspect = true;
        view.EnemySprite.color = sprite != null ? Color.white : definition.FallbackColor;
        view.EnemySprite.rectTransform.localScale = definition.FlipX ? new Vector3(-1f, 1f, 1f) : Vector3.one;
    }

    private void RefreshAllyPanelOverlays(AllyView view, bool selected, bool active, bool targetable, bool danger, bool hover, bool disabled)
    {
        if (!mainBattleSceneMode || view == null)
        {
            return;
        }

        Sprite selectedSprite = null;
        Sprite targetableSprite = battlePanelSprites != null ? battlePanelSprites.TargetableOverlay : null;
        Sprite dangerSprite = battlePanelSprites != null ? battlePanelSprites.DangerOverlay : null;
        Sprite hoverSprite = battlePanelSprites != null ? battlePanelSprites.HoverOverlay : null;
        Sprite disabledSprite = battlePanelSprites != null ? battlePanelSprites.DisabledOverlay : null;
        ApplyOverlayImage(view.HoverOverlay, hover, hoverSprite, new Color(1f, 1f, 1f, 0.18f));
        ApplyOverlayImage(view.SelectedHighlight, selected, selectedSprite, new Color(0.35f, 0.95f, 1f, 0.36f));
        ApplyOverlayImage(view.TargetableOverlay, targetable && !selected, targetableSprite, new Color(1f, 0.92f, 0.20f, 0.24f));
        ApplyOverlayImage(view.ActiveHighlight, active, targetableSprite, new Color(1f, 0.86f, 0.22f, 0.34f));
        ApplyOverlayImage(view.DangerOverlay, danger, dangerSprite, new Color(1f, 0.22f, 0.22f, 0.36f));
        ApplyOverlayImage(view.DisabledOverlay, disabled, disabledSprite, new Color(0.05f, 0.05f, 0.06f, 0.55f));
    }

    private void HideSceneGridOverlay(AllyView view)
    {
        if (view == null)
        {
            return;
        }

        view.Panel.sprite = null;
        view.Panel.color = Color.clear;
        ApplyOverlayImage(view.HoverOverlay, false, null, Color.clear);
        ApplyOverlayImage(view.SelectedHighlight, false, null, Color.clear);
        ApplyOverlayImage(view.TargetableOverlay, false, null, Color.clear);
        ApplyOverlayImage(view.ActiveHighlight, false, null, Color.clear);
        ApplyOverlayImage(view.DangerOverlay, false, null, Color.clear);
        ApplyOverlayImage(view.DisabledOverlay, false, null, Color.clear);
    }

    private void HideSceneGridOverlay(EnemyCellView view)
    {
        if (view == null)
        {
            return;
        }

        view.Panel.sprite = null;
        view.Panel.color = Color.clear;
        ApplyOverlayImage(view.Highlight, false, null, Color.clear);
        ApplyOverlayImage(view.TargetableOverlay, false, null, Color.clear);
        ApplyOverlayImage(view.DangerOverlay, false, null, Color.clear);
        ApplyOverlayImage(view.HoverOverlay, false, null, Color.clear);
        ApplyOverlayImage(view.DisabledOverlay, false, null, Color.clear);
    }

    private bool IsAllyPanelTargetable(AllyUnit ally)
    {
        if (ally == null || !ally.IsAlive || !IsPlayerTurn())
        {
            return false;
        }

        PrototypeCard card = GetSelectedChipCard();
        return card != null && card.TargetKind == PrototypeTargetKind.Ally;
    }

    private static void SetAllySpriteRect(RectTransform rectTransform, Vector2 scale, Vector2 offset)
    {
        Vector2 clampedScale = ClampSpriteScale(scale);
        Vector2 center = new Vector2(0.5f + offset.x, 0.56f + offset.y);
        center.x = Mathf.Clamp(center.x, clampedScale.x * 0.5f, 1f - clampedScale.x * 0.5f);
        center.y = Mathf.Clamp(center.y, clampedScale.y * 0.5f + 0.08f, 0.96f - clampedScale.y * 0.5f);
        rectTransform.anchorMin = new Vector2(center.x - clampedScale.x * 0.5f, center.y - clampedScale.y * 0.5f);
        rectTransform.anchorMax = new Vector2(center.x + clampedScale.x * 0.5f, center.y + clampedScale.y * 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static void SetEnemySpriteRect(RectTransform rectTransform, Vector2 scale, Vector2 offset)
    {
        Vector2 clampedScale = ClampSpriteScale(scale);
        Vector2 center = new Vector2(0.5f + offset.x, 0.58f + offset.y);
        center.x = Mathf.Clamp(center.x, clampedScale.x * 0.5f, 1f - clampedScale.x * 0.5f);
        center.y = Mathf.Clamp(center.y, clampedScale.y * 0.5f + 0.12f, 0.94f - clampedScale.y * 0.5f);
        rectTransform.anchorMin = new Vector2(center.x - clampedScale.x * 0.5f, center.y - clampedScale.y * 0.5f);
        rectTransform.anchorMax = new Vector2(center.x + clampedScale.x * 0.5f, center.y + clampedScale.y * 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static int GetAllyGridRow(PartyPosition position)
    {
        switch (position)
        {
            case PartyPosition.Middle:
                return 0;
            case PartyPosition.Front:
                return 1;
            default:
                return 2;
        }
    }

    private static Vector2 ScaleSpriteForBattleRow(Vector2 scale, int row)
    {
        float depthScale = GetBattleSpriteRowScale(row);
        return new Vector2(scale.x * depthScale, scale.y * depthScale);
    }

    private static Vector2 OffsetSpriteForBattleRow(Vector2 offset, int row)
    {
        float yOffset = (Mathf.Clamp(row, 0, BattleGridRows - 1) - 1f) * BattleSpriteYOffsetRowDelta;
        return new Vector2(offset.x, offset.y + yOffset);
    }

    private static float GetBattleSpriteRowScale(int row)
    {
        return 1f + (Mathf.Clamp(row, 0, BattleGridRows - 1) - 1f) * BattleSpriteScaleRowDelta;
    }

    private AllySpriteDefinition GetAllySpriteDefinition(AllyUnit ally)
    {
        if (ally == null || string.IsNullOrEmpty(ally.Name))
        {
            return null;
        }

        AllySpriteDefinition definition;
        return allySpriteDefinitions.TryGetValue(ally.Name, out definition) ? definition : null;
    }

    private Sprite GetAllyIdleSprite(AllySpriteDefinition definition)
    {
        if (definition == null || definition.IdleFrames == null || definition.IdleFrames.Length == 0)
        {
            return null;
        }

        int index = Mathf.Abs(allyIdleFrameIndex) % definition.IdleFrames.Length;
        return definition.IdleFrames[index];
    }

    private EnemySpriteDefinition GetEnemySpriteDefinition(EnemyUnit enemy)
    {
        if (enemy == null)
        {
            return null;
        }

        string key = !string.IsNullOrEmpty(enemy.SpriteKey) ? enemy.SpriteKey : enemy.Name;
        EnemySpriteDefinition definition;
        return enemySpriteDefinitions.TryGetValue(key, out definition) ? definition : null;
    }

    private Sprite GetEnemyIdleSprite(EnemySpriteDefinition definition)
    {
        if (definition == null || definition.IdleFrames == null || definition.IdleFrames.Length == 0)
        {
            return null;
        }

        int index = Mathf.Abs(enemyIdleFrameIndex) % definition.IdleFrames.Length;
        return definition.IdleFrames[index];
    }

    private void RefreshEnemies()
    {
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                EnemyCellView view = enemyCellViews[row, column];
                EnemyUnit enemy = GetEnemyAt(row, column);
                bool selected = enemy != null && selectedEnemy == enemy;
                bool active = activeUnit != null && !activeUnit.IsAlly && activeUnit.Enemy == enemy;
                bool targetable = IsPlayerTurn() && enemy != null && enemy.IsAlive && (!mainBattleSceneMode || IsEnemyInCurrentPrototypeAttackRange(enemy));
                if (enemy == null)
                {
                    view.Panel.sprite = mainBattleSceneMode ? GetBattlePanelBaseSprite(false) : GetSpriteOrNull(enemyGridSprites, s => s.Empty);
                    view.Panel.color = view.Panel.sprite != null ? Color.white : mainBattleSceneMode ? new Color(0.02f, 0.04f, 0.05f, 0.02f) : new Color(0.025f, 0.052f, 0.065f, 0.98f);
                    view.Highlight.gameObject.SetActive(false);
                    ApplyOverlayImage(view.TargetableOverlay, false, null, Color.clear);
                    ApplyOverlayImage(view.DangerOverlay, false, null, Color.clear);
                    ApplyOverlayImage(view.HoverOverlay, enemyPanelHoverStates[row, column], battlePanelSprites != null ? battlePanelSprites.HoverOverlay : null, new Color(1f, 1f, 1f, 0.12f));
                    ApplyOverlayImage(view.DisabledOverlay, false, null, Color.clear);
                    view.EnemyRoot.SetActive(false);
                    view.NameText.gameObject.SetActive(false);
                    view.NameText.text = string.Empty;
                    view.Label.gameObject.SetActive(showDebugLabels);
                    view.Label.text = "EMPTY\n[" + row + "," + column + "]";
                    if (useSceneBattleGridPrefabVisuals)
                    {
                        HideSceneGridOverlay(view);
                    }

                    continue;
                }

                view.Panel.sprite = GetEnemyPanelSprite(enemy, selected, active, targetable);
                view.Panel.color = view.Panel.sprite != null ? Color.white
                    : mainBattleSceneMode ? selected
                        ? new Color(1f, 0.45f, 0.55f, 0.16f)
                        : active ? new Color(1f, 0.86f, 0.22f, 0.14f) : new Color(0.02f, 0.04f, 0.05f, 0.02f)
                    : selected ? new Color(0.18f, 0.08f, 0.12f, 0.98f)
                    : new Color(0.07f, 0.035f, 0.048f, 0.98f);
                bool disabled = !enemy.IsAlive;
                Sprite hoverOverlay = mainBattleSceneMode && battlePanelSprites != null ? battlePanelSprites.HoverOverlay : null;
                Sprite selectedOverlay = mainBattleSceneMode ? null : GetSpriteOrNull(enemyGridSprites, s => s.HighlightOverlay);
                Sprite targetableOverlay = mainBattleSceneMode && battlePanelSprites != null ? battlePanelSprites.TargetableOverlay : null;
                Sprite dangerOverlay = mainBattleSceneMode && battlePanelSprites != null ? battlePanelSprites.DangerOverlay : null;
                Sprite disabledOverlay = mainBattleSceneMode && battlePanelSprites != null ? battlePanelSprites.DisabledOverlay : null;
                ApplyOverlayImage(view.HoverOverlay, enemyPanelHoverStates[row, column], hoverOverlay, new Color(1f, 1f, 1f, 0.18f));
                ApplyOverlayImage(view.Highlight, selected, selectedOverlay, new Color(0.2f, 0.95f, 1f, 0.58f));
                ApplyOverlayImage(view.TargetableOverlay, targetable && !selected && !disabled, targetableOverlay, new Color(1f, 0.92f, 0.20f, 0.24f));
                ApplyOverlayImage(view.DangerOverlay, active, dangerOverlay, new Color(1f, 0.22f, 0.22f, 0.36f));
                ApplyOverlayImage(view.DisabledOverlay, disabled, disabledOverlay, new Color(0.05f, 0.05f, 0.06f, 0.55f));
                view.EnemyRoot.SetActive(enemy.IsAlive);
                if (mainBattleSceneMode)
                {
                    ApplyEnemySprite(view, enemy);
                }
                else
                {
                    view.EnemySprite.sprite = GetEnemyUnitSprite(enemy);
                    view.EnemySprite.rectTransform.localScale = Vector3.one;
                    view.EnemySprite.color = view.EnemySprite.sprite != null ? Color.white : GetAttributeColor(enemy.Attribute);
                }
                float hpRate = enemy.MaxHp > 0 ? Mathf.Clamp01((float)enemy.Hp / enemy.MaxHp) : 0f;
                view.HpBack.gameObject.SetActive(!mainBattleSceneMode);
                view.HpFill.fillAmount = hpRate;
                if (mainBattleSceneMode)
                {
                    view.NameText.gameObject.SetActive(!useSceneBattleGridPrefabVisuals && enemy.IsAlive);
                    view.NameText.text = useSceneBattleGridPrefabVisuals ? string.Empty : FormatHpNumber(enemy.Hp);
                    if (!useSceneBattleGridPrefabVisuals)
                    {
                        UpdateEnemyHpNumberPosition(view, enemy);
                    }
                }
                else
                {
                    view.NameText.gameObject.SetActive(enemy.IsAlive);
                    view.NameText.text = showDebugLabels ? enemy.Name : ShortEnemyTimelineName(enemy);
                }
                view.Label.gameObject.SetActive(showDebugLabels);
                view.Label.text = enemy.Name + "\nHP " + enemy.Hp + "/" + enemy.MaxHp + "\n" + enemy.Attribute + " / Weak " + enemy.Weakness + "\n" + enemy.Status;
                if (useSceneBattleGridPrefabVisuals)
                {
                    HideSceneGridOverlay(view);
                }
            }
        }
    }

    private void EnsureBattleSceneEnemyHpOverlay()
    {
        if (!mainBattleSceneMode || useSceneBattleGridPrefabVisuals || battleSceneTimelineRoot == null)
        {
            return;
        }

        bool createdOverlay = battleSceneEnemyHpOverlayRoot == null;
        if (createdOverlay)
        {
            battleSceneEnemyHpOverlayRoot = CreateRect("BattleSceneEnemyHpOverlayRoot", battleSceneTimelineRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            battleSceneEnemyHpOverlayRoot.SetAsLastSibling();
        }

        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                EnemyCellView view = enemyCellViews[row, column];
                if (view != null && view.NameText != null && view.NameText.transform.parent != battleSceneEnemyHpOverlayRoot)
                {
                    view.NameText.rectTransform.SetParent(battleSceneEnemyHpOverlayRoot, false);
                    view.NameText.raycastTarget = false;
                    ConfigureBattleSceneEnemyHpNumberText(view.NameText);
                }
            }
        }
    }

    private void UpdateSceneEnemyHpNumbers()
    {
        if (!mainBattleSceneMode)
        {
            return;
        }

        if (useSceneBattleGridPrefabVisuals)
        {
            UpdateSceneEnemyRootHpTexts();
            return;
        }

        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                EnemyCellView view = enemyCellViews[row, column];
                EnemyUnit enemy = GetEnemyAt(row, column);
                if (view == null || view.NameText == null)
                {
                    continue;
                }

                bool visible = enemy != null && enemy.IsAlive;
                view.NameText.gameObject.SetActive(visible);
                if (!visible)
                {
                    view.NameText.text = string.Empty;
                    continue;
                }

                view.NameText.text = FormatHpNumber(enemy.Hp);
                UpdateEnemyHpNumberPosition(view, enemy);
            }
        }
    }

    private void UpdateSceneEnemyRootHpTexts()
    {
        if (battleSceneEnemyHpTexts.Count == 0)
        {
            CacheSceneBattleGridUnitRenderers();
        }

        GameObject grid = FindBattleSceneGridRoot();
        DisableLegacyBattleSceneEnemyHpTexts(grid != null ? grid.transform.Find("Units") : null);

        Transform hpRoot = EnsureBattleSceneEnemyHpRoot();
        if (hpRoot == null)
        {
            return;
        }

        DisableRetiredBattleSceneEnemyHpTexts(hpRoot);
        HashSet<string> visibleEnemyKeys = new HashSet<string>();
        for (int row = 0; row < 3; row++)
        {
            for (int column = 0; column < 3; column++)
            {
                EnemyCellView view = enemyCellViews[row, column];
                if (view != null && view.NameText != null)
                {
                    view.NameText.gameObject.SetActive(false);
                    view.NameText.text = string.Empty;
                }

                EnemyUnit enemy = GetEnemyAt(row, column);
                if (enemy == null || string.IsNullOrEmpty(enemy.Name))
                {
                    continue;
                }

                string enemyKey = enemy.Name;
                if (!battleSceneEnemyHpTexts.TryGetValue(enemyKey, out TextMeshPro hpText) || hpText == null)
                {
                    hpText = EnsureBattleSceneEnemyHpText(hpRoot, enemy);
                    if (hpText == null)
                    {
                        continue;
                    }

                    battleSceneEnemyHpTexts[enemyKey] = hpText;
                }

                bool visible = enemy != null && enemy.IsAlive;
                hpText.gameObject.SetActive(visible);
                PositionBattleSceneEnemyWorldHpText(hpText, row, column);
                if (visible)
                {
                    hpText.text = FormatHpNumber(enemy.Hp);
                    visibleEnemyKeys.Add(enemyKey);
                }
                else
                {
                    hpText.text = string.Empty;
                }
            }
        }

        foreach (KeyValuePair<string, TextMeshPro> pair in battleSceneEnemyHpTexts)
        {
            if (!visibleEnemyKeys.Contains(pair.Key) && pair.Value != null)
            {
                pair.Value.gameObject.SetActive(false);
            }
        }
    }

    private static void ConfigureBattleSceneEnemyWorldHpText(TextMeshPro hpText)
    {
        if (hpText == null)
        {
            return;
        }

        Transform textTransform = hpText.transform;
        textTransform.localRotation = Quaternion.identity;
        textTransform.localScale = BattleSceneEnemyHpLocalScale;

        RectTransform rectTransform = hpText.rectTransform;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = BattleSceneEnemyHpWorldTextSize;

        hpText.alignment = TextAlignmentOptions.Center;
        hpText.fontSize = 4.75f;
        hpText.fontStyle = FontStyles.Bold;
        hpText.fontWeight = FontWeight.Black;
        hpText.enableWordWrapping = false;
        hpText.richText = false;
        hpText.raycastTarget = false;
        hpText.extraPadding = true;
        hpText.color = Color.white;
        hpText.faceColor = new Color32(255, 255, 255, 255);
        hpText.outlineColor = new Color32(0, 0, 0, 255);
        hpText.outlineWidth = 0.2f;
        ConfigureBattleSceneEnemyWorldHpMaterial(hpText);

        MeshRenderer meshRenderer = hpText.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 260;
        }
    }

    private static void ConfigureBattleSceneEnemyWorldHpMaterial(TextMeshPro hpText)
    {
        if (hpText == null)
        {
            return;
        }

        Material material = hpText.fontMaterial;
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_FaceColor"))
        {
            material.SetColor("_FaceColor", Color.white);
        }

        if (material.HasProperty("_OutlineColor"))
        {
            material.SetColor("_OutlineColor", Color.black);
        }

        if (material.HasProperty("_OutlineWidth"))
        {
            material.SetFloat("_OutlineWidth", 0.2f);
        }

        if (material.HasProperty("_UnderlayColor"))
        {
            material.EnableKeyword("UNDERLAY_ON");
            material.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.72f));
        }

        if (material.HasProperty("_UnderlayOffsetX"))
        {
            material.SetFloat("_UnderlayOffsetX", 0.22f);
        }

        if (material.HasProperty("_UnderlayOffsetY"))
        {
            material.SetFloat("_UnderlayOffsetY", -0.22f);
        }

        if (material.HasProperty("_UnderlaySoftness"))
        {
            material.SetFloat("_UnderlaySoftness", 0.08f);
        }

        hpText.UpdateMeshPadding();
    }

    private static void PositionBattleSceneEnemyWorldHpText(TextMeshPro hpText, int row, int column)
    {
        ConfigureBattleSceneEnemyWorldHpText(hpText);
        if (hpText == null)
        {
            return;
        }

        Transform textTransform = hpText.transform;
        if (TryGetBattleSceneEnemyPanelHpLocalPosition(textTransform.parent, row, column, out Vector3 localPosition))
        {
            textTransform.localPosition = localPosition;
        }
        else
        {
            textTransform.localPosition = BattleSceneEnemyHpFallbackLocalPosition;
        }
    }

    private static bool TryGetBattleSceneEnemyPanelHpLocalPosition(Transform hpParent, int row, int column, out Vector3 localPosition)
    {
        localPosition = BattleSceneEnemyHpFallbackLocalPosition;
        if (hpParent == null)
        {
            return false;
        }

        if (!TryGetBattleSceneEnemyPanelHpWorldPosition(row, column, hpParent.position.z, out Vector3 worldPosition))
        {
            return false;
        }

        localPosition = hpParent.InverseTransformPoint(worldPosition);
        localPosition.z = BattleSceneEnemyHpFallbackLocalPosition.z;
        return true;
    }

    private static bool TryGetBattleSceneEnemyPanelHpWorldPosition(int row, int column, float z, out Vector3 worldPosition)
    {
        worldPosition = Vector3.zero;
        GameObject cellObject = GameObject.Find(GetBattleSceneEnemyCellName(row, column));
        if (cellObject == null)
        {
            return false;
        }

        Transform hpAnchor = cellObject.transform.Find(BattleSceneEnemyHpAnchorName);
        if (hpAnchor != null)
        {
            worldPosition = hpAnchor.position;
            worldPosition.z = z;
            return true;
        }

        Collider2D cellCollider = cellObject.GetComponent<Collider2D>();
        if (cellCollider != null)
        {
            worldPosition = GetBattleSceneEnemyPanelHpWorldPosition(cellCollider.bounds, z);
            return true;
        }

        SpriteRenderer cellRenderer = cellObject.GetComponentInChildren<SpriteRenderer>();
        if (cellRenderer != null)
        {
            worldPosition = GetBattleSceneEnemyPanelHpWorldPosition(cellRenderer.bounds, z);
            return true;
        }

        worldPosition = new Vector3(
            cellObject.transform.position.x,
            cellObject.transform.position.y + BattleSceneEnemyHpFallbackLocalPosition.y,
            z);
        return true;
    }

    private static Vector3 GetBattleSceneEnemyPanelHpWorldPosition(Bounds bounds, float z)
    {
        return new Vector3(
            bounds.center.x,
            bounds.min.y + bounds.size.y * BattleSceneEnemyHpPanelYRatioFromBottom,
            z);
    }

    private static GameObject FindBattleSceneGridRoot()
    {
        GameObject activeRoot = GameObject.Find(BattleGridBottomPrefabName);
        if (activeRoot != null)
        {
            return activeRoot;
        }

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject candidate = allObjects[i];
            if (candidate != null && candidate.name == BattleGridBottomPrefabName && candidate.scene.IsValid())
            {
                return candidate;
            }
        }

        return null;
    }

    private static string GetBattleSceneEnemyCellName(int row, int column)
    {
        return "Enemy_Cell_R" + row + "_C" + column;
    }

    private static string GetBattleScenePanelCellName(int row, int globalColumn)
    {
        int clampedRow = Mathf.Clamp(row, 0, BattleGridRows - 1);
        int clampedColumn = Mathf.Clamp(globalColumn, 0, BattleGridTotalCols - 1);
        if (clampedColumn < BattleGridAllyCols)
        {
            return "Ally_Cell_R" + clampedRow + "_C" + clampedColumn;
        }

        return "Enemy_Cell_R" + clampedRow + "_C" + (clampedColumn - BattleGridAllyCols);
    }

    private void UpdateEnemyHpNumberPosition(EnemyCellView view, EnemyUnit enemy)
    {
        if (!mainBattleSceneMode || view == null || view.NameText == null || enemy == null)
        {
            return;
        }

        EnsureBattleSceneEnemyHpOverlay();
        RectTransform textRect = view.NameText.rectTransform;
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = BattleSceneEnemyHpTextSize;

        if (!TryGetBattleSceneEnemyPanelHpWorldPosition(enemy.GridPosition.x, enemy.GridPosition.y, 0f, out Vector3 worldPoint))
        {
            view.NameText.gameObject.SetActive(false);
            return;
        }

        Camera camera = Camera.main;
        RectTransform parentRect = textRect.parent as RectTransform;
        if (camera == null || parentRect == null)
        {
            return;
        }

        Vector3 screenPoint = camera.WorldToScreenPoint(worldPoint);
        if (screenPoint.z < 0f)
        {
            view.NameText.gameObject.SetActive(false);
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, null, out Vector2 localPoint))
        {
            textRect.anchoredPosition = localPoint;
        }
    }

    private static string GetSceneEnemyUnitName(EnemyUnit enemy)
    {
        if (enemy == null)
        {
            return null;
        }

        switch (!string.IsNullOrEmpty(enemy.SpriteKey) ? enemy.SpriteKey : enemy.Name)
        {
            case "DrillMole":
                return "Enemy_DrillMole";
            case "ElecGecko":
                return "Enemy_ElecGecko";
            case "BladeBug":
                return "Enemy_BladeBug";
            default:
                return null;
        }
    }

    private void RefreshHand()
    {
        for (int i = 0; i < handViews.Count; i++)
        {
            CardButtonView view = handViews[i];
            if (i >= hand.Count || hand[i] == null)
            {
                view.Button.interactable = false;
                if (mainBattleSceneMode && view.Artwork != null)
                {
                    RefreshChipCardView(view, null, false, false, i);
                }
                else
                {
                    view.Panel.color = new Color(0.07f, 0.08f, 0.10f, 0.35f);
                    view.NameText.text = "-";
                    view.DetailText.text = string.Empty;
                }
                continue;
            }

            PrototypeCard card = hand[i];
            bool queued = queuedHandSlots[i];
            view.Button.interactable = IsPlayerTurn() && !queued;
            if (mainBattleSceneMode && view.Artwork != null)
            {
                RefreshChipCardView(view, card, queued, cardSelectOpen && selectedHandIndex == i, i);
            }
            else
            {
                view.Panel.color = queued ? new Color(0.35f, 0.30f, 0.12f, 0.72f) : GetCardColor(card);
                view.NameText.text = (i + 1) + ". " + GetCardDisplayName(card);
                view.DetailText.text = FormatCardMeta(card, showDebugLabels);
            }
        }

        if (deckText != null)
        {
            deckText.text = "Deck " + drawPile.Count + " / Discard " + discardPile.Count + " / " + (loadedSavedDeck ? "Saved" : "Test");
        }

        if (queueText != null)
        {
            queueText.text = FormatQueueText();
        }

        RefreshChipDetail();
    }

    private void RefreshCommands()
    {
        bool canAct = !battleEnded && IsPlayerTurn() && GetFirstAliveEnemy() != null && GetFirstAliveAlly() != null;
        if (weaponButton != null)
        {
            weaponButton.interactable = canAct && GetQueuedActionCost() < MaxQueuedActions;
        }

        if (swapFrontMiddleButton != null)
        {
            swapFrontMiddleButton.interactable = canAct && GetQueuedActionCost() < MaxQueuedActions;
        }

        if (swapMiddleBackButton != null)
        {
            swapMiddleBackButton.interactable = canAct && GetQueuedActionCost() < MaxQueuedActions;
        }

        if (resetButton != null)
        {
            resetButton.interactable = !battleEnded && queuedActions.Count > 0;
        }

        if (confirmButton != null)
        {
            confirmButton.interactable = !battleEnded && activeUnit != null && GetFirstAliveEnemy() != null && GetFirstAliveAlly() != null;
            Text confirmLabel = confirmButton.GetComponentInChildren<Text>();
            if (confirmLabel != null)
            {
                confirmLabel.text = IsPlayerTurn() ? "Confirm" : activeUnit != null && activeUnit.IsSkill ? "Skill Act" : "Enemy Act";
            }
        }

        if (debugButton != null)
        {
            Text debugLabel = debugButton.GetComponentInChildren<Text>();
            if (debugLabel != null)
            {
                debugLabel.text = showDebugLabels ? "DEBUG ON" : "DEBUG";
            }
        }

        if (cardSelectRoot != null)
        {
            cardSelectRoot.SetActive(!mainBattleSceneMode && cardSelectOpen && canAct);
        }

        RefreshBattleSceneCommandPanel();
        RefreshSelectedCommandName();
        RefreshChipQueueSlots();
        RefreshChipDetail();
    }

    private void RefreshBattleSceneCommandPanel()
    {
        if (battleSceneCommandRoot == null)
        {
            return;
        }

        bool canAct = mainBattleSceneMode && !battleEnded && IsPlayerTurn();
        battleSceneCommandRoot.SetActive(canAct);
        if (!canAct)
        {
            HideBattleSceneAttackRangeOverlay();
            return;
        }

        PrototypeAttackDefinition selectedAttack = GetSelectedPrototypeAttack();
        AllyUnit actor = activeUnit != null ? activeUnit.Ally : null;
        EnemyUnit target = ResolvePrototypeAttackTarget(actor, selectedAttack);

        if (battleSceneCommandActorText != null)
        {
            battleSceneCommandActorText.text = actor != null ? "ACTOR  " + actor.Name : "ACTOR  --";
        }

        if (battleSceneCommandTargetText != null)
        {
            bool directTarget = target != null && selectedEnemy == target;
            battleSceneCommandTargetText.text = target != null
                ? "TARGET " + target.Name + "  HP " + target.Hp + (directTarget ? string.Empty : "  RANGE")
                : "TARGET -- OUT OF RANGE";
        }

        if (battleSceneCommandSelectedText != null)
        {
            battleSceneCommandSelectedText.text = selectedAttack.Name + "  POW " + selectedAttack.Damage + "  D" + selectedAttack.Delay + "  " + selectedAttack.Attribute;
        }

        for (int i = 0; i < prototypeAttackViews.Count; i++)
        {
            PrototypeAttackButtonView view = prototypeAttackViews[i];
            if (view == null)
            {
                continue;
            }

            PrototypeAttackDefinition attack = GetPrototypeAttackDefinition(i);
            bool selected = i == selectedPrototypeAttackIndex;
            Color color = selected
                ? new Color(0.88f, 0.64f, 0.16f, 0.98f)
                : new Color(0.065f, 0.092f, 0.110f, 0.96f);
            if (view.Panel != null)
            {
                view.Panel.color = color;
            }

            if (view.Label != null)
            {
                view.Label.text = (i + 1) + "  " + attack.Name + "\nPOW " + attack.Damage + "   DELAY " + attack.Delay + "   " + attack.Attribute;
                view.Label.color = selected ? new Color(0.08f, 0.052f, 0.012f, 1f) : Color.white;
            }

            if (view.Button != null)
            {
                view.Button.interactable = canAct;
                ColorBlock colors = view.Button.colors;
                colors.normalColor = color;
                colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
                colors.pressedColor = Color.Lerp(color, Color.black, 0.24f);
                colors.selectedColor = colors.highlightedColor;
                view.Button.colors = colors;
            }
        }

        if (battleSceneCommandOkButton != null)
        {
            battleSceneCommandOkButton.interactable = canAct && actor != null && target != null;
        }

        if (battleSceneCommandOkImage != null)
        {
            battleSceneCommandOkImage.color = canAct && actor != null && target != null
                ? new Color(0.12f, 0.42f, 0.25f, 0.96f)
                : new Color(0.08f, 0.08f, 0.08f, 0.55f);
        }

        RefreshBattleSceneAttackRangeOverlay();
    }

    private void RefreshBattleSceneAttackRangeOverlay()
    {
        if (!mainBattleSceneMode || !HasBattleSceneAttackRangeVisuals())
        {
            return;
        }

        HideBattleSceneAttackRangeCells();
        if (battleEnded || !IsPlayerTurn() || activeUnit == null || !activeUnit.IsAlly)
        {
            if (battleSceneAttackRangeOverlayRoot != null)
            {
                battleSceneAttackRangeOverlayRoot.gameObject.SetActive(false);
            }

            return;
        }

        int attackIndex = hoveredPrototypeAttackIndex >= 0 ? hoveredPrototypeAttackIndex : selectedPrototypeAttackIndex;
        if (attackIndex < 0 || attackIndex >= BattleScenePrototypeAttackCount)
        {
            if (battleSceneAttackRangeOverlayRoot != null)
            {
                battleSceneAttackRangeOverlayRoot.gameObject.SetActive(false);
            }

            return;
        }

        PrototypeAttackDefinition attack = GetPrototypeAttackDefinition(attackIndex);
        int row = GetAllyGridRow(activeUnit.Ally.Position);
        Color rangeColor = hoveredPrototypeAttackIndex >= 0
            ? new Color(1f, 0.96f, 0.24f, 0.82f)
            : new Color(1f, 0.82f, 0.16f, 0.68f);

        switch (attack.RangePattern)
        {
            case PrototypeAttackRangePattern.RowToEnemyEdge:
                if (SetBattleSceneAttackRangeRow(row, true))
                {
                    break;
                }

                for (int column = BattleScenePrototypeAttackRangeStartColumn; column < BattleGridTotalCols; column++)
                {
                    SetBattleSceneAttackRangeCell(row, column, true, rangeColor);
                }
                break;
        }

        if (battleSceneAttackRangeOverlayRoot != null)
        {
            battleSceneAttackRangeOverlayRoot.gameObject.SetActive(true);
        }
    }

    private void HideBattleSceneAttackRangeOverlay()
    {
        HideBattleSceneAttackRangeCells();
        if (battleSceneAttackRangeOverlayRoot != null)
        {
            battleSceneAttackRangeOverlayRoot.gameObject.SetActive(false);
        }
    }

    private void HideBattleSceneAttackRangeCells()
    {
        for (int row = 0; row < BattleGridRows; row++)
        {
            SetBattleSceneAttackRangeRow(row, false);
        }

        for (int row = 0; row < BattleGridRows; row++)
        {
            for (int column = 0; column < BattleGridTotalCols; column++)
            {
                SetBattleSceneAttackRangeCell(row, column, false, Color.clear);
            }
        }
    }

    private bool SetBattleSceneAttackRangeRow(int row, bool visible)
    {
        if (row < 0 || row >= BattleGridRows)
        {
            return false;
        }

        SpriteRenderer rowRenderer = battleSceneAttackRangeSceneRowRenderers[row];
        if (rowRenderer == null)
        {
            return false;
        }

        rowRenderer.enabled = visible;
        rowRenderer.color = visible ? Color.white : Color.clear;
        return true;
    }

    private void SetBattleSceneAttackRangeCell(int row, int column, bool visible, Color color)
    {
        if (row < 0 || row >= BattleGridRows || column < 0 || column >= BattleGridTotalCols)
        {
            return;
        }

        Image cell = battleSceneAttackRangeCells[row, column];
        if (cell != null)
        {
            cell.color = visible ? color : Color.clear;
            cell.gameObject.SetActive(visible);
        }

        SpriteRenderer spriteRenderer = battleSceneAttackRangeSceneSpriteRenderers[row, column];
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = visible;
            spriteRenderer.color = visible ? Color.white : Color.clear;
        }

        GameObject colliderOverlayRoot = battleSceneAttackRangeColliderOverlayRoots[row, column];
        if (colliderOverlayRoot != null)
        {
            colliderOverlayRoot.SetActive(visible);
        }
    }

    private bool HasBattleSceneAttackRangeVisuals()
    {
        if (battleSceneAttackRangeOverlayRoot != null)
        {
            return true;
        }

        for (int row = 0; row < BattleGridRows; row++)
        {
            if (battleSceneAttackRangeSceneRowRenderers[row] != null)
            {
                return true;
            }
        }

        for (int row = 0; row < BattleGridRows; row++)
        {
            for (int column = 0; column < BattleGridTotalCols; column++)
            {
                if (battleSceneAttackRangeSceneSpriteRenderers[row, column] != null)
                {
                    return true;
                }

                if (battleSceneAttackRangeColliderOverlayRoots[row, column] != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void RefreshChipCardView(CardButtonView view, PrototypeCard card, bool queued, bool selected, int index)
    {
        if (view == null)
        {
            return;
        }

        if (view.Panel != null)
        {
            view.Panel.sprite = null;
            view.Panel.color = card == null
                ? new Color(0.05f, 0.055f, 0.065f, 0.78f)
                : queued ? new Color(0.42f, 0.32f, 0.08f, 1f)
                : selected ? new Color(0.12f, 0.22f, 0.25f, 1f)
                : new Color(0.105f, 0.105f, 0.11f, 1f);
        }

        if (view.NameText != null)
        {
            view.NameText.text = card == null ? "-" : GetCardDisplayName(card);
            view.NameText.color = card == null ? new Color(0.45f, 0.48f, 0.52f, 1f) : Color.white;
        }

        if (view.Artwork != null)
        {
            view.Artwork.sprite = null;
            view.Artwork.color = card == null ? new Color(0.08f, 0.08f, 0.09f, 1f) : GetChipArtworkColor(card, index);
        }

        if (view.AttributeIcon != null)
        {
            view.AttributeIcon.sprite = null;
            view.AttributeIcon.color = card == null ? new Color(0.06f, 0.065f, 0.075f, 1f) : GetAttributeColor(card.Attribute);
        }

        if (view.RankBox != null)
        {
            view.RankBox.sprite = null;
            view.RankBox.color = card != null && card.IsClearCard
                ? new Color(0.10f, 0.28f, 0.16f, 1f)
                : new Color(0.045f, 0.045f, 0.05f, 1f);
        }

        if (view.RankText != null)
        {
            view.RankText.text = card == null ? string.Empty : GetChipRank(card);
        }

        if (view.PowerText != null)
        {
            view.PowerText.text = card == null ? string.Empty : GetChipPowerText(card);
        }

        if (view.DetailText != null)
        {
            view.DetailText.gameObject.SetActive(card != null);
            view.DetailText.text = card == null
                ? string.Empty
                : showDebugLabels ? GetCardEffectLabel(card, true) + " D" + card.ActionDelay : GetChipAttributeCode(card.Attribute);
        }
    }

    private void RefreshChipDetail()
    {
        if (chipDetailNameText == null)
        {
            return;
        }

        PrototypeCard card = GetSelectedChipCard();
        if (chipSelectTitleText != null)
        {
            chipSelectTitleText.text = selectedAlly != null
                ? "BATTLE CHIP  " + ShortPosition(selectedAlly.Position)
                : "BATTLE CHIP";
        }

        chipDetailNameText.text = card == null ? "-" : GetCardDisplayName(card);
        if (chipDetailArtwork != null)
        {
            chipDetailArtwork.sprite = null;
            chipDetailArtwork.color = card == null ? new Color(0.08f, 0.08f, 0.09f, 1f) : GetChipArtworkColor(card, selectedHandIndex);
        }

        if (chipDetailAttributeIcon != null)
        {
            chipDetailAttributeIcon.sprite = null;
            chipDetailAttributeIcon.color = card == null ? new Color(0.06f, 0.065f, 0.075f, 1f) : GetAttributeColor(card.Attribute);
        }

        if (chipDetailRankBox != null)
        {
            chipDetailRankBox.sprite = null;
            chipDetailRankBox.color = card != null && card.IsClearCard
                ? new Color(0.10f, 0.28f, 0.16f, 1f)
                : new Color(0.045f, 0.045f, 0.05f, 1f);
        }

        if (chipDetailMetaText != null)
        {
            chipDetailMetaText.text = card == null ? string.Empty : GetChipRank(card);
        }

        if (chipDetailPowerText != null)
        {
            chipDetailPowerText.text = card == null ? string.Empty : GetChipPowerText(card);
        }
    }

    private void RefreshSelectedCommandName()
    {
        if (selectedCommandNameRoot == null && selectedCommandNameText == null)
        {
            return;
        }

        string commandName = string.Empty;
        if (mainBattleSceneMode && IsPlayerTurn())
        {
            PrototypeAttackDefinition attack = GetSelectedPrototypeAttack();
            commandName = attack != null ? attack.Name : string.Empty;
        }
        else if (cardSelectOpen && IsPlayerTurn())
        {
            PrototypeCard card = GetSelectedChipCard();
            commandName = card != null ? GetCardDisplayName(card) : string.Empty;
        }

        bool visible = !string.IsNullOrEmpty(commandName);
        if (selectedCommandNameText != null)
        {
            selectedCommandNameText.text = commandName;
            selectedCommandNameText.gameObject.SetActive(visible);
        }

        if (selectedCommandActorIcon != null)
        {
            Sprite actorIcon = visible ? GetSelectedCommandActorIconSprite() : null;
            selectedCommandActorIcon.sprite = actorIcon;
            selectedCommandActorIcon.color = actorIcon != null ? Color.white : Color.clear;
            selectedCommandActorIcon.gameObject.SetActive(visible && actorIcon != null);
        }

        if (selectedCommandNameRoot != null)
        {
            selectedCommandNameRoot.gameObject.SetActive(visible);
        }
    }

    private Sprite GetSelectedCommandActorIconSprite()
    {
        if (activeUnit == null || !activeUnit.IsAlly || activeUnit.Ally == null)
        {
            return null;
        }

        Sprite faceIcon = GetTimelineFaceIconSprite(true, allies.IndexOf(activeUnit.Ally));
        if (faceIcon != null)
        {
            return faceIcon;
        }

        return GetAllyIdleSprite(GetAllySpriteDefinition(activeUnit.Ally));
    }

    private void RefreshChipQueueSlots()
    {
        for (int i = 0; i < chipQueueSlotTexts.Count; i++)
        {
            Text label = chipQueueSlotTexts[i];
            if (label == null)
            {
                continue;
            }

            if (i < queuedActions.Count)
            {
                label.text = ShortChipLabel(queuedActions[i].Label);
                label.color = queuedActions[i].ConsumesAction ? new Color(1f, 0.94f, 0.36f, 1f) : new Color(0.45f, 1f, 0.65f, 1f);
            }
            else
            {
                label.text = string.Empty;
            }
        }
    }

    private PrototypeCard GetSelectedChipCard()
    {
        if (hand.Count == 0)
        {
            return null;
        }

        selectedHandIndex = Mathf.Clamp(selectedHandIndex, 0, hand.Count - 1);
        return hand[selectedHandIndex];
    }

    private static string ShortChipLabel(string label)
    {
        if (string.IsNullOrEmpty(label))
        {
            return string.Empty;
        }

        return label.Length <= 6 ? label : label.Substring(0, 6);
    }

    private static string GetChipRank(PrototypeCard card)
    {
        if (card == null)
        {
            return string.Empty;
        }

        switch (card.DeckType)
        {
            case CardDeckType.G:
                return "S";
            case CardDeckType.HC:
                return "A";
            default:
                return card.IsClearCard ? "C" : "B";
        }
    }

    private static string GetChipPowerText(PrototypeCard card)
    {
        if (card == null)
        {
            return string.Empty;
        }

        return card.Power > 0 ? card.Power.ToString() : "--";
    }

    private static string GetChipAttributeCode(CardAttribute attribute)
    {
        switch (attribute)
        {
            case CardAttribute.Slash:
                return "SLASH";
            case CardAttribute.Shot:
                return "SHOT";
            case CardAttribute.Fire:
                return "FIRE";
            case CardAttribute.Ice:
                return "ICE";
            case CardAttribute.Electric:
                return "ELEC";
            case CardAttribute.Water:
                return "WATER";
            case CardAttribute.Grass:
                return "GRASS";
            case CardAttribute.Break:
                return "BREAK";
            default:
                return "NEUTRAL";
        }
    }

    private static Color GetChipArtworkColor(PrototypeCard card, int index)
    {
        if (card == null)
        {
            return new Color(0.08f, 0.08f, 0.09f, 1f);
        }

        Color attribute = GetAttributeColor(card.Attribute);
        Color effect;
        switch (card.Effect)
        {
            case PrototypeCardEffect.RowDamage:
                effect = new Color(0.60f, 0.20f, 0.88f, 1f);
                break;
            case PrototypeCardEffect.PushDamage:
                effect = new Color(1f, 0.56f, 0.18f, 1f);
                break;
            case PrototypeCardEffect.DelayDamage:
                effect = new Color(0.38f, 1f, 0.95f, 1f);
                break;
            case PrototypeCardEffect.Heal:
                effect = new Color(0.38f, 0.92f, 0.38f, 1f);
                break;
            case PrototypeCardEffect.Unsupported:
                effect = new Color(0.42f, 0.44f, 0.48f, 1f);
                break;
            default:
                effect = new Color(0.26f + 0.08f * (index % 3), 0.42f, 0.78f, 1f);
                break;
        }

        return Color.Lerp(attribute, effect, 0.58f);
    }

    private string FormatQueueText()
    {
        if (queuedActions.Count == 0)
        {
            return "Queue: empty / normal actions " + GetQueuedActionCost() + "/" + MaxQueuedActions + " / wait D" + BattleActionDelayResolver.Resolve(BattleActionDelayKind.Wait);
        }

        string text = "Queue: ";
        for (int i = 0; i < queuedActions.Count; i++)
        {
            if (i > 0)
            {
                text += " > ";
            }

            text += queuedActions[i].Label + (queuedActions[i].ConsumesAction ? "" : " [CLEAR]");
        }

        text += " / normal actions " + GetQueuedActionCost() + "/" + MaxQueuedActions + " / total D" + ResolveQueuedActionDelay();
        return text;
    }

    private int GetQueuedActionCost()
    {
        int cost = 0;
        for (int i = 0; i < queuedActions.Count; i++)
        {
            if (queuedActions[i].ConsumesAction)
            {
                cost++;
            }
        }

        return cost;
    }

    private int ResolveQueuedActionDelay()
    {
        if (queuedActions.Count == 0)
        {
            return BattleActionDelayResolver.Resolve(BattleActionDelayKind.Wait);
        }

        int totalDelay = 0;
        for (int i = 0; i < queuedActions.Count; i++)
        {
            totalDelay += ResolveQueuedActionDelay(queuedActions[i]);
        }

        return Mathf.Max(1, totalDelay);
    }

    private int ResolveQueuedActionDelay(QueuedAction action)
    {
        if (action == null)
        {
            return BattleActionDelayResolver.Resolve(BattleActionDelayKind.Wait);
        }

        switch (action.Kind)
        {
            case ActionKind.Card:
                return action.Card != null && action.Card.ActionDelay > 0
                    ? action.Card.ActionDelay
                    : BattleActionDelayResolver.Resolve(BattleActionDelayKind.NormalCard);
            case ActionKind.Weapon:
                return BattleActionDelayResolver.Resolve(BattleActionDelayKind.Weapon);
            case ActionKind.Swap:
                return BattleActionDelayResolver.Resolve(BattleActionDelayKind.Swap);
            default:
                return BattleActionDelayResolver.Resolve(BattleActionDelayKind.Wait);
        }
    }

    private bool IsPlayerTurn()
    {
        return !battleEnded && activeUnit != null && activeUnit.IsAlly && GetFirstAliveEnemy() != null && GetFirstAliveAlly() != null;
    }

    private TimelineUnit GetCurrentActiveUnit()
    {
        List<TimelineUnit> units = new List<TimelineUnit>();
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].IsAlive)
            {
                units.Add(CreateTimelineUnit(allies[i], null, allies[i].NextReadyTick, i));
            }
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsAlive)
            {
                units.Add(CreateTimelineUnit(null, enemies[i], enemies[i].NextReadyTick, allies.Count + i));
            }
        }

        for (int i = 0; i < skillTimelineActions.Count; i++)
        {
            SkillTimelineAction skillAction = skillTimelineActions[i];
            if (skillAction.IsAlive)
            {
                units.Add(CreateSkillTimelineUnit(skillAction, allies.Count + enemies.Count + i));
            }
        }

        return GetEarliestUnit(units);
    }

    private TimelineUnit CreateTimelineUnit(AllyUnit ally, EnemyUnit enemy, int nextActTick, int sequence)
    {
        bool isAlly = ally != null;
        BattleTimelineEntry entry = new BattleTimelineEntry
        {
            UnitId = isAlly ? ally.Name : enemy != null ? enemy.Name : "Unknown",
            UnitName = isAlly ? GetAllyDisplayName(ally) : ShortEnemyName(enemy),
            EntryType = isAlly ? ActionTimelineEntryType.Ally : ActionTimelineEntryType.Enemy,
            IsAlly = isAlly,
            IsEnemy = !isAlly,
            IsSkill = false,
            IsWeapon = false,
            NextActTick = nextActTick,
            Speed = isAlly ? ally.Speed : enemy != null ? enemy.Speed : 0,
            Delay = isAlly ? BattleActionDelayResolver.Resolve(BattleActionDelayKind.NormalCard) : BattleActionDelayResolver.ResolveEnemyActionDelay(enemy != null && enemy.IsBoss),
            IsAlive = isAlly ? ally.IsAlive : enemy != null && enemy.IsAlive,
            IsActive = activeUnit != null && ((isAlly && activeUnit.Ally == ally) || (!isAlly && activeUnit.Enemy == enemy)),
            DisplayColor = isAlly ? GetPositionColor(ally.Position) : enemy != null ? GetAttributeColor(enemy.Attribute) : Color.gray,
            CurrentState = isAlly ? ally.Status : enemy != null ? enemy.Status : string.Empty,
            OwnerUnit = isAlly ? (object)ally : enemy,
            ActionData = null
        };

        return new TimelineUnit
        {
            Ally = ally,
            Enemy = enemy,
            IsAlly = isAlly,
            IsSkill = false,
            ReadyTick = nextActTick,
            Sequence = sequence,
            Entry = entry
        };
    }

    private TimelineUnit CreateSkillTimelineUnit(SkillTimelineAction skillAction, int sequence)
    {
        BattleTimelineEntry entry = new BattleTimelineEntry
        {
            UnitId = skillAction.Id,
            UnitName = skillAction.DisplayName,
            EntryType = ActionTimelineEntryType.Skill,
            IsAlly = false,
            IsEnemy = false,
            IsSkill = true,
            IsWeapon = false,
            NextActTick = skillAction.NextReadyTick,
            Speed = 0,
            Delay = skillAction.Delay,
            IsAlive = skillAction.IsAlive,
            IsActive = activeUnit != null && activeUnit.SkillAction == skillAction,
            DisplayColor = skillAction.DisplayColor,
            CurrentState = skillAction.Status,
            OwnerUnit = skillAction.Owner,
            ActionData = skillAction
        };

        return new TimelineUnit
        {
            SkillAction = skillAction,
            IsAlly = false,
            IsSkill = true,
            ReadyTick = skillAction.NextReadyTick,
            Sequence = sequence,
            Entry = entry
        };
    }

    private TimelineUnit GetEarliestUnit(List<TimelineUnit> units)
    {
        TimelineUnit next = null;
        for (int i = 0; i < units.Count; i++)
        {
            TimelineUnit candidate = units[i];
            if (next == null
                || candidate.ReadyTick < next.ReadyTick
                || (candidate.ReadyTick == next.ReadyTick && candidate.Sequence < next.Sequence))
            {
                next = candidate;
            }
        }

        return next;
    }

    private AllyUnit GetAllyAtPosition(PartyPosition position)
    {
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].Position == position && allies[i].IsAlive)
            {
                return allies[i];
            }
        }

        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].Position == position)
            {
                return allies[i];
            }
        }

        return null;
    }

    private AllyUnit GetFirstAliveAlly()
    {
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].IsAlive)
            {
                return allies[i];
            }
        }

        return null;
    }

    private AllyUnit GetPreferredEnemyAttackTarget()
    {
        PartyPosition[] positions = { PartyPosition.Front, PartyPosition.Middle, PartyPosition.Back };
        for (int i = 0; i < positions.Length; i++)
        {
            AllyUnit ally = GetAliveAllyAtPosition(positions[i]);
            if (ally != null)
            {
                return ally;
            }
        }

        return null;
    }

    private AllyUnit GetAliveAllyAtPosition(PartyPosition position)
    {
        for (int i = 0; i < allies.Count; i++)
        {
            if (allies[i].Position == position && allies[i].IsAlive)
            {
                return allies[i];
            }
        }

        return null;
    }

    private EnemyUnit GetEnemyAt(int row, int column)
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            EnemyUnit enemy = enemies[i];
            if (enemy.GridPosition.x == row && enemy.GridPosition.y == column)
            {
                return enemy;
            }
        }

        return null;
    }

    private EnemyUnit GetFirstAliveEnemy()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].IsAlive)
            {
                return enemies[i];
            }
        }

        return null;
    }

    private static string GetCardDisplayName(PrototypeCard card)
    {
        if (card == null)
        {
            return "-";
        }

        return string.IsNullOrEmpty(card.Name) ? card.CardId : card.Name;
    }

    private static string FormatCardMeta(PrototypeCard card, bool includeDebug)
    {
        if (card == null)
        {
            return string.Empty;
        }

        string clear = card.IsClearCard ? " CLEAR" : string.Empty;
        string unsupported = includeDebug && card.IsUnsupported ? " WIP" : string.Empty;
        return card.DeckType + clear + unsupported + "\n" + card.Attribute + " / " + GetCardEffectLabel(card, includeDebug) + " / D" + card.ActionDelay;
    }

    private static Color GetCardColor(PrototypeCard card)
    {
        if (card == null)
        {
            return new Color(0.10f, 0.12f, 0.15f, 0.55f);
        }

        if (card.IsClearCard)
        {
            return new Color(0.16f, 0.32f, 0.24f, 0.62f);
        }

        switch (card.DeckType)
        {
            case CardDeckType.HC:
                return new Color(0.14f, 0.25f, 0.42f, 0.62f);
            case CardDeckType.G:
                return new Color(0.38f, 0.16f, 0.18f, 0.62f);
            default:
                return new Color(0.13f, 0.17f, 0.22f, 0.62f);
        }
    }

    private static string GetCardEffectLabel(PrototypeCard card, bool includeDebug)
    {
        switch (card.Effect)
        {
            case PrototypeCardEffect.SingleDamage:
                return "Single " + card.Power;
            case PrototypeCardEffect.RowDamage:
                return "Row " + card.Power;
            case PrototypeCardEffect.PushDamage:
                return "Push " + card.Power;
            case PrototypeCardEffect.DelayDamage:
                return "Delay " + card.Power;
            case PrototypeCardEffect.Heal:
                return "Heal " + card.Power;
            case PrototypeCardEffect.Unsupported:
                return includeDebug ? "Unsupported" : "No effect";
            default:
                return card.Effect + " " + card.Power;
        }
    }

    private static Sprite LoadNamedPreviewSprite(string assetPath, params string[] spriteNames)
    {
#if UNITY_EDITOR
        UnityEngine.Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
        for (int i = 0; i < spriteNames.Length; i++)
        {
            string spriteName = spriteNames[i];
            for (int j = 0; j < assets.Length; j++)
            {
                Sprite sprite = assets[j] as Sprite;
                if (sprite != null && string.Equals(sprite.name, spriteName, StringComparison.OrdinalIgnoreCase))
                {
                    return sprite;
                }
            }
        }

        Sprite importedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (importedSprite != null)
        {
            return importedSprite;
        }
#endif

        return LoadPreviewSprite(assetPath);
    }

    private static Sprite[] LoadSpritesFromImportedSheet(string assetPath, int expectedCount)
    {
#if UNITY_EDITOR
        UnityEngine.Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
        List<Sprite> sprites = new List<Sprite>();
        for (int i = 0; i < assets.Length; i++)
        {
            Sprite sprite = assets[i] as Sprite;
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }

        if (sprites.Count > 0)
        {
            sprites.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
            return sprites.ToArray();
        }

        Sprite importedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (importedSprite != null && expectedCount <= 1)
        {
            return new[] { importedSprite };
        }
#endif

        return new Sprite[0];
    }

    private static Sprite[] LoadIndividualSpriteFiles(string[] assetPaths)
    {
        List<Sprite> sprites = new List<Sprite>();
        for (int i = 0; i < assetPaths.Length; i++)
        {
            Sprite sprite = LoadSpriteFromFile(assetPaths[i]);
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }

        return sprites.ToArray();
    }

    private static Sprite[] SliceSpriteSheetFromFile(string assetPath, int columns, int rows)
    {
        Texture2D texture = LoadTextureFromAssetFile(assetPath);
        if (texture == null || columns <= 0 || rows <= 0)
        {
            return new Sprite[0];
        }

        int frameWidth = texture.width / columns;
        int frameHeight = texture.height / rows;
        if (frameWidth <= 0 || frameHeight <= 0)
        {
            UnityEngine.Object.Destroy(texture);
            return new Sprite[0];
        }

        List<Sprite> frames = new List<Sprite>();
        for (int row = rows - 1; row >= 0; row--)
        {
            for (int column = 0; column < columns; column++)
            {
                Rect rect = new Rect(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
                frames.Add(Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect));
            }
        }

        return frames.ToArray();
    }

    private static Sprite LoadSpriteFromFile(string assetPath)
    {
        Texture2D texture = LoadTextureFromAssetFile(assetPath);
        if (texture == null)
        {
            return null;
        }

        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    private static Sprite LoadFullRectSpriteFromFile(string assetPath, FilterMode filterMode)
    {
        string fullPath = GetFullAssetFilePath(assetPath);
        if (!System.IO.File.Exists(fullPath))
        {
            return null;
        }

        byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(bytes))
        {
            UnityEngine.Object.Destroy(texture);
            Debug.LogWarning("BattleScene failed to load full rect sprite texture: " + assetPath);
            return null;
        }

        texture.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = filterMode;
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    private static Sprite LoadOptionalSprite(string assetPath)
    {
#if UNITY_EDITOR
        Sprite importedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (importedSprite != null)
        {
            return importedSprite;
        }
#endif

        return LoadSpriteFromFile(assetPath);
    }

    private static Texture2D LoadTextureFromAssetFile(string assetPath)
    {
        string fullPath = GetFullAssetFilePath(assetPath);
        if (!System.IO.File.Exists(fullPath))
        {
            return null;
        }

        byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(bytes))
        {
            UnityEngine.Object.Destroy(texture);
            Debug.LogWarning("BattleScene failed to load ally sprite texture: " + assetPath);
            return null;
        }

        texture.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;
        ApplyMagentaTransparency(texture);
        return texture;
    }

    private static string GetFullAssetFilePath(string assetPath)
    {
        string relativePath = assetPath.StartsWith("Assets/", StringComparison.Ordinal)
            ? assetPath.Substring("Assets/".Length)
            : assetPath;
        return System.IO.Path.Combine(Application.dataPath, relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
    }

    private static void ApplyMagentaTransparency(Texture2D texture)
    {
        Color32[] pixels = texture.GetPixels32();
        bool changed = false;
        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 pixel = pixels[i];
            if (pixel.r >= 220 && pixel.g <= 70 && pixel.b >= 220)
            {
                pixel.a = 0;
                pixels[i] = pixel;
                changed = true;
            }
        }

        if (changed)
        {
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
        }
    }

    private static Sprite LoadPreviewSprite(string assetPath)
    {
#if UNITY_EDITOR
        Sprite importedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (importedSprite != null)
        {
            return importedSprite;
        }
#endif

        string relativePath = assetPath.StartsWith("Assets/", StringComparison.Ordinal)
            ? assetPath.Substring("Assets/".Length)
            : assetPath;
        string fullPath = System.IO.Path.Combine(Application.dataPath, relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
        if (!System.IO.File.Exists(fullPath))
        {
            Debug.LogWarning("BattleTimelinePrototypeScene preview art not found: " + assetPath);
            return null;
        }

        byte[] bytes = System.IO.File.ReadAllBytes(fullPath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(bytes))
        {
            UnityEngine.Object.Destroy(texture);
            Debug.LogWarning("BattleTimelinePrototypeScene failed to load preview art: " + assetPath);
            return null;
        }

        texture.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
    }

    private static Sprite GetSpriteOrNull<T>(T spriteSet, Func<T, Sprite> selector) where T : class
    {
        if (spriteSet == null || selector == null)
        {
            return null;
        }

        return selector(spriteSet);
    }

    private Sprite GetTimelineFrameSprite(bool ally, bool active, bool selected, bool done)
    {
        if (timelineSprites == null)
        {
            return null;
        }

        if (ally)
        {
            if (active && timelineSprites.AllyActive != null)
            {
                return timelineSprites.AllyActive;
            }

            if (selected && timelineSprites.AllySelected != null)
            {
                return timelineSprites.AllySelected;
            }

            return done && timelineSprites.AllyDone != null ? timelineSprites.AllyDone : timelineSprites.AllyNormal;
        }

        if (active && timelineSprites.EnemyActive != null)
        {
            return timelineSprites.EnemyActive;
        }

        if (selected && timelineSprites.EnemySelected != null)
        {
            return timelineSprites.EnemySelected;
        }

        return done && timelineSprites.EnemyDone != null ? timelineSprites.EnemyDone : timelineSprites.EnemyNormal;
    }

    private Sprite GetTimelineUnitIconSprite(bool ally, bool active, bool selected)
    {
        if (timelineSprites == null)
        {
            return null;
        }

        if (ally)
        {
            return active && timelineSprites.AllyActive != null
                ? timelineSprites.AllyActive
                : selected && timelineSprites.AllySelected != null ? timelineSprites.AllySelected : timelineSprites.AllyNormal;
        }

        return active && timelineSprites.EnemyActive != null
            ? timelineSprites.EnemyActive
            : selected && timelineSprites.EnemySelected != null ? timelineSprites.EnemySelected : timelineSprites.EnemyNormal;
    }

    private Sprite GetTimelineFaceIconSprite(TimelineUnit unit)
    {
        if (unit == null || unit.IsSkill)
        {
            return null;
        }

        if (unit.IsAlly)
        {
            return GetTimelineFaceIconSprite(true, allies.IndexOf(unit.Ally));
        }

        return GetTimelineFaceIconSprite(false, enemies.IndexOf(unit.Enemy));
    }

    private Sprite GetTimelineFaceIconSprite(bool isAlly, int unitIndex)
    {
        int spriteIndex = isAlly ? unitIndex : unitIndex + 3;
        if (timelineFaceIconSprites == null || spriteIndex < 0 || spriteIndex >= timelineFaceIconSprites.Length)
        {
            return null;
        }

        return timelineFaceIconSprites[spriteIndex];
    }

    private Sprite GetTimelineUnitIconSprite(TimelineUnit unit, bool ally, bool active, bool selected)
    {
        if (mainBattleSceneMode && unit != null && !unit.IsSkill)
        {
            Sprite sprite = GetTimelineFaceIconSprite(unit);
            if (sprite != null)
            {
                return sprite;
            }

            sprite = unit.IsAlly
                ? GetAllyIdleSprite(GetAllySpriteDefinition(unit.Ally))
                : GetEnemyIdleSprite(GetEnemySpriteDefinition(unit.Enemy));
            if (sprite != null)
            {
                return sprite;
            }
        }

        return unit != null && unit.IsSkill ? null : GetTimelineUnitIconSprite(ally, active, selected);
    }

    private Vector3 GetTimelineUnitIconScale(TimelineUnit unit)
    {
        if (GetTimelineFaceIconSprite(unit) != null)
        {
            return Vector3.one;
        }

        if (!mainBattleSceneMode || unit == null || unit.IsSkill || unit.IsAlly)
        {
            return Vector3.one;
        }

        EnemySpriteDefinition definition = GetEnemySpriteDefinition(unit.Enemy);
        return definition != null && definition.FlipX ? new Vector3(-1f, 1f, 1f) : Vector3.one;
    }

    private Sprite GetAllyFrameSprite(PartyPosition position)
    {
        if (allySprites == null)
        {
            return null;
        }

        switch (position)
        {
            case PartyPosition.Front:
                return allySprites.FrontFrame;
            case PartyPosition.Middle:
                return allySprites.MiddleFrame;
            default:
                return allySprites.BackFrame;
        }
    }

    private Sprite GetEnemyPanelSprite(EnemyUnit enemy, bool selected, bool active, bool targetable)
    {
        if (mainBattleSceneMode)
        {
            return GetBattlePanelBaseSprite(false);
        }

        if (enemyGridSprites == null)
        {
            return null;
        }

        if (active && enemyGridSprites.Danger != null)
        {
            return enemyGridSprites.Danger;
        }

        if (selected && enemyGridSprites.Selected != null)
        {
            return enemyGridSprites.Selected;
        }

        if (targetable && enemyGridSprites.Targetable != null)
        {
            return enemyGridSprites.Targetable;
        }

        if (enemy != null)
        {
            if (enemy.Attribute == CardAttribute.Ice && enemyGridSprites.Ice != null)
            {
                return enemyGridSprites.Ice;
            }

            if (enemy.Attribute == CardAttribute.Fire && enemyGridSprites.Magma != null)
            {
                return enemyGridSprites.Magma;
            }
        }

        return enemyGridSprites.Normal;
    }

    private Sprite GetEnemyUnitSprite(EnemyUnit enemy)
    {
        if (enemy == null || enemySprites == null)
        {
            return null;
        }

        if (enemy.Attribute == CardAttribute.Fire && enemySprites.FireEnemy != null)
        {
            return enemySprites.FireEnemy;
        }

        if (enemy.Attribute == CardAttribute.Ice && enemySprites.IceEnemy != null)
        {
            return enemySprites.IceEnemy;
        }

        return enemySprites.NormalEnemy;
    }

    private static string GetAllyDisplayName(AllyUnit ally)
    {
        if (ally == null)
        {
            return "-";
        }

        if (ally.Name == "AllyFront")
        {
            return "Ally A";
        }

        if (ally.Name == "AllyMiddle")
        {
            return "Ally B";
        }

        if (ally.Name == "AllyBack")
        {
            return "Ally C";
        }

        return ally.Name;
    }

    private static string ShortAllyTimelineName(AllyUnit ally)
    {
        if (ally == null)
        {
            return "-";
        }

        if (ally.Name == "AllyFront")
        {
            return "A";
        }

        if (ally.Name == "AllyMiddle")
        {
            return "B";
        }

        if (ally.Name == "AllyBack")
        {
            return "C";
        }

        return !string.IsNullOrEmpty(ally.Name) ? ally.Name.Substring(0, 1) : "-";
    }

    private static string FormatHpNumber(int hp)
    {
        return Mathf.Max(0, hp).ToString();
    }

    private static string ShortEnemyTimelineName(EnemyUnit enemy)
    {
        if (enemy == null)
        {
            return "-";
        }

        if (enemy.Name == "Enemy1")
        {
            return "E1";
        }

        if (enemy.Name == "Enemy2")
        {
            return "E2";
        }

        if (enemy.Name == "Enemy3")
        {
            return "E3";
        }

        return ShortEnemyName(enemy);
    }

    private static string ShortEnemyName(EnemyUnit enemy)
    {
        if (enemy == null)
        {
            return "-";
        }

        if (enemy.Name == "Enemy1")
        {
            return "Enemy 1";
        }

        if (enemy.Name == "Enemy2")
        {
            return "Enemy 2";
        }

        if (enemy.Name == "Enemy3")
        {
            return "Enemy 3";
        }

        if (enemy.Attribute == CardAttribute.Fire)
        {
            return "FIRE";
        }

        if (enemy.Attribute == CardAttribute.Ice)
        {
            return "ICE";
        }

        return "ENEMY";
    }

    private static string ShortSkillName(SkillTimelineAction skillAction)
    {
        if (skillAction == null || string.IsNullOrEmpty(skillAction.DisplayName))
        {
            return "SK";
        }

        if (skillAction.DisplayName.Length <= 5)
        {
            return skillAction.DisplayName;
        }

        return skillAction.DisplayName.Substring(0, 5);
    }

    private static Color GetAttributeColor(CardAttribute attribute)
    {
        switch (attribute)
        {
            case CardAttribute.Fire:
                return new Color(1f, 0.34f, 0.18f, 1f);
            case CardAttribute.Ice:
                return new Color(0.35f, 0.84f, 1f, 1f);
            case CardAttribute.Electric:
                return new Color(1f, 0.92f, 0.22f, 1f);
            case CardAttribute.Water:
                return new Color(0.2f, 0.56f, 1f, 1f);
            default:
                return new Color(0.86f, 0.95f, 1f, 1f);
        }
    }

    private void SetOptionalDebugText(Text text)
    {
        if (text != null)
        {
            text.gameObject.SetActive(showDebugLabels);
        }
    }

    private static void SetActiveIfPresent(Text text, bool active)
    {
        if (text != null)
        {
            text.gameObject.SetActive(active);
        }
    }

    private static void SetImageColorIfPresent(Image image, Color color)
    {
        if (image != null)
        {
            image.color = color;
        }
    }

    private Sprite GetBattlePanelBaseSprite(bool playerSide)
    {
        if (HasImage2BattleGridArt())
        {
            return null;
        }

        if (battlePanelSprites == null)
        {
            return null;
        }

        return playerSide ? battlePanelSprites.PlayerNormal : battlePanelSprites.EnemyNormal;
    }

    private bool HasImage2BattleGridArt()
    {
        return mainBattleSceneMode && battleGridFullImage2Sprite != null;
    }

    private static void ApplyOverlayImage(Image image, bool visible, Sprite sprite, Color fallbackColor)
    {
        if (image == null)
        {
            return;
        }

        image.gameObject.SetActive(visible);
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.color = sprite != null ? Color.white : fallbackColor;
    }

    private static Color GetPositionColor(PartyPosition position)
    {
        switch (position)
        {
            case PartyPosition.Front:
                return new Color(0.18f, 0.95f, 1f, 1f);
            case PartyPosition.Middle:
                return new Color(0.68f, 1f, 0.28f, 1f);
            default:
                return new Color(1f, 0.78f, 0.24f, 1f);
        }
    }

    private static string ShortPosition(PartyPosition position)
    {
        switch (position)
        {
            case PartyPosition.Front:
                return "F";
            case PartyPosition.Middle:
                return "M";
            default:
                return "B";
        }
    }

    private RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        return CreatePanel(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero, color);
    }

    private RectTransform CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        RectTransform rectTransform = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
        Image image = rectTransform.gameObject.AddComponent<Image>();
        image.color = color;
        return rectTransform;
    }

    private RectTransform CreateBattleFieldRoot(Transform parent)
    {
        RectTransform rectTransform = CreateRect("Battle Field Root", parent, BattleGridAnchor, BattleGridAnchor, BattleFieldOffsetMin, BattleFieldOffsetMax);
        rectTransform.localEulerAngles = new Vector3(BattleFieldTiltDegrees, 0f, 0f);
        rectTransform.localScale = Vector3.one;
        return rectTransform;
    }

    private static void OrientBattleBillboard(RectTransform rectTransform)
    {
        if (rectTransform != null)
        {
            rectTransform.localEulerAngles = new Vector3(-BattleFieldTiltDegrees, 0f, 0f);
        }
    }

    private RectTransform CreateBattleGridSidePanel(string name, Transform parent, bool allySide)
    {
        int startColumn = allySide ? 0 : BattleGridAllyCols;
        int columns = allySide ? BattleGridAllyCols : BattleGridEnemyCols;
        Vector2 offsetMin = BattleGridOrigin + new Vector2(BattleGridTileSize * startColumn, 0f);
        Vector2 offsetMax = offsetMin + new Vector2(BattleGridTileSize * columns, BattleGridTileSize * BattleGridRows);
        return CreatePanel(name, parent, BattleGridAnchor, BattleGridAnchor, offsetMin, offsetMax, new Color(0.014f, 0.032f, 0.04f, 0f));
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

    private static void RegisterHoverEvents(GameObject target, Action<bool> setHover)
    {
        if (target == null || setHover == null)
        {
            return;
        }

        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = target.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener(_ => setHover(true));
        trigger.triggers.Add(enter);

        EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener(_ => setHover(false));
        trigger.triggers.Add(exit);
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
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 9;
        label.resizeTextMaxSize = fontSize;

        Shadow shadow = rectTransform.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return label;
    }

    private static void ConfigureHpNumberText(Text label)
    {
        if (label == null)
        {
            return;
        }

        label.fontSize = 24;
        label.resizeTextMinSize = 16;
        label.resizeTextMaxSize = 24;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.color = new Color(1f, 0.96f, 0.78f, 1f);

        Outline outline = label.GetComponent<Outline>();
        if (outline == null)
        {
            outline = label.gameObject.AddComponent<Outline>();
        }

        outline.effectColor = new Color(0f, 0f, 0f, 0.86f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);
    }

    private static void ConfigureBattleSceneEnemyHpNumberText(Text label)
    {
        ConfigureHpNumberText(label);
        if (label == null)
        {
            return;
        }

        label.fontSize = 28;
        label.resizeTextMinSize = 20;
        label.resizeTextMaxSize = 28;
        label.alignment = TextAnchor.MiddleCenter;

        Shadow shadow = label.GetComponent<Shadow>();
        if (shadow != null)
        {
            shadow.effectColor = new Color(0f, 0f, 0f, 0.82f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }
    }

    private Button CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, string labelText, int fontSize, Color color)
    {
        Image image = CreateImage(name, parent, anchorMin, anchorMax, offsetMin, offsetMax, color);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.24f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.08f, 0.08f, 0.08f, 0.55f);
        button.colors = colors;

        CreateText(name + " Text", image.transform, Vector2.zero, Vector2.one, new Vector2(8f, 6f), new Vector2(-8f, -6f), labelText, fontSize, TextAnchor.MiddleCenter, Color.white);
        return button;
    }

}
