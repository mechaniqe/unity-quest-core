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

        /// <summary>
        /// Gets the list of all active quests currently being tracked.
        /// </summary>
        public IReadOnlyList<QuestState> ActiveQuests => _log?.Active ?? Array.Empty<QuestState>();

        /// <summary>
        /// Event raised when a quest is successfully completed.
        /// </summary>
        public event Action<QuestState>? OnQuestCompleted;
        
        /// <summary>
        /// Event raised when a quest fails.
        /// </summary>
        public event Action<QuestState>? OnQuestFailed;
        
        /// <summary>
        /// Event raised when any objective's status changes.
        /// </summary>
        public event Action<ObjectiveState>? OnObjectiveStatusChanged;
        
        /// <summary>
        /// Event raised when a condition's met/unmet state changes.
        /// Provides the objective, condition instance, and new met state.
        /// </summary>
        public event Action<ObjectiveState, IConditionInstance, bool>? OnConditionStatusChanged;

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
            
            // Wire up events from processor with safe invocation
            _processor.OnQuestCompleted += (q) => SafeInvoke(OnQuestCompleted, q, "OnQuestCompleted");
            _processor.OnQuestFailed += (q) => SafeInvoke(OnQuestFailed, q, "OnQuestFailed");
            _processor.OnObjectiveStatusChanged += (o) => SafeInvoke(OnObjectiveStatusChanged, o, "OnObjectiveStatusChanged");
            
            // Set callback for evaluator to mark objectives as dirty
            _evaluator.SetDirtyCallback(_processor.MarkDirty);
            // Set callback for evaluator to notify about status changes
            _evaluator.SetStatusChangedCallback((o) => SafeInvoke(OnObjectiveStatusChanged, o, "OnObjectiveStatusChanged"));
            // Set callback for binding service to notify about condition changes
            _bindingService.SetConditionChangedCallback((obj, cond, isMet) => SafeInvokeCondition(OnConditionStatusChanged, obj, cond, isMet));
        }

        /// <summary>
        /// Safely invokes an event, catching exceptions from individual subscribers to prevent breaking the event chain.
        /// </summary>
        private void SafeInvoke<T>(Action<T>? eventDelegate, T arg, string eventName)
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
                    Debug.LogError($"Exception in {eventName} subscriber: {ex.Message}\n{ex.StackTrace}", this);
                }
            }
        }

        /// <summary>
        /// Safely invokes the condition status changed event with three parameters.
        /// </summary>
        private void SafeInvokeCondition(Action<ObjectiveState, IConditionInstance, bool>? eventDelegate, 
            ObjectiveState objective, IConditionInstance condition, bool isMet)
        {
            if (eventDelegate == null) return;

            foreach (Action<ObjectiveState, IConditionInstance, bool> handler in eventDelegate.GetInvocationList())
            {
                try
                {
                    handler(objective, condition, isMet);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Exception in OnConditionStatusChanged subscriber: {ex.Message}\n{ex.StackTrace}", this);
                }
            }
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

            Debug.Assert(_processor != null, "QuestManager._processor should be initialized in Awake()");
            _processor!.ProcessAll();
        }

        /// <summary>
        /// Starts a quest and activates objectives that are ready to progress.
        /// Immediately evaluates objectives in case they can complete instantly.
        /// </summary>
        /// <param name="questAsset">The quest definition to start.</param>
        /// <returns>The runtime state for the newly started quest.</returns>
        public QuestState StartQuest(QuestAsset questAsset)
        {
            Debug.Assert(_log != null, "QuestManager._log should be initialized in Awake()");
            Debug.Assert(_evaluator != null, "QuestManager._evaluator should be initialized in Awake()");

            var state = _log!.StartQuest(questAsset);;
            _evaluator!.ActivateReadyObjectives(state);
            
            // Immediately evaluate objectives in case they're already complete
            foreach (var obj in state.GetObjectiveStates())
            {
                if (obj.Status.IsActive())
                {
                    _processor!.MarkDirty(state, obj);
                }
            }
            
            return state;
        }

        /// <summary>
        /// Stops a quest and cleans up all event subscriptions and bindings.
        /// Quest will no longer be tracked or updated.
        /// </summary>
        /// <param name="questState">The quest state to stop.</param>
        public void StopQuest(QuestState questState)
        {
            Debug.Assert(_bindingService != null, "QuestManager._bindingService should be initialized in Awake()");
            Debug.Assert(_log != null, "QuestManager._log should be initialized in Awake()");

            _bindingService!.UnbindQuest(questState);
            _log!.RemoveQuest(questState);
        }

        /// <summary>
        /// Manually completes a quest (for debugging/editor support).
        /// Cleans up bindings and raises OnQuestCompleted event.
        /// </summary>
        /// <param name="questState">The quest state to complete.</param>
        public void CompleteQuest(QuestState questState)
            => EndQuest(questState, QuestStatus.Completed, OnQuestCompleted);

        /// <summary>
        /// Manually fails a quest (for debugging/editor support).
        /// Cleans up bindings and raises OnQuestFailed event.
        /// </summary>
        /// <param name="questState">The quest state to fail.</param>
        public void FailQuest(QuestState questState)
            => EndQuest(questState, QuestStatus.Failed, OnQuestFailed);

        /// <summary>
        /// Internal helper to end a quest with a specific status.
        /// Handles cleanup and event notification.
        /// </summary>
        private void EndQuest(QuestState questState, QuestStatus status, Action<QuestState>? eventHandler)
        {
            Debug.Assert(_bindingService != null, "QuestManager._bindingService should be initialized in Awake()");
            Debug.Assert(_log != null, "QuestManager._log should be initialized in Awake()");

            questState.SetStatus(status);
            eventHandler?.Invoke(questState);
            _bindingService!.UnbindQuest(questState);
            _log!.RemoveQuest(questState);
        }

        /// <summary>
        /// Manually processes all pending objective evaluations.
        /// Useful for testing, cutscenes, or forcing immediate evaluation before saving.
        /// </summary>
        public void ProcessPendingEvaluations()
        {
            Debug.Assert(_processor != null, "QuestManager._processor should be initialized in Awake()");
            _processor!.ProcessAll();
        }

        private void PollConditions()
        {
            Debug.Assert(_log != null, "QuestManager._log should be initialized in Awake()");
            Debug.Assert(_bindingService != null, "QuestManager._bindingService should be initialized in Awake()");

            foreach (var quest in _log!.Active)
            {
                if (quest.Status.IsTerminal())
                    continue;

                foreach (var obj in quest.GetObjectiveStates())
                {
                    if (!obj.CanProgress(quest))
                        continue;

                    _bindingService!.RefreshPollingConditions(obj, () => _processor!.MarkDirty(quest, obj));
                }
            }
        }
    }
}
