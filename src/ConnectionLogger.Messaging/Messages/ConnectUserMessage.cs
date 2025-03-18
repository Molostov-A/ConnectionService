namespace ConnectionLogger.Messaging.Messages;

public class ConnectUserMessage
{
    public long UserId { get; set; }

    public required string IpAddress { get; set; }

    public required string Protocol { get; set; }
}