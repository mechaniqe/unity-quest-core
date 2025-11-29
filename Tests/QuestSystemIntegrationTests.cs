using System;
using System.Collections;
using UnityEngine;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Conditions;
using DynamicBox.Quest.GameEvents;
using DynamicBox.EventManagement;
using System.Linq;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Integration tests for the quest system that test real Unity component interactions.
    /// These tests require Unity runtime and test actual MonoBehaviour functionality.
    /// </summary>
    public class QuestSystemIntegrationTests : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private float testDelay = 0.1f;

        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllIntegrationTests());
            }
        }

        public IEnumerator RunAllIntegrationTests()
        {
            Debug.Log("=== Starting Quest System Integration Tests ===");
            
            // Clean up any existing test objects before starting
            CleanupExistingTestObjects();
            
            yield return StartCoroutine(TestQuestManagerLifecycle());
            yield return StartCoroutine(TestQuestManagerPollingSystem());
            yield return StartCoroutine(TestQuestManagerEventProcessing());
            yield return StartCoroutine(TestMultipleQuestsSimultaneously());
            yield return StartCoroutine(TestQuestCompletionFlow());
            yield return StartCoroutine(TestQuestFailureFlow());
            yield return StartCoroutine(TestComplexQuestScenario());
            yield return StartCoroutine(TestMemoryManagement());
            yield return StartCoroutine(TestPerformanceUnderLoad());

            Debug.Log("=== All Quest System Integration Tests Completed ===");
        }

        private void CleanupExistingTestObjects()
        {
            // Find and destroy any existing test GameObjects
            var testQuestManagers = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.name.StartsWith("TestQuestManager"))
                .ToArray();
            
            var testPlayerRefs = FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .Where(go => go.name.StartsWith("TestPlayerRef"))
                .ToArray();

            foreach (var obj in testQuestManagers)
            {
                DestroyImmediate(obj);
            }

            foreach (var obj in testPlayerRefs)
            {
                DestroyImmediate(obj);
            }

            if (testQuestManagers.Length > 0 || testPlayerRefs.Length > 0)
            {
                Debug.Log($"Cleaned up {testQuestManagers.Length} test quest managers and {testPlayerRefs.Length} test player refs");
            }
        }

        private IEnumerator TestQuestManagerLifecycle()
        {
            Debug.Log("\n[INTEGRATION TEST] QuestManager Lifecycle");

            // Create QuestManager instance
            var questManagerGO = new GameObject("TestQuestManager");
            var questManager = questManagerGO.AddComponent<QuestManager>();
            
            var playerRefGO = new GameObject("TestPlayerRef");
            var playerRef = playerRefGO.AddComponent<QuestPlayerRef>();

            // Set player ref using reflection
            var playerRefField = typeof(QuestManager).GetField("playerRef",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerRefField?.SetValue(questManager, playerRef);

            // Wait for Awake/Start to be called
            yield return null;

            if (questManager.ActiveQuests.Count != 0)
                throw new Exception("QuestManager should start with no active quests");

            // Test adding a quest
            var quest = CreateTestQuest("lifecycle_quest");
            var questState = questManager.StartQuest(quest);

            if (questManager.ActiveQuests.Count != 1)
                throw new Exception("QuestManager should have 1 active quest after starting");

            // Test removing quest
            questManager.StopQuest(questState);

            if (questManager.ActiveQuests.Count != 0)
                throw new Exception("QuestManager should have 0 active quests after stopping");

            // Cleanup
            DestroyImmediate(questManagerGO);
            DestroyImmediate(playerRefGO);

            Debug.Log("✓ QuestManager lifecycle test passed");
            yield return new WaitForSeconds(testDelay);
        }

        private IEnumerator TestQuestManagerPollingSystem()
        {
            Debug.Log("\n[INTEGRATION TEST] QuestManager Polling System");

            var questManager = CreateQuestManager();
            try
            {
                // Enable polling with short interval for testing
                var enablePollingField = typeof(QuestManager).GetField("enablePolling",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var pollingIntervalField = typeof(QuestManager).GetField("pollingInterval",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                enablePollingField?.SetValue(questManager, true);
                pollingIntervalField?.SetValue(questManager, 0.1f); // Poll every 100ms for testing

                // Create quest with polling condition
                var pollingCondition = ScriptableObject.CreateInstance<TimeElapsedConditionAsset>();
                var requiredSecondsField = typeof(TimeElapsedConditionAsset).GetField("requiredSeconds",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                requiredSecondsField?.SetValue(pollingCondition, 0.2f);

                var objective = new ObjectiveBuilder()
                    .WithObjectiveId("timed_objective")
                    .WithCompletionCondition(pollingCondition)
                    .Build();

                var quest = new QuestBuilder()
                    .WithQuestId("timed_quest")
                    .AddObjective(objective)
                    .Build();

                bool questCompleted = false;
                questManager.OnQuestCompleted += (q) => questCompleted = true;

                questManager.StartQuest(quest);

                // Wait for polling to complete the quest
                float timeout = 1.0f;
                while (!questCompleted && timeout > 0)
                {
                    timeout -= Time.deltaTime;
                    yield return null;
                }

                if (!questCompleted)
                    throw new Exception("Quest should have completed via polling system");

                Debug.Log("✓ QuestManager polling system test passed");
                yield return new WaitForSeconds(testDelay);
            }
            finally
            {
                CleanupQuestManager(questManager);
            }
        }

        private IEnumerator TestQuestManagerEventProcessing()
        {
            Debug.Log("\n[INTEGRATION TEST] QuestManager Event Processing");

            var questManager = CreateQuestManager();
            try
            {
                var eventManager = EventManager.Instance;

                // Create quest with event-driven condition
                var itemCondition = ScriptableObject.CreateInstance<ItemCollectedConditionAsset>();
                var itemIdField = typeof(ItemCollectedConditionAsset).GetField("itemId",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var requiredCountField = typeof(ItemCollectedConditionAsset).GetField("requiredCount",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                itemIdField?.SetValue(itemCondition, "integration_test_item");
                requiredCountField?.SetValue(itemCondition, 1);

                var objective = new ObjectiveBuilder()
                    .WithObjectiveId("collect_objective")
                    .WithCompletionCondition(itemCondition)
                    .Build();

                var quest = new QuestBuilder()
                    .WithQuestId("event_quest")
                    .AddObjective(objective)
                    .Build();

                bool questCompleted = false;
                questManager.OnQuestCompleted += (q) => questCompleted = true;

                questManager.StartQuest(quest);

                // Publish event to complete quest
                eventManager.Raise(new ItemCollectedEvent("integration_test_item", 1));

                // Wait a frame for event processing
                yield return null;

                if (!questCompleted)
                    throw new Exception("Quest should have completed via event processing");

                Debug.Log("✓ QuestManager event processing test passed");
                yield return new WaitForSeconds(testDelay);
            }
            finally
            {
                CleanupQuestManager(questManager);
            }
        }

        private IEnumerator TestMultipleQuestsSimultaneously()
        {
            Debug.Log("\n[INTEGRATION TEST] Multiple Quests Simultaneously");

            var questManager = CreateQuestManager();
            try
            {
                var eventManager = EventManager.Instance;

                // Create multiple quests with different requirements
                var quest1 = CreateTestQuestWithItemCondition("quest1", "item1");
                var quest2 = CreateTestQuestWithItemCondition("quest2", "item2");
                var quest3 = CreateTestQuestWithItemCondition("quest3", "item3");

                int completedCount = 0;
                questManager.OnQuestCompleted += (q) => completedCount++;

                // Start all quests
                questManager.StartQuest(quest1);
                questManager.StartQuest(quest2);
                questManager.StartQuest(quest3);

                if (questManager.ActiveQuests.Count != 3)
                    throw new Exception("QuestManager should have 3 active quests");

                // Complete quests in different order
                eventManager.Raise(new ItemCollectedEvent("item2", 1)); // Complete quest2
                yield return null;

                eventManager.Raise(new ItemCollectedEvent("item1", 1)); // Complete quest1
                yield return null;

                eventManager.Raise(new ItemCollectedEvent("item3", 1)); // Complete quest3
                yield return null;

                if (completedCount != 3)
                    throw new Exception($"Expected 3 completed quests, got {completedCount}");

                if (questManager.ActiveQuests.Count != 0)
                    throw new Exception("All quests should be removed after completion");

                Debug.Log("✓ Multiple quests simultaneously test passed");
                yield return new WaitForSeconds(testDelay);
            }
            finally
            {
                CleanupQuestManager(questManager);
            }
        }

        private IEnumerator TestQuestCompletionFlow()
        {
            Debug.Log("\n[INTEGRATION TEST] Quest Completion Flow");

            var questManager = CreateQuestManager();
            try
            {
                var eventManager = EventManager.Instance;

                // Create quest with prerequisites
                var obj1 = new ObjectiveBuilder()
                    .WithObjectiveId("first_obj")
                    .WithCompletionCondition(CreateItemCondition("key"))
                    .Build();

                var obj2 = new ObjectiveBuilder()
                    .WithObjectiveId("second_obj")
                    .WithCompletionCondition(CreateItemCondition("treasure"))
                    .AddPrerequisite(obj1)
                    .Build();

                var quest = new QuestBuilder()
                    .WithQuestId("completion_quest")
                    .AddObjective(obj1)
                    .AddObjective(obj2)
                    .Build();

                bool questCompleted = false;
                int objectiveStatusChanges = 0;

                questManager.OnQuestCompleted += (q) => questCompleted = true;
                questManager.OnObjectiveStatusChanged += (obj) => objectiveStatusChanges++;

                var questState = questManager.StartQuest(quest);

                // Try to complete second objective first (should not work due to prerequisites)
                eventManager.Raise(new ItemCollectedEvent("treasure", 1));
                yield return null;

                if (questCompleted)
                    throw new Exception("Quest should not complete without prerequisites");

                // Complete first objective
                eventManager.Raise(new ItemCollectedEvent("key", 1));
                yield return null;

                // Now complete second objective
                eventManager.Raise(new ItemCollectedEvent("treasure", 1));
                yield return null;

                if (!questCompleted)
                    throw new Exception("Quest should complete after all objectives are met");

                if (objectiveStatusChanges < 2)
                    throw new Exception("Expected at least 2 objective status changes");

                Debug.Log("✓ Quest completion flow test passed");
                yield return new WaitForSeconds(testDelay);
            }
            finally
            {
                CleanupQuestManager(questManager);
            }
        }

        private IEnumerator TestQuestFailureFlow()
        {
            Debug.Log("\n[INTEGRATION TEST] Quest Failure Flow");

            var questManager = CreateQuestManager();
            try
            {
                // Create quest with fail condition
                var completionCondition = ScriptableObject.CreateInstance<MockConditionAsset>();
                var failCondition = ScriptableObject.CreateInstance<MockConditionAsset>();

                var objective = new ObjectiveBuilder()
                    .WithObjectiveId("risky_obj")
                    .WithCompletionCondition(completionCondition)
                    .WithFailCondition(failCondition)
                    .Build();

                var quest = new QuestBuilder()
                    .WithQuestId("failure_quest")
                    .AddObjective(objective)
                    .Build();

                bool questFailed = false;
                questManager.OnQuestFailed += (q) => questFailed = true;

                var questState = questManager.StartQuest(quest);

                // Trigger failure
                var failInstance = questState.Objectives["risky_obj"].GetFailInstance() as MockConditionInstance;
                failInstance?.SetMet(true);

                yield return null;

                if (!questFailed)
                    throw new Exception("Quest should fail when fail condition is triggered");

                if (questState.Status != QuestStatus.Failed)
                    throw new Exception("Quest status should be Failed");

                Debug.Log("✓ Quest failure flow test passed");
                yield return new WaitForSeconds(testDelay);
            }
            finally
            {
                CleanupQuestManager(questManager);
            }
        }

        private IEnumerator TestComplexQuestScenario()
        {
            Debug.Log("\n[INTEGRATION TEST] Complex Quest Scenario");

            var questManager = CreateQuestManager();
            try
            {
                var eventManager = EventManager.Instance;

                // Create a complex quest with multiple condition types
                var itemCondition = CreateItemCondition("artifact");
                var areaCondition = CreateAreaCondition("ancient_temple");
                var flagCondition = CreateFlagCondition("ritual_completed", true);

                // Create AND group with all conditions
                var conditionGroup = ScriptableObject.CreateInstance<ConditionGroupAsset>();
                var operatorField = typeof(ConditionGroupAsset).GetField("@operator",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var childrenField = typeof(ConditionGroupAsset).GetField("children",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                operatorField?.SetValue(conditionGroup, ConditionOperator.And);
                var children = new System.Collections.Generic.List<ConditionAsset> 
                { 
                    itemCondition, areaCondition, flagCondition 
                };
                childrenField?.SetValue(conditionGroup, children);

                var objective = new ObjectiveBuilder()
                    .WithObjectiveId("complex_obj")
                    .WithCompletionCondition(conditionGroup)
                    .Build();

                var quest = new QuestBuilder()
                    .WithQuestId("complex_quest")
                    .AddObjective(objective)
                    .Build();

                bool questCompleted = false;
                questManager.OnQuestCompleted += (q) => questCompleted = true;

                questManager.StartQuest(quest);

                // Complete conditions in sequence
                eventManager.Raise(new ItemCollectedEvent("artifact", 1));
                yield return null;

                if (questCompleted)
                    throw new Exception("Quest should not complete with only 1/3 conditions");

                eventManager.Raise(new AreaEnteredEvent("ancient_temple"));
                yield return null;

                if (questCompleted)
                    throw new Exception("Quest should not complete with only 2/3 conditions");

                eventManager.Raise(new FlagChangedEvent("ritual_completed", true));
                yield return null;

                if (!questCompleted)
                    throw new Exception("Quest should complete when all AND conditions are met");

                Debug.Log("✓ Complex quest scenario test passed");
                yield return new WaitForSeconds(testDelay);
            }
            finally
            {
                CleanupQuestManager(questManager);
            }
        }

        private IEnumerator TestMemoryManagement()
        {
            Debug.Log("\n[INTEGRATION TEST] Memory Management");

            var questManager = CreateQuestManager();
            try
            {
                var initialMemory = GC.GetTotalMemory(false);

                // Create and complete many quests to test memory management
                for (int i = 0; i < 10; i++)
                {
                    var quest = CreateTestQuestWithItemCondition($"memory_quest_{i}", $"item_{i}");
                    var questState = questManager.StartQuest(quest);
                    
                    // Complete quest immediately
                    EventManager.Instance.Raise(new ItemCollectedEvent($"item_{i}", 1));
                    yield return null;
                }

                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var finalMemory = GC.GetTotalMemory(false);
                var memoryIncrease = finalMemory - initialMemory;

                // Memory increase should be reasonable (less than 1MB for this test)
                if (memoryIncrease > 1024 * 1024)
                    Debug.LogWarning($"Memory increase seems high: {memoryIncrease} bytes");

                if (questManager.ActiveQuests.Count != 0)
                    throw new Exception("All quests should be cleaned up after completion");

                Debug.Log("✓ Memory management test passed");
                yield return new WaitForSeconds(testDelay);
            }
            finally
            {
                CleanupQuestManager(questManager);
            }
        }

        private IEnumerator TestPerformanceUnderLoad()
        {
            Debug.Log("\n[INTEGRATION TEST] Performance Under Load");

            var questManager = CreateQuestManager();
            try
            {
                var eventManager = EventManager.Instance;
                var startTime = Time.realtimeSinceStartup;

                // Create many quests simultaneously
                var questCount = 50;
                for (int i = 0; i < questCount; i++)
                {
                    var quest = CreateTestQuestWithItemCondition($"perf_quest_{i}", "shared_item");
                    questManager.StartQuest(quest);
                }

                if (questManager.ActiveQuests.Count != questCount)
                    throw new Exception($"Expected {questCount} active quests");

                // Complete all quests with single event
                eventManager.Raise(new ItemCollectedEvent("shared_item", 1));

                // Wait for all quests to complete
                var timeout = 2.0f;
                while (questManager.ActiveQuests.Count > 0 && timeout > 0)
                {
                    timeout -= Time.deltaTime;
                    yield return null;
                }

                var endTime = Time.realtimeSinceStartup;
                var elapsedTime = endTime - startTime;

                if (questManager.ActiveQuests.Count != 0)
                    throw new Exception("All quests should be completed");

                if (elapsedTime > 1.0f)
                    Debug.LogWarning($"Performance test took longer than expected: {elapsedTime:F3}s");

                Debug.Log($"✓ Performance under load test passed ({elapsedTime:F3}s for {questCount} quests)");
                yield return new WaitForSeconds(testDelay);
            }
            finally
            {
                CleanupQuestManager(questManager);
            }
        }

        // Helper methods
        private QuestManager CreateQuestManager()
        {
            var questManagerGO = new GameObject("TestQuestManager");
            questManagerGO.SetActive(false); // Disable to prevent Awake from running
            
            var questManager = questManagerGO.AddComponent<QuestManager>();
            
            var playerRefGO = new GameObject("TestPlayerRef");
            var playerRef = playerRefGO.AddComponent<QuestPlayerRef>();

            var playerRefField = typeof(QuestManager).GetField("playerRef",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            playerRefField?.SetValue(questManager, playerRef);

            // Now enable the GameObject to trigger Awake with proper setup
            questManagerGO.SetActive(true);

            return questManager;
        }

        private void CleanupQuestManager(QuestManager questManager)
        {
            if (questManager != null)
            {
                var playerRefField = typeof(QuestManager).GetField("playerRef",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var playerRef = playerRefField?.GetValue(questManager) as QuestPlayerRef;
                
                if (playerRef != null)
                    DestroyImmediate(playerRef.gameObject);
                
                DestroyImmediate(questManager.gameObject);
            }
        }

        private QuestAsset CreateTestQuest(string questId)
        {
            var objective = new ObjectiveBuilder()
                .WithObjectiveId($"{questId}_obj")
                .WithCompletionCondition(ScriptableObject.CreateInstance<MockConditionAsset>())
                .Build();

            return new QuestBuilder()
                .WithQuestId(questId)
                .AddObjective(objective)
                .Build();
        }

        private QuestAsset CreateTestQuestWithItemCondition(string questId, string itemId)
        {
            var itemCondition = CreateItemCondition(itemId);
            var objective = new ObjectiveBuilder()
                .WithObjectiveId($"{questId}_obj")
                .WithCompletionCondition(itemCondition)
                .Build();

            return new QuestBuilder()
                .WithQuestId(questId)
                .AddObjective(objective)
                .Build();
        }

        private ItemCollectedConditionAsset CreateItemCondition(string itemId)
        {
            var itemCondition = ScriptableObject.CreateInstance<ItemCollectedConditionAsset>();
            var itemIdField = typeof(ItemCollectedConditionAsset).GetField("itemId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var requiredCountField = typeof(ItemCollectedConditionAsset).GetField("requiredCount",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            itemIdField?.SetValue(itemCondition, itemId);
            requiredCountField?.SetValue(itemCondition, 1);
            return itemCondition;
        }

        private AreaEnteredConditionAsset CreateAreaCondition(string areaId)
        {
            var areaCondition = ScriptableObject.CreateInstance<AreaEnteredConditionAsset>();
            var areaIdField = typeof(AreaEnteredConditionAsset).GetField("_areaId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            areaIdField?.SetValue(areaCondition, areaId);
            return areaCondition;
        }

        private CustomFlagConditionAsset CreateFlagCondition(string flagId, bool expectedValue)
        {
            var flagCondition = ScriptableObject.CreateInstance<CustomFlagConditionAsset>();
            var flagIdField = typeof(CustomFlagConditionAsset).GetField("_flagId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var expectedValueField = typeof(CustomFlagConditionAsset).GetField("_expectedValue",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            flagIdField?.SetValue(flagCondition, flagId);
            expectedValueField?.SetValue(flagCondition, expectedValue);
            return flagCondition;
        }
    }
}
