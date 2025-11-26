# Quest System API Reference

## Quick Reference

### Starting a Quest

```csharp
QuestState questState = questManager.StartQuest(questAsset);
```

### Listening to Quest Events

```csharp
questManager.OnQuestCompleted += (quest) => { /* handle */ };
questManager.OnQuestFailed += (quest) => { /* handle */ };
questManager.OnObjectiveStatusChanged += (objective) => { /* handle */ };
```

### Stopping a Quest

```csharp
questManager.StopQuest(questState);
```

## Core Classes

### QuestAsset

**Location**: `GenericQuest.Core`

Designer-authored quest definition (ScriptableObject).

#### Properties

```csharp
public string QuestId { get; }
public string DisplayName { get; }
public string Description { get; }
public IReadOnlyList<ObjectiveAsset> Objectives { get; }
```

#### Usage

```csharp
[SerializeField] private QuestAsset tutorialQuest;

void Start() {
    var state = questManager.StartQuest(tutorialQuest);
}
```

---

### ObjectiveAsset

**Location**: `GenericQuest.Core`

Designer-authored objective definition (ScriptableObject).

#### Properties

```csharp
public string ObjectiveId { get; }
public string Title { get; }
public string Description { get; }
public bool IsOptional { get; }
public IReadOnlyList<ObjectiveAsset> Prerequisites { get; }
public ConditionAsset CompletionCondition { get; }
public ConditionAsset FailCondition { get; }
```

#### Notes

- `Prerequisites`: List of objectives that must complete first
- `IsOptional`: Quest completes without this objective if true
- Conditions can be individual or `ConditionGroupAsset` (AND/OR)

---

### ConditionAsset

**Location**: `GenericQuest.Core`

Base class for all condition types.

#### Abstract Methods

```csharp
public abstract IConditionInstance CreateInstance();
```

#### Subclasses (Built-in)

- `ConditionGroupAsset` – Composite (AND/OR)
- `ItemCollectedConditionAsset` – Item events
- `TimeElapsedConditionAsset` – Polling-based time

---

### ConditionGroupAsset

**Location**: `GenericQuest.Core`

Composite condition with AND/OR logic.

#### Properties

```csharp
public ConditionOperator Operator { get; }  // And or Or
public List<ConditionAsset> Children { get; }
```

#### ConditionOperator

```csharp
public enum ConditionOperator
{
    And,  // All children must be true
    Or    // At least one child must be true
}
```

---

### QuestState

**Location**: `GenericQuest.Core`

Runtime state of an active quest.

#### Properties

```csharp
public QuestAsset Definition { get; }
public QuestStatus Status { get; }
public IReadOnlyDictionary<string, ObjectiveState> Objectives { get; }
```

#### Status Values

```csharp
public enum QuestStatus
{
    NotStarted,
    InProgress,
    Completed,
    Failed
}
```

---

### ObjectiveState

**Location**: `GenericQuest.Core`

Runtime state of an active objective.

#### Properties

```csharp
public ObjectiveAsset Definition { get; }
public ObjectiveStatus Status { get; }
```

#### Status Values

```csharp
public enum ObjectiveStatus
{
    NotStarted,
    InProgress,
    Completed,
    Failed
}
```

---

### QuestManager

**Location**: `GenericQuest.Core`

MonoBehaviour that orchestrates quest evaluation.

#### Inspector Fields

```
Wiring
└─ Player Ref             (QuestPlayerRef)

Polling (optional)
├─ Enable Polling         (bool, default: true)
└─ Polling Interval       (float, default: 0.25s)
```

**Note**: QuestManager automatically uses `EventManager.Instance` - no manual EventManager reference needed.

#### Public Methods

```csharp
public QuestState StartQuest(QuestAsset questAsset)
public void StopQuest(QuestState questState)
```

#### Events

```csharp
public event Action<QuestState> OnQuestCompleted;
public event Action<QuestState> OnQuestFailed;
public event Action<ObjectiveState> OnObjectiveStatusChanged;
```

#### Usage

```csharp
public class GameController : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestAsset mainQuest;

    private void Start()
    {
        // QuestManager automatically uses EventManager.Instance
        questManager.OnQuestCompleted += OnQuestComplete;
        questManager.StartQuest(mainQuest);
    }

    private void OnQuestComplete(QuestState quest)
    {
        Debug.Log($"Completed: {quest.Definition.DisplayName}");
    }
    
    // Publish events through EventManager singleton
    private void CollectItem(string itemId)
    {
        EventManager.Instance.Raise(new ItemCollectedEvent(itemId, 1));
    }
}
```

