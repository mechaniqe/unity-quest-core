#nullable enable
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.Samples.EventManagerIntegration
{
    /// <summary>
    /// Event published when an item is collected.
    /// Implements <c>IGameEvent</c> so it can flow through both <see cref="EventManagerAdapter"/>
    /// and the DynamicBox EventManager pipeline.
    /// </summary>
    public sealed class ItemCollectedEvent : IGameEvent
    {
        /// <summary>Gets the unique identifier of the collected item.</summary>
        public string ItemId { get; }

        /// <summary>Gets the quantity collected.</summary>
        public int Amount { get; }

        /// <param name="itemId">The unique identifier of the collected item.</param>
        /// <param name="amount">The quantity collected (default 1).</param>
        public ItemCollectedEvent(string itemId, int amount = 1)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
