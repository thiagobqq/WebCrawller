using System.Collections.Generic;

namespace WebCrawler.Domain.Events
{
    public class QueueUpdatedEvent : DomainEvent
    {
        public int Count { get; set; }
        public List<string> Urls { get; set; }

        public QueueUpdatedEvent(int count, List<string> urls)
        {
            Count = count;
            Urls = urls;
        }
    }
}
