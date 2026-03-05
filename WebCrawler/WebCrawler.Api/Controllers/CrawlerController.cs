using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebCrawler.Domain.DTO;
using WebCrawler.Domain.Interfaces.Repositories;
using WebCrawler.Application;
using WebCrawler.Domain.Interfaces.Services;
using WebCrawler.Application.Events;
using WebCrawler.Domain.Events;

namespace WebCrawler.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CrawlerController : ControllerBase
    {
        private readonly ICrawlerService _crawlerService;

        public CrawlerController(ICrawlerService crawlerService) => _crawlerService = crawlerService;

        [HttpPost("enqueue")]
        public async Task<IActionResult> Enqueue([FromBody] string url)
        {
            _crawlerService.EnqueueUrl(url);
            
            // Publica evento de fila atualizada
            var queue = await _crawlerService.GetQueue();
            DomainEventPublisher.Publish(new QueueUpdatedEvent(queue.Count, queue));
            
            return Accepted();
        }

        [HttpPost("pause")]
        public IActionResult Pause()
        {
            _crawlerService.Pause(true);
            return Ok();
        }

        [HttpPost("resume")]
        public IActionResult Resume()
        {
            _crawlerService.Pause(false);
            return Ok();
        }


        [HttpPost("clear")]
        public async Task ClearCrawl()
        {
            throw new System.NotImplementedException("This endpoint is not implemented yet.");
        }

        [HttpPost("addUrl")]
        public async Task AddCrawledUrl(string url)
        {
            throw new System.NotImplementedException("This endpoint is not implemented yet.");
        }

        [HttpPost("removeUrl")]
        public async Task RemoveCrawledUrl(string url)
        {
            throw new System.NotImplementedException("This endpoint is not implemented yet.");
        }
    }
}
