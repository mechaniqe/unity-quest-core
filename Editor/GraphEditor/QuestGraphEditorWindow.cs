using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Editor.GraphEditor
{
    /// <summary>
    /// Main editor window for visual quest graph editing.
    /// Provides a node-based interface for creating and editing quests.
    /// </summary>
    public class QuestGraphEditorWindow : EditorWindow
    {
        private QuestGraphView _graphView;
        private NodeInspectorView _inspectorView;
        private GraphEditorKeyboardHandler _keyboardHandler;
        private QuestAsset _currentQuest;
        
        [MenuItem("Tools/DynamicBox/Quest System/Quest Graph Editor")]
        public static void OpenWindow()
        {
            var window = GetWindow<QuestGraphEditorWindow>();
            window.titleContent = new GUIContent("Quest Graph Editor");
            window.minSize = new Vector2(1000, 600);
        }

        /// <summary>
        /// Opens the graph editor for a specific quest asset.
        /// Called when double-clicking a QuestAsset in the project.
        /// </summary>
        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as QuestAsset;
            if (asset != null)
            {
                OpenWindow();
                var window = GetWindow<QuestGraphEditorWindow>();
                window.LoadQuest(asset);
                return true;
            }
            return false;
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
            
            // Subscribe to asset modification events for bidirectional sync
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            if (_graphView != null)
            {
                _graphView.OnNodeSelected -= OnNodeSelected;
            }
            
            if (_keyboardHandler != null)
            {
                _keyboardHandler.UnregisterHandlers(rootVisualElement);
            }
            
            // Unsubscribe from events
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            // Refresh graph when undo/redo is performed
            if (_currentQuest != null)
            {
                RefreshGraph();
            }
        }

        private double _lastUpdateTime;
        private const double UpdateInterval = 0.5; // Check every 0.5 seconds

        private void Update()
        {
            // Throttle external change detection to avoid performance issues
            if (EditorApplication.timeSinceStartup - _lastUpdateTime < UpdateInterval)
                return;

            _lastUpdateTime = EditorApplication.timeSinceStartup;

            // Periodically check if assets have been modified externally
            // This catches changes made through the Inspector or scripts
            if (_currentQuest != null && EditorUtility.IsDirty(_currentQuest))
            {
                RefreshGraph();
                EditorUtility.ClearDirty(_currentQuest);
            }
        }

        private void RefreshGraph()
        {
            if (_graphView != null && _currentQuest != null)
            {
                // Light refresh - just update node contents without recreating the graph
                _graphView.RefreshAllNodes();
            }
        }

        private void ConstructGraphView()
        {
            // Create main container for graph and inspector
            var mainContainer = new VisualElement();
            mainContainer.style.flexDirection = FlexDirection.Row;
            mainContainer.style.flexGrow = 1;
            
            // Graph view (left side)
            _graphView = new QuestGraphView(this)
            {
                name = "Quest Graph"
            };
            _graphView.style.flexGrow = 1;
            mainContainer.Add(_graphView);
            
            // Inspector view (right side)
            _inspectorView = new NodeInspectorView(this);
            mainContainer.Add(_inspectorView);
            
            rootVisualElement.Add(mainContainer);
            
            // Register selection callback
            _graphView.OnNodeSelected += OnNodeSelected;
            
            // Setup keyboard shortcuts
            _keyboardHandler = new GraphEditorKeyboardHandler(_graphView, this);
            _keyboardHandler.RegisterHandlers(rootVisualElement);
        }

        private void OnNodeSelected(BaseQuestNode node)
        {
            _inspectorView.UpdateSelection(node);
        }

        private void GenerateToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            toolbar.style.height = 22;
            toolbar.style.borderBottomWidth = 1;
            toolbar.style.borderBottomColor = new Color(0.1f, 0.1f, 0.1f, 1f);

            // New Quest button
            var newButton = new Button(() => CreateNewQuest())
            {
                text = "New Quest"
            };
            newButton.style.width = 80;
            newButton.style.height = 20;
            newButton.style.marginLeft = 2;
            newButton.style.marginTop = 1;
            toolbar.Add(newButton);

            // Load Quest button
            var loadButton = new Button(() => LoadQuestDialog())
            {
                text = "Load Quest"
            };
            loadButton.style.width = 80;
            loadButton.style.height = 20;
            loadButton.style.marginLeft = 2;
            loadButton.style.marginTop = 1;
            toolbar.Add(loadButton);

            // Save button
            var saveButton = new Button(() => SaveQuest())
            {
                text = "Save"
            };
            saveButton.style.width = 60;
            saveButton.style.height = 20;
            saveButton.style.marginLeft = 2;
            saveButton.style.marginTop = 1;
            toolbar.Add(saveButton);

            // Separator
            var separator = new VisualElement();
            separator.style.width = 1;
            separator.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            separator.style.marginLeft = 4;
            separator.style.marginRight = 4;
            separator.style.height = 18;
            separator.style.marginTop = 2;
            toolbar.Add(separator);

            // Snap to Grid toggle
            var snapToggle = new Button(() => ToggleSnapToGrid())
            {
                text = "🧲 Snap: ON",
                name = "snap-toggle-button"
            };
            snapToggle.style.width = 90;
            snapToggle.style.height = 20;
            snapToggle.style.marginLeft = 2;
            snapToggle.style.marginTop = 1;
            snapToggle.style.backgroundColor = new Color(0.3f, 0.5f, 0.3f, 1f);
            toolbar.Add(snapToggle);

            // Spacer
            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);

            // Help button
            var helpButton = new Button(() => ShowHelp())
            {
                text = "?"
            };
            helpButton.style.width = 25;
            helpButton.style.height = 20;
            helpButton.style.marginLeft = 2;
            helpButton.style.marginTop = 1;
            helpButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            toolbar.Add(helpButton);

            // Quest name label
            var questNameLabel = new Label("No Quest Loaded")
            {
                name = "quest-name-label"
            };
            questNameLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            questNameLabel.style.paddingLeft = 10;
            questNameLabel.style.color = Color.white;
            questNameLabel.style.marginTop = 3;
            toolbar.Add(questNameLabel);

            rootVisualElement.Add(toolbar);
        }

        private void CreateNewQuest()
        {
            // Show folder selection dialog
            string defaultPath = "Assets";
            if (_currentQuest != null)
            {
                var currentPath = AssetDatabase.GetAssetPath(_currentQuest);
                defaultPath = System.IO.Path.GetDirectoryName(currentPath);
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Create New Quest",
                "NewQuest",
                "asset",
                "Choose where to save the new quest",
                defaultPath
            );

            if (string.IsNullOrEmpty(path))
                return;

            // Create new quest asset
            var quest = ScriptableObject.CreateInstance<QuestAsset>();
            
            // Use reflection to set private fields for new quest
            var questIdField = typeof(QuestAsset).GetField("questId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var displayNameField = typeof(QuestAsset).GetField("displayName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var descriptionField = typeof(QuestAsset).GetField("description",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            questIdField?.SetValue(quest, System.IO.Path.GetFileNameWithoutExtension(path));
            displayNameField?.SetValue(quest, "New Quest");
            descriptionField?.SetValue(quest, "Quest description");

            AssetDatabase.CreateAsset(quest, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            LoadQuest(quest);
            
            Debug.Log($"Created new quest at: {path}");
        }

        private void LoadQuestDialog()
        {
            string path = EditorUtility.OpenFilePanel(
                "Load Quest Asset",
                "Assets",
                "asset"
            );

            if (string.IsNullOrEmpty(path))
                return;

            // Convert absolute path to relative
            if (path.StartsWith(Application.dataPath))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
            }

            var quest = AssetDatabase.LoadAssetAtPath<QuestAsset>(path);
            if (quest != null)
            {
                LoadQuest(quest);
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Invalid Asset",
                    "The selected file is not a valid QuestAsset.",
                    "OK"
                );
            }
        }

        public void LoadQuest(QuestAsset quest)
        {
            _currentQuest = quest;
            _graphView.LoadQuest(quest);
            
            // Update toolbar label
            var label = rootVisualElement.Q<Label>("quest-name-label");
            if (label != null)
            {
                label.text = $"Quest: {quest.DisplayName} ({quest.QuestId})";
            }
            
            Debug.Log($"Loaded quest: {quest.DisplayName}");
        }

        private void SaveQuest()
        {
            if (_currentQuest == null)
            {
                EditorUtility.DisplayDialog(
                    "No Quest Loaded",
                    "Please create or load a quest first.",
                    "OK"
                );
                return;
            }

            _graphView.SaveQuest();
            EditorUtility.SetDirty(_currentQuest);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Saved quest: {_currentQuest.DisplayName}");
        }

        public string GetQuestAssetPath()
        {
            if (_currentQuest != null)
            {
                return System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(_currentQuest));
            }
            return "Assets";
        }

        private void ToggleSnapToGrid()
        {
            if (_graphView == null)
                return;

            bool newState = !_graphView.IsSnapToGridEnabled();
            _graphView.SetSnapToGrid(newState);

            // Update button appearance
            var snapButton = rootVisualElement.Q<Button>("snap-toggle-button");
            if (snapButton != null)
            {
                snapButton.text = newState ? "🧲 Snap: ON" : "🧲 Snap: OFF";
                snapButton.style.backgroundColor = newState 
                    ? new Color(0.3f, 0.5f, 0.3f, 1f) 
                    : new Color(0.3f, 0.3f, 0.3f, 1f);
            }

            Debug.Log($"Grid snapping: {(newState ? "Enabled" : "Disabled")}");
        }

        private void ShowHelp()
        {
            var message = @"Quest Graph Editor - Keyboard Shortcuts

Navigation:
  Mouse Wheel - Zoom in/out
  Middle Mouse Drag - Pan view
  Alt + Drag - Pan view
  F - Frame selection (or frame all if nothing selected)

Selection:
  Click - Select node
  Shift + Click - Add to selection
  Ctrl/Cmd + A - Select all nodes

Editing:
  Delete/Backspace - Delete selected nodes and connections
  Ctrl/Cmd + S - Save graph
  Ctrl/Cmd + D - Duplicate selected (coming in Phase 3)

Graph:
  Right Click - Context menu (add nodes)
  🧲 Snap Toggle - Enable/disable grid snapping (20px grid)

Inspector:
  Click any node to edit its properties in the right panel
  Changes are saved automatically with Undo support";

            EditorUtility.DisplayDialog("Quest Graph Editor Help", message, "OK");
        }
    }
}
