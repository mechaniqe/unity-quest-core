#nullable enable
using UnityEngine;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Abstract base class for implementing IQuestAreaService.
    /// Extend this class to create custom area service implementations.
    /// </summary>
    public abstract class QuestAreaServiceBase : MonoBehaviour, IQuestAreaService
    {
        public abstract string? CurrentAreaId { get; }
        public abstract bool HasEnteredArea(string areaId);
        public abstract bool IsInArea(string areaId);
    }
}
