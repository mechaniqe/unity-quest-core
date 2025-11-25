using UnityEngine;

namespace GenericQuest.Core.Conditions
{
    [CreateAssetMenu(menuName = "Quests/Conditions/Item Collected", fileName = "NewItemCollectedCondition")]
    public class ItemCollectedConditionAsset : ConditionAsset
    {
        [SerializeField] private string itemId;
        [SerializeField] private int requiredCount = 1;

        public string ItemId => itemId;
        public int RequiredCount => requiredCount;

        public override IConditionInstance CreateInstance()
        {
            return new ItemCollectedConditionInstance(itemId, requiredCount);
        }
    }
}
