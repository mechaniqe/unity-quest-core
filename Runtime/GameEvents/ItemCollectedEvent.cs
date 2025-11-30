#nullable enable
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.GameEvents
{
    /// <summary>
    /// Event class that conditions can listen to when an item is collected.
    /// Games will publish this event from their item pickup systems.
    /// Immutable event following CQRS best practices.
    /// </summary>
    public sealed class ItemCollectedEvent : GameEvent
    {
        public string ItemId { get; }
        public int Amount { get; }

        public ItemCollectedEvent(string itemId, int amount = 1)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
