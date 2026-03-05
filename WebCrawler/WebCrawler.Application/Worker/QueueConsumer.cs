using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebCrawler.Domain.Interfaces.Repositories;
using WebCrawler.Application.Events;
using WebCrawler.Domain.Events;

namespace WebCrawler.Application.Worker
{
    public class QueueConsumer : BackgroundService
    {
        private readonly ILogger<QueueConsumer> _logger;
        private readonly IPageRepository   _pageRepository;       

        public QueueConsumer(ILogger<QueueConsumer> logger, IPageRepository pageRepository)
        {
            _logger = logger;
            _pageRepository = pageRepository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Init QueueConsumer");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!WebCrawler.SPIDER_MANAGER.IsPaused() && WebCrawler.SPIDER_MANAGER.HasUrlsToProcess())
                {
                    var url = WebCrawler.SPIDER_MANAGER.DequeueUrl();
                    
                    if (url != null)
                    {
                        _logger.LogInformation("Processing: {Url}", url);
                        var page = await WebCrawler.CRAWLER_MANAGER.ProcessPage(url);
                        if (page != null)
                        {
                            await _pageRepository.SavePageAsync(page);
                            _logger.LogInformation("URL saved: {Url}", url);
                            
                            // Publica evento de página salva
                            DomainEventPublisher.Publish(new PageSavedEvent(page.Url, page.Title));
                            
                            // Publica evento de fila atualizada (novas URLs encontradas)
                            var queue = WebCrawler.SPIDER_MANAGER.ListUrls();
                            var queueList = string.IsNullOrEmpty(queue) ? new System.Collections.Generic.List<string>() : new System.Collections.Generic.List<string>(queue.Split(Environment.NewLine));
                            DomainEventPublisher.Publish(new QueueUpdatedEvent(queueList.Count, queueList));
                        }
                        else
                        {
                            _logger.LogWarning("Failed to process URL: {Url}", url);
                        }                       
                            
                        
                    }
                }

                await Task.Delay(500, stoppingToken);
            }

            _logger.LogInformation("QueueConsumer stopped");
        }
    }
}