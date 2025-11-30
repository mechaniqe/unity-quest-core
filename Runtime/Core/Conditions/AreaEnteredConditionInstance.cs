#nullable enable
using DynamicBox.Quest.GameEvents;
using UnityEngine;

namespace DynamicBox.Quest.Core.Conditions
{
    /// <summary>
    /// Condition instance that tracks area entry events.
    /// Uses EventDrivenConditionBase to reduce boilerplate.
    /// </summary>
    public sealed class AreaEnteredConditionInstance : EventDrivenConditionBase<AreaEnteredEvent>
    {
        private readonly string _areaId;
        private readonly string? _areaDescription;
        private bool _isCompleted;

        public override bool IsMet => _isCompleted;

        public AreaEnteredConditionInstance(string areaId, string? areaDescription = null)
        {
            _areaId = areaId;
            _areaDescription = areaDescription;
        }

        protected override void HandleEvent(AreaEnteredEvent evt)
        {
            if (evt.AreaId == _areaId && !_isCompleted)
            {
                _isCompleted = true;
                NotifyChanged();
                Debug.Log($"Area entered condition completed: {_areaId}");
            }
        }

        public override string ToString()
        {
            return $"Enter area: {_areaDescription ?? _areaId}";
        }
    }
}
