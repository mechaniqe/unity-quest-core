# DynamicBox Quest Core

A Unity-first, designer-friendly, event-driven quest system for game development.

## Overview

DynamicBox Quest Core provides a complete, extensible framework for creating quest systems in Unity. It's designed to be:

- **Designer-Friendly** – Create quests entirely in the inspector with no coding required
- **Event-Driven** – Conditions evaluate based on game events for efficiency
- **Extensible** – Easy to add custom condition types for your game's needs
- **Testable** – Pure C# core with comprehensive unit tests
- **Production-Ready** – Clean architecture suitable for commercial projects

## Features

✨ **Designer-Authored Quests**
- Create quests and objectives as ScriptableObjects
- Define conditions visually without code
- Support for prerequisites and optional objectives
- **Visual Graph Editor** – Node-based interface for creating and visualizing quest structures

⚡ **Event-Driven Architecture**
- Conditions respond to game events in real-time
- Optional polling for continuous conditions (time-based, sensor-based, etc.)
- Efficient batch evaluation with dirty queue pattern

🎯 **Logical Composition**
- Combine conditions with AND/OR operators
- Build complex quest logic from simple pieces
- Reuse conditions across multiple quests

🔧 **Extensible Design**
- Add custom conditions by implementing `ConditionAsset` and `IConditionInstance`
- Integrate with your game's event system
- Inject services via `QuestContext`

## Installation

### Via Git URL (UPM)

In Unity Package Manager, click the `+` button and select "Add package from git URL":

```
https://github.com/mechaniqe/unity-quest-core.git
```

**Dependencies**: You must manually install the DynamicBox EventManagement package:

```
https://github.com/mechaniqe/event-management.git
```

### Manual Installation

1. Clone the repository
2. Copy the `Packages/net.dynamicbox.quest.core` folder to your project's `Packages/` directory
3. Install the DynamicBox EventManagement dependency manually:
   - Via UPM: `https://github.com/mechaniqe/event-management.git`
   - Or copy it to your `Packages/` directory

### Setup

The Quest System automatically uses `EventManager.Instance` - no manual setup required!

1. Add a QuestManager component to a GameObject in your scene
2. Assign a QuestPlayerRef to provide quest context
3. Start creating quests!

## Quick Start

### Step 1: Scene Setup

```csharp
// 1. Create a QuestManager in your scene
GameObject questManagerObj = new GameObject("QuestManager");
QuestManager questManager = questManagerObj.AddComponent<QuestManager>();

// 2. Create a QuestPlayerRef for context
GameObject playerRefObj = new GameObject("QuestPlayerRef");
QuestPlayerRef playerRef = playerRefObj.AddComponent<QuestPlayerRef>();

// 3. Assign the player reference to QuestManager
questManager.GetComponent<QuestManager>().playerRef = playerRef;
```

### Step 2: Create Quest Assets

**Method 1: Visual Graph Editor (Recommended)**

1. Open the Quest Graph Editor: `Tools → DynamicBox → Quest System → Quest Graph Editor`
2. Click "New Quest" to create a new quest
3. Add objectives and conditions using the node-based interface
4. Visually connect prerequisites and conditions
5. Save your quest

**Method 2: Inspector (Traditional)**

```
Right-click in Project → Create → Quests → Quest
├─ Quest ID: "collect_sword"
├─ Display Name: "Find the Legendary Sword"
├─ Description: "Search the ancient ruins for the legendary sword"
└─ Add Objective:
    ├─ Objective ID: "sword_objective"
    ├─ Display Name: "Collect Ancient Sword"
    ├─ Completion Condition:
    │   └─ Create → Quests → Conditions → Item Collected
    │       ├─ Item ID: "ancient_sword"
    │       └─ Required Count: 1
```

**Tip**: Double-click any QuestAsset to open it in the Graph Editor for visual editing!

### Step 3: Integrate with Game Code

