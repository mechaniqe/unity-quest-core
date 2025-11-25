using UnityEngine;
using System.Collections.Generic;

namespace GenericQuest.Core
{
    [CreateAssetMenu(menuName = "Quests/Objective", fileName = "NewObjective")]
    public class ObjectiveAsset : ScriptableObject
    {
        [SerializeField] private string objectiveId;
        [SerializeField] private string title;
        [TextArea] [SerializeField] private string description;
        [SerializeField] private bool isOptional;

        // Other objectives that must be completed first
        [SerializeField] private List<ObjectiveAsset> prerequisites = new();

        // Completion and failure conditions (see ConditionAsset)
        [SerializeField] private ConditionAsset completionCondition;  // can be ConditionGroupAsset
        [SerializeField] private ConditionAsset failCondition;        // optional

        public string ObjectiveId => objectiveId;
        public string Title => title;
        public string Description => description;
        public bool IsOptional => isOptional;

        public IReadOnlyList<ObjectiveAsset> Prerequisites => prerequisites;
        public ConditionAsset CompletionCondition => completionCondition;
        public ConditionAsset FailCondition => failCondition;
    }
}
