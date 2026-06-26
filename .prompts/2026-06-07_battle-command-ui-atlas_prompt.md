# Asset Prompt: battle-command-ui-atlas

## Purpose

Text-free 2D UI component atlas for the BattleScene command selection HUD in NeonCardia. The atlas provides row backgrounds, selected-state variants, icon frame, card overlay, and purple information panel parts. Runtime text remains TextMeshPro-driven in Unity.

## Visual Direction Card

- Asset type: 2D UI component atlas / engine atlas
- In-game purpose: Battle command rows and detail panel frame/background for the BattleScene top HUD
- Target resolution / aspect ratio: 2048x2048 square atlas, 4 columns x 4 rows, solid #FF00FF removable background
- On-screen size: rows around 310x28-34px, info panel around 500x204px at 1920x1080
- Background / transparency: magenta key background, components isolated with padding for transparent export
- Style family: project-native sci-fi battle HUD, matching existing cyan/purple timeline UI
- Color palette: deep navy base, cyan/blue card rows, green/yellow/magenta command rows, purple info panel
- Material language: flat layered polymer/metal HUD plates, thin rim light, small mechanical line engravings
- Lighting rule: single soft top-left UI highlight, restrained glow only on outer rims
- Shape language: chamfered long rectangles, thin rails, small corner brackets
- Readability requirement: large empty text bands, no text baked into assets, low-detail center areas
- Animation / state requirement: normal and selected state variants for rows/buttons
- Must include: 13 isolated parts plus optional rails/caps; sharp chamfered corners; 9-slice-friendly borders
- Must avoid: fake text, symbols with no meaning, character art, poster composition, excessive glow, glassmorphism, rainbow gradients

## Final Prompt

Create a 2048x2048 2D game UI sprite atlas for a sci-fi console RPG battle command HUD, project-native clean HD asset, not key art. Use a solid #FF00FF chroma key background. Arrange isolated UI components in a clean 4 columns x 4 rows atlas with generous padding, each component centered in its cell and fully inside the cell edges. No labels, no letters, no numbers, no fake text.

Components, one per cell where possible:
1 blue command card row normal: long low rectangular beveled panel, deep navy center, cyan rim, empty readable center band.
2 blue command card row selected: same size and silhouette, brighter cyan rim and restrained selected-state edge light.
3 subtle card back design overlay: transparent-looking cyan circuitry plate, very low visual density, meant to sit behind TMP text.
4 left card icon frame: small diamond/chamfered icon socket, cyan rim, empty middle.
5 SKILLS button normal: green beveled row panel, empty center, narrow mechanical line accents.
6 SKILLS button selected: same green panel with slightly brighter rim, not overglowing.
7 CHANGE button normal: amber/yellow beveled row panel, empty center.
8 CHANGE button selected: same amber panel with brighter selected rim.
9 RUN button normal: magenta/pink beveled row panel, empty center.
10 RUN button selected: same magenta panel with brighter selected rim.
11 purple information panel outer frame: wide rectangular frame with chamfered corners, empty transparent interior, violet rim.
12 purple information panel inner background: dark violet translucent-looking flat panel with very subtle scanline texture and large empty readable areas.
13 information panel divider parts: thin purple horizontal/vertical separator bars and tiny corner brackets, isolated pieces.
14-16 optional small accent rails and corner caps matching the atlas style.

Design constraints: sharp industrial HUD plates, limited palette, single top-left light source, restrained effects, slight asymmetry in small scratches, 9-slice-friendly borders, gameplay scale readability, production asset, fits existing cyan and purple top timeline HUD. Keep centers clean for real TextMeshPro text added in Unity. Avoid excessive ornament and avoid icon drawings that imply unreadable symbols. No fake text, no characters, no background scene, no poster composition, no glassmorphism, no rainbow gradients, no impossible reflections, no excessive particles, no cinematic lighting.

## Negative Prompt

No fake text, no unreadable glyphs, no excessive glow, no glassmorphism, no holographic gradients, no rainbow gradients, no meaningless symbols, no decorative clutter, no cinematic poster composition, no over-detailed background, no perfect symmetry, no inconsistent shadows, no impossible reflections, no UI elements that would be unusable in an actual game.

## Result Notes

Generated a 4x4 atlas and processed it through generate2dsprite with a 4x4 grid. The resulting components are text-free, strongly separated by command color, and keep empty center bands for TMP text. Some long row parts reach their source-cell horizontal bounds because they are intentionally wide UI plates; the transparent processed outputs still show complete silhouettes with readable interior space.

Checklist score: 13/14
- Purpose instantly clear: 2
- Readable at screen size: 2
- Fits existing UI/VFX: 2
- Decoration does not block function: 2
- Lighting/shadow consistency: 2
- No broken text/symbols: 2
- No AI-overdone feeling: 1

## Regeneration Notes

No regeneration performed. The generated components are production-usable after deterministic chroma-key cleanup and naming.
