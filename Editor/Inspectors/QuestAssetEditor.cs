using DynamicBox.Quest.Core;
using UnityEditor;
using UnityEngine;

namespace DynamicBox.Quest.Editor
{
    [CustomEditor(typeof(QuestAsset))]
    public class QuestAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty _questIdProp;
        private SerializedProperty _displayNameProp;
        private SerializedProperty _descriptionProp;
        private SerializedProperty _objectivesProp;

        #region Unity Methods

        void OnEnable()
        {
            _questIdProp = serializedObject.FindProperty("questId");
            _displayNameProp = serializedObject.FindProperty("displayName");
            _descriptionProp = serializedObject.FindProperty("description");
            _objectivesProp = serializedObject.FindProperty("objectives");
            
            // Validate that all properties were found
            if (_questIdProp == null || _displayNameProp == null || _descriptionProp == null || _objectivesProp == null)
            {
                Debug.LogError($"QuestAssetEditor: Could not find all required properties on QuestAsset. " +
                              $"questId: {_questIdProp != null}, displayName: {_displayNameProp != null}, " +
                              $"description: {_descriptionProp != null}, objectives: {_objectivesProp != null}");
            }
        }

        #endregion

        public override void OnInspectorGUI()
        {
            // Safety check - if properties are null, fall back to default inspector
            if (_questIdProp == null || _displayNameProp == null || _descriptionProp == null || _objectivesProp == null)
            {
                EditorGUILayout.HelpBox("Custom inspector has issues. Using default inspector.", MessageType.Warning);
                base.OnInspectorGUI();
                return;
            }
            
            serializedObject.Update();

            EditorGUILayout.LabelField("Quest Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Visual Graph Editor Button
            if (GUILayout.Button("Open in Quest Graph Editor", GUILayout.Height(30)))
            {
                OpenInGraphEditor();
            }
            
            EditorGUILayout.Space();

            // Basic Info
            EditorGUILayout.LabelField("Basic Information", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_questIdProp, new GUIContent("Quest ID"));
            EditorGUILayout.PropertyField(_displayNameProp, new GUIContent("Display Name"));
            
            EditorGUILayout.LabelField("Description");
            _descriptionProp.stringValue = EditorGUILayout.TextArea(_descriptionProp.stringValue, GUILayout.Height(60));
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(_objectivesProp, new GUIContent("Objectives"), true);

            if (GUILayout.Button("Add New Objective"))
            {
                ShowObjectiveCreationMenu();
            }

            EditorGUILayout.Space();

            // Validation
            ValidateQuest();

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowObjectiveCreationMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Create New Objective Asset"), false, CreateNewObjective);
            menu.ShowAsContext();
        }

        private void CreateNewObjective()
        {
            string assetPath = AssetDatabase.GetAssetPath(target);
            string directory = System.IO.Path.GetDirectoryName(assetPath);
            string objectivePath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/NewObjective.asset");
            
            var objective = CreateInstance<ObjectiveAsset>();
            objective.name = "New Objective";
            AssetDatabase.CreateAsset(objective, objectivePath);
            AssetDatabase.SaveAssets();
            
            // Add to quest
            _objectivesProp.arraySize++;
            _objectivesProp.GetArrayElementAtIndex(_objectivesProp.arraySize - 1).objectReferenceValue = objective;
            serializedObject.ApplyModifiedProperties();
            
            EditorGUIUtility.PingObject(objective);
        }

        private void ValidateQuest()
        {
            var quest = target as QuestAsset;
            if (quest == null) return;

            if (string.IsNullOrEmpty(quest.QuestId))
            {
                EditorGUILayout.HelpBox("Quest ID is required and should be unique.", MessageType.Error);
            }

            if (string.IsNullOrEmpty(quest.DisplayName))
            {
                EditorGUILayout.HelpBox("Display Name is required for UI purposes.", MessageType.Warning);
            }

            if (quest.Objectives == null || quest.Objectives.Count == 0)
            {
                EditorGUILayout.HelpBox("Quest requires at least one objective.", MessageType.Error);
            }
            else
            {
                for (int i = 0; i < quest.Objectives.Count; i++)
                {
                    if (quest.Objectives[i] == null)
                    {
                        EditorGUILayout.HelpBox($"Objective {i} is null and should be assigned.", MessageType.Error);
                    }
                }
            }
        }

        private void OpenInGraphEditor()
        {
            var quest = target as QuestAsset;
            if (quest == null) return;

            // Get the graph editor window type
            var graphEditorType = System.Type.GetType("DynamicBox.Quest.Editor.GraphEditor.QuestGraphEditorWindow, DynamicBox.Quest.Editor");
            if (graphEditorType == null)
            {
                Debug.LogError("Could not find QuestGraphEditorWindow type. Make sure the Graph Editor is in the project.");
                return;
            }

            // Open the window
            var window = EditorWindow.GetWindow(graphEditorType);
            window.titleContent = new GUIContent("Quest Graph Editor");
            window.minSize = new Vector2(1000, 600);

            // Load the quest using reflection
            var loadQuestMethod = graphEditorType.GetMethod("LoadQuest", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            
            if (loadQuestMethod != null)
            {
                loadQuestMethod.Invoke(window, new object[] { quest });
            }
            else
            {
                Debug.LogError("Could not find LoadQuest method on QuestGraphEditorWindow.");
            }

            window.Show();
            window.Focus();
        }
    }
}
