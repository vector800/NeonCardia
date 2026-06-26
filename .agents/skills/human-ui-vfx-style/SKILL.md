---
name: human-ui-vfx-style
description: Use this before imagegen / Imagen generation for game UI, HUD, icons, panels, buttons, particles, hit effects, aura effects, spell effects, or VFX when the user wants assets that do not look AI-generated. This skill creates strict art-direction constraints, negative prompts, and post-generation checks.
---

# Human UI / VFX Style Director

Use this skill as a pre-generation art director for imagegen / Imagen tasks.

## Required workflow

1. Read `docs/VISUAL_GENERATION_RULES.md` if it exists.
2. Ask no clarification unless a missing detail would make the asset unusable. Otherwise make a reasonable production assumption and state it.
3. Create a concise `Visual Direction Card`.
4. Convert the card into a strict image generation prompt.
5. Include a negative prompt that bans common AI-looking patterns.
6. Use imagegen / Imagen only after the direction is concrete.
7. After generation, review the asset against the checklist in `docs/VISUAL_GENERATION_RULES.md`.
8. Save the final prompt under `.prompts/` when working in a repository.

## Design stance

Prioritize:

- production asset over key art
- gameplay readability over beauty
- restrained lighting over dramatic glow
- functional hierarchy over decoration
- consistent material language over mixed styles
- limited palette over rainbow gradients
- slight asymmetry over perfect AI symmetry

Reject:

- meaningless neon glow
- glassmorphism by default
- fake text or fake glyphs
- decorative clutter
- impossible reflections
- generic premium/futuristic/cinematic styling
- over-polished symmetrical compositions
- assets that only look good as a standalone portfolio image

## Output format before generation

```md
## Visual Direction Card

- Asset type:
- In-game purpose:
- Target resolution / aspect ratio:
- On-screen size:
- Background / transparency:
- Style family:
- Color palette:
- Material language:
- Lighting rule:
- Shape language:
- Readability requirement:
- Animation / state requirement:
- Must include:
- Must avoid:
- Negative prompt:

## Image Generation Prompt

<final prompt>
```

## Default negative prompt

No fake text, no unreadable glyphs, no excessive glow, no neon unless explicitly required, no glassmorphism, no holographic gradients, no rainbow gradients, no meaningless symbols, no decorative clutter, no cinematic poster composition, no over-detailed background, no perfect symmetry, no inconsistent shadows, no impossible reflections, no UI elements that would be unusable in an actual game.
