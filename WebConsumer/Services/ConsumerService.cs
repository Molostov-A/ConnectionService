using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;
using System.Text;
using System.Diagnostics;
using System;
using System.Data.Common;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace WebConsumer.Services
{
    public class ConsumerService : BackgroundService
    {
        private readonly ILogger<ConsumerService> _logger;
        private IConnection _connection;
        private IChannel _channel;
        private const string QueueName = "connections";
        public ConsumerService(ILogger<ConsumerService> logger)
        {
            _logger = logger;
            InitializingСomponents();
        }

        private async Task InitializingСomponents()
        {
            
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            using (_connection = await factory.CreateConnectionAsync())
            using (_channel = await _connection.CreateChannelAsync())
            {
                Task<QueueDeclareOk> task =
                    _channel.QueueDeclareAsync(queue: "connections",
                                        durable: false,
                                        exclusive: false,
                                        autoDelete: false);
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (_channel == null)
            {
                _logger.LogWarning("⏳ Ожидание подключения к RabbitMQ...");
                await Task.Delay(1000, stoppingToken);
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    // Имитация обработки
                    _logger.LogInformation($"[Received] {message}");
                    await Task.Delay(500); // Симуляция обработки

                    // Подтверждение обработки сообщения
                    _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка обработки сообщения");
                }
            };

            _channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer);

            await Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel?.CloseAsync();
            _connection?.CloseAsync();
            base.Dispose();
        }
    }
}
