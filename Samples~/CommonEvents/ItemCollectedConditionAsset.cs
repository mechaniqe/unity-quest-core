#nullable enable
using DynamicBox.Quest.Core;
using UnityEngine;

namespace DynamicBox.Quest.Samples.CommonEvents
{
    /// <summary>
    /// ScriptableObject asset for the <see cref="ItemCollectedConditionInstance"/>.
    /// Configure the item ID via the parent <see cref="ConditionAsset.ConditionId"/> and the required count below.
    /// </summary>
    [CreateAssetMenu(menuName = "DynamicBox/Quest/Common Conditions/Item Collected", fileName = "NewItemCollectedCondition")]
    public class ItemCollectedConditionAsset : ConditionAsset
    {
        [SerializeField] private int requiredCount = 1;

        public int RequiredCount => requiredCount;

        public override IConditionInstance CreateInstance() =>
            new ItemCollectedConditionInstance(ConditionId, requiredCount);
    }
}
