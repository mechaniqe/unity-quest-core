#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace DynamicBox.Quest.Core.Services
{
    /// <summary>
    /// Example implementation of IQuestInventoryService.
    /// Provides a simple item tracking system for quests.
    /// For production, integrate with your actual inventory system.
    /// </summary>
    public class SimpleInventoryService : QuestInventoryServiceBase
    {
        private readonly Dictionary<string, int> _inventory = new();
        private readonly HashSet<string> _everCollected = new();

        [Header("Debug")]
        [SerializeField] private bool logInventoryChanges = false;

        public override int GetItemCount(string itemId)
        {
            return _inventory.TryGetValue(itemId, out int count) ? count : 0;
        }

        public override bool HasItem(string itemId, int quantity = 1)
        {
            return GetItemCount(itemId) >= quantity;
        }

        public override bool HasEverCollected(string itemId)
        {
            return _everCollected.Contains(itemId);
        }

        /// <summary>
        /// Adds items to the inventory (call from your item pickup logic).
        /// </summary>
        public void AddItem(string itemId, int quantity = 1)
        {
            if (quantity <= 0)
                return;

            int oldCount = GetItemCount(itemId);
            int newCount = oldCount + quantity;
            _inventory[itemId] = newCount;
            _everCollected.Add(itemId);

            if (logInventoryChanges)
            {
                Debug.Log($"[QuestInventory] Added {quantity}x '{itemId}' ({oldCount} → {newCount})");
            }
        }

        /// <summary>
        /// Removes items from the inventory.
        /// </summary>
        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (quantity <= 0)
                return false;

            int currentCount = GetItemCount(itemId);
            if (currentCount < quantity)
                return false;

            int newCount = currentCount - quantity;
            if (newCount <= 0)
            {
                _inventory.Remove(itemId);
            }
            else
            {
                _inventory[itemId] = newCount;
            }

            if (logInventoryChanges)
            {
                Debug.Log($"[QuestInventory] Removed {quantity}x '{itemId}' ({currentCount} → {newCount})");
            }

            return true;
        }

        /// <summary>
        /// Gets all items currently in inventory (for debugging or UI).
        /// </summary>
        public IReadOnlyDictionary<string, int> GetAllItems() => _inventory;

        /// <summary>
        /// Clears the inventory (useful for testing or new game).
        /// </summary>
        public void ClearInventory()
        {
            _inventory.Clear();
            _everCollected.Clear();
            
            if (logInventoryChanges)
            {
                Debug.Log("[QuestInventory] Inventory cleared");
            }
        }
    }
}
