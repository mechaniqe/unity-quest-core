using System;
using DynamicBox.EventManagement;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Conditions;
using DynamicBox.Quest.GameEvents;
using UnityEngine;

namespace DynamicBox.Quest.Samples
{
    /// <summary>
    /// Example of creating a custom condition for enemy kills.
    /// Shows the minimal code needed to extend the quest system.
    /// </summary>
    /// 
    // 1. Create the asset (designer-facing)
    [CreateAssetMenu(menuName = "Quest Samples/Conditions/Enemy Killed")]
    public class EnemyKilledCondition : ConditionAsset
    {
        [SerializeField] private string enemyType = "Goblin";
        [SerializeField] private int requiredKills = 3;

        public override IConditionInstance CreateInstance()
        {
            return new EnemyKilledConditionInstance(ConditionId, enemyType, requiredKills);
        }
    }

    // 2. Create the instance (runtime logic)
    public class EnemyKilledConditionInstance : IConditionInstance
    {
        private readonly string _conditionId;
        private readonly string _enemyType;
        private readonly int _requiredKills;
        private int _currentKills;
        private Action _onChanged;
        private EventManager.EventDelegate<EnemyKilledEvent> _eventHandler;

        public EnemyKilledConditionInstance(string conditionId, string enemyType, int requiredKills)
        {
            _conditionId = conditionId;
            _enemyType = enemyType;
            _requiredKills = requiredKills;
        }

        public bool IsMet => _currentKills >= _requiredKills;

        public void Bind(EventManager eventManager, QuestContext context, Action onChanged)
        {
            _onChanged = onChanged;
            _eventHandler = OnEnemyKilled;
            eventManager.AddListener(_eventHandler);
        }

        public void Unbind(EventManager eventManager, QuestContext context)
        {
            if (_eventHandler != null)
            {
                eventManager.RemoveListener(_eventHandler);
                _eventHandler = null;
            }
        }

        private void OnEnemyKilled(EnemyKilledEvent evt)
        {
            if (evt.EnemyType == _enemyType)
            {
                _currentKills++;
                _onChanged?.Invoke();
                
                Debug.Log($"Killed {_enemyType}: {_currentKills}/{_requiredKills}");
            }
        }

        public void Poll(QuestContext context) { }
        public string GetProgressText() => $"{_currentKills}/{_requiredKills} {_enemyType}s defeated";
    }

    // 3. Define your event (must extend GameEvent)
    public class EnemyKilledEvent : GameEvent
    {
        public string EnemyType { get; set; }
    }
}
