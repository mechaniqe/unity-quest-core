# Core Module Organization

The Core module has been organized into logical folders to improve maintainability and code navigation:

## 📁 Folder Structure

### **Assets/**
Contains ScriptableObject asset classes that can be created through Unity's Create Asset Menu:
- `QuestAsset.cs` - Main quest configuration asset
- `ObjectiveAsset.cs` - Individual objective configuration
- `ConditionAsset.cs` - Base class for condition assets
- `ConditionGroupAsset.cs` - Grouping conditions with AND/OR logic
- `AreaEnteredConditionAsset.cs` - Area-based trigger conditions
- `CustomFlagConditionAsset.cs` - Custom flag-based conditions
- `ItemCollectedConditionAsset.cs` - Item collection conditions
- `TimeElapsedConditionAsset.cs` - Time-based conditions

### **Conditions/**
Contains runtime condition instances and interfaces:
- `IConditionInstance.cs` - Core condition interface
- `ConditionGroupInstance.cs` - Runtime condition grouping
- `ItemCollectedConditionInstance.cs` - Item collection condition logic
- `TimeElapsedConditionInstance.cs` - Time-based condition logic

### **Management/**
Contains system management and core functionality:
- `QuestManager.cs` - Main quest system controller
- `QuestLog.cs` - Quest tracking and persistence
- `QuestContext.cs` - Runtime context and dependencies
- `QuestPlayerRef.cs` - Player reference management

### **State/**
Contains runtime state classes:
- `QuestState.cs` - Runtime quest state management
- `ObjectiveState.cs` - Individual objective state tracking
- `QuestStatus.cs` - Quest status enumeration

## 🔄 Benefits of This Organization

1. **Clear Separation of Concerns**: Each folder has a specific responsibility
2. **Easier Navigation**: Related classes are grouped together
3. **Better Maintainability**: Changes to specific functionality are isolated
4. **Logical Dependencies**: The folder structure reflects the dependency hierarchy
5. **Unity Editor Integration**: Asset classes are clearly separated from runtime logic

## 🚀 Usage

All classes maintain their original namespaces (`DynamicBox.Quest.Core`), so existing code continues to work without changes. The physical organization simply makes the codebase more manageable.
