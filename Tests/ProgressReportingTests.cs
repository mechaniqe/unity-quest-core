using System;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Conditions;
using DynamicBox.Quest.Core.Services;
using DynamicBox.Quest.GameEvents;
using DynamicBox.EventManagement;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Tests for IProgressReportingCondition interface and implementations.
    /// Covers progress calculation, description formatting, and aggregation logic.
    /// </summary>
    public static class ProgressReportingTests
    {
        public static void RunAllProgressTests()
        {
            Debug.Log("=== Running Progress Reporting Tests ===");
            
            TestItemCollectedProgress();
            TestItemCollectedProgressDescription();
            TestTimeElapsedProgress();
            TestTimeElapsedProgressDescription();
            TestConditionGroupProgressAND();
            TestConditionGroupProgressOR();
            TestConditionGroupProgressNested();
            TestProgressEdgeCases();
            TestProgressOverCompletion();
            
            Debug.Log("✓ All progress reporting tests passed!");
        }

        private static void TestItemCollectedProgress()
        {
            Debug.Log("\n[TEST] ItemCollectedCondition Progress");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);
            var conditionInstance = new ItemCollectedConditionInstance("sword", 5);
            var condition = conditionInstance as IProgressReportingCondition;

            if (condition == null)
                throw new Exception("ItemCollectedConditionInstance should implement IProgressReportingCondition");

            conditionInstance.Bind(eventManager, context, () => { });

            // Test 0% progress
            if (Math.Abs(condition.Progress - 0f) > 0.001f)
                throw new Exception($"Expected 0% progress initially, got {condition.Progress * 100}%");

            // Test 20% progress (1/5)
            eventManager.Raise(new ItemCollectedEvent("sword", 1));
            if (Math.Abs(condition.Progress - 0.2f) > 0.001f)
                throw new Exception($"Expected 20% progress (1/5), got {condition.Progress * 100}%");

            // Test 60% progress (3/5)
            eventManager.Raise(new ItemCollectedEvent("sword", 2));
            if (Math.Abs(condition.Progress - 0.6f) > 0.001f)
                throw new Exception($"Expected 60% progress (3/5), got {condition.Progress * 100}%");

            // Test 100% progress (5/5)
            eventManager.Raise(new ItemCollectedEvent("sword", 2));
            if (Math.Abs(condition.Progress - 1.0f) > 0.001f)
                throw new Exception($"Expected 100% progress (5/5), got {condition.Progress * 100}%");

            Debug.Log("✓ ItemCollectedCondition progress calculation works correctly");
        }

        private static void TestItemCollectedProgressDescription()
        {
            Debug.Log("\n[TEST] ItemCollectedCondition Progress Description");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);
            var conditionInstance = new ItemCollectedConditionInstance("potion", 10);
            var condition = conditionInstance as IProgressReportingCondition;

            conditionInstance.Bind(eventManager, context, () => { });

            // Initial description
            string desc = condition.ProgressDescription;
            if (!desc.Contains("0") || !desc.Contains("10"))
                throw new Exception($"Expected description with '0/10', got: {desc}");

            // After collecting some
            eventManager.Raise(new ItemCollectedEvent("potion", 3));
            desc = condition.ProgressDescription;
            if (!desc.Contains("3") || !desc.Contains("10"))
                throw new Exception($"Expected description with '3/10', got: {desc}");

            Debug.Log($"   Progress descriptions: '{condition.ProgressDescription}'");
            Debug.Log("✓ ItemCollectedCondition progress description works correctly");
        }

        private static void TestTimeElapsedProgress()
        {
            Debug.Log("\n[TEST] TimeElapsedCondition Progress");

            var gameObject = new GameObject("TestTimeService");
            try
            {
                var timeService = gameObject.AddComponent<DefaultTimeService>();
                var eventManager = EventManager.Instance;
                var context = new QuestContext(null, null, timeService);

                var timeAsset = ScriptableObject.CreateInstance<TimeElapsedConditionAsset>();
                var requiredSecondsField = typeof(TimeElapsedConditionAsset).GetField("requiredSeconds",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                requiredSecondsField?.SetValue(timeAsset, 10.0f);

                var conditionInstance = timeAsset.CreateInstance();
                var condition = conditionInstance as IProgressReportingCondition;
                var pollingCondition = conditionInstance as IPollingConditionInstance;

                if (condition == null)
                    throw new Exception("TimeElapsedConditionInstance should implement IProgressReportingCondition");

                conditionInstance.Bind(eventManager, context, () => { });

                // Test 0% progress
                if (Math.Abs(condition.Progress - 0f) > 0.001f)
                    throw new Exception($"Expected 0% progress initially, got {condition.Progress * 100}%");

                // Simulate 5 seconds passing (50% progress)
                var totalTimeField = typeof(DefaultTimeService).GetField("_totalGameTime",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                totalTimeField?.SetValue(timeService, 5.0f);

                pollingCondition?.Refresh(context, () => { });

                if (Math.Abs(condition.Progress - 0.5f) > 0.05f) // Allow small margin
                    throw new Exception($"Expected ~50% progress after 5s, got {condition.Progress * 100}%");

                // Simulate 10 seconds passing (100% progress)
                totalTimeField?.SetValue(timeService, 10.0f);
                pollingCondition?.Refresh(context, () => { });

                if (condition.Progress < 0.95f) // Should be at or near 100%
                    throw new Exception($"Expected ~100% progress after 10s, got {condition.Progress * 100}%");

                Debug.Log("✓ TimeElapsedCondition progress calculation works correctly");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        private static void TestTimeElapsedProgressDescription()
        {
            Debug.Log("\n[TEST] TimeElapsedCondition Progress Description");

            var gameObject = new GameObject("TestTimeService");
            try
            {
                var timeService = gameObject.AddComponent<DefaultTimeService>();
                var context = new QuestContext(null, null, timeService);
                
                var timeAsset = ScriptableObject.CreateInstance<TimeElapsedConditionAsset>();
                var requiredSecondsField = typeof(TimeElapsedConditionAsset).GetField("requiredSeconds",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                requiredSecondsField?.SetValue(timeAsset, 30.0f);

                var conditionInstance = timeAsset.CreateInstance();
                var condition = conditionInstance as IProgressReportingCondition;

                conditionInstance.Bind(EventManager.Instance, context, () => { });

                string desc = condition.ProgressDescription;
                if (!desc.ToLower().Contains("second"))
                    throw new Exception($"Expected time-based description, got: {desc}");

                Debug.Log($"   Time progress description: '{desc}'");
                Debug.Log("✓ TimeElapsedCondition progress description works correctly");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        private static void TestConditionGroupProgressAND()
        {
            Debug.Log("\n[TEST] ConditionGroup Progress (AND)");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);

            var cond1 = new ItemCollectedConditionInstance("sword", 2);
            var cond2 = new ItemCollectedConditionInstance("shield", 1);

            var groupInstance = new ConditionGroupInstance(ConditionOperator.And,
                new List<IConditionInstance> { cond1, cond2 });
            var group = groupInstance as IProgressReportingCondition;

            if (group == null)
                throw new Exception("ConditionGroupInstance should implement IProgressReportingCondition");

            groupInstance.Bind(eventManager, context, () => { });

            // Initial: 0% (0/2 children complete)
            if (Math.Abs(group.Progress - 0f) > 0.001f)
                throw new Exception($"Expected 0% progress initially, got {group.Progress * 100}%");

            // Complete first condition: 50% (1/2 children complete)
            eventManager.Raise(new ItemCollectedEvent("sword", 2));
            if (Math.Abs(group.Progress - 0.5f) > 0.05f)
                throw new Exception($"Expected ~50% progress (1/2 complete), got {group.Progress * 100}%");

            // Complete second condition: 100% (2/2 children complete)
            eventManager.Raise(new ItemCollectedEvent("shield", 1));
            if (Math.Abs(group.Progress - 1.0f) > 0.001f)
                throw new Exception($"Expected 100% progress (2/2 complete), got {group.Progress * 100}%");

            Debug.Log("✓ ConditionGroup AND progress aggregation works correctly");
        }

        private static void TestConditionGroupProgressOR()
        {
            Debug.Log("\n[TEST] ConditionGroup Progress (OR)");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);

            var cond1 = new ItemCollectedConditionInstance("sword", 2);
            var cond2 = new ItemCollectedConditionInstance("shield", 1);

            var groupInstance = new ConditionGroupInstance(ConditionOperator.Or,
                new List<IConditionInstance> { cond1, cond2 });
            var group = groupInstance as IProgressReportingCondition;

            groupInstance.Bind(eventManager, context, () => { });

            // For OR: progress should be the MAX of children
            eventManager.Raise(new ItemCollectedEvent("sword", 1));
            float swordProgress = 0.5f; // 1/2
            
            if (Math.Abs(group.Progress - swordProgress) > 0.05f)
                throw new Exception($"Expected OR progress to be max child progress (~50%), got {group.Progress * 100}%");

            // Complete shield (100%) - OR should now show 100%
            eventManager.Raise(new ItemCollectedEvent("shield", 1));
            if (Math.Abs(group.Progress - 1.0f) > 0.001f)
                throw new Exception($"Expected 100% progress (one child complete in OR), got {group.Progress * 100}%");

            Debug.Log("✓ ConditionGroup OR progress aggregation works correctly");
        }

        private static void TestConditionGroupProgressNested()
        {
            Debug.Log("\n[TEST] ConditionGroup Progress (Nested)");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);

            var cond1 = new ItemCollectedConditionInstance("sword", 1);
            var cond2 = new ItemCollectedConditionInstance("shield", 1);
            var innerGroup = new ConditionGroupInstance(ConditionOperator.And,
                new List<IConditionInstance> { cond1, cond2 });

            var cond3 = new ItemCollectedConditionInstance("potion", 1);
            var outerGroupInstance = new ConditionGroupInstance(ConditionOperator.And,
                new List<IConditionInstance> { innerGroup, cond3 });
            var outerGroup = outerGroupInstance as IProgressReportingCondition;

            outerGroupInstance.Bind(eventManager, context, () => { });

            // Initial: 0%
            if (Math.Abs(outerGroup.Progress - 0f) > 0.001f)
                throw new Exception($"Expected 0% progress initially, got {outerGroup.Progress * 100}%");

            // Complete inner group first child (sword)
            eventManager.Raise(new ItemCollectedEvent("sword", 1));
            
            // Progress should reflect partial completion
            if (outerGroup.Progress >= 1.0f)
                throw new Exception($"Progress should not be 100% with incomplete nested conditions");

            // Complete all conditions
            eventManager.Raise(new ItemCollectedEvent("shield", 1));
            eventManager.Raise(new ItemCollectedEvent("potion", 1));

            if (Math.Abs(outerGroup.Progress - 1.0f) > 0.001f)
                throw new Exception($"Expected 100% progress when all nested conditions complete, got {outerGroup.Progress * 100}%");

            Debug.Log("✓ ConditionGroup nested progress aggregation works correctly");
        }

        private static void TestProgressEdgeCases()
        {
            Debug.Log("\n[TEST] Progress Edge Cases");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);

            // Test zero-quantity condition
            var zeroConditionInstance = new ItemCollectedConditionInstance("item", 0);
            var zeroCondition = zeroConditionInstance as IProgressReportingCondition;
            zeroConditionInstance.Bind(eventManager, context, () => { });
            
            // Should be 100% complete immediately (0 required)
            if (Math.Abs(zeroCondition.Progress - 1.0f) > 0.001f)
                throw new Exception($"Zero-requirement condition should be 100% complete, got {zeroCondition.Progress * 100}%");

            // Test empty condition group
            var emptyGroupInstance = new ConditionGroupInstance(ConditionOperator.And,
                new List<IConditionInstance>());
            var emptyGroup = emptyGroupInstance as IProgressReportingCondition;
            emptyGroupInstance.Bind(eventManager, context, () => { });
            
            // Empty group should be 100% complete
            if (Math.Abs(emptyGroup.Progress - 1.0f) > 0.001f)
                throw new Exception($"Empty condition group should be 100% complete, got {emptyGroup.Progress * 100}%");

            Debug.Log("✓ Progress edge cases handled correctly");
        }

        private static void TestProgressOverCompletion()
        {
            Debug.Log("\n[TEST] Progress Over-Completion");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);
            var conditionInstance = new ItemCollectedConditionInstance("gem", 5);
            var condition = conditionInstance as IProgressReportingCondition;

            conditionInstance.Bind(eventManager, context, () => { });

            // Collect way more than required
            eventManager.Raise(new ItemCollectedEvent("gem", 100));

            // Progress should be clamped to 100%
            if (condition.Progress > 1.0f)
                throw new Exception($"Progress should be clamped to 100%, got {condition.Progress * 100}%");

            if (Math.Abs(condition.Progress - 1.0f) > 0.001f)
                throw new Exception($"Expected exactly 100% progress, got {condition.Progress * 100}%");

            Debug.Log("✓ Progress over-completion clamped correctly");
        }
    }
}
