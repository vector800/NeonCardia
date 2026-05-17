# NeonCardia Battle MVP

## BattleTimelinePrototypeScene MVP

`BattleTimelinePrototypeScene` was added as a separate prototype scene for a new turn-based card RPG battle flow. This scene does not replace or modify the existing `BattleScene`; the current `BattleScene`, `DeckBuildScene`, `MenuScene`, existing card processing, and existing battle result processing remain separate.

### Prototype Goals

- Validate a new battle format in `Assets/Scenes/BattleTimelinePrototypeScene.unity`.
- Keep the existing battle implementation intact.
- Reuse shared data only where it is low-risk, such as `CardData`, card attributes, deck type, and Clear Card flags.
- Keep the prototype self-contained in `BattleTimelinePrototypeController`.

### Current Battle Layout

- Ally party has three fixed position lanes: `Front`, `Middle`, and `Back`.
- Initial ally placement is `AllyFront`, `AllyMiddle`, and `AllyBack`.
- Each ally has name, HP, max HP, position, Speed, and placeholder status text.
- Enemy side uses a 3x3 grid.
- The prototype currently creates up to three enemies, with `Enemy1` placed in the center cell.
- Each enemy has name, HP, max HP, attribute, grid coordinate, next action preview, and Speed.
- The top of the screen shows a horizontal action timeline.
- The bottom of the screen shows a shared hand, Weapon, swap buttons, Reset, and Confirm.

### Current MVP Features

- Timeline order is calculated from ally and enemy Speed values.
- When a unit acts, it is returned to the back of the timeline with Speed-based recovery.
- Ally turns can queue card use, Weapon, or party position swaps.
- The action queue is already structured for up to three normal actions.
- Clear Cards are detected through existing `CardData.IsClearCard` and do not count against the normal action queue cap.
- Damage cards apply single-target damage to the selected enemy.
- Repair cards heal the selected ally.
- Unsupported card effects fall back to a simple placeholder effect or status update.
- Weapon deals placeholder single-target damage.
- Front/Middle/Back swaps are available through `Swap F/M`, `Swap M/B`, and `Swap F/B`.
- Enemy turns can be resolved with Confirm and currently apply placeholder damage to the front ally.
- The prototype loads existing cards from `Assets/Resources/Cards` when available.

### Not Implemented Yet

- Full compatibility with all existing card targeting patterns.
- Real multi-target card resolution.
- Production enemy AI.
- Status ailment rules beyond placeholder text and a simple freeze delay.
- Character-specific decks, equipment, skills, and passives.
- Targeting UI for every future card type.
- Battle result overlay, rewards, and progression for this prototype.
- Animation, sound effects, and production presentation.
- Automated tests.

### Verification Steps

1. Open the project in Unity Editor.
2. Confirm `Assets/Scenes/BattleScene.unity` still exists and has not been edited for this prototype.
3. Open `Assets/Scenes/BattleTimelinePrototypeScene.unity`.
4. Enter Play Mode and confirm there are no compile errors.
5. Confirm `AllyFront`, `AllyMiddle`, and `AllyBack` are visible.
6. Confirm `Front`, `Middle`, and `Back` positions are visible.
7. Confirm the enemy 3x3 grid is visible.
8. Confirm at least `Enemy1` is visible in the center grid cell.
9. Confirm the top timeline is visible and shows ally/enemy action order.
10. Confirm the shared hand is visible at the bottom.
11. Confirm card buttons, Weapon, swap buttons, Reset, and Confirm are visible.
12. Select an enemy cell, click a damage card, and press Confirm; confirm enemy HP changes.
13. Select an ally, click a Repair card if one is in hand, and press Confirm; confirm ally HP can recover.
14. Press Weapon and Confirm; confirm the selected enemy takes placeholder weapon damage.
15. Press a swap button and Confirm; confirm ally positions swap.
16. When an enemy is active on the timeline, press Confirm and confirm the enemy action resolves.
17. Reopen `Assets/Scenes/BattleScene.unity` and confirm the existing battle can still be opened independently.

### Changed Files

- `Assets/Scenes/BattleTimelinePrototypeScene.unity`
- `Assets/Scenes/BattleTimelinePrototypeScene.unity.meta`
- `Assets/Scripts/BattleTimelinePrototypeController.cs`
- `Assets/Scripts/BattleTimelinePrototypeController.cs.meta`
- `ProjectSettings/EditorBuildSettings.asset`
- `README.md`

## Implemented

- Added `BattleScene` and registered it in Build Settings.
- Added one player, one enemy, battle HP display, and internal grid position tracking.
- Added a 5-card hand, clickable card effects, and an end-turn button.
- Added simple enemy AI, victory/defeat checks, and an action log.
- Added a side-view 3x3 player panel and 3x3 enemy panel.
- Added card hover range preview and failure reasons for invalid attacks/movement.
- Added draw pile, hand, discard pile, turn-start draw, and discard reshuffle.
- Attack cards can now be used even when no target is in range; they miss and are discarded.
- Updated battle UI, card text, and action logs to Japanese display text.
- Card definitions are now managed as `CardData` ScriptableObject assets.
- Removed movement cards from the starter deck. Movement is now handled by always-available basic move commands.
- Added 3 player action points per turn. Normal cards and movement consume 1 action point.
- Added Clear Cards. `Guard`, `Repair`, and `Charge` are N cards that do not consume action points during normal player turns.
- Added the `ウエポン` basic command. It queues like an action, costs no action points, and is limited to once per player turn.
- Added action point UI. The current battle screen shows action points as a three-part gauge.
- Changed player turns from immediate action resolution to queued action planning.
- Added a temporary attack prediction interrupt system and Accel Gauge.
- Added a top-left Accel Gauge bar UI with gain effects and MAX blinking.
- Added `DeckBuildScene` MVP for editing, validating, saving, and using 30-card decks.
- Added `MenuScene` as the game entry menu with battle, deck edit, and placeholder RPG options.
- Added card attributes and enemy weakness attributes.
- Adjusted early battle balance around 1-2 turn normal fights and roughly 3 turn stage boss fights.
- Reorganized the BattleScene UI so the battle screen shows fewer persistent debug labels and uses larger, clearer text.
- Uses only generated rectangles, `Text`, and `Button`; no external assets are required.

## BattleScene UI Cleanup

The BattleScene UI has been reorganized to reduce always-visible debug information and make the panel battle easier to read.

- Removed the old `NEON CARDIA - 3x3パネルバトルMVP` title from BattleScene.
- Improved font readability with a lower Canvas reference resolution, larger text, stronger contrast, and generated shadow/outline effects.
- Changed the top-center turn display to `ROUND：1` plus `PLAYER TURN` / `ENEMY TURN`.
- Player HP is shown at the top-left as a number only.
- The Accel Gauge is placed next to the HP number and uses the label `ACCEEL`.
- Action points are shown below the ACCEEL gauge as a three-part gauge only. No `残り行動権`, `ACTION`, or `3/3` text is shown there.
- Draw pile, hand, and discard count text are hidden from the battle screen. The deck, hand, discard, draw, discard, and reshuffle logic still runs internally.
- The player-side and enemy-side 3x3 panels are visually connected into one 6-column by 3-row battle field. Internal `GridSide.Player` / `GridSide.Enemy` position management is unchanged.
- Removed `プレイヤーパネル` / `エネミーパネル` labels from BattleScene.
- Removed persistent right-side debug text such as enemy action count, enemy attack range, weakness, enemy type, Accel text, and attack prediction state. Enemy AI, weakness, attack prediction, and Accel logic are still active internally.
- The current enemy name is shown as a top-right battle UI label, matching the enemy-side name placement while debug-only enemy details remain hidden unless the debug panel is opened.
- Kept hand cards, movement buttons, `決定`, `選択リセット`, hover range preview, and the compact action log.

Changed files:

- `Assets/Scripts/BattleManager.cs`
- `Assets/Scripts/AccelGaugeUI.cs`
- `README.md`

## Battle Card UI And Hover Detail

The BattleScene hand cards now use a compact visual card layout instead of long text labels.

- Moved the action point gauge farther below the ACCEEL gauge so the two gauges do not visually crowd each other.
- Enemy HP is now shown as a floating number above the enemy panel cell and follows the enemy when it moves.
- Battle log UI is hidden during normal BattleScene play. `BattleLog` still records messages internally for debugging and logic flow.
- Hand cards no longer show `[N]`, `[HC]`, `[G]`, or `[CLEAR]` text.
- Hand card display is limited to card name, value, and a mini attribute icon.
- Card font sizes are larger than the previous text-heavy card layout.
- Card type is represented by color:
  - N: pale gray.
  - HC: sky blue.
  - G: pale red.
  - Clear Card: pale green background with the N / HC / G strip still visible at the edge.
- Attribute text is replaced by mini icons loaded from `Assets/Resources/UI/AttributeIcons`.
- Hovering a hand card opens a left-center detail panel with card name, placeholder artwork, effect text, value, attribute icon, range text, and any preview reason.
- Placeholder card artwork is loaded from `Assets/Resources/Cards/Placeholders`.
- The icon and artwork PNGs are simple original placeholder assets generated for this project. They are intended to be replaced later with production art.
- Runtime resolvers in `CardView.cs` create Sprites from those Resources textures, so replacing an asset only requires keeping the same file name or updating the resolver path.

Changed files and assets:

- `Assets/Scripts/BattleManager.cs`
- `Assets/Scripts/CardView.cs`
- `Assets/Resources/UI/AttributeIcons/*.png`
- `Assets/Resources/Cards/Placeholders/*.png`
- `README.md`

## Battle Effect Presentation

BattleScene now has a lightweight attack-effect presentation layer prepared for gpt-image-2 generated sprite sheets.

Effect folder layout:

- Hit effects: `Assets/Art/Effects/Hit/`
- Stage-change effects: `Assets/Art/Effects/Stage/`
- Placeholder backups: `Assets/Art/Effects/Placeholder/`
- Runtime presentation scripts: `Assets/Scripts/Battle/Presentation/`
- Reserved sprite output folder: `Assets/Sprites/Effects/`

Effect files:

