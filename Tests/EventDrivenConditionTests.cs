#nullable enable
using System;
using DynamicBox.EventManagement;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.GameEvents;
using UnityEngine;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// Tests for EventDrivenConditionBase abstract class behavior.
    /// Validates base class functionality using concrete implementations.
    /// </summary>
    public static class EventDrivenConditionTests
    {
        public static void RunAllEventDrivenTests()
        {
            Debug.Log("\n=== Running Event Driven Condition Tests ===");
            TestBindSubscribesToEvents();
            TestUnbindUnsubscribesFromEvents();
            TestHandleEventCalledOnEventRaised();
            TestNotifyChangedInvokesCallback();
            TestMultipleBindUnbindCycles();
            TestOnBindOnUnbindLifecycle();
            Debug.Log("✓ All event driven condition tests passed!");
        }

        private static void TestBindSubscribesToEvents()
        {
            Debug.Log("\n[TEST] Bind Subscribes To Events");

            // Arrange
            var eventBus = new SimpleEventBus();
            var context = new QuestContext();
            var condition = new TestEventDrivenCondition();
            bool callbackInvoked = false;

            // Act
            condition.Bind(eventBus, context, () => callbackInvoked = true);
            eventBus.Publish(new TestGameEvent("test"));

            // Assert - Event should be received and processed
            if (!condition.EventReceived)
                throw new Exception("Event was not received after Bind");
            if (condition.ReceivedEventData != "test")
                throw new Exception($"Expected event data 'test', got '{condition.ReceivedEventData}'");
            
            // Note: Callback is only invoked when condition calls NotifyChanged()
            // This test verifies event subscription works, not callback invocation
            if (callbackInvoked) { } // Used to suppress warning

            Debug.Log("✓ Bind subscribes to events correctly");
        }

        private static void TestUnbindUnsubscribesFromEvents()
        {
            Debug.Log("\n[TEST] Unbind Unsubscribes From Events");

            // Arrange
            var eventBus = new SimpleEventBus();
            var context = new QuestContext();
            var condition = new TestEventDrivenCondition();
            bool callbackInvoked = false;

            // Act
            condition.Bind(eventBus, context, () => callbackInvoked = true);
            if (!callbackInvoked) { } // Suppress unused warning - callback is tested implicitly
            condition.Unbind(eventBus, context);
            
            // Reset state and trigger event
            condition.Reset();
            eventBus.Publish(new TestGameEvent("after-unbind"));

            // Assert
            if (condition.EventReceived)
                throw new Exception("Event was received after Unbind");

            Debug.Log("✓ Unbind unsubscribes from events correctly");
        }

        private static void TestHandleEventCalledOnEventRaised()
        {
            Debug.Log("\n[TEST] HandleEvent Called On Event Raised");

            // Arrange
            var eventBus = new SimpleEventBus();
            var context = new QuestContext();
            var condition = new TestEventDrivenCondition();

            // Act
            condition.Bind(eventBus, context, () => { });
            eventBus.Publish(new TestGameEvent("data1"));
            eventBus.Publish(new TestGameEvent("data2"));

            // Assert
            if (!condition.EventReceived)
                throw new Exception("HandleEvent was not called");
            if (condition.HandleEventCallCount != 2)
                throw new Exception($"Expected HandleEvent called 2 times, got {condition.HandleEventCallCount}");
            if (condition.ReceivedEventData != "data2")
                throw new Exception($"Expected last event data 'data2', got '{condition.ReceivedEventData}'");

            Debug.Log("✓ HandleEvent called correctly on event raised");
        }

        private static void TestNotifyChangedInvokesCallback()
        {
            Debug.Log("\n[TEST] NotifyChanged Invokes Callback");

            // Arrange
            var eventBus = new SimpleEventBus();
            var context = new QuestContext();
            var condition = new TestEventDrivenCondition();
            int callbackCount = 0;

            // Act
            condition.Bind(eventBus, context, () => callbackCount++);
            condition.TriggerNotifyChanged(); // Direct call to NotifyChanged
            condition.TriggerNotifyChanged();

            // Assert
            if (callbackCount != 2)
                throw new Exception($"Expected callback invoked 2 times, got {callbackCount}");

            Debug.Log("✓ NotifyChanged invokes callback correctly");
        }

        private static void TestMultipleBindUnbindCycles()
        {
            Debug.Log("\n[TEST] Multiple Bind/Unbind Cycles");

            // Arrange
            var eventBus = new SimpleEventBus();
            var context = new QuestContext();
            var condition = new TestEventDrivenCondition();

            // Act & Assert - Cycle 1
            condition.Bind(eventBus, context, () => { });
            eventBus.Publish(new TestGameEvent("cycle1"));
            if (!condition.EventReceived || condition.ReceivedEventData != "cycle1")
                throw new Exception("Cycle 1 failed");

            condition.Unbind(eventBus, context);
            condition.Reset();

            // Act & Assert - Cycle 2
            condition.Bind(eventBus, context, () => { });
            eventBus.Publish(new TestGameEvent("cycle2"));
            if (!condition.EventReceived || condition.ReceivedEventData != "cycle2")
                throw new Exception("Cycle 2 failed");

            condition.Unbind(eventBus, context);
            condition.Reset();

            // Act & Assert - Cycle 3
            condition.Bind(eventBus, context, () => { });
            eventBus.Publish(new TestGameEvent("cycle3"));
            if (!condition.EventReceived || condition.ReceivedEventData != "cycle3")
                throw new Exception("Cycle 3 failed");

            Debug.Log("✓ Multiple bind/unbind cycles work correctly");
        }

        private static void TestOnBindOnUnbindLifecycle()
        {
            Debug.Log("\n[TEST] OnBind/OnUnbind Lifecycle Hooks");

            // Arrange
            var eventBus = new SimpleEventBus();
            var context = new QuestContext();
            var condition = new TestEventDrivenCondition();

            // Act
            condition.Bind(eventBus, context, () => { });

            // Assert
            if (!condition.OnBindCalled)
                throw new Exception("OnBind was not called during Bind");
            if (condition.OnUnbindCalled)
                throw new Exception("OnUnbind should not be called before Unbind");

            // Act
            condition.Unbind(eventBus, context);

            // Assert
            if (!condition.OnUnbindCalled)
                throw new Exception("OnUnbind was not called during Unbind");

            Debug.Log("✓ OnBind/OnUnbind lifecycle hooks work correctly");
        }

        // Test implementation of EventDrivenConditionBase
        private class TestEventDrivenCondition : EventDrivenConditionBase<TestGameEvent>
        {
            public bool EventReceived { get; private set; }
            public string? ReceivedEventData { get; private set; }
            public int HandleEventCallCount { get; private set; }
            public bool OnBindCalled { get; private set; }
            public bool OnUnbindCalled { get; private set; }
            private bool _isMet;

            public override bool IsMet => _isMet;

            protected override void HandleEvent(TestGameEvent evt)
            {
                EventReceived = true;
                ReceivedEventData = evt.Data;
                HandleEventCallCount++;
            }

            protected override void OnBind(QuestContext context)
            {
                OnBindCalled = true;
            }

            protected override void OnUnbind(QuestContext context)
            {
                OnUnbindCalled = true;
            }

            public void TriggerNotifyChanged()
            {
                _isMet = true;
                NotifyChanged();
            }

            public void Reset()
            {
                EventReceived = false;
                ReceivedEventData = null;
                HandleEventCallCount = 0;
            }
        }

        // Test event type
        private class TestGameEvent : GameEvent
        {
            public string Data { get; }

            public TestGameEvent(string data)
            {
                Data = data;
            }
        }
    }
}
