#nullable enable
using System;

namespace DynamicBox.Quest.Core
{
    /// <summary>
    /// Base class for event-driven conditions that handles common event subscription boilerplate.
    /// Reduces code duplication and ensures consistent event handler management.
    /// </summary>
    /// <typeparam name="TEvent">The type of event this condition listens to.</typeparam>
    public abstract class EventDrivenConditionBase<TEvent> : IConditionInstance
    {
        private IEventBus? _eventBus;
        private Action? _onChanged;
        private Action<TEvent>? _eventHandler;

        public abstract bool IsMet { get; }

        public void Bind(IEventBus eventBus, QuestContext context, Action onChanged)
        {
            _eventBus = eventBus;
            _onChanged = onChanged;
            _eventHandler = OnEventReceived;

            eventBus.Subscribe(_eventHandler);

            // Allow subclasses to perform additional initialization
            OnBind(context);
        }

        public void Unbind(IEventBus eventBus, QuestContext context)
        {
            if (_eventBus != null && _eventHandler != null)
            {
                eventBus.Unsubscribe(_eventHandler);
                _eventBus = null;
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
