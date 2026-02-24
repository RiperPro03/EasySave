using LogHub.Server.Infrastructure.Queueing;
using LogHub.Server.Infrastructure.Storage;
using LogHub.Server.Options;
using LogHub.Server.WebSockets;
using LogHub.Server.Workers;
using System.Net;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

var startupOptions = builder.Configuration.GetSection(LogHubOptions.SectionName).Get<LogHubOptions>() ?? new LogHubOptions();
builder.WebHost.ConfigureKestrel(kestrel =>
{
    // Ecoute sur toutes les interfaces reseau (conteneur, LAN, localhost).
    kestrel.Listen(IPAddress.Any, startupOptions.Port);
});

builder.Services.Configure<LogHubOptions>(builder.Configuration.GetSection(LogHubOptions.SectionName));
builder.Services.AddSingleton<ChannelLogQueue>();
builder.Services.AddSingleton<DailyFileLogWriter>();
builder.Services.AddHostedService<LogIngestWorker>();

var app = builder.Build();

app.UseWebSockets(new WebSocketOptions
{
    // Garde la connexion WS active avec un ping periodique.
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

app.MapGet("/", () => Results.Ok(new { service = "LogHub.Server", status = "ok" }));
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
string websocketPath = app.Services.GetRequiredService<IOptions<LogHubOptions>>().Value.WebSocketPath;
app.MapLogHubWebSocket(websocketPath);

app.Run();
