using MessageBrokerModelsLibrary.Models;
using WebConsumer.Interfaces;

namespace WebConsumer.Handlers
{
    public class UserConnectionHandler : IMessageHandler
    {
        private string type = typeof(UserConnectionMessage).Name;
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

        public async Task HandleAsync(string message, Dictionary<string, object> headers, string correlationId, IResponseProduser messageSender)
        {
            // Логика обработки
            await Task.Delay(100); // Имитация асинхронной обработки
            var response = $"Processed by {type} Handler: {message}";


            // Отправка результата обработки
            await messageSender.SendResponseAsync(correlationId, response);
        }
    }
}
