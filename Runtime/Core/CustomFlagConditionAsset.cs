using UnityEngine;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Core.Conditions
{
    [CreateAssetMenu(menuName = "DynamicBox Quest/Conditions/Custom Flag Condition", fileName = "NewCustomFlagCondition")]
    public class CustomFlagConditionAsset : ConditionAsset
    {
        [Header("Flag Settings")]
        [SerializeField] private string _flagId;
        [SerializeField] private bool _expectedValue = true;
        [SerializeField, TextArea(2, 3)] private string _description;

        public string FlagId => _flagId;
        public bool ExpectedValue => _expectedValue;
        public string Description => _description;

        public override IConditionInstance CreateInstance(QuestContext context)
        {
            return new CustomFlagConditionInstance(this, context);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_flagId))
            {
                _flagId = name.Replace(" ", "_").ToLower();
            }
        }
    }

    public class CustomFlagConditionInstance : IConditionInstance
    {
        private readonly CustomFlagConditionAsset _asset;
        private readonly QuestContext _context;
        private bool _isCompleted;

        public bool IsCompleted => _isCompleted;

        public CustomFlagConditionInstance(CustomFlagConditionAsset asset, QuestContext context)
        {
            _asset = asset;
            _context = context;
            
            // Subscribe to flag change events
            context.EventBus.Subscribe<FlagChangedEvent>(OnFlagChanged);
            
            // Check current flag state
            CheckCurrentState();
        }

        public void Dispose()
        {
            _context.EventBus.Unsubscribe<FlagChangedEvent>(OnFlagChanged);
        }

        private void OnFlagChanged(FlagChangedEvent evt)
        {
            if (evt.FlagId == _asset.FlagId)
            {
                CheckFlagValue(evt.NewValue);
            }
        }

        private void CheckCurrentState()
        {
            // In a real game, this would check against a persistent flag system
            // For now, we assume flags start as false
            CheckFlagValue(false);
        }

        private void CheckFlagValue(bool currentValue)
        {
            bool shouldComplete = (currentValue == _asset.ExpectedValue);
            
            if (shouldComplete && !_isCompleted)
            {
                _isCompleted = true;
                Debug.Log($"Custom flag condition completed: {_asset.FlagId} = {currentValue}");
            }
            else if (!shouldComplete && _isCompleted)
            {
                // Handle flag changing back (if needed)
                _isCompleted = false;
            }
        }

        public override string ToString()
        {
            string expectedText = _asset.ExpectedValue ? "true" : "false";
            return _asset.Description ?? $"Set flag '{_asset.FlagId}' to {expectedText}";
        }
    }

    /// <summary>
    /// Event published when a gameplay flag changes value.
    /// </summary>
    public class FlagChangedEvent
    {
        public string FlagId { get; set; }
        public bool NewValue { get; set; }
        public bool OldValue { get; set; }

        public FlagChangedEvent(string flagId, bool newValue, bool oldValue = false)
        {
            FlagId = flagId;
            NewValue = newValue;
            OldValue = oldValue;
        }
    }
}
