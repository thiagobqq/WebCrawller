using System;
using System.Collections.Generic;
using WebCrawler.Domain.Events;

namespace WebCrawler.Application.Events
{
    public static class DomainEventPublisher
    {
        private static List<Action<DomainEvent>> _subscribers = new();

        public static void Subscribe(Action<DomainEvent> handler)
        {
            _subscribers.Add(handler);
        }

        public static void Publish(DomainEvent @event)
        {
            foreach (var subscriber in _subscribers)
            {
                subscriber?.Invoke(@event);
            }
        }
    }
}
