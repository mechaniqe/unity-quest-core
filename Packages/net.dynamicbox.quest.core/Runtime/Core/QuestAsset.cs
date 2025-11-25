using UnityEngine;
using System.Collections.Generic;

namespace DynamicBox.Quest.Core
{
    [CreateAssetMenu(menuName = "DynamicBox Quest/Quest", fileName = "NewQuest")]
    public class QuestAsset : ScriptableObject
    {
        [SerializeField] private string questId;
        [SerializeField] private string displayName;
        [TextArea] [SerializeField] private string description;

        // For now, objectives are referenced as sub-assets or direct references.
        [SerializeField] private List<ObjectiveAsset> objectives = new();

        public string QuestId => questId;
        public string DisplayName => displayName;
        public string Description => description;
        public IReadOnlyList<ObjectiveAsset> Objectives => objectives;
    }
}
