using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace GenericQuest.Core
{
    public enum ConditionOperator
    {
        And,
        Or
    }

    [CreateAssetMenu(menuName = "Quests/Condition Group", fileName = "NewConditionGroup")]
    public class ConditionGroupAsset : ConditionAsset
    {
        [SerializeField] private ConditionOperator @operator = ConditionOperator.And;
        [SerializeField] private List<ConditionAsset> children = new();

        public override IConditionInstance CreateInstance()
        {
            var instances = children
                .Where(c => c != null)
                .Select(c => c.CreateInstance())
                .ToList();

            return new ConditionGroupInstance(@operator, instances);
        }
    }
}