- `WaterHit.png`
- `FireHit.png`
- `BreakHit.png`
- `IceHit.png`
- `WeaponHit.png`
- `EnemyHit.png`
- `StageChange_Ice.png`
- `StageChange_Grass.png`
- `StageChange_Magma.png`

EffectType mapping:

- `BattleEffectType.WaterHit` -> `Assets/Art/Effects/Hit/WaterHit.png`
- `BattleEffectType.FireHit` -> `Assets/Art/Effects/Hit/FireHit.png`
- `BattleEffectType.BreakHit` -> `Assets/Art/Effects/Hit/BreakHit.png`
- `BattleEffectType.IceHit` -> `Assets/Art/Effects/Hit/IceHit.png`
- `BattleEffectType.WeaponHit` -> `Assets/Art/Effects/Hit/WeaponHit.png`
- `BattleEffectType.EnemyHit` -> `Assets/Art/Effects/Hit/EnemyHit.png`
- `BattleEffectType.StageChange_Ice` -> `Assets/Art/Effects/Stage/StageChange_Ice.png`
- `BattleEffectType.StageChange_Grass` -> `Assets/Art/Effects/Stage/StageChange_Grass.png`
- `BattleEffectType.StageChange_Magma` -> `Assets/Art/Effects/Stage/StageChange_Magma.png`

Generated asset notes:

- The files in `Assets/Art/Effects/Hit/` and `Assets/Art/Effects/Stage/` are generated effect assets for the current project.
- They were generated for the image-2 effect request as 4x4 sprite-sheet-style images, then chroma-key processed into transparent PNGs.
- Final saved size is 1024x1024, intended as 4 columns x 4 rows / 16 frames, with 256x256 cells.
- Placeholder backup files remain in `Assets/Art/Effects/Placeholder/` and can be used if the generated assets are replaced or removed.
- All names are stable and are meant to be swapped later without changing `EffectAssetResolver`.

Generated effects and intended use:

- `WaterHit.png`: Water card hit effect.
- `FireHit.png`: Fire card hit effect.
- `BreakHit.png`: Break card hit effect.
- `IceHit.png`: Ice / freeze hit effect.
- `WeaponHit.png`: basic `ウエポン` command hit effect.
- `EnemyHit.png`: enemy attack hit effect on the player.
- `StageChange_Ice.png`: field-wide `フリーズステージ` effect.
- `StageChange_Grass.png`: field-wide `ソウゲンステージ` effect.
- `StageChange_Magma.png`: field-wide `カザンステージ` effect.

Implementation:

- `AttackEffectPlayer` creates `BattleEffectRoot` and `StageEffectRoot` at runtime under the BattleScene Canvas.
- `EffectAssetResolver` maps Water, Fire, Break, Ice, Weapon, Enemy, and stage-change actions to `BattleEffectType`.
- The generated PNGs are loaded by path in Editor and sliced at runtime by `AttackEffectPlayer`, so BattleScene can play them even if the Sprite Editor slice metadata is refreshed later.
- Card attacks show the card name, play the matching hit effect on the target panel, then apply damage and a damage popup.
- Weapon attacks play `WeaponHit`; enemy attacks play `EnemyHit`.
- Stage cards play `StageChange_Ice`, `StageChange_Grass`, or `StageChange_Magma` over the field before the panel update is visible.
- The effect animation is non-blocking in this MVP. Existing queued action resolution and battle logic remain synchronous.

Unity import settings for final assets:

- Texture Type: `Sprite`
- Sprite Mode: `Multiple`
- Pixels Per Unit: `100` as a starting point
- Mesh Type: `Full Rect`
- Filter Mode: `Bilinear` or `Point`
- Compression: `None` or low compression
- Alpha Is Transparency: enabled

Sprite slicing steps:

1. Select the effect PNG in Unity.
2. Set Sprite Mode to `Multiple`.
3. Open Sprite Editor.
4. Choose Slice, then `Grid By Cell Count`.
5. Set Column `4` and Row `4`.
6. Apply.

Replacing or regenerating image assets:

1. Generate a transparent 4x4 / 16-frame sprite sheet, or generate on a flat chroma-key background and remove the key color to alpha.
2. Save hit effects to `Assets/Art/Effects/Hit/`.
3. Save stage effects to `Assets/Art/Effects/Stage/`.
4. Keep the exact file names listed above.
5. Apply the Unity import and slicing settings.
6. Enter BattleScene Play Mode and trigger the matching card, weapon, enemy attack, or stage card.

Verification:

- In BattleScene, use Water, Fire, Break, and Ice cards and confirm the matching hit effect appears.
- Use `ウエポン` and confirm `WeaponHit` appears before damage.
- Let the enemy attack and confirm `EnemyHit` appears before damage.
- Use `フリーズステージ`, `ソウゲンステージ`, and `カザンステージ` and confirm a field-wide stage effect appears.
- Confirm damage popup text appears after a damaging attack.

Changed files and assets:

- `Assets/Scripts/BattleManager.cs`
- `Assets/Scripts/Battle/Presentation/AttackEffectPlayer.cs`
- `Assets/Art/Effects/Hit/*.png`
- `Assets/Art/Effects/Stage/*.png`
- `Assets/Art/Effects/Placeholder/*.png`
- `Assets/Sprites/Effects/.gitkeep`
- `README.md`

## Battle Weapon Command

BattleScene now has a basic `ウエポン` command in the lower operation UI.

- Moved `ウエポン`, `選択リセット`, and `決定` to the right side of the hand cards as a compact command column.
- `ウエポン` is a basic command, not a card. It does not enter the deck, hand, discard pile, or draw pile.
- `ウエポン` is added to the queued action list and resolves in order after pressing `決定`.
- `ウエポン` does not consume action points and does not reduce the action point gauge.
- `ウエポン` can be queued only once per player turn. `選択リセット` clears it from the current queue.
- Current temporary weapon effect: 10 power, Neutral attribute, same-row/front enemy attack. With the current single-enemy MVP, it hits the enemy when the enemy is on the player's row; otherwise it misses.
- The weapon performance is intentionally centralized in `BattleManager` constants so it can later be replaced by weapon data.
- The selected action queue is shown directly above the hand cards as horizontal card-like boxes, leaving the player-panel-left space available for hover card detail.
- Each queued action box shows its order number, action name, and an `ACT`, `FREE`, or `CLEAR` badge.
- Queued actions are color-coded: attack cards use blue, Clear Cards use pale green, movement uses yellow, Weapon uses gray/white, and fallback actions use a dark standard color.
- The queued action UI is rebuilt from the current queue count, so Clear Cards, Weapon, and future action-point increases can display more actions than the current three consuming actions.

Verification:

- Open `Assets/Scenes/BattleScene.unity` and enter Play Mode.
- Confirm `ウエポン`, `選択リセット`, and `決定` are grouped to the right of the hand cards.
- Queue `ウエポン` and confirm the action point gauge does not decrease.
- Queue `ウエポン` a second time in the same turn and confirm it is rejected.
- Press `選択リセット` and confirm the selected action list clears and `ウエポン` can be selected again.
- Press `決定` and confirm `ウエポン` resolves in queue order, deals 10 damage when the enemy is on the player's row, and misses otherwise.
- Confirm `ウエポン` does not change hand, draw pile, or discard pile behavior.
- Confirm selected cards, movement commands, Clear Cards, and `ウエポン` appear as horizontal boxes directly above the hand cards.
- Confirm the queued action boxes show order number, action name, color coding, and `ACT` / `FREE` / `CLEAR` badges.

Changed files:

- `Assets/Scripts/BattleManager.cs`
- `README.md`

## New N Cards

Four N cards were added to the card pool and are available in DeckBuildScene through `Resources.LoadAll<CardData>("Cards")`.

- `アクアショット`: N / normal card / Water / 40 power / forward single target. It can miss and is discarded normally. As a Water attack, it changes Magma panels in its route to Normal and can apply Frozen through the existing Ice-panel interaction.
- `バーナーブレス`: N / normal card / Fire / 60 power / forward 3 panels. It can miss and is discarded normally. As a Fire attack, it changes Grass panels in its route to Normal and deals double damage to targets standing on Grass panels.
- `テッキュウナゲ`: N / normal card / Break / 60 power / exactly 3 panels forward. It only hits a target exactly three panels ahead. It uses the existing Break interaction, so Frozen targets take double damage and thaw.
- `フリーズ`: N / Clear Card / Ice / no damage / forward single target. It applies Frozen to the first forward target, costs no action points during normal player turns, and is still consumed as the one prediction action during attack prediction. Fire-element units ignore the Frozen application, but the card is still used and discarded.

New range patterns:

- `ForwardSingle`: forward same-row target.
- `ForwardLine3`: the first three forward panels.
- `ForwardExactly3`: only the third forward panel.

Default deck update:

- The fallback 30-card starter deck now includes `アクアショット x2`, `バーナーブレス x2`, `テッキュウナゲ x2`, and `フリーズ x2`.
- Existing N-card counts were reduced to keep the deck at 30 cards and within the N same-name 4-copy limit.
- Saved PlayerPrefs decks are still respected. If a saved deck exists, edit it in DeckBuildScene to add the new cards.

UI notes:

- The new cards use the existing N-card and Clear Card visual rules.
- Water and Break attribute icons are generated at runtime if no PNG exists in `Assets/Resources/UI/AttributeIcons`, so the cards still show an attribute icon without external assets.

Verification:

- Open DeckBuildScene and confirm `アクアショット`, `バーナーブレス`, `テッキュウナゲ`, and `フリーズ` appear as N cards.
- Confirm the DeckValidator rejects five copies of the same new N card.
- In BattleScene, confirm `アクアショット` hits a forward same-row target for 40 Water damage and can change Magma panels to Normal.
- Confirm `バーナーブレス` hits targets within three forward panels for 60 Fire damage, changes Grass panels to Normal, and doubles damage on Grass.
- Confirm `テッキュウナゲ` only hits exactly three panels ahead for 60 Break damage and doubles damage plus clears Frozen on Frozen targets.
- Confirm `フリーズ` is shown as a Clear Card, costs no action points in normal turns, freezes a forward target, misses when no target is in range, and does not freeze Fire-element units.
- Confirm all four cards are discarded after resolution, including misses.

Changed files and assets:

