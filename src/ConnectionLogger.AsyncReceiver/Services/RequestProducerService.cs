﻿using ConnectionLogger.AsyncReceiver.Configurations;
using ConnectionLogger.AsyncReceiver.Interfaces;
using ConnectionLogger.Messaging.Configurations;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ConnectionLogger.AsyncReceiver.Services;

public class RequestProducerService : IRequestProducer, IDisposable
{
    protected readonly ILogger<RequestProducerService> _logger;
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchange = "headers_exchange";

    private readonly AppSettings _appSettings;
    private readonly RabbitMQSettings _rabbitMqSettings;
    private readonly string _queueName;

    public RequestProducerService(IOptions<AppSettings> appSettings, ILogger<RequestProducerService> logger)
    {
        _logger = logger;
        _appSettings = appSettings.Value;
        _rabbitMqSettings = _appSettings.RabbitMQ;
        _queueName = _appSettings.RabbitMQ.RequestQueue;

        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqSettings.HostName,
            Port = _rabbitMqSettings.Port,
            UserName = _rabbitMqSettings.UserName,
            Password = _rabbitMqSettings.Password
        };

        _connection = factory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;
        _channel.ExchangeDeclareAsync(exchange: _exchange, type: ExchangeType.Headers).Wait();

        _channel.QueueDeclareAsync(
            queue: _queueName,
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

    public async Task SendAsync(string message, string correlationId, Dictionary<string, object>? headers)
    {
        await _channel.QueueBindAsync(
            queue: _queueName,
            exchange: _exchange,
            routingKey: string.Empty,
            arguments: headers);

        var body = Encoding.UTF8.GetBytes(message);
        var properties = new BasicProperties();
        properties.ContentType = "text/plain";
        properties.CorrelationId = correlationId;
        properties.Headers = headers;

        await _channel.BasicPublishAsync(
            exchange: _exchange,
            routingKey: _queueName,
            mandatory: false,
            basicProperties: properties,
        body: body);
    }

    public void Dispose()
    {
        _channel?.CloseAsync().Wait();
        _connection?.CloseAsync().Wait();
    }
}