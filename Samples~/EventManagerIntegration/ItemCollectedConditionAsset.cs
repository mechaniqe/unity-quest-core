#nullable enable
using DynamicBox.Quest.Core;
using UnityEngine;

namespace DynamicBox.Quest.Samples.EventManagerIntegration
{
    [CreateAssetMenu(menuName = "DynamicBox/Quest/EventManager Conditions/Item Collected", fileName = "NewItemCollectedCondition")]
    public class ItemCollectedConditionAsset : ConditionAsset
    {
        [SerializeField] private int requiredCount = 1;

        public int RequiredCount => requiredCount;

        public override IConditionInstance CreateInstance() =>
            new ItemCollectedConditionInstance(ConditionId, requiredCount);
    }
}