- `Assets/Scripts/CardData.cs`
- `Assets/Scripts/BattleManager.cs`
- `Assets/Scripts/BattleText.cs`
- `Assets/Scripts/CardView.cs`
- `Assets/Scripts/DeckBuildManager.cs`
- `Assets/Resources/Cards/AquaShot.asset`
- `Assets/Resources/Cards/BurnerBreath.asset`
- `Assets/Resources/Cards/TekkyuNage.asset`
- `Assets/Resources/Cards/Freeze.asset`
- `README.md`

## Stage Change N Cards

Three N / Clear stage cards were added to the card pool and are available in DeckBuildScene through `Resources.LoadAll<CardData>("Cards")`.

- `フリーズステージ`: N / Clear Card / Ice / StageChange. It changes all player-side and enemy-side panels, 18 panels total, to `PanelType.Ice`. Units standing on those panels are moved to Ice visually, but no immediate Frozen effect is applied.
- `ソウゲンステージ`: N / Clear Card / Grass / StageChange. It changes all 18 panels to `PanelType.Grass`. Units standing on those panels do not receive immediate healing; Grass healing still happens only at turn start for Grass-element units.
- `カザンステージ`: N / Clear Card / Fire / StageChange. It changes all 18 panels to `PanelType.Magma`. If a unit's current panel becomes Magma during this card resolution, the existing Magma panel effect is applied immediately: normal units take 50 direct damage, and Fire-element units heal 50 without exceeding max HP.

Implementation notes:

- `CardEffectType.StageChange` and `CardData.TargetPanelType` were added so future cards can change the whole field to any `PanelType`.
- Stage cards are not attack cards, do not run target/miss checks, and resolve only after the player presses the confirm button.
- Because they are Clear Cards, they enter the action queue but do not consume normal-turn action points.
- They are still N cards for deck construction, so the same-name 4-copy limit applies and HC/G limits are unchanged.
- The fallback 30-card starter deck includes one copy each of `フリーズステージ`, `ソウゲンステージ`, and `カザンステージ`; Guard, Repair, and Freeze counts were reduced to keep the deck at 30 cards.
- BattleScene cards show `ICE`, `GRASS`, or `MAGMA` as the stage value. Hover detail uses existing attribute artwork when present, and generates a simple attribute-colored placeholder if no sprite exists yet.

Verification:

- Open DeckBuildScene and confirm `フリーズステージ`, `ソウゲンステージ`, and `カザンステージ` appear as N / Clear cards.
- Confirm DeckValidator rejects five copies of any one of those N cards.
- In BattleScene, queue each stage card and confirm the action point gauge does not decrease.
- Press confirm and verify `フリーズステージ` changes every player/enemy panel to Ice.
- Press confirm and verify `ソウゲンステージ` changes every player/enemy panel to Grass.
- Press confirm and verify `カザンステージ` changes every player/enemy panel to Magma and immediately applies the Magma effect to the player and enemy if they are standing on Magma.
- Confirm Fire-element units heal 50 from the immediate Magma effect, while normal units take 50 damage.
- Confirm Freeze and Grass stage changes do not apply immediate Frozen/healing effects.
- Confirm used stage cards are discarded after resolution and existing debug panel / Reset Battle State behavior still works.

Changed files and assets:

- `Assets/Scripts/CardData.cs`
- `Assets/Scripts/BattleManager.cs`
- `Assets/Scripts/BattleText.cs`
- `Assets/Scripts/CardView.cs`
- `Assets/Scripts/DeckBuildManager.cs`
- `Assets/Resources/Cards/FreezeStage.asset`
- `Assets/Resources/Cards/FreezeStage.asset.meta`
- `Assets/Resources/Cards/SougenStage.asset`
- `Assets/Resources/Cards/SougenStage.asset.meta`
- `Assets/Resources/Cards/KazanStage.asset`
- `Assets/Resources/Cards/KazanStage.asset.meta`
- `README.md`

## DeckBuildScene Scrolling Lists

DeckBuildScene now uses scrollable list panels for both the owned card list and the current deck list.

- The owned card list is placed inside a `ScrollRect` with a masked viewport.
- The current deck list also uses a `ScrollRect`, so long deck/card lists can be reviewed without overflowing behind the lower buttons.
- Mouse wheel scrolling is supported through the existing `InputSystemUIInputModule` scroll wheel binding.
- The list rows keep the existing add/remove click behavior.
- Scroll position is preserved when the UI refreshes after adding or removing cards.

Verification:

- Open DeckBuildScene and enter Play Mode.
- Hover the owned card list and use the mouse wheel to scroll through all owned cards.
- Confirm cards below the visible area can be reached and clicked.
- Hover the current deck list and scroll if the deck list exceeds the visible area.
- Confirm adding/removing cards still updates counts and validation.

Changed files:

- `Assets/Scripts/DeckBuildManager.cs`
- `README.md`

## Japanese UI Text

The current MVP uses fixed Japanese display text. It does not include a full localization or language switching system yet.

- BattleScene now keeps persistent battle labels minimal. Cards, buttons, logs, and deck builder text use Japanese, while the battle turn header uses `ROUND`, `PLAYER TURN`, and `ENEMY TURN`.
- Card names and descriptions are displayed in Japanese.
- Action logs are displayed in Japanese.
- Empty hand slots display `空き`.
- Card hover help displays `カードにカーソルを合わせると範囲を確認できます`.
- Shared display text and range descriptions are centralized in `BattleText.cs`.
- Runtime UI tries Japanese-capable OS fonts first, then falls back to Unity built-in fonts.

## 3x3 Panel System

- The battle field has two independent 3x3 grids: `PlayerPanel` on the left and `EnemyPanel` on the right.
- Positions are represented by `BattleGridPosition`, using `GridSide`, `Row`, and `Column`.
- Player units normally move only inside the player grid. Enemy units normally move only inside the enemy grid.
- Movement outside a 3x3 panel is rejected.
- Occupied destination panels are rejected.
- The old Back/Middle/Front position system was removed. Position management is unified under the 3x3 grid model.
- Panels currently have no permanent modifiers. There is no front-row damage bonus, back-row damage reduction, passive evasion, or passive defense from position.
- Cards no longer have use-position restrictions. A card can be selected from any panel position.
- Attack cards are usable even without a valid target. They miss, deal no damage, and are still discarded.
- Movement cards were removed from the starter deck and card assets. Movement is now a basic command.

## Actions And Movement

The player starts each turn with 3 action points. Actions are not resolved immediately.

- Clicking an attack, Guard, Repair, or Charge card adds it to the action queue.
- Clicking a basic movement command adds it to the action queue.
- Cards are not discarded when selected.
- Damage, healing, Guard, movement, and discard all happen after pressing `決定`.
- Normal cards and basic movement consume 1 action point.
- Clear Cards are queued but do not consume action points.
- Up to 3 action-point-consuming actions can be selected per player turn.
- Clear Cards can still be selected while action points are 0, as long as the card is in hand and is not already queued.
- `選択リセット` clears the current queue without consuming cards or changing position.
- The old `ターン終了` button is hidden in the queued action UI. `決定` is the button that resolves selected actions and advances the turn.
- Remaining action points are shown as a three-part gauge below the ACCEEL gauge. The battle screen no longer shows a text label such as `残り行動権 3 / 3`.
- The old persistent action queue debug text is hidden to keep the battle screen lighter. The queued action model still resolves selected actions in order after `決定`.

Basic movement commands are always shown during the player turn:

- `前進`: Move 1 panel toward the enemy side.
- `後退`: Move 1 panel toward the back of the player side.
- `上`: Move 1 panel up.
- `下`: Move 1 panel down.

Movement resolves after pressing `決定`. Movement fails if the destination is outside the 3x3 panel or occupied by another unit. In the queued action model, failed movement still counts as a resolved selected action, but it does not move the player. The action log shows the reason.

## Queued Turn Flow

Player turn flow:

1. Draw until hand reaches 5.
2. Select actions from cards and basic movement commands. Up to 3 selected actions may consume action points; Clear Cards do not count toward that limit.
3. Review the latest selection feedback in the action log.
4. Press `決定`.
5. Queued actions resolve from top to bottom.
6. Resolved card actions move the card from hand to discard.
7. Attack cards can still miss. Missed attacks are discarded.
8. After all player actions resolve, enemy actions resolve.
9. If battle is not over, the next round starts and the player draws back to 5 cards.

## Cards

Card definitions are stored as ScriptableObject assets under `Assets/Resources/Cards`.

Each `CardData` asset contains:

- Card ID
- Card name
- Description text
- Deck type (`N`, `HC`, or `G`)
- Clear Card flag
- Card effect type
- Power
- Cost
- Attribute
- Usable position
- Target type
- Target pattern
- Move direction

The current MVP keeps all existing cards usable from any position. `Cost` and `Usable Position` are stored for future systems, but no energy/cost payment or position restriction is enforced yet.

- `ストライク`: Slash. 同じ行の一番近い敵に35ダメージを与える。
- `ヘビーショット`: Shot. 同じ行の敵に70ダメージを与える。
- `ガード`: N / Clear / Neutral. 次に受けるダメージを40軽減する。Guard上限は80。
- `リペア`: N / Clear / Neutral. HPを45回復する。
- `チャージ`: N / Clear / Neutral. 次の攻撃カードのダメージを20増やす。

Attack cards can miss. If no enemy is in range, the card is still consumed, moves to discard, and logs the miss.

`Repair` can be used at full HP. It still consumes the card and logs that HP was already full.

## Clear Cards

Clear Card is separate from the deck-building type. A card can be `N`, `HC`, or `G`, and independently be either a normal card or a Clear Card.

Current Clear Cards:

- `ガード`: N card and Clear Card.
- `リペア`: N card and Clear Card.
- `チャージ`: N card and Clear Card.

Rules:

- Clear Cards do not consume action points during normal player turns.
- Clear Cards are still added to the action queue and resolve in queue order after pressing `決定`.
- Clear Cards are discarded after resolving, just like normal cards.
- Clear Cards still obey normal deck-building rules. `Guard`, `Repair`, and `Charge` are N cards, so each is limited to 4 copies by `DeckValidator`.
- Clear Cards do not change HC or G deck limits.
- During attack prediction, Clear Cards still consume the one allowed prediction interrupt action. This prevents unlimited actions during prediction.
- `Charge` currently gives `+20` damage to the next player attack card. The bonus is consumed by the next attack card, even if that attack misses.

