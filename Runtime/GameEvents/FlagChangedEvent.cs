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
        public string FlagId { get; }
        public bool NewValue { get; }
        public bool OldValue { get; }

        public FlagChangedEvent(string flagId, bool newValue, bool oldValue = false)
        {
            FlagId = flagId;
            NewValue = newValue;
            OldValue = oldValue;
        }
    }
}
