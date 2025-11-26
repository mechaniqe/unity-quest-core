#nullable enable
using DynamicBox.Quest.Core;
using DynamicBox.EventManagement;
using System;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// A mock/test condition that can be controlled programmatically.
    /// </summary>
    public sealed class MockConditionAsset : ConditionAsset
    {
        public override IConditionInstance CreateInstance()
        {
            return new MockConditionInstance();
        }
    }

    public sealed class MockConditionInstance : IConditionInstance
    {
        private bool _isMet = false;
        private Action? _onChanged;

        public bool IsMet => _isMet;

        public void SetMet(bool value)
        {
            if (_isMet != value)
            {
                _isMet = value;
                _onChanged?.Invoke();
            }
        }

        public void Bind(EventManager eventManager, QuestContext context, Action onChanged)
        {
            _onChanged = onChanged;
        }

        public void Unbind(EventManager eventManager, QuestContext context)
        {
            _onChanged = null;
        }
    }
}
