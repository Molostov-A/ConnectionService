using WebProducer.Models;

namespace WebProducer.Services
{
    public interface IConsumerService
    {
        Task<UserConnectionResponse> WaitForReplyAsync(string correlationId);
        Task StartListening();
        bool IsListening { get; }
    }
}
