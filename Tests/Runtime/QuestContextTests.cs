#nullable enable
using System;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Services;
using UnityEngine;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Tests for QuestContext service locator functionality.
    /// Validates service registration, retrieval, and type safety.
    /// </summary>
    public static class QuestContextTests
    {
        // Helper methods to create MonoBehaviour services properly
        private static DefaultTimeService CreateTimeService()
        {
            var go = new GameObject("TestTimeService");
            return go.AddComponent<DefaultTimeService>();
        }

        private static DefaultFlagService CreateFlagService()
        {
            var go = new GameObject("TestFlagService");
            return go.AddComponent<DefaultFlagService>();
        }

        public static void RunAllContextTests()
        {
            Debug.Log("\n=== Running Quest Context Tests ===");
            TestServiceRegistration();
            TestServiceRetrieval();
            TestMultipleServiceTypes();
            TestServiceNotFound();
            TestRequiredServiceThrows();
            TestHasService();
            TestServiceTypeSafety();
            TestServiceRetrievalPerformance();
            TestNullServiceRegistration();
            TestConvenienceProperties();
            TestGetRequiredServiceSuccess();
            TestMultipleServiceInstances();
            TestServiceInterfacePolymorphism();
            Debug.Log("✓ All context tests passed!");
        }

        private static void TestServiceRegistration()
        {
            Debug.Log("\n[TEST] Service Registration");

            // Arrange
            var timeService = CreateTimeService();
            var flagService = CreateFlagService();

            // Act
            var context = new QuestContext(
                timeService: timeService,
                flagService: flagService
            );

            // Assert
            if (!ReferenceEquals(context.TimeService, timeService))
                throw new Exception("TimeService not registered correctly");
            if (!ReferenceEquals(context.FlagService, flagService))
                throw new Exception("FlagService not registered correctly");
            if (context.AreaService != null)
                throw new Exception("AreaService should be null");
            if (context.InventoryService != null)
                throw new Exception("InventoryService should be null");

            Debug.Log("✓ Service registration works correctly");
        }

        private static void TestServiceRetrieval()
        {
            Debug.Log("\n[TEST] Service Retrieval via GetService<T>");

            // Arrange
            var timeService = CreateTimeService();
            var context = new QuestContext(timeService: timeService);

            // Act
            var retrievedService = context.GetService<IQuestTimeService>();

            // Assert
            if (retrievedService == null)
                throw new Exception("GetService<T> returned null for registered service");
            if (!ReferenceEquals(retrievedService, timeService))
                throw new Exception("GetService<T> returned wrong service instance");

            Debug.Log("✓ Service retrieval via GetService<T> works correctly");
        }

        private static void TestMultipleServiceTypes()
        {
            Debug.Log("\n[TEST] Multiple Service Types");

            // Arrange
            var timeService = CreateTimeService();
            var flagService = CreateFlagService();
            var areaService = new MockAreaService();

            var context = new QuestContext(
                timeService: timeService,
                flagService: flagService,
                areaService: areaService
            );

            // Act & Assert
            var time = context.GetService<IQuestTimeService>();
            var flag = context.GetService<IQuestFlagService>();
            var area = context.GetService<IQuestAreaService>();

            if (!ReferenceEquals(time, timeService))
                throw new Exception("TimeService not retrieved correctly");
            if (!ReferenceEquals(flag, flagService))
                throw new Exception("FlagService not retrieved correctly");
            if (!ReferenceEquals(area, areaService))
                throw new Exception("AreaService not retrieved correctly");

            Debug.Log("✓ Multiple service types handled correctly");
        }

        private static void TestServiceNotFound()
        {
            Debug.Log("\n[TEST] Service Not Found Returns Null");

            // Arrange
            var context = new QuestContext(); // Empty context

            // Act
            var service = context.GetService<IQuestTimeService>();

            // Assert
            if (service != null)
                throw new Exception("GetService<T> should return null for unregistered service");

            Debug.Log("✓ Service not found returns null correctly");
        }

        private static void TestRequiredServiceThrows()
        {
            Debug.Log("\n[TEST] GetRequiredService Throws When Missing");

            // Arrange
            var context = new QuestContext(); // Empty context

            // Act & Assert
            try
            {
                var service = context.GetRequiredService<IQuestTimeService>();
                throw new Exception("GetRequiredService should have thrown InvalidOperationException");
            }
            catch (InvalidOperationException ex)
            {
                if (!ex.Message.Contains("Required service"))
                    throw new Exception($"Exception message incorrect: {ex.Message}");
                if (!ex.Message.Contains("IQuestTimeService"))
                    throw new Exception($"Exception should mention service type: {ex.Message}");
            }

            Debug.Log("✓ GetRequiredService throws correctly when service missing");
        }

        private static void TestHasService()
        {
            Debug.Log("\n[TEST] HasService Check");

            // Arrange
            var timeService = CreateTimeService();
            var context = new QuestContext(timeService: timeService);

            // Act & Assert
            if (!context.HasService<IQuestTimeService>())
                throw new Exception("HasService should return true for registered service");
            if (context.HasService<IQuestFlagService>())
                throw new Exception("HasService should return false for unregistered service");

            Debug.Log("✓ HasService check works correctly");
        }

        private static void TestServiceTypeSafety()
        {
            Debug.Log("\n[TEST] Service Type Safety");

            // Arrange
            var timeService = CreateTimeService();
            var flagService = CreateFlagService();
            var context = new QuestContext(
                timeService: timeService,
                flagService: flagService
            );

            // Act - Retrieve services with correct types
            var retrievedTime = context.GetService<IQuestTimeService>();
            var retrievedFlag = context.GetService<IQuestFlagService>();

            // Assert - Type safety should be maintained
            if (retrievedTime == null)
                throw new Exception("Time service should be retrievable");
            if (retrievedFlag == null)
                throw new Exception("Flag service should be retrievable");

            // Verify they are the correct instances
            if (!ReferenceEquals(retrievedTime, timeService))
                throw new Exception("Retrieved time service should be the same instance");
            if (!ReferenceEquals(retrievedFlag, flagService))
                throw new Exception("Retrieved flag service should be the same instance");

            Debug.Log("✓ Service type safety maintained correctly");
        }

        private static void TestServiceRetrievalPerformance()
        {
            Debug.Log("\n[TEST] Service Retrieval Performance");

            // Arrange
            var timeService = CreateTimeService();
            var flagService = CreateFlagService();
            var areaService = new MockAreaService();
            var inventoryService = new MockInventoryService();

            var context = new QuestContext(
                timeService: timeService,
                flagService: flagService,
                areaService: areaService,
                inventoryService: inventoryService
            );

            // Act - Retrieve services multiple times
            var startTime = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                var _ = context.GetService<IQuestTimeService>();
                var __ = context.GetService<IQuestFlagService>();
                var ___ = context.GetService<IQuestAreaService>();
                var ____ = context.GetService<IQuestInventoryService>();
            }
            startTime.Stop();

            // Assert - Should be fast (dictionary lookup)
            if (startTime.ElapsedMilliseconds > 100)
                Debug.LogWarning($"Service retrieval took {startTime.ElapsedMilliseconds}ms for 4000 lookups");

            Debug.Log($"✓ Service retrieval performance: {startTime.ElapsedMilliseconds}ms for 4000 lookups");
        }

        private static void TestNullServiceRegistration()
        {
            Debug.Log("\n[TEST] Null Service Registration");

            // Arrange & Act - Create context with some null services
            var timeService = CreateTimeService();
            var context = new QuestContext(
                timeService: timeService,
                flagService: null,
                areaService: null,
                inventoryService: null
            );

            // Assert - Only non-null services should be registered
            if (!context.HasService<IQuestTimeService>())
                throw new Exception("TimeService should be registered");
            if (context.HasService<IQuestFlagService>())
                throw new Exception("FlagService should not be registered (was null)");
            if (context.HasService<IQuestAreaService>())
                throw new Exception("AreaService should not be registered (was null)");
            if (context.HasService<IQuestInventoryService>())
                throw new Exception("InventoryService should not be registered (was null)");

            Debug.Log("✓ Null service registration handled correctly");
        }

        private static void TestConvenienceProperties()
        {
            Debug.Log("\n[TEST] Convenience Properties");

            // Arrange
            var timeService = CreateTimeService();
            var flagService = CreateFlagService();
            var areaService = new MockAreaService();
            var inventoryService = new MockInventoryService();

            var context = new QuestContext(
                timeService: timeService,
                flagService: flagService,
                areaService: areaService,
                inventoryService: inventoryService
            );

            // Act & Assert - Convenience properties should work
            if (!ReferenceEquals(context.TimeService, timeService))
                throw new Exception("TimeService property should return registered service");
            if (!ReferenceEquals(context.FlagService, flagService))
                throw new Exception("FlagService property should return registered service");
            if (!ReferenceEquals(context.AreaService, areaService))
                throw new Exception("AreaService property should return registered service");
            if (!ReferenceEquals(context.InventoryService, inventoryService))
                throw new Exception("InventoryService property should return registered service");

            // Test with null services
            var emptyContext = new QuestContext();
            if (emptyContext.TimeService != null)
                throw new Exception("TimeService property should return null when not registered");
            if (emptyContext.FlagService != null)
                throw new Exception("FlagService property should return null when not registered");

            Debug.Log("✓ Convenience properties work correctly");
        }

        private static void TestGetRequiredServiceSuccess()
        {
            Debug.Log("\n[TEST] GetRequiredService Success Case");

            // Arrange
            var timeService = CreateTimeService();
            var context = new QuestContext(timeService: timeService);

            // Act
            var retrievedService = context.GetRequiredService<IQuestTimeService>();

            // Assert
            if (retrievedService == null)
                throw new Exception("GetRequiredService should return non-null for registered service");
            if (!ReferenceEquals(retrievedService, timeService))
                throw new Exception("GetRequiredService should return correct instance");

            Debug.Log("✓ GetRequiredService success case works correctly");
        }

        private static void TestMultipleServiceInstances()
        {
            Debug.Log("\n[TEST] Multiple Service Instances");

            // Arrange - Create multiple contexts with different service instances
            var timeService1 = CreateTimeService();
            var timeService2 = CreateTimeService();

            var context1 = new QuestContext(timeService: timeService1);
            var context2 = new QuestContext(timeService: timeService2);

            // Act
            var retrieved1 = context1.GetService<IQuestTimeService>();
            var retrieved2 = context2.GetService<IQuestTimeService>();

            // Assert - Each context should maintain its own service instance
            if (!ReferenceEquals(retrieved1, timeService1))
                throw new Exception("Context1 should return its own service instance");
            if (!ReferenceEquals(retrieved2, timeService2))
                throw new Exception("Context2 should return its own service instance");
            if (ReferenceEquals(retrieved1, retrieved2))
                throw new Exception("Different contexts should have different service instances");

            Debug.Log("✓ Multiple service instances handled correctly");
        }

        private static void TestServiceInterfacePolymorphism()
        {
            Debug.Log("\n[TEST] Service Interface Polymorphism");

            // Arrange - Use concrete implementations
            var timeService = CreateTimeService();
            var flagService = CreateFlagService();

            // Act - Register concrete types but retrieve via interface
            var context = new QuestContext(
                timeService: timeService,
                flagService: flagService
            );

            var retrievedTime = context.GetService<IQuestTimeService>();
            var retrievedFlag = context.GetService<IQuestFlagService>();

            // Assert - Should work polymorphically
            if (retrievedTime == null)
                throw new Exception("Should retrieve service via interface");
            if (retrievedFlag == null)
                throw new Exception("Should retrieve service via interface");

            // Verify they are actually the concrete types
            if (!(retrievedTime is DefaultTimeService))
                throw new Exception("Retrieved service should be DefaultTimeService");
            if (!(retrievedFlag is DefaultFlagService))
                throw new Exception("Retrieved service should be DefaultFlagService");

            Debug.Log("✓ Service interface polymorphism works correctly");
        }

        // Mock services for testing
        private class MockAreaService : IQuestAreaService
        {
            public string? CurrentAreaId => null;
            public bool HasEnteredArea(string areaId) => false;
            public bool IsInArea(string areaId) => false;
        }

        private class MockInventoryService : IQuestInventoryService
        {
            public int GetItemCount(string itemId) => 0;
            public bool HasItem(string itemId, int quantity = 1) => false;
            public bool HasEverCollected(string itemId) => false;
        }
    }
}
