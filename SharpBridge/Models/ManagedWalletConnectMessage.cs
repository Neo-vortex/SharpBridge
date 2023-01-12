namespace SharpBridge.Models;

public class ManagedWalletConnectMessage
{
    public ManagedWalletConnectMessage()
    {
        CreationTime = DateTime.UtcNow;
        ID = Guid.NewGuid();
    }

    public  WalletConnectMessage Message { get; set; }
    public byte[] Binary { get; set; }
    
    private DateTime CreationTime { get; set; }
    
    public bool Expired => DateTime.UtcNow - CreationTime > TimeSpan.FromMinutes(5);
    
    public Guid ID { get; set; }
}