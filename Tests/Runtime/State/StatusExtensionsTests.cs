using NUnit.Framework;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Tests.State
{
    [TestFixture]
    public class StatusExtensionsTests
    {
        // ── QuestStatus ─────────────────────────────────────────────────────────

        [Test]
        public void QuestStatus_Completed_IsTerminal() =>
            Assert.That(QuestStatus.Completed.IsTerminal(), Is.True);

        [Test]
        public void QuestStatus_Failed_IsTerminal() =>
            Assert.That(QuestStatus.Failed.IsTerminal(), Is.True);

        [Test]
        public void QuestStatus_InProgress_IsNotTerminal() =>
            Assert.That(QuestStatus.InProgress.IsTerminal(), Is.False);

        [Test]
        public void QuestStatus_NotStarted_IsNotTerminal() =>
            Assert.That(QuestStatus.NotStarted.IsTerminal(), Is.False);

        [Test]
        public void QuestStatus_InProgress_IsActive() =>
            Assert.That(QuestStatus.InProgress.IsActive(), Is.True);

        [Test]
        public void QuestStatus_NotStarted_IsNotActive() =>
            Assert.That(QuestStatus.NotStarted.IsActive(), Is.False);

        [Test]
        public void QuestStatus_Completed_IsNotActive() =>
            Assert.That(QuestStatus.Completed.IsActive(), Is.False);

        // ── ObjectiveStatus ─────────────────────────────────────────────────────

        [Test]
        public void ObjectiveStatus_Completed_IsTerminal() =>
            Assert.That(ObjectiveStatus.Completed.IsTerminal(), Is.True);

        [Test]
        public void ObjectiveStatus_Failed_IsTerminal() =>
            Assert.That(ObjectiveStatus.Failed.IsTerminal(), Is.True);

        [Test]
        public void ObjectiveStatus_InProgress_IsNotTerminal() =>
            Assert.That(ObjectiveStatus.InProgress.IsTerminal(), Is.False);

        [Test]
        public void ObjectiveStatus_InProgress_IsActive() =>
            Assert.That(ObjectiveStatus.InProgress.IsActive(), Is.True);

        [Test]
        public void ObjectiveStatus_NotStarted_IsNotActive() =>
            Assert.That(ObjectiveStatus.NotStarted.IsActive(), Is.False);

        // ── CanEvaluate ─────────────────────────────────────────────────────────

        [Test]
        public void CanEvaluate_InProgressObjective_InProgressQuest_True() =>
            Assert.That(ObjectiveStatus.InProgress.CanEvaluate(QuestStatus.InProgress), Is.True);

        [Test]
        public void CanEvaluate_AnyObjective_FailedQuest_False() =>
            Assert.That(ObjectiveStatus.InProgress.CanEvaluate(QuestStatus.Failed), Is.False);

        [Test]
        public void CanEvaluate_AnyObjective_CompletedQuest_False() =>
            Assert.That(ObjectiveStatus.InProgress.CanEvaluate(QuestStatus.Completed), Is.False);

        [Test]
        public void CanEvaluate_CompletedObjective_ActiveQuest_False() =>
            Assert.That(ObjectiveStatus.Completed.CanEvaluate(QuestStatus.InProgress), Is.False);

        // ── Transitions ─────────────────────────────────────────────────────────

        [Test]
        public void TransitionedToCompleted_FromInProgress_True() =>
            Assert.That(
                StatusExtensions.TransitionedToCompleted(ObjectiveStatus.InProgress, ObjectiveStatus.Completed),
                Is.True);

        [Test]
        public void TransitionedToCompleted_AlreadyCompleted_False() =>
            Assert.That(
                StatusExtensions.TransitionedToCompleted(ObjectiveStatus.Completed, ObjectiveStatus.Completed),
                Is.False);

        [Test]
        public void TransitionedToFailed_FromInProgress_True() =>
            Assert.That(
                StatusExtensions.TransitionedToFailed(ObjectiveStatus.InProgress, ObjectiveStatus.Failed),
                Is.True);

        [Test]
        public void TransitionedToFailed_AlreadyFailed_False() =>
            Assert.That(
                StatusExtensions.TransitionedToFailed(ObjectiveStatus.Failed, ObjectiveStatus.Failed),
                Is.False);
    }
}
