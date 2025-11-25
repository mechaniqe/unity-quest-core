# Unity Quest Core

🎯 **A production-ready Unity package providing a designer-friendly, event-driven quest system**

[![Unity Version](https://img.shields.io/badge/Unity-2021.3+-blue.svg)](https://unity3d.com/get-unity/download)
[![Package Version](https://img.shields.io/badge/Package-0.1.0-green.svg)]()
[![License](https://img.shields.io/badge/License-MIT-orange.svg)](LICENSE.md)

## ✨ Features

- **🎨 Designer-Friendly** - Visual ScriptableObject-based authoring with custom editors
- **⚡ Event-Driven** - Efficient condition evaluation through decoupled event system
- **🔧 Production-Ready** - Thread-safe event bus, comprehensive testing, optimized performance  
- **🎯 Extensible** - Easy custom condition creation with clean interfaces
- **🛠️ Developer Tools** - Quest debugger window, validation, and sample content
- **📦 Zero Dependencies** - Works out of the box with any Unity project

## 🚀 Quick Start

### Installation

#### Option 1: Package Manager (Local)
1. Download or clone this repository
2. In Unity, open **Package Manager**
3. Click **"+"** → **"Add package from disk..."**
4. Navigate to `Packages/net.dynamicbox.quest.core/package.json`

#### Option 2: Manual Installation
1. Copy `Packages/net.dynamicbox.quest.core/` to your Unity project's `Packages/` folder
2. Unity will automatically import the package

### Basic Usage

```csharp
// 1. Setup the quest system
var eventBus = new EventManagementQuestBus();
questManager.SetEventBus(eventBus);

// 2. Start a quest
questManager.StartQuest(questAsset);

// 3. Publish events from your game
eventBus.Publish(new ItemCollectedEvent("sword", 1));

// 4. Listen for quest completion
questManager.OnQuestCompleted += OnQuestCompleted;
```

### Import Sample

1. Open **Package Manager** in Unity
2. Select **"DynamicBox Quest Core"** from **"In Project"**
3. Expand **"Samples"** and import **"Basic Quest Example"**
4. Study the sample code and run the example scene

## 📚 Documentation

- **[API Reference](Packages/net.dynamicbox.quest.core/Documentation/API_REFERENCE.md)** - Complete API documentation
- **[Implementation Guide](Packages/net.dynamicbox.quest.core/Documentation/IMPLEMENTATION.md)** - Architecture and integration guide
- **[Sample README](Packages/net.dynamicbox.quest.core/Samples~/BasicQuestExample/README.md)** - Step-by-step sample walkthrough

## 🛠️ Editor Tools

- **Quest Asset Editor** - Visual quest configuration with validation
- **Condition Group Editor** - Drag-and-drop condition management  
- **Quest Debugger** - Runtime quest monitoring (**Tools > DynamicBox Quest > Quest Debugger**)

## 📖 Creating Your First Quest

1. **Create Quest Asset**: Right-click → **Create > DynamicBox Quest > Quest Asset**
2. **Create Objective**: Right-click → **Create > DynamicBox Quest > Objective Asset**  
3. **Create Conditions**: Right-click → **Create > DynamicBox Quest > Conditions > [Type]**
4. **Configure in Inspector**: Use the custom editors to set up your quest logic
5. **Start Quest**: Call `questManager.StartQuest(questAsset)` in your code

## 🧩 Built-in Condition Types

- **Item Collected** - Track when players collect specific items
- **Time Elapsed** - Time-based objectives and delays
- **Area Entered** - Location-based objectives  
- **Custom Flag** - Generic gameplay state conditions

## 🔧 Extending the System

Create custom conditions by extending `ConditionAsset`:

```csharp
[CreateAssetMenu(menuName = "DynamicBox Quest/Conditions/My Custom Condition")]
public class MyConditionAsset : ConditionAsset
{
    public override IConditionInstance CreateInstance(QuestContext context)
    {
        return new MyConditionInstance(this, context);
    }
}
```

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## 🤝 Contributing

This is a Unity package project. To contribute:

1. Clone the repository
2. Open the project in Unity 2021.3+
3. Make changes to files in `Packages/net.dynamicbox.quest.core/`
4. Test your changes using the included test suite
5. Submit a pull request

## 📞 Support

For questions, issues, or feature requests, please open an issue on the project repository.

---

*Made with ❤️ for the Unity community*

🎯 **A production-ready Unity package providing a designer-friendly, event-driven quest system**

[![Unity Version](https://img.shields.io/badge/Unity-2021.3+-blue.svg)](https://unity3d.com/get-unity/download)
[![Package Version](https://img.shields.io/badge/Package-0.1.0-green.svg)]()
[![License](https://img.shields.io/badge/License-MIT-orange.svg)](Packages/net.dynamicbox.quest.core/LICENSE.md)

## ✨ Features

- **🎨 Designer-Friendly**: Visual ScriptableObject-based authoring with custom editors
- **⚡ Event-Driven**: Efficient condition evaluation through decoupled event system
- **🔧 Production-Ready**: Thread-safe event bus, comprehensive testing, optimized performance  
- **🎯 Extensible**: Easy custom condition creation with clean interfaces
- **🛠️ Developer Tools**: Quest debugger window, validation, and sample content
- **📦 Zero Dependencies**: Works out of the box with any Unity project

## 🚀 Quick Start

### Installation
1. **Package Manager**: Add package from git URL: 
   ```
   https://github.com/your-org/unity-quest-core.git?path=/Packages/net.dynamicbox.quest.core
   ```
2. **Local Development**: Copy `Packages/net.dynamicbox.quest.core/` to your Unity project's `Packages/` folder

### Basic Usage
```csharp
// 1. Setup event bus
var eventBus = new EventManagementQuestBus();

// 2. Configure quest manager  
questManager.SetEventBus(eventBus);

// 3. Start a quest
questManager.StartQuest(questAsset);

// 4. Publish game events
eventBus.Publish(new ItemCollectedEvent("sword", 1));
```

### Sample Content
Import the **Basic Quest Example** sample via Package Manager to see a complete working implementation.

## 📚 Documentation

- **[API Reference](Packages/net.dynamicbox.quest.core/Documentation/API_REFERENCE.md)** - Complete API documentation
- **[Implementation Guide](Packages/net.dynamicbox.quest.core/Documentation/IMPLEMENTATION.md)** - Architecture and integration details
- **[Sample README](Packages/net.dynamicbox.quest.core/Samples~/BasicQuestExample/README.md)** - Step-by-step usage guide

## 🏗️ Architecture

- **ScriptableObject Authoring** - Designer-friendly quest creation
- **Event-Driven Conditions** - Efficient, decoupled evaluation
- **Composite Logic** - AND/OR condition groups
- **Thread-Safe Event Bus** - Production-ready messaging
- **Unity Package Manager** - Proper package structure and tooling

## 📋 Requirements

- **Unity 2021.3+**
- **No external dependencies**

## 📄 License

MIT License - See [LICENSE.md](Packages/net.dynamicbox.quest.core/LICENSE.md) for details

---

**Ready for production use in Unity projects** 🎮
- **Tested**: Comprehensive unit test coverage

## Quick Start

### 1. Create a Quest
Right-click in Project → Create → Quests → Quest

### 2. Add Quest Manager
Add `QuestManager` component to a GameObject in your scene

### 3. Start Quest
```csharp
[SerializeField] private QuestManager questManager;
[SerializeField] private QuestAsset myQuest;

void Start() {
    questManager.StartQuest(myQuest);
}
```

### 4. Publish Events
```csharp
// From your game systems
eventBus.Publish(new ItemCollectedEvent("sword", 1));
```

## Documentation

Complete documentation is available in the package:
- `Documentation/API_REFERENCE.md` - Full API documentation
- `Documentation/IMPLEMENTATION.md` - Architecture details

## Requirements

- Unity 2021.3 or later

## Status

Version 0.1.0 - Foundation complete with core quest system functionality.

Note: EventManagementQuestBus requires integration with your event system.

## License

MIT License - see LICENSE.md
