using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebCrawler.Infra.Data
{
    public class WebCrawlerDbContextFactory : IDesignTimeDbContextFactory<WebCrawlerDbContext>
    {
        public WebCrawlerDbContext CreateDbContext(string[] args)
        {
            Env.TraversePath().Load();

            var connectionString = Environment.GetEnvironmentVariable("WEBCRAWLLER_CONNECTION_STRING");
            
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("WEBCRAWLLER_CONNECTION_STRING não foi configurada nas variáveis de ambiente.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<WebCrawlerDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new WebCrawlerDbContext(optionsBuilder.Options);
        }
    }
}