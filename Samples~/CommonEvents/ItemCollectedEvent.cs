#nullable enable

namespace DynamicBox.Quest.Samples.CommonEvents
{
    /// <summary>
    /// Event published when an item is collected.
    /// Publish this from your item pickup systems via <c>questManager.EventBus.Publish(...)</c>.
    /// Immutable event following CQRS best practices.
    /// </summary>
    public sealed class ItemCollectedEvent
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
