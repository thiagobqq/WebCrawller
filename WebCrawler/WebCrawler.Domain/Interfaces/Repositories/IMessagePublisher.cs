namespace WebCrawler.Domain.Interfaces.Repositories;

public interface IMessagePublisher
{
    Task PublishPageScrapedAsync<T>(T message) where T : class;
}