using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WebCrawler.Domain.Events;
using WebCrawler.Api.Hubs;

namespace WebCrawler.Api.Events
{
    public class DomainEventHandler
    {
        private readonly IHubContext<WebCrawlerHub> _hubContext;

        public DomainEventHandler(IHubContext<WebCrawlerHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Handle(DomainEvent @event)
        {
            switch (@event)
            {
                case PageSavedEvent pageSaved:
                    await _hubContext.Clients.All.SendAsync("PageSaved", new { url = pageSaved.Url, title = pageSaved.Title });
                    break;

                case QueueUpdatedEvent queueUpdated:
                    await _hubContext.Clients.All.SendAsync("QueueUpdated", new { count = queueUpdated.Count, urls = queueUpdated.Urls });
                    break;
            }
        }
    }
}
