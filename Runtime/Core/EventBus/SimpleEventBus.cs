#nullable enable
using System;
using System.Collections.Generic;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Lightweight <see cref="IEventBus"/> implementation with no external dependencies.
    /// Events are dispatched synchronously to all handlers registered for a given type.
    /// </summary>
    /// <remarks>
    /// Each <see cref="SimpleEventBus"/> instance is isolated: only handlers subscribed
    /// to <i>this</i> instance receive published events. This makes it ideal for unit
    /// testing and for projects that do not need a global event manager singleton.
    /// </remarks>
    public sealed class SimpleEventBus : IEventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new();

        /// <inheritdoc />
        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            var type = typeof(TEvent);
            if (_handlers.TryGetValue(type, out var existing))
                _handlers[type] = (Action<TEvent>)existing + handler;
            else
                _handlers[type] = handler;
        }

        /// <inheritdoc />
        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            var type = typeof(TEvent);
            if (!_handlers.TryGetValue(type, out var existing))
                return;

            var updated = (Action<TEvent>)existing - handler;
            if (updated == null)
                _handlers.Remove(type);
            else
                _handlers[type] = updated;
        }

        /// <inheritdoc />
        public void Publish<TEvent>(TEvent evt)
        {
            if (_handlers.TryGetValue(typeof(TEvent), out var del))
                ((Action<TEvent>)del).Invoke(evt);
        }
    }
}
