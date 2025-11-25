using System;
using GenericQuest.Core;

namespace GenericQuest.EventManagement
{
    /// <summary>
    /// Adapter placeholder for the event-management system.
    /// TODO: Map to actual mechaniqe event-management API
    /// </summary>
    public sealed class EventManagementQuestBus : IQuestEventBus
    {
        // TODO: Inject actual EventManager instance
        // private readonly EventManager _eventManager;

        public EventManagementQuestBus(/* EventManager eventManager */)
        {
            // _eventManager = eventManager;
            throw new NotImplementedException("EventManagementQuestBus requires actual EventManager integration");
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            // TODO: Map to actual event-management API
            // _eventManager.RegisterListener(handler);
            throw new NotImplementedException();
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            // TODO: Map to actual event-management API
            // _eventManager.UnregisterListener(handler);
            throw new NotImplementedException();
        }

        public void Publish<TEvent>(TEvent evt) where TEvent : class
        {
            // TODO: Map to actual event-management API
            // _eventManager.DispatchEvent(evt);
            throw new NotImplementedException();
        }
    }
}
