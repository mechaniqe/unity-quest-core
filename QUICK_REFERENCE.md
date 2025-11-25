# Quick Commands Reference

## Navigation

```bash
# Go to project root
cd /Users/shahinaliyev/Projects/unity-quest-core

# View all files
ls -la

# View structure
find . -type f -name "*.cs" -o -name "*.md" | sort

# Count lines of code
find . -name "*.cs" | xargs wc -l
```

## Testing

```csharp
// Run all tests
GenericQuest.Tests.QuestSystemTests.RunAllTests();

// Individual test
GenericQuest.Tests.QuestSystemTests.TestItemCollectedConditionCompletion();
```

## Creating Assets (In Inspector)

```
Right-click → Create → Quests → Quest
Right-click → Create → Quests → Objective
Right-click → Create → Quests → Condition Group
Right-click → Create → Quests → Conditions → Item Collected
Right-click → Create → Quests → Conditions → Time Elapsed
```

## Common Code Patterns

### Create Quest Programmatically
```csharp
var objective = new ObjectiveBuilder()
    .WithObjectiveId("obj1")
    .WithTitle("Collect Items")
    .Build();

var quest = new QuestBuilder()
    .WithQuestId("quest1")
    .AddObjective(objective)
    .Build();

questManager.StartQuest(quest);
```

### Publish Event
```csharp
eventBus.Publish(new ItemCollectedEvent("sword", 1));
```

### Listen to Quest Events
```csharp
questManager.OnQuestCompleted += (quest) => { /* handle */ };
questManager.OnQuestFailed += (quest) => { /* handle */ };
questManager.OnObjectiveStatusChanged += (obj) => { /* handle */ };
```

## Documentation Quick Links

- **Getting Started**: `README.md`
- **API Docs**: `API_REFERENCE.md`
- **Architecture**: `IMPLEMENTATION.md`
- **Status**: `PROGRESS.md`
- **File Guide**: `INDEX.md`
- **Summary**: `COMPLETE.md`

## Directory Structure

```
Assets/GenericQuestCore/
├── Runtime/
│   ├── Core/          (18 C# files - main system)
│   └── EventManagementAdapter/  (1 C# file - event adapter)
├── Editor/            (TODO - inspectors and windows)
└── package.json

Tests/                 (6 C# files - unit tests)

Documentation/        (7 MD files - guides and refs)
```

## File Locations

### Core Runtime
```
Assets/GenericQuestCore/Runtime/Core/
  - QuestAsset.cs
  - ObjectiveAsset.cs
  - ConditionAsset.cs
  - QuestManager.cs
  - ... (15 more files)
```

### Tests
```
Tests/
  - QuestSystemTests.cs
  - FakeEventBus.cs
  - QuestBuilder.cs
  - ... (3 more files)
```

### Documentation
```
Root directory:
  - README.md
  - API_REFERENCE.md
  - IMPLEMENTATION.md
  - PROGRESS.md
  - COMPLETE.md
  - INDEX.md
  - specs.md
```

## Development Checklist

For implementing Phase 2 (Editor Support):

- [ ] Create `Assets/GenericQuestCore/Editor/Inspectors/QuestAssetEditor.cs`
- [ ] Create `Assets/GenericQuestCore/Editor/Inspectors/ObjectiveListDrawer.cs`
- [ ] Create `Assets/GenericQuestCore/Editor/Inspectors/ConditionGroupEditor.cs`
- [ ] Test inspector functionality
- [ ] Update documentation

For implementing Phase 1 (Integration):

- [ ] Get mechaniqe/event-management library reference
- [ ] Study actual EventManager API
- [ ] Update `EventManagementQuestBus.cs`
- [ ] Test with real game events
- [ ] Remove NotImplementedException

## Key Classes to Know

| Class | Purpose | Location |
|-------|---------|----------|
| QuestAsset | Quest definition | Runtime/Core/QuestAsset.cs |
| ObjectiveAsset | Objective definition | Runtime/Core/ObjectiveAsset.cs |
| ConditionAsset | Condition base class | Runtime/Core/ConditionAsset.cs |
| QuestManager | Quest orchestrator | Runtime/Core/QuestManager.cs |
| IConditionInstance | Condition interface | Runtime/Core/IConditionInstance.cs |
| IQuestEventBus | Event interface | Runtime/Core/IQuestEventBus.cs |

## Testing Commands

```bash
# See test file
cat Tests/QuestSystemTests.cs

# Run from C#
GenericQuest.Tests.TestRunner.Main();

# Review test utilities
cat Tests/FakeEventBus.cs
cat Tests/QuestBuilder.cs
cat Tests/MockCondition.cs
```

## Important Notes

- **Namespaces**: All code uses `GenericQuest.Core` or `GenericQuest.Tests`
- **Dependencies**: Only UnityEngine for MonoBehaviours and ScriptableObjects
- **Event Bus**: Currently a stub, needs mechaniqe integration
- **Polling**: Optional, controlled via `QuestManager.enablePolling`

## Common Tasks

### Add a New Condition

1. Create `YourConditionAsset` extending `ConditionAsset`
2. Create `YourConditionInstance` implementing `IConditionInstance`
3. Create event class `YourEvent`
4. Add test to `QuestSystemTests.cs`

### Debug a Quest

```csharp
Debug.Log($"Quest: {state.Definition.DisplayName}");
Debug.Log($"Status: {state.Status}");
foreach (var obj in state.Objectives.Values) {
    Debug.Log($"  {obj.Definition.ObjectiveId}: {obj.Status}");
}
```

### Create Test Quest

```csharp
var eventBus = new FakeEventBus();
var context = new QuestContext(null, null, null);
var quest = new QuestBuilder().Build();
var state = new QuestState(quest);
```

---

**Last Updated**: 2025-11-25  
**Version**: 0.1.0
