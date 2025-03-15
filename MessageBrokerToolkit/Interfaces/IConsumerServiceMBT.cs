namespace MessageBrokerToolkit.Interfaces;

public interface IConsumerServiceMBT
{
    Task StartConsumingAsync(string queue);
    Task<string> WaitForResponseAsync(string correlationId);
}