using NUnit.Framework;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Tests.Helpers;
using UnityEngine;

namespace DynamicBox.Quest.Tests.Management
{
    [TestFixture]
    public class QuestManagerTests
    {
        private GameObject _go;
        private QuestManager _questManager;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("QuestManager");
            _questManager = _go.AddComponent<QuestManager>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_go);
        }

        // ── StartQuest ──────────────────────────────────────────────────────

        [Test]
        public void StartQuest_AddsToActiveQuests()
        {
            var questAsset = new QuestBuilder().Build();

            _questManager.StartQuest(questAsset);

            Assert.That(_questManager.ActiveQuests.Count, Is.EqualTo(1));
        }

        [Test]
        public void StartQuest_ReturnsStateWithInProgressStatus()
        {
            var questAsset = new QuestBuilder().Build();

            var state = _questManager.StartQuest(questAsset);

            Assert.That(state.Status, Is.EqualTo(QuestStatus.InProgress));
        }

        // ── StopQuest ────────────────────────────────────────────────────────

        [Test]
        public void StopQuest_ByState_RemovesFromActiveQuests()
        {
            var questAsset = new QuestBuilder().Build();
            var state = _questManager.StartQuest(questAsset);

            _questManager.StopQuest(state);

            Assert.That(_questManager.ActiveQuests.Count, Is.EqualTo(0));
        }

        [Test]
        public void StopQuest_ByAsset_RemovesFromActiveQuests()
        {
            var questAsset = new QuestBuilder().Build();
            _questManager.StartQuest(questAsset);

            bool stopped = _questManager.StopQuest(questAsset);

            Assert.That(stopped, Is.True);
            Assert.That(_questManager.ActiveQuests.Count, Is.EqualTo(0));
        }

        [Test]
        public void StopQuest_ByAsset_NotStarted_ReturnsFalse()
        {
            var questAsset = new QuestBuilder().Build();

            bool stopped = _questManager.StopQuest(questAsset);

            Assert.That(stopped, Is.False);
        }

        // ── CompleteQuest / FailQuest ─────────────────────────────────────────

        [Test]
        public void CompleteQuest_SetsStatusCompleted()
        {
            var questAsset = new QuestBuilder().Build();
            var state = _questManager.StartQuest(questAsset);

            _questManager.CompleteQuest(state);

            Assert.That(state.Status, Is.EqualTo(QuestStatus.Completed));
        }

        [Test]
        public void CompleteQuest_FiresOnQuestCompleted()
        {
            var questAsset = new QuestBuilder().Build();
            var state = _questManager.StartQuest(questAsset);
            QuestState? fired = null;
            _questManager.OnQuestCompleted += q => fired = q;

            _questManager.CompleteQuest(state);

            Assert.That(fired, Is.SameAs(state));
        }

        [Test]
        public void CompleteQuest_RemovesFromActiveQuests()
        {
            var questAsset = new QuestBuilder().Build();
            var state = _questManager.StartQuest(questAsset);

            _questManager.CompleteQuest(state);

            Assert.That(_questManager.ActiveQuests.Count, Is.EqualTo(0));
        }

        [Test]
        public void FailQuest_SetsStatusFailed()
        {
            var questAsset = new QuestBuilder().Build();
            var state = _questManager.StartQuest(questAsset);

            _questManager.FailQuest(state);

            Assert.That(state.Status, Is.EqualTo(QuestStatus.Failed));
        }

        [Test]
        public void FailQuest_FiresOnQuestFailed()
        {
            var questAsset = new QuestBuilder().Build();
            var state = _questManager.StartQuest(questAsset);
            QuestState? fired = null;
            _questManager.OnQuestFailed += q => fired = q;

            _questManager.FailQuest(state);

            Assert.That(fired, Is.SameAs(state));
        }

        // ── Condition-driven completion ───────────────────────────────────────

        [Test]
        public void ConditionMet_SingleObjective_QuestCompletes()
        {
            var completionAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .WithCompletionCondition(completionAsset).Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            bool questCompleted = false;
            _questManager.OnQuestCompleted += _ => questCompleted = true;

            var questState = _questManager.StartQuest(questAsset);
            var conditionInstance = questState.Objectives["obj1"].CompletionInstance as MockConditionInstance;
            Assert.That(conditionInstance, Is.Not.Null);

            conditionInstance!.SetMet(true);
            _questManager.ProcessPendingEvaluations();

            Assert.That(questCompleted, Is.True);
        }

        [Test]
        public void ConditionMet_SingleObjective_ObjectiveStatusIsCompleted()
        {
            var completionAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .WithCompletionCondition(completionAsset).Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            var questState = _questManager.StartQuest(questAsset);
            var conditionInstance = questState.Objectives["obj1"].CompletionInstance as MockConditionInstance;

            conditionInstance!.SetMet(true);
            _questManager.ProcessPendingEvaluations();

            Assert.That(questState.Objectives["obj1"].Status, Is.EqualTo(ObjectiveStatus.Completed));
        }

        [Test]
        public void FailConditionMet_NonRetryable_QuestFails()
        {
            var failAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .WithFailCondition(failAsset).Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            bool questFailed = false;
            _questManager.OnQuestFailed += _ => questFailed = true;

            var questState = _questManager.StartQuest(questAsset);
            var conditionInstance = questState.Objectives["obj1"].FailInstance as MockConditionInstance;
            Assert.That(conditionInstance, Is.Not.Null);

            conditionInstance!.SetMet(true);
            _questManager.ProcessPendingEvaluations();

            Assert.That(questFailed, Is.True);
        }

        [Test]
        public void RetryableObjective_OnFailConditionMet_FiresOnObjectiveRetried()
        {
            var failAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .WithFailCondition(failAsset)
                .AsRetryable().Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            bool retriedFired = false;
            _questManager.OnObjectiveRetried += _ => retriedFired = true;

            var questState = _questManager.StartQuest(questAsset);
            var conditionInstance = questState.Objectives["obj1"].FailInstance as MockConditionInstance;

            conditionInstance!.SetMet(true);
            _questManager.ProcessPendingEvaluations();

            Assert.That(retriedFired, Is.True);
        }

        [Test]
        public void RetryableObjective_OnFailConditionMet_QuestStaysActive()
        {
            var failAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .WithFailCondition(failAsset)
                .AsRetryable().Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            var questState = _questManager.StartQuest(questAsset);
            var conditionInstance = questState.Objectives["obj1"].FailInstance as MockConditionInstance;

            conditionInstance!.SetMet(true);
            _questManager.ProcessPendingEvaluations();

            Assert.That(_questManager.ActiveQuests.Count, Is.EqualTo(1));
        }

        [Test]
        public void PrerequisiteObjective_BecomesActiveAfterPrerequisiteCompletes()
        {
            var completionAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var obj1Asset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .WithCompletionCondition(completionAsset).Build();
            var obj2Asset = new ObjectiveBuilder().WithObjectiveId("obj2")
                .AddPrerequisite(obj1Asset).Build();
            var questAsset = new QuestBuilder().AddObjective(obj1Asset).AddObjective(obj2Asset).Build();

            var questState = _questManager.StartQuest(questAsset);
            Assert.That(questState.Objectives["obj2"].Status, Is.EqualTo(ObjectiveStatus.NotStarted));

            var conditionInstance = questState.Objectives["obj1"].CompletionInstance as MockConditionInstance;
            conditionInstance!.SetMet(true);
            _questManager.ProcessPendingEvaluations();

            Assert.That(questState.Objectives["obj2"].Status, Is.EqualTo(ObjectiveStatus.InProgress));
        }

        // ── Retryable objective: fail → retry → succeed ───────────────────────

        [Test]
        public void RetryableObjective_AfterRetry_ObjectiveIsActiveAgainNotFailed()
        {
            var failAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .WithFailCondition(failAsset)
                .AsRetryable().Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            var questState = _questManager.StartQuest(questAsset);
            var failInstance = questState.Objectives["obj1"].FailInstance as MockConditionInstance;

            failInstance!.SetMet(true);
            _questManager.ProcessPendingEvaluations();

            // ActivateReadyObjectives runs as part of processing the retry result,
            // so the objective is InProgress (not Failed or NotStarted) after the call returns.
            Assert.That(questState.Objectives["obj1"].Status, Is.EqualTo(ObjectiveStatus.InProgress));
        }

        [Test]
        public void RetryableObjective_AfterRetry_ObjectiveStatusIsNotFailed()
        {
            var completionAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var failAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .WithCompletionCondition(completionAsset)
                .WithFailCondition(failAsset)
                .AsRetryable().Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            var questState = _questManager.StartQuest(questAsset);
            var failInstance = questState.Objectives["obj1"].FailInstance as MockConditionInstance;

            // Fail the objective — triggers retry
            failInstance!.SetMet(true);
            _questManager.ProcessPendingEvaluations();

            // Retry must not leave the objective in a terminal Failed state
            Assert.That(questState.Objectives["obj1"].Status, Is.Not.EqualTo(ObjectiveStatus.Failed));
        }

        [Test]
        public void RetryableObjective_FailThenSucceed_QuestCompletes()
        {
            var completionAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var failAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .WithCompletionCondition(completionAsset)
                .WithFailCondition(failAsset)
                .AsRetryable().Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            bool questCompleted = false;
            _questManager.OnQuestCompleted += _ => questCompleted = true;

            var questState = _questManager.StartQuest(questAsset);

            // Retrieve instances after StartQuest (binding creates them)
            var failInstance = questState.Objectives["obj1"].FailInstance as MockConditionInstance;
            Assert.That(failInstance, Is.Not.Null);

            // Step 1: fail → retry
            failInstance!.SetMet(true);
            _questManager.ProcessPendingEvaluations();
            Assert.That(questState.Objectives["obj1"].Status, Is.EqualTo(ObjectiveStatus.InProgress));

            // Step 2: succeed — retrieve fresh completion instance after rebind
            var completionInstance = questState.Objectives["obj1"].CompletionInstance as MockConditionInstance;
            Assert.That(completionInstance, Is.Not.Null);
            completionInstance!.SetMet(true);
            _questManager.ProcessPendingEvaluations();

            Assert.That(questCompleted, Is.True);
            Assert.That(questState.Status, Is.EqualTo(QuestStatus.Completed));
        }

        [Test]
        public void RetryableObjective_MultipleFailures_QuestRemainsActive()
        {
            var failAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .WithFailCondition(failAsset)
                .AsRetryable().Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            var questState = _questManager.StartQuest(questAsset);

            for (int i = 0; i < 3; i++)
            {
                var failInstance = questState.Objectives["obj1"].FailInstance as MockConditionInstance;
                failInstance!.SetMet(true);
                _questManager.ProcessPendingEvaluations();
                // Each retry re-activates the objective
                Assert.That(questState.Status, Is.EqualTo(QuestStatus.InProgress),
                    $"Quest should still be active after failure #{i + 1}");
            }

            Assert.That(_questManager.ActiveQuests.Count, Is.EqualTo(1));
        }

        // ── ReportObjectiveFailed ─────────────────────────────────────────────

        [Test]
        public void ReportObjectiveFailed_Retryable_FiresOnObjectiveRetried()
        {
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .AsRetryable().Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            bool retriedFired = false;
            _questManager.OnObjectiveRetried += _ => retriedFired = true;

            var questState = _questManager.StartQuest(questAsset);
            _questManager.ReportObjectiveFailed(questState.Objectives["obj1"]);

            Assert.That(retriedFired, Is.True);
        }

        [Test]
        public void ReportObjectiveFailed_Retryable_QuestStaysActive()
        {
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .AsRetryable().Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            var questState = _questManager.StartQuest(questAsset);
            _questManager.ReportObjectiveFailed(questState.Objectives["obj1"]);

            Assert.That(_questManager.ActiveQuests.Count, Is.EqualTo(1));
            Assert.That(questState.Status, Is.EqualTo(QuestStatus.InProgress));
        }

        [Test]
        public void ReportObjectiveFailed_Retryable_ObjectiveReactivatesToInProgress()
        {
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .AsRetryable().Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            var questState = _questManager.StartQuest(questAsset);
            _questManager.ReportObjectiveFailed(questState.Objectives["obj1"]);

            Assert.That(questState.Objectives["obj1"].Status, Is.EqualTo(ObjectiveStatus.InProgress));
        }

        [Test]
        public void ReportObjectiveFailed_Retryable_ResetsCompletionProgress()
        {
            var completionAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .WithCompletionCondition(completionAsset)
                .AsRetryable().Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            var questState = _questManager.StartQuest(questAsset);
            var completionInstance = questState.Objectives["obj1"].CompletionInstance as MockConditionInstance;
            completionInstance!.SetMet(true); // advance progress without processing

            _questManager.ReportObjectiveFailed(questState.Objectives["obj1"]);

            // After retry the instance is rebound; the original instance should have been reset
            Assert.That(completionInstance.IsMet, Is.False);
        }

        [Test]
        public void ReportObjectiveFailed_NonRetryable_FiresOnQuestFailed()
        {
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1").Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            QuestState? failedQuest = null;
            _questManager.OnQuestFailed += q => failedQuest = q;

            var questState = _questManager.StartQuest(questAsset);
            _questManager.ReportObjectiveFailed(questState.Objectives["obj1"]);

            Assert.That(failedQuest, Is.SameAs(questState));
        }

        [Test]
        public void ReportObjectiveFailed_NonRetryable_SetsQuestFailed()
        {
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1").Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            var questState = _questManager.StartQuest(questAsset);
            _questManager.ReportObjectiveFailed(questState.Objectives["obj1"]);

            Assert.That(questState.Status, Is.EqualTo(QuestStatus.Failed));
            Assert.That(_questManager.ActiveQuests.Count, Is.EqualTo(0));
        }

        [Test]
        public void ReportObjectiveFailed_NonRetryable_FiresOnObjectiveStatusChanged()
        {
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1").Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            ObjectiveState? changedObjective = null;
            _questManager.OnObjectiveStatusChanged += o => changedObjective = o;

            var questState = _questManager.StartQuest(questAsset);
            _questManager.ReportObjectiveFailed(questState.Objectives["obj1"]);

            Assert.That(changedObjective, Is.SameAs(questState.Objectives["obj1"]));
        }

        [Test]
        public void ReportObjectiveFailed_AlreadyTerminalQuest_IsNoOp()
        {
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1").Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            var questState = _questManager.StartQuest(questAsset);
            _questManager.FailQuest(questState);

            // Should not throw and should not fire any events
            bool anyEvent = false;
            _questManager.OnQuestFailed += _ => anyEvent = true;
            _questManager.OnObjectiveRetried += _ => anyEvent = true;

            Assert.DoesNotThrow(() => _questManager.ReportObjectiveFailed(questState.Objectives["obj1"]));
            Assert.That(anyEvent, Is.False);
        }

        [Test]
        public void ReportObjectiveFailed_AlreadyTerminalObjective_IsNoOp()
        {
            var completionAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .WithCompletionCondition(completionAsset).Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            var questState = _questManager.StartQuest(questAsset);
            var conditionInstance = questState.Objectives["obj1"].CompletionInstance as MockConditionInstance;
            conditionInstance!.SetMet(true);
            _questManager.ProcessPendingEvaluations(); // objective is now Completed

            bool anyEvent = false;
            _questManager.OnQuestFailed += _ => anyEvent = true;
            _questManager.OnObjectiveRetried += _ => anyEvent = true;

            Assert.DoesNotThrow(() => _questManager.ReportObjectiveFailed(questState.Objectives["obj1"]));
            Assert.That(anyEvent, Is.False);
        }

        [Test]
        public void ReportObjectiveFailed_Retryable_ThenSucceeds_QuestCompletes()
        {
            var completionAsset = ScriptableObject.CreateInstance<MockConditionAsset>();
            var objAsset = new ObjectiveBuilder().WithObjectiveId("obj1")
                .WithCompletionCondition(completionAsset)
                .AsRetryable().Build();
            var questAsset = new QuestBuilder().AddObjective(objAsset).Build();

            bool questCompleted = false;
            _questManager.OnQuestCompleted += _ => questCompleted = true;

            var questState = _questManager.StartQuest(questAsset);
            _questManager.ReportObjectiveFailed(questState.Objectives["obj1"]);

            // After retry, retrieve the rebound completion instance and complete it
            var completionInstance = questState.Objectives["obj1"].CompletionInstance as MockConditionInstance;
            Assert.That(completionInstance, Is.Not.Null);
            completionInstance!.SetMet(true);
            _questManager.ProcessPendingEvaluations();

            Assert.That(questCompleted, Is.True);
        }
    }
}
