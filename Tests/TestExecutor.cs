using UnityEngine;
using System;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Simple test executor that can be attached to a GameObject to run tests in Unity.
    /// Useful for running tests from the Unity Inspector or through scripts.
    /// </summary>
    public class TestExecutor : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runOnStart = false;
        [SerializeField] private bool runValidation = true;
        [SerializeField] private bool runUnitTests = true;
        [SerializeField] private bool runSmokeTest = true;
        
        [Header("Results")]
        [SerializeField] private string lastTestResult = "Not run yet";
        
        void Start()
        {
            if (runOnStart)
            {
                RunSelectedTests();
            }
        }
        
        [ContextMenu("Run All Tests")]
        public void RunSelectedTests()
        {
            Debug.Log("=== Starting Quest System Test Execution ===");
            
            try
            {
                bool allPassed = true;
                
                if (runValidation)
                {
                    Debug.Log("Running test validation...");
                    if (!TestValidation.ValidateAllComponents())
                    {
                        allPassed = false;
                        Debug.LogError("Test validation failed!");
                    }
                }
                
                if (runSmokeTest)
                {
                    Debug.Log("Running smoke test...");
                    if (!TestValidation.RunSmokeTest())
                    {
                        allPassed = false;
                        Debug.LogError("Smoke test failed!");
                    }
                }
                
                if (runUnitTests)
                {
                    Debug.Log("Running unit tests...");
                    QuestSystemTests.RunAllTests();
                    ProgressReportingTests.RunAllProgressTests();
                    QuestContextTests.RunAllContextTests();
                    EventDrivenConditionTests.RunAllEventDrivenTests();
                    FactoryMethodTests.RunAllFactoryMethodTests();
                    ImmutableEventTests.RunAllImmutableEventTests();
                    Debug.Log("Unit tests completed successfully!");
                }
                
                if (allPassed)
                {
                    lastTestResult = $"✓ All tests passed! ({DateTime.Now:HH:mm:ss})";
                    Debug.Log($"<color=green>{lastTestResult}</color>");
                }
                else
                {
                    lastTestResult = $"✗ Some tests failed! ({DateTime.Now:HH:mm:ss})";
                    Debug.LogError(lastTestResult);
                }
            }
            catch (Exception ex)
            {
                lastTestResult = $"✗ Test execution failed: {ex.Message} ({DateTime.Now:HH:mm:ss})";
                Debug.LogError(lastTestResult);
                Debug.LogException(ex);
            }
        }
        
        [ContextMenu("Run Validation Only")]
        public void RunValidationOnly()
        {
            Debug.Log("Running test validation...");
            
            try
            {
                if (TestValidation.ValidateAllComponents())
                {
                    lastTestResult = $"✓ Validation passed! ({DateTime.Now:HH:mm:ss})";
                    Debug.Log($"<color=green>{lastTestResult}</color>");
                }
                else
                {
                    lastTestResult = $"✗ Validation failed! ({DateTime.Now:HH:mm:ss})";
                    Debug.LogError(lastTestResult);
                }
            }
            catch (Exception ex)
            {
                lastTestResult = $"✗ Validation failed: {ex.Message} ({DateTime.Now:HH:mm:ss})";
                Debug.LogError(lastTestResult);
                Debug.LogException(ex);
            }
        }
        
        [ContextMenu("Run Unit Tests Only")]
        public void RunUnitTestsOnly()
        {
            Debug.Log("Running unit tests...");
            
            try
            {
                QuestSystemTests.RunAllTests();
                lastTestResult = $"✓ Unit tests passed! ({DateTime.Now:HH:mm:ss})";
                Debug.Log($"<color=green>{lastTestResult}</color>");
            }
            catch (Exception ex)
            {
                lastTestResult = $"✗ Unit tests failed: {ex.Message} ({DateTime.Now:HH:mm:ss})";
                Debug.LogError(lastTestResult);
                Debug.LogException(ex);
            }
        }
        
        [ContextMenu("Run Smoke Test Only")]
        public void RunSmokeTestOnly()
        {
            Debug.Log("Running smoke test...");
            
            try
            {
                if (TestValidation.RunSmokeTest())
                {
                    lastTestResult = $"✓ Smoke test passed! ({DateTime.Now:HH:mm:ss})";
                    Debug.Log($"<color=green>{lastTestResult}</color>");
                }
                else
                {
                    lastTestResult = $"✗ Smoke test failed! ({DateTime.Now:HH:mm:ss})";
                    Debug.LogError(lastTestResult);
                }
            }
            catch (Exception ex)
            {
                lastTestResult = $"✗ Smoke test failed: {ex.Message} ({DateTime.Now:HH:mm:ss})";
                Debug.LogError(lastTestResult);
                Debug.LogException(ex);
            }
        }
    }
}
