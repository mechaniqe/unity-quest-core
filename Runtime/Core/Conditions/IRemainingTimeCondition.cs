#nullable enable

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Optional interface for conditions that track a countdown to completion.
    /// Exposes the raw remaining seconds so UI can format or animate it freely.
    /// </summary>
    public interface IRemainingTimeCondition
    {
        /// <summary>
        /// Gets the number of seconds remaining until the condition is met.
        /// Returns 0 when the condition is already met.
        /// </summary>
        float RemainingSeconds { get; }
    }
}
