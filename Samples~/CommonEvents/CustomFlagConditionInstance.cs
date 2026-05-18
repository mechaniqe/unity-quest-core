#nullable enable
using DynamicBox.Quest.Core;
using UnityEngine;

namespace DynamicBox.Quest.Samples.CommonEvents
{
    /// <summary>
    /// Condition that tracks a custom gameplay flag and completes when it matches an expected value.
    /// Checks initial flag state via <see cref="IQuestFlagService"/> if available.
    /// </summary>
    public sealed class CustomFlagConditionInstance : EventDrivenConditionBase<FlagChangedEvent>
    {
        private readonly string _flagId;
        private readonly bool _expectedValue;
        private readonly string? _description;
        private bool _isCompleted;

        public override bool IsMet => _isCompleted;

        public CustomFlagConditionInstance(string flagId, bool expectedValue, string? description = null)
        {
            _flagId = flagId;
            _expectedValue = expectedValue;
            _description = description;
        }

        protected override void OnBind(QuestContext context)
        {
            if (context?.FlagService != null)
                CheckFlagValue(context.FlagService.GetFlag(_flagId));
            else
                CheckFlagValue(false);
        }

        protected override void HandleEvent(FlagChangedEvent evt)
        {
            if (evt.FlagId == _flagId)
                CheckFlagValue(evt.NewValue);
        }

        private void CheckFlagValue(bool currentValue)
        {
            bool shouldComplete = currentValue == _expectedValue;
            if (shouldComplete != _isCompleted)
            {
                _isCompleted = shouldComplete;
                NotifyChanged();
            }
        }

        public override void Reset() => _isCompleted = false;

        public override string ToString()
        {
            string expectedText = _expectedValue ? "true" : "false";
            return _description ?? $"Set flag '{_flagId}' to {expectedText}";
        }
    }
}
