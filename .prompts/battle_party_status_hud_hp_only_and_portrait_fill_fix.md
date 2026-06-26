# Battle Party Status HUD HP-only / Portrait Fill Fix

Date: 2026-06-05

## Purpose

Fix the BattleScene upper-left party status HUD so the left status area shows HP values only and the right portrait cut-ins fill their masks cleanly at 1920x1080.

## Visual Direction Card

- Asset type: compact battle party status HUD rows
- In-game purpose: show three active ally HP values and horizontal portrait cut-ins
- Target resolution / aspect ratio: 1920x1080, 16:9
- On-screen size: compact upper-left HUD, away from the action-order timeline
- Background / transparency: transparent UI over battle scene
- Style family: black metal tactical JRPG HUD, silver row trim, red selected-row highlight
- Color palette: black / charcoal base, silver-white trim, cyan portrait accents, red selected highlight
- Material language: hard metal edges and compact frame-only decoration
- Lighting rule: restrained bright trim, no broad glow field
- Shape language: thin horizontal rows with clipped portrait windows
- Readability requirement: HP values readable at FHD; no baked text in sprites
- Animation / state requirement: selected row highlight remains controlled by PartyMemberStatusRowView
- Must include: HPValue_TMP, RowFrame_Image, SelectedHighlight_Image, PortraitArea, PortraitClipMask, PortraitImage
- Must avoid: character name, HP/MP labels, HP/MP bars, MP values, SetNativeSize, fit-with-padding portraits, distorted portraits, fake text
- Negative prompt: no glassmorphism, no rainbow gradients, no fake letters, no decorative clutter, no excessive glow, no poster composition, no extra labels, no unreadable miniature UI text

Note: imagegen / Imagen was not used in this pass. The work used deterministic Unity prefab, script, sprite, and import-setting changes. The human-ui-vfx-style rules and docs/VISUAL_GENERATION_RULES.md were checked before final audit.

## Cause Analysis

- The previous compact HUD row still had NameText_TMP, HPLabel_TMP, MPLabel_TMP, MPValue_TMP, HPBar_Background, and MPBar_Background present in the left status area. Those objects made the red-frame area read as a dense status panel instead of HP-only.
- Preserve Aspect by itself behaved like a fit operation when PortraitImage matched the mask rect. This could leave portraits feeling underfilled or biased inside a compressed horizontal HUD.
- The portrait overlay frame and slash divider could visually interfere with face visibility. Later face-icon cleanup requirements removed that white face-frame/slash treatment, so those objects are retained for reference safety but disabled.
- The middle mech portrait was later found to be too close-up compared with the other rows. Its clean sprite was recut to a wider 768x168 HUD strip so it reads naturally beside the wolf and girl.

## Fix Strategy

- Keep the existing prefab contract and data binding intact.
- Hide unused left-area UI objects instead of deleting them, so existing SetStatus assignments remain safe.
- Add UIPortraitCoverCrop to produce a cover crop from the mask size, sprite aspect, overscan, zoom, and offset without SetNativeSize or stretching.
- Use clean 768x168 portrait sprites for the Party Status HUD only, loaded separately from timeline icons in BattleTimelinePrototypeController.
- Keep PortraitFrame_Image and SlashDivider_Image inactive to satisfy the later white-line removal request while preserving the prefab hierarchy.

## Current Prefab Structure

- Assets/Prefabs/UI/BattlePartyStatusHUD.prefab
- Row children:
  - RowFrame_Image: active
  - SelectedHighlight_Image: active only on selected row
  - HPValue_TMP: active
  - PortraitArea / PortraitClipMask / PortraitImage: active
  - PortraitFrame_Image: inactive
  - SlashDivider_Image: inactive
  - NameText_TMP, HPLabel_TMP, MPLabel_TMP, MPValue_TMP: inactive
  - HPBar_Background, HPBar_Fill, MPBar_Background, MPBar_Fill: inactive

## HP-only Result

- Only HPValue_TMP remains visible in the left status area.
- HP text uses TextMeshProUGUI, font size 18, auto-size minimum 14, bold, right aligned.
- Preview/runtime HP values stay aligned as 125/125, 150/150, 110/110 in the three rows.

