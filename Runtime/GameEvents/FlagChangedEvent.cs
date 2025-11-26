using DynamicBox.EventManagement;

namespace DynamicBox.Quest.GameEvents
{
    /// <summary>
    /// Event published when a gameplay flag changes value.
    /// </summary>
    public class FlagChangedEvent : GameEvent
    {
        public string FlagId { get; set; }
        public bool NewValue { get; set; }
        public bool OldValue { get; set; }

        public FlagChangedEvent(string flagId, bool newValue, bool oldValue = false)
        {
            FlagId = flagId;
            NewValue = newValue;
            OldValue = oldValue;
        }
    }
}
