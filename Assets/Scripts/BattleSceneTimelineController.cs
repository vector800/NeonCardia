public sealed class BattleSceneTimelineController : BattleTimelinePrototypeController
{
    protected override void Awake()
    {
        // BattleManager supplies BattleScene-specific HUD settings before startup.
    }

    public void InitializeFromBattleManager(bool showDebugLabels, bool usePrefabActionOrderHud, BattleTimelineHudView battleTimelineHudPrefab, BattlePartyStatusHUD battlePartyStatusHudPrefab, BattlePartyStatusPanelController battlePartyStatusPanelPrefab)
    {
        ConfigureActionOrderHud(usePrefabActionOrderHud, battleTimelineHudPrefab);
        ConfigurePartyStatusHud(battlePartyStatusHudPrefab);
        ConfigurePartyStatusPanel(battlePartyStatusPanelPrefab);
        SetInitialDebugLabels(showDebugLabels);
        InitializeController();
    }
}
