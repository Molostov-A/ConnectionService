namespace WebProducer.Services;

public interface IProduserService
{
    Task SendMessageAsync(object obj);
    Task SendMessageAsync(string message);
}
