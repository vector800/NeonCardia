## Visual Direction Card

- Asset type: Battle UI party status HUD face portraits.
- In-game purpose: Three horizontal face crops for the compact top-left party status HUD.
- Target resolution / aspect ratio: 768x168 PNG, 32:7 horizontal crop.
- On-screen size: Roughly 150x35 px at 1920x1080 after Unity UI scaling.
- Background / transparency: Opaque dark blue HUD backing inside the portrait crop; no baked frame or text.
- Style family: Existing NeonCardia cyber-anime pixel-clean HUD character art.
- Color palette: Dark navy backing, cyan/blue character accents, restrained contrast.
- Material language: Clean character crop only; frame, slash, HP, and text remain separate UI objects.
- Lighting rule: Preserve source character highlights, avoid new dramatic glow.
- Shape language: Eye-band close-up, horizontal cut-in composition.
- Readability requirement: Eyes, face silhouette, and key character colors must stay readable in FHD Game View.
- Animation / state requirement: Static Sprite used by code; no state baked into the asset.
- Must include: Wolf, mech, and girl face crops matching the existing characters.
- Must avoid: White frame lines, white slash dividers, fake text, extra symbols, decorative clutter, rainbow gradients, excessive glow.
- Negative prompt: No fake text, no unreadable glyphs, no excessive glow, no glassmorphism, no holographic gradients, no rainbow gradients, no meaningless symbols, no decorative clutter, no cinematic poster composition, no over-detailed background, no perfect symmetry, no inconsistent shadows, no impossible reflections, no UI frame baked into the portrait.

## Source / Regeneration Notes

- No imagegen regeneration was used for this pass.
- The clean portraits were deterministically re-cut from the existing runtime timeline face icons:
  - `Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_02.png` -> `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Wolf.png`
  - `Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_01.png` -> `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Mech.png`
  - `Assets/Art/UI/Battle/Timeline/FaceIcons/AllyFaceIcon_03.png` -> `Assets/Art/UI/Battle/PartyStatusHUD/UI_BattlePartyHUD_FaceClean_Girl.png`
- The original wolf face icon is a side-profile source with only one visible biological eye. The crop was adjusted to a natural eye-band composition without changing the character identity or inventing a different character.
- Unity import settings are applied by `BattlePartyStatusHudPrefabSetup.ConfigureSprite`: Sprite (2D and UI), Single, alpha transparency enabled, mipmaps disabled, uncompressed, max size 2048, bilinear filtering.
