#nullable enable
using UnityEngine;
using System.Collections.Generic;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Designer-authored objective definition (ScriptableObject).
    /// Contains objective metadata, prerequisites, and condition references.
    /// </summary>
    [CreateAssetMenu(menuName = "DynamicBox/Quest/Objective", fileName = "NewObjective")]
    public class ObjectiveAsset : ScriptableObject
    {
        [SerializeField] private string objectiveId = string.Empty;
        [SerializeField] private string title = string.Empty;
        [TextArea] [SerializeField] private string description = string.Empty;
        [SerializeField] private bool isOptional;

        [SerializeField] private List<ObjectiveAsset> prerequisites = new();
        [SerializeField] private ConditionAsset? completionCondition;
        [SerializeField] private ConditionAsset? failCondition;

        /// <summary>
        /// Gets the unique identifier for this objective.
        /// </summary>
        public string ObjectiveId => objectiveId;
        
        /// <summary>
        /// Gets the title shown to players.
        /// </summary>
        public string Title => title;
        
        /// <summary>
        /// Gets the detailed description of this objective.
        /// </summary>
        public string Description => description;
        
        /// <summary>
        /// Gets whether this objective is optional (quest can complete without it).
        /// </summary>
        public bool IsOptional => isOptional;

        /// <summary>
        /// Gets the display name, using Title if available, otherwise ObjectiveId.
        /// </summary>
        public string DisplayName => !string.IsNullOrEmpty(title) ? title : objectiveId;
        
        /// <summary>
        /// Gets the completion condition as a ConditionGroupAsset if it is one, otherwise null.
        /// Convenience property for accessing nested condition groups.
        /// </summary>
        public ConditionGroupAsset? ConditionGroup => completionCondition as ConditionGroupAsset;

        /// <summary>
        /// Gets the list of prerequisite objectives that must be completed before this one can start.
        /// </summary>
        public IReadOnlyList<ObjectiveAsset> Prerequisites => prerequisites;
        
        /// <summary>
        /// Gets the condition that must be met to complete this objective.
        /// </summary>
        public ConditionAsset? CompletionCondition => completionCondition;
        
        /// <summary>
        /// Gets the optional condition that causes this objective to fail if met.
        /// </summary>
        public ConditionAsset? FailCondition => failCondition;

#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
        /// <summary>
        /// Factory method for creating objective assets in tests.
        /// Only available in editor and test builds to avoid reflection overhead.
        /// </summary>
        public static ObjectiveAsset CreateForTest(
            string objectiveId,
            string title,
            string description,
            bool isOptional,
            List<ObjectiveAsset>? prerequisites,
            ConditionAsset? completionCondition,
            ConditionAsset? failCondition)
        {
            var objective = CreateInstance<ObjectiveAsset>();
            objective.objectiveId = objectiveId;
            objective.title = title;
            objective.description = description;
            objective.isOptional = isOptional;
            objective.prerequisites = prerequisites ?? new List<ObjectiveAsset>();
            objective.completionCondition = completionCondition;
            objective.failCondition = failCondition;
            return objective;
        }
#endif
    }
}
