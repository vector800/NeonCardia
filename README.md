# NeonCardia Battle MVP

## Implemented

- Added `BattleScene` and registered it in Build Settings.
- Added one player, one enemy, HP/Guard display, and current position display.
- Added a 5-card hand, clickable card effects, and an end-turn button.
- Added simple enemy AI, victory/defeat checks, and an action log.
- Uses only generated rectangles, `Text`, and `Button`; no external assets are required.

## Cards

- `Strike`: Deal 6 damage to the enemy.
- `Heavy Shot`: Front-only attack. Deal 14 damage to the enemy.
- `Guard`: Reduce the next incoming damage by 8.
- `Step Forward`: Move one position forward.
- `Step Back`: Move one position back.
- `Repair`: Recover 7 HP.

## Changed Files

- `Assets/Scripts/BattleManager.cs`
- `Assets/Scripts/BattleManager.cs.meta`
- `Assets/Scripts/CharacterUnit.cs`
- `Assets/Scripts/CharacterUnit.cs.meta`
- `Assets/Scripts/CardData.cs`
- `Assets/Scripts/CardData.cs.meta`
- `Assets/Scripts/CardView.cs`
- `Assets/Scripts/CardView.cs.meta`
- `Assets/Scripts/EnemyAI.cs`
- `Assets/Scripts/EnemyAI.cs.meta`
- `Assets/Scripts/BattleLog.cs`
- `Assets/Scripts/BattleLog.cs.meta`
- `Assets/Scenes/BattleScene.unity`
- `Assets/Scenes/BattleScene.unity.meta`
- `ProjectSettings/EditorBuildSettings.asset`
- `README.md`

## Script Structure

`BattleMvpController.cs` was replaced by the following split structure. `BattleScene` keeps the same script GUID through `BattleManager.cs`, so it can still be opened and played directly.

- `BattleManager.cs`: Scene entry point, battle flow, runtime UI generation, card resolution, turn handling, and victory/defeat checks.
- `CharacterUnit.cs`: Player/enemy HP, Guard, damage, healing, and battle position helpers.
- `CardData.cs`: Card definitions, card instances, and runtime deck/hand/discard logic.
- `CardView.cs`: One hand card button, label, color state, and click callback.
- `EnemyAI.cs`: Enemy turn behavior and position-based attack damage.
- `BattleLog.cs`: Bounded action log storage and display text formatting.

## Manual Test Steps

1. Open the project in Unity Editor.
2. Open `Assets/Scenes/BattleScene.unity`.
3. Enter Play Mode.
4. Click cards in the hand and confirm that each effect appears in the action log.
5. Use `Step Forward` to move to Front, then confirm that `Heavy Shot` can be used.
6. Use `Guard`, press `END TURN`, and confirm that incoming enemy damage is reduced.
7. Reduce enemy HP to 0 and confirm `VICTORY`.
8. Let player HP reach 0 and confirm `DEFEAT`.

## Not Implemented Yet

- ScriptableObject card, enemy, and deck data.
- Energy, card costs, draw pile UI, and discard pile UI.
- Multiple enemies, target selection, status effects, buffs, and debuffs.
- Animations, sound effects, card art, and polish.
- Save data, rewards, deck editing, and post-battle flow.
- Automated tests.
