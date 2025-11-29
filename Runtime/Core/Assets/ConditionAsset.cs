#nullable enable
using UnityEngine;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Base class for designer-authored condition configuration.
    /// IMPORTANT: Do not store runtime state here. All mutable state must live in IConditionInstance.
    /// Subclasses implement CreateInstance() to produce condition instances with immutable configuration.
    /// </summary>
    public abstract class ConditionAsset : ScriptableObject
    {
        /// <summary>
        /// Creates a new runtime instance of this condition.
        /// Each instance maintains its own state and can be bound to the event system.
        /// </summary>
        public abstract IConditionInstance CreateInstance();
    }
}
