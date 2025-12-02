# Quest Graph Editor - Phase 1 Implementation

## Overview

The Visual Quest Graph Editor is a node-based interface for creating and visualizing quests in Unity. This implementation provides the foundation for visual quest authoring.

## Features (Phase 1 - Foundation)

✅ **Main Editor Window**
- Open via `Tools → DynamicBox → Quest System → Quest Graph Editor`
- Toolbar with New, Load, and Save buttons
- Grid background with zoom and pan controls

✅ **Quest Visualization**
- Automatically generates graph from existing QuestAssets
- Double-click any QuestAsset in the Project to open in graph editor
- Visual node-based representation of quest structure

✅ **Node Types**
- **Quest Node** (Blue) - Represents the main quest
- **Objective Node** (Green) - Represents quest objectives
- **Condition Nodes** (Brown variants):
  - Item Collected Condition
  - Area Entered Condition
  - Time Elapsed Condition
  - Custom Flag Condition
  - Condition Group (AND/OR logic)

✅ **Visual Connections**
- Quest → Objectives (standard connections)
- Objective → Completion Condition (green port)
- Objective → Failure Condition (red port)
- Objective → Next Objective (blue port for prerequisites)

✅ **Dark Theme Styling**
- Professional dark theme similar to Shader Graph
- Color-coded nodes and connections
- Clear visual hierarchy

## How to Use

### Opening the Graph Editor

**Method 1: Menu**
1. Go to `Tools → DynamicBox → Quest System → Quest Graph Editor`
2. Click "Load Quest" to select an existing quest
3. Or click "New Quest" to create a new one

**Method 2: Asset Double-Click**
1. In the Project window, double-click any QuestAsset
2. The Graph Editor will automatically open and load that quest

### Creating a New Quest

1. Open the Graph Editor
2. Click "New Quest" in the toolbar
3. Choose a location and name for your quest
4. A new QuestAsset will be created and loaded

### Visualizing Existing Quests

1. Double-click a QuestAsset in your project
2. The graph will automatically generate:
   - Quest node at the top
   - Objective nodes below
   - Condition nodes connected to objectives

### Navigation

- **Zoom**: Mouse wheel or Content Zoomer manipulator
- **Pan**: Middle mouse drag or hold Alt + drag
- **Select**: Click on nodes or edges
- **Multi-select**: Hold Shift and click multiple nodes
- **Box Select**: Click and drag to select multiple nodes

### Right-Click Context Menu

Right-click in the graph to add new nodes:
- Add Quest Node
- Add Objective Node
- Add Condition → Item Collected
- Add Condition → Area Entered
- Add Condition → Time Elapsed
- Add Condition → Custom Flag
- Add Condition → Condition Group

*Note: In Phase 1, this creates placeholder nodes. Full creation functionality comes in Phase 3.*

## Node Details

### Quest Node (📜)
- **Color**: Blue (#2A4D69)
- **Shows**: Quest ID, Display Name, Description (truncated), Objective count
- **Ports**: Output ports for connecting to objectives

### Objective Node (🎯)
- **Color**: Green (#2D4A3E)
- **Shows**: Objective ID, Title, Description (truncated), Optional badge, Prerequisite count
- **Ports**:
  - Input: Connects from Quest or other Objectives
  - ✓ Completion: Connects to completion condition
  - ✗ Failure: Connects to failure condition
  - 🔗 Next Obj: Connects to dependent objectives

### Condition Nodes

All condition nodes display:
- Condition type with icon
- Relevant configuration (item ID, area ID, time, etc.)
- Service dependency (which game service is required)
- Evaluation type (Event-Driven or Polling)

**Item Collected (📦)**
- Shows: Item ID, Required Quantity
- Service: Inventory

**Area Entered (📍)**
- Shows: Area ID
- Service: Area

**Time Elapsed (⏱️)**
- Shows: Required seconds
- Service: Time
- Type: Polling

**Custom Flag (🏁)**
- Shows: Flag ID, Expected value
- Service: Flag

**Condition Group (🔀)**
- Shows: Logic type (AND/OR), Child count
- Type: Composite

## What's Coming Next

### Phase 2: Node System Enhancement (Week 2)
- Edit node properties directly in the graph
- Node inspector panel for detailed editing
- Create new assets from graph
- Connect/disconnect nodes
- Visual styling improvements

### Phase 3: Editing & Sync (Week 3)
- Full bidirectional sync between graph and assets
- Save graph changes back to ScriptableObjects
- Create new objectives and conditions from graph
- Undo/Redo support
- Drag-and-drop asset references

### Phase 4: Polish & Features (Week 4)
- Mini-map for large graphs
- Search and filter nodes
- Validation overlays (show errors/warnings)
- Copy/paste/duplicate nodes
- Export graph as image
- Sticky notes for documentation

## Technical Details

### Architecture

```
Editor/
├─ GraphEditor/
│   ├─ QuestGraphEditorWindow.cs    (Main window)
│   ├─ QuestGraphView.cs            (GraphView container)
│   ├─ Nodes/
│   │   ├─ BaseQuestNode.cs         (Abstract base)
│   │   ├─ QuestNode.cs             
│   │   ├─ ObjectiveNode.cs         
│   │   ├─ BaseConditionNode.cs     
│   │   └─ ConditionNodes.cs        (All condition types)
│   └─ USS/
│       └─ QuestGraphStyles.uss     (Dark theme)
└─ Resources/
    └─ QuestGraphStyles.uss         (Loaded at runtime)
```

### Unity GraphView API

This implementation uses Unity's native GraphView API, which provides:
- Built-in zoom, pan, selection
- Port connection system
- Grid background
- USS styling support
- Undo/Redo framework

### Compatibility

- **Unity Version**: 2021.3+
- **Dependencies**: None (uses built-in GraphView)
- **Platform**: Editor only

## Known Limitations (Phase 1)

⚠️ **Read-Only Visualization**
- Phase 1 is focused on visualization
- Editing nodes does not yet save changes
- Creating new nodes creates placeholders only
- Full editing comes in Phase 3

⚠️ **No Auto-Layout**
- Node positions are calculated based on simple offsets
- Auto-layout algorithm planned for Phase 4 (optional)

⚠️ **No Persistence**
- Graph layout is not yet saved
- Reopening a quest generates fresh layout
- Layout persistence comes in Phase 3

## Troubleshooting

**Q: Graph Editor window is blank**
- Check Console for errors
- Ensure QuestAsset is valid and has objectives
- Try closing and reopening the window

**Q: Nodes don't show data**
- This happens if the asset has null references
- Check that objectives and conditions are properly assigned
- Validate the quest in the inspector

**Q: Stylesheet not loading**
- Ensure `QuestGraphStyles.uss` is in `Editor/Resources/`
- Check that the file has no syntax errors
- Reimport the asset if needed

**Q: Double-click doesn't open graph**
- The `OnOpenAsset` callback should handle this
- Check that no other custom editor is intercepting
- Fall back to manual Load via toolbar

## Feedback & Next Steps

This is Phase 1 of 4. The foundation is now in place!

**What works:**
✅ Visual representation of quests
✅ Node-based graph interface
✅ Dark theme styling
✅ Basic navigation

**Coming soon:**
🔄 Full editing capabilities
🔄 Asset creation from graph
🔄 Layout persistence
🔄 Advanced features

Please test the visualization with your existing quests and report any issues or suggestions for Phase 2!
