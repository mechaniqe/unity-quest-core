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

**Dependencies**: The DynamicBox EventManagement package will be automatically installed.

### Manual Installation

1. Clone the repository
2. Copy the `Packages/net.dynamicbox.quest.core` folder to your project's `Packages/` directory
3. Ensure the DynamicBox EventManagement dependency is available

### Setup

The Quest System automatically uses `EventManager.Instance` - no manual setup required!

1. Add a QuestManager component to a GameObject in your scene
2. Assign a QuestPlayerRef to provide quest context
3. Start creating quests!

## Quick Start

### 1. Create a Quest in Inspector

```
Right-click → Create → Quests → Quest
├─ Quest ID: "collect_sword"
├─ Display Name: "Find the Sword"
└─ Add Objective:
    ├─ Objective ID: "obj_1"
    ├─ Completion Condition:
    │   └─ Create → Quests → Conditions → Item Collected
    │       ├─ Item ID: "sword"
    │       └─ Required Count: 1
```

### 2. Wire Quest Manager

```csharp
[SerializeField] private QuestManager questManager;
[SerializeField] private QuestAsset myQuest;

void Start() {
    questManager.OnQuestCompleted += HandleComplete;
    questManager.StartQuest(myQuest);
}

void HandleComplete(QuestState quest) {
    Debug.Log($"✓ {quest.Definition.DisplayName}");
}
```

### 3. Publish Events

```csharp
// In your inventory/loot system
eventBus.Publish(new ItemCollectedEvent("sword", 1));
```

## Building Custom Conditions

```csharp
// 1. Create the asset
[CreateAssetMenu(menuName = "Quests/Conditions/Area Entered")]
public class AreaEnteredConditionAsset : ConditionAsset
{
    [SerializeField] private string areaId;
    
    public override IConditionInstance CreateInstance()
    {
        return new AreaEnteredConditionInstance(areaId);
    }
}

// 2. Create the instance
public class AreaEnteredConditionInstance : IConditionInstance
{
    private readonly string _areaId;
    private bool _isMet;
    private Action _onChanged;

    public bool IsMet => _isMet;

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

## Documentation

- **[API Reference](Documentation/API_REFERENCE.md)** – Complete API documentation
- **[Architecture](Documentation/IMPLEMENTATION.md)** – Technical deep dive
- **[Examples](Samples~/)** – Sample projects and quests

## Testing

The package includes comprehensive unit tests:

```csharp
GenericQuest.Tests.QuestSystemTests.RunAllTests();
```

## Architecture

```
Game Event (ItemCollected)
    ↓
IQuestEventBus.Publish()
    ↓
ConditionInstance.OnItemCollected()
    ↓
QuestManager.MarkDirty()
    ↓
QuestManager.ProcessDirtyQueue()
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
- **C#**: 9.0+
- **Dependencies**: 
  - [DynamicBox EventManagement](https://github.com/mechaniqe/event-management) (for production event handling)

## Known Limitations (v0.1)

- No persistence/save system (planned as separate package)
- No multi-actor/party support (single player focus)
- Requires DynamicBox EventManagement package to function

## Roadmap

### 0.2.0 (Planned)
- Event system integration (mechaniqe)
- Custom editor inspectors
- Quest debugger window
- Additional example conditions

### 0.3.0+
- Performance optimization
- Save/load system integration
- Multi-actor support
- Graph editor for quest composition

## Support

For questions or issues:

1. Check the [API Reference](Documentation/API_REFERENCE.md)
2. Review [Architecture Documentation](Documentation/IMPLEMENTATION.md)
3. Study the [Unit Tests](Tests/)
4. Create an issue on GitHub

## License

MIT License – see [LICENSE.md](LICENSE.md)

## Contributing

Contributions are welcome! Please follow these guidelines:

1. Create a feature branch (`git checkout -b feature/your-feature`)
2. Commit your changes (`git commit -am 'Add feature'`)
3. Push to the branch (`git push origin feature/your-feature`)
4. Create a Pull Request

## Credits

Developed by [Your Studio Name]

Part of the DynamicBox Quest Core project – A mission to make quest systems accessible and extensible.

---

**Version**: 0.1.0  
**Last Updated**: 2025-11-25  
**Unity Version**: 2021.3+
