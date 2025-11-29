#nullable enable
using System;
using System.Collections.Generic;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Manages the dirty queue for objectives that need evaluation.
    /// Processes objectives in batch and fires appropriate events based on evaluation results.
    /// Extracted from QuestManager to follow Single Responsibility Principle.
    /// </summary>
    internal sealed class DirtyQueueProcessor
    {
        private readonly HashSet<(QuestState quest, ObjectiveState obj)> _dirtySet = new();
        private readonly ObjectiveEvaluator _evaluator;

        public event Action<QuestState>? OnQuestCompleted;
        public event Action<QuestState>? OnQuestFailed;
        public event Action<ObjectiveState>? OnObjectiveStatusChanged;

        public DirtyQueueProcessor(ObjectiveEvaluator evaluator)
        {
            _evaluator = evaluator;
        }

        /// <summary>
        /// Marks an objective as dirty for evaluation in the next processing cycle.
        /// </summary>
        public void MarkDirty(QuestState quest, ObjectiveState obj)
        {
            _dirtySet.Add((quest, obj));
        }

        /// <summary>
        /// Processes all dirty objectives and fires appropriate events.
        /// Should be called once per frame.
        /// </summary>
        public void ProcessAll()
        {
            if (_dirtySet.Count == 0)
                return;

            // Process all dirty objectives
            foreach (var (quest, obj) in _dirtySet)
            {
                var result = _evaluator.Evaluate(quest, obj);
                
                // Fire appropriate events based on evaluation result
                switch (result)
                {
                    case QuestEvaluationResult.ObjectiveCompleted:
                        OnObjectiveStatusChanged?.Invoke(obj);
                        _evaluator.ActivateReadyObjectives(quest);
                        break;
                    
                    case QuestEvaluationResult.QuestCompleted:
                        OnObjectiveStatusChanged?.Invoke(obj);
                        OnQuestCompleted?.Invoke(quest);
                        break;
                    
                    case QuestEvaluationResult.QuestFailed:
                        OnObjectiveStatusChanged?.Invoke(obj);
                        OnQuestFailed?.Invoke(quest);
                        break;
                }
            }

            _dirtySet.Clear();
        }

        /// <summary>
        /// Gets the current number of dirty objectives pending evaluation.
        /// </summary>
        public int DirtyCount => _dirtySet.Count;
    }
}
