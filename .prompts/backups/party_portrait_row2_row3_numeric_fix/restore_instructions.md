# Restore Instructions: party_portrait_row2_row3_numeric_fix

Backup root:

`.prompts/backups/party_portrait_row2_row3_numeric_fix`

This backup was created before adjusting the 2nd and 3rd row face icons for numeric alignment.

## Backed-up Files

Backed-up files are stored under:

`.prompts/backups/party_portrait_row2_row3_numeric_fix/files`

Key files:

- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png.meta`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Girl.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Girl.png.meta`
- `Assets/Prefabs/UI/BattlePartyStatusHUD.prefab`
- `Assets/Prefabs/UI/BattlePartyStatusHUD.prefab.meta`
- `Assets/Editor/BattlePartyStatusHudPrefabSetup.cs`
- `Assets/Editor/BattlePartyStatusHudPrefabSetup.cs.meta`
- `Assets/Scripts/UI/UIPortraitCoverCrop.cs`
- `Assets/Scripts/UI/PartyMemberStatusRowView.cs`
- `Assets/Scripts/UI/BattlePartyStatusHUD.cs`
- `Assets/Scripts/BattleTimelinePrototypeController.cs`
- `Screenshots/party_portrait_middle_compact_fix_check.png`
- `Screenshots/party_portrait_middle_compact_final_zoom.png`

## Restore Examples

Run these from the project root if you need to restore the backed-up state:

```powershell
Copy-Item -LiteralPath ".prompts/backups/party_portrait_row2_row3_numeric_fix/files/Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png" -Destination "Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png" -Force
Copy-Item -LiteralPath ".prompts/backups/party_portrait_row2_row3_numeric_fix/files/Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Girl.png" -Destination "Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Girl.png" -Force
Copy-Item -LiteralPath ".prompts/backups/party_portrait_row2_row3_numeric_fix/files/Assets/Prefabs/UI/BattlePartyStatusHUD.prefab" -Destination "Assets/Prefabs/UI/BattlePartyStatusHUD.prefab" -Force
Copy-Item -LiteralPath ".prompts/backups/party_portrait_row2_row3_numeric_fix/files/Assets/Editor/BattlePartyStatusHudPrefabSetup.cs" -Destination "Assets/Editor/BattlePartyStatusHudPrefabSetup.cs" -Force
```

Then refresh Unity from the editor.

## Notes

- `git_status_before.txt` records the dirty worktree before this task.
- `candidate_diff_before.patch` is a best-effort diff for tracked candidate files.
- Many HUD files are untracked, so the copied files under `files/` are the authoritative backup.
- Avoid broad `git reset` or checkout commands unless you intentionally want to discard unrelated project changes.
