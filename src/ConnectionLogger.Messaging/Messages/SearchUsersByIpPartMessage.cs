namespace ConnectionLogger.Messaging.Messages;

public class SearchUsersByIpPartMessage
{
    public required string Ip {  get; set; }

    public required string Protocol { get; set; }
}
