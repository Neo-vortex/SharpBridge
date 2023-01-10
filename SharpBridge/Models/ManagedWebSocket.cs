using System.Net.WebSockets;

namespace SharpBridge.Models;

public record ManagedWebSocket
{
    public  WebSocket WebSocket { get; set; }
    public Guid Topic { get; set; }

    public  Guid ID { get; set; }
}