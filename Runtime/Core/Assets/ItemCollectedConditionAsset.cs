#nullable enable
using UnityEngine;

namespace DynamicBox.Quest.Core.Conditions
{
    /// <summary>
    /// Condition asset that completes when a specific item is collected.
    /// Listens to ItemCollectedEvent from the event system.
    /// </summary>
    [CreateAssetMenu(menuName = "DynamicBox/Quest/Conditions/Item Collected", fileName = "NewItemCollectedCondition")]
    public class ItemCollectedConditionAsset : ConditionAsset
    {
        [SerializeField] private string itemId = string.Empty;
        [SerializeField] private int requiredCount = 1;

        public string ItemId => itemId;
        public int RequiredCount => requiredCount;

        public override IConditionInstance CreateInstance()
        {
            return new ItemCollectedConditionInstance(itemId, requiredCount);
        }
    }
}
