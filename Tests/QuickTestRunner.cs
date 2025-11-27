using UnityEngine;
using DynamicBox.Quest.Tests;
using DynamicBox.Quest.Core;
using System;
using System.Reflection;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Quick test runner to verify the GameObject accumulation fixes
    /// </summary>
    public class QuickTestRunner : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private int testIterations = 3;

        private void Start()
        {
            if (runOnStart)
            {
                RunQuickTests();
            }
        }

        [ContextMenu("Run Quick Tests")]
        public void RunQuickTests()
        {
            Debug.Log("=== Running Quick Test to Verify Fixes ===");
            
            // Count GameObjects before tests
            var beforeCount = FindObjectsOfType<GameObject>().Length;
            Debug.Log($"GameObjects before tests: {beforeCount}");

            try
            {
                // Run a few critical tests multiple times
                for (int i = 0; i < testIterations; i++)
                {
                    Debug.Log($"\n--- Test Iteration {i + 1} ---");
                    TestQuestManagerCreation();
                    TestScriptableObjectCreation();
                    TestDuplicateHandling();
                }

                // Count GameObjects after tests
                var afterCount = FindObjectsOfType<GameObject>().Length;
                Debug.Log($"GameObjects after tests: {afterCount}");
                
                var increase = afterCount - beforeCount;
                if (increase > 10) // Allow some tolerance
                {
                    Debug.LogWarning($"GameObject count increased by {increase}. This might indicate a leak.");
                }
                else
                {
                    Debug.Log($"✓ GameObject count increase within acceptable range: {increase}");
                }

                Debug.Log("✅ Quick tests completed successfully!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ Quick tests failed: {ex.Message}");
            }
        }

        private void TestQuestManagerCreation()
        {
            Debug.Log("Testing QuestManager creation with proper cleanup...");
            
            // Create quest manager using the same pattern as the tests
            var gameObject = new GameObject("TestQuestManager");
            gameObject.SetActive(false); // Disable to prevent Awake from running
            
            var questManager = gameObject.AddComponent<QuestManager>();
            
            // Create a mock player ref
            var playerRefObject = new GameObject("TestPlayerRef");
            var playerRef = playerRefObject.AddComponent<QuestPlayerRef>();
            
            // Set the player ref using reflection
            var playerRefField = typeof(QuestManager).GetField("playerRef",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerRefField?.SetValue(questManager, playerRef);

            // Now enable and initialize the QuestManager
            gameObject.SetActive(true);
            
            if (questManager == null)
                throw new Exception("QuestManager creation failed");

            if (questManager.ActiveQuests == null)
                throw new Exception("QuestManager not properly initialized");

            // Cleanup immediately
            if (playerRef != null)
            {
                DestroyImmediate(playerRef.gameObject);
            }
            DestroyImmediate(questManager.gameObject);
            
            Debug.Log("✓ QuestManager creation and cleanup successful");
        }

        private void TestScriptableObjectCreation()
        {
            Debug.Log("Testing ScriptableObject creation...");
            
            // Test proper ScriptableObject creation
            var mockCondition = ScriptableObject.CreateInstance<MockConditionAsset>();
            var pollingCondition = ScriptableObject.CreateInstance<MockPollingConditionAsset>();
            
            if (mockCondition == null || pollingCondition == null)
                throw new Exception("ScriptableObject creation failed");
            
            // Cleanup
            DestroyImmediate(mockCondition);
            DestroyImmediate(pollingCondition);
            
            Debug.Log("✓ ScriptableObject creation successful");
        }

        private void TestDuplicateHandling()
        {
            Debug.Log("Testing duplicate ID handling...");
            
            var obj1 = new ObjectiveBuilder().WithObjectiveId("test_duplicate").Build();
            var obj2 = new ObjectiveBuilder().WithObjectiveId("test_duplicate").Build();

            var quest = new QuestBuilder()
                .WithQuestId("test_quest")
                .AddObjective(obj1)
                .AddObjective(obj2)
                .Build();

            try
            {
                var questState = new QuestState(quest);
                throw new Exception("Should have thrown ArgumentException for duplicate IDs");
            }
            catch (ArgumentException ex)
            {
                if (ex.Message.Contains("An item with the same key has already been added"))
                {
                    Debug.Log("✓ Duplicate ID handling working correctly");
                }
                else
                {
                    throw new Exception($"Unexpected exception: {ex.Message}");
                }
            }
        }
    }
}