## Range Model

Cards now carry a `CardTargetPattern` so more range shapes can be added without replacing the whole battle flow.

- `SameRowNearestEnemy`: Used by `Strike` and `Heavy Shot`.
- `ForwardOnePanel`: Implemented for future close-range cards.
- `Row`: Implemented for future horizontal-row effects.
- `SingleTarget`: Implemented as a future targeting hook.
- `AroundSelf`: Implemented as a future area-of-effect hook.

Position itself is only a coordinate. Tactical meaning comes from card ranges and movement effects.

## Special Panels

BattleScene now has a basic special panel system layered onto the existing 3x3 player grid and 3x3 enemy grid. The field still behaves as two sides internally, but each panel stores a `PanelType` and updates its color immediately when the type changes.

Panel types:

- `Normal`: movable, no special effect.
- `Cracked`: movable. When a unit successfully moves away from it, the origin panel changes into `Hole`.
- `Hole`: normally not movable. Units with the temporary `HasFloatAbility` flag can enter it. Ground attacks stop when their route reaches a hole panel.
- `Ice`: movable. Electric attacks deal double damage to units on it. Water attacks freeze units on it unless the unit is Fire element.
- `Grass`: movable. Fire attacks deal double damage to units on it. Fire attacks that pass through grass panels change those panels to `Normal`. Grass-element units heal 20% of max HP when their turn starts on grass.
- `Magma`: movable. Entering it deals 50 direct damage to normal units. Fire-element units heal 50 instead. Water attacks that pass through magma panels change those panels to `Normal`.
- `Poison`: movable. A unit starting its own turn on poison takes 20% of max HP as direct damage.

Status and attribute notes:

- `CardAttribute` now includes `Water`, `Grass`, and `Break`.
- `UnitElement` is currently `Neutral`, `Fire`, `Grass`, or `Ice`. Enemy element is set from the current `EnemyType` profile for debug testing.
- Frozen units skip their own turns. The first frozen turn is skipped, and the second frozen turn also skips while clearing Frozen afterward.
- Fire-element units cannot be frozen.
- Break attacks deal double damage to Frozen units and clear Frozen.
- Existing player card attacks are treated as `AttackTravelType.Ground` for now. `AttackTravelType.Air` exists as a future extension point.
- Player and enemy floating movement is a temporary boolean setting on `BattleManager`; a full ability system is not implemented yet.

Current MVP attack-route support:

- Same-row and row attacks process panels from the attacker forward through the lane.
- Forward-one-panel attacks process the immediate forward panel.
- Single-target attacks process the target panel.
- Around-self attacks process the target panel only when a target exists.
- Fire/Water route changes can occur even if no unit is hit, as long as the attack route passes through the panel.

BattleScene starts with all panels set to `Normal`. Use the special panel debug presets to place Cracked, Hole, Ice, Grass, Magma, and Poison panels for testing.
The current default deck has Fire and Electric attack cards, but Water and Break should be checked by temporarily changing a test card's `CardAttribute` in the ScriptableObject Inspector or by adding a test card asset.

## Special Panel Debug Tools

BattleScene now includes a runtime debug panel for testing special panels. It uses the same `PanelType` data that the battle system uses; there is no separate debug-only panel state.

Debug controls:

- BattleScene starts with the large `DEBUG PANEL` closed. Only the small `DEBUG` button is shown by default.
- Click the `DEBUG` button to open or close the full debug panel.
- Click the `X` button inside the panel to close it.
- Choose one of `Normal`, `Cracked`, `Hole`, `Ice`, `Grass`, `Magma`, or `Poison`.
- Click any player-side or enemy-side panel to change that panel to the selected type.
- The panel view refreshes immediately after the change.
- The debug panel writes changes to `Debug.Log`, for example `Debug: Panel (Player, row 1, col 2) changed to Ice`.
- F1 toggles the debug panel visibility during Editor or Development builds.
- F2 cycles the selected `PanelType` while the debug panel is visible.

Preset buttons:

- `All Normal`: resets all panels to `Normal`.
- `All Types`: places every special panel type across the 6x3 field.
- `Crack`, `Hole`, `Ice`, `Grass`, `Magma`, and `Poison`: place simple layouts for checking each panel behavior.
- `Reset Battle State`: restores the current battle state without reloading the scene, then resets every panel to `Normal`.

Enemy debug controls:

- `ENEMY DEBUG` is shown inside the same debug panel and is hidden whenever the debug tools are disabled.
- Use `<` / `>` to choose a test enemy profile, then press `Apply Enemy` to rebuild the battle with that enemy at its initial position.
- `Current:` shows the currently applied enemy type, and the spec line shows the selected profile's HP, element, weakness, action count, floating flag, and boss flag.
- Available profiles:
  - `NormalEnemy`: HP70, Neutral, Shot weakness, attack 20, 1 action, no float, normal enemy.
  - `FireEnemy`: HP90, Fire, Water weakness, attack 25, 1 action, no float, normal enemy. Use it to test Freeze immunity and Magma healing.
  - `GrassEnemy`: HP80, Grass, Fire weakness, attack 20, 1 action, no float, normal enemy. Use it to test Grass panel turn-start healing.
  - `IceEnemy`: HP80, Ice, Fire weakness, attack 20, 1 action, no float, normal enemy. Use it to test Ice panel, Electric, Water, and Frozen behavior.
  - `HeavyEnemy`: HP150, Neutral, Break weakness, attack 30, 1 action, no float, normal enemy. Use it to test high HP and Break interactions.
  - `FloatingEnemy`: HP70, Neutral, Electric weakness, attack 18, 1 action, floating, normal enemy. Use it to test Hole panel movement.
  - `Stage1Boss`: HP300, Neutral, Shot weakness, attack 35, 2 actions, no float, boss enemy.
- Applying an enemy profile resets enemy HP/max HP, element, weakness/AI profile, action count, attack power, float ability, guard/status state, boss pattern progress, and position. It also restarts the current battle state so panel, card, prediction, and UI state are clean for testing.
- `Reset Battle State` keeps the currently applied enemy type. For example, after applying `FireEnemy`, reset returns to a fresh `FireEnemy` battle rather than reverting to `NormalEnemy`.

Battle reset details:

- Player HP, position, element, guard, Frozen state, float test flag, action points, queued actions, prediction state, and Accel Gauge return to battle-start values.
- Enemy HP, position, type, element, guard, Frozen state, float test flag, AI turn state, action count, and boss pattern progress return to battle-start values.
- Panel layout is reset to all `Normal` panels. Normal panels are also the default at battle start; special panels are now introduced through cards or debug presets.
- Draw pile, hand, discard pile, and used cards are rebuilt from the same starting deck configuration. The shuffle order is re-randomized for this MVP.
- Round and turn state return to round 1 / player turn, and card hover/preview/selected action state is cleared.
- The reset writes `Debug: Battle state reset to initial state.` to the Console.
- This is an in-scene state restore using `BattleManager.DebugResetBattleToInitialState()`, not a scene reload.

Release/disable notes:

- `BattleManager.showDebugPanelTools` controls whether the tools are shown.
- The tools are only shown in `UNITY_EDITOR` or `DEVELOPMENT_BUILD`; non-development builds return false from `ShouldShowDebugPanelTools()`.
- To hide them during development, set `showDebugPanelTools` to false on the `BattleManager` component. This hides both the small `DEBUG` button and the full debug panel.
- You can also disable the runtime `Debug Toggle Root` and `Debug Panel Root` GameObjects while in Play Mode.

## Deck System

The player deck is managed by `DeckManager` and split into three zones:

- `drawPile`: Face-down deck used for drawing.
- `Hand`: Cards currently shown in the 5-card hand UI.
- `discardPile`: Cards successfully used from hand.

At battle start, `BattleManager` first tries to load a saved 30-card deck through `DeckStorage`. If the saved deck is missing or invalid, it falls back to the default deck created by `CardData.CreateStarterDeck()`.

Default 30-card deck:

- `ストライク` x4
- `ガード` x4
- `リペア` x4
- `ワイドショット` x4
- `クイックショット` x4
- `ピアースショット` x4
- `チャージ` x1
- `ヘビーショット` x1
- `ハイバースト` x1
- `ブーストガード` x1
- `ヒールフィールド` x1
- `レールキャノン` x1

Total: 30 cards. Movement cards are not included in the deck, draw pile, hand, or discard pile.

Turn-start draw rules:

- Hand limit is 5.
- At battle start and each new player turn, draw until hand reaches 5.
- If hand is already 5, draw 0 cards.
- If the draw pile is empty and cards are in discard, the discard pile is shuffled back into the draw pile.
- If there are still not enough cards, draw as many as possible.

Card use and discard rules:

- Successfully resolved cards are removed from hand and moved to discard.
- Attack cards with no valid target count as resolved misses and move to discard.
- Normal cards consume 1 player action point when selected for the queue. Clear Cards do not consume action points.
- `Repair` at full HP still resolves and moves to discard.
- `Charge` resolves as a Clear Card and gives the next player attack card +20 damage.
- When discard is recycled into the draw pile, the action log shows `捨て札をシャッフルして山札に戻しました。`

Deck UI:

- Shows draw pile count.
- Shows hand count and hand limit.
- Shows discard pile count.
- The action log shows how many cards were drawn at turn start.

## Deck Build Scene

`DeckBuildScene` is a functional MVP deck editor for the 30-card deck rule. The scene is registered in Build Settings and creates its temporary UI at runtime through `DeckBuildManager`.

Readability and layout updates:

- The old `NEON CARDIA - デッキビルドMVP` title is removed.
- The left owned-card list and right current-deck list now use most of the screen width and height.
- Canvas pixel-perfect rendering is enabled.
- All runtime text uses larger font sizes with Unity UI `Outline` and `Shadow`.
- Dark backing panels are used behind status text and list content.
- The deck count moved next to `現在のデッキ` and uses compact notation: `30/30   N:24   HC:5   G:1`.
- The label `デッキ枚数：` is no longer shown.
- TextMeshPro is not used; the improvement uses Unity standard UI only.

Main UI:

