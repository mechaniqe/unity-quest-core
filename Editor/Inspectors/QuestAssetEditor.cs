using UnityEngine;
using UnityEditor;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Editor
{
    [CustomEditor(typeof(QuestAsset))]
    public class QuestAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty _questIdProp;
        private SerializedProperty _displayNameProp;
        private SerializedProperty _descriptionProp;
        private SerializedProperty _categoryProp;
        private SerializedProperty _priorityProp;
        private SerializedProperty _objectivesProp;
        private SerializedProperty _prerequisitesProp;
        private SerializedProperty _autoStartProp;
        private SerializedProperty _allowConcurrentProp;
        private SerializedProperty _maxAttemptsProp;

        private void OnEnable()
        {
            _questIdProp = serializedObject.FindProperty("_questId");
            _displayNameProp = serializedObject.FindProperty("_displayName");
            _descriptionProp = serializedObject.FindProperty("_description");
            _categoryProp = serializedObject.FindProperty("_category");
            _priorityProp = serializedObject.FindProperty("_priority");
            _objectivesProp = serializedObject.FindProperty("_objectives");
            _prerequisitesProp = serializedObject.FindProperty("_prerequisites");
            _autoStartProp = serializedObject.FindProperty("_autoStart");
            _allowConcurrentProp = serializedObject.FindProperty("_allowConcurrent");
            _maxAttemptsProp = serializedObject.FindProperty("_maxAttempts");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Quest Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Basic Info
            EditorGUILayout.LabelField("Basic Information", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_questIdProp, new GUIContent("Quest ID"));
            EditorGUILayout.PropertyField(_displayNameProp, new GUIContent("Display Name"));
            
            EditorGUILayout.LabelField("Description");
            _descriptionProp.stringValue = EditorGUILayout.TextArea(_descriptionProp.stringValue, GUILayout.Height(60));
            
            EditorGUILayout.Space();

            // Configuration
            EditorGUILayout.LabelField("Configuration", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_categoryProp, new GUIContent("Category"));
            EditorGUILayout.PropertyField(_priorityProp, new GUIContent("Priority"));
            EditorGUILayout.PropertyField(_autoStartProp, new GUIContent("Auto Start", "Automatically start when prerequisites are met"));
            EditorGUILayout.PropertyField(_allowConcurrentProp, new GUIContent("Allow Concurrent", "Allow multiple instances of this quest"));
            EditorGUILayout.PropertyField(_maxAttemptsProp, new GUIContent("Max Attempts", "Maximum attempts allowed (0 = unlimited)"));
            
            EditorGUILayout.Space();

            // Prerequisites
            EditorGUILayout.LabelField("Prerequisites", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_prerequisitesProp, new GUIContent("Required Quests"), true);
            
            EditorGUILayout.Space();

            // Objectives
            EditorGUILayout.LabelField("Objectives", EditorStyles.miniLabel);
            if (_objectivesProp.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Quest needs at least one objective to be functional.", MessageType.Warning);
            }
            
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
    }
}
