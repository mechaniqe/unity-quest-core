using UnityEngine;
using UnityEditor;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Editor
{
    /// <summary>
    /// Menu items for testing quest creation after fixing the editor inspectors
    /// </summary>
    public static class QuestCreationTest
    {
        [MenuItem("Tools/DynamicBox/Quest System/Test Quest Creation")]
        public static void TestQuestCreation()
        {
            // Create a simple test quest
            var quest = ScriptableObject.CreateInstance<QuestAsset>();
            
            // Create the asset
            string assetPath = "Assets/TestQuest.asset";
            AssetDatabase.CreateAsset(quest, assetPath);
            AssetDatabase.SaveAssets();
            
            // Ping it in the project window
            EditorGUIUtility.PingObject(quest);
            
            Debug.Log("Test quest created successfully at: " + assetPath);
        }

        [MenuItem("Tools/DynamicBox/Quest System/Test Objective Creation")]
        public static void TestObjectiveCreation()
        {
            // Create a simple test objective
            var objective = ScriptableObject.CreateInstance<ObjectiveAsset>();
            
            // Create the asset
            string assetPath = "Assets/TestObjective.asset";
            AssetDatabase.CreateAsset(objective, assetPath);
            AssetDatabase.SaveAssets();
            
            // Ping it in the project window
            EditorGUIUtility.PingObject(objective);
            
            Debug.Log("Test objective created successfully at: " + assetPath);
        }

        [MenuItem("Tools/DynamicBox/Quest System/Test Condition Group Creation")]
        public static void TestConditionGroupCreation()
        {
            // Create a simple test condition group
            var conditionGroup = ScriptableObject.CreateInstance<ConditionGroupAsset>();
            
            // Create the asset
            string assetPath = "Assets/TestConditionGroup.asset";
            AssetDatabase.CreateAsset(conditionGroup, assetPath);
            AssetDatabase.SaveAssets();
            
            // Ping it in the project window
            EditorGUIUtility.PingObject(conditionGroup);
            
            Debug.Log("Test condition group created successfully at: " + assetPath);
        }
    }
}
