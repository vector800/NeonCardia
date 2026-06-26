\# AGENTS.md - NC / NeonCardia



\## Project Identity



NC / NeonCardia is a Unity 2D card-battle RPG project.



The target quality is not a rough prototype.

Aim for the feel of a polished Japanese console game:

\- readable battle UI

\- clear card state and action feedback

\- stable battle flow

\- consistent spacing, alignment, and visual hierarchy

\- restrained but satisfying VFX and motion

\- no debug-looking layout unless explicitly requested



When making decisions, prioritize:

1\. Stable playable battle scene

2\. Clear and readable battle HUD

3\. Good game feel and feedback

4\. Console-game-like polish

5\. New feature expansion only after the current screen remains stable



\## Language



Respond to the user in Japanese unless explicitly requested otherwise.



\## General Working Rules



Before changing code, scenes, prefabs, or assets:

\- Inspect the existing structure first.

\- Identify the smallest safe change.

\- Avoid unrelated refactoring.

\- Do not rewrite large systems unless the task explicitly asks for it.

\- Do not delete, rename, or move important assets without explicit approval.

\- Do not add new packages, third-party assets, fonts, or dependencies without explicit approval.

\- Preserve existing public APIs and serialized references unless there is a clear reason to change them.



When the request is ambiguous:

\- Prefer a short plan before implementation.

\- Ask only necessary clarifying questions.

\- Do not invent project specifications that are not visible in the repository.



\## Unity Development Rules



For Unity work:

\- Prefer Prefab-based and serialized-reference-based UI over large runtime-generated UI.

\- Do not casually override Prefab layout from scripts.

\- Do not make broad Scene changes for small visual fixes.

\- Be careful with `.unity`, `.prefab`, `.asset`, and `.meta` files.

\- Do not change unrelated camera, enemy scale, Canvas settings, import settings, or animation settings.

\- Keep runtime layout logic simple, explicit, and predictable.

\- Avoid hardcoded one-off values unless the task is specifically a temporary prototype.



For UI:

\- Check actual Play Mode appearance, not only code correctness.

\- Assume 16:9 as the primary layout target unless otherwise specified.

\- Avoid overlaps between text, cards, markers, connector lines, HP, enemy UI, and action previews.

\- Ensure important text is readable at gameplay distance.

\- Maintain consistent padding, spacing, alignment, and hierarchy.

\- Make CURRENT card, selectable cards, disabled cards, HP, enemy state, and action feedback visually distinguishable.



\## Battle UI / HUD Quality Bar



When editing battle UI, judge the result by whether it looks closer to a finished console game screen.



Required checks:

\- HP and card names are readable.

\- The current action/card is immediately understandable.

\- The player can distinguish current, selectable, disabled, and resolved states.

\- UI elements do not overlap at 16:9.

\- Spacing and alignment feel intentional.

\- Effects do not hide gameplay information.

\- The screen does not look like a debug prototype unless debug UI was requested.



Prefer:

\- clear hierarchy

\- restrained glow

\- readable contrast

\- consistent panels and margins

\- short, responsive feedback



Avoid:

\- excessive bloom/glow

\- cluttered decoration

\- unreadable text

\- over-polished AI-looking visuals

\- fake symbols or fake text

\- UI that looks good as a still image but fails during play



\## Visual Generation Rules for UI / HUD / Icons / VFX



Before using imagegen, Imagen, or any image generation skill for game UI, HUD, icons, panels, buttons, particles, hit effects, aura effects, spell effects, or other visual assets, read and follow:



`docs/VISUAL\_GENERATION\_RULES.md`



Hard requirements:

\- Do not generate a visual asset from a vague prompt such as “cool”, “modern”, “futuristic”, “premium”, “cinematic”, or “AI-like”.

\- First create a short Visual Direction Card.

\- Prefer production assets that look usable in-game over portfolio/key-art images.

\- Avoid excessive glow, glassmorphism, rainbow gradients, fake text, meaningless symbols, decorative clutter, and symmetrical over-polished AI compositions.

\- For UI, preserve empty areas where real text and components will be rendered by code.

\- For VFX, specify timing, usage, transparency/background, silhouette, and whether it is a sprite, overlay, or loopable texture.

\- After generation, score the result using the checklist in `docs/VISUAL\_GENERATION\_RULES.md`.

\- Regenerate if the score is below the pass threshold.

\- Save the final prompt and regeneration notes under `.prompts/`.

\- When the generated asset conflicts with these rules, treat the result as failed even if it looks visually impressive.



Visual Direction Card template:

\- Purpose:

\- Target size:

\- Game context:

\- Palette:

\- Material language:

\- Light source:

\- Readability requirement:

\- Transparency/background:

\- Timing/looping, if VFX:

\- Negative prompt:

\- In-game usage location:



If `docs/VISUAL\_GENERATION\_RULES.md` is missing or unclear, stop and report the issue before generating assets.



\## Verification



After implementation, verify as much as possible.



For code changes:

\- Check for compile errors.

\- Check that no unrelated files were modified.

\- Review the diff for accidental large rewrites.



For Unity changes:

\- Confirm the relevant Scene opens.

\- Confirm the Unity Console has no new errors.

\- Use Play Mode when possible.

\- Check 16:9 layout for UI/HUD changes.

\- Check that the changed behavior works in the actual screen, not only in code.



If Unity MCP or editor automation is available, use it for relevant Unity checks.

If a check cannot be run, clearly report it as unverified.



\## Final Report Format



After each task, report briefly:

\- What changed

\- Files changed

\- What was verified

\- What could not be verified

\- Risks or follow-up polish items