- Owned card list.
- Current deck list.
- Compact deck count and N / HC / G counts next to the current deck heading.
- Compact validation and message text.
- `デッキ保存` button.
- `バトルへ進む` button.
- `元に戻す` button.

Current MVP assumptions:

- All card assets under `Assets/Resources/Cards` are treated as owned cards.
- Move cards are excluded from owned card loading and cannot be added to a deck.
- The owned list displays card name, deck type, `CLEAR` tag, effect value, attribute, and current deck count.
- Clicking an owned card attempts to add it to the editing deck.
- Clicking a deck row removes one copy of that card.
- `元に戻す` restores the editing deck to the state it had when entering `DeckBuildScene`.
- `デフォルトデッキ作成` and `デッキ初期化` are no longer shown.

Deck rules are centralized in `DeckValidator`:

- Deck size is exactly 30 cards.
- `N` cards are limited to 4 copies of the same card.
- `HC` cards are limited to 5 total.
- `HC` cards cannot include duplicate card IDs.
- `G` cards are limited to 1 total.
- `Move` cards are invalid.

Saving and BattleScene linkage:

- `DeckStorage` saves the deck to PlayerPrefs key `NeonCardia.SavedDeck.CardIds`.
- The saved value is a comma-separated list of card IDs.
- `DeckBuildScene` saves before moving to `BattleScene`.
- `BattleManager` validates the saved deck at battle start and uses it if valid.
- If no saved deck exists, or the saved deck is invalid, BattleScene uses the default 30-card deck.

## Menu Scene

`MenuScene` is the current entry scene for the MVP and is registered first in Build Settings. The Build Settings scene order is:

- `MenuScene`
- `DeckBuildScene`
- `BattleScene`

The menu UI is generated at runtime by `MainMenuController.cs` and uses only Unity UI `Text`, `Image`, and `Button` objects. No TextMeshPro package, external assets, logos, character images, or external fonts are used.

Main layout:

- Large title display: `NEON CARDIA`.
- Subtitle: `ネオンカーディア / PANEL CARD BATTLE RPG`.
- Vertical menu buttons: `バトルへ`, `デッキ編集へ`, `RPGへ`.
- Top progress marker objects exist as `Progress Marker Root`, but are hidden by default until the future RPG progression system is implemented.
- Dark digital background with generated grid lines, circuit-like line decoration, translucent panels, and large symbol motifs.
- Footer text: `© 2026 NEON CARDIA PROJECT / PROTOTYPE VERSION / MVP BUILD`.

Readability adjustments:

- Canvas pixel-perfect rendering is enabled.
- Title, subtitle, menu items, message text, footer, and marker text use larger font sizes.
- Readable menu text uses Unity UI `Outline` and `Shadow`.
- Title, menu, message, and footer text have dark backing panels so grid lines do not sit directly behind the letters.
- The background grid and circuit decoration opacity is lower than the first menu MVP.
- `ShowProgressMarkers(bool visible)` can reveal or hide the future RPG progress marker group.
- The initial `↑ / ↓ で選択、Enterで決定` help text is hidden; message text appears only when needed, such as the RPG placeholder notice.

Menu behavior:

- `バトルへ` loads `BattleScene`.
- `デッキ編集へ` loads `DeckBuildScene`.
- `RPGへ` does not load a scene yet and displays `RPGモードはまだ未実装です`.
- Mouse hover changes the selected menu item.
- `↑` / `↓` changes menu selection.
- `Enter` confirms the selected item.
- `Escape` currently shows a prototype message and does not quit the application.

## Enemy AI

Enemy behavior is selected through `EnemyType`.

- `SmallEnemy`: HP40, 1 action, attack 15, range is same row nearest target.
- `NormalEnemy`: HP70, 1 action, attack 20, range is same row nearest target. Current default in BattleScene.
- `HeavyEnemy`: HP100, 1 action, attack 25, range is forward 1 panel or same row.
- `Stage1Boss`: HP300, 2 actions, attack 35, supports same-row, row attack, and strong attack patterns.

The MVP still spawns one enemy, but `BattleManager` exposes the enemy type so later scenes can switch to a heavier enemy or the stage boss. The current balance target is 1-2 turns for early normal fights and around 3 turns for a stage 1 boss fight.

Stage1Boss uses a simple pattern hook for future boss-specific actions:

- Pattern 1: Same-row boss shot.
- Pattern 2: Row attack.
- Pattern 3: Strong attack.
- The pattern repeats and can be expanded later without changing player queue handling.

## Attack Prediction And Accel Gauge

Attack prediction is a temporary interrupt system that triggers immediately before an enemy attack resolves.

Current MVP rules:

- Attack prediction chance is 100%.
- The chance is provided through `IAttackPredictionChanceProvider` / `TestAttackPredictionChanceProvider`.
- Enemy guide / encyclopedia progress is not implemented yet.
- Later, prediction chance is intended to be driven by enemy guide completion or enemy data.
- During attack prediction, the normal player action queue is not used.
- The player can take exactly one immediate prediction action.
- Prediction actions can be one basic movement command or one card from hand.
- Prediction movement resolves immediately.
- Prediction card use resolves immediately and then moves the card to discard.
- If movement leaves the enemy attack range, Accel Gauge increases by 20% and the enemy attack misses.
- If a normal enemy is defeated by a prediction card, Accel Gauge increases by 50%.
- If a normal enemy weakness is hit, Accel Gauge increases by 50% and the current enemy action is canceled.
- Normal enemy defeat and weakness rewards can stack, allowing +100%.
- If a boss weakness is hit, Accel Gauge increases by 50% and the current boss action is canceled.
- Defeating a boss does not grant Accel Gauge by itself unless a weakness was hit.
- Accel Gauge is clamped between 0% and 100%.
- Accel Gauge currently has no gameplay effect after filling.
- Current enemy action cancellation only cancels the attack being processed. If the enemy has multiple actions, the next enemy action may still proceed.

Temporary Accel carryover:

- A battle normally starts at 0%.
- If the previous battle ended with Accel Gauge at 50% or more, the next battle starts at 50%.
- If the previous battle ended below 50%, the next battle starts at 0%.
- This is currently held in a static temporary value, not save data.

## Accel Gauge UI

The Accel Gauge is generated at runtime by `BattleManager.BuildAccelGaugeUi()` using `AccelGaugeUI.cs`. It is anchored to the top-left of the Canvas, so it stays in the top-left when the screen size changes.

The UI is built only from Unity UI objects and uses no external assets:

- `AccelGaugeRoot`
- `GaugePanel`
- `GaugeBackground`
- `GaugeFill`
- `GaugeFrame`
- `GaugeLabelText`
- `GaugePercentText`
- `GaugeGainText`
- `GaugeFlashImage`
- `GaugeMaxText`

Display behavior:

- Shows the `ACCEEL` label and a horizontal Fill bar. The persistent percent text is hidden in the cleaned BattleScene UI.
- Fill width follows the 0-100 Accel Gauge value.
- Battle start and temporary carryover values are reflected immediately.
- If the value decreases in future systems, the Fill width will also decrease.
- When the gauge increases, the whole gauge pops slightly.
- Increase text such as `+20%`, `+50%`, or `+100%` floats near the gauge and fades out.
- Prediction success labels such as `回避成功`, `弱点ヒット`, or `予測成功` are shown with the gain text.
- The gauge flashes briefly when Accel Gauge increases.
- At 100%, the Fill and frame pulse gently and `MAX` appears.
- If the value drops below 100% in a future consume system, the blinking stops.

The 100% Accel Gauge gameplay effect is not implemented yet. Current work is UI feedback only.

Attributes:

- `Neutral`
- `Slash`
- `Shot`
- `Fire`
- `Ice`
- `Electric`

Enemy weaknesses:

- `SmallEnemy`: Slash
- `NormalEnemy`: Shot
- `HeavyEnemy`: Electric
- `Stage1Boss`: Shot

## Adding Cards

1. In Unity Editor, right-click in the Project window.
2. Select `Create > NeonCardia > Card Data`.
3. Save the new asset under `Assets/Resources/Cards` if it should be loadable by the default starter deck code.
4. Set the card ID, card name, description, deck type (`N`, `HC`, or `G`), Clear Card flag, effect type, power, cost, attribute, usable position, target type, target pattern, and move direction.
5. To make the card appear in `DeckBuildScene`, keep it under `Assets/Resources/Cards` and avoid `CardEffectType.Move`.
6. Deck legality is checked by `DeckValidator`, so new cards must fit the 30-card / N / HC / G rules.
7. To include the card in the current BattleScene deck without the deck builder, select the `Battle Manager` GameObject and add the card asset to the `Starter Deck` list in the Inspector.
8. If the `Starter Deck` list is empty, BattleManager uses a saved deck from `DeckStorage`, then falls back to the built-in default deck assembled from the existing assets in `Assets/Resources/Cards`.
9. Do not add movement cards to the starter deck. Basic movement is handled by the always-available movement buttons.

## Changed Files

