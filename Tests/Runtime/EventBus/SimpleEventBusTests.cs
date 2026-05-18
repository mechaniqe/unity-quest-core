using System;
using NUnit.Framework;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Tests.EventBus
{
    [TestFixture]
    public class SimpleEventBusTests
    {
        private struct TestEvent { public int Value; }
        private struct OtherEvent { public string Data; }

        [Test]
        public void Subscribe_ThenPublish_HandlerInvoked()
        {
            var bus = new SimpleEventBus();
            int received = 0;
            bus.Subscribe<TestEvent>(e => received = e.Value);

            bus.Publish(new TestEvent { Value = 42 });

            Assert.That(received, Is.EqualTo(42));
        }

        [Test]
        public void Subscribe_MultipleHandlers_AllInvoked()
        {
            var bus = new SimpleEventBus();
            int count = 0;
            bus.Subscribe<TestEvent>(_ => count++);
            bus.Subscribe<TestEvent>(_ => count++);

            bus.Publish(new TestEvent());

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void Unsubscribe_AfterSubscribe_HandlerNotInvoked()
        {
            var bus = new SimpleEventBus();
            int received = 0;
            Action<TestEvent> handler = _ => received++;

            bus.Subscribe(handler);
            bus.Unsubscribe(handler);
            bus.Publish(new TestEvent());

            Assert.That(received, Is.EqualTo(0));
        }

        [Test]
        public void Publish_WithNoSubscribers_DoesNotThrow()
        {
            var bus = new SimpleEventBus();
            Assert.DoesNotThrow(() => bus.Publish(new TestEvent()));
        }

        [Test]
        public void Publish_DifferentEventTypes_OnlyMatchingHandlerInvoked()
        {
            var bus = new SimpleEventBus();
            bool testFired = false;
            bool otherFired = false;

            bus.Subscribe<TestEvent>(_ => testFired = true);
            bus.Subscribe<OtherEvent>(_ => otherFired = true);

            bus.Publish(new TestEvent());

            Assert.That(testFired, Is.True);
            Assert.That(otherFired, Is.False);
        }

        [Test]
        public void Unsubscribe_NonexistentHandler_DoesNotThrow()
        {
            var bus = new SimpleEventBus();
            Action<TestEvent> handler = _ => { };
            Assert.DoesNotThrow(() => bus.Unsubscribe(handler));
        }

        [Test]
        public void Subscribe_ThenUnsubscribeOneOfTwo_RemainingHandlerStillInvoked()
        {
            var bus = new SimpleEventBus();
            int count = 0;
            Action<TestEvent> h1 = _ => count++;
            Action<TestEvent> h2 = _ => count++;

            bus.Subscribe(h1);
            bus.Subscribe(h2);
            bus.Unsubscribe(h1);
            bus.Publish(new TestEvent());

            Assert.That(count, Is.EqualTo(1));
        }
    }
}
