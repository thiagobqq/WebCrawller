using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebCrawler.Application.Manager;
using WebCrawler.Application.Services;
using WebCrawler.Domain.Interfaces.Repositories;
using WebCrawler.Domain.Interfaces.Services;
using WebCrawler.Infra.Repositories;

namespace WebCrawler.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            services.AddScoped<IPageRepository, PageRepository>();
            services.AddScoped<ICrawlerService, CrawlerService>();
            services.AddScoped<IPageService, PageService>();

            services.AddSingleton<SpiderManager>();
            services.AddSingleton<CrawlerManager>();
            
            return services;
        }
    }
}