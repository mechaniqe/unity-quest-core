using NUnit.Framework;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Tests.Helpers;

namespace DynamicBox.Quest.Tests.Management
{
    [TestFixture]
    public class QuestLogTests
    {
        private QuestLog _log;

        [SetUp]
        public void SetUp()
        {
            _log = new QuestLog();
        }

        [Test]
        public void StartQuest_AddsToActiveList()
        {
            var questAsset = new QuestBuilder().Build();

            _log.StartQuest(questAsset);

            Assert.That(_log.Active.Count, Is.EqualTo(1));
        }

        [Test]
        public void StartQuest_ReturnsStateWithInProgressStatus()
        {
            var questAsset = new QuestBuilder().Build();

            var state = _log.StartQuest(questAsset);

            Assert.That(state.Status, Is.EqualTo(QuestStatus.InProgress));
        }

        [Test]
        public void StartQuest_ReturnsStateWithCorrectDefinition()
        {
            var questAsset = new QuestBuilder().WithQuestId("q-xyz").Build();

            var state = _log.StartQuest(questAsset);

            Assert.That(state.Definition.QuestId, Is.EqualTo("q-xyz"));
        }

        [Test]
        public void RemoveQuest_RemovesFromActiveList()
        {
            var questAsset = new QuestBuilder().Build();
            var state = _log.StartQuest(questAsset);

            _log.RemoveQuest(state);

            Assert.That(_log.Active.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveQuest_NonExistentState_DoesNotThrow()
        {
            var questAsset = new QuestBuilder().Build();
            var untracked = new QuestState(questAsset);

            Assert.DoesNotThrow(() => _log.RemoveQuest(untracked));
        }

        [Test]
        public void Active_MultipleQuests_AllTracked()
        {
            var q1 = new QuestBuilder().WithQuestId("q1").Build();
            var q2 = new QuestBuilder().WithQuestId("q2").Build();

            _log.StartQuest(q1);
            _log.StartQuest(q2);

            Assert.That(_log.Active.Count, Is.EqualTo(2));
        }

        [Test]
        public void StartQuest_MultipleQuests_EachHasOwnState()
        {
            var q1 = new QuestBuilder().WithQuestId("q1").Build();
            var q2 = new QuestBuilder().WithQuestId("q2").Build();

            var s1 = _log.StartQuest(q1);
            var s2 = _log.StartQuest(q2);

            Assert.That(s1, Is.Not.SameAs(s2));
            Assert.That(s1.Definition.QuestId, Is.EqualTo("q1"));
            Assert.That(s2.Definition.QuestId, Is.EqualTo("q2"));
        }
    }
}
