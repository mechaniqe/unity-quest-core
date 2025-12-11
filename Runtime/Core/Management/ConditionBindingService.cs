#nullable enable
using System;
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Manages binding and unbinding of condition instances to the event system.
    /// Extracted from QuestManager to follow Single Responsibility Principle.
    /// </summary>
    internal sealed class ConditionBindingService
    {
        private readonly EventManager _eventManager;
        private readonly QuestContext _context;
        private Action<ObjectiveState, IConditionInstance, bool>? _onConditionChanged;

        public ConditionBindingService(EventManager eventManager, QuestContext context)
        {
            _eventManager = eventManager;
            _context = context;
        }

        /// <summary>
        /// Sets the callback to invoke when a condition's status changes.
        /// </summary>
        /// <param name="callback">The callback with objective, condition instance, and new met state.</param>
        public void SetConditionChangedCallback(Action<ObjectiveState, IConditionInstance, bool> callback)
        {
            _onConditionChanged = callback;
        }

        /// <summary>
        /// Binds all conditions for an objective to the event system.
        /// </summary>
        public void BindObjective(QuestState quest, ObjectiveState objective, Action onDirty)
        {
            if (objective.CompletionInstance != null)
            {
                var condition = objective.CompletionInstance;
                objective.CompletionInstance.Bind(_eventManager, _context, () =>
                {
                    _onConditionChanged?.Invoke(objective, condition, condition.IsMet);
                    onDirty();
                });
            }

            if (objective.FailInstance != null)
            {
                var condition = objective.FailInstance;
                objective.FailInstance.Bind(_eventManager, _context, () =>
                {
                    _onConditionChanged?.Invoke(objective, condition, condition.IsMet);
                    onDirty();
                });
            }
        }

        /// <summary>
        /// Unbinds all conditions for an objective from the event system.
        /// </summary>
        public void UnbindObjective(ObjectiveState objective)
        {
            if (objective.CompletionInstance != null)
            {
                objective.CompletionInstance.Unbind(_eventManager, _context);
            }

            if (objective.FailInstance != null)
            {
                objective.FailInstance.Unbind(_eventManager, _context);
            }
        }

        /// <summary>
        /// Unbinds all objectives in a quest from the event system.
        /// </summary>
        public void UnbindQuest(QuestState quest)
        {
            foreach (var objective in quest.GetObjectiveStates())
            {
                UnbindObjective(objective);
            }
        }

        /// <summary>
        /// Refreshes polling conditions for an objective.
        /// </summary>
        public void RefreshPollingConditions(ObjectiveState objective, Action onDirty)
        {
            if (objective.CompletionInstance is IPollingConditionInstance pollingCompletion)
            {
                var condition = objective.CompletionInstance;
                pollingCompletion.Refresh(_context, () =>
                {
                    _onConditionChanged?.Invoke(objective, condition, condition.IsMet);
                    onDirty();
                });
            }

            if (objective.FailInstance is IPollingConditionInstance pollingFail)
            {
                var condition = objective.FailInstance;
                pollingFail.Refresh(_context, () =>
                {
                    _onConditionChanged?.Invoke(objective, condition, condition.IsMet);
                    onDirty();
                });
            }
        }
    }
}
