using System;

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

        public void Bind(IQuestEventBus eventBus, QuestContext context, Action onChanged)
        {
            _onChanged = onChanged;
            _elapsedTime = 0f;
        }

        public void Unbind(IQuestEventBus eventBus, QuestContext context)
        {
            _onChanged = null;
        }

        public void Refresh(QuestContext context, Action onChanged)
        {
            if (context?.TimeService == null)
                return;

            float oldTime = _elapsedTime;
            _elapsedTime += UnityEngine.Time.deltaTime;

            if (oldTime < _requiredSeconds && _elapsedTime >= _requiredSeconds)
                onChanged?.Invoke();
        }
    }

    // Helper for Mathf.Max reference
    internal static class Mathf
    {
        public static float Max(float a, float b) => a > b ? a : b;
    }
}
