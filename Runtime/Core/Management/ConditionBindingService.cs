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

        public ConditionBindingService(EventManager eventManager, QuestContext context)
        {
            _eventManager = eventManager;
            _context = context;
        }

        /// <summary>
        /// Binds all conditions for an objective to the event system.
        /// </summary>
        public void BindObjective(QuestState quest, ObjectiveState objective, Action onDirty)
        {
            if (objective.CompletionInstance != null)
            {
                objective.CompletionInstance.Bind(_eventManager, _context, onDirty);
            }

            if (objective.FailInstance != null)
            {
                objective.FailInstance.Bind(_eventManager, _context, onDirty);
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
                pollingCompletion.Refresh(_context, onDirty);
            }

            if (objective.FailInstance is IPollingConditionInstance pollingFail)
            {
                pollingFail.Refresh(_context, onDirty);
            }
        }
    }
}
