using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Application.Manager;
using WebCrawler.Domain.Interfaces.Repositories;
using WebCrawler.Domain.Interfaces.Services;

namespace WebCrawler.Application.Services
{
    public class CrawlerService : ICrawlerService
    {
        private readonly IPageRepository _pageRepository;
        private readonly SpiderManager _spiderManager;
        public CrawlerService(IPageRepository pageRepository, SpiderManager spiderManager)
        {
            _pageRepository = pageRepository;
            _spiderManager = spiderManager;
        }
        public void EnqueueUrl(string url)
        {
            if(string.IsNullOrEmpty(url))
                throw new ArgumentException("url is required");
            if(_pageRepository.IsPageAlreadyVisited(url).Result)
                return;
            
            _spiderManager.EnqueueUrl(url);
        }

        public void Pause(bool pause)
        {
            if (pause)
                _spiderManager.Pause();
            else
                _spiderManager.Resume();
        }

        public async Task<List<string>> GetQueue()
        {
            var list = _spiderManager.ListUrls();
            return string.IsNullOrEmpty(list) ? new List<string>() : new List<string>(list.Split(Environment.NewLine));
        }
        
    }
}