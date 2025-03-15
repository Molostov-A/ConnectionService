using MessageBrokerModelsLibrary.Models;
using WebConsumer.Services;

namespace WebConsumer.Handlers
{
    public class UserConnectionHandler : IMessageHandler
    {
        private string type = typeof(UserConnectionMessage).Name;
        public bool CanHandle(Dictionary<string, object> headers)
        {
            // Проверка на null и наличие ключа "type"
            return headers != null && headers.ContainsKey("type") && headers["type"].ToString() == type;
        }

        public async Task HandleAsync(string message, Dictionary<string, object> headers, string correlationId, IMessageSender messageSender)
        {
            // Логика обработки
            await Task.Delay(100); // Имитация асинхронной обработки
            var response = $"Processed by {type} Handler: {message}";

            // Отправка результата обработки
            await messageSender.SendResponseAsync(correlationId, response);
        }
    }
}
