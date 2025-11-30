#nullable enable
using System;
using DynamicBox.EventManagement;
using UnityEngine;

namespace DynamicBox.Quest.Core.Conditions
{
    /// <summary>
    /// Condition that completes after a specified amount of time has elapsed.
    /// Uses IQuestTimeService for time tracking instead of Unity's Time.deltaTime.
    /// Implements progress reporting for UI integration.
    /// </summary>
    public sealed class TimeElapsedConditionInstance : IConditionInstance, IPollingConditionInstance, IProgressReportingCondition
    {
        private readonly float _requiredSeconds;
        private float _elapsedTime;
        private Action? _onChanged;
        private bool _isInitialized;

        public bool IsMet => _elapsedTime >= _requiredSeconds;

        public float Progress => _requiredSeconds > 0 ? Mathf.Clamp01(_elapsedTime / _requiredSeconds) : 1f;
        
        public string ProgressDescription
        {
            get
            {
                float remaining = Mathf.Max(0f, _requiredSeconds - _elapsedTime);
                return $"{remaining:F1} seconds remaining";
            }
        }

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
                Debug.LogWarning(
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
        /// Gets the remaining time in seconds.
        /// </summary>
        public float GetRemainingTime()
        {
            return Mathf.Max(0f, _requiredSeconds - _elapsedTime);
        }
    }
}
