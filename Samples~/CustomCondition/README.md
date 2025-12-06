# Custom Condition Sample

Shows how to create a custom condition type for your game's specific needs.

## What It Shows

- Creating a `ConditionAsset` (ScriptableObject for designer setup)
- Implementing `IConditionInstance` (runtime condition logic)
- Event-driven condition evaluation
- Progress tracking with `GetProgressText()`

## Structure

1. **EnemyKilledCondition** - Asset class with designer-facing properties
2. **EnemyKilledConditionInstance** - Runtime logic that subscribes to events
3. **EnemyKilledEvent** - Custom event for enemy deaths

## Usage

1. Create asset: Right-click → Create → Quest Samples → Conditions → Enemy Killed
2. Configure enemy type and required kills
3. Use in quest objectives like any built-in condition
4. Trigger events in your game: `EventManager.Instance.Raise(new EnemyKilledEvent { EnemyType = "Goblin" })`

**~70 lines** for a complete custom condition with event handling.
