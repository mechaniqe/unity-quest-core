#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace DynamicBox.Quest.Core.Services
{
    /// <summary>
    /// Example implementation of IQuestAreaService.
    /// Tracks player location through trigger zones.
    /// Extend this or create your own implementation based on your game's area system.
    /// </summary>
    public class SimpleAreaService : QuestAreaServiceBase
    {
        [Header("Configuration")]
        [SerializeField] private string startingAreaId = "starting_zone";

        private string? _currentAreaId;
        private readonly HashSet<string> _visitedAreas = new();

        public override string? CurrentAreaId => _currentAreaId;

        private void Awake()
        {
            _currentAreaId = startingAreaId;
            if (!string.IsNullOrEmpty(startingAreaId))
            {
                _visitedAreas.Add(startingAreaId);
            }
        }

        public override bool HasEnteredArea(string areaId)
        {
            return _visitedAreas.Contains(areaId);
        }

        public override bool IsInArea(string areaId)
        {
            return _currentAreaId == areaId;
        }

        /// <summary>
        /// Sets the current area (call this from your area transition logic).
        /// </summary>
        public void EnterArea(string areaId)
        {
            if (_currentAreaId == areaId)
                return;

            string? previousArea = _currentAreaId;
            _currentAreaId = areaId;
            _visitedAreas.Add(areaId);

            Debug.Log($"[QuestAreaService] Area transition: {previousArea ?? "null"} → {areaId}");
        }

        /// <summary>
        /// Clears the current area (player left all tracked zones).
        /// </summary>
        public void ExitArea()
        {
            _currentAreaId = null;
        }

        /// <summary>
        /// Gets all visited areas (for debugging or save/load).
        /// </summary>
        public IReadOnlyCollection<string> GetVisitedAreas() => _visitedAreas;

        /// <summary>
        /// Clears all visited area history (useful for new game).
        /// </summary>
        public void ClearHistory()
        {
            _visitedAreas.Clear();
            _currentAreaId = startingAreaId;
            if (!string.IsNullOrEmpty(startingAreaId))
            {
                _visitedAreas.Add(startingAreaId);
            }
        }
    }
}
