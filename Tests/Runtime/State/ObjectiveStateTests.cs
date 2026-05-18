using NUnit.Framework;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Tests.Helpers;
using UnityEngine;

namespace DynamicBox.Quest.Tests.State
{
    [TestFixture]
    public class ObjectiveStateTests
    {
        [Test]
        public void Constructor_InitialStatus_IsNotStarted()
        {
            var asset = new ObjectiveBuilder().Build();
            var state = new ObjectiveState(asset);

            Assert.That(state.Status, Is.EqualTo(ObjectiveStatus.NotStarted));
        }

        [Test]
        public void CompletionInstance_NullWhenNoConditionAsset()
        {
            var asset = new ObjectiveBuilder().Build();
            var state = new ObjectiveState(asset);

            Assert.That(state.CompletionInstance, Is.Null);
        }

        [Test]
        public void FailInstance_NullWhenNoConditionAsset()
        {
            var asset = new ObjectiveBuilder().Build();
            var state = new ObjectiveState(asset);

            Assert.That(state.FailInstance, Is.Null);
        }

        [Test]
        public void CompletionInstance_CreatedFromAsset()
        {
            var conditionAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithCompletionCondition(conditionAsset).Build();
            var state = new ObjectiveState(objAsset);

            Assert.That(state.CompletionInstance, Is.Not.Null);
            Assert.That(state.CompletionInstance, Is.InstanceOf<MockConditionInstance>());
        }

        [Test]
        public void FailInstance_CreatedFromAsset()
        {
            var conditionAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithFailCondition(conditionAsset).Build();
            var state = new ObjectiveState(objAsset);

            Assert.That(state.FailInstance, Is.Not.Null);
            Assert.That(state.FailInstance, Is.InstanceOf<MockConditionInstance>());
        }

        [Test]
        public void CanProgress_NoPrerequisites_ReturnsTrue()
        {
            var asset = new ObjectiveBuilder().Build();
            var questAsset = new QuestBuilder().AddObjective(asset).Build();
            var questState = new QuestState(questAsset);

            Assert.That(questState.Objectives["test-objective"].CanProgress(questState), Is.True);
        }

        [Test]
        public void CanProgress_PrerequisiteNotCompleted_ReturnsFalse()
        {
            var prereqAsset = new ObjectiveBuilder().WithObjectiveId("prereq").Build();
            var goalAsset = new ObjectiveBuilder().WithObjectiveId("goal").AddPrerequisite(prereqAsset).Build();
            var questAsset = new QuestBuilder().AddObjective(prereqAsset).AddObjective(goalAsset).Build();
            var questState = new QuestState(questAsset);

            Assert.That(questState.Objectives["goal"].CanProgress(questState), Is.False);
        }

        [Test]
        public void CanProgress_PrerequisiteCompleted_ReturnsTrue()
        {
            var prereqAsset = new ObjectiveBuilder().WithObjectiveId("prereq").Build();
            var goalAsset = new ObjectiveBuilder().WithObjectiveId("goal").AddPrerequisite(prereqAsset).Build();
            var questAsset = new QuestBuilder().AddObjective(prereqAsset).AddObjective(goalAsset).Build();
            var questState = new QuestState(questAsset);

            questState.Objectives["prereq"].SetStatus(ObjectiveStatus.Completed);

            Assert.That(questState.Objectives["goal"].CanProgress(questState), Is.True);
        }

        [Test]
        public void CanProgress_TerminalObjective_ReturnsFalse()
        {
            var asset = new ObjectiveBuilder().WithObjectiveId("obj").Build();
            var questAsset = new QuestBuilder().AddObjective(asset).Build();
            var questState = new QuestState(questAsset);

            questState.Objectives["obj"].SetStatus(ObjectiveStatus.Completed);

            Assert.That(questState.Objectives["obj"].CanProgress(questState), Is.False);
        }

        [Test]
        public void Definition_MatchesAsset()
        {
            var asset = new ObjectiveBuilder().WithObjectiveId("my-id").Build();
            var state = new ObjectiveState(asset);

            Assert.That(state.Definition.ObjectiveId, Is.EqualTo("my-id"));
        }
    }
}