- `Assets/Scripts/BattleManager.cs`
- `Assets/Scripts/Battle/Presentation/AttackEffectPlayer.cs`
- `Assets/Scripts/BattleDebugPanelController.cs`
- `Assets/Scripts/BattleDebugPanelController.cs.meta`
- `Assets/Scripts/BattleManager.cs.meta`
- `Assets/Scripts/AttackPredictionChanceProvider.cs`
- `Assets/Scripts/AttackPredictionChanceProvider.cs.meta`
- `Assets/Scripts/AccelGaugeUI.cs`
- `Assets/Scripts/AccelGaugeUI.cs.meta`
- `Assets/Scripts/CharacterUnit.cs`
- `Assets/Scripts/CharacterUnit.cs.meta`
- `Assets/Scripts/CardData.cs`
- `Assets/Scripts/CardData.cs.meta`
- `Assets/Scripts/DeckManager.cs`
- `Assets/Scripts/DeckManager.cs.meta`
- `Assets/Scripts/DeckValidationResult.cs`
- `Assets/Scripts/DeckValidationResult.cs.meta`
- `Assets/Scripts/DeckValidator.cs`
- `Assets/Scripts/DeckValidator.cs.meta`
- `Assets/Scripts/DeckStorage.cs`
- `Assets/Scripts/DeckStorage.cs.meta`
- `Assets/Scripts/PlayerCardCollection.cs`
- `Assets/Scripts/PlayerCardCollection.cs.meta`
- `Assets/Scripts/DeckBuildManager.cs`
- `Assets/Scripts/DeckBuildManager.cs.meta`
- `Assets/Scripts/MainMenuController.cs`
- `Assets/Scripts/MainMenuController.cs.meta`
- `Assets/Scripts/CardView.cs`
- `Assets/Scripts/CardView.cs.meta`
- `Assets/Scripts/EnemyAI.cs`
- `Assets/Scripts/EnemyAI.cs.meta`
- `Assets/Scripts/BattleLog.cs`
- `Assets/Scripts/BattleLog.cs.meta`
- `Assets/Scripts/BattleText.cs`
- `Assets/Scripts/BattleText.cs.meta`
- `Assets/Resources.meta`
- `Assets/Resources/Cards.meta`
- `Assets/Resources/Cards/Strike.asset`
- `Assets/Resources/Cards/Strike.asset.meta`
- `Assets/Resources/Cards/HeavyShot.asset`
- `Assets/Resources/Cards/HeavyShot.asset.meta`
- `Assets/Resources/Cards/Guard.asset`
- `Assets/Resources/Cards/Guard.asset.meta`
- `Assets/Resources/Cards/Repair.asset`
- `Assets/Resources/Cards/Repair.asset.meta`
- `Assets/Resources/Cards/WideShot.asset`
- `Assets/Resources/Cards/WideShot.asset.meta`
- `Assets/Resources/Cards/PierceShot.asset`
- `Assets/Resources/Cards/PierceShot.asset.meta`
- `Assets/Resources/Cards/QuickShot.asset`
- `Assets/Resources/Cards/QuickShot.asset.meta`
- `Assets/Resources/Cards/Charge.asset`
- `Assets/Resources/Cards/Charge.asset.meta`
- `Assets/Resources/Cards/HighBurst.asset`
- `Assets/Resources/Cards/HighBurst.asset.meta`
- `Assets/Resources/Cards/BoostGuard.asset`
- `Assets/Resources/Cards/BoostGuard.asset.meta`
- `Assets/Resources/Cards/HealField.asset`
- `Assets/Resources/Cards/HealField.asset.meta`
- `Assets/Resources/Cards/RailCannon.asset`
- `Assets/Resources/Cards/RailCannon.asset.meta`
- `Assets/Resources/Cards/GigantBreak.asset`
- `Assets/Resources/Cards/GigantBreak.asset.meta`
- `Assets/Resources/Cards/AquaShot.asset`
- `Assets/Resources/Cards/AquaShot.asset.meta`
- `Assets/Resources/Cards/BurnerBreath.asset`
- `Assets/Resources/Cards/BurnerBreath.asset.meta`
- `Assets/Resources/Cards/TekkyuNage.asset`
- `Assets/Resources/Cards/TekkyuNage.asset.meta`
- `Assets/Resources/Cards/Freeze.asset`
- `Assets/Resources/Cards/Freeze.asset.meta`
- `Assets/Resources/Cards/FreezeStage.asset`
- `Assets/Resources/Cards/FreezeStage.asset.meta`
- `Assets/Resources/Cards/SougenStage.asset`
- `Assets/Resources/Cards/SougenStage.asset.meta`
- `Assets/Resources/Cards/KazanStage.asset`
- `Assets/Resources/Cards/KazanStage.asset.meta`
- `Assets/Art/Effects/Hit/*.png`
- `Assets/Art/Effects/Stage/*.png`
- `Assets/Art/Effects/Placeholder/*.png`
- `Assets/Sprites/Effects/.gitkeep`
- `Assets/Scenes/BattleScene.unity`
- `Assets/Scenes/BattleScene.unity.meta`
- `Assets/Scenes/DeckBuildScene.unity`
- `Assets/Scenes/DeckBuildScene.unity.meta`
- `Assets/Scenes/MenuScene.unity`
- `Assets/Scenes/MenuScene.unity.meta`
- `ProjectSettings/EditorBuildSettings.asset`
- `README.md`

Removed files:

- `Assets/Resources/Cards/StepForward.asset`
- `Assets/Resources/Cards/StepForward.asset.meta`
- `Assets/Resources/Cards/StepBack.asset`
- `Assets/Resources/Cards/StepBack.asset.meta`
- `Assets/Resources/Cards/StepUp.asset`
- `Assets/Resources/Cards/StepUp.asset.meta`
- `Assets/Resources/Cards/StepDown.asset`
- `Assets/Resources/Cards/StepDown.asset.meta`

## Battle Result Overlay

BattleScene displays a runtime-generated battle result overlay after victory. It is not a scene transition: the result UI is created under the existing BattleScene Canvas, placed as the last sibling, and shown over the battle screen with a dark translucent background.

Displayed result contents:

- `HUNTING LEVEL`
- `S` / `A` / `B`
- One temporary reward card name chosen randomly
- ChatGPT Image 2 generated reward icon
- `もう一度戦う`
- `メニューへ戻る`

Result presentation:

- Victory is detected by the existing `CheckBattleEnd()` flow.
- The overlay waits briefly before appearing.
- The result background and panel fade in with `CanvasGroup`.
- Hunting level, rank, reward card name, and buttons fade in in sequence.
- The reward card name is a single line and is fit inside the lower reward frame.
- No external animation libraries are used.
- The result panel art and reward thumbnail were regenerated with ChatGPT Image 2 and stored under `Assets/Resources/UI/BattleResult`.
- The panel avoids a direct copy of the reference design: no red circular badge, no specific existing-game mark, one top data frame, and an original sci-fi frame silhouette.
- The hunting level letter is drawn as Unity text over the generated panel so `S` / `A` / `B` remains clearly visible and can update from evaluator output.

Text-safe layout:

- The ChatGPT Image 2 result panel is treated as background/frame art only.
- Result wording does not rely on text baked into the image. `HUNTING LEVEL`, rank, reward card name, and button labels are rendered by Unity UI on top of the image.
- Result text uses Unity standard `Text` with Best Fit, min/max font sizes, `VerticalWrapMode.Truncate`, and `RectMask2D` safe areas.
- The result panel defines dedicated safe `RectTransform` areas: `HuntingLevelArea`, `HuntingRankArea`, `RewardCardNameArea`, `RewardIconArea`, and `ResultButtonArea`.
- The upper large frame shows only `HUNTING LEVEL` and `S` / `A` / `B`.
- The lower frame shows only the reward card name; long names are resized and clipped inside `RewardCardNameArea`.
- The reward card name fades in through `CanvasGroup`.
- `ResultOverlayRoot` stretches over the screen, while `ResultPanel` is kept in a centered 1792:1024 aspect-fit area so the image and text keep their relative placement.
- The background image uses `preserveAspect` and is used only as frame/background decoration.
- A disabled-by-default `showResultSafeAreaDebug` flag can show thin safe-area guides during layout debugging.
- The upper frame text was nudged upward so `HUNTING LEVEL` and `S` / `A` / `B` read closer to the visual center of the generated frame.
- The lower reward card name area was nudged upward so the card name sits closer to the center of the long reward frame.
- The reward icon now uses centered normalized anchors inside `RewardIconArea`, with `preserveAspect` enabled and a slightly larger default scale.
- Runtime layout tuning values are exposed on `BattleResultOverlay`: `huntingLevelVerticalOffset`, `huntingRankVerticalOffset`, `rewardTextVerticalOffset`, `rewardIconScale`, and `rewardIconOffset`.
- Background images that contain embedded text should be replaced with textless assets such as `ResultPanel_NoText.png` / `ResultFrame_BackgroundOnly.png` in future art passes.

Initialization safety fix:

- `BattleResultOverlay.Build()` is no longer allowed to stop the normal BattleScene startup path.
- If result overlay construction throws, `BattleManager.BuildBattleResultOverlay()` logs the exception, destroys the partial overlay object, leaves `battleResultOverlay` as `null`, and allows `StartBattle()` to continue.
- This restores the normal startup refresh path: player/enemy creation, panel initialization, deck loading, initial hand draw, `RefreshUi()`, card view binding, enemy HP display, player HP display, and `ROUND` / turn display updates.
- The result overlay does not create `TextMeshProUGUI`, `TMP_FontAsset`, or any TMP runtime object, so Play Mode does not open the TMP Importer or try to import TMP Essentials.
- `StartBattle()` still calls `BattleResultOverlay.HideImmediate()` when the overlay exists, keeping the result root inactive with alpha `0`, `interactable = false`, and `blocksRaycasts = false`.
- If victory occurs and the overlay is missing because startup construction failed, `ShowBattleResult()` attempts to rebuild it from the stored BattleScene Canvas root before displaying the result.
- Normal Battle UI and Result UI remain separated under the same Canvas. The result overlay is hidden during battle and moved to the front only when shown after victory.

TMP Importer safety:

- TMP Essentials are treated as an Editor-side manual setup step, not a runtime dependency.
- The result overlay uses Unity standard `Text`, `Image`, `Button`, `CanvasGroup`, `RectMask2D`, and `AspectRatioFitter`.
- If TMP Essentials are not imported, the result overlay still displays because it does not touch TMP APIs.
- This prevents `Cannot import package in play mode` from being triggered by the battle result UI.

Battle result records:

- `BattleResultData.isBossBattle`: derived from the active `EnemyType` via `EnemyAI.IsBoss()`.
- `BattleResultData.victoryTurn`: the current BattleScene `ROUND` value when victory is detected.
- `BattleResultData.playerDamageTakenCount`: increments only when the player's HP actually decreases.
- `BattleResultData.maxSimultaneousDefeatCount`: stores the largest defeat batch size. Current single-enemy battles record up to `1`; the tracker already accepts larger future batch values.
- `BattleResultData.huntingLevel`: calculated by `HuntingLevelEvaluator`.

Damage taken count rules:

- Enemy attacks count only when HP decreases after Guard.
- Poison and Magma panel damage also count when they reduce player HP.
- Damage `0` and fully guarded damage do not count.

Normal enemy hunting level:

