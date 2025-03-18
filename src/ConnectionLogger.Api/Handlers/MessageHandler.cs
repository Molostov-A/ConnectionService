using ConnectionLogger.Api.Interfaces;

namespace ConnectionLogger.Api.Handlers;

public abstract class MessageHandler<TModel> : IMessageHandler
    where TModel : class
{
    protected string type = typeof(TModel).Name;

    public bool CanHandle(Dictionary<string, object> headers)
    {
        if (headers.ContainsKey("type") && headers != null)
        {
            var value = headers["type"].ToString();
            return value == type;
        }
        else
        {
            return false;
        }
    }

    public virtual async Task HandleAsync(string message, Dictionary<string, object> headers, string correlationId)
    {
        // Логика обработки
        await Task.Delay(100); // Имитация асинхронной обработки
        var response = $"Processed by {type} Handler: {message}";
    }
}
