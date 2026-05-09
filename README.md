# NeonCardia Battle MVP

## Implemented

- Added `BattleScene` and registered it in Build Settings.
- Added one player, one enemy, HP/Guard display, and current grid position display.
- Added a 5-card hand, clickable card effects, and an end-turn button.
- Added simple enemy AI, victory/defeat checks, and an action log.
- Added a side-view 3x3 player panel and 3x3 enemy panel.
- Added card hover range preview and failure reasons for invalid attacks/movement.
- Added draw pile, hand, discard pile, turn-start draw, and discard reshuffle.
- Attack cards can now be used even when no target is in range; they miss and are discarded.
- Updated battle UI, card text, and action logs to Japanese display text.
- Card definitions are now managed as `CardData` ScriptableObject assets.
- Removed movement cards from the starter deck. Movement is now handled by always-available basic move commands.
- Added 3 player actions per turn. Cards and successful basic movement each consume 1 action.
- Added a dedicated remaining-action UI panel with a large count and three action markers.
- Changed player turns from immediate action resolution to queued action planning.
- Added a temporary attack prediction interrupt system and Accel Gauge.
- Added a top-left Accel Gauge bar UI with gain effects and MAX blinking.
- Added card attributes and enemy weakness attributes.
- Adjusted early battle balance around 1-2 turn normal fights and roughly 3 turn stage boss fights.
- Uses only generated rectangles, `Text`, and `Button`; no external assets are required.

## Japanese UI Text

The current MVP uses fixed Japanese display text. It does not include a full localization or language switching system yet.

- Main battle labels use Japanese text such as `プレイヤー`, `エネミー`, `プレイヤーターン`, `ターン終了`, `プレイヤーパネル`, and `エネミーパネル`.
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

The player starts each turn with 3 action selections. Actions are not resolved immediately.

- Clicking an attack, Guard, or Repair card adds it to the action queue.
- Clicking a basic movement command adds it to the action queue.
- Cards are not discarded when selected.
- Damage, healing, Guard, movement, and discard all happen after pressing `決定`.
- Up to 3 actions can be selected per player turn.
- `選択リセット` clears the current queue without consuming cards or changing position.
- The old `ターン終了` button is hidden in the queued action UI. `決定` is the button that resolves selected actions and advances the turn.
- Remaining selections are shown in a dedicated center UI panel as `残り選択可能数 3 / 3` plus three action markers.

Basic movement commands are always shown during the player turn:

- `前進`: Move 1 panel toward the enemy side.
- `後退`: Move 1 panel toward the back of the player side.
- `上`: Move 1 panel up.
- `下`: Move 1 panel down.

Movement resolves after pressing `決定`. Movement fails if the destination is outside the 3x3 panel or occupied by another unit. In the queued action model, failed movement still counts as a resolved selected action, but it does not move the player. The action log shows the reason.

## Queued Turn Flow

Player turn flow:

1. Draw until hand reaches 5.
2. Select up to 3 actions from cards and basic movement commands.
3. Review the `選択中アクション` queue.
4. Press `決定`.
5. Queued actions resolve from top to bottom.
6. Resolved card actions move the card from hand to discard.
7. Attack cards can still miss. Missed attacks are discarded.
8. After all player actions resolve, enemy actions resolve.
9. If battle is not over, the next round starts and the player draws back to 5 cards.

## Cards

Card definitions are stored as ScriptableObject assets under `Assets/Resources/Cards`.

Each `CardData` asset contains:

- Card name
- Description text
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
- `ガード`: Neutral. 次に受けるダメージを40軽減する。Guard上限は80。
- `リペア`: Neutral. HPを45回復する。

Attack cards can miss. If no enemy is in range, the card is still consumed, moves to discard, and logs the miss.

`Repair` can be used at full HP. It still consumes the card and logs that HP was already full.

## Range Model

Cards now carry a `CardTargetPattern` so more range shapes can be added without replacing the whole battle flow.

- `SameRowNearestEnemy`: Used by `Strike` and `Heavy Shot`.
- `ForwardOnePanel`: Implemented for future close-range cards.
- `Row`: Implemented for future horizontal-row effects.
- `SingleTarget`: Implemented as a future targeting hook.
- `AroundSelf`: Implemented as a future area-of-effect hook.

Position itself is only a coordinate. Tactical meaning comes from card ranges and movement effects.

## Deck System

The player deck is managed by `DeckManager` and split into three zones:

- `drawPile`: Face-down deck used for drawing.
- `Hand`: Cards currently shown in the 5-card hand UI.
- `discardPile`: Cards successfully used from hand.

The starter deck is created at battle start, shuffled, and then drawn from.

Starter deck:

- `ストライク` x8
- `ヘビーショット` x4
- `ガード` x2
- `リペア` x2

Total: 16 cards. Movement cards are not included in the deck, draw pile, hand, or discard pile.

Turn-start draw rules:

- Hand limit is 5.
- At battle start and each new player turn, draw until hand reaches 5.
- If hand is already 5, draw 0 cards.
- If the draw pile is empty and cards are in discard, the discard pile is shuffled back into the draw pile.
- If there are still not enough cards, draw as many as possible.

Card use and discard rules:

- Successfully resolved cards are removed from hand and moved to discard.
- Attack cards with no valid target count as resolved misses and move to discard.
- Cards consume 1 player action when they resolve.
- `Repair` at full HP still resolves and moves to discard.
- When discard is recycled into the draw pile, the action log shows `捨て札をシャッフルして山札に戻しました。`

Deck UI:

- Shows draw pile count.
- Shows hand count and hand limit.
- Shows discard pile count.
- The action log shows how many cards were drawn at turn start.

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

