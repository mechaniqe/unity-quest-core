using System;
using System.Collections.Generic;
using System.Linq;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Conditions;
using DynamicBox.Quest.Core.State;
using UnityEngine;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Tests for QuestStateManager's restore functionality.
    /// Ensures quests can be properly loaded and re-initialized from snapshots.
    /// </summary>
    public static class QuestStateRestorationTests
    {
        public static void RunAllRestorationTests()
        {
            Debug.Log("\n=== Running Quest State Restoration Tests ===");
            
            TestRestoreBasicQuestState();
            TestRestoreWithObjectiveProgress();
            TestRestoreWithContextBinding();
            TestRestoreMultipleQuests();
            TestRestoreWithMissingObjectives();
            TestRestoreInvalidSnapshot();
            TestRestoreWithMismatchedQuestId();
            
            Debug.Log("✓ All restoration tests passed!");
        }

        private static void TestRestoreBasicQuestState()
        {
            Debug.Log("\n[TEST] Restore Basic Quest State");

            // Arrange - Create and capture a basic quest
            var questAsset = CreateTestQuest();
            var originalState = new QuestState(questAsset);
            originalState.SetStatus(QuestStatus.InProgress);
            
            var snapshot = QuestStateManager.CaptureSnapshot(originalState);
            var context = CreateTestContext();

            // Act - Restore from snapshot
            var restoredState = QuestStateManager.RestoreFromSnapshot(snapshot, questAsset, context);

            // Assert
            if (restoredState.Definition.QuestId != originalState.Definition.QuestId)
                throw new Exception("Quest ID mismatch after restoration");
            if (restoredState.Status != originalState.Status)
                throw new Exception("Quest status mismatch after restoration");

            Debug.Log("✓ Basic quest state restored correctly");
        }

        private static void TestRestoreWithObjectiveProgress()
        {
            Debug.Log("\n[TEST] Restore With Objective Progress");

            // Arrange - Create quest with partial progress
            var objectives = new List<ObjectiveAsset>();
            for (int i = 0; i < 5; i++)
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

            var questAsset = QuestAsset.CreateForTest("test_quest", "Test", "Test", objectives);
            var originalState = new QuestState(questAsset);
            originalState.SetStatus(QuestStatus.InProgress);

            // Complete some objectives
            var objStates = originalState.GetObjectiveStates().ToList();
            objStates[0].SetStatus(ObjectiveStatus.Completed);
            objStates[1].SetStatus(ObjectiveStatus.Completed);
            objStates[2].SetStatus(ObjectiveStatus.InProgress);

            var snapshot = QuestStateManager.CaptureSnapshot(originalState);
            var context = CreateTestContext();

            // Act - Restore
            var restoredState = QuestStateManager.RestoreFromSnapshot(snapshot, questAsset, context);

            // Assert - Verify objective progress is preserved
            var restoredObjs = restoredState.GetObjectiveStates().ToList();
            if (restoredObjs[0].Status != ObjectiveStatus.Completed)
                throw new Exception("Objective 0 status not restored");
            if (restoredObjs[1].Status != ObjectiveStatus.Completed)
                throw new Exception("Objective 1 status not restored");
            if (restoredObjs[2].Status != ObjectiveStatus.InProgress)
                throw new Exception("Objective 2 status not restored");
            if (restoredObjs[3].Status != ObjectiveStatus.NotStarted)
                throw new Exception("Objective 3 should remain NotStarted");

            Debug.Log("✓ Objective progress restored correctly");
        }

        private static void TestRestoreWithContextBinding()
        {
            Debug.Log("\n[TEST] Restore With Context Binding");

            // Arrange
            var questAsset = CreateTestQuest();
            var originalState = new QuestState(questAsset);
            originalState.SetStatus(QuestStatus.InProgress);
            
            var snapshot = QuestStateManager.CaptureSnapshot(originalState);
            var context = CreateTestContext();

            // Act - Restore from snapshot
            var restoredState = QuestStateManager.RestoreFromSnapshot(snapshot, questAsset, context);

            // Assert - Verify quest state is valid
            var isValid = true;
            try
            {
                // Verify the quest state structure is valid
                foreach (var obj in restoredState.GetObjectiveStates())
                {
                    // Accessing objectives verifies the state is properly restored
                    isValid = obj.Definition != null;
                }
            }
            catch (NullReferenceException)
            {
                throw new Exception("Quest state not properly restored");
            }

            if (!isValid)
                throw new Exception("Quest state not properly initialized");

            Debug.Log("✓ Quest state restored correctly (binding happens when added to QuestManager)");
        }

        private static void TestRestoreMultipleQuests()
        {
            Debug.Log("\n[TEST] Restore Multiple Quests");

            // Arrange - Create multiple quests with different states
            var questAssets = new Dictionary<string, QuestAsset>();
            var originalStates = new List<QuestState>();

            for (int i = 0; i < 5; i++)
            {
                var asset = QuestAsset.CreateForTest(
                    $"quest_{i}",
                    $"Quest {i}",
                    "Test",
                    new List<ObjectiveAsset>()
                );
                questAssets[asset.QuestId] = asset;

                var state = new QuestState(asset);
                state.SetStatus(i % 2 == 0 ? QuestStatus.Completed : QuestStatus.InProgress);
                originalStates.Add(state);
            }

            var saveData = QuestStateManager.CaptureAllSnapshots(originalStates, "Test save");
            var context = CreateTestContext();

            // Act - Restore all quests
            var restoredStates = QuestStateManager.RestoreAllFromSnapshots(saveData, questAssets, context);

            // Assert
            if (restoredStates.Count != originalStates.Count)
                throw new Exception($"Quest count mismatch: {restoredStates.Count} != {originalStates.Count}");

            for (int i = 0; i < originalStates.Count; i++)
            {
                if (restoredStates[i].Definition.QuestId != originalStates[i].Definition.QuestId)
                    throw new Exception($"Quest {i} ID mismatch");
                if (restoredStates[i].Status != originalStates[i].Status)
                    throw new Exception($"Quest {i} status mismatch");
            }

            Debug.Log($"✓ All {restoredStates.Count} quests restored correctly");
        }

        private static void TestRestoreWithMissingObjectives()
        {
            Debug.Log("\n[TEST] Restore With Missing Objectives");

            // Arrange - Create quest with 3 objectives
            var objectives = new List<ObjectiveAsset>();
            for (int i = 0; i < 3; i++)
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

            var questAsset = QuestAsset.CreateForTest("test_quest", "Test", "Test", objectives);
            var state = new QuestState(questAsset);
            
            var objStates = state.GetObjectiveStates().ToList();
            objStates[0].SetStatus(ObjectiveStatus.Completed);
            objStates[1].SetStatus(ObjectiveStatus.InProgress);

            var snapshot = QuestStateManager.CaptureSnapshot(state);

            // Remove one objective from the snapshot to simulate version mismatch
            snapshot.ObjectiveStatuses.RemoveAt(2);

            var context = CreateTestContext();

            // Act - Restore (should handle missing objective gracefully)
            var restoredState = QuestStateManager.RestoreFromSnapshot(snapshot, questAsset, context);

            // Assert - Should restore successfully, missing objective should be NotStarted
            var restoredObjs = restoredState.GetObjectiveStates().ToList();
            if (restoredObjs.Count != 3)
                throw new Exception("Should have all 3 objectives from asset");
            if (restoredObjs[0].Status != ObjectiveStatus.Completed)
                throw new Exception("Objective 0 should be completed");
            if (restoredObjs[1].Status != ObjectiveStatus.InProgress)
                throw new Exception("Objective 1 should be in progress");
            if (restoredObjs[2].Status != ObjectiveStatus.NotStarted)
                throw new Exception("Objective 2 should default to NotStarted");

            Debug.Log("✓ Missing objectives handled gracefully");
        }

        private static void TestRestoreInvalidSnapshot()
        {
            Debug.Log("\n[TEST] Restore Invalid Snapshot");

            // Arrange
            var questAsset = CreateTestQuest();
            var context = CreateTestContext();
            var invalidSnapshot = new QuestStateSnapshot(); // Missing QuestId

            // Act & Assert - Should throw exception
            try
            {
                QuestStateManager.RestoreFromSnapshot(invalidSnapshot, questAsset, context);
                throw new Exception("Should have thrown exception for invalid snapshot");
            }
            catch (ArgumentException ex)
            {
                if (!ex.Message.Contains("Invalid"))
                    throw new Exception($"Wrong exception message: {ex.Message}");
            }

            Debug.Log("✓ Invalid snapshot properly rejected");
        }

        private static void TestRestoreWithMismatchedQuestId()
        {
            Debug.Log("\n[TEST] Restore With Mismatched Quest ID");

            // Arrange
            var questAsset1 = QuestAsset.CreateForTest("quest_1", "Quest 1", "Test", new List<ObjectiveAsset>());
            var questAsset2 = QuestAsset.CreateForTest("quest_2", "Quest 2", "Test", new List<ObjectiveAsset>());
            
            var state1 = new QuestState(questAsset1);
            var snapshot1 = QuestStateManager.CaptureSnapshot(state1);
            var context = CreateTestContext();

            // Act & Assert - Try to restore quest 1 snapshot with quest 2 asset
            try
            {
                QuestStateManager.RestoreFromSnapshot(snapshot1, questAsset2, context);
                throw new Exception("Should have thrown exception for mismatched quest ID");
            }
            catch (ArgumentException ex)
            {
                if (!ex.Message.Contains("doesn't match"))
                    throw new Exception($"Wrong exception message: {ex.Message}");
            }

            Debug.Log("✓ Mismatched quest ID properly detected");
        }

        // Helper methods

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

        private static QuestContext CreateTestContext()
        {
            // Use ServiceTestHelpers for memory-safe service creation
            return ServiceTestHelpers.CreateContextWithAllServices();
        }
    }
}
