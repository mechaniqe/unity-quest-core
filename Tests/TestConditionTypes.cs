#nullable enable
using DynamicBox.Quest.Core;
using UnityEngine;

namespace DynamicBox.Quest.Tests
{
    // ---------------------------------------------------------------------------
    // Test-local event types — stand-ins for the bundled GameEvents (now in Samples~)
    // ---------------------------------------------------------------------------

    public sealed class TestItemEvent
    {
        public string ItemId { get; }
        public int Amount { get; }
        public TestItemEvent(string itemId, int amount = 1) { ItemId = itemId; Amount = amount; }
    }

    public sealed class TestAreaEvent
    {
        public string AreaId { get; }
        public TestAreaEvent(string areaId) { AreaId = areaId; }
    }

    public sealed class TestFlagEvent
    {
        public string FlagId { get; }
        public bool NewValue { get; }
        public bool OldValue { get; }
        public TestFlagEvent(string flagId, bool newValue, bool oldValue = false)
        {
            FlagId = flagId;
            NewValue = newValue;
            OldValue = oldValue;
        }
    }

    // ---------------------------------------------------------------------------
    // Test condition instances
    // ---------------------------------------------------------------------------

    public sealed class TestItemConditionInstance : EventDrivenConditionBase<TestItemEvent>, IProgressReportingCondition
    {
        private readonly string _itemId;
        private readonly int _requiredCount;
        private int _currentCount;

        public override bool IsMet => _currentCount >= _requiredCount;
        public int CurrentCount => _currentCount;
        public int RequiredCount => _requiredCount;
        public float Progress => _requiredCount > 0 ? Mathf.Clamp01((float)_currentCount / _requiredCount) : 1f;
        public string ProgressDescription => $"{_currentCount}/{_requiredCount} items collected";

        public TestItemConditionInstance(string itemId, int requiredCount)
        {
            _itemId = itemId;
            _requiredCount = requiredCount;
        }

        protected override void HandleEvent(TestItemEvent evt)
        {
            if (evt.ItemId != _itemId)
                return;
            int oldCount = _currentCount;
            _currentCount += evt.Amount;
            if (oldCount < _requiredCount)
                NotifyChanged();
        }
    }

    public sealed class TestAreaConditionInstance : EventDrivenConditionBase<TestAreaEvent>
    {
        private readonly string _areaId;
        private bool _isCompleted;

        public override bool IsMet => _isCompleted;

        public TestAreaConditionInstance(string areaId) { _areaId = areaId; }

        protected override void HandleEvent(TestAreaEvent evt)
        {
            if (evt.AreaId == _areaId && !_isCompleted)
            {
                _isCompleted = true;
                NotifyChanged();
            }
        }
    }

    public sealed class TestFlagConditionInstance : EventDrivenConditionBase<TestFlagEvent>
    {
        private readonly string _flagId;
        private readonly bool _expectedValue;
        private bool _isCompleted;

        public override bool IsMet => _isCompleted;

        public TestFlagConditionInstance(string flagId, bool expectedValue)
        {
            _flagId = flagId;
            _expectedValue = expectedValue;
        }

        protected override void OnBind(QuestContext context)
        {
            if (context?.FlagService != null)
                CheckFlagValue(context.FlagService.GetFlag(_flagId));
        }

        protected override void HandleEvent(TestFlagEvent evt)
        {
            if (evt.FlagId == _flagId)
                CheckFlagValue(evt.NewValue);
        }

        private void CheckFlagValue(bool currentValue)
        {
            bool shouldComplete = currentValue == _expectedValue;
            if (shouldComplete != _isCompleted)
            {
                _isCompleted = shouldComplete;
                NotifyChanged();
            }
        }
    }

    // ---------------------------------------------------------------------------
    // Test condition assets (used via ScriptableObject.CreateInstance<T>() in tests)
    // ---------------------------------------------------------------------------

    public sealed class TestItemConditionAsset : ConditionAsset
    {
#pragma warning disable CS0414
        [UnityEngine.SerializeField] private int requiredCount = 1;
#pragma warning restore CS0414

        public override IConditionInstance CreateInstance() =>
            new TestItemConditionInstance(ConditionId, requiredCount);
    }

    public sealed class TestAreaConditionAsset : ConditionAsset
    {
        public override IConditionInstance CreateInstance() =>
            new TestAreaConditionInstance(ConditionId);
    }

    public sealed class TestFlagConditionAsset : ConditionAsset
    {
#pragma warning disable CS0414
        [UnityEngine.SerializeField] private bool _expectedValue = true;
#pragma warning restore CS0414

        public override IConditionInstance CreateInstance() =>
            new TestFlagConditionInstance(ConditionId, _expectedValue);
    }
}
