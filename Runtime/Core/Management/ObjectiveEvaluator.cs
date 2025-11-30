#nullable enable
using System;
using System.Linq;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Handles objective and quest evaluation logic.
    /// Extracted from QuestManager to follow Single Responsibility Principle.
    /// </summary>
    internal sealed class ObjectiveEvaluator
    {
        private readonly QuestLog _log;
        private readonly ConditionBindingService _bindingService;
        private Action<QuestState, ObjectiveState>? _onDirtyCallback;

        public ObjectiveEvaluator(QuestLog log, ConditionBindingService bindingService)
        {
            _log = log;
            _bindingService = bindingService;
        }

        /// <summary>
        /// Sets the callback to invoke when an objective becomes dirty and needs re-evaluation.
        /// </summary>
        /// <param name="callback">The callback action to invoke with quest and objective state.</param>
        public void SetDirtyCallback(Action<QuestState, ObjectiveState> callback)
        {
            _onDirtyCallback = callback;
        }

        /// <summary>
        /// Evaluates an objective and updates quest state accordingly.
        /// Checks fail conditions first, then completion conditions.
        /// </summary>
        /// <param name="quest">The quest containing the objective.</param>
        /// <param name="objective">The objective to evaluate.</param>
        /// <returns>The result of the evaluation indicating what changed.</returns>
        public QuestEvaluationResult Evaluate(QuestState quest, ObjectiveState objective)
        {
            if (quest.Status.IsTerminal())
                return QuestEvaluationResult.NoChange;

            if (!objective.CanProgress(quest))
                return QuestEvaluationResult.NoChange;

            // Check failure condition first
            if (objective.FailInstance != null && objective.FailInstance.IsMet)
            {
                _bindingService.UnbindObjective(objective);
                objective.SetStatus(ObjectiveStatus.Failed);
                
                quest.SetStatus(QuestStatus.Failed);
                _log.RemoveQuest(quest);
                
                return QuestEvaluationResult.QuestFailed;
            }

            // Check completion condition
            if (objective.CompletionInstance != null && objective.CompletionInstance.IsMet)
            {
                if (objective.Status == ObjectiveStatus.NotStarted)
                {
                    objective.SetStatus(ObjectiveStatus.InProgress);
                }

                _bindingService.UnbindObjective(objective);
                objective.SetStatus(ObjectiveStatus.Completed);
                
                return CheckQuestCompletion(quest);
            }

            return QuestEvaluationResult.NoChange;
        }

        /// <summary>
        /// Activates objectives that have their prerequisites met.
        /// </summary>
        public void ActivateReadyObjectives(QuestState quest)
        {
            foreach (var obj in quest.GetObjectiveStates())
            {
                if (obj.Status == ObjectiveStatus.NotStarted && obj.CanProgress(quest))
                {
                    obj.SetStatus(ObjectiveStatus.InProgress);
                    _bindingService.BindObjective(quest, obj, () => _onDirtyCallback?.Invoke(quest, obj));
                }
            }
        }

        private QuestEvaluationResult CheckQuestCompletion(QuestState quest)
        {
            var allRequiredComplete = quest.GetObjectiveStates()
                .Where(o => !o.Definition.IsOptional)
                .All(o => o.Status == ObjectiveStatus.Completed);

            if (allRequiredComplete)
            {
                quest.SetStatus(QuestStatus.Completed);
                _bindingService.UnbindQuest(quest);
                _log.RemoveQuest(quest);
                
                return QuestEvaluationResult.QuestCompleted;
            }

            // Some objectives completed, might activate new ones
            return QuestEvaluationResult.ObjectiveCompleted;
        }
    }

    /// <summary>
    /// Result of an objective evaluation.
    /// </summary>
    internal enum QuestEvaluationResult
    {
        NoChange,
        ObjectiveCompleted,
        QuestCompleted,
        QuestFailed
    }
}
