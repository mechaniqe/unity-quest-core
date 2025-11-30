#nullable enable
using System;
using DynamicBox.Quest.GameEvents;
using UnityEngine;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Tests for immutable event classes.
    /// Validates that events are properly sealed and immutable following CQRS best practices.
    /// </summary>
    public static class ImmutableEventTests
    {
        public static void RunAllImmutableEventTests()
        {
            Debug.Log("\n=== Running Immutable Event Tests ===");
            TestItemCollectedEventIsSealed();
            TestItemCollectedEventPropertiesAreReadonly();
            TestAreaEnteredEventIsSealed();
            TestAreaEnteredEventPropertiesAreReadonly();
            TestFlagChangedEventIsSealed();
            TestFlagChangedEventPropertiesAreReadonly();
            TestEventClassesFollowCQRSPattern();
            Debug.Log("✓ All immutable event tests passed!");
        }

        private static void TestItemCollectedEventIsSealed()
        {
            Debug.Log("\n[TEST] ItemCollectedEvent Is Sealed");

            // Arrange
            var eventType = typeof(ItemCollectedEvent);

            // Act & Assert
            if (!eventType.IsSealed)
                throw new Exception("ItemCollectedEvent should be sealed to prevent inheritance");

            Debug.Log("✓ ItemCollectedEvent is sealed");
        }

        private static void TestItemCollectedEventPropertiesAreReadonly()
        {
            Debug.Log("\n[TEST] ItemCollectedEvent Properties Are Readonly");

            // Arrange
            var evt = new ItemCollectedEvent("gold-coin", 5);

            // Act - Try to access properties
            var itemId = evt.ItemId;
            var amount = evt.Amount;

            // Assert
            if (itemId != "gold-coin")
                throw new Exception($"Expected ItemId 'gold-coin', got '{itemId}'");
            if (amount != 5)
                throw new Exception($"Expected Amount 5, got {amount}");

            // Verify properties have getters only (no setters)
            var itemIdProperty = typeof(ItemCollectedEvent).GetProperty(nameof(ItemCollectedEvent.ItemId));
            var amountProperty = typeof(ItemCollectedEvent).GetProperty(nameof(ItemCollectedEvent.Amount));

            if (itemIdProperty?.SetMethod != null)
                throw new Exception("ItemId should be readonly (no setter)");
            if (amountProperty?.SetMethod != null)
                throw new Exception("Amount should be readonly (no setter)");

            Debug.Log("✓ ItemCollectedEvent properties are readonly");
        }

        private static void TestAreaEnteredEventIsSealed()
        {
            Debug.Log("\n[TEST] AreaEnteredEvent Is Sealed");

            // Arrange
            var eventType = typeof(AreaEnteredEvent);

            // Act & Assert
            if (!eventType.IsSealed)
                throw new Exception("AreaEnteredEvent should be sealed to prevent inheritance");

            Debug.Log("✓ AreaEnteredEvent is sealed");
        }

        private static void TestAreaEnteredEventPropertiesAreReadonly()
        {
            Debug.Log("\n[TEST] AreaEnteredEvent Properties Are Readonly");

            // Arrange
            var position = new Vector3(10f, 20f, 30f);
            var evt = new AreaEnteredEvent("forest-zone", position, "The Dark Forest");

            // Act
            var areaId = evt.AreaId;
            var pos = evt.Position;
            var areaName = evt.AreaName;

            // Assert
            if (areaId != "forest-zone")
                throw new Exception($"Expected AreaId 'forest-zone', got '{areaId}'");
            if (pos != position)
                throw new Exception($"Expected Position {position}, got {pos}");
            if (areaName != "The Dark Forest")
                throw new Exception($"Expected AreaName 'The Dark Forest', got '{areaName}'");

            // Verify properties have getters only
            var areaIdProperty = typeof(AreaEnteredEvent).GetProperty(nameof(AreaEnteredEvent.AreaId));
            var positionProperty = typeof(AreaEnteredEvent).GetProperty(nameof(AreaEnteredEvent.Position));
            var areaNameProperty = typeof(AreaEnteredEvent).GetProperty(nameof(AreaEnteredEvent.AreaName));

            if (areaIdProperty?.SetMethod != null)
                throw new Exception("AreaId should be readonly (no setter)");
            if (positionProperty?.SetMethod != null)
                throw new Exception("Position should be readonly (no setter)");
            if (areaNameProperty?.SetMethod != null)
                throw new Exception("AreaName should be readonly (no setter)");

            Debug.Log("✓ AreaEnteredEvent properties are readonly");
        }

        private static void TestFlagChangedEventIsSealed()
        {
            Debug.Log("\n[TEST] FlagChangedEvent Is Sealed");

            // Arrange
            var eventType = typeof(FlagChangedEvent);

            // Act & Assert
            if (!eventType.IsSealed)
                throw new Exception("FlagChangedEvent should be sealed to prevent inheritance");

            Debug.Log("✓ FlagChangedEvent is sealed");
        }

        private static void TestFlagChangedEventPropertiesAreReadonly()
        {
            Debug.Log("\n[TEST] FlagChangedEvent Properties Are Readonly");

            // Arrange
            var evt = new FlagChangedEvent("boss-defeated", true);

            // Act
            var flagId = evt.FlagId;
            var value = evt.NewValue;

            // Assert
            if (flagId != "boss-defeated")
                throw new Exception($"Expected FlagId 'boss-defeated', got '{flagId}'");
            if (!value)
                throw new Exception($"Expected NewValue true, got {value}");

            // Verify properties have getters only
            var flagIdProperty = typeof(FlagChangedEvent).GetProperty(nameof(FlagChangedEvent.FlagId));
            var valueProperty = typeof(FlagChangedEvent).GetProperty(nameof(FlagChangedEvent.NewValue));

            if (flagIdProperty?.SetMethod != null)
                throw new Exception("FlagId should be readonly (no setter)");
            if (valueProperty?.SetMethod != null)
                throw new Exception("NewValue should be readonly (no setter)");

            Debug.Log("✓ FlagChangedEvent properties are readonly");
        }

        private static void TestEventClassesFollowCQRSPattern()
        {
            Debug.Log("\n[TEST] Event Classes Follow CQRS Pattern");

            // CQRS Pattern Requirements:
            // 1. Events are immutable (sealed class, readonly properties)
            // 2. Events have all data set in constructor
            // 3. Events don't expose behavior, only data
            // 4. Events are snapshots of what happened

            // Arrange & Act
            var itemEvent = new ItemCollectedEvent("sword", 1);
            var areaEvent = new AreaEnteredEvent("castle", Vector3.zero);
            var flagEvent = new FlagChangedEvent("quest-started", true);

            // Assert - All events should be creatable with constructor data
            if (itemEvent.ItemId != "sword" || itemEvent.Amount != 1)
                throw new Exception("ItemCollectedEvent doesn't properly capture constructor data");
            if (areaEvent.AreaId != "castle")
                throw new Exception("AreaEnteredEvent doesn't properly capture constructor data");
            if (flagEvent.FlagId != "quest-started" || !flagEvent.NewValue)
                throw new Exception("FlagChangedEvent doesn't properly capture constructor data");

            // Verify no public methods other than inherited ones
            var itemMethods = typeof(ItemCollectedEvent).GetMethods(
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.DeclaredOnly);
            
            if (itemMethods.Length > 0)
            {
                // Filter out property getters
                var nonPropertyMethods = System.Linq.Enumerable.Where(itemMethods, 
                    m => !m.Name.StartsWith("get_"));
                if (System.Linq.Enumerable.Any(nonPropertyMethods))
                    throw new Exception("Event classes should not expose behavior methods");
            }

            Debug.Log("✓ Event classes follow CQRS pattern correctly");
        }
    }
}