- `S`: victory on turn 1, or 2+ simultaneous defeats by turn 2.
- `A`: victory on turn 2, or player damage taken exactly 1 time.
- `B`: victory on turn 3 or later, or player damage taken 2+ times.
- `S` is prioritized when its condition is met. After that, `B` downgrade conditions are checked before `A` so 2+ damage taken remains `B`.

Boss hunting level:

- `S`: victory within 3 turns.
- `A`: victory on turn 4, or player damage taken exactly 3 times.
- `B`: victory on turn 5 or later, or player damage taken 4+ times. Damage taken 6+ times is also `B`.
- `S` is prioritized when its condition is met. After that, `B` downgrade conditions are checked before `A`.

Temporary rewards:

- Current MVP displays one random reward card name only. Credits, rates, damage count, turn count, and long reward descriptions are hidden from the result panel for now.

- `S`: randomly displays one of `フリーズ`, `バーナーブレス`, or `テッキュウナゲ`.
- `A`: randomly displays one of `仮カード`, `アクアショット`, or `フリーズ`.
- `B`: randomly displays one of `仮カード`, `アクアショット`, or `テッキュウナゲ`.
- Rewards are display-only for now; no inventory, credit wallet, or card acquisition storage is updated yet.
- The reward card image is a temporary ChatGPT Image 2 generated asset.

Result buttons:

- `もう一度戦う`: restarts the current BattleScene battle state with the current enemy type and initial Accel Gauge baseline.
- `メニューへ戻る`: loads `MenuScene`.

Battle result manual checks:

1. Open `Assets/Scenes/BattleScene.unity` and enter Play Mode.
2. Defeat a normal enemy and confirm the result overlay appears over the BattleScene instead of changing scenes.
3. Confirm the overlay fades in after a short wait and one reward line fades in inside the lower reward frame.
4. Defeat a normal enemy on round 1 and confirm hunting level `S`.
5. Defeat a normal enemy on round 2 and confirm hunting level `A` if damage taken is below 2.
6. Defeat a normal enemy on round 3 or later and confirm hunting level `B`.
7. Take player HP damage 2+ times in a normal battle and confirm the result is `B` unless an `S` condition was met.
8. Use the debug enemy selector to apply `Stage1Boss`.
9. Defeat `Stage1Boss` within 3 rounds and confirm hunting level `S`.
10. Defeat `Stage1Boss` on round 4 and confirm hunting level `A` if damage taken is below 4.
11. Take player HP damage 4+ times in a boss battle and confirm the result is `B` unless an `S` condition was met.
12. Confirm `NORMAL BATTLE`, `DAMAGE TAKEN`, `TURN`, `RESULT`, `WIN`, and `GET DATA` are not displayed on the result overlay.
13. Confirm the upper large frame contains only `HUNTING LEVEL` and the `S` / `A` / `B` rank.
14. Confirm `HUNTING LEVEL` and the rank look vertically centered in the upper frame and do not touch the decorative border.
15. Confirm the lower frame contains only the reward card name, the reward card name is vertically centered, and it fades in.
16. Confirm the reward icon is slightly larger than before, centered in the lower-right icon frame, and keeps its aspect ratio.
17. Resize the Game view within 16:9 sizes and confirm the background image keeps its aspect ratio and Unity text stays aligned to the frame.
18. Press `もう一度戦う` and confirm BattleScene restarts without breaking cards, special panels, attack prediction, Accel Gauge, or deck handling.
19. Press `メニューへ戻る` and confirm `MenuScene` loads.

Changed files for battle result:

- `Assets/Scripts/BattleManager.cs`
- `Assets/Scripts/BattleResultData.cs`
- `Assets/Scripts/BattleStatsTracker.cs`
- `Assets/Scripts/HuntingLevelEvaluator.cs`
- `Assets/Scripts/BattleResultOverlay.cs`
- `Assets/Resources/UI/BattleResult.meta`
- `Assets/Resources/UI/BattleResult/ResultPanel.png`
- `Assets/Resources/UI/BattleResult/ResultPanel.png.meta`
- `Assets/Resources/UI/BattleResult/RewardCard.png`
- `Assets/Resources/UI/BattleResult/RewardCard.png.meta`
- `Assets/Scripts/BattleResultData.cs.meta`
- `Assets/Scripts/BattleStatsTracker.cs.meta`
- `Assets/Scripts/HuntingLevelEvaluator.cs.meta`
- `Assets/Scripts/BattleResultOverlay.cs.meta`
- `README.md`

Battle result items not implemented yet:

- Real reward grant/storage.
- Reward rarity rolls.
- Multiple enemy UI and target selection.
- Multi-enemy simultaneous defeat detection beyond the tracker API.
- Result sound effects and production animation polish.

Battle result startup repair notes:

- Cause investigated: `BuildUi()` creates the normal battle UI first, then calls `BuildBattleResultOverlay()`, and `Awake()` calls `StartBattle()` only after `BuildUi()` returns.
- Because of that order, any exception while building the result overlay can stop before `StartBattle()`, leaving generated UI frames visible but preventing player HP, enemy display, enemy HP, hand cards, and `ROUND` / turn text from being populated by `RefreshUi()`.
- The result overlay build is now isolated with exception handling so normal BattleScene initialization continues even if result UI setup fails.
- Victory still calls `ShowBattleResult()`. If the result overlay was not available at startup, it is rebuilt from the stored BattleScene Canvas root before display.
- The result overlay starts hidden and non-interactive: inactive root, alpha `0`, `interactable = false`, and `blocksRaycasts = false`.
- Current result display fields are `HUNTING LEVEL`, `S` / `A` / `B`, one random reward card name, `もう一度戦う`, and `メニューへ戻る`.
- The normal battle UI and result UI remain separate. Hiding or rebuilding `ResultOverlayRoot` does not clear CardView, EnemyView, player HP, enemy HP, or round/turn text.
- Verification focus: after entering BattleScene Play Mode, confirm player HP `180`, Accel Gauge, action gauge, `ROUND: 1` / `PLAYER TURN`, player cell, normal enemy HP `70`, card names, card values, and attribute icons appear before any result overlay is shown.
## Script Structure

`BattleMvpController.cs` was replaced by the following split structure. `BattleScene` keeps the same script GUID through `BattleManager.cs`, so it can still be opened and played directly.

- `BattleManager.cs`: Scene entry point, 3x3 grid UI, queued player actions, Clear Card action-point handling, Charge damage bonus, attack prediction interrupt handling, Accel Gauge, card range resolution, basic movement commands, action count, turn handling, and victory/defeat checks.
- `BattleResultData.cs`: Battle result DTO with boss flag, victory turn, damage taken count, simultaneous defeat count, and hunting level.
- `BattleStatsTracker.cs`: Runtime battle stat recording for player damage taken and max simultaneous enemy defeats.
- `HuntingLevelEvaluator.cs`: Normal enemy and boss enemy hunting level evaluation rules.
- `BattleResultOverlay.cs`: Runtime-generated result overlay UI, Unity standard Text safe-area layout, fade sequence, reward card name reveal, retry button, and menu button.
- `CharacterUnit.cs`: Player/enemy HP, capped Guard, damage, healing, `GridSide`, and `BattleGridPosition`.
- `CardData.cs`: ScriptableObject card definitions, deck type, Clear Card flag, attributes, card metadata, and starter deck loading.
- `DeckManager.cs`: Card instances, shuffled draw pile, hand, discard pile, draw-to-limit, and discard recycling.
- `DeckValidationResult.cs`: Validation result object with deck counts, error list, and display message.
- `DeckValidator.cs`: 30-card deck validation and add-card rule checks for N / HC / G cards.
- `DeckStorage.cs`: PlayerPrefs save/load for the active deck card ID list.
- `PlayerCardCollection.cs`: MVP owned-card provider that loads all non-movement card assets from `Assets/Resources/Cards`.
- `DeckBuildManager.cs`: Runtime UI and editing flow for `DeckBuildScene`.
- `MainMenuController.cs`: Runtime UI, keyboard/mouse menu selection, placeholder progress markers, and scene navigation for `MenuScene`.
- `CardView.cs`: Compact hand card visuals, card color rules, attribute icon resolver, placeholder artwork resolver, hover detail panel, click callback, and hover preview callback.
- `EnemyAI.cs`: Enemy type HP/attack/weakness profile selection and 3x3 panel movement/attack planning.
- `BattleLog.cs`: Bounded action log storage and display text formatting.
- `BattleText.cs`: Japanese display text, range descriptions, and grid position formatting.
- `AttackPredictionChanceProvider.cs`: Temporary 100% attack prediction chance provider for future enemy guide integration.
- `AccelGaugeUI.cs`: Runtime-generated top-left Accel Gauge bar, fill update, gain text, flash, pop, and MAX blink behavior.

## Manual Test Steps

1. Open the project in Unity Editor.
2. Open `Assets/Scenes/MenuScene.unity`.
3. Enter Play Mode.
4. Confirm that the `NEON CARDIA` title, subtitle, digital grid background, circuit-like decorations, and footer are visible.
5. Confirm that the title, subtitle, menu items, message text, and footer are sharp and readable.
6. Confirm that text is not buried in the grid because dark backing panels are behind the text areas.
7. Confirm that the top progress marker labels are not visible on initial display.
8. Confirm that `バトルへ`, `デッキ編集へ`, and `RPGへ` are shown as vertical menu buttons.
9. Confirm that mouse hover or `↑` / `↓` changes the selected menu and the `▶` marker moves.
10. Confirm that `RPGへ` shows `RPGモードはまだ未実装です` and does not transition.
11. Confirm that `バトルへ` loads `BattleScene`.
12. Return to `MenuScene`, then confirm that `デッキ編集へ` loads `DeckBuildScene`.
13. Open `Assets/Scenes/BattleScene.unity`.
14. Enter Play Mode.
15. Confirm that the old `NEON CARDIA - 3x3パネルバトルMVP` title is not shown.
16. Confirm that the top-center display shows `ROUND：1` and `PLAYER TURN` or `ENEMY TURN`.
17. Confirm that player HP is shown at the top-left as a number only.
18. Confirm that the `ACCEEL` gauge is to the right of the HP number.
19. Confirm that the action point gauge is below the ACCEEL gauge, has visible spacing from it, and has no text label.
20. Confirm that the player and enemy 3x3 panels are connected visually as one 6-column by 3-row field.
21. Confirm that `プレイヤーパネル` and `エネミーパネル` labels are not shown.
22. Confirm that draw pile, hand, and discard count text are not shown.
23. Confirm that enemy action count, attack range, weakness, enemy type, and attack prediction debug text are not shown on the right side.
24. Confirm that visible BattleScene text is sharper and easier to read.
25. Confirm that BattleScene does not show the battle log UI.
26. Confirm that enemy HP appears as a number above the enemy unit.
27. Move the enemy through enemy actions if possible and confirm the enemy HP number follows the enemy cell.
    - Confirm that the current enemy name is shown in the top-right battle UI area.
