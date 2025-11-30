#nullable enable
using System.Collections.Generic;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Registry for active quests. Manages the collection of quests currently in progress.
    /// </summary>
    public sealed class QuestLog
    {
        private readonly List<QuestState> _active = new();

        /// <summary>
        /// Gets the read-only list of all quests currently in progress.
        /// </summary>
        public IReadOnlyList<QuestState> Active => _active;

        /// <summary>
        /// Starts tracking a new quest and sets its status to InProgress.
        /// </summary>
        /// <param name="quest">The quest definition to start tracking.</param>
        /// <returns>The newly created quest state.</returns>
        public QuestState StartQuest(QuestAsset quest)
        {
            var state = new QuestState(quest);
            state.SetStatus(QuestStatus.InProgress);
            _active.Add(state);
            return state;
        }

        /// <summary>
        /// Removes a quest from the active list (used when completed or failed).
        /// </summary>
        /// <param name="state">The quest state to remove from tracking.</param>
        public void RemoveQuest(QuestState state)
        {
            _active.Remove(state);
        }
    }
}