```csharp
using DynamicBox.Quest.Core;
using DynamicBox.Quest.GameEvents;
using DynamicBox.EventManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header ("References")]
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestAsset tutorialQuest;
    
    void Start()
    {
        // Subscribe to quest events
        questManager.OnQuestCompleted += HandleQuestCompleted;
        questManager.OnQuestFailed += HandleQuestFailed;
        questManager.OnObjectiveStatusChanged += HandleObjectiveChanged;
        questManager.OnConditionStatusChanged += HandleConditionChanged;
        
        // Start the tutorial quest
        questManager.StartQuest(tutorialQuest);
    }
    
    void HandleQuestCompleted(QuestState questState)
    {
        Debug.Log($"Quest completed: {questState.Definition.DisplayName}");
        // Grant rewards, unlock new content, etc.
    }
    
    void HandleQuestFailed(QuestState questState)
    {
        Debug.Log($"Quest failed: {questState.Definition.DisplayName}");
        // Handle failure consequences
    }
    
    void HandleObjectiveChanged(ObjectiveState objectiveState)
    {
        Debug.Log($"Objective updated: {objectiveState.Definition.DisplayName} - {objectiveState.Status}");
        // Update UI, show notifications
    }
    
    void HandleConditionChanged(ObjectiveState objective, IConditionInstance condition, bool isMet)
    {
        Debug.Log($"Condition changed for {objective.Definition.DisplayName}: {condition.GetType().Name} = {isMet}");
        // Update detailed progress UI (e.g., "3/5 enemies killed")
    }
}
```

### Step 4: Publish Game Events

```csharp
// In your inventory system
public class InventorySystem : MonoBehaviour
{
    public void CollectItem(string itemId, int amount)
    {
        // Add to inventory logic here...
        
        // Notify quest system
        EventManager.Instance.Raise(new ItemCollectedEvent(itemId, amount));
    }
}

// In your area/trigger system
public class AreaTrigger : MonoBehaviour
{
    [SerializeField] private string areaId;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.Instance.Raise(new AreaEnteredEvent(areaId));
        }
    }
}

// In your game state system
public class GameStateManager : MonoBehaviour
{
    public void SetFlag(string flagId, bool value)
    {
        // Update game state...
        
        // Notify quest system
        EventManager.Instance.Raise(new FlagChangedEvent(flagId, value));
    }
}
```

## Usage Examples

### Creating Complex Quests

#### Multi-Objective Quest with Prerequisites
```
Create → Quests → Quest: "Prepare for Battle"
├─ Objective 1: "Collect 10 Health Potions"
│   └─ Completion: Item Collected (health_potion, 10)
├─ Objective 2: "Find a Weapon" 
│   └─ Completion: Item Collected (weapon, 1)
└─ Objective 3: "Enter the Dungeon"
    ├─ Prerequisites: [Objective 1, Objective 2]
    └─ Completion: Area Entered (dungeon_entrance)
```

#### Quest with Failure Conditions
```
Create → Quests → Quest: "Stealth Mission"
├─ Objective: "Reach the Target"
│   ├─ Completion: Area Entered (target_zone)
│   └─ Fail Condition: Custom Flag (alarm_triggered, true)
```

#### Quest with Optional Objectives
```
Create → Quests → Quest: "Explore the Forest"
├─ Objective 1: "Find the Main Path" (Required)
│   └─ Completion: Area Entered (main_path)
├─ Objective 2: "Discover Secret Cave" (Optional)
│   └─ Completion: Area Entered (secret_cave)
└─ Objective 3: "Collect Rare Herbs" (Optional)
    └─ Completion: Item Collected (rare_herb, 3)
```

### Runtime Quest Management

```csharp
public class QuestController : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    
    // Start a quest programmatically
    public void StartQuest(QuestAsset quest)
    {
        var questState = questManager.StartQuest(quest);
        if (questState != null)
        {
            Debug.Log($"Started quest: {quest.DisplayName}");
        }
    }
    
    // Check quest status
    public bool IsQuestActive(string questId)
    {
        return questManager.ActiveQuests.Any(q => q.Definition.QuestId == questId);
    }
    
    // Complete quest manually (for testing/cheat codes)
    public void CompleteQuest(string questId)
    {
        var quest = questManager.ActiveQuests.FirstOrDefault(q => q.Definition.QuestId == questId);
        if (quest != null)
        {
            questManager.CompleteQuest(quest);
        }
    }
    
    // Get quest progress
    public float GetQuestProgress(string questId)
    {
        var quest = questManager.ActiveQuests.FirstOrDefault(q => q.Definition.QuestId == questId);
        if (quest == null) return 0f;
        
        var completedObjectives = quest.GetObjectiveStates().Count(obj => obj.Status == ObjectiveStatus.Completed);
        var totalObjectives = quest.GetObjectiveStates().Count(obj => !obj.Definition.IsOptional);
        
        return totalObjectives > 0 ? (float)completedObjectives / totalObjectives : 0f;
    }
}
```

