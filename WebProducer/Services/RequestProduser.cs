using MessageBrokerModelsLibrary.Configurations;
using MessageBrokerToolkit.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using WebProducer.Configurations;
using WebProducer.Interfaces;

namespace WebProducer.Services;
public class RequestProduser : IRequestProduser, IDisposable

{
    protected readonly ILogger<RequestProduser> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly AppSettings _appSettings;
    private readonly RabbitMQSettings _rabbitMqSettings;

    public RequestProduser(IOptions<AppSettings> appSettings, ILogger<RequestProduser> logger)
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
        _channel.ExchangeDeclareAsync(exchange: "headers_exchange", type: ExchangeType.Headers);

        _channel.QueueDeclareAsync(
            queue: _appSettings.RabbitMQ.RequestQueue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null).Wait();
    }

    public async Task SendAsync(object obj, string correlationId, Dictionary<string, object> headers)
    {
        var message = JsonSerializer.Serialize(obj);
        await SendAsync(message, correlationId, headers);
    }

    public async Task SendAsync(string message, string correlationId, Dictionary<string, object> headers)
    {
        var body = Encoding.UTF8.GetBytes(message);
        var properties = new BasicProperties();
        properties.ContentType = "text/plain";
        properties.DeliveryMode = (DeliveryModes)2;
        properties.CorrelationId = correlationId;
        properties.Headers = headers;

        await _channel.BasicPublishAsync(
            exchange: "headers_exchange",
            routingKey: _appSettings.RabbitMQ.RequestQueue,
            mandatory: true,
            basicProperties: properties,
            body: body);

        _logger.LogInformation($"Sent message with headers: {string.Join(", ", headers)}");
    }

    public void Dispose()
    {
        _channel?.CloseAsync().Wait();
        _connection?.CloseAsync().Wait();
    }
}