using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading.Channels;
using MessageBrokerModelsLibrary.Models;
using WebProducer.Configurations;

namespace WebProducer.Services;

public class ResponseConsumerService : BackgroundService, IDisposable
{
    private readonly ILogger<ResponseConsumerService> _logger;
    private readonly AppSettings _appSettings;

    private IConnection _connection;
    private IChannel _channel;
    private readonly string _exchange = "headers_exchange";

    private ResponsePool _responsePool;
    

    public ResponseConsumerService(ILogger<ResponseConsumerService> logger, ResponsePool responsePool, IOptions<AppSettings> appSettings)
    {
        _logger = logger;
        _responsePool = responsePool;
        _appSettings = appSettings.Value;
        Task.Run(InitializeComponentsAsync).Wait();
    }

    private async Task InitializeComponentsAsync()
    {
        var rabbitMq = _appSettings.RabbitMQ;

        var factory = new ConnectionFactory
        {
            HostName = rabbitMq.HostName,
            Port = rabbitMq.Port,
            UserName = rabbitMq.UserName,
            Password = rabbitMq.Password
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(queue: rabbitMq.ResponseQueue,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

        _logger.LogInformation("✅ Подключение к RabbitMQ для прослушивания ответов установлено.");

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
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var correlationId = ea.BasicProperties.CorrelationId;

                _responsePool.AddResponse(correlationId, message);

                _logger.LogInformation($"Received response with CorrelationId: {correlationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "Ошибка обработки сообщения");
            }
        };

        await _channel.BasicConsumeAsync(queue: _appSettings.RabbitMQ.ResponseQueue, autoAck: false, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
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
