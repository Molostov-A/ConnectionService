namespace ConnectionLogger.Api.Interfaces;

public interface IMessageHandler
{
    bool CanHandle(Dictionary<string, object> headers);

    Task HandleAsync(string message, Dictionary<string, object> headers, string correlationId, IResponseProduser messageSender);
}
