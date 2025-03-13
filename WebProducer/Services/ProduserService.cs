using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using WebProducer.RabbitMq;

public class ProduserService : IProduserService
{
    public async Task SendMessageAsync(object obj)
    {
        var message = JsonSerializer.Serialize(obj);
        await SendMessageAsync(message);
    }

    public async Task SendMessageAsync(string message)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        using (var connection = await factory.CreateConnectionAsync())
        using (var channel = await connection.CreateChannelAsync())
        {
            Task<QueueDeclareOk> task = 
                channel.QueueDeclareAsync(queue: "connections",
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false,
                                        arguments: null);

            var body = Encoding.UTF8.GetBytes(message);
            var props = new BasicProperties();
            props.ContentType = "text/plain";
            props.DeliveryMode = (DeliveryModes)2;

            await channel.BasicPublishAsync(exchange: "",
                                                routingKey: "connections",
                                                mandatory: true,
                                                basicProperties: props,
                                                body: body);
        }
    }
}