#nullable enable
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.GameEvents
{
    /// <summary>
    /// Event published when a gameplay flag changes value.
    /// Immutable event following CQRS best practices.
    /// </summary>
    public sealed class FlagChangedEvent : GameEvent
    {
        /// <summary>
        /// Gets the unique identifier of the flag that changed.
        /// </summary>
        public string FlagId { get; }
        
        /// <summary>
        /// Gets the new value of the flag after the change.
        /// </summary>
        public bool NewValue { get; }
        
        /// <summary>
        /// Gets the previous value of the flag before the change.
        /// </summary>
        public bool OldValue { get; }

        /// <summary>
        /// Creates a new flag changed event.
        /// </summary>
        /// <param name="flagId">The unique identifier of the flag.</param>
        /// <param name="newValue">The new value of the flag.</param>
        /// <param name="oldValue">The previous value of the flag (default false).</param>
        public FlagChangedEvent(string flagId, bool newValue, bool oldValue = false)
        {
            FlagId = flagId;
            NewValue = newValue;
            OldValue = oldValue;
        }
    }
}
