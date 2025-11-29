#nullable enable
using System;
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Runtime instance of a condition. Maintains mutable state and responds to events.
    /// All condition instances must implement this interface to participate in quest evaluation.
    /// </summary>
    public interface IConditionInstance
    {
        /// <summary>
        /// Gets whether this condition is currently met.
        /// Evaluated during objective processing to determine completion/failure.
        /// </summary>
        bool IsMet { get; }

        /// <summary>
        /// Binds this condition to the event system and context.
        /// Subscribe to relevant events and invoke onChanged when IsMet value changes.
        /// </summary>
        /// <param name="eventManager">The event manager to subscribe to events.</param>
        /// <param name="context">Game services available to this condition.</param>
        /// <param name="onChanged">Callback to invoke when condition state changes.</param>
        void Bind(EventManager eventManager, QuestContext context, Action onChanged);

        /// <summary>
        /// Unbinds this condition from the event system.
        /// Clean up event subscriptions and release references.
        /// </summary>
        /// <param name="eventManager">The event manager to unsubscribe from.</param>
        /// <param name="context">Game services context.</param>
        void Unbind(EventManager eventManager, QuestContext context);
    }

    /// <summary>
    /// Optional interface for conditions that need periodic polling in addition to events.
    /// Useful for continuous conditions like time-based or sensor-based checks.
    /// </summary>
    public interface IPollingConditionInstance
    {
        /// <summary>
        /// Called periodically by the quest system to refresh condition state.
        /// Check current state and invoke onChanged if IsMet value changes.
        /// </summary>
        /// <param name="context">Game services available to this condition.</param>
        /// <param name="onChanged">Callback to invoke when condition state changes.</param>
        void Refresh(QuestContext context, Action onChanged);
    }
}
