using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;

namespace WebConsumer.Services;

public class ConsumerService : BackgroundService
{
    private readonly ILogger<ConsumerService> _logger;
    private IConnection _connection;
    private IChannel _channel;
    private const string QueueName = "connections";
    public ConsumerService(ILogger<ConsumerService> logger)
    {
        _logger = logger;
    }

    private async Task InitializeComponentsAsync()
    {

        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(queue: QueueName,
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

        _logger.LogInformation("✅ Подключение к RabbitMQ установлено.");

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

                // Имитация обработки
                _logger.LogInformation($"📩 Получено сообщение: {message}");
                await Task.Delay(500); // Симуляция обработки

                // Подтверждение обработки сообщения
                _channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки сообщения");
            }
        };

        await _channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        //await Task.CompletedTask;
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
