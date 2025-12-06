using System;
using System.Collections.Generic;
using UnityEngine;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Services;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Centralized helpers for test service creation and cleanup.
    /// Ensures all test GameObjects are properly tracked and destroyed to prevent memory leaks.
    /// </summary>
    public static class ServiceTestHelpers
    {
        private static readonly List<GameObject> _testGameObjects = new List<GameObject>();
        private static int _serviceCounter = 0;

        /// <summary>
        /// Creates a DefaultTimeService on a new GameObject.
        /// The GameObject is automatically tracked for cleanup.
        /// </summary>
        public static DefaultTimeService CreateTimeService(string customName = null)
        {
            var name = customName ?? $"TestTimeService_{++_serviceCounter}";
            var go = new GameObject(name);
            var service = go.AddComponent<DefaultTimeService>();
            TrackGameObject(go);
            return service;
        }

        /// <summary>
        /// Creates a DefaultFlagService on a new GameObject.
        /// The GameObject is automatically tracked for cleanup.
        /// </summary>
        public static DefaultFlagService CreateFlagService(string customName = null)
        {
            var name = customName ?? $"TestFlagService_{++_serviceCounter}";
            var go = new GameObject(name);
            var service = go.AddComponent<DefaultFlagService>();
            TrackGameObject(go);
            return service;
        }

        /// <summary>
        /// Creates a SimpleInventoryService on a new GameObject.
        /// The GameObject is automatically tracked for cleanup.
        /// </summary>
        public static SimpleInventoryService CreateInventoryService(string customName = null)
        {
            var name = customName ?? $"TestInventoryService_{++_serviceCounter}";
            var go = new GameObject(name);
            var service = go.AddComponent<SimpleInventoryService>();
            TrackGameObject(go);
            return service;
        }

        /// <summary>
        /// Creates a SimpleAreaService on a new GameObject.
        /// The GameObject is automatically tracked for cleanup.
        /// </summary>
        public static SimpleAreaService CreateAreaService(string customName = null)
        {
            var name = customName ?? $"TestAreaService_{++_serviceCounter}";
            var go = new GameObject(name);
            var service = go.AddComponent<SimpleAreaService>();
            TrackGameObject(go);
            return service;
        }

        /// <summary>
        /// Tracks a GameObject for automatic cleanup.
        /// Use this if you create GameObjects manually in tests.
        /// </summary>
        public static void TrackGameObject(GameObject go)
        {
            if (go != null && !_testGameObjects.Contains(go))
            {
                _testGameObjects.Add(go);
            }
        }

        /// <summary>
        /// Cleans up all tracked GameObjects.
        /// Call this in test cleanup or teardown methods.
        /// </summary>
        public static void CleanupAll()
        {
            foreach (var go in _testGameObjects)
            {
                if (go != null)
                {
                    UnityEngine.Object.DestroyImmediate(go);
                }
            }
            _testGameObjects.Clear();
            _serviceCounter = 0;
        }

        /// <summary>
        /// Wraps a test action with automatic cleanup.
        /// Use this to ensure cleanup happens even if the test throws an exception.
        /// </summary>
        /// <param name="testAction">The test code to execute</param>
        /// <param name="testName">Optional name for logging</param>
        public static void RunWithCleanup(Action testAction, string testName = null)
        {
            try
            {
                testAction();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(testName))
                {
                    Debug.LogError($"Test '{testName}' failed: {ex.Message}");
                }
                throw;
            }
            finally
            {
                CleanupAll();
            }
        }

        /// <summary>
        /// Gets count of currently tracked GameObjects.
        /// Useful for debugging memory leaks.
        /// </summary>
        public static int GetTrackedObjectCount()
        {
            // Remove null entries (objects destroyed externally)
            _testGameObjects.RemoveAll(go => go == null);
            return _testGameObjects.Count;
        }

        /// <summary>
        /// Validates that all tracked objects have been cleaned up.
        /// Returns true if no leaks detected, false otherwise.
        /// </summary>
        public static bool ValidateNoLeaks()
        {
            _testGameObjects.RemoveAll(go => go == null);
            
            if (_testGameObjects.Count > 0)
            {
                Debug.LogWarning($"Memory leak detected: {_testGameObjects.Count} GameObjects not cleaned up:");
                foreach (var go in _testGameObjects)
                {
                    Debug.LogWarning($"  - {go.name}");
                }
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Creates a QuestContext with all services initialized.
        /// All services are automatically tracked for cleanup.
        /// </summary>
        public static QuestContext CreateContextWithAllServices()
        {
            var timeService = CreateTimeService();
            var flagService = CreateFlagService();
            var inventoryService = CreateInventoryService();
            var areaService = CreateAreaService();

            return new QuestContext(
                timeService: timeService,
                flagService: flagService,
                inventoryService: inventoryService,
                areaService: areaService
            );
        }

        /// <summary>
        /// Creates a minimal QuestContext with only time and flag services.
        /// Services are automatically tracked for cleanup.
        /// </summary>
        public static QuestContext CreateMinimalContext()
        {
            var timeService = CreateTimeService();
            var flagService = CreateFlagService();

            return new QuestContext(
                timeService: timeService,
                flagService: flagService
            );
        }
    }
}
