using NUnit.Framework;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Conditions;
using DynamicBox.Quest.Core.Services;

namespace DynamicBox.Quest.Tests.Conditions
{
    /// <summary>
    /// Tests for TimeElapsedConditionInstance using a pure C# mock time service.
    /// No MonoBehaviour required.
    /// </summary>
    [TestFixture]
    public class TimeElapsedConditionInstanceTests
    {
        // ── Pure C# mock — no MonoBehaviour needed ───────────────────────────────
        private class FakeTimeService : IQuestTimeService
        {
            public float TotalGameTime { get; set; }
            public float DeltaTime => 0f;
            public float TimeOfDay => 0f;
            public int CurrentDay => 0;
        }

        private FakeTimeService _time;
        private SimpleEventBus _bus;
        private QuestContext _context;

        [SetUp]
        public void SetUp()
        {
            _time = new FakeTimeService { TotalGameTime = 0f };
            _bus = new SimpleEventBus();
            _context = new QuestContext(timeService: _time);
        }

        [Test]
        public void InitialState_IsMetFalse()
        {
            var condition = new TimeElapsedConditionInstance(5f);
            Assert.That(condition.IsMet, Is.False);
        }

        [Test]
        public void InitialState_ProgressIsZero()
        {
            var condition = new TimeElapsedConditionInstance(5f);
            Assert.That(condition.Progress, Is.EqualTo(0f));
        }

        [Test]
        public void Refresh_InsufficientTime_IsMetFalse()
        {
            var condition = new TimeElapsedConditionInstance(5f);
            condition.Bind(_bus, _context, () => { });

            _time.TotalGameTime = 3f;
            condition.Refresh(_context, () => { });

            Assert.That(condition.IsMet, Is.False);
        }

        [Test]
        public void Refresh_SufficientTime_IsMetTrue()
        {
            var condition = new TimeElapsedConditionInstance(5f);
            condition.Bind(_bus, _context, () => { });

            _time.TotalGameTime = 5f;
            condition.Refresh(_context, () => { });

            Assert.That(condition.IsMet, Is.True);
        }

        [Test]
        public void Refresh_SufficientTime_InvokesOnChanged()
        {
            var condition = new TimeElapsedConditionInstance(5f);
            bool fired = false;
            condition.Bind(_bus, _context, () => { });

            _time.TotalGameTime = 5f;
            condition.Refresh(_context, () => fired = true);

            Assert.That(fired, Is.True);
        }

        [Test]
        public void Progress_ClampedBetweenZeroAndOne()
        {
            var condition = new TimeElapsedConditionInstance(5f);
            condition.Bind(_bus, _context, () => { });

            _time.TotalGameTime = 3f;
            condition.Refresh(_context, () => { });

            Assert.That(condition.Progress, Is.InRange(0f, 1f));
        }

        [Test]
        public void GetRemainingTime_BeforeCompletion_IsPositive()
        {
            var condition = new TimeElapsedConditionInstance(5f);
            condition.Bind(_bus, _context, () => { });

            _time.TotalGameTime = 2f;
            condition.Refresh(_context, () => { });

            Assert.That(condition.GetRemainingTime(), Is.GreaterThan(0f));
        }

        [Test]
        public void GetRemainingTime_AfterCompletion_IsZero()
        {
            var condition = new TimeElapsedConditionInstance(5f);
            condition.Bind(_bus, _context, () => { });

            _time.TotalGameTime = 10f;
            condition.Refresh(_context, () => { });

            Assert.That(condition.GetRemainingTime(), Is.EqualTo(0f));
        }

        [Test]
        public void Reset_ClearsElapsedTime_IsMetFalse()
        {
            var condition = new TimeElapsedConditionInstance(5f);
            condition.Bind(_bus, _context, () => { });
            _time.TotalGameTime = 5f;
            condition.Refresh(_context, () => { });
            Assert.That(condition.IsMet, Is.True);

            condition.Reset();

            Assert.That(condition.IsMet, Is.False);
        }

        [Test]
        public void ProgressDescription_ContainsSecondsText()
        {
            var condition = new TimeElapsedConditionInstance(5f);
            condition.Bind(_bus, _context, () => { });

            Assert.That(condition.ProgressDescription, Does.Contain("second"));
        }
    }
}
