using Microsoft.AspNetCore.Mvc;

namespace SharpBridge.Controllers;

public class Hello : ControllerBase
{
    
    /// <summary>
    /// respecting https://docs.walletconnect.com/1.0/bridge-server#test-hello-world
    /// </summary>
    /// <returns>version and name</returns>
    [HttpGet("v1/hello")]
    public ActionResult<string> GetHello()
    {
        return "Hello from Sharp Bridge, This is WalletConnect V1";
    }
}