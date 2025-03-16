using MessageBrokerModelsLibrary.Configurations;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using WebConsumer.Configurations;
using WebConsumer.Interfaces;

namespace WebConsumer.Services;

public class ResponseProduserService : IResponseProduser, IDisposable
{
    protected readonly ILogger<ResponseProduserService> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    private readonly AppSettings _appSettings;
    private readonly RabbitMQSettings _rabbitMqSettings;
    private readonly string _queueName;

    public ResponseProduserService(IOptions<AppSettings> appSettings, ILogger<ResponseProduserService> logger)
    {
        _logger = logger;
        _appSettings = appSettings.Value;
        _rabbitMqSettings = _appSettings.RabbitMQ;
        _queueName = _appSettings.RabbitMQ.ResponseQueue;

        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqSettings.HostName,
            Port = _rabbitMqSettings.Port,
            UserName = _rabbitMqSettings.UserName,
            Password = _rabbitMqSettings.Password
        };

        _connection = factory.CreateConnectionAsync().Result; // Подключаемся один раз
        _channel = _connection.CreateChannelAsync().Result;

        _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null).Wait();
    }

    public async Task SendResponseAsync(string correlationId, string response)
    {
        var properties = new BasicProperties();
        properties.CorrelationId = correlationId;
        var body = Encoding.UTF8.GetBytes(response);

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: _queueName,
            mandatory: true,
            basicProperties: properties,
            body: body);

        //_logger.LogInformation($"Sent response with CorrelationId: {correlationId}");
    }

    public void Dispose()
    {
        _channel?.CloseAsync().Wait();
        _connection?.CloseAsync().Wait();
    }
}
