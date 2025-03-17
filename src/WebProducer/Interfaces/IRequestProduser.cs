namespace ConnectionLogger.WebProducer.Interfaces;

public interface IRequestProduser
{
    Task SendAsync(object obj, string correlationId, Dictionary<string, object> headers);

    Task SendAsync(string message, string correlationId, Dictionary<string, object> headers);
}
