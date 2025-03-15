namespace MessageBrokerToolkit.Interfaces;

public interface IProduserServiceMBT
{
    Task SendAsync(object obj, string correlationId, string queue);
    Task SendAsync(string message, string correlationId, string queue);
}
