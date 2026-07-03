using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebCrawler.Domain.Events;
using WebCrawler.Application.Events;
using WebCrawler.Api.Events;
using WebCrawler.Api.Hubs;
using WebCrawler.Infra.Data;

namespace WebCrawler.Api.Configuration
{
    public static class ApplicationBuilderExtensions
    {
        public static WebApplication ApplyMigrations(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                var connectionString = app.Configuration.GetConnectionString("DefaultConnection")
                    ?? app.Configuration["CONNECTION_STRING"]
                    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
                    ?? throw new InvalidOperationException("Connection string nao configurada.");

                EnsureDatabaseExists(connectionString, logger);

                var dbContext = services.GetRequiredService<WebCrawlerDbContext>();
                var hasMigrations = dbContext.Database.GetMigrations().Any();

                if (hasMigrations)
                {
                    dbContext.Database.Migrate();
                }
                else
                {
                    dbContext.Database.EnsureCreated();
                }

                var eventHandler = services.GetRequiredService<DomainEventHandler>();
                DomainEventPublisher.Subscribe(async @event => await eventHandler.Handle(@event));
            }

            return app;
        }

        public static WebApplication UseApi(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API WebCrawler"));
            }

            app.UseCors();
            app.MapHub<WebCrawlerHub>("/crawlerHub");
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.MapControllers();

            return app;
        }

        private static void EnsureDatabaseExists(string connectionString, ILogger logger)
        {
            var csb = new SqlConnectionStringBuilder(connectionString);
            var dbName = csb.InitialCatalog;

            if (string.IsNullOrWhiteSpace(dbName))
            {
                logger.LogInformation("Sem InitialCatalog definido; pulando criacao de DB.");
                return;
            }

            var masterCsb = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };

            for (var attempt = 1; attempt <= 30; attempt++)
            {
                try
                {
                    using var conn = new SqlConnection(masterCsb.ConnectionString);
                    conn.Open();

                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"IF DB_ID(N'{dbName.Replace("'", "''")}') IS NULL CREATE DATABASE [{dbName}]";
                    cmd.ExecuteNonQuery();

                    logger.LogInformation("Banco {DbName} disponivel.", dbName);
                    return;
                }
                catch (Exception ex)
                {
                    logger.LogInformation("Tentativa {Attempt}/30 ao SQL (master) falhou: {Msg}", attempt, ex.Message);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }
            }

            throw new Exception("Nao foi possivel conectar ao SQL Server para criar o DB apos 30 tentativas.");
        }
    }
}
