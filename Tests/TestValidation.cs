using System;
using UnityEngine;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Validation script to ensure all test components are properly set up.
    /// Can be used to verify test integrity before running comprehensive tests.
    /// Updated: 2025-11-26
    /// </summary>
    public static class TestValidation
    {
        /// <summary>
        /// Validates that all test components are properly configured.
        /// </summary>
        public static bool ValidateAllComponents()
        {
            Debug.Log("=== Quest System Test Validation ===");
            
            try
            {
                // Validate basic test setup
                if (!ValidateTestClasses())
                    return false;
                    
                // Validate mock components
                if (!ValidateMockComponents())
                    return false;
                    
                // Validate helper classes
                if (!ValidateHelperClasses())
                    return false;
                
                Debug.Log("✓ All test components validation passed!");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Test validation failed: {ex.Message}");
                return false;
            }
        }
        
        private static bool ValidateTestClasses()
        {
            Debug.Log("Validating test classes...");
            
            // Check QuestSystemTests
            var unitTestType = typeof(QuestSystemTests);
            if (unitTestType == null)
            {
                Debug.LogError("QuestSystemTests class not found");
                return false;
            }
            
            // Check QuestSystemIntegrationTests
            var integrationTestType = typeof(QuestSystemIntegrationTests);
            if (integrationTestType == null)
            {
                Debug.LogError("QuestSystemIntegrationTests class not found");
                return false;
            }
            
            // Check TestRunner
            var runnerType = typeof(TestRunner);
            if (runnerType == null)
            {
                Debug.LogError("TestRunner class not found");
                return false;
            }
            
            Debug.Log("✓ Test classes validation passed");
            return true;
        }
        
        private static bool ValidateMockComponents()
        {
            Debug.Log("Validating mock components...");
            
            try
            {
                // Test MockConditionAsset
                var mockAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
                if (mockAsset == null)
                {
                    Debug.LogError("MockConditionAsset creation failed");
                    return false;
                }
                
                var mockInstance = mockAsset.CreateInstance();
                if (mockInstance == null)
                {
                    Debug.LogError("MockConditionInstance creation failed");
                    return false;
                }
                
                Debug.Log("✓ Mock components validation passed");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Mock components validation failed: {ex.Message}");
                return false;
            }
        }
        
        private static bool ValidateHelperClasses()
        {
            Debug.Log("Validating helper classes...");
            
            try
            {
                // Test QuestBuilder
                var questBuilder = new QuestBuilder();
                if (questBuilder == null)
                {
                    Debug.LogError("QuestBuilder creation failed");
                    return false;
                }
                
                // Test ObjectiveBuilder
                var objectiveBuilder = new ObjectiveBuilder();
                if (objectiveBuilder == null)
                {
                    Debug.LogError("ObjectiveBuilder creation failed");
                    return false;
                }
                
                Debug.Log("✓ Helper classes validation passed");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Helper classes validation failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Runs a quick smoke test to verify basic functionality.
        /// </summary>
        public static bool RunSmokeTest()
        {
            Debug.Log("=== Running Quest System Smoke Test ===");
            
            try
            {
                // Create a simple quest with one objective
                var objective = new ObjectiveBuilder()
                    .WithObjectiveId("smoke_test_obj")
                    .Build();
                    
                var quest = new QuestBuilder()
                    .WithQuestId("smoke_test_quest")
                    .AddObjective(objective)
                    .Build();
                
                // Create quest state
                var questState = new DynamicBox.Quest.Core.QuestState(quest);
                
                if (questState == null)
                {
                    Debug.LogError("Quest state creation failed");
                    return false;
                }
                
                if (questState.Status != DynamicBox.Quest.Core.QuestStatus.NotStarted)
                {
                    Debug.LogError("Quest state initial status incorrect");
                    return false;
                }
                
                Debug.Log("✓ Smoke test passed!");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Smoke test failed: {ex.Message}");
                return false;
            }
        }
    }
}
