using Microsoft.AspNetCore.Mvc;
using SharpBridge.Models;
using SharpBridge.Services;

namespace SharpBridge.Controllers;



public class WalletConnectBridge : ControllerBase
{
    private readonly Comminucation _comminucator;

    public WalletConnectBridge(Comminucation comminucator)
    {
        _comminucator = comminucator;
    }

    [HttpGet("v1/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync(new WebSocketAcceptContext()
            {
                DangerousEnableCompression = false,
            });
            await _comminucator.Handle(new ManagedWebSocket() {WebSocket = webSocket });
        }
        else
        {
            await HttpContext.Response.WriteAsync("You need to connect via WebSocket protocol");
        }
    }
}