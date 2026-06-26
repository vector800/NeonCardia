# Battle Party Status HUD Generation

## Purpose

Create a Unity-usable battle party status HUD for a 1920x1080 RPG battle screen. The HUD uses three stacked horizontal rows, each with TextMeshPro-driven name/HP/MP values, fill-controlled HP/MP bars, a selected-state highlight, a right-side horizontal portrait strip, and separate frame/divider assets.

## Rules Read

- `AGENTS.md`
- `docs/VISUAL_GENERATION_RULES.md`
- `.agents/skills/human-ui-vfx-style/SKILL.md`

Rules applied: production asset over key art, no baked text, no fake glyphs, restrained lighting, limited palette, explicit gameplay scale, separated Unity-controllable parts, post-generation scoring, and prompt notes saved under `.prompts/`.

## Existing UI Structure Notes

- Existing `BattlePartyStatusPanel` files already existed and represent an active/reserve compact panel.
- New work keeps that panel intact and adds a separate `BattlePartyStatusHUD` path for the requested horizontal 3-row FHD HUD.
- `BattleTimelinePrototypeController` is the BattleScene runtime connection point for prefab UI.

## Visual Direction Card

- Asset type: Battle Party Status HUD; UI frame assets; character portrait strip assets; HP/MP bar assets; selected highlight asset
- In-game purpose: party status readout during the BattleScene timeline combat screen
- Target resolution / aspect ratio: 1920x1080 Game View; HUD preview composed at FHD
- Target Game View: 1920x1080
- On-screen size: approximately 1080x360 px in the lower-right FHD safe area
- Layout position: three horizontal status rows stacked vertically; portrait strip on the right of each row
- Number of party members: 3
- Row structure: frame, selected highlight, name text, HP label/value, HP fill, MP label/value, MP fill, slash divider, portrait mask/image
- Background / transparency: split PNG parts use alpha where appropriate; preview uses a dark FHD background only for readability checking
- Style family: 2000s to 2010s console JRPG combat HUD
- Color palette: black gunmetal, restrained silver, crisp white linework, cyan-green HP, deep blue MP, red selected highlight
- Material language: black metal panels with silver trim and small worn asymmetric edge marks
- Lighting rule: single cool top-left light source; no broad glow field
- Shape language: long angular bars, diagonal slash dividers, hard-edged portrait strip masks
- Portrait direction: horizontal anime eye close-up strips cropped into the right side of each row
- HP / MP bar direction: horizontal fill from left to right, controlled by Unity `Image.fillAmount`
- Selected-state direction: red angular rim highlight, visible but not overbright
- Readability requirement: no text in generated images; all runtime values are TextMeshPro; readable in FHD
- Implementation requirement: separated sprites, 9-slice-capable frame paths, Canvas Scaler at 1920x1080
- Must include: 3 rows, HP/MP distinction, right-side portraits, slash divider, black metal/silver frame, red selected highlight
- Must avoid: fake text, unreadable symbols, rainbow gradients, glassmorphism, excessive glow, one-piece baked HUD, decorative clutter
- Negative prompt: no fake letters, no unreadable glyphs, no neon overload, no glassmorphism, no excessive particles, no one-piece cinematic illustration, no tiny lines, no blurry or compressed edges

## Final Image Prompt

Create a production-ready separated UI asset sheet for a 1920x1080 JRPG battle party status HUD, not key art and not a poster. Asset sheet contains clean separated parts on a transparent or flat removable dark background: three long horizontal row frames for party member status, one 9-slice-capable black gunmetal HUD frame, HP fill strip in cyan-green, MP fill strip in deep blue, selected-state red rim highlight, diagonal white slash divider strips, black shadow dividers, and wide horizontal anime eye close-up portrait strip placeholders without text. Game context: 2000s to 2010s console JRPG combat interface. Layout language: sharp angular black metal bars, restrained silver metallic trim, crisp white divider lines, organized empty text zones for Unity TextMeshPro values, portrait strip area on the right side of each row. Single cool top-left light source, slight asymmetry in scuffs and edge wear, limited palette: black gunmetal, silver, white, cyan-green, blue, red. Production asset, readable at actual gameplay scale, clean separated UI parts, no baked text, no numbers, no labels, no fake symbols.

