using MessageBrokerToolkit.Models;

namespace MessageBrokerToolkit.Services;

public interface IConsumerService
{
    Task<UserConnectionResponse> WaitForReplyAsync(string correlationId);
    Task StartListening();
    bool IsListening { get; }
}
