using UnityEngine;
using UnityEditor;
using GenericQuest.Core;

namespace GenericQuest.Editor
{
    [CustomEditor(typeof(ObjectiveAsset))]
    public class ObjectiveAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty _objectiveIdProp;
        private SerializedProperty _displayNameProp;
        private SerializedProperty _descriptionProp;
        private SerializedProperty _isOptionalProp;
        private SerializedProperty _hideWhenCompleteProp;
        private SerializedProperty _conditionGroupProp;
        private SerializedProperty _prerequisitesProp;

        private void OnEnable()
        {
            _objectiveIdProp = serializedObject.FindProperty("_objectiveId");
            _displayNameProp = serializedObject.FindProperty("_displayName");
            _descriptionProp = serializedObject.FindProperty("_description");
            _isOptionalProp = serializedObject.FindProperty("_isOptional");
            _hideWhenCompleteProp = serializedObject.FindProperty("_hideWhenComplete");
            _conditionGroupProp = serializedObject.FindProperty("_conditionGroup");
            _prerequisitesProp = serializedObject.FindProperty("_prerequisites");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Objective Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Basic Info
            EditorGUILayout.LabelField("Basic Information", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_objectiveIdProp, new GUIContent("Objective ID"));
            EditorGUILayout.PropertyField(_displayNameProp, new GUIContent("Display Name"));
            
            EditorGUILayout.LabelField("Description");
            _descriptionProp.stringValue = EditorGUILayout.TextArea(_descriptionProp.stringValue, GUILayout.Height(40));
            
            EditorGUILayout.Space();

            // Configuration
            EditorGUILayout.LabelField("Configuration", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_isOptionalProp, new GUIContent("Is Optional", "Optional objectives don't block quest completion"));
            EditorGUILayout.PropertyField(_hideWhenCompleteProp, new GUIContent("Hide When Complete", "Hide from UI when completed"));
            
            EditorGUILayout.Space();

            // Prerequisites
            EditorGUILayout.LabelField("Prerequisites", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_prerequisitesProp, new GUIContent("Required Objectives"), true);
            
            EditorGUILayout.Space();

            // Condition Group
            EditorGUILayout.LabelField("Completion Conditions", EditorStyles.miniLabel);
            if (_conditionGroupProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Objective needs a ConditionGroup to define when it's complete.", MessageType.Warning);
            }
            
            EditorGUILayout.PropertyField(_conditionGroupProp, new GUIContent("Condition Group"));

            if (GUILayout.Button("Create New Condition Group"))
            {
                CreateNewConditionGroup();
            }

            EditorGUILayout.Space();

            // Validation
            ValidateObjective();

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateNewConditionGroup()
        {
            string assetPath = AssetDatabase.GetAssetPath(target);
            string directory = System.IO.Path.GetDirectoryName(assetPath);
            string conditionPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/NewConditionGroup.asset");
            
            var conditionGroup = CreateInstance<ConditionGroupAsset>();
            conditionGroup.name = "New Condition Group";
            AssetDatabase.CreateAsset(conditionGroup, conditionPath);
            AssetDatabase.SaveAssets();
            
            // Assign to objective
            _conditionGroupProp.objectReferenceValue = conditionGroup;
            serializedObject.ApplyModifiedProperties();
            
            EditorGUIUtility.PingObject(conditionGroup);
        }

        private void ValidateObjective()
        {
            var objective = target as ObjectiveAsset;
            if (objective == null) return;

            if (string.IsNullOrEmpty(objective.ObjectiveId))
            {
                EditorGUILayout.HelpBox("Objective ID is required and should be unique.", MessageType.Error);
            }

            if (string.IsNullOrEmpty(objective.DisplayName))
            {
                EditorGUILayout.HelpBox("Display Name is required for UI purposes.", MessageType.Warning);
            }

            if (objective.ConditionGroup == null)
            {
                EditorGUILayout.HelpBox("ConditionGroup is required to define completion criteria.", MessageType.Error);
            }
        }
    }
}
