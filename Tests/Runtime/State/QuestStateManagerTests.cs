using System;
using System.Collections.Generic;
using NUnit.Framework;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.State;
using DynamicBox.Quest.Tests.Helpers;

namespace DynamicBox.Quest.Tests.State
{
    [TestFixture]
    public class QuestStateManagerTests
    {
        private QuestContext _context;

        [SetUp]
        public void SetUp()
        {
            _context = new QuestContext();
        }

        private QuestState BuildInProgressQuest(string questId = "test-quest", string objId = "obj1")
        {
            var objAsset = new ObjectiveBuilder().WithObjectiveId(objId).Build();
            var questAsset = new QuestBuilder().WithQuestId(questId).AddObjective(objAsset).Build();
            var state = new QuestState(questAsset);
            state.SetStatus(QuestStatus.InProgress);
            state.Objectives[objId].SetStatus(ObjectiveStatus.InProgress);
            return state;
        }

        [Test]
        public void CaptureSnapshot_QuestId_MatchesState()
        {
            var questState = BuildInProgressQuest("my-quest");

            var snapshot = QuestStateManager.CaptureSnapshot(questState);

            Assert.That(snapshot.QuestId, Is.EqualTo("my-quest"));
        }

        [Test]
        public void CaptureSnapshot_Status_MatchesState()
        {
            var questState = BuildInProgressQuest();
            questState.SetStatus(QuestStatus.Completed);

            var snapshot = QuestStateManager.CaptureSnapshot(questState);

            Assert.That(snapshot.Status, Is.EqualTo(QuestStatus.Completed));
        }

        [Test]
        public void CaptureSnapshot_IncludesObjectiveStatuses()
        {
            var questState = BuildInProgressQuest("q1", "obj1");

            var snapshot = QuestStateManager.CaptureSnapshot(questState);

            Assert.That(snapshot.ObjectiveStatuses.Count, Is.EqualTo(1));
            Assert.That(snapshot.ObjectiveStatuses[0].ObjectiveId, Is.EqualTo("obj1"));
        }

        [Test]
        public void CaptureSnapshot_NullState_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => QuestStateManager.CaptureSnapshot(null));
        }

        [Test]
        public void RestoreFromSnapshot_RestoresQuestStatus()
        {
            var questState = BuildInProgressQuest("q1");
            var snapshot = QuestStateManager.CaptureSnapshot(questState);
            snapshot.Status = QuestStatus.Completed;

            var restored = QuestStateManager.RestoreFromSnapshot(snapshot, questState.Definition, _context);

            Assert.That(restored.Status, Is.EqualTo(QuestStatus.Completed));
        }

        [Test]
        public void RestoreFromSnapshot_RestoresObjectiveStatuses()
        {
            var questState = BuildInProgressQuest("q1", "obj1");
            questState.Objectives["obj1"].SetStatus(ObjectiveStatus.Completed);
            var snapshot = QuestStateManager.CaptureSnapshot(questState);

            var restored = QuestStateManager.RestoreFromSnapshot(snapshot, questState.Definition, _context);

            Assert.That(restored.Objectives["obj1"].Status, Is.EqualTo(ObjectiveStatus.Completed));
        }

        [Test]
        public void RestoreFromSnapshot_InvalidSnapshot_ThrowsArgumentException()
        {
            var invalidSnapshot = new QuestStateSnapshot { QuestId = "" };
            var questAsset = new QuestBuilder().Build();

            Assert.Throws<ArgumentException>(() =>
                QuestStateManager.RestoreFromSnapshot(invalidSnapshot, questAsset, _context));
        }

        [Test]
        public void RestoreFromSnapshot_WrongQuestId_ThrowsArgumentException()
        {
            var snapshot = new QuestStateSnapshot { QuestId = "wrong-id" };
            var questAsset = new QuestBuilder().WithQuestId("correct-id").Build();

            Assert.Throws<ArgumentException>(() =>
                QuestStateManager.RestoreFromSnapshot(snapshot, questAsset, _context));
        }

        [Test]
        public void CaptureAllSnapshots_IncludesAllQuests()
        {
            var state1 = BuildInProgressQuest("q1");
            var state2 = BuildInProgressQuest("q2", "obj2");

            var saveData = QuestStateManager.CaptureAllSnapshots(new[] { state1, state2 });

            Assert.That(saveData.Quests.Count, Is.EqualTo(2));
        }

        [Test]
        public void RoundTrip_CaptureAndRestore_PreservesState()
        {
            var questState = BuildInProgressQuest("round-trip-quest", "rt-obj");
            questState.SetStatus(QuestStatus.InProgress);
            questState.Objectives["rt-obj"].SetStatus(ObjectiveStatus.Completed);

            var snapshot = QuestStateManager.CaptureSnapshot(questState);
            var restored = QuestStateManager.RestoreFromSnapshot(snapshot, questState.Definition, _context);

            Assert.That(restored.Status, Is.EqualTo(questState.Status));
            Assert.That(restored.Objectives["rt-obj"].Status, Is.EqualTo(ObjectiveStatus.Completed));
        }

        [Test]
        public void RestoreAllFromSnapshots_NullSaveData_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                QuestStateManager.RestoreAllFromSnapshots(null, new Dictionary<string, QuestAsset>(), _context));
        }
    }
}
