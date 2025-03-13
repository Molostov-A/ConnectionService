using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using CommonData.Services;
using DataLibrary.Models;
using WebConsumer.Configurations;
using Microsoft.Extensions.Options;

namespace WebConsumer.Services;

public class ConsumerService : BackgroundService
{
    private readonly ILogger<ConsumerService> _logger;
    private readonly IDataService _dataService;
    private readonly AppSettings _appSettings;
    private IConnection _connection;
    private IChannel _channel;
    public ConsumerService(ILogger<ConsumerService> logger, IDataService dataService, IOptions<AppSettings> appSettings)
    {
        _logger = logger;
        _dataService = dataService;
        _appSettings = appSettings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeComponentsAsync();

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

                long userId = 1; // Пример, ты будешь извлекать реальные данные
                string address = "192.168.1.1";
                string protocol = "IPv4";
                // Имитация обработки
                _logger.LogInformation($"📩 Получено сообщение: {message}");
                await _dataService.SaveConnectionAsync(userId, address, protocol);
                

                // Подтверждение обработки сообщения
                _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки сообщения");
            }
        };

        await _channel.BasicConsumeAsync(queue: _appSettings.RabbitMQ.QueueName, autoAck: false, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
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

        await _channel.QueueDeclareAsync(queue: rabbitMq.QueueName,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

        _logger.LogInformation("✅ Подключение к RabbitMQ установлено.");

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