## Building Custom Conditions

### Creating Event-Driven Conditions

```csharp
// 1. Create the asset class
[CreateAssetMenu(menuName = "Quests/Conditions/Enemy Killed")]
public class EnemyKilledConditionAsset : ConditionAsset
{
    [SerializeField] private string enemyType;
    [SerializeField] private int requiredKills = 1;
    
    public override IConditionInstance CreateInstance()
    {
        return new EnemyKilledConditionInstance(enemyType, requiredKills);
    }
}

// 2. Create the instance class
public class EnemyKilledConditionInstance : IConditionInstance
{
    private readonly string _enemyType;
    private readonly int _requiredKills;
    private int _currentKills;
    private Action _onChanged;

    public bool IsMet => _currentKills >= _requiredKills;

    public EnemyKilledConditionInstance(string enemyType, int requiredKills)
    {
        _enemyType = enemyType;
        _requiredKills = requiredKills;
    }

    public void Bind(EventManager eventManager, QuestContext context, Action onChanged)
    {
        _onChanged = onChanged;
        eventManager.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
    }

    public void Unbind(EventManager eventManager, QuestContext context)
    {
        eventManager.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
    }

    private void OnEnemyKilled(EnemyKilledEvent evt)
    {
        if (evt.EnemyType == _enemyType)
        {
            _currentKills++;
            if (_currentKills <= _requiredKills)
            {
                _onChanged?.Invoke();
            }
        }
    }
}

// 3. Create the corresponding event
public class EnemyKilledEvent
{
    public string EnemyType { get; }
    public Vector3 Position { get; }
    
    public EnemyKilledEvent(string enemyType, Vector3 position)
    {
        EnemyType = enemyType;
        Position = position;
    }
}
```

### Creating Polling Conditions

```csharp
// For conditions that need continuous checking (distance, time, etc.)
[CreateAssetMenu(menuName = "Quests/Conditions/Player Distance")]
public class PlayerDistanceConditionAsset : ConditionAsset
{
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float requiredDistance = 5f;
    
    public override IConditionInstance CreateInstance()
    {
        return new PlayerDistanceConditionInstance(targetPosition, requiredDistance);
    }
}

public class PlayerDistanceConditionInstance : IConditionInstance, IPollingConditionInstance
{
    private readonly Vector3 _targetPosition;
    private readonly float _requiredDistance;
    private bool _isMet;
    private Action _onChanged;

    public bool IsMet => _isMet;

    public PlayerDistanceConditionInstance(Vector3 targetPosition, float requiredDistance)
    {
        _targetPosition = targetPosition;
        _requiredDistance = requiredDistance;
    }

    public void Bind(EventManager eventManager, QuestContext context, Action onChanged)
    {
        _onChanged = onChanged;
    }

    public void Unbind(EventManager eventManager, QuestContext context)
    {
        _onChanged = null;
    }

    public void Refresh(QuestContext context, Action onChanged)
    {
        if (context.Player == null) return;
        
        float distance = Vector3.Distance(context.Player.position, _targetPosition);
        bool newMet = distance <= _requiredDistance;
        
        if (newMet != _isMet)
        {
            _isMet = newMet;
            onChanged?.Invoke();
        }
    }
}
```

## Advanced Usage

### Quest Progress UI Integration

