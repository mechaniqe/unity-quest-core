using System;
using System.Collections.Generic;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.EventManagement
{
    /// <summary>
    /// Production-ready event bus implementation for quest system.
    /// Can be used standalone or replaced with external event management systems.
    /// </summary>
    public sealed class EventManagementQuestBus : IQuestEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        private readonly object _lock = new object();

        public EventManagementQuestBus()
        {
            // Ready to use out of the box
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null) return;

            lock (_lock)
            {
                var eventType = typeof(TEvent);
                if (!_handlers.ContainsKey(eventType))
                {
                    _handlers[eventType] = new List<Delegate>();
                }
                
                _handlers[eventType].Add(handler);
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            if (handler == null) return;

            lock (_lock)
            {
                var eventType = typeof(TEvent);
                if (_handlers.TryGetValue(eventType, out var handlerList))
                {
                    handlerList.Remove(handler);
                    if (handlerList.Count == 0)
                    {
                        _handlers.Remove(eventType);
                    }
                }
            }
        }

        public void Publish<TEvent>(TEvent evt) where TEvent : class
        {
            if (evt == null) return;

            List<Delegate> handlersToCall;
            lock (_lock)
            {
                var eventType = typeof(TEvent);
                if (!_handlers.TryGetValue(eventType, out var handlerList) || handlerList.Count == 0)
                {
                    return;
                }
                
                // Create a copy to avoid issues with concurrent modifications
                handlersToCall = new List<Delegate>(handlerList);
            }

            // Call handlers outside the lock to avoid deadlocks
            foreach (var handler in handlersToCall)
            {
                try
                {
                    ((Action<TEvent>)handler)(evt);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Error in event handler for {typeof(TEvent).Name}: {ex}");
                }
            }
        }

        /// <summary>
        /// Clear all event handlers. Useful for cleanup.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _handlers.Clear();
            }
        }

        /// <summary>
        /// Get the number of handlers for a specific event type.
        /// </summary>
        public int GetHandlerCount<TEvent>() where TEvent : class
        {
            lock (_lock)
            {
                var eventType = typeof(TEvent);
                return _handlers.TryGetValue(eventType, out var handlerList) ? handlerList.Count : 0;
            }
        }
    }
}
