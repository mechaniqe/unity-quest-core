using System;

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
            Console.WriteLine("=== Running Quest System Unit Tests ===");
            try
            {
                QuestSystemTests.RunAllTests();
                Console.WriteLine("=== Unit Tests Completed Successfully ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== Unit Tests Failed: {ex.Message} ===");
                throw;
            }
        }

        /// <summary>
        /// Main entry point - runs unit tests by default.
        /// For integration tests, use QuestSystemIntegrationTests component in Unity.
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
                    Console.WriteLine("ERROR: QuestSystemTests class not found");
                    return false;
                }
                
                if (integrationTestType == null)
                {
                    Console.WriteLine("ERROR: QuestSystemIntegrationTests class not found");
                    return false;
                }

                Console.WriteLine("✓ Test setup validation passed");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: Test setup validation failed: {ex.Message}");
                return false;
            }
        }
    }
}
