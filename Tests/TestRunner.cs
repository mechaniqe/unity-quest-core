using System;
using UnityEngine;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Comprehensive test runner for all quest system tests.
    /// Can be invoked from unit test framework, standalone, or integration test setup.
    /// </summary>
    public static class TestRunner
    {
        /// <summary>
        /// Runs all unit tests (non-Unity dependent).
        /// </summary>
        public static void RunUnitTests()
        {
            Debug.Log("=== Running Quest System Unit Tests ===");
            try
            {
                QuestSystemTests.RunAllTests();
                ProgressReportingTests.RunAllProgressTests();
                QuestContextTests.RunAllContextTests();
                EventDrivenConditionTests.RunAllEventDrivenTests();
                FactoryMethodTests.RunAllFactoryMethodTests();
                ImmutableEventTests.RunAllImmutableEventTests();
                Debug.Log("=== Unit Tests Completed Successfully ===");
            }
            catch (Exception ex)
            {
                Debug.LogError($"=== Unit Tests Failed: {ex.Message} ===");
                throw;
            }
        }

        /// <summary>
        /// Runs performance benchmark tests.
        /// Note: These tests measure performance and may take longer to execute.
        /// </summary>
        public static void RunBenchmarks()
        {
            Debug.Log("=== Running Performance Benchmarks ===");
            try
            {
                PerformanceBenchmarkTests.RunAllBenchmarks();
                Debug.Log("=== Benchmarks Completed Successfully ===");
            }
            catch (Exception ex)
            {
                Debug.LogError($"=== Benchmarks Failed: {ex.Message} ===");
                throw;
            }
        }

        /// <summary>
        /// Main entry point - runs unit tests by default.
        /// For integration tests, use QuestSystemIntegrationTests component in Unity.
        /// For benchmarks, call RunBenchmarks() explicitly.
        /// </summary>
        public static void Main()
        {
            RunUnitTests();
        }

        /// <summary>
        /// Validates that all test classes are properly set up.
        /// </summary>
        public static bool ValidateTestSetup()
        {
            try
            {
                // Check if all required test classes exist
                var unitTestType = typeof(QuestSystemTests);
                // Note: QuestSystemIntegrationTests is a MonoBehaviour, so we can't instantiate it directly
                var integrationTestType = typeof(QuestSystemIntegrationTests);
                
                if (unitTestType == null)
                {
                    Debug.LogError("QuestSystemTests class not found");
                    return false;
                }
                
                if (integrationTestType == null)
                {
                    Debug.LogError("QuestSystemIntegrationTests class not found");
                    return false;
                }

                Debug.Log("✓ Test setup validation passed");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Test setup validation failed: {ex.Message}");
                return false;
            }
        }
    }
}
