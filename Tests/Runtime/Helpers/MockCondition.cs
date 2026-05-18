using System;
using DynamicBox.Quest.Core;
using UnityEngine;

namespace DynamicBox.Quest.Tests.Helpers
{
    /// <summary>
    /// Test double implementing both IConditionInstance and IPollingConditionInstance.
    /// Provides settable IsMet and tracks all method invocations.
    /// </summary>
    public class MockConditionInstance : IConditionInstance, IPollingConditionInstance, IProgressReportingCondition
    {
        private bool _isMet;
        private Action? _onChanged;

        public bool IsMet => _isMet;
        public float Progress => _isMet ? 1f : 0f;
        public string ProgressDescription => _isMet ? "Complete" : "In Progress";
        public bool BindCalled { get; private set; }
        public bool UnbindCalled { get; private set; }
        public bool ResetCalled { get; private set; }
        public bool RefreshCalled { get; private set; }
        public int BindCallCount { get; private set; }
        public int RefreshCallCount { get; private set; }

        public void SetMet(bool value)
        {
            if (_isMet != value)
            {
                _isMet = value;
                _onChanged?.Invoke();
            }
        }

        public void Bind(IEventBus eventBus, QuestContext context, Action onChanged)
        {
            _onChanged = onChanged;
            BindCalled = true;
            BindCallCount++;
        }

        public void Unbind(IEventBus eventBus, QuestContext context)
        {
            _onChanged = null;
            UnbindCalled = true;
        }

        public void Reset()
        {
            _isMet = false;
            ResetCalled = true;
        }

        public void Refresh(QuestContext context, Action onChanged)
        {
            RefreshCalled = true;
            RefreshCallCount++;
        }
    }

    /// <summary>
    /// ScriptableObject that creates MockConditionInstance instances.
    /// </summary>
    public class MockConditionAsset : ConditionAsset
    {
        public MockConditionInstance? LastCreatedInstance { get; private set; }

        public override IConditionInstance CreateInstance()
        {
            LastCreatedInstance = new MockConditionInstance();
            return LastCreatedInstance;
        }
    }
}
