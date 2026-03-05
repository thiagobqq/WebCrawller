using Microsoft.EntityFrameworkCore;
using WebCrawler.Api;
using WebCrawler.Domain.Interfaces.Repositories;
using WebCrawler.Application.Worker;
using WebCrawler.Infra.Data;
using WebCrawler.Infra.Repositories;
using WebCrawler.Api.Hubs;
using WebCrawler.Application.Events;
using WebCrawler.Api.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WebCrawlerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDependencies();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<QueueConsumer>();
builder.WebHost.UseUrls("http://0.0.0.0:5000");

builder.Services.AddSignalR();
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => {
        policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true);
    });
});
builder.Services.AddScoped<DomainEventHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WebCrawlerDbContext>();    
    var eventHandler = scope.ServiceProvider.GetRequiredService<DomainEventHandler>();
    db.Database.Migrate();    
    DomainEventPublisher.Subscribe(async @event => await eventHandler.Handle(@event));
}


if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors();
app.MapHub<WebCrawlerHub>("/crawlerHub");

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API"));

app.MapControllers();

app.Run();
