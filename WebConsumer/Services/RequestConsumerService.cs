using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using WebConsumer.Configurations;
using Microsoft.Extensions.Options;
using ModelsLibrary.Configurations;
using WebConsumer.Interfaces;

namespace WebConsumer.Services;

public class RequestConsumerService : BackgroundService, IDisposable
{
    private readonly ILogger<RequestConsumerService> _logger;
    private readonly AppSettings _appSettings;
    private readonly RabbitMQSettings _rabbitMqSettings;
    private readonly string _queueName;

    private IConnection _connection;
    private IChannel _channel;

    private readonly IEnumerable<IMessageHandler> _handlers;
    private readonly IResponseProduser _messageSender;

    public RequestConsumerService(IEnumerable<IMessageHandler> handlers, IResponseProduser messageSender, ILogger<RequestConsumerService> logger, IOptions<AppSettings> appSettings)
    {
        _handlers = handlers;
        _messageSender = messageSender;

        _logger = logger;
        _appSettings = appSettings.Value;
        _rabbitMqSettings = _appSettings.RabbitMQ;
        _queueName = _appSettings.RabbitMQ.RequestQueue;

        Task.Run(InitializeComponentsAsync).Wait();        
    }

    private async Task InitializeComponentsAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqSettings.HostName,
            Port = _rabbitMqSettings.Port,
            UserName = _rabbitMqSettings.UserName,
            Password = _rabbitMqSettings.Password
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(queue: _queueName,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

        await _channel.ExchangeDeclareAsync(exchange: "headers_exchange", type: ExchangeType.Headers);
        //_logger.LogInformation("✅ Подключение к RabbitMQ для прослушивания ЗАПРОСОВ установлено.");

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            //_logger.LogError("❌ _channel не инициализирован.");
            return;
        }

        //_logger.LogInformation("Consumer запущен, ожидаю сообщения...");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                if (ea == null)
                {
                    //_logger.LogWarning("❌ Получено некорректное сообщение.");
                    return;
                }

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Dictionary<string, object> headers = (Dictionary<string, object>)ea.BasicProperties.Headers;
                var correlationId = ea.BasicProperties.CorrelationId;
                var convertedHeaders = headers.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value is byte[] byteArray ? Encoding.UTF8.GetString(byteArray) : kvp.Value
);
                //_logger.LogWarning("Вызов обработчиков.");

                bool handled = false;

                // Вызов подходящего обработчика
                foreach (var handler in _handlers)
                {
                    if (handler.CanHandle(convertedHeaders))
                    {
                        await handler.HandleAsync(message, convertedHeaders, correlationId, _messageSender);
                        handled = true;
                        break;
                    }
                }

                if (handled)
                {
                    await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
                }
                else
                {
                    //_logger.LogWarning("⚠ Сообщение не обработано ни одним обработчиком.");
                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false);
                }

            }
            catch (Exception ex)
            {
                //_logger.LogError($"❌ Ошибка обработки сообщения: {ex.Message}");
                await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        await _channel.BasicConsumeAsync(queue: _queueName, autoAck: false, consumer: consumer);
        await Task.CompletedTask;

    }

    public override async void Dispose()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
        }
        if (_connection != null)
        {
            await _connection.CloseAsync();
        }

        base.Dispose();
    }
}
