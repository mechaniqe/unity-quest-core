using System;
using System.Collections.Generic;
using DynamicBox.Quest.Core;

namespace DynamicBox.Quest.Tests.Helpers
{
    /// <summary>
    /// Test double for IEventBus that records all published events.
    /// Delegates routing to an internal SimpleEventBus so subscriptions still work.
    /// </summary>
    public class MockEventBus : IEventBus
    {
        private readonly List<object> _published = new();
        private readonly SimpleEventBus _inner = new();

        public IReadOnlyList<object> Published => _published;
        public int SubscribeCallCount { get; private set; }
        public int UnsubscribeCallCount { get; private set; }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            SubscribeCallCount++;
            _inner.Subscribe(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            UnsubscribeCallCount++;
            _inner.Unsubscribe(handler);
        }

        public void Publish<TEvent>(TEvent evt)
        {
            _published.Add(evt!);
            _inner.Publish(evt);
        }

        public bool WasPublished<TEvent>() => _published.Exists(e => e is TEvent);
        public int PublishCount<TEvent>() => _published.FindAll(e => e is TEvent).Count;
    }
}
