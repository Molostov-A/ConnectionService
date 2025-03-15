namespace WebConsumer.Services
{
    public interface IMessageHandler
    {
        bool CanHandle(Dictionary<string, object> headers);
        Task HandleAsync(string message, Dictionary<string, object> headers, string correlationId, IMessageSender messageSender);
    }
}