## Negative Prompt

AI-looking design, excessive glow, neon overload, rainbow gradients, glassmorphism, fake text, unreadable glyphs, random symbols, cluttered background, poster art, cinematic illustration, symmetrical over-decoration, blurry edges, tiny unreadable lines, compression artifacts, low-resolution upscaling, noisy details, photorealistic faces, over-rendered fantasy particles, UI elements that cannot be implemented in Unity.

## Generated Asset List

- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_Frame_9Slice.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_RowFrame.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_HPFill.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_MPFill.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_SelectedHighlight.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_SlashDivider.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_PortraitMask.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_Portrait_01.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_Portrait_02.png`
- `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_Portrait_03.png`
- `.prompts/battle_party_status_hud_imagegen_reference.png`
- `Screenshots/battle_party_status_hud_fhd_check.png`
- `Screenshots/battle_party_status_hud_unity_render.png`
- `Assets/Prefabs/UI/BattlePartyStatusHUD.prefab`
- `Assets/Scripts/UI/BattlePartyStatusHUD.cs`
- `Assets/Scripts/UI/PartyMemberStatusRowView.cs`
- `Assets/Editor/BattlePartyStatusHudPrefabSetup.cs`

## Checklist Score

Project checklist: 20 / 20

- 1920x1080 readable text/value design: 2
- HP/MP bars clearly distinct: 2
- Three party status rows organized: 2
- Right-side horizontal portraits fit naturally: 2
- 2000s-2010s JRPG battle HUD feel: 2
- Black metal, white line, silver trim direction: 2
- Red selected highlight readable: 2
- Avoids fake text/symbols and overdone AI decoration: 2
- Unity-controllable split asset / prefab structure: 2
- No blur/compression/low-res scaling in final split assets: 2

Readable-critical items have no 0 score. The Unity-rendered FHD preview was adjusted to keep HP/MP values away from the diagonal portrait divider.

## Regeneration Notes

The imagegen concept passed style direction but was not used as a single baked HUD. It was retained as a reference and used to derive portrait strip direction. Production HUD parts were split into transparent PNG assets so Unity can control text, fill amounts, selected state, masks, and layout.

## Unity Import Settings

Apply with `Tools/NeonCardia/Apply Battle Party Status HUD Prefab Setup`.

Expected settings:

- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single
- Alpha Source: Input Texture Alpha
- Alpha Is Transparency: enabled
- Mip Maps: disabled
- Compression: Uncompressed
- Filter Mode: Bilinear
- Max Size: 2048 for row/frame/portrait assets, 512 for bar/divider assets
- 9-slice borders: set by the editor setup script for frame, row frame, and selected highlight

## Verification

- FHD preview: `Screenshots/battle_party_status_hud_fhd_check.png`
- Unity-rendered FHD check: `Screenshots/battle_party_status_hud_unity_render.png`
- Unity menu setup: `Tools/NeonCardia/Apply Battle Party Status HUD Prefab Setup`
- Unity capture menu: `Tools/NeonCardia/Capture Battle Party Status HUD FHD Check`
- Prefab: `Assets/Prefabs/UI/BattlePartyStatusHUD.prefab`
- Runtime connection: `BattleManager.battlePartyStatusHudPrefab` set by the editor setup script
- Console check after setup/capture: no errors or warnings; setup logged `BattleScene linked: True`
- Play Mode smoke check: entered Play Mode and observed no errors or warnings during battle scene initialization

## Open Items

- If real MP data is added later, replace the current connector's speed-derived preview MP value with the authoritative battle resource.
