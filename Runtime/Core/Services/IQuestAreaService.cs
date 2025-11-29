#nullable enable

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Service interface for tracking player location and area-based conditions.
    /// Implement this interface in your game's area/zone management system.
    /// </summary>
    public interface IQuestAreaService
    {
        /// <summary>
        /// Gets the ID of the area/zone the player is currently in.
        /// </summary>
        string? CurrentAreaId { get; }

        /// <summary>
        /// Checks if the player has entered a specific area at least once.
        /// </summary>
        bool HasEnteredArea(string areaId);

        /// <summary>
        /// Checks if the player is currently in a specific area.
        /// </summary>
        bool IsInArea(string areaId);
    }
}
