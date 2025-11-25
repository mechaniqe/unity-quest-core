using System.Collections.Generic;
using System.Linq;

namespace GenericQuest.Core
{
    public sealed class QuestState
    {
        public QuestAsset Definition { get; }
        public QuestStatus Status { get; private set; } = QuestStatus.NotStarted;

        // Keyed by ObjectiveId
        public IReadOnlyDictionary<string, ObjectiveState> Objectives => _objectives;
        private readonly Dictionary<string, ObjectiveState> _objectives;

        public QuestState(QuestAsset definition)
        {
            Definition = definition;
            _objectives = definition.Objectives
                .Where(o => o != null)
                .ToDictionary(
                    o => o.ObjectiveId,
                    o => new ObjectiveState(o)
                );
        }

        internal void SetStatus(QuestStatus status)
        {
            Status = status;
        }

        internal IEnumerable<ObjectiveState> GetObjectiveStates() => _objectives.Values;

        internal bool TryGetObjective(string objectiveId, out ObjectiveState state)
            => _objectives.TryGetValue(objectiveId, out state);
    }
}
