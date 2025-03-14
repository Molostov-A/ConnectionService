using WebProducer.Models;

namespace WebConsumer.Services;

public interface IConsumerService
{
    Task<UserConnectionResponse> WaitForReplyAsync(string correlationId);
    Task StartListening();
    bool IsListening { get; }
}
