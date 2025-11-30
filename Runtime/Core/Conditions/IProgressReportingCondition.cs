#nullable enable

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Optional interface for conditions that can report progress information.
    /// Useful for UI elements that display quest/objective progress.
    /// </summary>
    public interface IProgressReportingCondition
    {
        /// <summary>
        /// Gets the current progress as a normalized value (0.0 to 1.0).
        /// Returns 0.0 when no progress has been made, 1.0 when complete.
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// Gets a human-readable description of the current progress.
        /// For example: "3/5 items collected" or "15 seconds remaining"
        /// </summary>
        string ProgressDescription { get; }
    }
}
