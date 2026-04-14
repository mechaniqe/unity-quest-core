#nullable enable
using DynamicBox.Quest.Core;
using UnityEngine;

namespace DynamicBox.Quest.Samples.EventManagerIntegration
{
    [CreateAssetMenu(menuName = "DynamicBox/Quest/EventManager Conditions/Area Entered", fileName = "NewAreaEnteredCondition")]
    public class AreaEnteredConditionAsset : ConditionAsset
    {
        [SerializeField, TextArea(2, 3)] private string _areaDescription = string.Empty;

        public string AreaDescription => _areaDescription;

        public override IConditionInstance CreateInstance() =>
            new AreaEnteredConditionInstance(ConditionId, _areaDescription);
    }
}
