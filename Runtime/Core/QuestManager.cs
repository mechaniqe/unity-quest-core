using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using DynamicBox.Quest.EventManagement;

namespace DynamicBox.Quest.Core
{
    public class QuestManager : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] private QuestPlayerRef playerRef;          // builds QuestContext

        [Header("Polling (optional)")]
        [SerializeField] private bool enablePolling = true;
        [SerializeField] private float pollingInterval = 0.25f;

        private IQuestEventBus _eventBus;
        private QuestLog _log;
        private QuestContext _context;

        private readonly Queue<(QuestState quest, ObjectiveState obj)> _dirtyQueue = new();
        private float _pollTimer;

        public event Action<QuestState> OnQuestCompleted;
        public event Action<QuestState> OnQuestFailed;
        public event Action<ObjectiveState> OnObjectiveStatusChanged;

        private void Awake()
        {
            // Create event bus - can be replaced with external event management systems
            _eventBus = CreateEventBus();
            _log = new QuestLog();
            _context = playerRef.BuildContext();
        }

        private IQuestEventBus CreateEventBus()
        {
            // Using the built-in event bus implementation.
            // This can be overridden or changed to use DI/external systems.
            return new EventManagementQuestBus();
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

            ProcessDirtyQueue();
        }

        public QuestState StartQuest(QuestAsset questAsset)
        {
            var state = _log.StartQuest(questAsset);
            BindQuestConditions(state);
            return state;
        }

        public void StopQuest(QuestState questState)
        {
            UnbindQuestConditions(questState);
            _log.RemoveQuest(questState);
        }

        private void BindQuestConditions(QuestState quest)
        {
            foreach (var obj in quest.GetObjectiveStates())
            {
                if (obj.CompletionInstance != null)
                    obj.CompletionInstance.Bind(_eventBus, _context, () => MarkDirty(quest, obj));

                if (obj.FailInstance != null)
                    obj.FailInstance.Bind(_eventBus, _context, () => MarkDirty(quest, obj));
            }
        }

        private void UnbindQuestConditions(QuestState quest)
        {
            foreach (var obj in quest.GetObjectiveStates())
            {
                if (obj.CompletionInstance != null)
                    obj.CompletionInstance.Unbind(_eventBus, _context);

                if (obj.FailInstance != null)
                    obj.FailInstance.Unbind(_eventBus, _context);
            }
        }

        private void PollConditions()
        {
            foreach (var quest in _log.Active)
            {
                if (quest.Status is QuestStatus.Completed or QuestStatus.Failed)
                    continue;

                foreach (var obj in quest.GetObjectiveStates())
                {
                    if (!CanProgressObjective(obj, quest))
                        continue;

                    if (obj.CompletionInstance is IPollingConditionInstance pComp)
                        pComp.Refresh(_context, () => MarkDirty(quest, obj));

                    if (obj.FailInstance is IPollingConditionInstance pFail)
                        pFail.Refresh(_context, () => MarkDirty(quest, obj));
                }
            }
        }

        private void MarkDirty(QuestState quest, ObjectiveState obj)
        {
            _dirtyQueue.Enqueue((quest, obj));
        }

        private void ProcessDirtyQueue()
        {
            while (_dirtyQueue.Count > 0)
            {
                var (quest, obj) = _dirtyQueue.Dequeue();
                EvaluateObjectiveAndQuest(quest, obj);
            }
        }

        private void EvaluateObjectiveAndQuest(QuestState quest, ObjectiveState obj)
        {
            if (quest.Status is QuestStatus.Completed or QuestStatus.Failed)
                return;

            // Ensure objective can be active
            if (!CanProgressObjective(obj, quest))
                return;

            // Fail first
            if (obj.FailInstance != null && obj.FailInstance.IsMet)
            {
                obj.SetStatus(ObjectiveStatus.Failed);
                OnObjectiveStatusChanged?.Invoke(obj);

                quest.SetStatus(QuestStatus.Failed);
                OnQuestFailed?.Invoke(quest);

                UnbindQuestConditions(quest);
                _log.RemoveQuest(quest);
                return;
            }

            // Complete
            if (obj.CompletionInstance != null && obj.CompletionInstance.IsMet)
            {
                if (obj.Status == ObjectiveStatus.NotStarted)
                {
                    obj.SetStatus(ObjectiveStatus.InProgress);
                    OnObjectiveStatusChanged?.Invoke(obj);
                }

                obj.SetStatus(ObjectiveStatus.Completed);
                OnObjectiveStatusChanged?.Invoke(obj);
            }

            // Quest completion check
            if (quest.GetObjectiveStates().All(o =>
                    o.Definition.IsOptional || o.Status == ObjectiveStatus.Completed))
            {
                quest.SetStatus(QuestStatus.Completed);
                OnQuestCompleted?.Invoke(quest);

                UnbindQuestConditions(quest);
                _log.RemoveQuest(quest);
            }
        }

        private static bool CanProgressObjective(ObjectiveState obj, QuestState quest)
        {
            if (obj.Status is ObjectiveStatus.Completed or ObjectiveStatus.Failed)
                return false;

            var prereq = obj.Definition.Prerequisites;
            if (prereq == null || prereq.Count == 0)
                return true;

            foreach (var pre in prereq)
            {
                if (pre == null) continue;

                if (!quest.TryGetObjective(pre.ObjectiveId, out var preState))
                    continue; // or treat missing as incomplete?

                if (preState.Status != ObjectiveStatus.Completed)
                    return false;
            }

            return true;
        }
    }
}
