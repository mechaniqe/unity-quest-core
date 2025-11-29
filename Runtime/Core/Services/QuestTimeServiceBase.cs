#nullable enable
using UnityEngine;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Abstract base class for implementing IQuestTimeService.
    /// Extend this class to create custom time service implementations.
    /// </summary>
    public abstract class QuestTimeServiceBase : MonoBehaviour, IQuestTimeService
    {
        public abstract float TotalGameTime { get; }
        public abstract float DeltaTime { get; }
        public abstract float TimeOfDay { get; }
        public abstract int CurrentDay { get; }
    }
}
