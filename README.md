# Generic Quest Core

A Unity-first, designer-friendly, event-driven quest system for game development.

## Features

- **Data-Driven Design**: Quests and objectives defined as ScriptableObjects
- **Event-Driven Architecture**: Conditions evaluate based on game events with optional polling
- **Composite Objectives**: Support for prerequisites and optional objectives
- **Logical Composition**: AND/OR condition groups for complex logic
- **Extensible**: Easy to add custom condition types
- **Testable**: Pure C# core with no engine dependencies in test layer

## Quick Start

### 1. Setting Up a Quest

#### In Code (Programmatic)

```csharp
using GenericQuest.Core;

// Create a simple condition
var itemCondition = new ItemCollectedConditionAsset();
// (Set itemId and requiredCount in inspector or via reflection)

// Create an objective
var objective = new ObjectiveBuilder()
    .WithObjectiveId("collect_sword")
    .WithTitle("Collect the Sword")
    .WithCompletionCondition(itemCondition)
    .Build();

// Create a quest
var quest = new QuestBuilder()
    .WithQuestId("quest_001")
    .WithDisplayName("Find the Weapon")
    .AddObjective(objective)
    .Build();

// Start the quest
questManager.StartQuest(quest);
```

#### In Editor (Recommended)

1. Right-click in Assets → Create → Quests → Quest
2. Set Quest ID, Display Name, Description
3. Add objectives ("+New Objective" button)
4. For each objective, add conditions:
   - Right-click → Create → Quests → Conditions → Item Collected
   - Set Item ID and Required Count
5. Assign objectives to quest

### 2. Wiring the Quest Manager

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestAsset tutorialQuest;

    private void Start()
    {
        questManager.OnQuestCompleted += HandleQuestCompleted;
        questManager.OnQuestFailed += HandleQuestFailed;
        questManager.OnObjectiveStatusChanged += HandleObjectiveChanged;
        
        questManager.StartQuest(tutorialQuest);
    }

    private void HandleQuestCompleted(QuestState quest)
    {
        Debug.Log($"Quest completed: {quest.Definition.DisplayName}");
    }

    private void HandleQuestFailed(QuestState quest)
    {
        Debug.Log($"Quest failed: {quest.Definition.DisplayName}");
    }

    private void HandleObjectiveChanged(ObjectiveState obj)
    {
        Debug.Log($"Objective {obj.Definition.ObjectiveId}: {obj.Status}");
    }
}
```

### 3. Publishing Game Events

From your inventory system:

```csharp
public class Inventory
{
    private IQuestEventBus _questEventBus;

    public void AddItem(string itemId, int amount)
    {
        // ... inventory logic ...
        
        // Publish event for quest system
        _questEventBus.Publish(new ItemCollectedEvent(itemId, amount));
    }
}
```

## Building Custom Conditions

### 1. Create the Asset Class

```csharp
using GenericQuest.Core;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Conditions/Area Entered", fileName = "NewAreaEnteredCondition")]
public class AreaEnteredConditionAsset : ConditionAsset
{
    [SerializeField] private string areaId;

    public override IConditionInstance CreateInstance()
    {
        return new AreaEnteredConditionInstance(areaId);
    }
}
```

### 2. Create the Event Class

```csharp
public sealed class AreaEnteredEvent
{
    public string AreaId { get; }

    public AreaEnteredEvent(string areaId)
    {
        AreaId = areaId;
    }
}
```

### 3. Create the Instance Class

```csharp
public sealed class AreaEnteredConditionInstance : IConditionInstance
{
    private readonly string _areaId;
    private bool _isMet;
    private Action? _onChanged;

    public bool IsMet => _isMet;

    public AreaEnteredConditionInstance(string areaId)
    {
        _areaId = areaId;
    }

    public void Bind(IQuestEventBus eventBus, QuestContext context, Action onChanged)
    {
        _onChanged = onChanged;
        eventBus.Subscribe<AreaEnteredEvent>(OnAreaEntered);
    }

    public void Unbind(IQuestEventBus eventBus, QuestContext context)
    {
        eventBus.Unsubscribe<AreaEnteredEvent>(OnAreaEntered);
    }

    private void OnAreaEntered(AreaEnteredEvent evt)
    {
        if (evt.AreaId == _areaId && !_isMet)
        {
            _isMet = true;
            _onChanged?.Invoke();
        }
    }
}
```

## Testing

The system includes a comprehensive test suite:

```csharp
using GenericQuest.Tests;

// Run all tests
QuestSystemTests.RunAllTests();
```

Tests cover:
- Event-driven condition evaluation
- Fail conditions
- Condition groups (AND/OR)
- Prerequisite objectives
- Optional objectives

## API Reference

### Core Classes

**QuestAsset**
- `QuestId` (string)
- `DisplayName` (string)
- `Description` (string)
- `Objectives` (IReadOnlyList<ObjectiveAsset>)

**ObjectiveAsset**
- `ObjectiveId` (string)
- `Title` (string)
- `Description` (string)
- `IsOptional` (bool)
- `Prerequisites` (IReadOnlyList<ObjectiveAsset>)
- `CompletionCondition` (ConditionAsset)
- `FailCondition` (ConditionAsset)

**IConditionInstance**
- `IsMet` (bool) - Read-only property
- `Bind()` - Subscribe to events
- `Unbind()` - Unsubscribe from events

**IPollingConditionInstance** (optional)
- `Refresh()` - Called periodically for time-based conditions

**QuestManager**
- `StartQuest(QuestAsset)` → QuestState
- `StopQuest(QuestState)`
- `OnQuestCompleted` - Event
- `OnQuestFailed` - Event
- `OnObjectiveStatusChanged` - Event

### Enums

**QuestStatus**: NotStarted, InProgress, Completed, Failed

**ObjectiveStatus**: NotStarted, InProgress, Completed, Failed

**ConditionOperator**: And, Or

## Architecture

```
┌─────────────────────────────────────┐
│     Game Event (ItemCollected)      │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│       IQuestEventBus.Publish()      │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│  ConditionInstance.OnItemCollected()│
│  → IsMet = true                     │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│  QuestManager.MarkDirty()           │
│  → Add to dirty queue               │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│  QuestManager.ProcessDirtyQueue()   │
│  → Evaluate objective               │
│  → Update quest status              │
└──────────────┬──────────────────────┘
               │
               ▼
┌─────────────────────────────────────┐
│   OnQuestCompleted / OnQuestFailed  │
│   (or OnObjectiveStatusChanged)     │
└─────────────────────────────────────┘
```

## Project Structure

```
Assets/
  GenericQuestCore/
    Runtime/
      Core/
        (Asset definitions, runtime state, core logic)
      EventManagementAdapter/
        (Integration with mechaniqe/event-management)
    Editor/
      Inspectors/
        (Custom inspectors for ScriptableObjects)
      Windows/
        (Optional: Quest debugger window)

Tests/
  (Unit tests and test infrastructure)
```

## Known Limitations (v0.1)

- No save/load persistence (planned for separate system)
- No multi-actor/party support (assumes single player context)
- No cycle detection in objective prerequisites (document as caveat)
- EventManagementQuestBus is a stub (needs real integration)
- Editor inspectors are minimal (can be enhanced later)

## Contributing

To add a new built-in condition:

1. Create `YourConditionAsset.cs` extending `ConditionAsset`
2. Create `YourConditionInstance.cs` implementing `IConditionInstance`
3. Add corresponding event class (e.g., `YourEvent.cs`)
4. Add tests to `QuestSystemTests.cs`
5. Document in this README

## License

[Your License Here]
