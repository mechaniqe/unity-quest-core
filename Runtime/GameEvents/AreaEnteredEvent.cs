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
        public string AreaId { get; }
        public Vector3 Position { get; }
        public string AreaName { get; }

        public AreaEnteredEvent(string areaId, Vector3 position = default, string? areaName = null)
        {
            AreaId = areaId;
            Position = position;
            AreaName = areaName ?? areaId;
        }
    }
}
