using System;
using DynamicBox.Quest.Core;
using DynamicBox.Quest.Core.Conditions;
using UnityEngine;

namespace DynamicBox.Quest.Samples
{
    /// <summary>
    /// Example of creating a custom condition for enemy kills.
    /// Shows the minimal code needed to extend the quest system with <see cref="EventDrivenConditionBase{TEvent}"/>.
    /// </summary>

    // 1. Define your event — plain C#, no base class required
    public sealed class EnemyKilledEvent
    {
        public string EnemyType { get; }
        public EnemyKilledEvent(string enemyType) { EnemyType = enemyType; }
    }

    // 2. Create the condition instance — extend EventDrivenConditionBase<TEvent>
    public sealed class EnemyKilledConditionInstance : EventDrivenConditionBase<EnemyKilledEvent>
    {
        private readonly string _enemyType;
        private readonly int _requiredKills;
        private int _currentKills;

        public override bool IsMet => _currentKills >= _requiredKills;

        public EnemyKilledConditionInstance(string conditionId, string enemyType, int requiredKills)
        {
            _enemyType = enemyType;
            _requiredKills = requiredKills;
        }

        protected override void HandleEvent(EnemyKilledEvent evt)
        {
            if (evt.EnemyType != _enemyType)
                return;

            _currentKills++;
            NotifyChanged();

            Debug.Log($"Killed {_enemyType}: {_currentKills}/{_requiredKills}");
        }

        public string GetProgressText() => $"{_currentKills}/{_requiredKills} {_enemyType}s defeated";
    }

    // 3. Create the asset (designer-facing ScriptableObject)
    [CreateAssetMenu(menuName = "Quest Samples/Conditions/Enemy Killed")]
    public sealed class EnemyKilledCondition : ConditionAsset
    {
        [SerializeField] private string enemyType = "Goblin";
        [SerializeField] private int requiredKills = 3;

        public override IConditionInstance CreateInstance() =>
            new EnemyKilledConditionInstance(ConditionId, enemyType, requiredKills);
    }
}

