using WebCrawler.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.AddApi()
    .AddDatabase()
    .AddDependencyInjection();

var app = builder.Build();
app.ApplyMigrations();
app.UseApi();

app.Run();
