#nullable enable
using System;
using UnityEngine;
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.Core.Conditions
{
    public sealed class TimeElapsedConditionInstance : IConditionInstance, IPollingConditionInstance
    {
        private readonly float _requiredSeconds;
        private float _elapsedTime;
        private Action? _onChanged;

        public bool IsMet => _elapsedTime >= _requiredSeconds;

        public TimeElapsedConditionInstance(float requiredSeconds)
        {
            _requiredSeconds = Mathf.Max(0f, requiredSeconds);
            _elapsedTime = 0f;
        }

        public void Bind(EventManager eventManager, QuestContext context, Action onChanged)
        {
            _onChanged = onChanged;
            _elapsedTime = 0f;
        }

        public void Unbind(EventManager eventManager, QuestContext context)
        {
            _onChanged = null;
        }

        public void Refresh(QuestContext context, Action onChanged)
        {
            if (context?.TimeService == null)
                return;

            float oldTime = _elapsedTime;
            _elapsedTime += Time.deltaTime;

            if (oldTime < _requiredSeconds && _elapsedTime >= _requiredSeconds)
                onChanged?.Invoke();
        }
    }
}
