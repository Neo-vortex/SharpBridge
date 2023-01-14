namespace SharpBridge.Models;

public class ManagedWalletConnectMessage
{
    public ManagedWalletConnectMessage()
    {
        CreationTime = DateTime.UtcNow;
        ID = Guid.NewGuid();
    }

    public  WalletConnectMessage Message { get; init; }
    public byte[] Binary { get; init; }
    
    private DateTime CreationTime { get; set; }
    
    public bool Expired => DateTime.UtcNow - CreationTime > TimeSpan.FromMinutes(2);
    
    public Guid ID { get; set; }

    public DateTime LastTry { get; set; } = DateTime.MinValue;
}