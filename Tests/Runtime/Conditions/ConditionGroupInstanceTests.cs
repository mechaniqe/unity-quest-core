using System.Collections.Generic;
using NUnit.Framework;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Tests.Helpers;

namespace DynamicBox.Quest.Tests.Conditions
{
    [TestFixture]
    public class ConditionGroupInstanceTests
    {
        private SimpleEventBus _bus;
        private QuestContext _context;

        [SetUp]
        public void SetUp()
        {
            _bus = new SimpleEventBus();
            _context = new QuestContext();
        }

        // --- AND logic ---

        [Test]
        public void And_BothFalse_IsMetFalse()
        {
            var c1 = new MockConditionInstance();
            var c2 = new MockConditionInstance();
            var group = MakeGroup(ConditionOperator.And, c1, c2);
            group.Bind(_bus, _context, () => { });

            Assert.That(group.IsMet, Is.False);
        }

        [Test]
        public void And_OneTrueOneFalse_IsMetFalse()
        {
            var c1 = new MockConditionInstance();
            var c2 = new MockConditionInstance();
            var group = MakeGroup(ConditionOperator.And, c1, c2);
            group.Bind(_bus, _context, () => { });

            c1.SetMet(true);

            Assert.That(group.IsMet, Is.False);
        }

        [Test]
        public void And_BothTrue_IsMetTrue()
        {
            var c1 = new MockConditionInstance();
            var c2 = new MockConditionInstance();
            var group = MakeGroup(ConditionOperator.And, c1, c2);
            group.Bind(_bus, _context, () => { });

            c1.SetMet(true);
            c2.SetMet(true);

            Assert.That(group.IsMet, Is.True);
        }

        // --- OR logic ---

        [Test]
        public void Or_BothFalse_IsMetFalse()
        {
            var c1 = new MockConditionInstance();
            var c2 = new MockConditionInstance();
            var group = MakeGroup(ConditionOperator.Or, c1, c2);
            group.Bind(_bus, _context, () => { });

            Assert.That(group.IsMet, Is.False);
        }

        [Test]
        public void Or_OneTrueOtherFalse_IsMetTrue()
        {
            var c1 = new MockConditionInstance();
            var c2 = new MockConditionInstance();
            var group = MakeGroup(ConditionOperator.Or, c1, c2);
            group.Bind(_bus, _context, () => { });

            c1.SetMet(true);

            Assert.That(group.IsMet, Is.True);
        }

        // --- Callbacks ---

        [Test]
        public void Bind_PropagatesOnChangedWhenChildChanges()
        {
            var c1 = new MockConditionInstance();
            var group = MakeGroup(ConditionOperator.Or, c1);
            bool changed = false;
            group.Bind(_bus, _context, () => changed = true);

            c1.SetMet(true);

            Assert.That(changed, Is.True);
        }

        [Test]
        public void Unbind_ClearsGroupOnChanged()
        {
            var c1 = new MockConditionInstance();
            var group = MakeGroup(ConditionOperator.Or, c1);
            bool changed = false;
            group.Bind(_bus, _context, () => changed = true);
            group.Unbind(_bus, _context);

            // After unbind the group clears its _onChanged; child callbacks still exist
            // but the group's propagation path is cleared.
            Assert.DoesNotThrow(() => c1.SetMet(true));
            Assert.That(changed, Is.False);
        }

        // --- Reset ---

        [Test]
        public void Reset_SetsIsMetFalse()
        {
            var c1 = new MockConditionInstance();
            var group = MakeGroup(ConditionOperator.Or, c1);
            group.Bind(_bus, _context, () => { });
            c1.SetMet(true);
            Assert.That(group.IsMet, Is.True);

            group.Reset();

            Assert.That(group.IsMet, Is.False);
        }

        [Test]
        public void Reset_CallsResetOnChildren()
        {
            var c1 = new MockConditionInstance();
            var group = MakeGroup(ConditionOperator.Or, c1);
            group.Bind(_bus, _context, () => { });

            group.Reset();

            Assert.That(c1.ResetCalled, Is.True);
        }

        // --- Polling ---

        [Test]
        public void Refresh_CallsRefreshOnPollingChildren()
        {
            var polling = new MockConditionInstance(); // implements IPollingConditionInstance
            var group = MakeGroup(ConditionOperator.Or, polling);
            group.Bind(_bus, _context, () => { });

            group.Refresh(_context, () => { });

            Assert.That(polling.RefreshCalled, Is.True);
        }

        // --- Progress (no IProgressReportingCondition children → fallback) ---

        [Test]
        public void Progress_ReturnsZeroWhenNotMet()
        {
            var c1 = new MockConditionInstance();
            var group = MakeGroup(ConditionOperator.And, c1);
            group.Bind(_bus, _context, () => { });

            Assert.That(group.Progress, Is.EqualTo(0f));
        }

        [Test]
        public void Progress_ReturnsOneWhenMet()
        {
            var c1 = new MockConditionInstance();
            var group = MakeGroup(ConditionOperator.Or, c1);
            group.Bind(_bus, _context, () => { });
            c1.SetMet(true);

            Assert.That(group.Progress, Is.EqualTo(1f));
        }

        [Test]
        public void ProgressDescription_ContainsSeparator()
        {
            var c1 = new MockConditionInstance();
            var c2 = new MockConditionInstance();
            var group = MakeGroup(ConditionOperator.And, c1, c2);
            group.Bind(_bus, _context, () => { });

            Assert.That(group.ProgressDescription, Does.Contain("/"));
        }

        // --- Helpers ---

        private static ConditionGroupInstance MakeGroup(ConditionOperator op, params MockConditionInstance[] children)
        {
            var list = new List<IConditionInstance>(children);
            return new ConditionGroupInstance(op, list);
        }
    }
}
