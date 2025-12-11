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
                        SafeInvoke(OnObjectiveStatusChanged, obj);
                        _evaluator.ActivateReadyObjectives(quest);
                        break;
                    
                    case QuestEvaluationResult.QuestCompleted:
                        SafeInvoke(OnObjectiveStatusChanged, obj);
                        SafeInvoke(OnQuestCompleted, quest);
                        break;
                    
                    case QuestEvaluationResult.QuestFailed:
                        SafeInvoke(OnObjectiveStatusChanged, obj);
                        SafeInvoke(OnQuestFailed, quest);
                        break;
                }
            }

            _dirtySet.Clear();
        }

        /// <summary>
        /// Gets the current number of dirty objectives pending evaluation.
        /// </summary>
        public int DirtyCount => _dirtySet.Count;

        /// <summary>
        /// Safely invokes an event, catching exceptions from individual subscribers to prevent breaking the event chain.
        /// </summary>
        private void SafeInvoke<T>(Action<T>? eventDelegate, T arg)
        {
            if (eventDelegate == null) return;

            foreach (Action<T> handler in eventDelegate.GetInvocationList())
            {
                try
                {
                    handler(arg);
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"Exception in quest event subscriber: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }
    }
}
