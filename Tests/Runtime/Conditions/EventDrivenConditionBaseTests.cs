using NUnit.Framework;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Tests.Conditions
{
    [TestFixture]
    public class EventDrivenConditionBaseTests
    {
        // ── Test event ──────────────────────────────────────────────────────────
        private struct TestGameEvent { public string Data; }

        // ── Concrete test double ────────────────────────────────────────────────
        private class TestConditionDouble : EventDrivenConditionBase<TestGameEvent>
        {
            private bool _isMet;
            public override bool IsMet => _isMet;
            public int HandleEventCallCount { get; private set; }
            public string? LastEventData { get; private set; }
            public bool OnBindCalled { get; private set; }
            public bool OnUnbindCalled { get; private set; }

            protected override void HandleEvent(TestGameEvent evt)
            {
                HandleEventCallCount++;
                LastEventData = evt.Data;
                _isMet = true;
                NotifyChanged();
            }

            protected override void OnBind(QuestContext context) => OnBindCalled = true;
            protected override void OnUnbind(QuestContext context) => OnUnbindCalled = true;
        }

        // ── Fixtures ────────────────────────────────────────────────────────────
        private SimpleEventBus _bus;
        private QuestContext _context;

        [SetUp]
        public void SetUp()
        {
            _bus = new SimpleEventBus();
            _context = new QuestContext();
        }

        // ── Tests ───────────────────────────────────────────────────────────────

        [Test]
        public void Bind_SubscribesToEventBus_HandlerReceivesEvents()
        {
            var condition = new TestConditionDouble();
            condition.Bind(_bus, _context, () => { });

            _bus.Publish(new TestGameEvent { Data = "hello" });

            Assert.That(condition.HandleEventCallCount, Is.EqualTo(1));
        }

        [Test]
        public void Unbind_UnsubscribesFromEventBus_HandlerStopReceivingEvents()
        {
            var condition = new TestConditionDouble();
            condition.Bind(_bus, _context, () => { });
            condition.Unbind(_bus, _context);

            _bus.Publish(new TestGameEvent());

            Assert.That(condition.HandleEventCallCount, Is.EqualTo(0));
        }

        [Test]
        public void Publish_AfterBind_HandleEventCalledWithCorrectData()
        {
            var condition = new TestConditionDouble();
            condition.Bind(_bus, _context, () => { });

            _bus.Publish(new TestGameEvent { Data = "event_data" });

            Assert.That(condition.LastEventData, Is.EqualTo("event_data"));
        }

        [Test]
        public void NotifyChanged_InvokesOnChangedCallback()
        {
            var condition = new TestConditionDouble();
            bool callbackFired = false;
            condition.Bind(_bus, _context, () => callbackFired = true);

            _bus.Publish(new TestGameEvent());

            Assert.That(callbackFired, Is.True);
        }

        [Test]
        public void OnBind_IsCalledDuringBind()
        {
            var condition = new TestConditionDouble();
            condition.Bind(_bus, _context, () => { });

            Assert.That(condition.OnBindCalled, Is.True);
        }

        [Test]
        public void OnUnbind_IsCalledDuringUnbind()
        {
            var condition = new TestConditionDouble();
            condition.Bind(_bus, _context, () => { });
            condition.Unbind(_bus, _context);

            Assert.That(condition.OnUnbindCalled, Is.True);
        }

        [Test]
        public void MultipleBindUnbindCycles_ReceivesEventOnFinalBind()
        {
            var condition = new TestConditionDouble();

            condition.Bind(_bus, _context, () => { });
            condition.Unbind(_bus, _context);
            condition.Bind(_bus, _context, () => { });

            _bus.Publish(new TestGameEvent());

            Assert.That(condition.HandleEventCallCount, Is.EqualTo(1));
        }

        [Test]
        public void Reset_BaseImplementation_DoesNotThrow()
        {
            var condition = new TestConditionDouble();
            Assert.DoesNotThrow(() => condition.Reset());
        }
    }
}