## Portrait Fill Result

- UIPortraitCoverCrop computes a cover-crop rect from the PortraitClipMask size and portrait sprite aspect.
- PortraitImage is centered, preserveAspect is true, localScale is one, Image Type is Simple, and SetNativeSize is not used.
- Current prefab crop values are zoom 1.00 for all rows, overscan (4, 2), offset (0, 0).
- Clean portrait sprites are 768x168:
  - UI_BattlePartyHUD_FaceClean_Wolf.png
  - UI_BattlePartyHUD_FaceClean_Mech.png
  - UI_BattlePartyHUD_FaceClean_Girl.png
- The mech clean sprite was recut on 2026-06-05 to reduce the too-close-up look while leaving the wolf and girl unchanged.

## Import Settings

Configured by BattlePartyStatusHudPrefabSetup:

- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single
- Alpha Is Transparency: on
- Mip Maps: off
- Compression: Uncompressed for the default importer setting
- Filter Mode: Bilinear
- Max Size: 2048 for clean portraits and main HUD sprites
- Mesh Type: Full Rect for clean portraits and most UI sprites
- PortraitClipMask uses Tight mesh for the angled mask silhouette

## Verification

- Unity refresh/compile was requested and completed with no console errors.
- BattleScene was active and Play Mode Game View was captured at 1920x1080.
- Latest FHD Game View screenshot:
  - Screenshots/battle_party_status_hud_hp_only_portrait_fill_check.png
- Console error check after capture:
  - 0 errors
- Re-audit on 2026-06-05 06:57:
  - The screenshot above was recaptured from the current Play Mode Game View.
  - Unity console error count remained 0.
  - BattleScene still references Assets/Prefabs/UI/BattlePartyStatusHUD.prefab through battlePartyStatusHudPrefab.
  - HPBar_Fill and MPBar_Fill were also changed to inactive, not just hidden under inactive parent bar backgrounds.

Visual check:

- Left status area shows HP numeric text only.
- Name, HP label, MP label, HP bar, HP fill, MP bar, MP fill, and MP value are not visible.
- Portrait cut-ins fill the right-side clipped area with no blank padding.
- The face-frame and slash-divider white overlays are not visible.
- Wolf and girl remain in their accepted positions.
- The mech portrait now has a wider, less cramped face balance.
- HUD stays compact and does not collide with the upper action-order timeline.

## Visual-generation Checklist

Score: 13 / 14, pass.

- Clear purpose: 2
- Gameplay-scale size: 2
- Existing UI match: 2
- Functional decoration only: 1
- Consistent light / trim: 2
- No fake text: 2
- Not poster/key art: 2

The one-point reduction is for retained row-line decoration, which is intentional HUD framing rather than portrait interference.

## Changed Files

- Assets/Editor/BattlePartyStatusHudPrefabSetup.cs
- Assets/Scripts/UI/UIPortraitCoverCrop.cs
- Assets/Scripts/UI/PartyMemberStatusRowView.cs
- Assets/Scripts/UI/BattlePartyStatusHUD.cs
- Assets/Scripts/BattleTimelinePrototypeController.cs
- Assets/Prefabs/UI/BattlePartyStatusHUD.prefab
- Assets/Scenes/BattleScene.unity
- Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Wolf.png
- Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png
- Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Girl.png
- Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_RowFrame.png
- Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_SelectedHighlight.png
- Assets/Art/UI/Battle/PartyStatusHUD/UI_BattleHUD_PortraitClipMask.png
- Screenshots/battle_party_status_hud_hp_only_portrait_fill_check.png
- .prompts/battle_party_status_hud_hp_only_and_portrait_fill_fix.md

## Unresolved / Notes

- No unresolved issues for the requested HUD surface.
- Existing project warnings outside this HUD scope were not changed.
- PortraitFrame_Image and SlashDivider_Image remain in the prefab but inactive because the newer face-icon cleanup request explicitly removed the white portrait-frame/slash decoration.
