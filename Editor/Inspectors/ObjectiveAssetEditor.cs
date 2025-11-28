using UnityEngine;
using UnityEditor;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Conditions;
using System.Linq;

namespace DynamicBox.Quest.Editor
{
    [CustomEditor(typeof(ObjectiveAsset))]
    public class ObjectiveAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty _objectiveIdProp;
        private SerializedProperty _titleProp;
        private SerializedProperty _descriptionProp;
        private SerializedProperty _isOptionalProp;
        private SerializedProperty _prerequisitesProp;
        private SerializedProperty _completionConditionProp;
        private SerializedProperty _failConditionProp;

        private void OnEnable()
        {
            _objectiveIdProp = serializedObject.FindProperty("objectiveId");
            _titleProp = serializedObject.FindProperty("title");
            _descriptionProp = serializedObject.FindProperty("description");
            _isOptionalProp = serializedObject.FindProperty("isOptional");
            _prerequisitesProp = serializedObject.FindProperty("prerequisites");
            _completionConditionProp = serializedObject.FindProperty("completionCondition");
            _failConditionProp = serializedObject.FindProperty("failCondition");
            
            // Validate that all properties were found
            if (_objectiveIdProp == null || _titleProp == null || _descriptionProp == null || 
                _isOptionalProp == null || _prerequisitesProp == null || 
                _completionConditionProp == null || _failConditionProp == null)
            {
                Debug.LogError($"ObjectiveAssetEditor: Could not find all required properties on ObjectiveAsset.");
            }
        }