```csharp
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private Transform questContainer;
    [SerializeField] private GameObject questItemPrefab;
    
    void Start()
    {
        questManager.OnQuestCompleted += OnQuestCompleted;
        questManager.OnObjectiveStatusChanged += OnObjectiveChanged;
        RefreshUI();
    }
    
    void RefreshUI()
    {
        // Clear existing UI elements
        foreach (Transform child in questContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create UI elements for active quests
        foreach (var questState in questManager.ActiveQuests)
        {
            CreateQuestUI(questState);
        }
    }
    
    void CreateQuestUI(QuestState questState)
    {
        GameObject questItem = Instantiate(questItemPrefab, questContainer);
        var questDisplay = questItem.GetComponent<QuestDisplay>();
        questDisplay.Setup(questState);
    }
    
    void OnQuestCompleted(QuestState questState)
    {
        // Show completion notification
        ShowNotification($"Quest Completed: {questState.Definition.DisplayName}");
        RefreshUI();
    }
    
    void OnObjectiveChanged(ObjectiveState objectiveState)
    {
        // Update objective progress in UI
        RefreshUI();
    }
    
    void ShowNotification(string message)
    {
        // Implement notification system
        Debug.Log($"🎉 {message}");
    }
}

public class QuestDisplay : MonoBehaviour
{
    [SerializeField] private Text questTitle;
    [SerializeField] private Text questDescription;
    [SerializeField] private Transform objectiveContainer;
    [SerializeField] private GameObject objectivePrefab;
    
    public void Setup(QuestState questState)
    {
        questTitle.text = questState.Definition.DisplayName;
        questDescription.text = questState.Definition.Description;
        
        // Clear existing objectives
        foreach (Transform child in objectiveContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create objective UI elements
        foreach (var objectiveState in questState.GetObjectiveStates())
        {
            GameObject objItem = Instantiate(objectivePrefab, objectiveContainer);
            var objDisplay = objItem.GetComponent<ObjectiveDisplay>();
            objDisplay.Setup(objectiveState);
        }
    }
}
```

### Service Integration

```csharp
// Custom service for quest context
public interface IQuestInventoryService
{
    int GetItemCount(string itemId);
    bool HasItem(string itemId);
}

public class InventoryService : MonoBehaviour, IQuestInventoryService
{
    [SerializeField] private List<InventoryItem> items = new List<InventoryItem>();
    
    public int GetItemCount(string itemId)
    {
        var item = items.FirstOrDefault(i => i.itemId == itemId);
        return item?.count ?? 0;
    }
    
    public bool HasItem(string itemId)
    {
        return GetItemCount(itemId) > 0;
    }
}

// Integrate with quest context
public class CustomQuestPlayerRef : QuestPlayerRef
{
    [SerializeField] private InventoryService inventoryService;
    
    public override QuestContext BuildContext()
    {
        var context = base.BuildContext();
        
        // Add custom services
        var inventoryContextService = new QuestInventoryContextService(inventoryService);
        
        return new QuestContext(
            context.Player,
            inventoryContextService,
            context.AreaService,
            context.TimeService
        );
    }
}
```

### Quest State Serialization

The quest system provides serializable snapshots for quest progress. You decide when and how to persist them.

#### Basic Usage

```csharp
using DynamicBox.Quest.Core.State;

// Capture snapshot
var snapshot = QuestStateManager.CaptureSnapshot(questState);
string json = JsonUtility.ToJson(snapshot, prettyPrint: true);

// Restore from snapshot
var snapshot = JsonUtility.FromJson<QuestStateSnapshot>(json);
var restored = QuestStateManager.RestoreFromSnapshot(snapshot, questAsset, context);
```

#### Multiple Quests

```csharp
// Capture all quests
var saveData = QuestStateManager.CaptureAllSnapshots(
    questManager.ActiveQuests,
    metadata: "Chapter 3"
);

// Serialize (your choice of format)
string json = JsonUtility.ToJson(saveData);
// or: byte[] binary = MySerializer.Serialize(saveData);
// or: await cloudAPI.Upload(saveData);

// Restore all quests
var questAssetMap = Resources.LoadAll<QuestAsset>("Quests")
    .ToDictionary(q => q.QuestId);

var restored = QuestStateManager.RestoreAllFromSnapshots(
    saveData,
    questAssetMap,
    context
);

foreach (var quest in restored)
{
    questManager.AddQuest(quest);
}
```

#### Optional File I/O Helpers

For simple cases, use the built-in file helpers:

```csharp
// Save to file
QuestStateManager.SaveAllQuestsToFile(
    questManager.ActiveQuests,
    "path/to/save.json",
    metadata: "Player Save"
);

// Load from file
var quests = QuestStateManager.LoadAllQuestsFromFile(
    "path/to/save.json",
    questAssetMap,
    context
);
```

**Design Philosophy:** The quest system provides serializable data structures but doesn't dictate your persistence strategy. Integrate with any save system: local files, cloud saves, server APIs, platform-specific storage, or third-party assets.

See `Documentation/API_REFERENCE.md` for detailed API documentation.

### Testing and Debugging

