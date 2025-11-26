using UnityEngine;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.GameEvents;
using DynamicBox.EventManagement;

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
        private EventManager _eventManager;
        private bool _isCompleted;
        private System.Action _onChanged;
        private EventManager.EventDelegate<FlagChangedEvent> _eventHandler;

        public bool IsMet => _isCompleted;

        public CustomFlagConditionInstance(CustomFlagConditionAsset asset)
        {
            _asset = asset;
        }

        public void Bind(EventManager eventManager, QuestContext context, System.Action onChanged)
        {
            _eventManager = eventManager;
            _context = context;
            _onChanged = onChanged;
            
            // Create and store the event handler delegate
            _eventHandler = OnFlagChanged;
            
            // Subscribe to flag change events
            _eventManager.AddListener<FlagChangedEvent>(_eventHandler);
            
            // Check current flag state
            CheckCurrentState();
        }

        public void Unbind(EventManager eventManager, QuestContext context)
        {
            if (_eventManager != null && _eventHandler != null)
            {
                _eventManager.RemoveListener<FlagChangedEvent>(_eventHandler);
                _eventManager = null;
                _eventHandler = null;
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
}
