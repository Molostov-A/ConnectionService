namespace WebConsumer.Services
{
    public interface IMessageSender
    {
        Task SendResponseAsync(string correlationId, string response);
    }
}
