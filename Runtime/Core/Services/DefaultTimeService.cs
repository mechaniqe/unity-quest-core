#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace DynamicBox.Quest.Core.Services
{
    /// <summary>
    /// Default implementation of IQuestTimeService using Unity's Time API.
    /// Suitable for most single-player games.
    /// </summary>
    public class DefaultTimeService : QuestTimeServiceBase
    {
        [Header("Configuration")]
        [SerializeField] private bool useRealTime = false;
        [SerializeField] private float timeScale = 1.0f;
        [SerializeField] private float hoursPerDay = 24f;
        [SerializeField] private float secondsPerHour = 60f;

        private float _totalGameTime;
        private float _currentTimeOfDay;
        private int _currentDay;

        public override float TotalGameTime => _totalGameTime;
        public override float DeltaTime => useRealTime ? Time.unscaledDeltaTime : Time.deltaTime;
        public override float TimeOfDay => _currentTimeOfDay;
        public override int CurrentDay => _currentDay;

        private void Awake()
        {
            _totalGameTime = 0f;
            _currentTimeOfDay = 6f; // Start at 6 AM
            _currentDay = 1;
        }

        private void Update()
        {
            float deltaTime = DeltaTime * timeScale;
            _totalGameTime += deltaTime;

            // Update time of day
            float hourDelta = deltaTime / secondsPerHour;
            _currentTimeOfDay += hourDelta;

            // Handle day transitions
            if (_currentTimeOfDay >= hoursPerDay)
            {
                _currentTimeOfDay -= hoursPerDay;
                _currentDay++;
            }
        }

        /// <summary>
        /// Sets the current time of day (useful for debugging or save/load).
        /// </summary>
        public void SetTimeOfDay(float hour)
        {
            _currentTimeOfDay = Mathf.Clamp(hour, 0f, hoursPerDay);
        }

        /// <summary>
        /// Sets the current day (useful for save/load).
        /// </summary>
        public void SetDay(int day)
        {
            _currentDay = Mathf.Max(1, day);
        }
    }
}
