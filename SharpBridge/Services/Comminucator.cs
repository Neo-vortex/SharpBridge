using System.Net.WebSockets;
using SharpBridge.Models;

namespace SharpBridge.Services;

public class Comminucator
{
    private readonly Dispatcher _dispatcher;
    private readonly ILogger<Comminucator> _logger;

    public Comminucator(Dispatcher dispatcher, ILogger<Comminucator> logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
    }

    public async Task Handle(ManagedWebSocket managedWebSocket)
    {
        managedWebSocket.ID = Guid.NewGuid();
        var buffer = new byte[1024*100];
        while (managedWebSocket.WebSocket.State == WebSocketState.Open)
        {
            try
            {
                Array.Clear(buffer , 0, buffer.Length);
                var len = (await managedWebSocket.WebSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None)).Count;
                var pure = buffer.SkipLast((1024 * 100) - len).ToArray();
                #if DEBUG
                    _logger.LogInformation(System.Text.Encoding.UTF8.GetString(pure));
                #endif
                var managedMessage = new ManagedWalletConnectMessage()
                {
                    Binary = pure,
                    Message = WalletConnectMessage.FromJson(System.Text.Encoding.UTF8.GetString(pure))
                };
                switch (managedMessage.Message.Type)
                {
                    case "pub":
                        _dispatcher.AddNewMessage(managedMessage);
                        break;
                    case "sub":
                        _dispatcher.AddNewWebSocket(managedWebSocket with {Topic = (Guid) managedMessage.Message.Topic!} );
                        break;
                    case "ack":
                        break;
                }
              
            }
            catch (Exception e)
            {
                _dispatcher.RemoveWebSocket(managedWebSocket.ID);
                break;
            }

        }
    }
    
}