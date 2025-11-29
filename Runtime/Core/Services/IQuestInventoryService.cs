#nullable enable

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Service interface for inventory and item collection tracking.
    /// Implement this interface in your game's inventory system.
    /// </summary>
    public interface IQuestInventoryService
    {
        /// <summary>
        /// Gets the quantity of a specific item in the player's inventory.
        /// </summary>
        int GetItemCount(string itemId);

        /// <summary>
        /// Checks if the player has at least the specified quantity of an item.
        /// </summary>
        bool HasItem(string itemId, int quantity = 1);

        /// <summary>
        /// Checks if the player has collected an item at any point (even if consumed).
        /// Useful for "have you ever found this" type quests.
        /// </summary>
        bool HasEverCollected(string itemId);
    }
}