        public override void OnInspectorGUI()
        {
            // Safety check - if properties are null, fall back to default inspector
            if (_objectiveIdProp == null || _titleProp == null || _descriptionProp == null || 
                _isOptionalProp == null || _prerequisitesProp == null || 
                _completionConditionProp == null || _failConditionProp == null)
            {
                EditorGUILayout.HelpBox("Custom inspector has issues. Using default inspector.", MessageType.Warning);
                base.OnInspectorGUI();
                return;
            }
            
            serializedObject.Update();

            EditorGUILayout.LabelField("Objective Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Basic Info
            EditorGUILayout.LabelField("Basic Information", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_objectiveIdProp, new GUIContent("Objective ID"));
            EditorGUILayout.PropertyField(_titleProp, new GUIContent("Title"));
            
            EditorGUILayout.LabelField("Description");
            _descriptionProp.stringValue = EditorGUILayout.TextArea(_descriptionProp.stringValue, GUILayout.Height(40));
            
            EditorGUILayout.Space();

            // Configuration
            EditorGUILayout.LabelField("Configuration", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_isOptionalProp, new GUIContent("Is Optional", "Optional objectives don't block quest completion"));
            
            EditorGUILayout.Space();

            // Prerequisites
            EditorGUILayout.LabelField("Prerequisites", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_prerequisitesProp, new GUIContent("Required Objectives"), true);
            
            EditorGUILayout.Space();

            // Completion Conditions
            EditorGUILayout.LabelField("Completion Conditions", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_completionConditionProp, new GUIContent("Completion Condition"));

            if (GUILayout.Button("Create New Condition"))
            {
                ShowConditionCreationMenu(false);
            }

            EditorGUILayout.Space();
            
            // Fail Conditions
            EditorGUILayout.LabelField("Failure Conditions (Optional)", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_failConditionProp, new GUIContent("Fail Condition"));
            
            if (GUILayout.Button("Create New Fail Condition"))
            {
                ShowConditionCreationMenu(true);
            }

            EditorGUILayout.Space();

            // Validation
            ValidateObjective();

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowConditionCreationMenu(bool isFailCondition)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Item Collected Condition"), false, () => CreateConditionByMenuName("DynamicBox/Quest/Conditions/Item Collected", isFailCondition));
            menu.AddItem(new GUIContent("Area Entered Condition"), false, () => CreateConditionByMenuName("DynamicBox/Quest/Conditions/Area Entered Condition", isFailCondition));
            menu.AddItem(new GUIContent("Custom Flag Condition"), false, () => CreateConditionByMenuName("DynamicBox/Quest/Conditions/Custom Flag Condition", isFailCondition));
            menu.AddItem(new GUIContent("Time Elapsed Condition"), false, () => CreateConditionByMenuName("DynamicBox/Quest/Conditions/Time Elapsed Condition", isFailCondition));
            menu.AddItem(new GUIContent("Condition Group"), false, () => CreateCondition<ConditionGroupAsset>(isFailCondition));
            menu.ShowAsContext();
        }

        private void CreateConditionByMenuName(string menuName, bool isFailCondition)
        {
            // Use Unity's menu system to create the asset
            string assetPath = AssetDatabase.GetAssetPath(target);
            string directory = System.IO.Path.GetDirectoryName(assetPath);
            
            // Create the asset using reflection to avoid direct type references
            var conditionTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(ConditionAsset).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();
                
            System.Type targetType = null;
            if (menuName.Contains("Item Collected"))
                targetType = conditionTypes.FirstOrDefault(t => t.Name == "ItemCollectedConditionAsset");
            else if (menuName.Contains("Area Entered"))
                targetType = conditionTypes.FirstOrDefault(t => t.Name == "AreaEnteredConditionAsset");
            else if (menuName.Contains("Custom Flag"))
                targetType = conditionTypes.FirstOrDefault(t => t.Name == "CustomFlagConditionAsset");
            else if (menuName.Contains("Time Elapsed"))
                targetType = conditionTypes.FirstOrDefault(t => t.Name == "TimeElapsedConditionAsset");
                
            if (targetType != null)
            {
                var condition = ScriptableObject.CreateInstance(targetType) as ConditionAsset;
                if (condition != null)
                {
                    string conditionName = isFailCondition ? "NewFailCondition" : "NewCondition";
                    string conditionPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{conditionName}.asset");
                    
                    condition.name = conditionName;
                    AssetDatabase.CreateAsset(condition, conditionPath);
                    AssetDatabase.SaveAssets();
                    
                    // Assign to objective
                    if (isFailCondition)
                    {
                        _failConditionProp.objectReferenceValue = condition;
                    }
                    else
                    {
                        _completionConditionProp.objectReferenceValue = condition;
                    }
                    serializedObject.ApplyModifiedProperties();
                    
                    EditorGUIUtility.PingObject(condition);
                }
            }
            else
            {
                Debug.LogWarning($"Could not find condition type for menu: {menuName}");
            }
        }

        private void CreateCondition<T>(bool isFailCondition) where T : ConditionAsset
        {
            string assetPath = AssetDatabase.GetAssetPath(target);
            string directory = System.IO.Path.GetDirectoryName(assetPath);
            string conditionName = isFailCondition ? "NewFailCondition" : "NewCondition";
            string conditionPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{conditionName}.asset");
            
            var condition = CreateInstance<T>();
            condition.name = conditionName;
            AssetDatabase.CreateAsset(condition, conditionPath);
            AssetDatabase.SaveAssets();
            
            // Assign to objective
            if (isFailCondition)
            {
                _failConditionProp.objectReferenceValue = condition;
            }
            else
            {
                _completionConditionProp.objectReferenceValue = condition;
            }
            serializedObject.ApplyModifiedProperties();
            
            EditorGUIUtility.PingObject(condition);
        }

        private void ValidateObjective()
        {
            var objective = target as ObjectiveAsset;
            if (objective == null) return;

            if (string.IsNullOrEmpty(objective.ObjectiveId))
            {
                EditorGUILayout.HelpBox("Objective ID is required and should be unique.", MessageType.Error);
            }

            if (string.IsNullOrEmpty(objective.Title))
            {
                EditorGUILayout.HelpBox("Title is required for UI purposes.", MessageType.Warning);
            }

            if (objective.CompletionCondition == null)
            {
                EditorGUILayout.HelpBox("Completion Condition is required to define completion criteria.", MessageType.Error);
            }
        }
    }
}
