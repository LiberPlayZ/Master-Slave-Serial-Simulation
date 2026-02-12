using System.Text;
using SharedConfig;
using Visualizer.Models;

namespace Visualizer.Services;

public sealed class LogTailService : BackgroundService
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

    public string DistanceFilePath => _distanceFilePath;
    public string PositionsFilePath => _positionsFilePath;
    public DateTime? LastDistanceReadUtc { get; private set; }
    public DateTime? LastPositionsReadUtc { get; private set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _distancePosition = await ReadNewLinesAsync(_distanceFilePath, isDistance: true, _distancePosition, stoppingToken);
            _positionsPosition = await ReadNewLinesAsync(_positionsFilePath, isDistance: false, _positionsPosition, stoppingToken);
            await Task.Delay(250, stoppingToken);
        }
    }

    public LogTailHealth GetHealthSnapshot()
    {
        return new LogTailHealth(
            DistanceFilePath,
            File.Exists(_distanceFilePath),
            File.Exists(_distanceFilePath) ? File.GetLastWriteTimeUtc(_distanceFilePath) : null,
            LastDistanceReadUtc,
            PositionsFilePath,
            File.Exists(_positionsFilePath),
            File.Exists(_positionsFilePath) ? File.GetLastWriteTimeUtc(_positionsFilePath) : null,
            LastPositionsReadUtc);
    }

    public async Task<string> ReadDistanceCsvAsync(CancellationToken cancellationToken)
    {
        return await ReadCsvAsync(_distanceFilePath, cancellationToken);
    }

    public async Task<string> ReadPositionsCsvAsync(CancellationToken cancellationToken)
    {
        return await ReadCsvAsync(_positionsFilePath, cancellationToken);
    }

    private static async Task<string> ReadCsvAsync(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return string.Empty;
        }

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync(cancellationToken);
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
                if (isDistance)
                {
                    LastDistanceReadUtc = DateTime.UtcNow;
                }
                else
                {
                    LastPositionsReadUtc = DateTime.UtcNow;
                }
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
