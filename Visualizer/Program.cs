using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using SharedConfig;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<LogBroadcaster>();
builder.Services.AddHostedService<LogTailService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseWebSockets();

app.MapGet("/health", () => Results.Ok("ok"));

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
            if (result.MessageType == WebSocketMessageType.Close)
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
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "closing", CancellationToken.None);
        }
        catch
        {
            // Ignore close failures
        }
    }
});

app.Run("http://localhost:5080");

sealed class LogBroadcaster
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();

    public Guid AddClient(WebSocket socket)
    {
        var id = Guid.NewGuid();
        _clients[id] = socket;
        return id;
    }

    public void RemoveClient(Guid id)
    {
        _clients.TryRemove(id, out _);
    }

    public async Task BroadcastAsync(object payload, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);
        var segment = new ArraySegment<byte>(bytes);

        foreach (var entry in _clients)
        {
            var socket = entry.Value;
            if (socket.State != WebSocketState.Open)
            {
                RemoveClient(entry.Key);
                continue;
            }

            try
            {
                await socket.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
            }
            catch
            {
                RemoveClient(entry.Key);
            }
        }
    }
}

sealed class LogTailService : BackgroundService
{
    private readonly LogBroadcaster _broadcaster;

    private readonly string _distanceFilePath;
    private readonly string _positionsFilePath;

    private long _distancePosition;
    private long _positionsPosition;

    public LogTailService(LogBroadcaster broadcaster)
    {
        _broadcaster = broadcaster;

        var root = FindRepoRoot();
        var logsPath = ConfigManager.Get("LOGS_PATH").Trim();
        var distanceName = ConfigManager.Get("DISTANCE_CSV_NAME").Trim();
        var positionsName = ConfigManager.Get("POSITIONS_CSV_NAME").Trim();

        var resolvedLogsPath = ResolvePath(root, logsPath);
        if (!Directory.Exists(resolvedLogsPath))
        {
            var fallback = Path.Combine(root, "SimulationMaster", logsPath);
            resolvedLogsPath = ResolvePath(root, fallback);
        }
        _distanceFilePath = Path.Combine(resolvedLogsPath, distanceName);
        _positionsFilePath = Path.Combine(resolvedLogsPath, positionsName);

        _distancePosition = 0;
        _positionsPosition = 0;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _distancePosition = await ReadNewLinesAsync(_distanceFilePath, isDistance: true, _distancePosition, stoppingToken);
            _positionsPosition = await ReadNewLinesAsync(_positionsFilePath, isDistance: false, _positionsPosition, stoppingToken);
            await Task.Delay(250, stoppingToken);
        }
    }

    private async Task<long> ReadNewLinesAsync(string filePath, bool isDistance, long position, CancellationToken stoppingToken)
    {
        if (!File.Exists(filePath))
        {
            return position;
        }

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        if (stream.Length < position)
        {
            position = 0;
        }
        stream.Seek(position, SeekOrigin.Begin);

        using var reader = new StreamReader(stream, Encoding.UTF8);
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            if (line.StartsWith("Timestamp", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var payload = isDistance ? ParseDistance(line) : ParsePosition(line);
            if (payload != null)
            {
                await _broadcaster.BroadcastAsync(payload, stoppingToken);
            }
        }

        position = stream.Position;
        return position;
    }

    private static object? ParseDistance(string line)
    {
        var parts = line.Split(',');
        if (parts.Length < 5)
        {
            return null;
        }

        return new
        {
            type = "distance",
            timestamp = parts[0],
            requestId = parts[1],
            timer = parts[2],
            distance = parts[3],
            anchorId = parts[4]
        };
    }

    private static object? ParsePosition(string line)
    {
        var parts = line.Split(',');
        if (parts.Length < 8)
        {
            return null;
        }

        return new
        {
            type = "position",
            timestamp = parts[0],
            requestId = parts[1],
            timer = parts[2],
            role = parts[3],
            id = parts[4],
            x = parts[5],
            y = parts[6],
            z = parts[7]
        };
    }

    private static string ResolvePath(string root, string path)
    {
        return Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(Path.Combine(root, path));
    }

    private static string FindRepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(dir))
        {
            if (File.Exists(Path.Combine(dir, ".env")) || File.Exists(Path.Combine(dir, "takshaon.sln")))
            {
                return dir;
            }
            dir = Directory.GetParent(dir)?.FullName;
        }
        return Directory.GetCurrentDirectory();
    }
}
