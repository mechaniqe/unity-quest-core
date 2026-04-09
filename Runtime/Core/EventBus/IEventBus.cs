#nullable enable
using System;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Abstraction for the event system used by quest conditions.
    /// Implement this interface to integrate any publish/subscribe solution,
    /// or use the built-in <see cref="SimpleEventBus"/> for scenarios without
    /// an external event framework.
    /// </summary>
    /// <remarks>
    /// To integrate the DynamicBox EventManager package, wrap it with
    /// <see cref="EventManagerAdapter"/> and assign it to
    /// <see cref="QuestManager.EventBus"/> before the manager initializes.
    /// </remarks>
    public interface IEventBus
    {
        /// <summary>
        /// Subscribes <paramref name="handler"/> to events of type <typeparamref name="TEvent"/>.
        /// </summary>
        void Subscribe<TEvent>(Action<TEvent> handler);

        /// <summary>
        /// Unsubscribes a previously registered <paramref name="handler"/>.
        /// </summary>
        void Unsubscribe<TEvent>(Action<TEvent> handler);

        /// <summary>
        /// Publishes <paramref name="evt"/> to all subscribers registered for
        /// <typeparamref name="TEvent"/>.
        /// </summary>
        void Publish<TEvent>(TEvent evt);
    }
}
