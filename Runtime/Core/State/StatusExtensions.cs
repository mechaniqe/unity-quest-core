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
        /// Checks if an objective can be evaluated for progress.
        /// Returns false if either the quest or objective is in a terminal state.
        /// </summary>
        /// <param name="objectiveStatus">The objective status to check.</param>
        /// <param name="questStatus">The parent quest status to check.</param>
        /// <returns>True if both statuses allow evaluation; false otherwise.</returns>
        public static bool CanEvaluate(this ObjectiveStatus objectiveStatus, QuestStatus questStatus)
        {
            return !questStatus.IsTerminal() && !objectiveStatus.IsTerminal();
        }

        /// <summary>
        /// Checks if a status change represents a transition to completion.
        /// Useful for detecting when to trigger completion events.
        /// </summary>
        /// <param name="oldStatus">The previous objective status.</param>
        /// <param name="newStatus">The new objective status.</param>
        /// <returns>True if transitioning from non-completed to completed; false otherwise.</returns>
        public static bool TransitionedToCompleted(ObjectiveStatus oldStatus, ObjectiveStatus newStatus)
        {
            return oldStatus != ObjectiveStatus.Completed && newStatus == ObjectiveStatus.Completed;
        }

        /// <summary>
        /// Checks if a status change represents a transition to failure.
        /// Useful for detecting when to trigger failure events.
        /// </summary>
        /// <param name="oldStatus">The previous objective status.</param>
        /// <param name="newStatus">The new objective status.</param>
        /// <returns>True if transitioning from non-failed to failed; false otherwise.</returns>
        public static bool TransitionedToFailed(ObjectiveStatus oldStatus, ObjectiveStatus newStatus)
        {
            return oldStatus != ObjectiveStatus.Failed && newStatus == ObjectiveStatus.Failed;
        }
    }
}