28. Confirm that hand cards do not show `[N]`, `[HC]`, `[G]`, or `[CLEAR]` text.
29. Confirm that N cards are pale gray, HC cards are sky blue, G cards are pale red, and Clear Cards are pale green with an edge strip.
30. Confirm that hand cards show only card name, value, and a mini attribute icon.
31. Confirm that attribute text is not shown on hand cards.
32. Confirm that card font size is larger than the old text-heavy card layout.
33. Hover a card and confirm that the left-center detail panel appears.
34. Confirm that the detail panel shows card name, placeholder artwork, rules text, value, attribute icon, and range text.
35. Move the cursor off the card and confirm the detail panel hides.
36. Confirm that `前進`, `後退`, `上`, and `下` movement buttons are visible outside the hand area.
37. Confirm that the old `ターン終了` button is not shown and only `決定` / `選択リセット` are shown in that command area.
38. Confirm that `Step Forward`, `Step Back`, `Step Up`, and `Step Down` do not appear in hand.
39. Confirm that the default deck is a 30-card deck and movement cards are not included.
40. Click a card and confirm that no damage, healing, Guard, discard, or turn advance happens immediately.
41. Click a movement button and confirm that the player does not move immediately.
42. Confirm that selecting Guard, Repair, or Charge does not reduce remaining action points.
43. Confirm that selecting Strike, Heavy Shot, or a movement command reduces remaining action points by 1.
44. Confirm that the queue can contain Clear Cards plus up to 3 action-point-consuming actions.
45. Press `選択リセット` and confirm the queue clears without consuming cards.
46. Select actions and press `決定`; confirm actions resolve in order.
47. Confirm that resolved Clear Cards move to discard internally by seeing later hand refill behavior.
48. Queue Charge before an attack and confirm that the next attack gains +20 damage.
49. Move off the enemy row and queue `ストライク`; press `決定` and confirm that it misses and is discarded internally.
50. Queue movement, press `決定`, and confirm movement resolves at that timing.
51. Try an invalid queued movement and confirm the player does not move.
52. Confirm that player action resolution is followed by enemy action.
53. Confirm that Accel Gauge, attack prediction state, predicted enemy range, enemy weakness, and range-in/out logic still work internally even though their debug text is hidden.
54. Confirm that the top-left ACCEEL Gauge bar is visible with frame, background, and Fill.
55. Confirm that 0% is empty, 20% is around one fifth, 50% is around half, and 100% is full.
56. Confirm that enemy attacks trigger attack prediction every time in the current test build.
57. During attack prediction, click a movement command and confirm it resolves immediately instead of entering the normal queue.
58. During attack prediction, click one card, including a Clear Card if available, and confirm the prediction action ends after that one card.
59. Move out of the predicted enemy attack range and confirm Accel Gauge increases by 20%, `+20%` appears, the gauge flashes, and the enemy attack misses.
60. Use a weakness card during prediction and confirm Accel Gauge increases by 50%, `+50%` appears, and the current enemy attack is canceled.
61. Defeat a normal enemy with a prediction card and confirm Accel Gauge increases by 50%.
62. Confirm that Accel Gauge never exceeds 100%.
63. Confirm that the gauge blinks and shows `MAX` at 100%.
64. Confirm that draw pile, hand, and discard count text stays hidden, while resolved card actions still discard cards and refill the hand on later turns.
65. Use cards over several turns and confirm that hand refills to 5 at turn start.
66. Empty the draw pile and confirm that discard is shuffled back into the draw pile.
67. Confirm that the current enemy has the new HP balance value.
68. Reduce enemy HP to 0 and confirm `VICTORY`.
69. Let player HP reach 0 and confirm `DEFEAT`.
70. Confirm that special panels are visible by color in BattleScene: Cracked, Hole, Ice, Grass, Magma, and Poison.
71. Move a unit away from a Cracked panel and confirm the origin panel changes to Hole.
72. Try to move a normal unit onto a Hole panel and confirm movement fails.
73. Enable `playerHasFloatAbility` or `enemyHasFloatAbility` in the `BattleManager` Inspector and confirm that floating movement can enter Hole panels.
74. Move onto a Magma panel and confirm normal units take 50 direct damage; set the unit element to Fire and confirm Magma heals 50 instead.
75. Start a turn on Poison and confirm the unit takes 20% max HP damage.
76. Set a unit element to Grass, start its turn on Grass, and confirm it heals 20% max HP.
77. Use a Fire attack through Grass and confirm the Grass panel changes to Normal.
78. Temporarily set a test attack card to Water, use it through Magma, and confirm the Magma panel changes to Normal.
79. Put a target on Ice and use an Electric attack to confirm damage is doubled.
80. Put a target on Ice and use a temporary Water attack to confirm Frozen is applied unless the target is Fire element.
81. Confirm that a Frozen unit skips its turn and clears Frozen on the second skipped frozen turn.
82. Temporarily set a test attack card to Break, use it on a Frozen unit, and confirm damage is doubled and Frozen is cleared.
83. Put a Hole panel in a ground attack route and confirm the attack stops at the hole.
    Debug panel checks:
    - Confirm that only the small `DEBUG` button appears initially in BattleScene while running in Editor or Development builds.
    - Press the `DEBUG` button and confirm that `DEBUG PANEL` appears.
    - Press the `DEBUG` button again and confirm that `DEBUG PANEL` closes.
    - Press the `X` button inside the panel and confirm that `DEBUG PANEL` closes.
    - Select each `PanelType` button and confirm the `Selected:` line updates.
    - Click player-side and enemy-side panels and confirm they change to the selected panel type immediately.
    - Confirm that panel changes are written to the Console with `Debug:` logs.
    - Press `All Normal` and confirm every panel returns to `Normal`.
    - Press `All Types` and confirm all special panel colors appear on the field.
    - Press each focused preset button and confirm the layout changes.
    - Press `Reset Battle State` and confirm all panels become `Normal`, while units, HP, hand, discard, round, turn, action queue, prediction state, and Accel Gauge return to battle-start state.
    - Press F1 and confirm the debug panel hides/shows.
    - Press F2 and confirm the selected `PanelType` cycles.
    - Use the `ENEMY DEBUG` `<` / `>` buttons and confirm the selected enemy profile changes.
    - Press `Apply Enemy` for `NormalEnemy`, `FireEnemy`, `GrassEnemy`, `IceEnemy`, `HeavyEnemy`, `FloatingEnemy`, and `Stage1Boss`, and confirm the enemy HP number updates to 70, 90, 80, 80, 150, 70, and 300 respectively.
    - Confirm `FireEnemy` ignores Frozen and heals from Magma, `GrassEnemy` heals on Grass at turn start, `FloatingEnemy` can enter Hole panels, and `Stage1Boss` takes 2 actions.
    - Apply a non-default enemy, press `Reset Battle State`, and confirm the same enemy type remains active.
    - Set `showDebugPanelTools` false on `BattleManager` and confirm the `DEBUG` button and full debug panel stay hidden.
84. Open `Assets/Scenes/DeckBuildScene.unity`.
85. Enter Play Mode and confirm that all deck builder text is sharper and easier to read.
86. Confirm that `NEON CARDIA - デッキビルドMVP` is not shown.
87. Confirm that the owned card list is large on the left and the current deck list is large on the right.
88. Confirm that `デフォルトデッキ作成` and `デッキ初期化` are not shown.
89. Confirm that `デッキ保存`, `バトルへ進む`, and `元に戻す` are shown.
90. Confirm that `現在のデッキ` has compact counts next to it, such as `30/30   N:24   HC:5   G:1`.
91. Confirm that the label `デッキ枚数：` is not shown.
92. Confirm that Guard, Repair, and Charge show `[N][CLEAR]` in the deck builder.
93. Click owned cards and confirm that they are added to the editing deck.
94. Click deck rows and confirm that one copy is removed.
95. Press `元に戻す` and confirm that the editing deck returns to the state it had when entering DeckBuildScene.
96. Confirm that Guard, Repair, and Charge are still limited as N cards with 4 copies per card.
97. Confirm that N cards cannot exceed 4 copies of the same card.
98. Confirm that HC cards cannot exceed 5 total and cannot include duplicate card IDs.
99. Confirm that G cards cannot exceed 1 total.
100. Confirm that decks below 30 cards are invalid.
101. Press `デッキ保存` and confirm that the save message appears.
102. Press `バトルへ進む` and confirm that BattleScene starts with the saved deck.

## Not Implemented Yet

- ScriptableObject enemy and deck data.
- RPG mode scene and RPG progression logic.
- Real progress marker state, unlock conditions, and save integration.
- Full localization and language switching.
- Energy and card cost payment.
- Dedicated boss scene setup.
- Detailed boss telegraph UI and separate enemy ScriptableObject profiles.
- Enemy guide / encyclopedia progress and prediction chance scaling.
- Accel Gauge spend effects.
- Full Accel Gauge consume flow and gameplay effects at 100%.
- Production-quality effect sprites, sound effects, and animation polish for Accel Gauge.
- Multiple units per side, clickable target selection, panel ownership, panel breaking, obstacles, traps, and summons.
- Full use of all range patterns by real cards.
- Additional status effects beyond the temporary Frozen MVP, plus full buff/debuff UI.
- Dedicated Water and Break card assets in the default deck.
- Animations, sound effects, card art, and polish.
- Real card acquisition, reward storage, multiple deck slots, ability-based HC/G limit increases, and full post-battle progression flow.
- Dedicated Clear Card deck-building limits or advanced Clear Card-specific rules.
- Production-quality deck builder scrolling, filtering, sorting, and card-detail UI.
- Automated tests.
