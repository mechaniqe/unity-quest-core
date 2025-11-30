#nullable enable
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Logical operator for combining multiple conditions.
    /// </summary>
    public enum ConditionOperator
    {
        /// <summary>
        /// All child conditions must be met for the group to succeed.
        /// </summary>
        And,
        
        /// <summary>
        /// At least one child condition must be met for the group to succeed.
        /// </summary>
        Or
    }

    /// <summary>
    /// Composite condition that combines multiple child conditions with AND/OR logic.
    /// Allows building complex condition logic from simple pieces.
    /// </summary>
    [CreateAssetMenu(menuName = "DynamicBox/Quest/Condition Group", fileName = "NewConditionGroup")]
    public class ConditionGroupAsset : ConditionAsset
    {
        [SerializeField] private ConditionOperator @operator = ConditionOperator.And;
        [SerializeField] private List<ConditionAsset> children = new();

        /// <summary>
        /// Gets the logical operator used to combine child conditions.
        /// </summary>
        public ConditionOperator Operator => @operator;
        
        /// <summary>
        /// Gets the list of child conditions in this group.
        /// </summary>
        public IReadOnlyList<ConditionAsset> Conditions => children;

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
