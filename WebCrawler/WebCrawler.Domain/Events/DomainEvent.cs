using System;

namespace WebCrawler.Domain.Events
{
    public abstract class DomainEvent
    {
        public DateTime OccurredAt { get; } = DateTime.UtcNow;
    }
}
