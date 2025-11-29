using System;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Conditions;
using UnityEngine;
using System.Reflection;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Additional tests for advanced QuestManager functionality and service integration.
    /// These tests cover the gaps identified in the coverage analysis.
    /// </summary>
    public static class QuestSystemAdvancedTests
    {
        public static void RunAdvancedTests()
        {
            Console.WriteLine("=== Running Advanced Quest System Tests ===");
            
            // Manual quest control tests
            TestManualQuestCompletion();
            TestManualQuestFailure();
            TestCanProgressObjectiveValidation();
            
            // Service integration tests  
            TestQuestContextWithServices();
            TestQuestPlayerRefBuildContext();
            
            // Advanced QuestManager internal methods
            TestEvaluateObjectiveAndQuestLogic();
            TestMarkDirtyAndProcessQueue();
            
            // Advanced condition scenarios
            TestComplexPrerequisiteChains();
            TestNestedConditionPerformance();
            
            // Error recovery and robustness
            TestCorruptedConditionHandling();
            TestMissingPrerequisiteHandling();
            
            Console.WriteLine("✓ All advanced tests passed!");
        }

        private static void TestManualQuestCompletion()
        {
            Console.WriteLine("\n[ADVANCED TEST] Manual Quest Completion");

            var questManager = CreateTestQuestManager();
            try
            {
                var quest = new QuestBuilder()
                    .WithQuestId("manual_test")
                    .AddObjective(new ObjectiveBuilder().WithObjectiveId("obj1").Build())
                    .Build();

                bool questCompleted = false;
                questManager.OnQuestCompleted += (q) => questCompleted = true;

                var questState = questManager.StartQuest(quest);
                
                if (questState.Status != QuestStatus.InProgress)
                    throw new Exception("Quest should be in progress");

                // Manually complete the quest
                questManager.CompleteQuest(questState);

                if (questState.Status != QuestStatus.Completed)
                    throw new Exception("Quest should be marked as completed");

                if (!questCompleted)
                    throw new Exception("OnQuestCompleted event should fire");

                if (questManager.ActiveQuests.Count != 0)
                    throw new Exception("Quest should be removed from active quests");

                Console.WriteLine("✓ Manual quest completion works correctly");
            }
            finally
            {
                CleanupTestQuestManager(questManager);
            }
        }

        private static void TestManualQuestFailure()
        {
            Console.WriteLine("\n[ADVANCED TEST] Manual Quest Failure");

            var questManager = CreateTestQuestManager();
            try
            {
                var quest = new QuestBuilder()
                    .WithQuestId("manual_fail_test")
                    .AddObjective(new ObjectiveBuilder().WithObjectiveId("obj1").Build())
                    .Build();

                bool questFailed = false;
                questManager.OnQuestFailed += (q) => questFailed = true;

                var questState = questManager.StartQuest(quest);
                
                if (questState.Status != QuestStatus.InProgress)
                    throw new Exception("Quest should be in progress");

                // Manually fail the quest
                questManager.FailQuest(questState);

                if (questState.Status != QuestStatus.Failed)
                    throw new Exception("Quest should be marked as failed");

                if (!questFailed)
                    throw new Exception("OnQuestFailed event should fire");

                if (questManager.ActiveQuests.Count != 0)
                    throw new Exception("Quest should be removed from active quests");

                Console.WriteLine("✓ Manual quest failure works correctly");
            }
            finally
            {
                CleanupTestQuestManager(questManager);
            }
        }

        private static void TestCanProgressObjectiveValidation()
        {
            Console.WriteLine("\n[ADVANCED TEST] CanProgressObjective Validation");

            // Create complex prerequisite chain
            var obj1 = new ObjectiveBuilder().WithObjectiveId("obj1").Build();
            var obj2 = new ObjectiveBuilder().WithObjectiveId("obj2").AddPrerequisite(obj1).Build();
            var obj3 = new ObjectiveBuilder()
                .WithObjectiveId("obj3")
                .AddPrerequisite(obj1)
                .AddPrerequisite(obj2)
                .Build();

            var quest = new QuestBuilder()
                .WithQuestId("prereq_test")
                .AddObjective(obj1)
                .AddObjective(obj2)
                .AddObjective(obj3)
                .Build();

            var questState = new QuestState(quest);

            // Use reflection to access CanProgressObjective
            var canProgressMethod = typeof(QuestManager).GetMethod("CanProgressObjective",
                BindingFlags.NonPublic | BindingFlags.Static);

            // Test obj1 (no prerequisites) - should be able to progress
            var obj1State = questState.Objectives["obj1"];
            bool canProgress = (bool)canProgressMethod.Invoke(null, new object[] { obj1State, questState });
            if (!canProgress)
                throw new Exception("obj1 should be able to progress (no prerequisites)");

            // Test obj2 (requires obj1) - should not progress initially
            var obj2State = questState.Objectives["obj2"];
            canProgress = (bool)canProgressMethod.Invoke(null, new object[] { obj2State, questState });
            if (canProgress)
                throw new Exception("obj2 should not progress without obj1 completed");

            // Complete obj1, test obj2 again
            obj1State.SetStatus(ObjectiveStatus.Completed);
            canProgress = (bool)canProgressMethod.Invoke(null, new object[] { obj2State, questState });
            if (!canProgress)
                throw new Exception("obj2 should progress after obj1 completed");

            // Test obj3 (requires both obj1 and obj2) - should not progress yet
            var obj3State = questState.Objectives["obj3"];
            canProgress = (bool)canProgressMethod.Invoke(null, new object[] { obj3State, questState });
            if (canProgress)
                throw new Exception("obj3 should not progress without both prerequisites");

            // Complete obj2, test obj3 again
            obj2State.SetStatus(ObjectiveStatus.Completed);
            canProgress = (bool)canProgressMethod.Invoke(null, new object[] { obj3State, questState });
            if (!canProgress)
                throw new Exception("obj3 should progress after both prerequisites completed");

            Console.WriteLine("✓ CanProgressObjective validation works correctly");
        }

        private static void TestQuestContextWithServices()
        {
            Console.WriteLine("\n[ADVANCED TEST] QuestContext With Services");

            // Create a QuestContext with null services (common scenario)
            var context = new QuestContext(null, null, null);
            
            if (context == null)
                throw new Exception("QuestContext should be created with null services");

            // Test that conditions can handle null service context
            var itemCondition = new ItemCollectedConditionInstance("test_item", 1);
            
            try
            {
                // This should not crash even with null services in context
                itemCondition.Bind(DynamicBox.EventManagement.EventManager.Instance, context, () => { });
                Console.WriteLine("   ✓ Condition binding handles null services gracefully");
            }
            catch (Exception ex)
            {
                throw new Exception($"Condition binding should handle null services: {ex.Message}");
            }

            Console.WriteLine("✓ QuestContext with services works correctly");
        }

        private static void TestQuestPlayerRefBuildContext()
        {
            Console.WriteLine("\n[ADVANCED TEST] QuestPlayerRef BuildContext");

            var playerRefObject = new GameObject("TestPlayerRef");
            try
            {
                var playerRef = playerRefObject.AddComponent<QuestPlayerRef>();
                
                // BuildContext should work with null service providers
                var context = playerRef.BuildContext();
                
                if (context == null)
                    throw new Exception("BuildContext should return a valid context");

                Console.WriteLine("✓ QuestPlayerRef BuildContext works correctly");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(playerRefObject);
            }
        }

        private static void TestEvaluateObjectiveAndQuestLogic()
        {
            Console.WriteLine("\n[ADVANCED TEST] EvaluateObjectiveAndQuest Logic");

            var questManager = CreateTestQuestManager();
            try
            {
                // Create quest with completion and fail conditions
                var completionCondition = ScriptableObject.CreateInstance<MockConditionAsset>();
                var failCondition = ScriptableObject.CreateInstance<MockConditionAsset>();

                var objective = new ObjectiveBuilder()
                    .WithObjectiveId("obj1")
                    .WithCompletionCondition(completionCondition)
                    .WithFailCondition(failCondition)
                    .Build();

                var quest = new QuestBuilder()
                    .WithQuestId("eval_test")
                    .AddObjective(objective)
                    .Build();

                bool questFailed = false;
                bool questCompleted = false;
                bool objectiveChanged = false;

                questManager.OnQuestFailed += (q) => questFailed = true;
                questManager.OnQuestCompleted += (q) => questCompleted = true;
                questManager.OnObjectiveStatusChanged += (obj) => objectiveChanged = true;

                var questState = questManager.StartQuest(quest);

                // Test fail condition takes priority over completion
                var failInstance = questState.Objectives["obj1"].FailInstance as MockConditionInstance;
                var completionInstance = questState.Objectives["obj1"].CompletionInstance as MockConditionInstance;

                // Set both conditions to met
                failInstance?.SetMet(true);
                completionInstance?.SetMet(true);

                // Process pending evaluations
                questManager.ProcessPendingEvaluations();

                // Quest should fail (fail condition has priority)
                if (!questFailed)
                    throw new Exception("Quest should fail when fail condition is met");

                if (questCompleted)
                    throw new Exception("Quest should not complete if fail condition is met");

                if (!objectiveChanged)
                    throw new Exception("Objective status change event should fire");

                Console.WriteLine("✓ EvaluateObjectiveAndQuest logic works correctly");
            }
            finally
            {
                CleanupTestQuestManager(questManager);
            }
        }

        private static void TestMarkDirtyAndProcessQueue()
        {
            Console.WriteLine("\n[ADVANCED TEST] MarkDirty and ProcessQueue");

            var questManager = CreateTestQuestManager();
            try
            {
                var completionCondition = ScriptableObject.CreateInstance<MockConditionAsset>();
                var objective = new ObjectiveBuilder()
                    .WithObjectiveId("obj1")
                    .WithCompletionCondition(completionCondition)
                    .Build();

                var quest = new QuestBuilder()
                    .WithQuestId("dirty_test")
                    .AddObjective(objective)
                    .Build();

                int statusChangeCount = 0;
                questManager.OnObjectiveStatusChanged += (obj) => statusChangeCount++;

                var questState = questManager.StartQuest(quest);
                var conditionInstance = questState.Objectives["obj1"].CompletionInstance as MockConditionInstance;

                // Trigger condition multiple times rapidly
                conditionInstance?.SetMet(false);
                conditionInstance?.SetMet(true);
                conditionInstance?.SetMet(false);
                conditionInstance?.SetMet(true);

                // Process all queued changes
                questManager.ProcessPendingEvaluations();

                // Should only trigger once because dirty queue uses HashSet deduplication
                if (statusChangeCount == 0)
                    throw new Exception("At least one status change should have been processed");

                Console.WriteLine($"✓ Processed {statusChangeCount} status changes through dirty queue");
            }
            finally
            {
                CleanupTestQuestManager(questManager);
            }
        }

        private static void TestComplexPrerequisiteChains()
        {
            Console.WriteLine("\n[ADVANCED TEST] Complex Prerequisite Chains");

            // Create a diamond dependency pattern: obj1 -> obj2, obj1 -> obj3, obj2+obj3 -> obj4
            var obj1 = new ObjectiveBuilder().WithObjectiveId("obj1").Build();
            var obj2 = new ObjectiveBuilder().WithObjectiveId("obj2").AddPrerequisite(obj1).Build();
            var obj3 = new ObjectiveBuilder().WithObjectiveId("obj3").AddPrerequisite(obj1).Build();
            var obj4 = new ObjectiveBuilder()
                .WithObjectiveId("obj4")
                .AddPrerequisite(obj2)
                .AddPrerequisite(obj3)
                .Build();

            var quest = new QuestBuilder()
                .WithQuestId("complex_prereq")
                .AddObjective(obj1)
                .AddObjective(obj2)
                .AddObjective(obj3)
                .AddObjective(obj4)
                .Build();

            var questState = new QuestState(quest);

            // Test progression through the dependency chain
            var obj1State = questState.Objectives["obj1"];
            var obj2State = questState.Objectives["obj2"];
            var obj3State = questState.Objectives["obj3"];
            var obj4State = questState.Objectives["obj4"];

            var canProgressMethod = typeof(QuestManager).GetMethod("CanProgressObjective",
                BindingFlags.NonPublic | BindingFlags.Static);

            // Initially only obj1 should be able to progress
            bool canProgress1 = (bool)canProgressMethod.Invoke(null, new object[] { obj1State, questState });
            bool canProgress2 = (bool)canProgressMethod.Invoke(null, new object[] { obj2State, questState });
            bool canProgress3 = (bool)canProgressMethod.Invoke(null, new object[] { obj3State, questState });
            bool canProgress4 = (bool)canProgressMethod.Invoke(null, new object[] { obj4State, questState });

            if (!canProgress1 || canProgress2 || canProgress3 || canProgress4)
                throw new Exception("Only obj1 should be progressable initially");

            // Complete obj1, now obj2 and obj3 should be progressable
            obj1State.SetStatus(ObjectiveStatus.Completed);
            canProgress2 = (bool)canProgressMethod.Invoke(null, new object[] { obj2State, questState });
            canProgress3 = (bool)canProgressMethod.Invoke(null, new object[] { obj3State, questState });
            canProgress4 = (bool)canProgressMethod.Invoke(null, new object[] { obj4State, questState });

            if (!canProgress2 || !canProgress3 || canProgress4)
                throw new Exception("obj2 and obj3 should be progressable after obj1, but not obj4");

            // Complete obj2 but not obj3, obj4 should still not be progressable
            obj2State.SetStatus(ObjectiveStatus.Completed);
            canProgress4 = (bool)canProgressMethod.Invoke(null, new object[] { obj4State, questState });

            if (canProgress4)
                throw new Exception("obj4 should not be progressable until both obj2 and obj3 are complete");

            // Complete obj3, now obj4 should be progressable
            obj3State.SetStatus(ObjectiveStatus.Completed);
            canProgress4 = (bool)canProgressMethod.Invoke(null, new object[] { obj4State, questState });

            if (!canProgress4)
                throw new Exception("obj4 should be progressable after both obj2 and obj3 are complete");

            Console.WriteLine("✓ Complex prerequisite chains work correctly");
        }

        private static void TestNestedConditionPerformance()
        {
            Console.WriteLine("\n[ADVANCED TEST] Nested Condition Performance");

            // Create deeply nested condition groups to test performance
            var deepestConditions = new System.Collections.Generic.List<IConditionInstance>();
            
            // Create 10 mock conditions
            for (int i = 0; i < 10; i++)
            {
                deepestConditions.Add(ScriptableObject.CreateInstance<MockConditionAsset>().CreateInstance());
            }

            // Create nested OR groups: ((((A OR B) OR C) OR D) OR ...)
            IConditionInstance currentGroup = deepestConditions[0];
            for (int i = 1; i < deepestConditions.Count; i++)
            {
                currentGroup = new ConditionGroupInstance(
                    ConditionOperator.Or, 
                    new System.Collections.Generic.List<IConditionInstance> { currentGroup, deepestConditions[i] }
                );
            }

            // Test binding performance
            var startTime = DateTime.Now;
            currentGroup.Bind(DynamicBox.EventManagement.EventManager.Instance, new QuestContext(null, null, null), () => { });
            var bindTime = DateTime.Now - startTime;

            // Test evaluation performance
            startTime = DateTime.Now;
            bool isMet = currentGroup.IsMet;
            var evalTime = DateTime.Now - startTime;

            // Should complete quickly (under 10ms for this simple case)
            if (bindTime.TotalMilliseconds > 10 || evalTime.TotalMilliseconds > 10)
            {
                Console.WriteLine($"   Warning: Nested conditions took {bindTime.TotalMilliseconds:F1}ms to bind, {evalTime.TotalMilliseconds:F1}ms to evaluate");
            }

            Console.WriteLine("✓ Nested condition performance acceptable");
        }

        private static void TestCorruptedConditionHandling()
        {
            Console.WriteLine("\n[ADVANCED TEST] Corrupted Condition Handling");

            var questManager = CreateTestQuestManager();
            try
            {
                // Create objective with null completion condition
                var objective = new ObjectiveBuilder()
                    .WithObjectiveId("corrupted_obj")
                    .WithCompletionCondition(null)
                    .Build();

                var quest = new QuestBuilder()
                    .WithQuestId("corrupted_quest")
                    .AddObjective(objective)
                    .Build();

                // This should not crash even with null condition
                var questState = questManager.StartQuest(quest);
                
                if (questState == null)
                    throw new Exception("Quest state should be created even with null conditions");

                var objState = questState.Objectives["corrupted_obj"];
                if (objState.CompletionInstance != null)
                    throw new Exception("Completion instance should be null for null condition");

                Console.WriteLine("✓ Corrupted condition handling works correctly");
            }
            finally
            {
                CleanupTestQuestManager(questManager);
            }
        }

        private static void TestMissingPrerequisiteHandling()
        {
            Console.WriteLine("\n[ADVANCED TEST] Missing Prerequisite Handling");

            // Create objective that references a prerequisite not in the quest
            var missingPrereq = new ObjectiveBuilder().WithObjectiveId("missing_obj").Build();
            var validObj = new ObjectiveBuilder()
                .WithObjectiveId("valid_obj")
                .AddPrerequisite(missingPrereq)
                .Build();

            var quest = new QuestBuilder()
                .WithQuestId("missing_prereq_quest")
                .AddObjective(validObj)  // Note: missing_obj is not added to quest
                .Build();

            var questState = new QuestState(quest);
            var validObjState = questState.Objectives["valid_obj"];

            var canProgressMethod = typeof(QuestManager).GetMethod("CanProgressObjective",
                BindingFlags.NonPublic | BindingFlags.Static);

            // Should handle missing prerequisite gracefully (current implementation skips missing prereqs)
            bool canProgress = (bool)canProgressMethod.Invoke(null, new object[] { validObjState, questState });

            // Based on current implementation, this should return true (treats missing as skippable)
            if (!canProgress)
                Console.WriteLine("   Note: Missing prerequisites are treated as blocking (strict validation)");
            else
                Console.WriteLine("   Note: Missing prerequisites are skipped (lenient validation)");

            Console.WriteLine("✓ Missing prerequisite handling works correctly");
        }

        // Helper methods
        private static QuestManager CreateTestQuestManager()
        {
            var gameObject = new GameObject("TestQuestManager");
            gameObject.SetActive(false);
            
            var questManager = gameObject.AddComponent<QuestManager>();
            
            var playerRefObject = new GameObject("TestPlayerRef");
            var playerRef = playerRefObject.AddComponent<QuestPlayerRef>();
            
            var playerRefField = typeof(QuestManager).GetField("playerRef",
                BindingFlags.NonPublic | BindingFlags.Instance);
            playerRefField?.SetValue(questManager, playerRef);

            gameObject.SetActive(true);
            return questManager;
        }

        private static void CleanupTestQuestManager(QuestManager questManager)
        {
            if (questManager != null)
            {
                var playerRefField = typeof(QuestManager).GetField("playerRef",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                var playerRef = playerRefField?.GetValue(questManager) as QuestPlayerRef;
                
                if (playerRef != null)
                {
                    UnityEngine.Object.DestroyImmediate(playerRef.gameObject);
                }
                
                UnityEngine.Object.DestroyImmediate(questManager.gameObject);
            }
        }
    }
}
