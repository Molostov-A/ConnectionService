using MessageBrokerModelsLibrary.Configurations;
using MessageBrokerToolkit.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace MessageBrokerToolkit.Services;
public class ProduserServiceMBT<TAppSettings> : IProduserServiceMBT, IDisposable
    where TAppSettings: AppSettingsBase, new()
    
{
    protected readonly ILogger<ProduserServiceMBT<TAppSettings>> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly TAppSettings _appSettings;
    private readonly RabbitMQSettings _rabbitMqSettings;

    public ProduserServiceMBT(IOptions<TAppSettings> appSettings, ILogger<ProduserServiceMBT<TAppSettings>> logger)
    {
        _logger = logger;
        _appSettings = appSettings.Value;
        _rabbitMqSettings = _appSettings.RabbitMQ;

        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqSettings.HostName,
            Port = _rabbitMqSettings.Port,
            UserName = _rabbitMqSettings.UserName,
            Password = _rabbitMqSettings.Password
        };

        _connection = factory.CreateConnectionAsync().Result; // Подключаемся один раз
        _channel = _connection.CreateChannelAsync().Result;
    }

    public async Task SendAsync(object obj, string correlationId, string queue)
    {
        var message = JsonSerializer.Serialize(obj);
        await SendAsync(message, correlationId, queue);
    }

    public async Task SendAsync(string message, string correlationId, string queue)
    {
        _channel.QueueDeclareAsync(
            queue: queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null).Wait();

        var body = Encoding.UTF8.GetBytes(message);
        var properties = new BasicProperties();
        properties.ContentType = "text/plain";
        properties.DeliveryMode = (DeliveryModes)2;
        properties.CorrelationId = correlationId;

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: queue,
            mandatory: true,
            basicProperties: properties,
            body: body);
    }

    public void Dispose()
    {
        _channel?.CloseAsync().Wait();
        _connection?.CloseAsync().Wait();
    }
}