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
            TestProgressWithNegativeValues();
            TestProgressPartialUpdates();
            TestProgressConcurrentEvents();
            TestProgressDescriptionFormatting();
            TestProgressBoundaryConditions();
            TestConditionGroupMixedProgress();
            TestProgressWithoutProgressReporting();
            
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

        private static void TestProgressWithNegativeValues()
        {
            Debug.Log("\n[TEST] Progress With Negative Values");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);
            var conditionInstance = new ItemCollectedConditionInstance("coin", 10);
            var condition = conditionInstance as IProgressReportingCondition;

            conditionInstance.Bind(eventManager, context, () => { });

            // Try to add negative amount (edge case)
            eventManager.Raise(new ItemCollectedEvent("coin", -5));

            // Progress should not go negative
            if (condition.Progress < 0.0f)
                throw new Exception($"Progress should not be negative, got {condition.Progress * 100}%");

            // Now add positive amount
            eventManager.Raise(new ItemCollectedEvent("coin", 5));
            
            // Progress should be correct based on implementation
            if (condition.Progress > 1.0f || condition.Progress < 0.0f)
                throw new Exception($"Progress should be between 0-100%, got {condition.Progress * 100}%");

            Debug.Log("✓ Progress with negative values handled correctly");
        }

        private static void TestProgressPartialUpdates()
        {
            Debug.Log("\n[TEST] Progress Partial Updates");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);
            var conditionInstance = new ItemCollectedConditionInstance("resource", 100);
            var condition = conditionInstance as IProgressReportingCondition;

            bool progressChanged = false;
            conditionInstance.Bind(eventManager, context, () => { progressChanged = true; });

            // Small incremental updates
            float lastProgress = 0f;
            for (int i = 1; i <= 10; i++)
            {
                progressChanged = false;
                eventManager.Raise(new ItemCollectedEvent("resource", 10));
                
                float expectedProgress = i * 0.1f;
                if (Math.Abs(condition.Progress - expectedProgress) > 0.01f)
                    throw new Exception($"Expected {expectedProgress * 100}% progress, got {condition.Progress * 100}%");

                // Progress should increase monotonically
                if (condition.Progress < lastProgress)
                    throw new Exception($"Progress decreased from {lastProgress * 100}% to {condition.Progress * 100}%");

                lastProgress = condition.Progress;

                // Should notify on each change
                if (!progressChanged && i < 10) // Last one completes, behavior may vary
                    Debug.LogWarning($"Progress change notification not fired at iteration {i}");
            }

            Debug.Log("✓ Progress partial updates work correctly");
        }

        private static void TestProgressConcurrentEvents()
        {
            Debug.Log("\n[TEST] Progress Concurrent Events");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);
            
            var cond1 = new ItemCollectedConditionInstance("gold", 10);
            var cond2 = new ItemCollectedConditionInstance("silver", 20);
            var cond3 = new ItemCollectedConditionInstance("bronze", 30);
            
            var groupInstance = new ConditionGroupInstance(ConditionOperator.And,
                new List<IConditionInstance> { cond1, cond2, cond3 });
            var group = groupInstance as IProgressReportingCondition;

            groupInstance.Bind(eventManager, context, () => { });

            // Fire multiple events in quick succession
            eventManager.Raise(new ItemCollectedEvent("gold", 5));
            eventManager.Raise(new ItemCollectedEvent("silver", 10));
            eventManager.Raise(new ItemCollectedEvent("bronze", 15));

            // All should be at 50%
            float expectedProgress = 0.5f;
            if (Math.Abs(group.Progress - expectedProgress) > 0.1f)
                throw new Exception($"Expected ~{expectedProgress * 100}% progress, got {group.Progress * 100}%");

            // Complete all
            eventManager.Raise(new ItemCollectedEvent("gold", 5));
            eventManager.Raise(new ItemCollectedEvent("silver", 10));
            eventManager.Raise(new ItemCollectedEvent("bronze", 15));

            if (Math.Abs(group.Progress - 1.0f) > 0.001f)
                throw new Exception($"Expected 100% progress, got {group.Progress * 100}%");

            Debug.Log("✓ Progress concurrent events handled correctly");
        }

        private static void TestProgressDescriptionFormatting()
        {
            Debug.Log("\n[TEST] Progress Description Formatting");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);

            // Test various quantity formats
            var tests = new[]
            {
                ("item1", 1, "single item"),
                ("item2", 100, "large quantity"),
                ("item3", 999, "very large quantity")
            };

            foreach (var (itemId, count, testName) in tests)
            {
                var conditionInstance = new ItemCollectedConditionInstance(itemId, count);
                var condition = conditionInstance as IProgressReportingCondition;
                conditionInstance.Bind(eventManager, context, () => { });

                string desc = condition.ProgressDescription;
                
                // Description should contain the count
                if (!desc.Contains(count.ToString()))
                    throw new Exception($"Description '{desc}' should contain count {count} ({testName})");

                // Description should not be empty
                if (string.IsNullOrWhiteSpace(desc))
                    throw new Exception($"Description should not be empty ({testName})");

                Debug.Log($"   {testName}: '{desc}'");
            }

            Debug.Log("✓ Progress description formatting works correctly");
        }

        private static void TestProgressBoundaryConditions()
        {
            Debug.Log("\n[TEST] Progress Boundary Conditions");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);

            // Test with quantity of 1 (single item)
            var singleInstance = new ItemCollectedConditionInstance("unique", 1);
            var single = singleInstance as IProgressReportingCondition;
            singleInstance.Bind(eventManager, context, () => { });

            // Should be 0% initially
            if (Math.Abs(single.Progress - 0f) > 0.001f)
                throw new Exception($"Single-item condition should start at 0%, got {single.Progress * 100}%");

            // Should jump to 100% after one event
            eventManager.Raise(new ItemCollectedEvent("unique", 1));
            if (Math.Abs(single.Progress - 1.0f) > 0.001f)
                throw new Exception($"Single-item condition should be 100% after one event, got {single.Progress * 100}%");

            // Test with very large quantity
            var largeInstance = new ItemCollectedConditionInstance("huge", int.MaxValue);
            var large = largeInstance as IProgressReportingCondition;
            largeInstance.Bind(eventManager, context, () => { });

            eventManager.Raise(new ItemCollectedEvent("huge", 1000000));
            
            // Progress should be very small but non-zero
            if (large.Progress <= 0.0f || large.Progress > 0.1f)
                throw new Exception($"Large quantity should have small progress, got {large.Progress * 100}%");

            Debug.Log("✓ Progress boundary conditions handled correctly");
        }

        private static void TestConditionGroupMixedProgress()
        {
            Debug.Log("\n[TEST] ConditionGroup Mixed Progress States");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);

            // Mix of complete and incomplete conditions
            var complete1 = new ItemCollectedConditionInstance("done1", 1);
            var complete2 = new ItemCollectedConditionInstance("done2", 1);
            var incomplete = new ItemCollectedConditionInstance("todo", 10);

            var groupInstance = new ConditionGroupInstance(ConditionOperator.And,
                new List<IConditionInstance> { complete1, complete2, incomplete });
            var group = groupInstance as IProgressReportingCondition;

            groupInstance.Bind(eventManager, context, () => { });

            // Complete first two
            eventManager.Raise(new ItemCollectedEvent("done1", 1));
            eventManager.Raise(new ItemCollectedEvent("done2", 1));

            // Progress should be between 0-100% (2 of 3 complete = ~66%)
            float progress = group.Progress;
            if (progress <= 0.5f || progress >= 1.0f)
                throw new Exception($"Expected progress ~66% with 2/3 conditions complete, got {progress * 100}%");

            // Partially complete the third
            eventManager.Raise(new ItemCollectedEvent("todo", 5));
            
            // Progress should have increased
            if (group.Progress <= progress)
                throw new Exception("Progress should have increased after partial completion");

            Debug.Log("✓ ConditionGroup mixed progress states work correctly");
        }

        private static void TestProgressWithoutProgressReporting()
        {
            Debug.Log("\n[TEST] Conditions Without Progress Reporting");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);

            // Create a condition group with a non-progress-reporting condition
            var progressCond = new ItemCollectedConditionInstance("item", 10);
            
            // Mock condition that doesn't implement IProgressReportingCondition
            var mockCond = new MockConditionInstance();

            var groupInstance = new ConditionGroupInstance(ConditionOperator.And,
                new List<IConditionInstance> { progressCond, mockCond });
            var group = groupInstance as IProgressReportingCondition;

            groupInstance.Bind(eventManager, context, () => { });

            // Should still calculate progress based on conditions that support it
            eventManager.Raise(new ItemCollectedEvent("item", 5));
            
            // Progress should be calculable (50% from one condition)
            float progress = group.Progress;
            if (progress < 0.0f || progress > 1.0f)
                throw new Exception($"Progress should be valid even with non-reporting conditions, got {progress * 100}%");

            Debug.Log("✓ Conditions without progress reporting handled correctly");
        }
    }
}
