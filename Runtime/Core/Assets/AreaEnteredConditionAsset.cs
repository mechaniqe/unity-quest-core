#nullable enable
using UnityEngine;
using DynamicBox.Quest.Core.Conditions;

namespace DynamicBox.Quest.Core.Conditions
{
    /// <summary>
    /// Condition asset that completes when a specific area is entered.
    /// Listens to AreaEnteredEvent from the event system.
    /// </summary>
    [CreateAssetMenu(menuName = "DynamicBox/Quest/Conditions/Area Entered Condition", fileName = "NewAreaEnteredCondition")]
    public class AreaEnteredConditionAsset : ConditionAsset
    {
        [Header("Area Settings")]
        [SerializeField] private string _areaId = string.Empty;
        [SerializeField, TextArea(2, 3)] private string _areaDescription = string.Empty;

        public string AreaId => _areaId;
        public string AreaDescription => _areaDescription;

        public override IConditionInstance CreateInstance()
        {
            return new AreaEnteredConditionInstance(_areaId, _areaDescription);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_areaId))
            {
                _areaId = name.Replace(" ", "_").ToLower();
            }
        }
    }
}
