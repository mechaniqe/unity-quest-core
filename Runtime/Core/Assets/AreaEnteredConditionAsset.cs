using UnityEngine;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.GameEvents;
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.Core.Conditions
{
    [CreateAssetMenu(menuName = "DynamicBox Quest/Conditions/Area Entered Condition", fileName = "NewAreaEnteredCondition")]
    public class AreaEnteredConditionAsset : ConditionAsset
    {
        [Header("Area Settings")]
        [SerializeField] private string _areaId;
        [SerializeField, TextArea(2, 3)] private string _areaDescription;

        public string AreaId => _areaId;
        public string AreaDescription => _areaDescription;

        public override IConditionInstance CreateInstance()
        {
            return new AreaEnteredConditionInstance(this);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_areaId))
            {
                _areaId = name.Replace(" ", "_").ToLower();
            }
        }
    }

    public class AreaEnteredConditionInstance : IConditionInstance
    {
        private readonly AreaEnteredConditionAsset _asset;
        private QuestContext _context;
        private EventManager _eventManager;
        private bool _isCompleted;
        private System.Action _onChanged;
        private EventManager.EventDelegate<AreaEnteredEvent> _eventHandler;

        public bool IsMet => _isCompleted;

        public AreaEnteredConditionInstance(AreaEnteredConditionAsset asset)
        {
            _asset = asset;
        }

        public void Bind(EventManager eventManager, QuestContext context, System.Action onChanged)
        {
            _eventManager = eventManager;
            _context = context;
            _onChanged = onChanged;
            
            // Create and store the event handler delegate
            _eventHandler = OnAreaEntered;
            
            // Subscribe to area entry events
            _eventManager.AddListener<AreaEnteredEvent>(_eventHandler);
        }

        public void Unbind(EventManager eventManager, QuestContext context)
        {
            if (_eventManager != null && _eventHandler != null)
            {
                _eventManager.RemoveListener<AreaEnteredEvent>(_eventHandler);
                _eventManager = null;
                _eventHandler = null;
            }
            _context = null;
            _onChanged = null;
        }

        private void OnAreaEntered(AreaEnteredEvent evt)
        {
            if (evt.AreaId == _asset.AreaId && !_isCompleted)
            {
                _isCompleted = true;
                _onChanged?.Invoke();
                Debug.Log($"Area entered condition completed: {_asset.AreaId}");
            }
        }

        public override string ToString()
        {
            return $"Enter area: {_asset.AreaDescription ?? _asset.AreaId}";
        }
    }
}
