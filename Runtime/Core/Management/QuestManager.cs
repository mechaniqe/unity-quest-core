#nullable enable
using DynamicBox.EventManagement;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Main orchestrator for the quest system. Manages quest lifecycle and delegates 
    /// evaluation and binding logic to specialized services.
    /// Refactored to follow Single Responsibility Principle.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private QuestPlayerRef? playerRef;

        [Header("Polling (optional)")]
        [SerializeField] private bool enablePolling = true;
        [SerializeField] private float pollingInterval = 0.25f;

        private EventManager? _eventManager;
        private QuestLog? _log;
        private QuestContext? _context;
        private ConditionBindingService? _bindingService;
        private ObjectiveEvaluator? _evaluator;
        private DirtyQueueProcessor? _processor;

        private float _pollTimer;

        // Public properties for editor and debugging
        public IReadOnlyList<QuestState> ActiveQuests => _log?.Active ?? Array.Empty<QuestState>();

        public event Action<QuestState>? OnQuestCompleted;
        public event Action<QuestState>? OnQuestFailed;
        public event Action<ObjectiveState>? OnObjectiveStatusChanged;

        private void Awake()
        {
            if (playerRef == null)
            {
                Debug.LogError("QuestManager requires a QuestPlayerRef to be assigned!", this);
                enabled = false;
                return;
            }

            _eventManager = EventManager.Instance;
            _log = new QuestLog();
            _context = playerRef.BuildContext();
            _bindingService = new ConditionBindingService(_eventManager, _context);
            _evaluator = new ObjectiveEvaluator(_log, _bindingService);
            _processor = new DirtyQueueProcessor(_evaluator);
            
            // Wire up events from processor
            _processor.OnQuestCompleted += (q) => OnQuestCompleted?.Invoke(q);
            _processor.OnQuestFailed += (q) => OnQuestFailed?.Invoke(q);
            _processor.OnObjectiveStatusChanged += (o) => OnObjectiveStatusChanged?.Invoke(o);
            
            // Set callback for evaluator to mark objectives as dirty
            _evaluator.SetDirtyCallback(_processor.MarkDirty);
        }

        private void Update()
        {
            if (enablePolling)
            {
                _pollTimer += Time.deltaTime;
                if (_pollTimer >= pollingInterval)
                {
                    _pollTimer = 0f;
                    PollConditions();
                }
            }

            _processor?.ProcessAll();
        }

        /// <summary>
        /// Starts a quest and activates objectives that are ready to progress.
        /// </summary>
        public QuestState StartQuest(QuestAsset questAsset)
        {
            if (_log == null || _evaluator == null)
            {
                Debug.LogError("QuestManager not properly initialized!", this);
                throw new InvalidOperationException("QuestManager not initialized");
            }

            var state = _log.StartQuest(questAsset);
            _evaluator.ActivateReadyObjectives(state);
            
            // Immediately evaluate objectives in case they're already complete
            foreach (var obj in state.GetObjectiveStates())
            {
                if (obj.Status.IsActive())
                {
                    _processor?.MarkDirty(state, obj);
                }
            }
            
            return state;
        }

        /// <summary>
        /// Stops a quest and cleans up all bindings.
        /// </summary>
        public void StopQuest(QuestState questState)
        {
            if (_bindingService == null || _log == null)
                return;

            _bindingService.UnbindQuest(questState);
            _log.RemoveQuest(questState);
        }

        /// <summary>
        /// Manually completes a quest (for debugging/editor support).
        /// </summary>
        public void CompleteQuest(QuestState questState)
        {
            if (_bindingService == null || _log == null)
                return;

            questState.SetStatus(QuestStatus.Completed);
            OnQuestCompleted?.Invoke(questState);
            _bindingService.UnbindQuest(questState);
            _log.RemoveQuest(questState);
        }

        /// <summary>
        /// Manually fails a quest (for debugging/editor support).
        /// </summary>
        public void FailQuest(QuestState questState)
        {
            if (_bindingService == null || _log == null)
                return;

            questState.SetStatus(QuestStatus.Failed);
            OnQuestFailed?.Invoke(questState);
            _bindingService.UnbindQuest(questState);
            _log.RemoveQuest(questState);
        }

        /// <summary>
        /// Manually processes all pending objective evaluations.
        /// Useful for testing, cutscenes, or forcing immediate evaluation before saving.
        /// </summary>
        public void ProcessPendingEvaluations()
        {
            _processor?.ProcessAll();
        }

        private void PollConditions()
        {
            if (_log == null || _bindingService == null)
                return;

            foreach (var quest in _log.Active)
            {
                if (quest.Status.IsTerminal())
                    continue;

                foreach (var obj in quest.GetObjectiveStates())
                {
                    if (!obj.CanProgress(quest))
                        continue;

                    _bindingService.RefreshPollingConditions(obj, () => _processor?.MarkDirty(quest, obj));
                }
            }
        }
    }
}
