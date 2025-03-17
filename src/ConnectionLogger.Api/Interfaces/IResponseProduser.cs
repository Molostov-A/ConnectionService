namespace ConnectionLogger.Api.Interfaces;

public interface IResponseProduser
{
    Task SendResponseAsync(string correlationId, string response);
}