```csharp
// Debug utilities for quest system
public class QuestDebugger : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private bool enableDebugUI = true;
    
    void OnGUI()
    {
        if (!enableDebugUI) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, Screen.height - 20));
        GUILayout.Label("Quest Debugger", GUI.skin.box);
        
        foreach (var questState in questManager.ActiveQuests)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label($"Quest: {questState.Definition.DisplayName}");
            GUILayout.Label($"Status: {questState.Status}");
            
            if (GUILayout.Button("Complete Quest"))
            {
                questManager.CompleteQuest(questState);
            }
            
            if (GUILayout.Button("Fail Quest"))
            {
                questManager.FailQuest(questState);
            }
            
            // Show objectives
            foreach (var obj in questState.GetObjectiveStates())
            {
                GUILayout.Label($"  • {obj.Definition.DisplayName}: {obj.Status}");
            }
            
            GUILayout.EndVertical();
        }
        
        GUILayout.EndArea();
    }
    
    [ContextMenu("Complete All Active Quests")]
    void CompleteAllQuests()
    {
        foreach (var quest in questManager.ActiveQuests.ToArray())
        {
            questManager.CompleteQuest(quest);
        }
    }
    
    [ContextMenu("Trigger Test Events")]
    void TriggerTestEvents()
    {
        // Useful for testing
        EventManager.Instance.Raise(new ItemCollectedEvent("test_item", 1));
        EventManager.Instance.Raise(new AreaEnteredEvent("test_area"));
        EventManager.Instance.Raise(new FlagChangedEvent("test_flag", true));
    }
}
```

## Tools and Debugging

### Quest Debugger Window
Access the visual quest debugger via Unity menu:
```
Tools → DynamicBox → Quest System → Quest Debugger
```

Features:
- Real-time quest status monitoring
- Manual quest completion/failure for testing
- Objective progress visualization
- Auto-refresh during play mode

### Built-in Testing
Run comprehensive test suite:
```csharp
// From Unity Console
DynamicBox.Quest.Tests.QuestSystemTests.RunAllTests();

// Test specific areas
DynamicBox.Quest.Tests.TestValidation.ValidateAllComponents();
DynamicBox.Quest.Tests.QuestSystemAdvancedTests.RunAdvancedTests();
```

### Inspector Integration
The package includes custom inspectors for:
- `QuestAsset` - Enhanced quest editing with objective management and validation
- `ObjectiveAsset` - Streamlined objective configuration with condition setup
- `ConditionGroupAsset` - Visual AND/OR logic builder with condition management

## Documentation

- **[API Reference](Documentation/API_REFERENCE.md)** – Complete API documentation with examples
- **[Architecture Deep Dive](Documentation/IMPLEMENTATION.md)** – Technical implementation details
- **[Sample Projects](Samples~/BasicQuestExample/)** – Complete example implementation
- **[Test Suite](Tests/README.md)** – Comprehensive testing guide and validation tools

## Testing and Validation

The package includes a comprehensive test suite with 90-95% coverage:

### Running Tests
```csharp
// Complete test suite (34+ tests)
DynamicBox.Quest.Tests.QuestSystemTests.RunAllTests();

// Integration tests (Unity environment)
// Add TestExecutor component to GameObject and run via Inspector

// Advanced functionality tests
DynamicBox.Quest.Tests.QuestSystemAdvancedTests.RunAdvancedTests();

// Infrastructure validation
DynamicBox.Quest.Tests.TestValidation.ValidateAllComponents();
```

### Test Categories
- **Unit Tests** (25+ tests) - Core functionality, conditions, state management
- **Integration Tests** (9 tests) - Unity component interaction, GameObject lifecycle
- **Advanced Tests** (10+ tests) - Manual quest control, service integration, performance
- **Edge Cases** - Error handling, null safety, circular dependencies

## Architecture

```
Game Event (ItemCollected)
    ↓
EventManager.Raise()
    ↓
ConditionInstance.HandleEvent()
    ↓
DirtyQueueProcessor.MarkDirty()
    ↓
DirtyQueueProcessor.ProcessAll() [in Update()]
    ↓
OnQuestCompleted event
```

## Core Types

### Assets (Designer-Authored Data)
- `QuestAsset` – Quest definition
- `ObjectiveAsset` – Objective definition
- `ConditionAsset` – Base condition (inherit to create custom conditions)
- `ConditionGroupAsset` – Composite conditions (AND/OR)

### State (Runtime)
- `QuestState` – Quest progress
- `ObjectiveState` – Objective progress
- `QuestLog` – Active quests registry

