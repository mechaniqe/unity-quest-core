#nullable enable
using System.Linq;

namespace DynamicBox.Quest.Core
{
    public sealed class ObjectiveState
    {
        public ObjectiveAsset Definition { get; }
        public ObjectiveStatus Status { get; private set; } = ObjectiveStatus.NotStarted;

        internal IConditionInstance? CompletionInstance { get; }
        internal IConditionInstance? FailInstance { get; }

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
