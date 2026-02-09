using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Visualizer.Services;

public sealed class LogBroadcaster
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
