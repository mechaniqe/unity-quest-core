using System;
using System.Collections.Generic;
using System.Linq;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Conditions;
using UnityEngine;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Tests for quest state serialization and persistence.
    /// Ensures quests can be saved and loaded correctly for game save systems.
    /// </summary>
    public static class QuestSerializationTests
    {
        public static void RunAllSerializationTests()
        {
            Debug.Log("\n=== Running Quest Serialization Tests ===");
            
            TestQuestStateJsonSerialization();
            TestObjectiveStateStatusSerialization();
            TestMultipleQuestsSerialization();
            TestPartialQuestProgressSerialization();
            TestSerializedDataIntegrity();
            TestDeserializationWithMissingData();
            TestSerializationPerformance();
            
            Debug.Log("✓ All serialization tests passed!");
        }

        private static void TestQuestStateJsonSerialization()
        {
            Debug.Log("\n[TEST] Quest State JSON Serialization");

            // Arrange - Create a quest with some progress
            var questAsset = CreateTestQuest();
            var questState = new QuestState(questAsset);
            questState.SetStatus(QuestStatus.InProgress);

            // Act - Serialize to JSON
            var snapshot = CaptureQuestSnapshot(questState);
            var json = JsonUtility.ToJson(snapshot, true);

            if (string.IsNullOrEmpty(json))
                throw new Exception("Serialization produced empty JSON");

            Debug.Log($"   Serialized JSON length: {json.Length} characters");

            // Deserialize back
            var restoredSnapshot = JsonUtility.FromJson<QuestStateSnapshot>(json);

            // Assert - Verify data integrity
            if (restoredSnapshot.QuestId != snapshot.QuestId)
                throw new Exception($"QuestId mismatch: {restoredSnapshot.QuestId} != {snapshot.QuestId}");
            if (restoredSnapshot.Status != snapshot.Status)
                throw new Exception($"Status mismatch: {restoredSnapshot.Status} != {snapshot.Status}");
            if (restoredSnapshot.ObjectiveStatuses.Count != snapshot.ObjectiveStatuses.Count)
                throw new Exception("Objective count mismatch");

            Debug.Log("✓ Quest state JSON serialization works correctly");
        }

        private static void TestObjectiveStateStatusSerialization()
        {
            Debug.Log("\n[TEST] Objective State Status Serialization");

            // Arrange - Create quest with multiple objectives in different states
            var questAsset = CreateTestQuest();
            var questState = new QuestState(questAsset);
            questState.SetStatus(QuestStatus.InProgress);

            var objectives = questState.GetObjectiveStates().ToList();
            if (objectives.Count >= 3)
            {
                objectives[0].SetStatus(ObjectiveStatus.Completed);
                objectives[1].SetStatus(ObjectiveStatus.InProgress);
                objectives[2].SetStatus(ObjectiveStatus.NotStarted);
            }

            // Act - Serialize and deserialize
            var snapshot = CaptureQuestSnapshot(questState);
            var json = JsonUtility.ToJson(snapshot);
            var restored = JsonUtility.FromJson<QuestStateSnapshot>(json);

            // Assert - Verify all objective statuses are preserved
            var restoredDict = restored.GetObjectiveStatusesDict();
            var snapshotDict = snapshot.GetObjectiveStatusesDict();
            
            foreach (var kvp in snapshotDict)
            {
                if (!restoredDict.TryGetValue(kvp.Key, out var restoredStatus))
                    throw new Exception($"Objective {kvp.Key} missing after deserialization");
                if (restoredStatus != kvp.Value)
                    throw new Exception($"Objective {kvp.Key} status mismatch: {restoredStatus} != {kvp.Value}");
            }

            Debug.Log($"✓ All {snapshot.ObjectiveStatuses.Count} objective statuses preserved correctly");
        }

        private static void TestMultipleQuestsSerialization()
        {
            Debug.Log("\n[TEST] Multiple Quests Serialization");

            // Arrange - Create multiple quests with different states
            var quests = new List<QuestStateSnapshot>();
            
            for (int i = 0; i < 5; i++)
            {
                var questAsset = QuestAsset.CreateForTest(
                    $"test_quest_{i}",
                    $"Test Quest {i}",
                    $"Description {i}",
                    new List<ObjectiveAsset>()
                );
                var questState = new QuestState(questAsset);
                
                // Vary the status
                questState.SetStatus(i % 3 == 0 ? QuestStatus.Completed :
                                   i % 3 == 1 ? QuestStatus.InProgress :
                                   QuestStatus.NotStarted);
                
                quests.Add(CaptureQuestSnapshot(questState));
            }

            // Act - Serialize all quests
            var container = new QuestSaveData { Quests = quests };
            var json = JsonUtility.ToJson(container, true);
            var restored = JsonUtility.FromJson<QuestSaveData>(json);

            // Assert - Verify all quests are preserved
            if (restored.Quests.Count != quests.Count)
                throw new Exception($"Quest count mismatch: {restored.Quests.Count} != {quests.Count}");

            for (int i = 0; i < quests.Count; i++)
            {
                if (restored.Quests[i].QuestId != quests[i].QuestId)
                    throw new Exception($"Quest {i} ID mismatch");
                if (restored.Quests[i].Status != quests[i].Status)
                    throw new Exception($"Quest {i} status mismatch");
            }

            Debug.Log($"✓ All {quests.Count} quests serialized and restored correctly");
        }

        private static void TestPartialQuestProgressSerialization()
        {
            Debug.Log("\n[TEST] Partial Quest Progress Serialization");

            // Arrange - Create quest and complete some objectives
            var objectives = new List<ObjectiveAsset>();
            for (int i = 0; i < 5; i++)
            {
                var objAsset = ObjectiveAsset.CreateForTest(
                    $"obj_{i}",
                    $"Objective {i}",
                    "Test objective",
                    false,
                    null,
                    CreateMockCondition(),
                    null
                );
                objectives.Add(objAsset);
            }

            var questAsset = QuestAsset.CreateForTest("test_quest", "Test Quest", "Test", objectives);
            var questState = new QuestState(questAsset);
            questState.SetStatus(QuestStatus.InProgress);

            // Complete first 3 objectives
            var objStates = questState.GetObjectiveStates().ToList();
            for (int i = 0; i < 3; i++)
            {
                objStates[i].SetStatus(ObjectiveStatus.Completed);
            }
            objStates[3].SetStatus(ObjectiveStatus.InProgress);
            objStates[4].SetStatus(ObjectiveStatus.NotStarted);

            // Act - Serialize and restore
            var snapshot = CaptureQuestSnapshot(questState);
            var json = JsonUtility.ToJson(snapshot);
            var restored = JsonUtility.FromJson<QuestStateSnapshot>(json);

            // Assert - Verify progress is preserved
            int completedCount = 0;
            int inProgressCount = 0;
            int notStartedCount = 0;

            foreach (var entry in restored.ObjectiveStatuses)
            {
                if (entry.Status == ObjectiveStatus.Completed) completedCount++;
                else if (entry.Status == ObjectiveStatus.InProgress) inProgressCount++;
                else if (entry.Status == ObjectiveStatus.NotStarted) notStartedCount++;
            }

            if (completedCount != 3)
                throw new Exception($"Expected 3 completed objectives, got {completedCount}");
            if (inProgressCount != 1)
                throw new Exception($"Expected 1 in-progress objective, got {inProgressCount}");
            if (notStartedCount != 1)
                throw new Exception($"Expected 1 not-started objective, got {notStartedCount}");

            Debug.Log($"✓ Partial progress preserved: {completedCount}/5 completed");
        }

        private static void TestSerializedDataIntegrity()
        {
            Debug.Log("\n[TEST] Serialized Data Integrity");

            // Arrange - Create quest with known data
            var questAsset = QuestAsset.CreateForTest(
                "integrity_test_quest",
                "Integrity Test Quest",
                "Testing data integrity",
                new List<ObjectiveAsset>()
            );
            var questState = new QuestState(questAsset);
            questState.SetStatus(QuestStatus.InProgress);

            // Act - Serialize multiple times and verify consistency
            var json1 = JsonUtility.ToJson(CaptureQuestSnapshot(questState));
            var json2 = JsonUtility.ToJson(CaptureQuestSnapshot(questState));

            // Assert - Multiple serializations should produce identical results
            if (json1 != json2)
                throw new Exception("Multiple serializations produced different results");

            // Verify deserialization is also consistent
            var restored1 = JsonUtility.FromJson<QuestStateSnapshot>(json1);
            var restored2 = JsonUtility.FromJson<QuestStateSnapshot>(json2);

            if (restored1.QuestId != restored2.QuestId)
                throw new Exception("Deserialization inconsistency in QuestId");
            if (restored1.Status != restored2.Status)
                throw new Exception("Deserialization inconsistency in Status");

            Debug.Log("✓ Serialization is deterministic and consistent");
        }

        private static void TestDeserializationWithMissingData()
        {
            Debug.Log("\n[TEST] Deserialization With Missing Data");

            // Arrange - Create JSON with minimal data
            var minimalJson = @"{
                ""QuestId"": ""minimal_quest"",
                ""Status"": 1,
                ""ObjectiveStatuses"": []
            }";

            // Act - Attempt to deserialize
            QuestStateSnapshot restored = null;
            try
            {
                restored = JsonUtility.FromJson<QuestStateSnapshot>(minimalJson);
            }
            catch (Exception ex)
            {
                throw new Exception($"Deserialization failed with minimal data: {ex.Message}");
            }

            // Assert - Should handle missing data gracefully
            if (restored == null)
                throw new Exception("Deserialization returned null");
            if (string.IsNullOrEmpty(restored.QuestId))
                throw new Exception("QuestId should not be empty");
            if (restored.ObjectiveStatuses == null)
                throw new Exception("ObjectiveStatuses should be initialized (empty list, not null)");

            Debug.Log("✓ Deserialization handles missing data gracefully");
        }

        private static void TestSerializationPerformance()
        {
            Debug.Log("\n[TEST] Serialization Performance");

            // Arrange - Create a large quest with many objectives
            var objectives = new List<ObjectiveAsset>();
            for (int i = 0; i < 50; i++)
            {
                objectives.Add(ObjectiveAsset.CreateForTest(
                    $"obj_{i}",
                    $"Objective {i}",
                    "Test",
                    false,
                    null,
                    CreateMockCondition(),
                    null
                ));
            }

            var questAsset = QuestAsset.CreateForTest("perf_test", "Performance Test", "Test", objectives);
            var questState = new QuestState(questAsset);
            questState.SetStatus(QuestStatus.InProgress);

            // Act - Benchmark serialization
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            const int iterations = 100;

            for (int i = 0; i < iterations; i++)
            {
                var snapshot = CaptureQuestSnapshot(questState);
                var json = JsonUtility.ToJson(snapshot);
                var restored = JsonUtility.FromJson<QuestStateSnapshot>(json);
            }

            stopwatch.Stop();
            double avgMs = stopwatch.Elapsed.TotalMilliseconds / iterations;

            Debug.Log($"   Average serialization time (50 objectives): {avgMs:F3}ms");
            Debug.Log($"   Total time for {iterations} iterations: {stopwatch.Elapsed.TotalMilliseconds:F2}ms");

            // Assert - Should be reasonably fast
            if (avgMs > 10.0)
            {
                Debug.LogWarning($"⚠️ Serialization slower than expected: {avgMs:F3}ms > 10ms");
            }
            else
            {
                Debug.Log("   ✓ Performance within acceptable range");
            }
        }

        // Helper methods

        private static QuestStateSnapshot CaptureQuestSnapshot(QuestState state)
        {
            var snapshot = new QuestStateSnapshot
            {
                QuestId = state.Definition.QuestId,
                Status = state.Status,
                ObjectiveStatuses = new List<ObjectiveStatusEntry>()
            };

            foreach (var obj in state.GetObjectiveStates())
            {
                snapshot.ObjectiveStatuses.Add(new ObjectiveStatusEntry
                {
                    ObjectiveId = obj.Definition.ObjectiveId,
                    Status = obj.Status
                });
            }

            return snapshot;
        }

        private static QuestAsset CreateTestQuest()
        {
            var objectives = new List<ObjectiveAsset>();
            for (int i = 0; i < 3; i++)
            {
                objectives.Add(ObjectiveAsset.CreateForTest(
                    $"test_obj_{i}",
                    $"Test Objective {i}",
                    "Test objective",
                    false,
                    null,
                    CreateMockCondition(),
                    null
                ));
            }

            return QuestAsset.CreateForTest(
                "test_quest",
                "Test Quest",
                "A test quest",
                objectives
            );
        }

        private static ConditionAsset CreateMockCondition()
        {
            var mockAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var conditionIdField = typeof(ConditionAsset).GetField("conditionId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            conditionIdField?.SetValue(mockAsset, "mock_condition");
            return mockAsset;
        }
    }

    /// <summary>
    /// Serializable snapshot of quest state for save/load systems.
    /// Contains only the data needed to restore quest progress.
    /// </summary>
    [Serializable]
    public class QuestStateSnapshot
    {
        public string QuestId;
        public QuestStatus Status;
        public List<ObjectiveStatusEntry> ObjectiveStatuses = new List<ObjectiveStatusEntry>();

        // Helper method to get objective status as dictionary
        public Dictionary<string, ObjectiveStatus> GetObjectiveStatusesDict()
        {
            var dict = new Dictionary<string, ObjectiveStatus>();
            foreach (var entry in ObjectiveStatuses)
            {
                dict[entry.ObjectiveId] = entry.Status;
            }
            return dict;
        }
    }

    /// <summary>
    /// Serializable entry for objective status (Unity JsonUtility doesn't support Dictionary).
    /// </summary>
    [Serializable]
    public class ObjectiveStatusEntry
    {
        public string ObjectiveId;
        public ObjectiveStatus Status;
    }

    /// <summary>
    /// Container for multiple quest snapshots (full save file).
    /// </summary>
    [Serializable]
    public class QuestSaveData
    {
        public List<QuestStateSnapshot> Quests = new List<QuestStateSnapshot>();
    }
}
