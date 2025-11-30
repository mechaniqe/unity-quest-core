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
        public static void RunAllContextTests()
        {
            Debug.Log("\n=== Running Quest Context Tests ===");
            TestServiceRegistration();
            TestServiceRetrieval();
            TestMultipleServiceTypes();
            TestServiceNotFound();
            TestRequiredServiceThrows();
            TestHasService();
            Debug.Log("✓ All context tests passed!");
        }

        private static void TestServiceRegistration()
        {
            Debug.Log("\n[TEST] Service Registration");

            // Arrange
            var timeService = new DefaultTimeService();
            var flagService = new DefaultFlagService();

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
            var timeService = new DefaultTimeService();
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
            var timeService = new DefaultTimeService();
            var flagService = new DefaultFlagService();
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
            var timeService = new DefaultTimeService();
            var context = new QuestContext(timeService: timeService);

            // Act & Assert
            if (!context.HasService<IQuestTimeService>())
                throw new Exception("HasService should return true for registered service");
            if (context.HasService<IQuestFlagService>())
                throw new Exception("HasService should return false for unregistered service");

            Debug.Log("✓ HasService check works correctly");
        }

        // Mock service for testing
        private class MockAreaService : IQuestAreaService
        {
            public string? CurrentAreaId => null;
            public bool HasEnteredArea(string areaId) => false;
            public bool IsInArea(string areaId) => false;
        }
    }
}
