#nullable enable
using UnityEngine;
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.GameEvents
{
    /// <summary>
    /// Event published when the player enters a specific area/zone.
    /// Immutable event following CQRS best practices.
    /// </summary>
    public sealed class AreaEnteredEvent : GameEvent
    {
        /// <summary>
        /// Gets the unique identifier of the area that was entered.
        /// </summary>
        public string AreaId { get; }
        
        /// <summary>
        /// Gets the world position where the area was entered.
        /// </summary>
        public Vector3 Position { get; }
        
        /// <summary>
        /// Gets the display name of the area (defaults to AreaId if not provided).
        /// </summary>
        public string AreaName { get; }

        /// <summary>
        /// Creates a new area entered event.
        /// </summary>
        /// <param name="areaId">The unique identifier of the area.</param>
        /// <param name="position">The world position where the area was entered.</param>
        /// <param name="areaName">Optional display name for the area.</param>
        public AreaEnteredEvent(string areaId, Vector3 position = default, string? areaName = null)
        {
            AreaId = areaId;
            Position = position;
            AreaName = areaName ?? areaId;
        }
    }
}
