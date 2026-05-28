public sealed class BattleSceneTimelineController : BattleTimelinePrototypeController
{
    protected override void Awake()
    {
        // BattleManager supplies BattleScene-specific HUD settings before startup.
    }

    public void InitializeFromBattleManager(bool showDebugLabels, bool usePrefabActionOrderHud, BattleTimelineHudView battleTimelineHudPrefab)
    {
        ConfigureActionOrderHud(usePrefabActionOrderHud, battleTimelineHudPrefab);
        SetInitialDebugLabels(showDebugLabels);
        InitializeController();
    }
}
