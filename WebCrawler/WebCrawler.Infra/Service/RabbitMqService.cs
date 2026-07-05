using EasyNetQ;
using WebCrawler.Domain.Interfaces.Repositories;

namespace WebCrawler.Infra.Service;

public class RabbitMqService : IMessagePublisher
{
    private readonly IBus _bus;

    public RabbitMqService(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishPageScrapedAsync<T>(T message) where T : class
    {
        await _bus.PubSub.PublishAsync(message);
    }
}
    
