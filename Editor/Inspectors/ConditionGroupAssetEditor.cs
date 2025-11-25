using UnityEngine;
using UnityEditor;
using DynamicBox.Quest.Core;
using System.Linq;

namespace DynamicBox.Quest.Editor
{
    [CustomEditor(typeof(ConditionGroupAsset))]
    public class ConditionGroupAssetEditor : UnityEditor.Editor
    {
        private SerializedProperty _logicTypeProp;
        private SerializedProperty _conditionsProp;

        private void OnEnable()
        {
            _logicTypeProp = serializedObject.FindProperty("_logicType");
            _conditionsProp = serializedObject.FindProperty("_conditions");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Condition Group", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Logic Type
            EditorGUILayout.PropertyField(_logicTypeProp, new GUIContent("Logic Type", "AND = all conditions must be met, OR = any condition can be met"));
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Conditions", EditorStyles.miniLabel);

            // Show conditions with custom UI
            for (int i = 0; i < _conditionsProp.arraySize; i++)
            {
                DrawConditionElement(i);
            }

            // Add condition buttons
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Item Collection"))
            {
                AddCondition("ItemCollectedConditionAsset");
            }
            if (GUILayout.Button("Add Time Elapsed"))
            {
                AddCondition("TimeElapsedConditionAsset");
            }
            if (GUILayout.Button("Add Custom"))
            {
                ShowAddConditionMenu();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Validation
            ValidateConditionGroup();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawConditionElement(int index)
        {
            var conditionProp = _conditionsProp.GetArrayElementAtIndex(index);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            
            // Condition reference
            EditorGUILayout.PropertyField(conditionProp, new GUIContent($"Condition {index + 1}"), GUILayout.ExpandWidth(true));
            
            // Remove button
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                _conditionsProp.DeleteArrayElementAtIndex(index);
                return;
            }
            
            EditorGUILayout.EndHorizontal();

            // Show condition details if assigned
            if (conditionProp.objectReferenceValue != null)
            {
                var condition = conditionProp.objectReferenceValue as ConditionAsset;
                if (condition != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Type:", EditorStyles.miniLabel, GUILayout.Width(35));
                    EditorGUILayout.LabelField(condition.GetType().Name.Replace("ConditionAsset", ""), EditorStyles.miniLabel);
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Assign a condition asset", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private void AddCondition(string typeName)
        {
            string assetPath = AssetDatabase.GetAssetPath(target);
            string directory = System.IO.Path.GetDirectoryName(assetPath);
            string conditionPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/New{typeName}.asset");
            
            var conditionType = System.Type.GetType($"GenericQuest.Core.{typeName}, GenericQuest.Core");
            if (conditionType == null)
            {
                Debug.LogError($"Could not find condition type: {typeName}");
                return;
            }

            var condition = CreateInstance(conditionType) as ConditionAsset;
            condition.name = $"New {typeName.Replace("ConditionAsset", "")}";
            AssetDatabase.CreateAsset(condition, conditionPath);
            AssetDatabase.SaveAssets();
            
            // Add to condition group
            _conditionsProp.arraySize++;
            _conditionsProp.GetArrayElementAtIndex(_conditionsProp.arraySize - 1).objectReferenceValue = condition;
            serializedObject.ApplyModifiedProperties();
            
            EditorGUIUtility.PingObject(condition);
        }

        private void ShowAddConditionMenu()
        {
            var menu = new GenericMenu();
            
            // Find all condition types
            var conditionTypes = System.AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(ConditionAsset)) && !type.IsAbstract)
                .ToArray();

            foreach (var type in conditionTypes)
            {
                string typeName = type.Name;
                menu.AddItem(new GUIContent($"Custom/{typeName}"), false, () => AddCondition(typeName));
            }

            if (conditionTypes.Length == 0)
            {
                menu.AddDisabledItem(new GUIContent("No custom conditions found"));
            }

            menu.ShowAsContext();
        }

        private void ValidateConditionGroup()
        {
            var conditionGroup = target as ConditionGroupAsset;
            if (conditionGroup == null) return;

            if (conditionGroup.Conditions == null || conditionGroup.Conditions.Length == 0)
            {
                EditorGUILayout.HelpBox("Condition group needs at least one condition.", MessageType.Error);
                return;
            }

            for (int i = 0; i < conditionGroup.Conditions.Length; i++)
            {
                if (conditionGroup.Conditions[i] == null)
                {
                    EditorGUILayout.HelpBox($"Condition {i} is null and should be assigned.", MessageType.Error);
                }
            }
        }
    }
}
