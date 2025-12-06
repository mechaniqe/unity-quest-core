#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Conditions;
using DynamicBox.Quest.Core.Services;
using DynamicBox.Quest.GameEvents;
using DynamicBox.EventManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Performance benchmark tests for the quest system.
    /// Establishes baseline performance metrics and detects regressions.
    /// </summary>
    public static class PerformanceBenchmarkTests
    {
        private const int WarmupIterations = 10;

        public static void RunAllBenchmarks()
        {
            Debug.Log("\n=== Running Performance Benchmarks ===");
            Debug.Log("Note: Results may vary based on hardware and Unity version");

            BenchmarkQuestActivation();
            BenchmarkConditionEvaluation();
            BenchmarkPollingOverhead();
            BenchmarkLargeQuestHierarchy();
            BenchmarkMultipleSimultaneousQuests();

            Debug.Log("\n✓ All performance benchmarks complete!");
        }

        private static void BenchmarkQuestActivation()
        {
            Debug.Log("\n[BENCHMARK] Quest Activation Time");

            var gameObject = new GameObject("BenchmarkQuestManager");
            try
            {
                var playerRef = gameObject.AddComponent<QuestPlayerRef>();
                var questManager = gameObject.AddComponent<QuestManager>();
                
                // Setup via reflection
                var playerRefField = typeof(QuestManager).GetField("playerRef",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                playerRefField?.SetValue(questManager, playerRef);

                questManager.SendMessage("Awake");

                // Create a quest with 10 objectives
                var objectives = new List<ObjectiveAsset>();
                for (int i = 0; i < 10; i++)
                {
                    var objAsset = ObjectiveAsset.CreateForTest(
                        $"obj_{i}",
                        $"Objective {i}",
                        "Test objective",
                        false,
                        null,
                        CreateSimpleCondition($"item_{i}"),
                        null
                    );
                    objectives.Add(objAsset);
                }

                var questAsset = QuestAsset.CreateForTest(
                    "benchmark_quest",
                    "Benchmark Quest",
                    "Performance test quest",
                    objectives
                );

                // Warmup
                for (int i = 0; i < WarmupIterations; i++)
                {
                    var warmupState = questManager.StartQuest(questAsset);
                    questManager.StopQuest(warmupState);
                }

                // Benchmark
                var stopwatch = Stopwatch.StartNew();
                const int iterations = 1000;

                for (int i = 0; i < iterations; i++)
                {
                    var state = questManager.StartQuest(questAsset);
                    questManager.StopQuest(state);
                }

                stopwatch.Stop();
                double avgMs = stopwatch.Elapsed.TotalMilliseconds / iterations;

                Debug.Log($"   Average quest activation time: {avgMs:F3}ms ({iterations} iterations)");
                Debug.Log($"   Total time: {stopwatch.Elapsed.TotalMilliseconds:F2}ms");

                if (avgMs > 5.0) // Threshold: 5ms per quest activation
                {
                    Debug.LogWarning($"⚠️ Quest activation slower than expected: {avgMs:F3}ms > 5.0ms");
                }
                else
                {
                    Debug.Log("   ✓ Performance within acceptable range");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        private static void BenchmarkConditionEvaluation()
        {
            Debug.Log("\n[BENCHMARK] Condition Evaluation Speed");

            var eventManager = EventManager.Instance;
            var context = new QuestContext(null, null, null);

            // Create 100 item collection conditions
            var conditions = new List<ItemCollectedConditionInstance>();
            for (int i = 0; i < 100; i++)
            {
                var condition = new ItemCollectedConditionInstance($"item_{i}", 1);
                condition.Bind(eventManager, context, () => { });
                conditions.Add(condition);
            }

            // Warmup
            for (int i = 0; i < WarmupIterations; i++)
            {
                eventManager.Raise(new ItemCollectedEvent("item_0", 1));
            }

            // Benchmark: Raise 10000 events
            var stopwatch = Stopwatch.StartNew();
            const int eventCount = 10000;

            for (int i = 0; i < eventCount; i++)
            {
                eventManager.Raise(new ItemCollectedEvent($"item_{i % 100}", 1));
            }

            stopwatch.Stop();
            double avgMicroseconds = (stopwatch.Elapsed.TotalMilliseconds * 1000) / eventCount;

            Debug.Log($"   Average condition evaluation: {avgMicroseconds:F2}μs per event ({eventCount} events)");
            Debug.Log($"   Total time: {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
            Debug.Log($"   Throughput: {eventCount / stopwatch.Elapsed.TotalSeconds:F0} events/second");

            // Cleanup
            foreach (var condition in conditions)
            {
                condition.Unbind(eventManager, context);
            }

            if (avgMicroseconds > 100) // Threshold: 100μs per event
            {
                Debug.LogWarning($"⚠️ Condition evaluation slower than expected: {avgMicroseconds:F2}μs > 100μs");
            }
            else
            {
                Debug.Log("   ✓ Performance within acceptable range");
            }
        }

        private static void BenchmarkPollingOverhead()
        {
            Debug.Log("\n[BENCHMARK] Polling Overhead with Multiple Quests");

            var gameObject = new GameObject("BenchmarkQuestManager");
            try
            {
                var timeService = gameObject.AddComponent<DefaultTimeService>();
                var playerRef = gameObject.AddComponent<QuestPlayerRef>();
                
                // Setup time service via reflection
                var timeServiceField = typeof(QuestPlayerRef).GetField("timeService",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                timeServiceField?.SetValue(playerRef, timeService);

                var questManager = gameObject.AddComponent<QuestManager>();
                var playerRefField = typeof(QuestManager).GetField("playerRef",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                playerRefField?.SetValue(questManager, playerRef);

                questManager.SendMessage("Awake");

                // Create 50 quests with time-based conditions
                var activeQuests = new List<QuestState>();
                for (int i = 0; i < 50; i++)
                {
                    var timeConditionAsset = ScriptableObject.CreateInstance<TimeElapsedConditionAsset>();
                    var requiredSecondsField = typeof(TimeElapsedConditionAsset).GetField("requiredSeconds",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    requiredSecondsField?.SetValue(timeConditionAsset, 100.0f);

                    var objective = ObjectiveAsset.CreateForTest(
                        $"time_obj_{i}",
                        "Time Objective",
                        "Wait",
                        false,
                        null,
                        timeConditionAsset,
                        null
                    );

                    var questAsset = QuestAsset.CreateForTest(
                        $"time_quest_{i}",
                        "Time Quest",
                        "Test",
                        new List<ObjectiveAsset> { objective }
                    );

                    activeQuests.Add(questManager.StartQuest(questAsset));
                }

                // Warmup
                for (int i = 0; i < WarmupIterations; i++)
                {
                    CallPollConditions(questManager);
                }

                // Benchmark: Simulate polling cycle
                var stopwatch = Stopwatch.StartNew();
                const int pollCycles = 1000;

                for (int i = 0; i < pollCycles; i++)
                {
                    CallPollConditions(questManager);
                }

                stopwatch.Stop();
                double avgMs = stopwatch.Elapsed.TotalMilliseconds / pollCycles;

                Debug.Log($"   Average polling cycle (50 quests): {avgMs:F3}ms ({pollCycles} cycles)");
                Debug.Log($"   Total time: {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
                Debug.Log($"   Per-quest overhead: {avgMs / 50:F4}ms");

                // Cleanup
                foreach (var quest in activeQuests)
                {
                    questManager.StopQuest(quest);
                }

                if (avgMs > 2.0) // Threshold: 2ms per poll cycle with 50 quests
                {
                    Debug.LogWarning($"⚠️ Polling overhead higher than expected: {avgMs:F3}ms > 2.0ms");
                }
                else
                {
                    Debug.Log("   ✓ Performance within acceptable range");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        private static void BenchmarkLargeQuestHierarchy()
        {
            Debug.Log("\n[BENCHMARK] Large Quest Hierarchy (100 Objectives with Prerequisites)");

            var gameObject = new GameObject("BenchmarkQuestManager");
            try
            {
                var playerRef = gameObject.AddComponent<QuestPlayerRef>();
                var questManager = gameObject.AddComponent<QuestManager>();
                
                var playerRefField = typeof(QuestManager).GetField("playerRef",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                playerRefField?.SetValue(questManager, playerRef);

                questManager.SendMessage("Awake");

                // Create a quest with 100 objectives in a chain (each depends on previous)
                var objectives = new List<ObjectiveAsset>();
                for (int i = 0; i < 100; i++)
                {
                    var prerequisites = i > 0 ? new List<ObjectiveAsset> { objectives[i - 1] } : null;
                    
                    var objAsset = ObjectiveAsset.CreateForTest(
                        $"chain_obj_{i}",
                        $"Objective {i}",
                        "Test objective",
                        false,
                        prerequisites,
                        CreateSimpleCondition($"item_{i}"),
                        null
                    );
                    objectives.Add(objAsset);
                }

                var questAsset = QuestAsset.CreateForTest(
                    "large_quest",
                    "Large Quest",
                    "100 objectives",
                    objectives
                );

                // Benchmark: Quest activation with large hierarchy
                var stopwatch = Stopwatch.StartNew();
                var state = questManager.StartQuest(questAsset);
                stopwatch.Stop();

                Debug.Log($"   Quest activation time: {stopwatch.Elapsed.TotalMilliseconds:F3}ms");

                // Benchmark: Complete objectives sequentially
                stopwatch.Restart();
                var eventManager = EventManager.Instance;
                
                for (int i = 0; i < 100; i++)
                {
                    eventManager.Raise(new ItemCollectedEvent($"item_{i}", 1));
                    questManager.ProcessPendingEvaluations();
                }

                stopwatch.Stop();
                double avgMs = stopwatch.Elapsed.TotalMilliseconds / 100;

                Debug.Log($"   Average objective completion: {avgMs:F3}ms per objective");
                Debug.Log($"   Total quest completion time: {stopwatch.Elapsed.TotalMilliseconds:F2}ms");

                if (stopwatch.Elapsed.TotalMilliseconds > 100) // Threshold: 100ms total for 100 objectives
                {
                    Debug.LogWarning($"⚠️ Large hierarchy slower than expected: {stopwatch.Elapsed.TotalMilliseconds:F2}ms > 100ms");
                }
                else
                {
                    Debug.Log("   ✓ Performance within acceptable range");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        private static void BenchmarkMultipleSimultaneousQuests()
        {
            Debug.Log("\n[BENCHMARK] Multiple Simultaneous Quests (100 quests)");

            var gameObject = new GameObject("BenchmarkQuestManager");
            try
            {
                var playerRef = gameObject.AddComponent<QuestPlayerRef>();
                var questManager = gameObject.AddComponent<QuestManager>();
                
                var playerRefField = typeof(QuestManager).GetField("playerRef",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                playerRefField?.SetValue(questManager, playerRef);

                questManager.SendMessage("Awake");

                // Benchmark: Start 100 quests
                var stopwatch = Stopwatch.StartNew();
                var activeQuests = new List<QuestState>();

                for (int i = 0; i < 100; i++)
                {
                    var objective = ObjectiveAsset.CreateForTest(
                        $"obj_{i}",
                        $"Objective {i}",
                        "Test",
                        false,
                        null,
                        CreateSimpleCondition($"item_{i}"),
                        null
                    );

                    var questAsset = QuestAsset.CreateForTest(
                        $"quest_{i}",
                        $"Quest {i}",
                        "Test",
                        new List<ObjectiveAsset> { objective }
                    );

                    activeQuests.Add(questManager.StartQuest(questAsset));
                }

                stopwatch.Stop();
                Debug.Log($"   Time to start 100 quests: {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
                Debug.Log($"   Average per quest: {stopwatch.Elapsed.TotalMilliseconds / 100:F3}ms");

                // Benchmark: Complete all quests with single event burst
                stopwatch.Restart();
                var eventManager = EventManager.Instance;
                
                for (int i = 0; i < 100; i++)
                {
                    eventManager.Raise(new ItemCollectedEvent($"item_{i}", 1));
                }
                
                questManager.ProcessPendingEvaluations();
                stopwatch.Stop();

                Debug.Log($"   Time to complete 100 quests: {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
                Debug.Log($"   Average per quest: {stopwatch.Elapsed.TotalMilliseconds / 100:F3}ms");

                if (stopwatch.Elapsed.TotalMilliseconds > 50) // Threshold: 50ms for 100 quests
                {
                    Debug.LogWarning($"⚠️ Multiple quest handling slower than expected: {stopwatch.Elapsed.TotalMilliseconds:F2}ms > 50ms");
                }
                else
                {
                    Debug.Log("   ✓ Performance within acceptable range");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        // Helper methods

        private static ConditionAsset CreateSimpleCondition(string itemId)
        {
            var conditionAsset = ScriptableObject.CreateInstance<ItemCollectedConditionAsset>();
            var conditionIdField = typeof(ConditionAsset).GetField("conditionId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var requiredCountField = typeof(ItemCollectedConditionAsset).GetField("requiredCount",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            conditionIdField?.SetValue(conditionAsset, itemId);
            requiredCountField?.SetValue(conditionAsset, 1);
            
            return conditionAsset;
        }

        private static void CallPollConditions(QuestManager questManager)
        {
            var method = typeof(QuestManager).GetMethod("PollConditions",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(questManager, null);
        }
    }
}
