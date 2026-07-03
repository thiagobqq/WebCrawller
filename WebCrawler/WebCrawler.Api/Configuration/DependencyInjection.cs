using WebCrawler.Api.Events;
using WebCrawler.Application.Manager;
using WebCrawler.Application.Services;
using WebCrawler.Domain.Interfaces.Repositories;
using WebCrawler.Domain.Interfaces.Services;
using WebCrawler.Infra.Repositories;

namespace WebCrawler.Api.Configuration
{
    public static class DependencyInjection
    {
        public static WebApplicationBuilder AddDependencyInjection(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IPageRepository, PageRepository>();
            builder.Services.AddScoped<ICrawlerService, CrawlerService>();
            builder.Services.AddScoped<IPageService, PageService>();
            builder.Services.AddSingleton<SpiderManager>();
            builder.Services.AddSingleton<CrawlerManager>();
            builder.Services.AddScoped<DomainEventHandler>();

            return builder;
        }
    }
}
