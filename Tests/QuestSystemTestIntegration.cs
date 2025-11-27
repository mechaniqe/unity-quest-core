using UnityEngine;
using DynamicBox.Quest.Tests;

/// <summary>
/// Example script showing how to integrate quest system tests into your project.
/// </summary>
public class QuestSystemTestIntegration : MonoBehaviour
{
    [Header("Test Configuration")]
    public bool runTestsOnAwake = false;
    public bool showDetailedResults = true;
    
    void Awake()
    {
        if (runTestsOnAwake)
        {
            RunQuestSystemTests();
        }
    }
    
    /// <summary>
    /// Call this method to run quest system validation in your code
    /// </summary>
    public bool RunQuestSystemTests()
    {
        Debug.Log("=== Quest System Test Integration ===");
        
        try
        {
            // Step 1: Validate infrastructure
            if (!TestValidation.ValidateAllComponents())
            {
                Debug.LogError("❌ Quest system test validation failed!");
                return false;
            }
            
            if (showDetailedResults)
                Debug.Log("✅ Test infrastructure validated");
            
            // Step 2: Run smoke test
            if (!TestValidation.RunSmokeTest())
            {
                Debug.LogError("❌ Quest system smoke test failed!");
                return false;
            }
            
            if (showDetailedResults)
                Debug.Log("✅ Smoke test passed");
            
            // Step 3: Run comprehensive unit tests
            QuestSystemTests.RunAllTests();
            
            Debug.Log("🎉 <color=green>All Quest System Tests Passed!</color>");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Quest system tests failed: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Quick validation check - useful for CI/CD or automated testing
    /// </summary>
    public bool QuickValidation()
    {
        return TestValidation.ValidateAllComponents() && TestValidation.RunSmokeTest();
    }
    
    // Context menu for easy access
    [ContextMenu("Run Quest Tests")]
    public void RunTestsFromContextMenu()
    {
        RunQuestSystemTests();
    }
    
    [ContextMenu("Quick Validation")]
    public void QuickValidationFromContextMenu()
    {
        bool result = QuickValidation();
        Debug.Log(result ? "✅ Quick validation passed" : "❌ Quick validation failed");
    }
}
