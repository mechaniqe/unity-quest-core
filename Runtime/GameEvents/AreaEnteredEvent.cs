using UnityEngine;
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.GameEvents
{
    /// <summary>
    /// Event published when the player enters a specific area/zone.
    /// </summary>
    public class AreaEnteredEvent : GameEvent
    {
        public string AreaId { get; set; }
        public Vector3 Position { get; set; }
        public string AreaName { get; set; }

        public AreaEnteredEvent(string areaId, Vector3 position = default, string areaName = null)
        {
            AreaId = areaId;
            Position = position;
            AreaName = areaName ?? areaId;
        }
    }
}
