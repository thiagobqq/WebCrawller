using DotNetEnv;
using WebCrawler.Api.Configuration;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);
builder.AddApi()
    .AddDatabase()
    .AddDependencyInjection();

var app = builder.Build();
app.ApplyMigrations();
app.UseApi();

app.Run();
