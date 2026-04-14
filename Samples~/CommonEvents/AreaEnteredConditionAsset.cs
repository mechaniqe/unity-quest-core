#nullable enable
using DynamicBox.Quest.Core;
using UnityEngine;

namespace DynamicBox.Quest.Samples.CommonEvents
{
    /// <summary>
    /// ScriptableObject asset for the <see cref="AreaEnteredConditionInstance"/>.
    /// The condition ID is used as the area ID to match against incoming <see cref="AreaEnteredEvent"/>s.
    /// </summary>
    [CreateAssetMenu(menuName = "DynamicBox/Quest/Common Conditions/Area Entered", fileName = "NewAreaEnteredCondition")]
    public class AreaEnteredConditionAsset : ConditionAsset
    {
        [SerializeField, TextArea(2, 3)] private string _areaDescription = string.Empty;

        public string AreaDescription => _areaDescription;

        public override IConditionInstance CreateInstance() =>
            new AreaEnteredConditionInstance(ConditionId, _areaDescription);
    }
}
