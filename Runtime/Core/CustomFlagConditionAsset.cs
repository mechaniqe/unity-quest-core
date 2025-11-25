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

    public class CustomFlagConditionInstance : IConditionInstance
    {
        private readonly CustomFlagConditionAsset _asset;
        private QuestContext _context;
        private IQuestEventBus _eventBus;
        private bool _isCompleted;
        private System.Action _onChanged;

        public bool IsMet => _isCompleted;

        public CustomFlagConditionInstance(CustomFlagConditionAsset asset)
        {
            _asset = asset;
        }

        public void Bind(IQuestEventBus eventBus, QuestContext context, System.Action onChanged)
        {
            _eventBus = eventBus;
            _context = context;
            _onChanged = onChanged;
            
            // Subscribe to flag change events
            _eventBus.Subscribe<FlagChangedEvent>(OnFlagChanged);
            
            // Check current flag state
            CheckCurrentState();
        }

        public void Unbind(IQuestEventBus eventBus, QuestContext context)
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<FlagChangedEvent>(OnFlagChanged);
                _eventBus = null;
            }
            _context = null;
            _onChanged = null;
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
                _onChanged?.Invoke();
                Debug.Log($"Custom flag condition completed: {_asset.FlagId} = {currentValue}");
            }
            else if (!shouldComplete && _isCompleted)
            {
                // Handle flag changing back (if needed)
                _isCompleted = false;
                _onChanged?.Invoke();
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
