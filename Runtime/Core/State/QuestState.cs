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
        /// <summary>
        /// Gets the designer-authored definition for this quest.
        /// </summary>
        public QuestAsset Definition { get; }
        
        /// <summary>
        /// Gets the current status of this quest.
        /// </summary>
        public QuestStatus Status { get; private set; } = QuestStatus.NotStarted;

        /// <summary>
        /// Gets all objective states in this quest, indexed by objective ID.
        /// </summary>
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

        /// <summary>
        /// Gets all objective states in this quest as an enumerable collection.
        /// </summary>
        /// <returns>Enumerable of all objective states.</returns>
        public IEnumerable<ObjectiveState> GetObjectiveStates() => _objectives.Values;

        /// <summary>
        /// Attempts to get an objective state by its ID.
        /// </summary>
        /// <param name="objectiveId">The unique identifier of the objective.</param>
        /// <param name="state">The objective state if found, otherwise null.</param>
        /// <returns>True if the objective was found; false otherwise.</returns>
        public bool TryGetObjective(string objectiveId, out ObjectiveState? state)
            => _objectives.TryGetValue(objectiveId, out state);
    }
}
