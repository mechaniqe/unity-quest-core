#nullable enable
using UnityEngine;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Abstract base class for implementing IQuestInventoryService.
    /// Extend this class to create custom inventory service implementations.
    /// </summary>
    public abstract class QuestInventoryServiceBase : MonoBehaviour, IQuestInventoryService
    {
        public abstract int GetItemCount(string itemId);
        public abstract bool HasItem(string itemId, int quantity = 1);
        public abstract bool HasEverCollected(string itemId);
    }
}
