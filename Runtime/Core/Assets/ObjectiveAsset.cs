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
    }
}
