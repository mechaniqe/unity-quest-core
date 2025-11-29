#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace DynamicBox.Quest.Core.Services
{
    /// <summary>
    /// Default implementation of IQuestFlagService using in-memory storage.
    /// For production, extend this to integrate with your save/load system.
    /// </summary>
    public class DefaultFlagService : QuestFlagServiceBase
    {
        private readonly Dictionary<string, bool> _flags = new();
        private readonly Dictionary<string, int> _counters = new();
        private readonly HashSet<string> _everSetFlags = new();

        [Header("Debug")]
        [SerializeField] private bool logFlagChanges = false;

        public override bool GetFlag(string flagId)
        {
            return _flags.TryGetValue(flagId, out bool value) && value;
        }

        public override void SetFlag(string flagId, bool value)
        {
            bool oldValue = GetFlag(flagId);
            _flags[flagId] = value;
            
            if (value)
            {
                _everSetFlags.Add(flagId);
            }

            if (logFlagChanges && oldValue != value)
            {
                Debug.Log($"[QuestFlagService] Flag '{flagId}' changed: {oldValue} → {value}");
            }
        }

        public override int GetCounter(string counterId)
        {
            return _counters.TryGetValue(counterId, out int value) ? value : 0;
        }

        public override void SetCounter(string counterId, int value)
        {
            int oldValue = GetCounter(counterId);
            _counters[counterId] = value;

            if (logFlagChanges && oldValue != value)
            {
                Debug.Log($"[QuestFlagService] Counter '{counterId}' changed: {oldValue} → {value}");
            }
        }

        public override int IncrementCounter(string counterId, int amount = 1)
        {
            int newValue = GetCounter(counterId) + amount;
            SetCounter(counterId, newValue);
            return newValue;
        }

        public override bool HasFlagBeenSet(string flagId)
        {
            return _everSetFlags.Contains(flagId);
        }

        /// <summary>
        /// Clears all flags and counters (useful for testing or new game).
        /// </summary>
        public void ClearAll()
        {
            _flags.Clear();
            _counters.Clear();
            _everSetFlags.Clear();
            
            if (logFlagChanges)
            {
                Debug.Log("[QuestFlagService] All flags and counters cleared");
            }
        }

        /// <summary>
        /// Gets all current flags (for debugging or save/load).
        /// </summary>
        public IReadOnlyDictionary<string, bool> GetAllFlags() => _flags;

        /// <summary>
        /// Gets all current counters (for debugging or save/load).
        /// </summary>
        public IReadOnlyDictionary<string, int> GetAllCounters() => _counters;
    }
}
