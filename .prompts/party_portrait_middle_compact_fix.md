# Party Portrait Middle Compact Fix

Date: 2026-06-05

## Purpose

Fix only the 2nd-row party status HUD face icon. The middle mech portrait had lower visual density than the 1st-row wolf and 3rd-row girl, and the selected-row red line was visible around the 2nd row.

## Backup

Backup root:

- `.prompts/backups/party_portrait_middle_compact_fix/`

Saved before editing:

- `git_status_before.txt`
- `candidate_diff_before.patch`
- `restore_instructions.md`
- File copies under `.prompts/backups/party_portrait_middle_compact_fix/files/`

Key backed-up files:

- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png.meta`
- `Assets/Prefabs/UI/BattlePartyStatusHUD.prefab`
- `Assets/Prefabs/UI/BattlePartyStatusHUD.prefab.meta`
- `Assets/Editor/BattlePartyStatusHudPrefabSetup.cs`
- `Assets/Editor/BattlePartyStatusHudPrefabSetup.cs.meta`
- `Assets/Scripts/UI/UIPortraitCoverCrop.cs`
- `Assets/Scripts/UI/PartyMemberStatusRowView.cs`
- `Assets/Scripts/UI/BattlePartyStatusHUD.cs`
- `Assets/Scripts/BattleTimelinePrototypeController.cs`
- `Screenshots/battle_party_status_hud_hp_only_portrait_fill_check.png`

## Cause Analysis

The three HUD rows use the same UI layout values:

- `PortraitArea`: anchorMin `(0.600, 0.030)`, anchorMax `(0.980, 0.970)`
- `PortraitClipMask`: anchorMin `(0, 0)`, anchorMax `(1, 1)`, localScale `(1, 1, 1)`
- `PortraitImage`: `Image.Type.Simple`, `preserveAspect=true`, localScale `(1, 1, 1)`
- `UIPortraitCoverCrop`: row zoom `1.00`, overscan `(4, 2)`, offset `(0, 0)`
- No `SetNativeSize`, `AspectRatioFitter`, `ContentSizeFitter`, or LayoutGroup is used in the target HUD path.

The Import Settings were also effectively identical for the clean face sprites:

- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single
- Alpha Is Transparency: on
- Mip Maps: off
- Max Size: 2048
- Filter Mode: Bilinear
- Sprite Pixels Per Unit: 100
- Mesh Type: Full Rect

Therefore, the low-density issue came from the 2nd-row sprite composition, not from a different UI RectTransform or importer setting.

## Numeric Difference

Visible subject bbox was measured from each 768x168 clean portrait using alpha and luminance thresholds.

- Row 1 wolf: bbox `[121, 0, 686, 168]`, bbox ratio `0.7357`, subject pixel ratio `0.5049`
- Row 2 mech before: bbox `[172, 0, 533, 168]`, bbox ratio `0.4701`, subject pixel ratio `0.2322`
- Row 3 girl: bbox `[9, 0, 755, 168]`, bbox ratio `0.9714`, subject pixel ratio `0.7023`

The 2nd-row mech had much larger left/right dark margins, especially right margin `235px`, so it looked sparse beside the other portraits.

## Red Line Cause

The unwanted red line came from `SelectedHighlight_Image`.

- It is only active on the selected 2nd row in the current prefab preview/runtime state.
- Before the fix it covered the row and introduced red framing/lines near the 2nd portrait area.
- `RowFrame_Image` is the normal grey/black frame and was not the red-line source.
- `SlashDivider_Image` and `PortraitFrame_Image` are inactive and were not causing the red line.

## Loop 1

Changes:

- Recut only `UI_BattlePartyHUD_FaceClean_Mech.png`.
- Used `AllyFaceIcon_01.png` as the source.
- Tested candidate scales from `540` to `620`.
- Best Loop 1 candidate reached bbox ratio `0.5964`.
- Limited the 2nd-row `SelectedHighlight_Image` width to stop before `PortraitArea`.

Result:

- Density improved, but a red highlight line was still visible near the 2nd row/portrait boundary.
- Continued to Loop 2 because red-line removal was not complete.

Loop 1 screenshots:

- `Screenshots/mech_compact_candidates_loop1.png`
- `Screenshots/party_portrait_middle_compact_loop1.png`

## Loop 2

Changes:

- Recut only the 2nd-row mech sprite again using:
  - source: `Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_01.png`
  - target: 768x168
  - scale: `710`
  - position: `(-45, -285)`
- Set the 2nd-row `SelectedHighlight_Image` alpha to `0`.
- Kept 1st-row wolf and 3rd-row girl sprites unchanged.

Result:

- Row 2 mech after: bbox `[60, 0, 580, 168]`, bbox ratio `0.6771`, subject pixel ratio `0.3407`
- The mech portrait is now much closer to the wolf density while still showing the white face mask / visor.
- Red pixels in the sampled middle portrait area: `0 / 6800`.

Loop 2 screenshots:

- `Screenshots/party_portrait_middle_compact_loop2.png`
- `Screenshots/party_portrait_middle_compact_final_zoom.png`
- `Screenshots/party_portrait_middle_compact_fix_check.png`

## Modified Files

- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png`
- `Assets/Editor/BattlePartyStatusHudPrefabSetup.cs`
- `Assets/Prefabs/UI/BattlePartyStatusHUD.prefab`
- `Screenshots/party_portrait_middle_compact_loop1.png`
- `Screenshots/party_portrait_middle_compact_loop2.png`
- `Screenshots/party_portrait_middle_compact_final_zoom.png`
- `Screenshots/party_portrait_middle_compact_fix_check.png`
- `.prompts/party_portrait_middle_compact_fix.md`
- `.prompts/backups/party_portrait_middle_compact_fix/`

