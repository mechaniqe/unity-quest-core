#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Adapts the <c>DynamicBox.EventManagement.EventManager</c> to the
    /// <see cref="IEventBus"/> interface, allowing quest conditions to participate
    /// in an existing EventManager-based event pipeline without changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All event types passed to this adapter must implement <c>IGameEvent</c>
    /// (required by EventManager). For event types that do not,
    /// use <see cref="SimpleEventBus"/> instead.
    /// </para>
    /// <para>
    /// Assign an instance to <see cref="QuestManager.EventBus"/> before the manager
    /// initializes to enable EventManager integration:
    /// <code>
    /// questManager.EventBus = new EventManagerAdapter();
    /// </code>
    /// </para>
    /// </remarks>
    public sealed class EventManagerAdapter : IEventBus
    {
        private readonly EventManager _eventManager;

        // Maps the caller's Action<TEvent> to the EventDelegate<TEvent> wrapper
        // so we can pass the exact same instance to RemoveListener.
        private readonly Dictionary<Delegate, Delegate> _handlerMap = new();

        // Cached MethodInfo for the constrained helpers (reflection bridges).
        private static readonly MethodInfo _subscribeImplMethod =
            typeof(EventManagerAdapter).GetMethod(
                nameof(SubscribeImpl),
                BindingFlags.NonPublic | BindingFlags.Instance)!;

        private static readonly MethodInfo _unsubscribeImplMethod =
            typeof(EventManagerAdapter).GetMethod(
                nameof(UnsubscribeImpl),
                BindingFlags.NonPublic | BindingFlags.Instance)!;

        /// <summary>Creates an adapter that wraps <c>EventManager.Instance</c>.</summary>
        public EventManagerAdapter() : this(EventManager.Instance) { }

        /// <summary>
        /// Creates an adapter that wraps the provided <paramref name="eventManager"/>.
        /// </summary>
        public EventManagerAdapter(EventManager eventManager)
        {
            _eventManager = eventManager ?? throw new ArgumentNullException(nameof(eventManager));
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown when <typeparamref name="TEvent"/> does not implement <c>IGameEvent</c>.
        /// </exception>
        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            AssertGameEvent<TEvent>();
            _subscribeImplMethod
                .MakeGenericMethod(typeof(TEvent))
                .Invoke(this, new object[] { handler });
        }

        /// <inheritdoc />
        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (_handlerMap.TryGetValue(handler, out var wrapped))
            {
                _unsubscribeImplMethod
                    .MakeGenericMethod(typeof(TEvent))
                    .Invoke(this, new object[] { wrapped });
                _handlerMap.Remove(handler);
            }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="evt"/> does not implement <c>IGameEvent</c>.
        /// </exception>
        public void Publish<TEvent>(TEvent evt)
        {
            if (evt is IGameEvent gameEvent)
                _eventManager.Raise(gameEvent);
            else
                throw new InvalidOperationException(
                    $"EventManagerAdapter.Publish requires a type that implements IGameEvent. " +
                    $"'{typeof(TEvent).Name}' does not implement IGameEvent. " +
                    $"Use SimpleEventBus for custom event types.");
        }

        // --- Constrained helpers (called via reflection to cross the generic constraint boundary) ---

        private void SubscribeImpl<TEvent>(Action<TEvent> handler) where TEvent : IGameEvent
        {
            EventManager.EventDelegate<TEvent> wrapped = evt => handler(evt);
            _handlerMap[handler] = wrapped;
            _eventManager.AddListener(wrapped);
        }

        private void UnsubscribeImpl<TEvent>(Delegate wrapped) where TEvent : IGameEvent
        {
            _eventManager.RemoveListener((EventManager.EventDelegate<TEvent>)wrapped);
        }

        private static void AssertGameEvent<TEvent>()
        {
            if (!typeof(IGameEvent).IsAssignableFrom(typeof(TEvent)))
                throw new InvalidOperationException(
                    $"EventManagerAdapter requires event types that implement IGameEvent. " +
                    $"'{typeof(TEvent).Name}' does not. Use SimpleEventBus for custom event types.");
        }
    }
}
