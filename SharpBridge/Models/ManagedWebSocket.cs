using System.Net.WebSockets;

namespace SharpBridge.Models;

public record ManagedWebSocket
{
    public  WebSocket WebSocket { get; init; }
    public Guid Topic { get; init; }

    public  Guid ID { get; set; }
}