No new BattleScene diff was introduced by this fix. The current `Assets/Scenes/BattleScene.unity` diff matches the pre-work candidate diff.

## Unity Verification

- Refreshed Unity and compiled scripts.
- Applied `Tools/NeonCardia/Apply Battle Party Status HUD Prefab Setup` outside Play Mode.
- Console errors after correct menu run: `0`.
- Captured 1920x1080 Game View:
  - `Screenshots/party_portrait_middle_compact_fix_check.png`
- Created enlarged HUD crop:
  - `Screenshots/party_portrait_middle_compact_final_zoom.png`
- Console errors after FHD capture: `0`.

## Final Score

Score: `9.2 / 10`

- Red-line removal: `2.0 / 2`
- 2nd-row whitespace reduction: `1.8 / 2`
- Face mask / visor visibility: `1.9 / 2`
- 1st-row and 3rd-row no visual change: `2.0 / 2`
- Other UI no side effects: `1.5 / 2`

The only minor deduction is that the mech source character has a naturally narrower silhouette than the girl portrait, so its density does not exactly match the 3rd row without making it too close-up.

## Restore Method

Use the backup instructions:

- `.prompts/backups/party_portrait_middle_compact_fix/restore_instructions.md`

Example:

```powershell
Copy-Item -LiteralPath ".prompts/backups/party_portrait_middle_compact_fix/files/Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png" -Destination "Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png" -Force
Copy-Item -LiteralPath ".prompts/backups/party_portrait_middle_compact_fix/files/Assets/Prefabs/UI/BattlePartyStatusHUD.prefab" -Destination "Assets/Prefabs/UI/BattlePartyStatusHUD.prefab" -Force
Copy-Item -LiteralPath ".prompts/backups/party_portrait_middle_compact_fix/files/Assets/Editor/BattlePartyStatusHudPrefabSetup.cs" -Destination "Assets/Editor/BattlePartyStatusHudPrefabSetup.cs" -Force
```

Then refresh Unity.

## Unresolved

No unresolved issues for the requested scope.
