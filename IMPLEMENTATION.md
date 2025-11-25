# Unity Quest Core – Technical Implementation (v0.1)

## Project Status

This is the initial implementation scaffold for the Generic Quest Core system as defined in `specs.md`.

## What's Implemented

### Core Runtime (`Assets/GenericQuestCore/Runtime/Core/`)

✅ **Data Model (ScriptableObjects)**
- `QuestAsset.cs` - Quest definition with objectives
- `ObjectiveAsset.cs` - Objective definition with conditions and prerequisites
- `ConditionAsset.cs` - Base class for all condition types
- `ConditionGroupAsset.cs` - Composite conditions (AND/OR logic)

✅ **Runtime State Management**
- `QuestState.cs` - Runtime state of a quest
- `ObjectiveState.cs` - Runtime state of an objective
- `QuestLog.cs` - Registry of active quests
- `QuestStatus.cs` - Status enums for quests and objectives

✅ **Condition System**
- `IConditionInstance.cs` - Core condition interface with Bind/Unbind lifecycle
- `ConditionGroupInstance.cs` - Composite condition evaluation (AND/OR)
- `ItemCollectedConditionAsset.cs` & `ItemCollectedConditionInstance.cs` - Example condition
- `TimeElapsedConditionAsset.cs` & `TimeElapsedConditionInstance.cs` - Example condition

✅ **Infrastructure**
- `QuestContext.cs` - Service container for quest runtime (area, inventory, time services)
- `IQuestEventBus.cs` - Event bus interface for condition subscriptions
- `QuestManager.cs` - MonoBehaviour that orchestrates quest evaluation and event polling
- `QuestPlayerRef.cs` - Helper to build QuestContext from game services

### Event Management Adapter (`Assets/GenericQuestCore/Runtime/EventManagementAdapter/`)

⚠️ **EventManagementQuestBus.cs** - Placeholder adapter
- Currently a stub; needs integration with actual `mechaniqe/event-management` library
- TODO: Map to the real EventManager API

### Testing (`Tests/`)

✅ **Test Infrastructure**
- `FakeEventBus.cs` - In-memory event bus implementation for tests
- `QuestBuilder.cs` - Fluent builder for programmatic quest creation
- `ObjectiveBuilder.cs` - Fluent builder for programmatic objective creation
- `MockCondition.cs` - Controllable mock condition for testing
- `QuestSystemTests.cs` - Comprehensive unit tests covering:
  - Item collected condition firing
  - Fail conditions triggering quest failure
  - AND/OR condition group logic
  - Prerequisite objective validation
  - Optional objective handling
- `TestRunner.cs` - Entry point for running tests

## Architecture Overview

```
Game Events (ItemCollected, etc.)
    ↓
IQuestEventBus (event subscription)
    ↓
ConditionAsset → CreateInstance() → IConditionInstance
    ├─ ItemCollectedConditionInstance
    ├─ TimeElapsedConditionInstance
    ├─ ConditionGroupInstance (AND/OR)
    └─ Custom conditions
    ↓
QuestManager
    ├─ Binds conditions to events
    ├─ Polls conditions (optional)
    ├─ Evaluates quest progress
    └─ Fires OnQuestCompleted/OnQuestFailed events
```

## Key Design Decisions

1. **Separation of Concerns**: Designer-authored assets (ConditionAsset) are completely separate from runtime instances (IConditionInstance). No state is stored in assets.

2. **Event-Driven with Optional Polling**: The system is primarily event-driven, but supports polling for continuous conditions (time-based, sensor-based, etc.).

3. **Dirty Queue Pattern**: QuestManager uses a dirty queue to batch-process condition changes, avoiding deep call stacks and enabling deterministic evaluation order.

4. **No State Persistence**: For v0.1, quests are memory-only. Save/load is out of scope.

5. **Extensibility**: New condition types can be added by creating ConditionAsset subclasses and corresponding IConditionInstance implementations.

## Next Steps

### Immediate TODOs

1. **EventManagementQuestBus Integration**
   - Get actual mechaniqe/event-management API
   - Implement Subscribe/Unsubscribe/Publish mappings

2. **Editor Support**
   - Create `QuestAssetEditor.cs` with reorderable objective list
   - Create `ConditionGroupEditor.cs` for operator/children display
   - (Optional) Create `QuestDebuggerWindow.cs` for runtime debugging

3. **Example Conditions**
   - `CustomFlagConditionAsset.cs` - Flag-based conditions
   - Example: `AreaEnteredConditionAsset.cs`

4. **Integration Testing**
   - Test with actual UnityEngine components
   - Test MonoBehaviour lifecycle (Awake, Update)

5. **Documentation**
   - README with setup instructions
   - Example quest creation walkthrough
   - Custom condition authoring guide

## File Structure

```
Assets/
  GenericQuestCore/
    Runtime/
      Core/
        [Data Model]
        QuestAsset.cs
        ObjectiveAsset.cs
        ConditionAsset.cs
        ConditionGroupAsset.cs
        
        [Runtime State]
        QuestState.cs
        ObjectiveState.cs
        QuestLog.cs
        QuestStatus.cs
        
        [Condition System]
        IConditionInstance.cs
        ConditionGroupInstance.cs
        ItemCollectedConditionAsset.cs
        ItemCollectedConditionInstance.cs
        TimeElapsedConditionAsset.cs
        TimeElapsedConditionInstance.cs
        
        [Infrastructure]
        QuestContext.cs
        IQuestEventBus.cs
        QuestManager.cs
        QuestPlayerRef.cs
        
      EventManagementAdapter/
        EventManagementQuestBus.cs
        
    Editor/
      Inspectors/
        QuestAssetEditor.cs           [TODO]
        ObjectiveListDrawer.cs        [TODO]
        ConditionGroupEditor.cs       [TODO]
      Windows/
        QuestDebuggerWindow.cs        [TODO - optional]

Tests/
  FakeEventBus.cs
  QuestBuilder.cs
  ObjectiveBuilder.cs
  MockCondition.cs
  QuestSystemTests.cs
  TestRunner.cs
```

## Testing

To run tests from C# (integrate with your test framework):

```csharp
GenericQuest.Tests.QuestSystemTests.RunAllTests();
```

Current test coverage:
- ✓ Item collected condition
- ✓ Fail conditions
- ✓ AND/OR condition groups
- ✓ Prerequisite objectives
- ✓ Optional objectives

## Notes for Developers

1. **Unity ScriptableObjects**: The reflection-based builders are necessary because ScriptableObject fields are private. In production, these would typically be created through the inspector.

2. **Event Bus Implementation**: Currently using a simplified in-memory bus for testing. The `EventManagementQuestBus` adapter needs to map to the real mechaniqe event system.

3. **Polling Interval**: QuestManager's polling interval (default 0.25s) is configurable. Set `enablePolling = false` if all conditions are purely event-driven.

4. **Service Injection**: The `QuestPlayerRef` pattern can be replaced with any DI framework; it's just one approach to injecting services into the QuestContext.

## References

- Specs: `specs.md`
- Event Management Library: https://github.com/mechaniqe/event-management