---

### QuestContext

**Location**: `GenericQuest.Core`

Container for game services accessed by conditions.

#### Constructor

```csharp
public QuestContext(
    IQuestAreaService? areaService,
    IQuestInventoryService? inventoryService,
    IQuestTimeService? timeService)
```

#### Properties

```csharp
public IQuestAreaService? AreaService { get; }
public IQuestInventoryService? InventoryService { get; }
public IQuestTimeService? TimeService { get; }
```

#### Service Interfaces

```csharp
public interface IQuestAreaService { }
public interface IQuestInventoryService { }
public interface IQuestTimeService { }
```

#### Usage

```csharp
public class InventoryService : MonoBehaviour, IQuestInventoryService
{
    // Implement as needed
}

public class MyQuestPlayerRef : QuestPlayerRef
{
    public override QuestContext BuildContext()
    {
        return new QuestContext(
            GetComponent<IQuestAreaService>(),
            GetComponent<IQuestInventoryService>(),
            GetComponent<IQuestTimeService>()
        );
    }
}
```

---

### QuestPlayerRef

**Location**: `GenericQuest.Core`

MonoBehaviour helper to build QuestContext.

#### Inspector Fields

```
Area Service Provider      (MonoBehaviour)
Inventory Service Provider (MonoBehaviour)
Time Service Provider      (MonoBehaviour)
```

#### Public Methods

```csharp
public QuestContext BuildContext()
```

---

### IQuestEventBus

**Location**: `GenericQuest.Core`

Interface for publishing/subscribing to game events.

#### Methods

```csharp
public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
public void Publish<TEvent>(TEvent evt) where TEvent : class;
```

#### Usage

```csharp
// In your game code
public class Inventory
{
    private IQuestEventBus _eventBus;

    public void AddItem(string itemId)
    {
        // ... inventory logic ...
        _eventBus.Publish(new ItemCollectedEvent(itemId, 1));
    }
}
```

---

### IConditionInstance

**Location**: `GenericQuest.Core`

Interface all condition instances must implement.

#### Properties

```csharp
public bool IsMet { get; }
```

#### Methods

```csharp
public void Bind(IQuestEventBus eventBus, QuestContext context, Action onChanged);
public void Unbind(IQuestEventBus eventBus, QuestContext context);
```

#### Lifecycle

1. **Bind()** – Called when objective becomes active
   - Subscribe to events
   - Initialize state
   
2. **IsMet** – Checked periodically (on events or polling)
   - Return true when condition is satisfied
   
3. **Unbind()** – Called when objective completes/fails
   - Unsubscribe from events
   - Clean up resources

---

### IPollingConditionInstance

**Location**: `GenericQuest.Core`

Optional interface for conditions that need periodic updates.

#### Methods

```csharp
public void Refresh(QuestContext context, Action onChanged);
```

#### Usage

For time-based conditions, sensor polling, etc.

#### Example

```csharp
public class TimeElapsedConditionInstance : IConditionInstance, IPollingConditionInstance
{
    private float _elapsed;
    
    public void Refresh(QuestContext context, Action onChanged)
    {
        _elapsed += Time.deltaTime;
        if (_elapsed >= _required)
            onChanged();
    }
}
```

---

## Built-in Conditions

### ItemCollectedConditionAsset

**Location**: `GenericQuest.Core.Conditions`

Listens for `ItemCollectedEvent`.

#### Asset Fields

```
Item Id          (string)
Required Count   (int)
```

#### Event

```csharp
public class ItemCollectedEvent
{
    public string ItemId { get; }
    public int Amount { get; }
}
```

#### Usage

```csharp
// In inspector: Create Quests/Conditions/Item Collected
// Set Item Id to "key" and Required Count to 1

// In code: Publish event
eventBus.Publish(new ItemCollectedEvent("key", 1));
```

---

### TimeElapsedConditionAsset

**Location**: `GenericQuest.Core.Conditions`

Polling-based condition that tracks elapsed time.

#### Asset Fields

```
Required Seconds   (float)
```

#### Usage

```csharp
// In inspector: Create Quests/Conditions/Time Elapsed
// Set Required Seconds to 30

// Automatically polls every [PollingInterval] seconds
// Completes after 30 seconds elapsed
```

---

## Testing Utilities

### FakeEventBus

**Location**: `GenericQuest.Tests`

In-memory event bus for testing.

```csharp
var eventBus = new FakeEventBus();
eventBus.Subscribe<ItemCollectedEvent>(handler);
eventBus.Publish(new ItemCollectedEvent("sword", 1));
```

