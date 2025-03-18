namespace ConnectionLogger.AsyncReceiver.Controllers.Models;

public record Connection
{
    public required string Ip { get; init; }
}
