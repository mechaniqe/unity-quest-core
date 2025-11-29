#nullable enable

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Service interface for time-based quest conditions.
    /// Implement this interface to provide game time tracking.
    /// </summary>
    public interface IQuestTimeService
    {
        /// <summary>
        /// Gets the total elapsed game time in seconds since the game started.
        /// Used by time-based conditions for polling.
        /// </summary>
        float TotalGameTime { get; }

        /// <summary>
        /// Gets the delta time since the last frame in seconds.
        /// Used for incremental time tracking in polling conditions.
        /// </summary>
        float DeltaTime { get; }

        /// <summary>
        /// Gets the current in-game time of day (0-24 hours).
        /// Useful for "wait until dawn" type quests.
        /// </summary>
        float TimeOfDay { get; }

        /// <summary>
        /// Gets the current in-game day number.
        /// Useful for "survive N days" type quests.
        /// </summary>
        int CurrentDay { get; }
    }
}
