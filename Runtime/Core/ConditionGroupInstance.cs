#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.Core
{
    public sealed class ConditionGroupInstance : IConditionInstance, IPollingConditionInstance
    {
        private readonly ConditionOperator _operator;
        private readonly List<IConditionInstance> _children;
        private readonly List<IPollingConditionInstance> _pollingChildren;

        private bool _isMet;
        private Action? _onChanged;

        public bool IsMet => _isMet;

        public ConditionGroupInstance(
            ConditionOperator @operator,
            List<IConditionInstance> children)
        {
            _operator = @operator;
            _children = children ?? new List<IConditionInstance>();
            _pollingChildren = _children.OfType<IPollingConditionInstance>().ToList();
        }

        public void Bind(EventManager eventManager, QuestContext context, Action onChanged)
        {
            _onChanged = onChanged;

            foreach (var child in _children)
                child.Bind(eventManager, context, ChildChanged);

            Recompute();
        }

        public void Unbind(EventManager eventManager, QuestContext context)
        {
            foreach (var child in _children)
                child.Unbind(eventManager, context);

            _onChanged = null;
        }

        public void Refresh(QuestContext context, Action onChanged)
        {
            foreach (var child in _pollingChildren)
                child.Refresh(context, ChildChanged);
        }

        private void ChildChanged()
        {
            if (Recompute())
                _onChanged?.Invoke();
        }

        private bool Recompute()
        {
            bool old = _isMet;

            _isMet = _operator switch
            {
                ConditionOperator.And => _children.All(c => c.IsMet),
                ConditionOperator.Or  => _children.Any(c => c.IsMet),
                _                     => _isMet
            };

            return _isMet != old;
        }
    }
}
