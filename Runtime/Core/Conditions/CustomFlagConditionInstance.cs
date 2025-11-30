#nullable enable
using DynamicBox.Quest.GameEvents;
using UnityEngine;

namespace DynamicBox.Quest.Core.Conditions
{
    /// <summary>
    /// Condition instance that tracks custom game flags.
    /// Uses both event-driven updates (FlagChangedEvent) and service queries (IQuestFlagService).
    /// </summary>
    public sealed class CustomFlagConditionInstance : EventDrivenConditionBase<FlagChangedEvent>
    {
        private readonly string _flagId;
        private readonly bool _expectedValue;
        private readonly string? _description;
        private bool _isCompleted;
        private QuestContext? _context;

        public override bool IsMet => _isCompleted;

        public CustomFlagConditionInstance(string flagId, bool expectedValue, string? description = null)
        {
            _flagId = flagId;
            _expectedValue = expectedValue;
            _description = description;
        }

        protected override void OnBind(QuestContext context)
        {
            _context = context;
            
            // Check initial flag state from service if available
            if (context?.FlagService != null)
            {
                bool currentValue = context.FlagService.GetFlag(_flagId);
                CheckFlagValue(currentValue);
            }
            else
            {
                // Fallback: assume false if no service
                CheckFlagValue(false);
                
                Debug.LogWarning(
                    $"CustomFlagCondition for '{_flagId}' has no IQuestFlagService. " +
                    $"It will only respond to FlagChangedEvents. " +
                    $"Consider adding a FlagService to QuestPlayerRef for initial state checks.");
            }
        }

        protected override void OnUnbind(QuestContext context)
        {
            _context = null;
        }

        protected override void HandleEvent(FlagChangedEvent evt)
        {
            if (evt.FlagId == _flagId)
            {
                CheckFlagValue(evt.NewValue);
            }
        }

        private void CheckFlagValue(bool currentValue)
        {
            bool shouldComplete = (currentValue == _expectedValue);
            
            if (shouldComplete != _isCompleted)
            {
                _isCompleted = shouldComplete;
                NotifyChanged();
            }
        }

        public override string ToString()
        {
            string expectedText = _expectedValue ? "true" : "false";
            return _description ?? $"Set flag '{_flagId}' to {expectedText}";
        }
    }
}
