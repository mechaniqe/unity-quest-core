using System.Collections.Generic;

namespace GenericQuest.Core
{
    public sealed class QuestLog
    {
        private readonly List<QuestState> _active = new();

        public IReadOnlyList<QuestState> Active => _active;

        public QuestState StartQuest(QuestAsset quest)
        {
            var state = new QuestState(quest);
            state.SetStatus(QuestStatus.InProgress);
            _active.Add(state);
            return state;
        }

        public void RemoveQuest(QuestState state)
        {
            _active.Remove(state);
        }
    }
}
