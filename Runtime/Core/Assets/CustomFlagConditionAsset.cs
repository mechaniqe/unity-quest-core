#nullable enable
using UnityEngine;
using DynamicBox.Quest.Core.Conditions;

namespace DynamicBox.Quest.Core.Conditions
{
    /// <summary>
    /// Condition asset that completes when a custom flag matches an expected value.
    /// Listens to FlagChangedEvent from the event system.
    /// </summary>
    [CreateAssetMenu(menuName = "DynamicBox/Quest/Conditions/Custom Flag Condition", fileName = "NewCustomFlagCondition")]
    public class CustomFlagConditionAsset : ConditionAsset
    {
        [Header("Flag Settings")]
        [SerializeField] private string _flagId = string.Empty;
        [SerializeField] private bool _expectedValue = true;
        [SerializeField, TextArea(2, 3)] private string _description = string.Empty;

        /// <summary>
        /// Gets the unique identifier of the flag to check.
        /// </summary>
        public string FlagId => _flagId;
        
        /// <summary>
        /// Gets the expected boolean value for this flag to complete the condition.
        /// </summary>
        public bool ExpectedValue => _expectedValue;
        
        /// <summary>
        /// Gets the descriptive text for this flag condition (optional).
        /// </summary>
        public string Description => _description;

        public override IConditionInstance CreateInstance()
        {
            return new CustomFlagConditionInstance(_flagId, _expectedValue, _description);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_flagId))
            {
                _flagId = name.Replace(" ", "_").ToLower();
            }
        }
    }
}
