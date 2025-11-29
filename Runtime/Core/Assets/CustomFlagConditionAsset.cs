#nullable enable
using UnityEngine;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.GameEvents;
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.Core.Conditions
{
    [CreateAssetMenu(menuName = "DynamicBox/Quest/Conditions/Custom Flag Condition", fileName = "NewCustomFlagCondition")]
    public class CustomFlagConditionAsset : ConditionAsset
    {
        [Header("Flag Settings")]
        [SerializeField] private string _flagId = string.Empty;
        [SerializeField] private bool _expectedValue = true;
        [SerializeField, TextArea(2, 3)] private string _description = string.Empty;

        public string FlagId => _flagId;
        public bool ExpectedValue => _expectedValue;
        public string Description => _description;

        public override IConditionInstance CreateInstance()
        {
            return new CustomFlagConditionInstance(this);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_flagId))
            {
                _flagId = name.Replace(" ", "_").ToLower();
            }
        }
    }

    /// <summary>
    /// Condition instance that tracks custom game flags.
    /// Uses both event-driven updates (FlagChangedEvent) and service queries (IQuestFlagService).
    /// </summary>
    public class CustomFlagConditionInstance : EventDrivenConditionBase<FlagChangedEvent>
    {
        private readonly CustomFlagConditionAsset _asset;
        private bool _isCompleted;
        private QuestContext? _context;

        public override bool IsMet => _isCompleted;

        public CustomFlagConditionInstance(CustomFlagConditionAsset asset)
        {
            _asset = asset;
        }

        protected override void OnBind(QuestContext context)
        {
            _context = context;
            
            // Check initial flag state from service if available
            if (context?.FlagService != null)
            {
                bool currentValue = context.FlagService.GetFlag(_asset.FlagId);
                CheckFlagValue(currentValue);
            }
            else
            {
                // Fallback: assume false if no service
                CheckFlagValue(false);
                
                UnityEngine.Debug.LogWarning(
                    $"CustomFlagCondition for '{_asset.FlagId}' has no IQuestFlagService. " +
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
            if (evt.FlagId == _asset.FlagId)
            {
                CheckFlagValue(evt.NewValue);
            }
        }

        private void CheckFlagValue(bool currentValue)
        {
            bool shouldComplete = (currentValue == _asset.ExpectedValue);
            
            if (shouldComplete != _isCompleted)
            {
                _isCompleted = shouldComplete;
                NotifyChanged();
                
                if (_isCompleted)
                {
                    UnityEngine.Debug.Log($"Custom flag condition completed: {_asset.FlagId} = {currentValue}");
                }
            }
        }

        public override string ToString()
        {
            string expectedText = _asset.ExpectedValue ? "true" : "false";
            return _asset.Description ?? $"Set flag '{_asset.FlagId}' to {expectedText}";
        }
    }
}
