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

        public IReadOnlyList<QuestState> Active => _active;

        /// <summary>
        /// Starts tracking a new quest and sets its status to InProgress.
        /// </summary>
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
        public void RemoveQuest(QuestState state)
        {
            _active.Remove(state);
        }
    }
}
