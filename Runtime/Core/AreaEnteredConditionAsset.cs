using UnityEngine;
using DynamicBox.Quest.Core;

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
        private IQuestEventBus _eventBus;
        private bool _isCompleted;
        private System.Action _onChanged;

        public bool IsMet => _isCompleted;

        public AreaEnteredConditionInstance(AreaEnteredConditionAsset asset)
        {
            _asset = asset;
        }

        public void Bind(IQuestEventBus eventBus, QuestContext context, System.Action onChanged)
        {
            _eventBus = eventBus;
            _context = context;
            _onChanged = onChanged;
            
            // Subscribe to area entry events
            _eventBus.Subscribe<AreaEnteredEvent>(OnAreaEntered);
        }

        public void Unbind(IQuestEventBus eventBus, QuestContext context)
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<AreaEnteredEvent>(OnAreaEntered);
                _eventBus = null;
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

    /// <summary>
    /// Event published when the player enters a specific area/zone.
    /// </summary>
    public class AreaEnteredEvent
    {
        public string AreaId { get; set; }
        public Vector3 Position { get; set; }
        public string AreaName { get; set; }

        public AreaEnteredEvent(string areaId, Vector3 position = default, string areaName = null)
        {
            AreaId = areaId;
            Position = position;
            AreaName = areaName ?? areaId;
        }
    }
}
