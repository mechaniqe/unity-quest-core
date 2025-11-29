#nullable enable
using UnityEngine;
using System.Collections.Generic;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Designer-authored quest definition (ScriptableObject).
    /// Contains quest metadata and references to objective assets.
    /// </summary>
    [CreateAssetMenu(menuName = "DynamicBox/Quest/Quest", fileName = "NewQuest")]
    public class QuestAsset : ScriptableObject
    {
        [SerializeField] private string questId = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [TextArea] [SerializeField] private string description = string.Empty;

        [SerializeField] private List<ObjectiveAsset> objectives = new();

        public string QuestId => questId;
        public string DisplayName => displayName;
        public string Description => description;
        public IReadOnlyList<ObjectiveAsset> Objectives => objectives;
    }
}
