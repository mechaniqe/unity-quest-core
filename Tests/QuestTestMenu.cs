using UnityEngine;
using UnityEditor;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Unity Editor menu items for running quest system tests.
    /// Provides easy access to test execution from Unity's top menu.
    /// </summary>
    public static class QuestTestMenu
    {
        // [MenuItem("Tools/DynamicBox/Quest System/Run All Tests")]
        public static void RunAllTests()
        {
            Debug.Log("=== Running Quest System Tests from Menu ===");
            
            try
            {
                if (TestValidation.ValidateAllComponents())
                {
                    QuestSystemTests.RunAllTests();
                    Debug.Log("<color=green>🎉 All Quest System Tests Passed!</color>");
                }
                else
                {
                    Debug.LogError("❌ Test validation failed - check setup");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Test execution failed: {ex.Message}");
            }
        }
        
        // [MenuItem("Tools/DynamicBox/Quest System/Validate Test Setup")]
        public static void ValidateSetup()
        {
            Debug.Log("=== Validating Quest System Test Setup ===");
            
            bool validation = TestValidation.ValidateAllComponents();
            bool smokeTest = TestValidation.RunSmokeTest();
            
            if (validation && smokeTest)
            {
                Debug.Log("<color=green>✅ Quest System test setup is valid and ready!</color>");
            }
            else
            {
                Debug.LogError("❌ Quest System test setup has issues - check console for details");
            }
        }
        
        // [MenuItem("Tools/DynamicBox/Quest System/Run Quick Smoke Test")]
        public static void RunSmokeTest()
        {
            Debug.Log("=== Running Quest System Smoke Test ===");
            
            if (TestValidation.RunSmokeTest())
            {
                Debug.Log("<color=green>✅ Smoke test passed!</color>");
            }
            else
            {
                Debug.LogError("❌ Smoke test failed - check console for details");
            }
        }
        
        // [MenuItem("Tools/DynamicBox/Quest System/Open Test Documentation")]
        public static void OpenTestDocs()
        {
            string[] docFiles = {
                "Assets/unity-quest-core/Tests/TEST_GUIDE.md",
                "Assets/unity-quest-core/Tests/TEST_COVERAGE.md",
                "Assets/unity-quest-core/Tests/FINAL_STATUS_REPORT.md"
            };
            
            foreach (string docFile in docFiles)
            {
                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(docFile);
                if (asset != null)
                {
                    AssetDatabase.OpenAsset(asset);
                    break;
                }
            }
        }
    }
}
