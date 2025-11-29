#nullable enable
using System;
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.Core.Conditions
{
    /// <summary>
    /// Condition that completes after a specified amount of time has elapsed.
    /// Uses IQuestTimeService for time tracking instead of Unity's Time.deltaTime.
    /// </summary>
    public sealed class TimeElapsedConditionInstance : IConditionInstance, IPollingConditionInstance
    {
        private readonly float _requiredSeconds;
        private float _elapsedTime;
        private Action? _onChanged;
        private bool _isInitialized;

        public bool IsMet => _elapsedTime >= _requiredSeconds;

        public TimeElapsedConditionInstance(float requiredSeconds)
        {
            _requiredSeconds = requiredSeconds;
            _elapsedTime = 0f;
            _isInitialized = false;
        }

        public void Bind(EventManager eventManager, QuestContext context, Action onChanged)
        {
            _onChanged = onChanged;
            _elapsedTime = 0f;
            _isInitialized = true;

            // Check if TimeService is available
            if (context?.TimeService == null)
            {
                UnityEngine.Debug.LogWarning(
                    $"TimeElapsedCondition requires IQuestTimeService but none is available. " +
                    $"Condition will not function. Ensure a TimeService is configured in QuestPlayerRef.");
            }
        }

        public void Unbind(EventManager eventManager, QuestContext context)
        {
            _onChanged = null;
            _isInitialized = false;
        }

        public void Refresh(QuestContext context, Action onChanged)
        {
            if (!_isInitialized || context?.TimeService == null)
                return;

            float oldTime = _elapsedTime;
            _elapsedTime += context.TimeService.DeltaTime;

            // Notify when transitioning from not-met to met
            if (oldTime < _requiredSeconds && _elapsedTime >= _requiredSeconds)
            {
                onChanged?.Invoke();
            }
        }

        /// <summary>
        /// Gets the current progress (0.0 to 1.0).
        /// </summary>
        public float GetProgress()
        {
            return _requiredSeconds > 0 ? _elapsedTime / _requiredSeconds : 1f;
        }

        /// <summary>
        /// Gets the remaining time in seconds.
        /// </summary>
        public float GetRemainingTime()
        {
            return UnityEngine.Mathf.Max(0f, _requiredSeconds - _elapsedTime);
        }
    }
}
