#nullable enable
using DynamicBox.Quest.GameEvents;
using UnityEngine;

namespace DynamicBox.Quest.Core.Conditions
{
    /// <summary>
    /// Condition that tracks item collection events and completes when required count is reached.
    /// Uses EventDrivenConditionBase to reduce boilerplate and implements progress reporting.
    /// </summary>
    public sealed class ItemCollectedConditionInstance : EventDrivenConditionBase<ItemCollectedEvent>, IProgressReportingCondition
    {
        private readonly string _itemId;
        private readonly int _requiredCount;
        private int _currentCount;

        public override bool IsMet => _currentCount >= _requiredCount;

        public float Progress => _requiredCount > 0 ? Mathf.Clamp01((float)_currentCount / _requiredCount) : 1f;
        
        public string ProgressDescription => $"{_currentCount}/{_requiredCount} items collected";

        public ItemCollectedConditionInstance(string itemId, int requiredCount)
        {
            _itemId = itemId;
            _requiredCount = requiredCount;
            _currentCount = 0;
        }

        protected override void HandleEvent(ItemCollectedEvent evt)
        {
            if (evt.ItemId != _itemId)
                return;

            int oldCount = _currentCount;
            _currentCount += evt.Amount;

            // Notify only if the met status changed or we're still tracking progress
            if (oldCount < _requiredCount && _currentCount >= _requiredCount)
            {
                NotifyChanged();
            }
            else if (oldCount < _requiredCount && _currentCount < _requiredCount)
            {
                NotifyChanged(); // Progress update
            }
            // If already met and still met, no notification needed
        }
    }
}