### Conditions
- `IConditionInstance` – Event-driven condition interface
- `IPollingConditionInstance` – Optional polling interface
- `ConditionGroupInstance` – AND/OR logic
- `ItemCollectedConditionInstance` – Example condition
- `TimeElapsedConditionInstance` – Example polling condition

### Infrastructure
- `QuestManager` – Main MonoBehaviour orchestrator
- `QuestContext` – Service container
- `IQuestEventBus` – Event bus interface
- `QuestPlayerRef` – Context builder

## Requirements

- **Unity**: 2021.3 or later
- **C#**: .NET Standard 2.1 compatible
- **Dependencies**: 
  - [DynamicBox EventManagement](https://github.com/mechaniqe/event-management) (automatically installed via UPM)

## Performance Considerations

- **Event-Driven**: Conditions only evaluate when relevant events occur
- **Dirty Queue Pattern**: Batch processing prevents excessive updates
- **Polling Optimization**: Time-based conditions use configurable update rates
- **Memory Management**: Automatic cleanup prevents GameObject accumulation
- **Test Coverage**: Validated performance with 1000+ simultaneous quests

## Common Integration Patterns

### Pattern 1: Event-Based Quest Triggers
```csharp
// Trigger quests based on game events
public class QuestTriggerSystem : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestAsset[] levelQuests;
    
    void Start()
    {
        EventManager.Instance.Subscribe<LevelStartedEvent>(OnLevelStarted);
        EventManager.Instance.Subscribe<PlayerLevelUpEvent>(OnPlayerLevelUp);
    }
    
    void OnLevelStarted(LevelStartedEvent evt)
    {
        var questForLevel = levelQuests.FirstOrDefault(q => q.QuestId.Contains(evt.LevelName));
        if (questForLevel != null)
        {
            questManager.StartQuest(questForLevel);
        }
    }
}
```

### Pattern 2: Quest Chain Management
```csharp
// Automatic quest chain progression
public class QuestChainManager : MonoBehaviour
{
    [System.Serializable]
    public struct QuestChain
    {
        public QuestAsset currentQuest;
        public QuestAsset nextQuest;
    }
    
    [SerializeField] private QuestManager questManager;
    [SerializeField] private QuestChain[] questChains;
    
    void Start()
    {
        questManager.OnQuestCompleted += OnQuestCompleted;
    }
    
    void OnQuestCompleted(QuestState completedQuest)
    {
        var chain = questChains.FirstOrDefault(c => 
            c.currentQuest.QuestId == completedQuest.Definition.QuestId);
            
        if (chain.nextQuest != null)
        {
            questManager.StartQuest(chain.nextQuest);
        }
    }
}
```

### Pattern 3: Save/Load Integration
```csharp
// Quest progress persistence (basic example)
[System.Serializable]
public class QuestSaveData
{
    public string questId;
    public QuestStatus status;
    public Dictionary<string, ObjectiveStatus> objectiveStatuses;
}

public class QuestPersistence : MonoBehaviour
{
    [SerializeField] private QuestManager questManager;
    
    public void SaveQuestProgress()
    {
        var saveData = new List<QuestSaveData>();
        
        foreach (var quest in questManager.ActiveQuests)
        {
            var data = new QuestSaveData
            {
                questId = quest.Definition.QuestId,
                status = quest.Status,
                objectiveStatuses = new Dictionary<string, ObjectiveStatus>()
            };
            
            foreach (var obj in quest.GetObjectiveStates())
            {
                data.objectiveStatuses[obj.Definition.ObjectiveId] = obj.Status;
            }
            
            saveData.Add(data);
        }
        
        // Save to PlayerPrefs, file, or your persistence system
        var json = JsonUtility.ToJson(new Serializable<List<QuestSaveData>>(saveData));
        PlayerPrefs.SetString("QuestProgress", json);
    }
    
    public void LoadQuestProgress()
    {
        // Load and restore quest states
        var json = PlayerPrefs.GetString("QuestProgress", "");
        if (string.IsNullOrEmpty(json)) return;
        
        var saveData = JsonUtility.FromJson<Serializable<List<QuestSaveData>>>(json).target;
        
        // Restore quest states...
        // Note: Full implementation requires additional quest restoration logic
    }
}
```

## Known Limitations & Roadmap

### Current Limitations (v0.1)
- **No Built-in Persistence**: Save/load system requires custom implementation (examples provided)
- **Single Player Focus**: Multi-actor/party support not included (planned for future)
- **EventManager Dependency**: Requires DynamicBox EventManagement package

### Version 0.2.0 (Planned)
- 🔄 Enhanced event system integration
- 🎨 Improved custom editor inspectors and UI
- 🐛 Visual quest debugger window (available now in beta)
- 📦 Additional built-in condition types
- 📊 Quest analytics and metrics

### Version 0.3.0+ (Future)
- 🚀 Performance optimizations for large-scale games
- 💾 Official save/load system package
- 👥 Multi-actor and party quest support  
- 🎮 Visual quest graph editor
- 🌐 Network synchronization support

### Contributing Features
We welcome contributions! Priority areas:
- Custom condition implementations
- Performance optimizations
- Documentation improvements
- Unit test coverage expansion
- Sample project contributions

## Support & Community

### Getting Help
1. **📚 Check Documentation**: [API Reference](Documentation/API_REFERENCE.md) and [Implementation Guide](Documentation/IMPLEMENTATION.md)
2. **🧪 Review Examples**: Study the [Sample Projects](Samples~/BasicQuestExample/) and [Unit Tests](Tests/)
3. **🐛 Search Issues**: Check [GitHub Issues](https://github.com/mechaniqe/unity-quest-core/issues) for known problems
4. **💬 Ask Questions**: Create a new issue with the `question` label
5. **🚨 Report Bugs**: Use the bug report template with reproduction steps

### FAQ

**Q: How do I create a quest that requires collecting multiple different items?**  
A: Use a ConditionGroup with AND logic containing multiple ItemCollectedConditions.

**Q: Can quests be saved and loaded?**  
A: The core system doesn't include persistence, but examples show how to implement save/load with your preferred method.

**Q: How do I make a quest available only after reaching a certain level?**  
A: Create a custom condition that checks player level, or use prerequisites on quest objectives.

**Q: Can I create branching quest storylines?**  
A: Yes! Use quest chains with conditional triggers based on previous quest completion outcomes.

**Q: How do I optimize performance for many quests?**  
A: The system uses event-driven architecture and dirty queue patterns. Our tests validate performance with 1000+ quests.

### Troubleshooting

**Quest not progressing**: Check that events are being published correctly using `EventManager.Instance.Raise()`  
**Conditions not evaluating**: Verify condition binding in the QuestDebugger window  
**UI not updating**: Ensure you're subscribed to `OnObjectiveStatusChanged` events  
**Performance issues**: Use the profiler to check event frequency and consider polling rate adjustments

## License & Credits

**License**: MIT License – see [LICENSE](LICENSE) for full terms

**Created by**: DynamicBox Team  
**Maintainer**: [Your Name/Studio]  
**Contributors**: See [Contributors](https://github.com/mechaniqe/unity-quest-core/contributors)

### Acknowledgments
- Unity Technologies for the excellent game engine
- The Unity community for feedback and inspiration
- Open source contributors who make projects like this possible

## Contributing

We welcome contributions from the community! Here's how to get started:

### 🐛 Bug Reports
- Use the issue template
- Include Unity version, package version, and reproduction steps
- Attach minimal reproduction project if possible

### 🚀 Feature Requests  
- Check existing issues first
- Describe the use case and benefit
- Consider implementing and submitting a PR

### 💻 Code Contributions
1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Make** your changes following the existing code style
4. **Add** unit tests for new functionality
5. **Test** thoroughly (run the full test suite)
6. **Commit** with clear, descriptive messages (`git commit -m 'Add amazing feature'`)
7. **Push** to your branch (`git push origin feature/amazing-feature`)
8. **Submit** a Pull Request with detailed description

### 📝 Documentation
- Improve existing documentation
- Add more usage examples
- Create tutorials or blog posts
- Translate documentation to other languages

### Code Style Guidelines
- Follow existing patterns in the codebase
- Add XML documentation for public APIs
- Include unit tests for new features
- Use descriptive variable and method names
- Keep classes focused and cohesive

---

**Version**: 0.1.0  
**Last Updated**: November 28, 2025  
**Unity Compatibility**: 2021.3 LTS+  
**Status**: Production Ready ✅

*"Making quest systems accessible, extensible, and delightful to work with."*
