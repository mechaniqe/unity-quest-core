#nullable enable
using System.Linq;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Runtime state for an objective within a quest.
    /// Maintains status and references to condition instances.
    /// </summary>
    public sealed class ObjectiveState
    {
        /// <summary>
        /// Gets the designer-authored definition for this objective.
        /// </summary>
        public ObjectiveAsset Definition { get; }
        
        /// <summary>
        /// Gets the current status of this objective.
        /// </summary>
        public ObjectiveStatus Status { get; private set; } = ObjectiveStatus.NotStarted;

        internal IConditionInstance? CompletionInstance { get; }
        internal IConditionInstance? FailInstance { get; }

        /// <summary>
        /// Returns progress reporting for the completion condition, if it supports it.
        /// Use <see cref="IProgressReportingCondition.Progress"/> (0–1) for a slider
        /// and <see cref="IProgressReportingCondition.ProgressDescription"/> for a readable label
        /// (e.g. "9.5 seconds remaining").
        /// </summary>
        public IProgressReportingCondition? CompletionProgress => CompletionInstance as IProgressReportingCondition;

        /// <summary>
        /// Returns remaining-time access for the completion condition, if it is time-based.
        /// Returns <c>null</c> for non-timed objectives — use this to show/hide a countdown widget.
        /// </summary>
        public IRemainingTimeCondition? RemainingTime => CompletionInstance as IRemainingTimeCondition;

        /// <summary>
        /// Returns remaining-time access for the fail condition, if it is time-based.
        /// Returns <c>null</c> when there is no timed fail condition.
        /// </summary>
        public IRemainingTimeCondition? FailRemainingTime => FailInstance as IRemainingTimeCondition;

        public ObjectiveState(ObjectiveAsset definition)
        {
            Definition = definition;

            if (definition.CompletionCondition != null)
                CompletionInstance = definition.CompletionCondition.CreateInstance();

            if (definition.FailCondition != null)
                FailInstance = definition.FailCondition.CreateInstance();
        }

        internal void SetStatus(ObjectiveStatus status)
        {
            Status = status;
        }

        /// <summary>
        /// Returns true if the objective can make progress (not terminal and prerequisites met).
        /// </summary>
        /// <param name="parentQuest">The parent quest state to check prerequisite objectives against.</param>
        /// <returns>True if this objective can be evaluated; false if blocked by prerequisites or already terminal.</returns>
        public bool CanProgress(QuestState parentQuest)
        {
            if (Status.IsTerminal())
                return false;

            var prerequisites = Definition.Prerequisites;
            if (prerequisites == null || prerequisites.Count == 0)
                return true;

            return prerequisites
                .Where(p => p != null)
                .All(prerequisite =>
                {
                    if (!parentQuest.TryGetObjective(prerequisite.ObjectiveId, out var prerequisiteState))
                        return true; // Missing prerequisite doesn't block

                    return prerequisiteState?.Status == ObjectiveStatus.Completed;
                });
        }
    }
}
