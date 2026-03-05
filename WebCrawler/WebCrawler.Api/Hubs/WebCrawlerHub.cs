using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WebCrawler.Domain.Interfaces.Services;

namespace WebCrawler.Api.Hubs
{
    public class WebCrawlerHub : Hub
    {
        private readonly ICrawlerService _crawlerService;

        public WebCrawlerHub(ICrawlerService crawlerService) => _crawlerService = crawlerService;
        public async Task SendCrawlProgress(string url, int progress)
        {
            await Clients.All.SendAsync("ReceiveCrawlProgress", url, progress);
        }         
        public async Task GetCrawlQueue()
        {
            var queue = await _crawlerService.GetQueue();            
            await Clients.Caller.SendAsync("ReceiveCrawlQueue", new { count = queue.Count, urls = queue });
        }
    }
}