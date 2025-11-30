#nullable enable
using System;
using System.Collections.Generic;
using DynamicBox.Quest.Core;
using UnityEngine;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Tests for CreateForTest factory methods on asset classes.
    /// Validates that factory methods create properly configured assets.
    /// </summary>
    public static class FactoryMethodTests
    {
        public static void RunAllFactoryMethodTests()
        {
            Debug.Log("\n=== Running Factory Method Tests ===");
            TestQuestAssetCreateForTest();
            TestObjectiveAssetCreateForTest();
            TestFactoryMethodsSetCorrectDefaults();
            TestFactoryMethodsHandleNullParameters();
            TestFactoryCreatedAssetsAreUsable();
            Debug.Log("✓ All factory method tests passed!");
        }

        private static void TestQuestAssetCreateForTest()
        {
            Debug.Log("\n[TEST] QuestAsset.CreateForTest");

            // Arrange
            var objectives = new List<ObjectiveAsset>();
            
            // Act
            var quest = QuestAsset.CreateForTest(
                questId: "test-quest-1",
                displayName: "Test Quest",
                description: "A test quest description",
                objectives: objectives
            );

            // Assert
            if (quest == null)
                throw new Exception("CreateForTest returned null");
            if (quest.QuestId != "test-quest-1")
                throw new Exception($"Expected questId 'test-quest-1', got '{quest.QuestId}'");
            if (quest.DisplayName != "Test Quest")
                throw new Exception($"Expected displayName 'Test Quest', got '{quest.DisplayName}'");
            if (quest.Description != "A test quest description")
                throw new Exception($"Expected description 'A test quest description', got '{quest.Description}'");
            if (quest.Objectives != objectives)
                throw new Exception("Objectives list not set correctly");

            Debug.Log("✓ QuestAsset.CreateForTest works correctly");
        }

        private static void TestObjectiveAssetCreateForTest()
        {
            Debug.Log("\n[TEST] ObjectiveAsset.CreateForTest");

            // Arrange
            var prerequisites = new List<ObjectiveAsset>();
            var completionCondition = ScriptableObject.CreateInstance<MockConditionAsset>();
            var failCondition = ScriptableObject.CreateInstance<MockConditionAsset>();

            // Act
            var objective = ObjectiveAsset.CreateForTest(
                objectiveId: "test-obj-1",
                title: "Test Objective",
                description: "An objective description",
                isOptional: true,
                prerequisites: prerequisites,
                completionCondition: completionCondition,
                failCondition: failCondition
            );

            // Assert
            if (objective == null)
                throw new Exception("CreateForTest returned null");
            if (objective.ObjectiveId != "test-obj-1")
                throw new Exception($"Expected objectiveId 'test-obj-1', got '{objective.ObjectiveId}'");
            if (objective.Title != "Test Objective")
                throw new Exception($"Expected title 'Test Objective', got '{objective.Title}'");
            if (objective.Description != "An objective description")
                throw new Exception($"Expected description 'An objective description', got '{objective.Description}'");
            if (!objective.IsOptional)
                throw new Exception("Expected isOptional true");
            if (objective.Prerequisites != prerequisites)
                throw new Exception("Prerequisites list not set correctly");
            if (objective.CompletionCondition != completionCondition)
                throw new Exception("CompletionCondition not set correctly");
            if (objective.FailCondition != failCondition)
                throw new Exception("FailCondition not set correctly");

            Debug.Log("✓ ObjectiveAsset.CreateForTest works correctly");
        }

        private static void TestFactoryMethodsSetCorrectDefaults()
        {
            Debug.Log("\n[TEST] Factory Methods Set Correct Defaults");

            // Act - Create with minimal parameters
            var quest = QuestAsset.CreateForTest("q1", "Quest", "Description", null!);
            var objective = ObjectiveAsset.CreateForTest("o1", "Objective", "Desc", false, null, null, null);

            // Assert
            if (quest.Objectives == null)
                throw new Exception("QuestAsset.Objectives should default to empty list, not null");
            if (quest.Objectives.Count != 0)
                throw new Exception($"Expected empty objectives list, got {quest.Objectives.Count} items");

            if (objective.Prerequisites == null)
                throw new Exception("ObjectiveAsset.Prerequisites should default to empty list, not null");
            if (objective.Prerequisites.Count != 0)
                throw new Exception($"Expected empty prerequisites list, got {objective.Prerequisites.Count} items");

            Debug.Log("✓ Factory methods set correct defaults");
        }

        private static void TestFactoryMethodsHandleNullParameters()
        {
            Debug.Log("\n[TEST] Factory Methods Handle Null Parameters");

            // Act - Pass explicit nulls to test null coalescing
            var quest = QuestAsset.CreateForTest("q1", "Quest", "Desc", null!);
            var objective = ObjectiveAsset.CreateForTest("o1", "Obj", "Desc", false, null, null, null);

            // Assert
            if (quest.Objectives == null)
                throw new Exception("Null objectives should be replaced with empty list");
            if (objective.Prerequisites == null)
                throw new Exception("Null prerequisites should be replaced with empty list");
            if (objective.CompletionCondition != null)
                throw new Exception("Null completionCondition should remain null");
            if (objective.FailCondition != null)
                throw new Exception("Null failCondition should remain null");

            Debug.Log("✓ Factory methods handle null parameters correctly");
        }

        private static void TestFactoryCreatedAssetsAreUsable()
        {
            Debug.Log("\n[TEST] Factory Created Assets Are Usable");

            // Arrange - Create a complete quest hierarchy using factory methods
            var completionCondition = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objective = ObjectiveAsset.CreateForTest(
                "obj-1",
                "Test Objective",
                "Complete the task",
                false,
                null,
                completionCondition,
                null
            );

            var quest = QuestAsset.CreateForTest(
                "quest-1",
                "Test Quest",
                "A quest created via factory",
                new List<ObjectiveAsset> { objective }
            );

            // Act - Try to use the quest (this is what matters for factory methods)
            var questState = new QuestState(quest);

            // Assert
            if (questState == null)
                throw new Exception("Factory-created quest should be usable via CreateInstance");
            if (questState.Definition.QuestId != "quest-1")
                throw new Exception("Quest instance has wrong ID");
            if (questState.Objectives.Count != 1)
                throw new Exception($"Expected 1 objective, got {questState.Objectives.Count}");
            
            var objectiveState = questState.Objectives["obj-1"];
            if (objectiveState.Definition.ObjectiveId != "obj-1")
                throw new Exception("Objective not properly instantiated");

            Debug.Log("✓ Factory created assets are fully usable");
        }
    }
}
