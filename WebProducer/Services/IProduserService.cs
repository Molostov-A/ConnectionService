namespace WebProducer.Services;

public interface IProduserService
{
    Task SendAsync(object obj, string correlationId);
    Task SendAsync(string message, string correlationId);
}
