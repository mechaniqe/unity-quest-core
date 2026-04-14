#nullable enable
using DynamicBox.Quest.Core;
using UnityEngine;

namespace DynamicBox.Quest.Samples.CommonEvents
{
    /// <summary>
    /// ScriptableObject asset for the <see cref="CustomFlagConditionInstance"/>.
    /// The condition ID is used as the flag ID to watch.
    /// </summary>
    [CreateAssetMenu(menuName = "DynamicBox/Quest/Common Conditions/Custom Flag", fileName = "NewCustomFlagCondition")]
    public class CustomFlagConditionAsset : ConditionAsset
    {
        [SerializeField] private bool _expectedValue = true;
        [SerializeField, TextArea(2, 3)] private string _description = string.Empty;

        public bool ExpectedValue => _expectedValue;
        public string Description => _description;

        public override IConditionInstance CreateInstance() =>
            new CustomFlagConditionInstance(ConditionId, _expectedValue, _description);
    }
}
