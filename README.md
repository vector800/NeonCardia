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
- Added 3 player action points per turn. Normal cards and movement consume 1 action point.
- Added Clear Cards. `Guard`, `Repair`, and `Charge` are N cards that do not consume action points during normal player turns.
- Added a dedicated remaining-action UI panel with a large count and three action markers.
- Changed player turns from immediate action resolution to queued action planning.
- Added a temporary attack prediction interrupt system and Accel Gauge.
- Added a top-left Accel Gauge bar UI with gain effects and MAX blinking.
- Added `DeckBuildScene` MVP for editing, validating, saving, and using 30-card decks.
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
- Remaining action points are shown in a dedicated center UI panel as `残り行動権 3 / 3` plus three action markers.
- The action queue UI shows current action point cost and marks Clear Cards as `[CLEAR / 行動権消費なし]`.

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

Main UI:

- Owned card list.
- Current deck list.
- Deck count and N / HC / G counts.
- Validation and message text.
- `デフォルトデッキ作成` button.
- `デッキ初期化` button.
- `デッキ保存` button.
- `BattleSceneへ進む` button.

Current MVP assumptions:

- All card assets under `Assets/Resources/Cards` are treated as owned cards.
- Move cards are excluded from owned card loading and cannot be added to a deck.
- The owned list displays card name, deck type, `CLEAR` tag, effect value, attribute, and current deck count.
- Clicking an owned card attempts to add it to the editing deck.
- Clicking a deck row removes one copy of that card.

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
4. Set the card ID, card name, description, deck type (`N`, `HC`, or `G`), Clear Card flag, effect type, power, cost, attribute, usable position, target type, target pattern, and move direction.
5. To make the card appear in `DeckBuildScene`, keep it under `Assets/Resources/Cards` and avoid `CardEffectType.Move`.
6. Deck legality is checked by `DeckValidator`, so new cards must fit the 30-card / N / HC / G rules.
7. To include the card in the current BattleScene deck without the deck builder, select the `Battle Manager` GameObject and add the card asset to the `Starter Deck` list in the Inspector.
8. If the `Starter Deck` list is empty, BattleManager uses a saved deck from `DeckStorage`, then falls back to the built-in default deck assembled from the existing assets in `Assets/Resources/Cards`.
9. Do not add movement cards to the starter deck. Basic movement is handled by the always-available movement buttons.

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
- `Assets/Scenes/BattleScene.unity`
- `Assets/Scenes/BattleScene.unity.meta`
- `Assets/Scenes/DeckBuildScene.unity`
- `Assets/Scenes/DeckBuildScene.unity.meta`
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

