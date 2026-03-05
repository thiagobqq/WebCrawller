using WebCrawler.Domain.DTO;

namespace WebCrawler.Domain.Events
{
    public class PageSavedEvent : DomainEvent
    {
        public string Url { get; set; }
        public string Title { get; set; }

        public PageSavedEvent(string url, string title)
        {
            Url = url;
            Title = title;
        }
    }
}
