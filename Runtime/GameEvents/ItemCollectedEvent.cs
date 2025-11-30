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
        /// <summary>
        /// Gets the unique identifier of the item that was collected.
        /// </summary>
        public string ItemId { get; }
        
        /// <summary>
        /// Gets the quantity of the item that was collected.
        /// </summary>
        public int Amount { get; }

        /// <summary>
        /// Creates a new item collected event.
        /// </summary>
        /// <param name="itemId">The unique identifier of the collected item.</param>
        /// <param name="amount">The quantity collected (default 1).</param>
        public ItemCollectedEvent(string itemId, int amount = 1)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
