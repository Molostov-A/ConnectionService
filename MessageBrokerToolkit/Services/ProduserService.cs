using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Data.Common;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using WebConsumer.Configurations;

namespace WebProducer.Services;
public class ProduserService : IProduserService,IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly AppSettings _appSettings;
    private readonly RabbitMQSettings _rabbitMqSettings;

    public ProduserService(IOptions<AppSettings> appSettings)
    {
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

        _channel.QueueDeclareAsync(queue: _rabbitMqSettings.RequestQueue,
                                   durable: false,
                                   exclusive: false,
                                   autoDelete: false,
                                   arguments: null).Wait();
    }

    public async Task SendAsync(object obj, string correlationId)
    {
        var message = JsonSerializer.Serialize(obj);
        await SendAsync(message, correlationId);
    }

    public async Task SendAsync(string message, string correlationId)
    {
        var body = Encoding.UTF8.GetBytes(message);
        var properties = new BasicProperties();
        properties.ContentType = "text/plain";
        properties.DeliveryMode = (DeliveryModes)2;
        properties.CorrelationId = correlationId;

        await _channel.BasicPublishAsync(exchange: "",
                                                routingKey: _rabbitMqSettings.RequestQueue,
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