using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Cryptography;
using SharpBridge.Models;

namespace SharpBridge.Services;

public class Dispatch
{
    private  readonly ConcurrentDictionary<Guid ,  List<ManagedWebSocket> > _managedWebSockets = new();

    private  readonly BlockingCollection<ManagedWalletConnectMessage> _managedWalletConnectMessages = new();


    private readonly ILogger<Dispatch> _logger;
    public Dispatch(ILogger<Dispatch> logger)
    {
        _logger = logger;
        for (var i = 0; i < Environment.ProcessorCount; i++)
        {
           new Thread(() => Committer(CancellationToken.None)).Start();
        }

    }

    private async Task Committer(CancellationToken token)
    {
        _logger.LogInformation($"Dispatch thread started : {Thread.CurrentThread.ManagedThreadId}");
        while (true)
        {
            try
            {
                var message =   _managedWalletConnectMessages.Take(token);
                if (  DateTime.UtcNow - message.LastTry  > TimeSpan.FromSeconds(2))
                {
                    _logger.LogInformation($"processing { message.ID } from thread {Thread.CurrentThread.ManagedThreadId}");
                    var result= await  DispatchMessages(message);
                    message.LastTry = DateTime.UtcNow;
                    if (!message.Expired & ! result)
                    {
                        _managedWalletConnectMessages.Add(message, token);
                    }
                    else
                    {
                        _logger.LogWarning($"Message {message.ID} expired");
                    }
                }
                else
                {
                    _managedWalletConnectMessages.Add(message, token);
                }
                Thread.Sleep(10);
            }
            catch (Exception)
            {
               break;
            }
          
        }
        
    }

    public void RemoveWebSocket(Guid id)
    {
        foreach (var clients in _managedWebSockets)
        {
            var counter = 0;
            foreach (var client  in clients.Value.Where(client => client.ID == id))
            {
                _managedWebSockets.TryUpdate(clients.Key, clients.Value.Where(client => client.ID != id).ToList(),
                    clients.Value);
                if (_managedWebSockets[clients.Key].Count != 0) continue;
                var result =     _managedWebSockets.TryRemove(clients.Key, out _);
                _logger.LogInformation(result ? $"WebSocket {id} with handler {counter} was closed" : $"WebSocket {id} was failed to closed");
                counter++;
            }
        }
    }

    public void AddNewWebSocket(ManagedWebSocket managedWebSocket)
    {
        _logger.LogInformation( $"New subscriber was added" );
        _managedWebSockets.AddOrUpdate( managedWebSocket.Topic , guid => new List<ManagedWebSocket>(){managedWebSocket} , (guid, list) =>
        {
            list.Add(managedWebSocket);
            return list;
        });
    }

    public void AddNewMessage(ManagedWalletConnectMessage message )
    {
       var result =  _managedWalletConnectMessages.TryAdd( message);
       if (result)
       {
           _logger.LogInformation( $"New message {message.ID} was added" );
       }
       else
       {
           _logger.LogWarning( $"Failed to add message {message.ID}" );
       }
    }

    private async Task<bool> DispatchMessages(ManagedWalletConnectMessage message)
    {
        var result = false;
        if (!_managedWebSockets.ContainsKey((Guid)message.Message.Topic!)) return result;
        var clients = _managedWebSockets[ (Guid) message.Message.Topic!]; 
        foreach (var client in clients)
        {
            try
            {
                await client.WebSocket.SendAsync(new ReadOnlyMemory<byte>((message.Binary)),
                    WebSocketMessageType.Text, true, CancellationToken.None).AsTask().WaitAsync(CancellationToken.None);
                result = true;
            }
            catch (Exception e)
            {
                _logger.LogWarning("Failed to write message to websocket pipeline." + Environment.NewLine + e.Message);
                RemoveWebSocket(client.ID);
            }
        }
        return result;
    }
}