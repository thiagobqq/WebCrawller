using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WebCrawler.Infra.Data;
using WebCrawler.Application.Worker;

namespace WebCrawler.Api.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static WebApplicationBuilder AddApi(this WebApplicationBuilder builder)
        {
            var envConn = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            if (!string.IsNullOrWhiteSpace(envConn))
            {
                builder.Configuration["ConnectionStrings:DefaultConnection"] = envConn;
            }

            builder.Services.AddControllers()
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true);
                });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSignalR();
            builder.Services.AddHostedService<QueueConsumer>();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API WebCrawler", Version = "v1" });
            });

            builder.WebHost.UseUrls("http://0.0.0.0:5000");

            return builder;
        }

        public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
        {
            var connectionString = ResolveConnectionString(builder.Configuration);

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDbContext<WebCrawlerDbContext>((sp, options) =>
            {
                options.UseSqlServer(connectionString);
            });

            return builder;
        }

        private static string ResolveConnectionString(IConfiguration configuration)
        {
            return configuration.GetConnectionString("DefaultConnection")
                ?? configuration["CONNECTION_STRING"]
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
                ?? throw new InvalidOperationException("Connection string nao configurada.");
        }
    }
}
