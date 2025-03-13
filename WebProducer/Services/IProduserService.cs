namespace WebProducer.RabbitMq
{
    public interface IProduserService
    {
        Task SendMessageAsync(object obj);
        Task SendMessageAsync(string message);
    }
}
