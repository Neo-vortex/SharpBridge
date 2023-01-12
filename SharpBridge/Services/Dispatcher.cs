using System.Collections.Concurrent;
using System.Net.WebSockets;
using SharpBridge.Models;

namespace SharpBridge.Services;

public class Dispatcher
{
    private  readonly ConcurrentDictionary<Guid ,  List<ManagedWebSocket> > ManagedWebSockets = new();

    private  readonly ConcurrentDictionary< Guid ,ManagedWalletConnectMessage> ManagedWalletConnectMessages = new();


    private readonly ILogger<Dispatcher> _logger;
    public Dispatcher(ILogger<Dispatcher> logger)
    {
        _logger = logger;
        new Thread(Committer).Start();
    }

    private void Committer()
    {
        while (true)
        {
            foreach (var message in ManagedWalletConnectMessages)
            { 
                var result =  DispatchMessages(message.Value);
               if (result || message.Value.Expired)
               {
                   ManagedWalletConnectMessages.TryRemove(message.Key,out _);
               }
            }
            Thread.Sleep(1);
        }
    }

    public void RemoveWebSocket(Guid ID)
    {

        foreach (var clients in ManagedWebSockets)
        {
            foreach (var client in clients.Value.Where(client => client.ID == ID))
            {
                ManagedWebSockets.TryUpdate(clients.Key, clients.Value.Where(client => client.ID != ID).ToList(),
                    clients.Value);
                if (ManagedWebSockets[clients.Key].Count != 0) continue;
                var result =     ManagedWebSockets.TryRemove(clients.Key, out _);
                _logger.LogInformation(result ? $"WebSocket {ID} was closed" : $"WebSocket {ID} was failed to closed");
            }
        }
    }

    public void AddNewWebSocket(ManagedWebSocket managedWebSocket)
    {
        _logger.LogInformation( $"New subscriber was added" );
        ManagedWebSockets.AddOrUpdate( managedWebSocket.Topic , guid => new List<ManagedWebSocket>(){managedWebSocket} , (guid, list) =>
        {
            list.Add(managedWebSocket);
            return list;
        });
    }

    public void AddNewMessage(ManagedWalletConnectMessage message )
    {
       var result =  ManagedWalletConnectMessages.TryAdd( message.ID, message);
       if (result)
       {
           _logger.LogInformation( $"New message {message.ID} was added" );
       }
       else
       {
           _logger.LogWarning( $"Failed to add message {message.ID}" );
       }
    }

    private bool  DispatchMessages(ManagedWalletConnectMessage message)
    {
        var result = false;
        if (ManagedWebSockets.Count > 50)
        {
            Parallel.ForEach(ManagedWebSockets, async pair =>
            {
                if (pair.Key != message.Message.Topic) return;
                foreach (var clients in pair.Value)
                {
                    try
                    {
                        clients.WebSocket.SendAsync(new ReadOnlyMemory<byte>((message.Binary)),
                            WebSocketMessageType.Text, true, CancellationToken.None).AsTask().Wait();
                        _logger.LogInformation( $"Message {message.ID} was delivered" );
                        result = true;
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning($"Failed to write message  {message.ID} to websocket pipeline." + Environment.NewLine + e.Message);
                    }
                }
            });
        }
        else
        {
            foreach (var pair in ManagedWebSockets)
            {
                if (pair.Key != message.Message.Topic) continue;
                foreach (var clients in pair.Value)
                {
                    try
                    {
                        clients.WebSocket.SendAsync(new ReadOnlyMemory<byte>((message.Binary)),
                            WebSocketMessageType.Text, true, CancellationToken.None).AsTask().Wait();
                        result = true;
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning("Failed to write message to websocket pipeline." + Environment.NewLine + e.Message);
                    }
                }
            }
        }
        return result;
    }
}