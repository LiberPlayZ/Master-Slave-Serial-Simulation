using SharedConfig;
using Visualizer.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<LogBroadcaster>();
builder.Services.AddSingleton<LogTailService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<LogTailService>());

var app = builder.Build();
app.UseWebSockets();

app.MapGet("/health", (LogTailService tailService) => Results.Ok(tailService.GetHealthSnapshot()));

app.Map("/ws", async context =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        return;
    }

    var broadcaster = context.RequestServices.GetRequiredService<LogBroadcaster>();
    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
    var clientId = broadcaster.AddClient(webSocket);

    try
    {
        var buffer = new byte[1024];
        while (!context.RequestAborted.IsCancellationRequested)
        {
            var result = await webSocket.ReceiveAsync(buffer, context.RequestAborted);
            if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Close)
            {
                break;
            }
        }
    }
    finally
    {
        broadcaster.RemoveClient(clientId);
        try
        {
            await webSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "closing", CancellationToken.None);
        }
        catch
        {
            // Ignore close failures
        }
    }
});

var host = ConfigManager.Get("VISUALIZER_HOST").Trim();
var port = ConfigManager.GetInt("VISUALIZER_PORT");
app.Run($"http://{host}:{port}");
