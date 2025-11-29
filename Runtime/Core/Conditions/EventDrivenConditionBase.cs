#nullable enable
using System;
using DynamicBox.EventManagement;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Base class for event-driven conditions that handles common event subscription boilerplate.
    /// Reduces code duplication and ensures consistent event handler management.
    /// </summary>
    /// <typeparam name="TEvent">The type of game event this condition listens to.</typeparam>
    public abstract class EventDrivenConditionBase<TEvent> : IConditionInstance where TEvent : GameEvent
    {
        private EventManager? _eventManager;
        private Action? _onChanged;
        private EventManager.EventDelegate<TEvent>? _eventHandler;

        public abstract bool IsMet { get; }

        public void Bind(EventManager eventManager, QuestContext context, Action onChanged)
        {
            _eventManager = eventManager;
            _onChanged = onChanged;
            _eventHandler = OnEventReceived;
            
            eventManager.AddListener(_eventHandler);
            
            // Allow subclasses to perform additional initialization
            OnBind(context);
        }

        public void Unbind(EventManager eventManager, QuestContext context)
        {
            if (_eventManager != null && _eventHandler != null)
            {
                eventManager.RemoveListener(_eventHandler);
                _eventManager = null;
                _eventHandler = null;
            }
            
            _onChanged = null;
            
            // Allow subclasses to perform cleanup
            OnUnbind(context);
        }

        /// <summary>
        /// Notifies listeners that the condition state has changed.
        /// Call this when IsMet changes value.
        /// </summary>
        protected void NotifyChanged()
        {
            _onChanged?.Invoke();
        }

        /// <summary>
        /// Called when an event of type TEvent is received.
        /// Implement condition-specific logic here.
        /// </summary>
        protected abstract void HandleEvent(TEvent evt);

        /// <summary>
        /// Called after the condition is bound to the event system.
        /// Override to perform additional initialization.
        /// </summary>
        protected virtual void OnBind(QuestContext context) { }

        /// <summary>
        /// Called before the condition is unbound from the event system.
        /// Override to perform cleanup.
        /// </summary>
        protected virtual void OnUnbind(QuestContext context) { }

        private void OnEventReceived(TEvent evt)
        {
            HandleEvent(evt);
        }
    }
}
