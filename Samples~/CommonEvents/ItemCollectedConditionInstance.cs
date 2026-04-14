#nullable enable
using DynamicBox.Quest.Core;
using UnityEngine;

namespace DynamicBox.Quest.Samples.CommonEvents
{
    /// <summary>
    /// Condition that tracks item collection and completes when the required count is reached.
    /// Implements progress reporting so objectives can show a progress bar.
    /// </summary>
    public sealed class ItemCollectedConditionInstance : EventDrivenConditionBase<ItemCollectedEvent>, IProgressReportingCondition
    {
        private readonly string _itemId;
        private readonly int _requiredCount;
        private int _currentCount;

        public override bool IsMet => _currentCount >= _requiredCount;

        public int CurrentCount => _currentCount;
        public int RequiredCount => _requiredCount;
        public float Progress => _requiredCount > 0 ? Mathf.Clamp01((float)_currentCount / _requiredCount) : 1f;
        public string ProgressDescription => $"{_currentCount}/{_requiredCount} items collected";

        public ItemCollectedConditionInstance(string itemId, int requiredCount)
        {
            _itemId = itemId;
            _requiredCount = requiredCount;
        }

        protected override void HandleEvent(ItemCollectedEvent evt)
        {
            if (evt.ItemId != _itemId)
                return;

            int oldCount = _currentCount;
            _currentCount += evt.Amount;

            if (oldCount < _requiredCount)
                NotifyChanged();
        }
    }
}
