using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Editor.GraphEditor
{
    /// <summary>
    /// Main graph view for quest visualization and editing.
    /// Handles node creation, connections, and user interactions.
    /// </summary>
    public class QuestGraphView : GraphView
    {
        private readonly QuestGraphEditorWindow _editorWindow;
        private QuestAsset _currentQuest;
        private QuestGraphLayout _layout;
        
        private readonly Vector2 _defaultNodeSize = new Vector2(250, 150);
        
        // Grid snapping settings
        private const float GridSize = 20f;
        private bool _snapToGrid = true;

        // Selection callback
        public event Action<BaseQuestNode> OnNodeSelected;

        public QuestGraphView(QuestGraphEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;
            
            // Add grid background
            var gridBackground = new GridBackground();
            Insert(0, gridBackground);
            gridBackground.StretchToParentSize();

            // Enable zoom and pan
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Register callbacks for node position changes
            graphViewChanged += OnGraphViewChanged;
            
            // Register selection callback
            RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);

            // Load stylesheet
            var styleSheet = Resources.Load<StyleSheet>("QuestGraphStyles");
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }

            // Set up view
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        }

        /// <summary>
        /// Loads a quest asset and generates the graph visualization.
        /// </summary>
        public void LoadQuest(QuestAsset quest)
        {
            _currentQuest = quest;
            
            // Load layout
            var questPath = AssetDatabase.GetAssetPath(quest);
            var layoutPath = QuestGraphLayout.GetLayoutPath(questPath);
            _layout = QuestGraphLayout.Load(layoutPath);
            
            // Clear existing graph
            ClearGraph();
            
            // Generate nodes from quest
            if (quest != null)
            {
                GenerateGraphFromQuest(quest);
            }
        }

        /// <summary>
        /// Refreshes all nodes in the graph to reflect current asset state.
        /// Used for bidirectional sync when assets are modified externally.
        /// </summary>
        public void RefreshAllNodes()
        {
            foreach (var element in graphElements.ToList())
            {
                if (element is BaseQuestNode node)
                {
                    node.RefreshNode();
                }
            }
        }

        /// <summary>
        /// Saves the current graph state back to the quest asset.
        /// </summary>
        public void SaveQuest()
        {
            if (_currentQuest == null)
                return;

            SaveLayout();
            Debug.Log("Graph layout saved");
        }

        /// <summary>
        /// Saves the current node positions and view state to disk.
        /// </summary>
        public void SaveLayout()
        {
            if (_currentQuest == null || _layout == null)
                return;

            // Save all node positions
            foreach (var element in graphElements.ToList())
            {
                if (element is BaseQuestNode node)
                {
                    var nodeId = QuestGraphLayout.GetNodeId(node.GetAsset());
                    if (!string.IsNullOrEmpty(nodeId))
                    {
                        _layout.SetNodePosition(nodeId, node.GetPosition());
                    }
                }
            }

            // Save to file
            var questPath = AssetDatabase.GetAssetPath(_currentQuest);
            var layoutPath = QuestGraphLayout.GetLayoutPath(questPath);
            _layout.Save(layoutPath);
        }

        private void ClearGraph()
        {
            // Remove all nodes
            foreach (var node in nodes.ToList())
            {
                RemoveElement(node);
            }

            // Remove all edges
            foreach (var edge in edges.ToList())
            {
                RemoveElement(edge);
            }
        }

        private void GenerateGraphFromQuest(QuestAsset quest)
        {
            if (quest == null)
                return;

            // Create quest node - use saved position or default
            var questNodeId = QuestGraphLayout.GetNodeId(quest);
            var questPosition = _layout?.GetNodePosition(questNodeId) ?? new Rect(100, 100, 250, 150);
            var questNode = CreateQuestNode(quest, questPosition.position);
            AddElement(questNode);

            // Create objective nodes with grid-aligned spacing
            float yOffset = 320; // Aligned to grid (16 * 20)
            float xOffset = 100;
            float objectiveSpacing = 300; // 15 grid cells
            int objectiveIndex = 0;

            foreach (var objective in quest.Objectives)
            {
                if (objective == null)
                    continue;

                // Use saved position or calculate default
                var objectiveNodeId = QuestGraphLayout.GetNodeId(objective);
                var savedPosition = _layout?.GetNodePosition(objectiveNodeId);
                var position = savedPosition?.position ?? new Vector2(xOffset + (objectiveIndex * objectiveSpacing), yOffset);
                
                var objectiveNode = CreateObjectiveNode(objective, position);
                AddElement(objectiveNode);

                // Connect quest to objective
                var edge = questNode.OutputPort.ConnectTo(objectiveNode.InputPort);
                AddElement(edge);

                objectiveIndex++;

                // Create condition nodes if they exist
                if (objective.CompletionCondition != null)
                {
                    var conditionNodeId = QuestGraphLayout.GetNodeId(objective.CompletionCondition);
                    var savedConditionPos = _layout?.GetNodePosition(conditionNodeId);
                    var conditionPos = savedConditionPos?.position ?? new Vector2(position.x, position.y + 260);
                    
                    var conditionNode = CreateConditionNode(objective.CompletionCondition, conditionPos);
                    if (conditionNode != null)
                    {
                        AddElement(conditionNode);
                        
                        // Connect objective completion port to condition
                        var conditionEdge = objectiveNode.CompletionPort.ConnectTo(conditionNode.InputPort);
                        AddElement(conditionEdge);
                    }
                }
            }
        }

        private QuestNode CreateQuestNode(QuestAsset quest, Vector2 position)
        {
            var snappedPosition = SnapToGrid(position);
            var node = new QuestNode(quest);
            node.SetPosition(new Rect(snappedPosition, _defaultNodeSize));
            return node;
        }

        private ObjectiveNode CreateObjectiveNode(ObjectiveAsset objective, Vector2 position)
        {
            var snappedPosition = SnapToGrid(position);
            var node = new ObjectiveNode(objective);
            node.SetPosition(new Rect(snappedPosition, _defaultNodeSize));
            return node;
        }

        private BaseConditionNode CreateConditionNode(ConditionAsset condition, Vector2 position)
        {
            BaseConditionNode node = null;

            // Determine condition type and create appropriate node
            var conditionType = condition.GetType().Name;
            
            switch (conditionType)
            {
                case "ItemCollectedConditionAsset":
                    node = new ItemCollectedConditionNode(condition);
                    break;
                case "AreaEnteredConditionAsset":
                    node = new AreaEnteredConditionNode(condition);
                    break;
                case "TimeElapsedConditionAsset":
                    node = new TimeElapsedConditionNode(condition);
                    break;
                case "CustomFlagConditionAsset":
                    node = new CustomFlagConditionNode(condition);
                    break;
                case "ConditionGroupAsset":
                    node = new ConditionGroupConditionNode(condition);
                    break;
                default:
                    Debug.LogWarning($"Unknown condition type: {conditionType}");
                    node = new GenericConditionNode(condition);
                    break;
            }

            if (node != null)
            {
                var snappedPosition = SnapToGrid(position);
                node.SetPosition(new Rect(snappedPosition, new Vector2(200, 120)));
            }

            return node;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                // Can't connect to self
                if (startPort.node == port.node)
                    return;

                // Can't connect same direction
                if (startPort.direction == port.direction)
                    return;

                // Validate port compatibility based on node types
                if (!ArePortsCompatible(startPort, port))
                    return;
                
                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private bool ArePortsCompatible(Port startPort, Port endPort)
        {
            var outputNode = (startPort.direction == Direction.Output ? startPort.node : endPort.node) as BaseQuestNode;
            var inputNode = (startPort.direction == Direction.Input ? startPort.node : endPort.node) as BaseQuestNode;

            if (outputNode == null || inputNode == null)
                return false;

            // Quest -> Objective: Valid
            if (outputNode is QuestNode && inputNode is ObjectiveNode)
                return true;

            // Objective -> Objective: Valid (prerequisites)
            if (outputNode is ObjectiveNode && inputNode is ObjectiveNode)
                return true;

            // Objective -> Condition: Valid (only from specific ports)
            if (outputNode is ObjectiveNode && inputNode is BaseConditionNode)
            {
                // Only allow connections from Completion or Failure ports
                var portName = startPort.direction == Direction.Output ? startPort.portName : endPort.portName;
                return portName == "Completion" || portName == "Failure";
            }

            // All other combinations are invalid
            return false;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (evt.target is GraphView)
            {
                var mousePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
                
                // Add node creation menu
                evt.menu.AppendAction("Add Node/Quest", 
                    (a) => CreateNodeAtPosition<QuestNode>(mousePosition));
                
                evt.menu.AppendAction("Add Node/Objective", 
                    (a) => CreateNodeAtPosition<ObjectiveNode>(mousePosition));
                
                evt.menu.AppendSeparator();
                
                evt.menu.AppendAction("Add Condition/📦 Item Collected", 
                    (a) => CreateConditionNodeAtPosition("ItemCollected", mousePosition));
                
                evt.menu.AppendAction("Add Condition/📍 Area Entered", 
                    (a) => CreateConditionNodeAtPosition("AreaEntered", mousePosition));
                
                evt.menu.AppendAction("Add Condition/⏱️ Time Elapsed", 
                    (a) => CreateConditionNodeAtPosition("TimeElapsed", mousePosition));
                
                evt.menu.AppendAction("Add Condition/🏁 Custom Flag", 
                    (a) => CreateConditionNodeAtPosition("CustomFlag", mousePosition));
                
                evt.menu.AppendAction("Add Condition/🔀 Condition Group", 
                    (a) => CreateConditionNodeAtPosition("ConditionGroup", mousePosition));
            }

            base.BuildContextualMenu(evt);
        }

        private void CreateNodeAtPosition<T>(Vector2 position) where T : BaseQuestNode, new()
        {
            var snappedPosition = SnapToGrid(position);
            var node = new T();
            node.SetPosition(new Rect(snappedPosition, _defaultNodeSize));
            
            // Node is created without asset - inspector will show "Create Asset" button
            AddElement(node);
            
            // Select the new node so user can see the inspector
            ClearSelection();
            AddToSelection(node);
        }

        private void CreateConditionNodeAtPosition(string conditionType, Vector2 position)
        {
            var snappedPosition = SnapToGrid(position);
            BaseConditionNode node = null;

            switch (conditionType)
            {
                case "ItemCollected":
                    node = new ItemCollectedConditionNode(null);
                    break;
                case "AreaEntered":
                    node = new AreaEnteredConditionNode(null);
                    break;
                case "TimeElapsed":
                    node = new TimeElapsedConditionNode(null);
                    break;
                case "CustomFlag":
                    node = new CustomFlagConditionNode(null);
                    break;
                case "ConditionGroup":
                    node = new ConditionGroupConditionNode(null);
                    break;
            }

            if (node != null)
            {
                node.SetPosition(new Rect(snappedPosition, _defaultNodeSize));
                AddElement(node);
                
                // Select the new node
                ClearSelection();
                AddToSelection(node);
            }
        }

        /// <summary>
        /// Snaps a position to the grid.
        /// </summary>
        private Vector2 SnapToGrid(Vector2 position)
        {
            if (!_snapToGrid)
                return position;

            return new Vector2(
                Mathf.Round(position.x / GridSize) * GridSize,
                Mathf.Round(position.y / GridSize) * GridSize
            );
        }

        /// <summary>
        /// Called when the graph changes (nodes moved, added, removed, etc.)
        /// </summary>
        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            // Handle moved elements (grid snapping)
            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    if (element is Node node)
                    {
                        if (_snapToGrid)
                        {
                            var currentPos = node.GetPosition().position;
                            var snappedPos = SnapToGrid(currentPos);
                            
                            if (currentPos != snappedPos)
                            {
                                var currentRect = node.GetPosition();
                                node.SetPosition(new Rect(snappedPos, currentRect.size));
                            }
                        }
                    }
                }
                
                // Save layout after moving nodes
                SaveLayout();
            }

            // Handle edge creation
            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    HandleEdgeCreated(edge);
                }
            }

            // Handle element removal (edges and nodes)
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    if (element is Edge edge)
                    {
                        HandleEdgeRemoved(edge);
                    }
                    else if (element is BaseQuestNode node)
                    {
                        HandleNodeRemoved(node);
                    }
                }
            }

            return change;
        }

        private void HandleEdgeCreated(Edge edge)
        {
            var outputNode = edge.output.node as BaseQuestNode;
            var inputNode = edge.input.node as BaseQuestNode;

            if (outputNode == null || inputNode == null)
                return;

            // Quest -> Objective connection
            if (outputNode is QuestNode questNode && inputNode is ObjectiveNode objectiveNode)
            {
                if (questNode.Asset != null && objectiveNode.Asset != null)
                {
                    Undo.RecordObject(questNode.Asset, "Add Objective to Quest");
                    
                    var objectivesField = typeof(QuestAsset).GetField("objectives",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var objectives = objectivesField?.GetValue(questNode.Asset) as System.Collections.Generic.List<ObjectiveAsset>;
                    
                    if (objectives != null && !objectives.Contains(objectiveNode.Asset))
                    {
                        objectives.Add(objectiveNode.Asset);
                        EditorUtility.SetDirty(questNode.Asset);
                    }
                }
            }
            // Objective -> Objective (prerequisite) connection
            else if (outputNode is ObjectiveNode sourceObj && inputNode is ObjectiveNode targetObj)
            {
                if (sourceObj.Asset != null && targetObj.Asset != null)
                {
                    Undo.RecordObject(targetObj.Asset, "Add Prerequisite to Objective");
                    
                    var prerequisitesField = typeof(ObjectiveAsset).GetField("prerequisites",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var prerequisites = prerequisitesField?.GetValue(targetObj.Asset) as System.Collections.Generic.List<ObjectiveAsset>;
                    
                    if (prerequisites != null && !prerequisites.Contains(sourceObj.Asset))
                    {
                        prerequisites.Add(sourceObj.Asset);
                        EditorUtility.SetDirty(targetObj.Asset);
                        targetObj.RefreshNode();
                    }
                }
            }
            // Objective -> Condition (completion/failure) connection
            else if (outputNode is ObjectiveNode objNode && inputNode is BaseConditionNode condNode)
            {
                if (objNode.Asset != null && condNode.Asset != null)
                {
                    Undo.RecordObject(objNode.Asset, "Set Objective Condition");
                    
                    // Determine which port was used (completion or failure)
                    var portName = edge.output.portName;
                    
                    if (portName == "Completion")
                    {
                        var completionField = typeof(ObjectiveAsset).GetField("completionCondition",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        completionField?.SetValue(objNode.Asset, condNode.Asset);
                    }
                    else if (portName == "Failure")
                    {
                        var failureField = typeof(ObjectiveAsset).GetField("failCondition",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        failureField?.SetValue(objNode.Asset, condNode.Asset);
                    }
                    
                    EditorUtility.SetDirty(objNode.Asset);
                    objNode.RefreshNode();
                }
            }
        }

        private void HandleEdgeRemoved(Edge edge)
        {
            var outputNode = edge.output.node as BaseQuestNode;
            var inputNode = edge.input.node as BaseQuestNode;

            if (outputNode == null || inputNode == null)
                return;

            // Quest -> Objective disconnection
            if (outputNode is QuestNode questNode && inputNode is ObjectiveNode objectiveNode)
            {
                if (questNode.Asset != null && objectiveNode.Asset != null)
                {
                    Undo.RecordObject(questNode.Asset, "Remove Objective from Quest");
                    
                    var objectivesField = typeof(QuestAsset).GetField("objectives",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var objectives = objectivesField?.GetValue(questNode.Asset) as System.Collections.Generic.List<ObjectiveAsset>;
                    
                    if (objectives != null && objectives.Contains(objectiveNode.Asset))
                    {
                        objectives.Remove(objectiveNode.Asset);
                        EditorUtility.SetDirty(questNode.Asset);
                    }
                }
            }
            // Objective -> Objective (prerequisite) disconnection
            else if (outputNode is ObjectiveNode sourceObj && inputNode is ObjectiveNode targetObj)
            {
                if (sourceObj.Asset != null && targetObj.Asset != null)
                {
                    Undo.RecordObject(targetObj.Asset, "Remove Prerequisite from Objective");
                    
                    var prerequisitesField = typeof(ObjectiveAsset).GetField("prerequisites",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var prerequisites = prerequisitesField?.GetValue(targetObj.Asset) as System.Collections.Generic.List<ObjectiveAsset>;
                    
                    if (prerequisites != null && prerequisites.Contains(sourceObj.Asset))
                    {
                        prerequisites.Remove(sourceObj.Asset);
                        EditorUtility.SetDirty(targetObj.Asset);
                        targetObj.RefreshNode();
                    }
                }
            }
            // Objective -> Condition disconnection
            else if (outputNode is ObjectiveNode objNode && inputNode is BaseConditionNode condNode)
            {
                if (objNode.Asset != null && condNode.Asset != null)
                {
                    Undo.RecordObject(objNode.Asset, "Remove Objective Condition");
                    
                    var portName = edge.output.portName;
                    
                    if (portName == "Completion")
                    {
                        var completionField = typeof(ObjectiveAsset).GetField("completionCondition",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (ReferenceEquals(completionField?.GetValue(objNode.Asset), condNode.Asset))
                        {
                            completionField.SetValue(objNode.Asset, null);
                        }
                    }
                    else if (portName == "Failure")
                    {
                        var failureField = typeof(ObjectiveAsset).GetField("failCondition",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (ReferenceEquals(failureField?.GetValue(objNode.Asset), condNode.Asset))
                        {
                            failureField.SetValue(objNode.Asset, null);
                        }
                    }
                    
                    EditorUtility.SetDirty(objNode.Asset);
                    objNode.RefreshNode();
                }
            }
        }

        private void HandleNodeRemoved(BaseQuestNode node)
        {
            // When a node is removed, we need to clean up any references to its asset
            if (node is ObjectiveNode objectiveNode && objectiveNode.Asset != null)
            {
                // Remove from quest objectives
                if (_currentQuest != null)
                {
                    var objectivesField = typeof(QuestAsset).GetField("objectives",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var objectives = objectivesField?.GetValue(_currentQuest) as System.Collections.Generic.List<ObjectiveAsset>;
                    
                    if (objectives != null && objectives.Contains(objectiveNode.Asset))
                    {
                        Undo.RecordObject(_currentQuest, "Remove Objective from Quest");
                        objectives.Remove(objectiveNode.Asset);
                        EditorUtility.SetDirty(_currentQuest);
                    }
                }
                
                // Remove from other objectives' prerequisites
                foreach (var n in nodes.ToList())
                {
                    if (n is ObjectiveNode otherObjNode && otherObjNode.Asset != null && otherObjNode != objectiveNode)
                    {
                        var prerequisitesField = typeof(ObjectiveAsset).GetField("prerequisites",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        var prerequisites = prerequisitesField?.GetValue(otherObjNode.Asset) as System.Collections.Generic.List<ObjectiveAsset>;
                        
                        if (prerequisites != null && prerequisites.Contains(objectiveNode.Asset))
                        {
                            Undo.RecordObject(otherObjNode.Asset, "Remove Prerequisite");
                            prerequisites.Remove(objectiveNode.Asset);
                            EditorUtility.SetDirty(otherObjNode.Asset);
                            otherObjNode.RefreshNode();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Toggles grid snapping on/off.
        /// </summary>
        public void SetSnapToGrid(bool enabled)
        {
            _snapToGrid = enabled;
        }

        /// <summary>
        /// Gets the current grid snap setting.
        /// </summary>
        public bool IsSnapToGridEnabled()
        {
            return _snapToGrid;
        }

        /// <summary>
        /// Handles mouse down to detect node selection.
        /// </summary>
        private void OnMouseDown(MouseDownEvent evt)
        {
            var clickedElement = evt.target as VisualElement;
            
            // Walk up the hierarchy to find if we clicked on a node
            while (clickedElement != null)
            {
                if (clickedElement is BaseQuestNode node)
                {
                    OnNodeSelected?.Invoke(node);
                    return;
                }
                clickedElement = clickedElement.parent;
            }
            
            // Clicked on empty space
            OnNodeSelected?.Invoke(null);
        }
    }
}