- `BattleManager.cs`: Scene entry point, 3x3 grid UI, queued player actions, Clear Card action-point handling, Charge damage bonus, attack prediction interrupt handling, Accel Gauge, card range resolution, basic movement commands, action count, turn handling, and victory/defeat checks.
- `CharacterUnit.cs`: Player/enemy HP, capped Guard, damage, healing, `GridSide`, and `BattleGridPosition`.
- `CardData.cs`: ScriptableObject card definitions, deck type, Clear Card flag, attributes, card metadata, and starter deck loading.
- `DeckManager.cs`: Card instances, shuffled draw pile, hand, discard pile, draw-to-limit, and discard recycling.
- `DeckValidationResult.cs`: Validation result object with deck counts, error list, and display message.
- `DeckValidator.cs`: 30-card deck validation and add-card rule checks for N / HC / G cards.
- `DeckStorage.cs`: PlayerPrefs save/load for the active deck card ID list.
- `PlayerCardCollection.cs`: MVP owned-card provider that loads all non-movement card assets from `Assets/Resources/Cards`.
- `DeckBuildManager.cs`: Runtime UI and editing flow for `DeckBuildScene`.
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
6. Confirm that the dedicated `残り行動権 3 / 3` panel and three action markers are easy to see near the center of the screen.
7. Confirm that `前進`, `後退`, `上`, and `下` movement buttons are visible outside the hand area.
8. Confirm that the old `ターン終了` button is not shown and only `決定` / `選択リセット` are shown in that command area.
9. Confirm that `Step Forward`, `Step Back`, `Step Up`, and `Step Down` do not appear in hand.
10. Confirm that the default deck is a 30-card deck and movement cards are not included.
11. Click a card and confirm that no damage, healing, Guard, discard, or turn advance happens immediately.
12. Click a movement button and confirm that the player does not move immediately.
13. Confirm that selected actions appear in `選択中アクション` in selection order.
14. Confirm that Guard, Repair, and Charge show `[N][CLEAR]`.
15. Confirm that selecting Guard, Repair, or Charge does not reduce remaining action points.
16. Confirm that selecting Strike, Heavy Shot, or a movement command reduces remaining action points by 1.
17. Confirm that the queue can contain Clear Cards plus up to 3 action-point-consuming actions.
18. Confirm that Clear Cards are shown in the queue with `[CLEAR / 行動権消費なし]`.
19. Press `選択リセット` and confirm the queue clears without consuming cards.
20. Select actions and press `決定`; confirm actions resolve in order.
21. Confirm that resolved Clear Cards move to discard.
22. Queue Charge before an attack and confirm that the next attack gains +20 damage.
23. Move off the enemy row and queue `ストライク`; press `決定` and confirm that it misses, is discarded, and discard count increases.
24. Queue movement, press `決定`, and confirm movement resolves at that timing.
25. Try an invalid queued movement and confirm the failure reason appears when `決定` resolves it.
26. Confirm that player action resolution is followed by enemy action.
27. Confirm that the enemy action count and attack range description are visible.
28. Confirm that Accel Gauge, attack prediction state, predicted enemy range, enemy weakness, and range-in/out state are visible.
29. Confirm that the top-left Accel Gauge bar is visible with frame, background, Fill, and percent text.
30. Confirm that 0% is empty, 20% is around one fifth, 50% is around half, and 100% is full.
31. Confirm that enemy attacks trigger attack prediction every time in the current test build.
32. During attack prediction, click a movement command and confirm it resolves immediately instead of entering the normal queue.
33. During attack prediction, click one card, including a Clear Card if available, and confirm the prediction action ends after that one card.
34. Move out of the predicted enemy attack range and confirm Accel Gauge increases by 20%, `+20%` appears, the gauge flashes, and the enemy attack misses.
35. Use a weakness card during prediction and confirm Accel Gauge increases by 50%, `+50%` appears, and the current enemy attack is canceled.
36. Defeat a normal enemy with a prediction card and confirm Accel Gauge increases by 50%.
37. Confirm that Accel Gauge never exceeds 100%.
38. Confirm that the gauge blinks and shows `MAX` at 100%.
39. Confirm that draw pile, hand, and discard counts update after resolved card actions.
40. Use cards over several turns and confirm that hand refills to 5 at turn start.
41. Empty the draw pile and confirm that discard is shuffled back into the draw pile.
42. Confirm that the current enemy has the new HP balance value.
43. Reduce enemy HP to 0 and confirm `勝利`.
44. Let player HP reach 0 and confirm `敗北`.
45. Open `Assets/Scenes/DeckBuildScene.unity`.
46. Enter Play Mode and confirm that the owned card list and current deck list are visible.
47. Confirm that Guard, Repair, and Charge show `[N][CLEAR]` in the deck builder.
48. Click owned cards and confirm that they are added to the editing deck.
49. Click deck rows and confirm that one copy is removed.
50. Confirm that deck count, N count, HC count, and G count update immediately.
51. Confirm that Guard, Repair, and Charge are still limited as N cards with 4 copies per card.
52. Confirm that N cards cannot exceed 4 copies of the same card.
53. Confirm that HC cards cannot exceed 5 total and cannot include duplicate card IDs.
54. Confirm that G cards cannot exceed 1 total.
55. Confirm that decks below 30 cards are invalid.
56. Press `デフォルトデッキ作成` and confirm that a valid 30-card deck is created.
57. Press `デッキ保存` and confirm that the save message appears.
58. Press `BattleSceneへ進む` and confirm that BattleScene starts with the saved deck.

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
- Card acquisition, rewards, multiple deck slots, ability-based HC/G limit increases, and post-battle flow.
- Dedicated Clear Card deck-building limits or advanced Clear Card-specific rules.
- Production-quality deck builder scrolling, filtering, sorting, and card-detail UI.
- Automated tests.
