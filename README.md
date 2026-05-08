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
- Movement cards still fail if their destination is invalid.

## Cards

- `ストライク`: 同じ行の一番近い敵に6ダメージを与える。
- `ヘビーショット`: 同じ行の敵に14ダメージを与える。
- `ガード`: 次に受けるダメージを8軽減する。
- `前進`: 敵側に近い方向へ1マス移動する。
- `後退`: 自陣奥へ1マス移動する。
- `上移動`: 上に1マス移動する。
- `下移動`: 下に1マス移動する。
- `リペア`: HPを7回復する。

Attack cards can miss. If no enemy is in range, the card is still consumed, moves to discard, and logs the miss.

Movement cards still require a valid destination. If a movement card cannot resolve, it is not consumed and the turn does not advance. Examples include:

- Destination is outside the panel.
- Destination is occupied.

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

- `ストライク` x4
- `ヘビーショット` x2
- `ガード` x2
- `リペア` x2
- `前進` x2
- `後退` x2
- `上移動` x1
- `下移動` x1

Turn-start draw rules:

- Hand limit is 5.
- At battle start and each new player turn, draw until hand reaches 5.
- If hand is already 5, draw 0 cards.
- If the draw pile is empty and cards are in discard, the discard pile is shuffled back into the draw pile.
- If there are still not enough cards, draw as many as possible.

Card use and discard rules:

- Successfully resolved cards are removed from hand and moved to discard.
- Attack cards with no valid target count as resolved misses and move to discard.
- Failed movement cards are not consumed and remain in hand.
- Movement failure examples include destination outside the panel or occupied destination.
- `Repair` at full HP still resolves and moves to discard.
- When discard is recycled into the draw pile, the action log shows `捨て札をシャッフルして山札に戻しました。`

Deck UI:

- Shows draw pile count.
- Shows hand count and hand limit.
- Shows discard pile count.
- The action log shows how many cards were drawn at turn start.

## Enemy AI

Enemy behavior is selected through `EnemyType`.

- `MeleeEnemy`: Moves toward the player's row and attacks more easily from the same row.
- `ShooterEnemy`: Current default. Attacks from the same row and retreats when low on HP.
- `GuardEnemy`: Guards often and attacks on a periodic timing.

The MVP still spawns one enemy, but the AI shape is ready for adding more enemy profiles.

## Changed Files

- `Assets/Scripts/BattleManager.cs`
- `Assets/Scripts/BattleManager.cs.meta`
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
- `Assets/Scenes/BattleScene.unity`
- `Assets/Scenes/BattleScene.unity.meta`
- `ProjectSettings/EditorBuildSettings.asset`
- `README.md`

## Script Structure

`BattleMvpController.cs` was replaced by the following split structure. `BattleScene` keeps the same script GUID through `BattleManager.cs`, so it can still be opened and played directly.

- `BattleManager.cs`: Scene entry point, 3x3 grid UI, card range resolution, movement, turn handling, and victory/defeat checks.
- `CharacterUnit.cs`: Player/enemy HP, Guard, damage, healing, `GridSide`, and `BattleGridPosition`.
- `CardData.cs`: Card definitions and range pattern data.
- `DeckManager.cs`: Card instances, shuffled draw pile, hand, discard pile, draw-to-limit, and discard recycling.
- `CardView.cs`: One hand card button, label, color state, click callback, and hover preview callback.
- `EnemyAI.cs`: Enemy type selection and 3x3 panel movement/attack behavior.
- `BattleLog.cs`: Bounded action log storage and display text formatting.
- `BattleText.cs`: Japanese display text, range descriptions, and grid position formatting.

## Manual Test Steps

1. Open the project in Unity Editor.
2. Open `Assets/Scenes/BattleScene.unity`.
3. Enter Play Mode.
4. Confirm that the left 3x3 grid shows the player and the right 3x3 grid shows the enemy.
5. Confirm that visible UI labels, card names, card descriptions, and action logs are displayed in Japanese.
6. Hover cards and confirm that range preview cells and Japanese range text update.
7. Click `ストライク` or `ヘビーショット` while aligned with the enemy row and confirm damage/log output.
8. Move with `前進`, `後退`, `上移動`, and `下移動`.
9. Try moving outside the panel and confirm that the card is not consumed and a Japanese reason is logged.
10. Move off the enemy row and try `ストライク`; confirm that it misses, is discarded, and discard count increases.
11. Use `リペア` at full HP and confirm that it is discarded and logs that HP was already full.
12. Use `ガード`, press `ターン終了`, and confirm that incoming enemy damage is reduced.
13. Confirm that draw pile, hand, and discard counts update after card use.
14. Use cards over several turns and confirm that hand refills to 5 at turn start.
15. Empty the draw pile and confirm that discard is shuffled back into the draw pile.
16. Reduce enemy HP to 0 and confirm `勝利`.
17. Let player HP reach 0 and confirm `敗北`.

## Not Implemented Yet

- ScriptableObject card, enemy, and deck data.
- Full localization and language switching.
- Energy and card costs.
- Multiple units per side, clickable target selection, panel ownership, panel breaking, obstacles, traps, and summons.
- Full use of all range patterns by real cards.
- Status effects, buffs, and debuffs.
- Animations, sound effects, card art, and polish.
- Save data, rewards, deck editing, and post-battle flow.
- Automated tests.