- Shows `アクセル 0%` through `アクセル 100%`.
- Fill width follows the 0-100 Accel Gauge value.
- Battle start and temporary carryover values are reflected immediately.
- If the value decreases in future systems, the Fill width and percent text will also decrease.
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
4. Set the card name, description, effect type, power, cost, attribute, usable position, target type, target pattern, and move direction.
5. To include the card in the current BattleScene deck without code changes, select the `Battle Manager` GameObject and add the card asset to the `Starter Deck` list in the Inspector.
6. If the `Starter Deck` list is empty, BattleManager falls back to the built-in starter deck assembled from the existing assets in `Assets/Resources/Cards`.
7. Do not add movement cards to the starter deck. Basic movement is handled by the always-available movement buttons.

## Changed Files

- `Assets/Scripts/BattleManager.cs`
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
- `Assets/Scenes/BattleScene.unity`
- `Assets/Scenes/BattleScene.unity.meta`
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

## Script Structure

`BattleMvpController.cs` was replaced by the following split structure. `BattleScene` keeps the same script GUID through `BattleManager.cs`, so it can still be opened and played directly.

- `BattleManager.cs`: Scene entry point, 3x3 grid UI, queued player actions, attack prediction interrupt handling, Accel Gauge, card range resolution, basic movement commands, action count, turn handling, and victory/defeat checks.
- `CharacterUnit.cs`: Player/enemy HP, capped Guard, damage, healing, `GridSide`, and `BattleGridPosition`.
- `CardData.cs`: ScriptableObject card definitions, attributes, card metadata, and starter deck loading.
- `DeckManager.cs`: Card instances, shuffled draw pile, hand, discard pile, draw-to-limit, and discard recycling.
- `CardView.cs`: One hand card button, label, color state, click callback, and hover preview callback.
- `EnemyAI.cs`: Enemy type HP/attack/weakness profile selection and 3x3 panel movement/attack planning.
- `BattleLog.cs`: Bounded action log storage and display text formatting.
- `BattleText.cs`: Japanese display text, range descriptions, and grid position formatting.
- `AttackPredictionChanceProvider.cs`: Temporary 100% attack prediction chance provider for future enemy guide integration.
- `AccelGaugeUI.cs`: Runtime-generated top-left Accel Gauge bar, fill update, gain text, flash, pop, and MAX blink behavior.

## Manual Test Steps

1. Open the project in Unity Editor.
2. Open `Assets/Scenes/BattleScene.unity`.
3. Enter Play Mode.
4. Confirm that the left 3x3 grid shows the player and the right 3x3 grid shows the enemy.
5. Confirm that visible UI labels, enemy type, round count, remaining actions, card names, card descriptions, and action logs are displayed in Japanese.
6. Confirm that the dedicated `残り選択可能数 3 / 3` panel and three action markers are easy to see near the center of the screen.
7. Confirm that `前進`, `後退`, `上`, and `下` movement buttons are visible outside the hand area.
8. Confirm that the old `ターン終了` button is not shown and only `決定` / `選択リセット` are shown in that command area.
9. Confirm that `Step Forward`, `Step Back`, `Step Up`, and `Step Down` do not appear in hand.
10. Confirm that the starter deck contains only `ストライク`, `ヘビーショット`, `ガード`, and `リペア`.
11. Click a card and confirm that no damage, healing, Guard, discard, or turn advance happens immediately.
12. Click a movement button and confirm that the player does not move immediately.
13. Confirm that selected actions appear in `選択中アクション` in selection order.
14. Confirm that only up to 3 actions can be selected.
15. Press `選択リセット` and confirm the queue clears without consuming cards.
16. Select actions and press `決定`; confirm actions resolve in order.
17. Move off the enemy row and queue `ストライク`; press `決定` and confirm that it misses, is discarded, and discard count increases.
18. Queue movement, press `決定`, and confirm movement resolves at that timing.
19. Try an invalid queued movement and confirm the failure reason appears when `決定` resolves it.
20. Confirm that player action resolution is followed by enemy action.
21. Confirm that the enemy action count and attack range description are visible.
22. Confirm that Accel Gauge, attack prediction state, predicted enemy range, enemy weakness, and range-in/out state are visible.
23. Confirm that the top-left Accel Gauge bar is visible with frame, background, Fill, and percent text.
24. Confirm that 0% is empty, 20% is around one fifth, 50% is around half, and 100% is full.
25. Confirm that enemy attacks trigger attack prediction every time in the current test build.
26. During attack prediction, click a movement command and confirm it resolves immediately instead of entering the normal queue.
27. During attack prediction, click one card and confirm it resolves immediately and moves to discard.
28. Move out of the predicted enemy attack range and confirm Accel Gauge increases by 20%, `+20%` appears, the gauge flashes, and the enemy attack misses.
29. Use a weakness card during prediction and confirm Accel Gauge increases by 50%, `+50%` appears, and the current enemy attack is canceled.
30. Defeat a normal enemy with a prediction card and confirm Accel Gauge increases by 50%.
31. Confirm that Accel Gauge never exceeds 100%.
32. Confirm that the gauge blinks and shows `MAX` at 100%.
33. Confirm that draw pile, hand, and discard counts update after resolved card actions.
34. Use cards over several turns and confirm that hand refills to 5 at turn start.
35. Empty the draw pile and confirm that discard is shuffled back into the draw pile.
36. Confirm that the current enemy has the new HP balance value.
37. Reduce enemy HP to 0 and confirm `勝利`.
38. Let player HP reach 0 and confirm `敗北`.

## Not Implemented Yet

- ScriptableObject enemy and deck data.
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
- Status effects, buffs, and debuffs.
- Animations, sound effects, card art, and polish.
- Save data, rewards, deck editing, and post-battle flow.
- Automated tests.
