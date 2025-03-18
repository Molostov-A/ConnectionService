namespace ConnectionLogger.Messaging.Messages;

public class ConnectMessage
{
    public long UserId { get; init; }

    public required string Ip { get; set; }

    public DateTime ConnectedAt { get; set; }
}
