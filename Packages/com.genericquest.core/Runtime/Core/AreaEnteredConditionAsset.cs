using UnityEngine;
using GenericQuest.Core;

namespace GenericQuest.Core.Conditions
{
    [CreateAssetMenu(menuName = "Generic Quest/Conditions/Area Entered Condition", fileName = "NewAreaEnteredCondition")]
    public class AreaEnteredConditionAsset : ConditionAsset
    {
        [Header("Area Settings")]
        [SerializeField] private string _areaId;
        [SerializeField, TextArea(2, 3)] private string _areaDescription;

        public string AreaId => _areaId;
        public string AreaDescription => _areaDescription;

        public override IConditionInstance CreateInstance(QuestContext context)
        {
            return new AreaEnteredConditionInstance(this, context);
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
        private readonly QuestContext _context;
        private bool _isCompleted;

        public bool IsCompleted => _isCompleted;

        public AreaEnteredConditionInstance(AreaEnteredConditionAsset asset, QuestContext context)
        {
            _asset = asset;
            _context = context;
            
            // Subscribe to area entry events
            context.EventBus.Subscribe<AreaEnteredEvent>(OnAreaEntered);
        }

        public void Dispose()
        {
            _context.EventBus.Unsubscribe<AreaEnteredEvent>(OnAreaEntered);
        }

        private void OnAreaEntered(AreaEnteredEvent evt)
        {
            if (evt.AreaId == _asset.AreaId && !_isCompleted)
            {
                _isCompleted = true;
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
