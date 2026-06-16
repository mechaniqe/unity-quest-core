using System;
using System.Collections.Generic;
using NUnit.Framework;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Tests.Helpers;

namespace DynamicBox.Quest.Tests.State
{
    [TestFixture]
    public class QuestStateTests
    {
        [Test]
        public void Constructor_InitialStatus_IsNotStarted()
        {
            var quest = new QuestBuilder().Build();
            var state = new QuestState(quest);

            Assert.That(state.Status, Is.EqualTo(QuestStatus.NotStarted));
        }

        [Test]
        public void Constructor_CreatesObjectiveForEachAsset()
        {
            var obj1 = new ObjectiveBuilder().WithObjectiveId("obj1").Build();
            var obj2 = new ObjectiveBuilder().WithObjectiveId("obj2").Build();
            var quest = new QuestBuilder().AddObjective(obj1).AddObjective(obj2).Build();

            var state = new QuestState(quest);

            Assert.That(state.Objectives.Count, Is.EqualTo(2));
        }

        [Test]
        public void Objectives_IndexedById()
        {
            var obj = new ObjectiveBuilder().WithObjectiveId("my-obj").Build();
            var quest = new QuestBuilder().AddObjective(obj).Build();

            var state = new QuestState(quest);

            Assert.That(state.Objectives.ContainsKey("my-obj"), Is.True);
        }

        [Test]
        public void TryGetObjective_ExistingId_ReturnsTrueAndState()
        {
            var obj = new ObjectiveBuilder().WithObjectiveId("obj1").Build();
            var quest = new QuestBuilder().AddObjective(obj).Build();
            var state = new QuestState(quest);

            bool found = state.TryGetObjective("obj1", out var result);

            Assert.That(found, Is.True);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TryGetObjective_MissingId_ReturnsFalse()
        {
            var quest = new QuestBuilder().Build();
            var state = new QuestState(quest);

            bool found = state.TryGetObjective("nonexistent", out _);

            Assert.That(found, Is.False);
        }

        [Test]
        public void DuplicateObjectiveIds_ThrowsArgumentException()
        {
            var obj1 = new ObjectiveBuilder().WithObjectiveId("duplicate").Build();
            var obj2 = new ObjectiveBuilder().WithObjectiveId("duplicate").Build();
            var quest = new QuestBuilder().AddObjective(obj1).AddObjective(obj2).Build();

            Assert.Throws<ArgumentException>(() => new QuestState(quest));
        }

        [Test]
        public void GetObjectiveStates_ReturnsAllObjectives()
        {
            var obj1 = new ObjectiveBuilder().WithObjectiveId("obj1").Build();
            var obj2 = new ObjectiveBuilder().WithObjectiveId("obj2").Build();
            var quest = new QuestBuilder().AddObjective(obj1).AddObjective(obj2).Build();
            var state = new QuestState(quest);

            var all = new List<ObjectiveState>(state.GetObjectiveStates());

            Assert.That(all.Count, Is.EqualTo(2));
        }

        // ── ActiveObjectives ───────────────────────────────────────────────────

        [Test]
        public void ActiveObjectives_NoObjectives_ReturnsEmpty()
        {
            var quest = new QuestBuilder().Build();
            var state = new QuestState(quest);

            Assert.That(state.ActiveObjectives, Is.Empty);
        }

        [Test]
        public void ActiveObjectives_AllNotStarted_ReturnsEmpty()
        {
            var obj1 = new ObjectiveBuilder().WithObjectiveId("obj1").Build();
            var quest = new QuestBuilder().AddObjective(obj1).Build();
            var state = new QuestState(quest);

            Assert.That(state.ActiveObjectives, Is.Empty);
        }

        [Test]
        public void ActiveObjectives_ReturnsOnlyInProgressObjectives()
        {
            var obj1 = new ObjectiveBuilder().WithObjectiveId("obj1").Build();
            var obj2 = new ObjectiveBuilder().WithObjectiveId("obj2").Build();
            var quest = new QuestBuilder().AddObjective(obj1).AddObjective(obj2).Build();
            var state = new QuestState(quest);

            state.Objectives["obj1"].SetStatus(ObjectiveStatus.InProgress);
            // obj2 stays NotStarted

            var active = new List<ObjectiveState>(state.ActiveObjectives);

            Assert.That(active.Count, Is.EqualTo(1));
            Assert.That(active[0], Is.SameAs(state.Objectives["obj1"]));
        }

        [Test]
        public void ActiveObjectives_ExcludesCompletedAndFailed()
        {
            var obj1 = new ObjectiveBuilder().WithObjectiveId("obj1").Build();
            var obj2 = new ObjectiveBuilder().WithObjectiveId("obj2").Build();
            var obj3 = new ObjectiveBuilder().WithObjectiveId("obj3").Build();
            var quest = new QuestBuilder().AddObjective(obj1).AddObjective(obj2).AddObjective(obj3).Build();
            var state = new QuestState(quest);

            state.Objectives["obj1"].SetStatus(ObjectiveStatus.Completed);
            state.Objectives["obj2"].SetStatus(ObjectiveStatus.Failed);
            state.Objectives["obj3"].SetStatus(ObjectiveStatus.InProgress);

            var active = new List<ObjectiveState>(state.ActiveObjectives);

            Assert.That(active.Count, Is.EqualTo(1));
            Assert.That(active[0], Is.SameAs(state.Objectives["obj3"]));
        }

        [Test]
        public void Definition_MatchesAsset()
        {
            var quest = new QuestBuilder().WithQuestId("quest-xyz").Build();
            var state = new QuestState(quest);

            Assert.That(state.Definition.QuestId, Is.EqualTo("quest-xyz"));
        }
    }
}
