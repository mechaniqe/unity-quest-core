using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DynamicBox.Quest.Editor.GraphEditor
{
    /// <summary>
    /// Handles keyboard shortcuts for the quest graph editor.
    /// </summary>
    public class GraphEditorKeyboardHandler
    {
        private readonly QuestGraphView _graphView;
        private readonly QuestGraphEditorWindow _window;

        public GraphEditorKeyboardHandler(QuestGraphView graphView, QuestGraphEditorWindow window)
        {
            _graphView = graphView;
            _window = window;
        }

        /// <summary>
        /// Registers keyboard event handlers.
        /// </summary>
        public void RegisterHandlers(VisualElement root)
        {
            root.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        /// <summary>
        /// Unregisters keyboard event handlers.
        /// </summary>
        public void UnregisterHandlers(VisualElement root)
        {
            root.UnregisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            // Check if we're typing in a text field
            if (evt.target is TextField || evt.target is TextElement)
            {
                return; // Don't process shortcuts while typing
            }

            switch (evt.keyCode)
            {
                case KeyCode.Delete:
                case KeyCode.Backspace:
                    DeleteSelected();
                    evt.StopPropagation();
                    break;

                case KeyCode.F:
                    if (!evt.ctrlKey && !evt.commandKey)
                    {
                        FrameSelection();
                        evt.StopPropagation();
                    }
                    break;

                case KeyCode.A:
                    if (evt.ctrlKey || evt.commandKey)
                    {
                        SelectAll();
                        evt.StopPropagation();
                    }
                    break;

                case KeyCode.S:
                    if (evt.ctrlKey || evt.commandKey)
                    {
                        SaveGraph();
                        evt.StopPropagation();
                    }
                    break;

                case KeyCode.D:
                    if (evt.ctrlKey || evt.commandKey)
                    {
                        DuplicateSelected();
                        evt.StopPropagation();
                    }
                    break;
            }
        }

        private void DeleteSelected()
        {
            var selectedNodes = _graphView.selection.OfType<UnityEditor.Experimental.GraphView.Node>().ToList();
            var selectedEdges = _graphView.selection.OfType<UnityEditor.Experimental.GraphView.Edge>().ToList();

            if (selectedNodes.Count == 0 && selectedEdges.Count == 0)
                return;

            if (EditorUtility.DisplayDialog(
                "Delete Selected",
                $"Delete {selectedNodes.Count} node(s) and {selectedEdges.Count} connection(s)?",
                "Delete",
                "Cancel"))
            {
                foreach (var edge in selectedEdges)
                {
                    _graphView.RemoveElement(edge);
                }

                foreach (var node in selectedNodes)
                {
                    _graphView.RemoveElement(node);
                }

                Debug.Log($"Deleted {selectedNodes.Count} node(s) and {selectedEdges.Count} edge(s)");
            }
        }

        private void FrameSelection()
        {
            var selectedElements = _graphView.selection.OfType<UnityEditor.Experimental.GraphView.GraphElement>().ToList();
            
            if (selectedElements.Count > 0)
            {
                _graphView.FrameSelection();
            }
            else
            {
                _graphView.FrameAll();
            }
        }

        private void SelectAll()
        {
            _graphView.ClearSelection();
            
            foreach (var node in _graphView.nodes)
            {
                _graphView.AddToSelection(node);
            }

            Debug.Log($"Selected {_graphView.selection.Count()} nodes");
        }

        private void SaveGraph()
        {
            // Trigger save through the window
            var saveMethod = typeof(QuestGraphEditorWindow).GetMethod("SaveQuest",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            saveMethod?.Invoke(_window, null);
        }

        private void DuplicateSelected()
        {
            var selectedNodes = _graphView.selection.OfType<BaseQuestNode>().ToList();
            
            if (selectedNodes.Count == 0)
            {
                Debug.Log("No nodes selected to duplicate");
                return;
            }

            _graphView.ClearSelection();
            var duplicatedNodes = new System.Collections.Generic.List<BaseQuestNode>();
            var offset = new Vector2(40, 40); // Offset for duplicated nodes

            foreach (var selectedNode in selectedNodes)
            {
                BaseQuestNode newNode = null;
                
                // Create appropriate node type based on selected node
                if (selectedNode is QuestNode questNode && questNode.Asset != null)
                {
                    newNode = new QuestNode(questNode.Asset);
                }
                else if (selectedNode is ObjectiveNode objectiveNode && objectiveNode.Asset != null)
                {
                    newNode = new ObjectiveNode(objectiveNode.Asset);
                }
                else if (selectedNode is ItemCollectedConditionNode itemNode && itemNode.Asset != null)
                {
                    newNode = new ItemCollectedConditionNode(itemNode.Asset);
                }
                else if (selectedNode is AreaEnteredConditionNode areaNode && areaNode.Asset != null)
                {
                    newNode = new AreaEnteredConditionNode(areaNode.Asset);
                }
                else if (selectedNode is TimeElapsedConditionNode timeNode && timeNode.Asset != null)
                {
                    newNode = new TimeElapsedConditionNode(timeNode.Asset);
                }
                else if (selectedNode is CustomFlagConditionNode flagNode && flagNode.Asset != null)
                {
                    newNode = new CustomFlagConditionNode(flagNode.Asset);
                }
                else if (selectedNode is ConditionGroupConditionNode groupNode && groupNode.Asset != null)
                {
                    newNode = new ConditionGroupConditionNode(groupNode.Asset);
                }

                if (newNode != null)
                {
                    // Position the new node with offset
                    var originalPos = selectedNode.GetPosition();
                    newNode.SetPosition(new Rect(originalPos.position + offset, originalPos.size));
                    
                    _graphView.AddElement(newNode);
                    duplicatedNodes.Add(newNode);
                }
            }

            // Select duplicated nodes
            foreach (var node in duplicatedNodes)
            {
                _graphView.AddToSelection(node);
            }

            Debug.Log($"Duplicated {duplicatedNodes.Count} node(s)");
        }
    }
}
