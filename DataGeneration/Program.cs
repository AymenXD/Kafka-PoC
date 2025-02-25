var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddLogging(configure => configure.AddConsole());
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

app.MapControllers();

app.Run();
