using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using CommonData.Services;
using WebConsumer.Configurations;
using Microsoft.Extensions.Options;
using System.Text.Json;
using MessageBrokerModelsLibrary.Models;
using System.Runtime.CompilerServices;
using MessageBrokerToolkit.Interfaces;
using MessageBrokerModelsLibrary.Configurations;

namespace WebConsumer.Services;

public class ConsumerBackgroundService : BackgroundService, IDisposable
{
    private readonly ILogger<ConsumerBackgroundService> _logger;
    private readonly AppSettings _appSettings;
    private readonly RabbitMQSettings _rabbitMqSettings;
    private readonly string _queueName;

    private IConnection _connection;
    private IChannel _channel;

    private readonly IEnumerable<IMessageHandler> _handlers;
    private readonly IMessageSender _messageSender;

    public ConsumerBackgroundService(IEnumerable<IMessageHandler> handlers, IMessageSender messageSender, ILogger<ConsumerBackgroundService> logger, IOptions<AppSettings> appSettings)
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
        _logger.LogInformation("✅ Подключение к RabbitMQ для прослушивания ЗАПРОСОВ установлено.");

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel == null)
        {
            _logger.LogError("❌ _channel не инициализирован.");
            return;
        }

        _logger.LogInformation("Consumer запущен, ожидаю сообщения...");

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                if (ea == null)
                {
                    _logger.LogWarning("❌ Получено некорректное сообщение.");
                    return;
                }

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Dictionary<string, object> headers = (Dictionary<string, object>)ea.BasicProperties.Headers;
                var correlationId = ea.BasicProperties.CorrelationId;

                _logger.LogWarning("Вызов обработчиков.");

                // Вызов подходящего обработчика
                foreach (var handler in _handlers)
                {
                    if (handler.CanHandle(headers))
                    {
                        await handler.HandleAsync(message, headers, correlationId, _messageSender);
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Ошибка обработки сообщения: {ex.Message}");
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
