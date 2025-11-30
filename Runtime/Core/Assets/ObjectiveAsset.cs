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

        public string ObjectiveId => objectiveId;
        public string Title => title;
        public string Description => description;
        public bool IsOptional => isOptional;

        public string DisplayName => !string.IsNullOrEmpty(title) ? title : objectiveId;
        public ConditionGroupAsset? ConditionGroup => completionCondition as ConditionGroupAsset;

        public IReadOnlyList<ObjectiveAsset> Prerequisites => prerequisites;
        public ConditionAsset? CompletionCondition => completionCondition;
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
