using MessageBrokerModelsLibrary.Configurations;
using MessageBrokerModelsLibrary.Models;
using MessageBrokerToolkit.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Threading.Channels;
using WebConsumer.Configurations;

namespace WebConsumer.Services;

public class ConsumerService : ConsumerServiceMBT<AppSettings>, IConsumerService
{
    public ConsumerService(IOptions<AppSettings> appSettings, ILogger<ConsumerServiceMBT<AppSettings>> logger) : base(appSettings, logger)
    {
    }

    // Событие для уведомления о получении сообщения
    public event EventHandler<MessageEventArgs> MessageReceived;

    public override async Task StartConsumingAsync(string queue)
    {
        await InitializeComponentsAsync();

        await _channel.QueueDeclareAsync(
            queue: queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var correlationId = ea.BasicProperties.CorrelationId;

                _logger.LogInformation($"Получено сообщение с CorrelationId: {correlationId}");

                if (_pendingRequests.TryGetValue(correlationId, out var tcs))
                {
                    // Вызываем событие MessageReceived
                    MessageReceived?.Invoke(this, new MessageEventArgs
                    {
                        Body = message,
                        CorrelationId = correlationId
                    });
                }
                else
                {
                    _logger.LogWarning($"Сообщение с CorrelationId: {correlationId} не имеет ожидающего запроса.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке сообщения.");
            }

            // Явно возвращаем Task, чтобы удовлетворить сигнатуру асинхронного метода
            await Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(
            queue: queue,
            autoAck: true,
            consumer: consumer);
    }


}
