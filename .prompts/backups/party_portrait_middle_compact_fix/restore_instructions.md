# Restore Instructions: party_portrait_middle_compact_fix

Backup root:
.prompts/backups/party_portrait_middle_compact_fix

This backup was created before changing the 2nd-row party HUD portrait density/red-line issue.

## Files backed up

Backed up files are under:
.prompts/backups/party_portrait_middle_compact_fix/files

The key candidate files are:
- Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png
- Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png.meta
- Assets/Prefabs/UI/BattlePartyStatusHUD.prefab
- Assets/Prefabs/UI/BattlePartyStatusHUD.prefab.meta
- Assets/Editor/BattlePartyStatusHudPrefabSetup.cs
- Assets/Editor/BattlePartyStatusHudPrefabSetup.cs.meta
- Assets/Scripts/UI/UIPortraitCoverCrop.cs
- Assets/Scripts/UI/PartyMemberStatusRowView.cs
- Assets/Scripts/UI/BattlePartyStatusHUD.cs
- Assets/Scripts/BattleTimelinePrototypeController.cs
- Screenshots/battle_party_status_hud_hp_only_portrait_fill_check.png

## Manual restore

From the project root, copy a backed-up file back over the live file. Example:

```powershell
Copy-Item -LiteralPath ".prompts/backups/party_portrait_middle_compact_fix/files/Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png" -Destination "Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png" -Force
Copy-Item -LiteralPath ".prompts/backups/party_portrait_middle_compact_fix/files/Assets/Prefabs/UI/BattlePartyStatusHUD.prefab" -Destination "Assets/Prefabs/UI/BattlePartyStatusHUD.prefab" -Force
Copy-Item -LiteralPath ".prompts/backups/party_portrait_middle_compact_fix/files/Assets/Editor/BattlePartyStatusHudPrefabSetup.cs" -Destination "Assets/Editor/BattlePartyStatusHudPrefabSetup.cs" -Force
```

Then refresh Unity:

```powershell
# In Unity: Assets > Refresh, or run the project-specific refresh tool if available.
```

## Notes

- `git_status_before.txt` records the dirty worktree before this fix.
- `candidate_diff_before.patch` records a best-effort pre-change diff for tracked candidate files. Many HUD files are untracked, so exact file copies under `files/` are the authoritative backup.
- Do not run broad git reset/checkout commands unless you explicitly intend to revert unrelated user/project changes.
