using Microsoft.AspNetCore.Mvc;
using SharpBridge.Models;
using SharpBridge.Services;

namespace SharpBridge.Controllers;



public class WalletConnectBridge : ControllerBase
{
    private readonly Comminucator _comminucator;

    public WalletConnectBridge(Comminucator comminucator)
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
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}