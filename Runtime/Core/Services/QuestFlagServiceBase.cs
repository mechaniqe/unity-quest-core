#nullable enable
using UnityEngine;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Abstract base class for implementing IQuestFlagService.
    /// Extend this class to create custom flag service implementations.
    /// </summary>
    public abstract class QuestFlagServiceBase : MonoBehaviour, IQuestFlagService
    {
        public abstract bool GetFlag(string flagId);
        public abstract void SetFlag(string flagId, bool value);
        public abstract int GetCounter(string counterId);
        public abstract void SetCounter(string counterId, int value);
        public abstract int IncrementCounter(string counterId, int amount = 1);
        public abstract bool HasFlagBeenSet(string flagId);
    }
}
