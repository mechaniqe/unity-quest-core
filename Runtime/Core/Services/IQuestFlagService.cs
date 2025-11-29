#nullable enable

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Service interface for custom game flags and persistent state.
    /// Implement this interface to provide quest-related flag tracking.
    /// </summary>
    public interface IQuestFlagService
    {
        /// <summary>
        /// Gets the value of a boolean flag.
        /// </summary>
        bool GetFlag(string flagId);

        /// <summary>
        /// Sets the value of a boolean flag.
        /// </summary>
        void SetFlag(string flagId, bool value);

        /// <summary>
        /// Gets the value of an integer flag/counter.
        /// </summary>
        int GetCounter(string counterId);

        /// <summary>
        /// Sets the value of an integer flag/counter.
        /// </summary>
        void SetCounter(string counterId, int value);

        /// <summary>
        /// Increments an integer counter and returns the new value.
        /// </summary>
        int IncrementCounter(string counterId, int amount = 1);

        /// <summary>
        /// Checks if a flag has ever been set (even if currently false).
        /// Useful for tracking "have you ever" conditions.
        /// </summary>
        bool HasFlagBeenSet(string flagId);
    }
}
