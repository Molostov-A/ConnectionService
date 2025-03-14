using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebConsumer.Configurations;
using WebProducer.Models;
using WebProducer.Services;

public class ConsumerService : IConsumerService, IDisposable
{
    private readonly RabbitMQSettings _rabbitMqSettings;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<UserConnectionResponse>> _pendingResponses = new();
    private IConnection _connection;
    private IChannel _channel;
    private bool _isListening;

    public bool IsListening => _isListening;

    public ConsumerService(IOptions<AppSettings> appSettings)
    {
        _rabbitMqSettings = appSettings.Value.RabbitMQ;
    }

    public Task<UserConnectionResponse> WaitForReplyAsync(string correlationId)
    {
        var tcs = new TaskCompletionSource<UserConnectionResponse>();
        _pendingResponses[correlationId] = tcs;
        return tcs.Task;
    }

    public async Task StartListening()
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqSettings.HostName,
            Port = _rabbitMqSettings.Port,
            UserName = _rabbitMqSettings.UserName,
            Password = _rabbitMqSettings.Password
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(
            queue: _rabbitMqSettings.ResponseQueue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            string json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var response = JsonSerializer.Deserialize<UserConnectionResponse>(json);

            if (response != null && _pendingResponses.TryRemove(response.CorrelationId, out var tcs))
            {
                tcs.SetResult(response);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: _rabbitMqSettings.ResponseQueue,
            autoAck: true,
            consumer: consumer);

        _isListening = true;
    }

    public async void Dispose()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
        }
        if (_connection != null)
        {
            await _connection.CloseAsync();
        }

        Dispose();
    }

}
