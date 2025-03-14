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

    public ProduserService(IConfiguration configuration, IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;

        var factory = new ConnectionFactory
        {
            HostName = _appSettings.RabbitMQ.HostName,
            Port = _appSettings.RabbitMQ.Port,
            UserName = _appSettings.RabbitMQ.UserName,
            Password = _appSettings.RabbitMQ.Password
        };

        _connection = factory.CreateConnectionAsync().Result; // Подключаемся один раз
        _channel = _connection.CreateChannelAsync().Result;

        _channel.QueueDeclareAsync(queue: _appSettings.RabbitMQ.QueueName,
                                   durable: false,
                                   exclusive: false,
                                   autoDelete: false,
                                   arguments: null).Wait();
    }

    public async Task SendMessageAsync(object obj)
    {
        var message = JsonSerializer.Serialize(obj);
        await SendMessageAsync(message);
    }

    public async Task SendMessageAsync(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        var props = new BasicProperties();
        props.ContentType = "text/plain";
        props.DeliveryMode = (DeliveryModes)2;

        await _channel.BasicPublishAsync(exchange: "",
                                                routingKey: _appSettings.RabbitMQ.QueueName,
                                                mandatory: true,
                                                basicProperties: props,
                                                body: body);        
    }

    public void Dispose()
    {
        _channel?.CloseAsync().Wait();
        _connection?.CloseAsync().Wait();
    }
}