using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebCrawler.Domain.Interfaces.Repositories;
using WebCrawler.Application.Events;
using WebCrawler.Application.Manager;
using WebCrawler.Domain.Events;

namespace WebCrawler.Application.Worker
{
    public class QueueConsumer : BackgroundService
    {
        private readonly ILogger<QueueConsumer> _logger;
        private readonly IPageRepository   _pageRepository;       
        private readonly SpiderManager _spiderManager;
        private readonly CrawlerManager _crawlerManager;

        public QueueConsumer(ILogger<QueueConsumer> logger, IPageRepository pageRepository,  SpiderManager spiderManager, CrawlerManager crawlerManager)
        {
            _logger = logger;
            _pageRepository = pageRepository;
            _spiderManager = spiderManager;
            _crawlerManager = crawlerManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Init QueueConsumer");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_spiderManager.IsPaused() && _spiderManager.HasUrlsToProcess())
                {
                    var url = _spiderManager.DequeueUrl();
                    
                    if (url != null)
                    {
                        _logger.LogInformation("Processing: {Url}", url);
                        var page = await _crawlerManager.ProcessPage(url);
                        if (page != null)
                        {
                            await _pageRepository.SavePageAsync(page);
                            _logger.LogInformation("URL saved: {Url}", url);
                            
                            DomainEventPublisher.Publish(new PageSavedEvent(page.Url, page.Title));
                            
                            var queue = _spiderManager.ListUrls();
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