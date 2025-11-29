namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Extension methods for quest and objective status enums.
    /// Provides cleaner, more readable status checking throughout the codebase.
    /// </summary>
    public static class StatusExtensions
    {
        /// <summary>
        /// Returns true if the quest is in a terminal state (Completed or Failed).
        /// </summary>
        public static bool IsTerminal(this QuestStatus status)
        {
            return status is QuestStatus.Completed or QuestStatus.Failed;
        }

        /// <summary>
        /// Returns true if the quest is actively in progress.
        /// </summary>
        public static bool IsActive(this QuestStatus status)
        {
            return status == QuestStatus.InProgress;
        }

        /// <summary>
        /// Returns true if the objective is in a terminal state (Completed or Failed).
        /// </summary>
        public static bool IsTerminal(this ObjectiveStatus status)
        {
            return status is ObjectiveStatus.Completed or ObjectiveStatus.Failed;
        }

        /// <summary>
        /// Returns true if the objective is actively in progress.
        /// </summary>
        public static bool IsActive(this ObjectiveStatus status)
        {
            return status == ObjectiveStatus.InProgress;
        }

        /// <summary>
        /// Returns true if the objective can make progress (not terminal and prerequisites met).
        /// </summary>
        public static bool CanProgress(this ObjectiveState objective, QuestState quest)
        {
            if (objective.Status.IsTerminal())
                return false;

            var prerequisites = objective.Definition.Prerequisites;
            if (prerequisites == null || prerequisites.Count == 0)
                return true;

            foreach (var prerequisite in prerequisites)
            {
                if (prerequisite == null)
                    continue;

                if (!quest.TryGetObjective(prerequisite.ObjectiveId, out var prerequisiteState))
                    continue;

                if (prerequisiteState.Status != ObjectiveStatus.Completed)
                    return false;
            }

            return true;
        }
    }
}
