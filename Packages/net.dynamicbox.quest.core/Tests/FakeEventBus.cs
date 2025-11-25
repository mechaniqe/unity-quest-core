using System;
using System.Collections.Generic;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Tests
{
    /// <summary>
    /// A simple in-memory event bus for testing purposes.
    /// </summary>
    public sealed class FakeEventBus : IQuestEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            var eventType = typeof(TEvent);
            if (!_subscribers.ContainsKey(eventType))
                _subscribers[eventType] = new List<Delegate>();

            _subscribers[eventType].Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            var eventType = typeof(TEvent);
            if (_subscribers.ContainsKey(eventType))
                _subscribers[eventType].Remove(handler);
        }

        public void Publish<TEvent>(TEvent evt) where TEvent : class
        {
            var eventType = typeof(TEvent);
            if (_subscribers.ContainsKey(eventType))
            {
                foreach (var handler in _subscribers[eventType])
                {
                    if (handler is Action<TEvent> typedHandler)
                        typedHandler(evt);
                }
            }
        }
    }
}