---

### QuestBuilder

**Location**: `GenericQuest.Tests`

Fluent builder for programmatic quest creation.

```csharp
var quest = new QuestBuilder()
    .WithQuestId("test_quest")
    .WithDisplayName("Test Quest")
    .AddObjective(objective)
    .Build();
```

---

### ObjectiveBuilder

**Location**: `GenericQuest.Tests`

Fluent builder for programmatic objective creation.

```csharp
var objective = new ObjectiveBuilder()
    .WithObjectiveId("obj1")
    .WithTitle("Collect Items")
    .WithCompletionCondition(condition)
    .AsOptional(false)
    .Build();
```

---

### MockCondition

**Location**: `GenericQuest.Tests`

Controllable condition for testing.

```csharp
var condition = new MockConditionAsset().CreateInstance() as MockConditionInstance;
condition.SetMet(true);  // Manually trigger
```

---

## Common Patterns

### Creating a Simple Quest

```csharp
// 1. Create in inspector:
//    - Assets/Create/Quests/Quest
//    - Add objective
//    - Add completion condition
//
// 2. Reference in code:
[SerializeField] private QuestAsset myQuest;

private void Start() {
    questManager.StartQuest(myQuest);
}
```

### Composite Conditions

```csharp
// In inspector:
// 1. Create Quests/Condition Group
// 2. Set Operator to "Or"
// 3. Add two Item Collected conditions
// Quest completes when either condition is met
```

### Dependent Objectives

```csharp
// In inspector:
// Objective 2:
//   - Add Objective 1 to Prerequisites
//   - Objective 1 must complete before Objective 2 starts
```

### Optional Content

```csharp
// In inspector:
// Side Quest Objective:
//   - Check "Is Optional"
//   - Quest completes without completing this
```

---

## Error Handling

### EventManagementQuestBus Not Implemented

If you see `NotImplementedException` from `EventManagementQuestBus`:

1. Check `Assets/GenericQuestCore/Runtime/EventManagementAdapter/EventManagementQuestBus.cs`
2. Implement the adapter for your EventManager
3. Map Subscribe/Unsubscribe/Publish methods

### Objectives Not Progressing

1. Verify conditions are bound correctly
2. Check that events are being published
3. Enable logging in QuestManager to debug

### Quest Stuck in InProgress

1. Check prerequisite conditions are met
2. Verify polling is enabled (if condition needs it)
3. Check that completion condition's IsMet is true

---

## Performance Considerations

### Event Subscription

- Conditions subscribe once (on Bind)
- Unsubscribe when objective completes (on Unbind)
- No per-frame event registration

### Polling

- Default interval: 0.25 seconds
- Adjust `QuestManager.pollingInterval` for performance
- Set `enablePolling = false` if all conditions are event-driven

### Memory

- Active quests stored in `QuestLog`
- Remove completed quests with `StopQuest()`
- Event handlers cleaned up on Unbind

---

## Debugging

### Enable Logging

```csharp
// In your QuestManager subclass
private void EvaluateObjectiveAndQuest(QuestState quest, ObjectiveState obj)
{
    Debug.Log($"Evaluating {obj.Definition.ObjectiveId}: met={obj.CompletionInstance?.IsMet}");
    // ... rest of logic
}
```

### Check Quest Status

```csharp
Debug.Log($"Quest: {questState.Definition.DisplayName}");
Debug.Log($"Status: {questState.Status}");
foreach (var obj in questState.Objectives.Values)
{
    Debug.Log($"  - {obj.Definition.ObjectiveId}: {obj.Status}");
}
```

### Event Flow

```csharp
// Trace event publishing
public class DebugEventBus : IQuestEventBus
{
    private readonly IQuestEventBus _inner;

    public void Publish<TEvent>(TEvent evt) where TEvent : class
    {
        Debug.Log($"Event: {typeof(TEvent).Name}");
        _inner.Publish(evt);
    }
    // ... other methods delegate to _inner
}
```

---

## Migration & Troubleshooting

### Moving from Other Quest Systems

1. Map your quest data to QuestAsset/ObjectiveAsset
2. Implement custom ConditionAsset for your event types
3. Replace event bus with EventManagementQuestBus
4. Test with QuestSystemTests as reference

### Performance Profiling

```csharp
var watch = System.Diagnostics.Stopwatch.StartNew();
questManager.StartQuest(largeQuest);
watch.Stop();
Debug.Log($"Quest setup: {watch.ElapsedMilliseconds}ms");
```

---

Last Updated: 2025-11-25
