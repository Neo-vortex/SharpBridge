using System.Collections.Concurrent;
using System.Net.WebSockets;
using SharpBridge.Models;

namespace SharpBridge.Services;

public class Dispatcher
{
    private static readonly ConcurrentDictionary<Guid ,  List<ManagedWebSocket> > ManagedWebSockets = new();

    private static readonly ConcurrentDictionary< Guid ,ManagedWalletConnectMessage> ManagedWalletConnectMessages = new();


    public Dispatcher()
    {
        new Thread(Committer).Start();
    }

    private void Committer()
    {
        while (true)
        {
            foreach (var message in ManagedWalletConnectMessages)
            {
               var result =  DispatchMessages(message.Value);
               if (result)
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
                if (ManagedWebSockets[clients.Key].Count == 0)
                {
                    ManagedWebSockets.TryRemove(clients.Key, out _);
                }
            }
        }
    }

    public void AddNewWebSocket(ManagedWebSocket managedWebSocket)
    {
        ManagedWebSockets.AddOrUpdate( managedWebSocket.Topic , guid => new List<ManagedWebSocket>(){managedWebSocket} , (guid, list) =>
        {
            list.Add(managedWebSocket);
            return list;
        });
    }

    public void AddNewMessage(ManagedWalletConnectMessage message )
    {
        ManagedWalletConnectMessages.TryAdd( (Guid)message.Message.Topic!, message);
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
                        result = true;
                    }
                    catch (Exception e)
                    {
                        continue;
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
                        continue;
                    }
                }
            }
        }
        return result;
    }
}