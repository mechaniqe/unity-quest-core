using UnityEngine;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Base class for designer-authored condition configuration.
    /// IMPORTANT: Do not store runtime state here. All mutable state must live in IConditionInstance.
    /// </summary>
    public abstract class ConditionAsset : ScriptableObject
    {
        public abstract IConditionInstance CreateInstance();
    }
}
