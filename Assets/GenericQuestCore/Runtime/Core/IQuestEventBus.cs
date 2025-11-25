using System;

namespace GenericQuest.Core
{
    public interface IQuestEventBus
    {
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
        void Publish<TEvent>(TEvent evt) where TEvent : class; // optional use
    }
}
