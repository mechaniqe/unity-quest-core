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

        /// <summary>
        /// Gets the unique identifier for this quest.
        /// </summary>
        public string QuestId => questId;
        
        /// <summary>
        /// Gets the display name shown to players.
        /// </summary>
        public string DisplayName => displayName;
        
        /// <summary>
        /// Gets the detailed description of this quest.
        /// </summary>
        public string Description => description;
        
        /// <summary>
        /// Gets the read-only list of objectives required to complete this quest.
        /// </summary>
        public IReadOnlyList<ObjectiveAsset> Objectives => objectives;

#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
        /// <summary>
        /// Factory method for creating quest assets in tests.
        /// Only available in editor and test builds to avoid reflection overhead.
        /// </summary>
        public static QuestAsset CreateForTest(
            string questId,
            string displayName,
            string description,
            List<ObjectiveAsset> objectives)
        {
            var quest = CreateInstance<QuestAsset>();
            quest.questId = questId;
            quest.displayName = displayName;
            quest.description = description;
            quest.objectives = objectives ?? new List<ObjectiveAsset>();
            return quest;
        }
#endif
    }
}
