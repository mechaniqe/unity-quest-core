#nullable enable
using DynamicBox.EventManagement;
using UnityEngine;

namespace DynamicBox.Quest.Samples.EventManagerIntegration
{
    /// <summary>
    /// Event published when the player enters a specific area/zone.
    /// Implements <c>IGameEvent</c> so it can flow through both <see cref="EventManagerAdapter"/>
    /// and the DynamicBox EventManager pipeline.
    /// </summary>
    public sealed class AreaEnteredEvent : IGameEvent
    {
        /// <summary>Gets the unique identifier of the area that was entered.</summary>
        public string AreaId { get; }

        /// <summary>Gets the world position where the area was entered.</summary>
        public Vector3 Position { get; }

        /// <summary>Gets the display name of the area (defaults to AreaId if not provided).</summary>
        public string AreaName { get; }

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
