#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Runtime state for an active quest.
    /// Contains quest status and objective states indexed by objective ID.
    /// </summary>
    public sealed class QuestState
    {
        public QuestAsset Definition { get; }
        public QuestStatus Status { get; private set; } = QuestStatus.NotStarted;

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

        public void SetStatus(QuestStatus status)
        {
            Status = status;
        }

        public IEnumerable<ObjectiveState> GetObjectiveStates() => _objectives.Values;

        public bool TryGetObjective(string objectiveId, out ObjectiveState? state)
            => _objectives.TryGetValue(objectiveId, out state);
    }
}
