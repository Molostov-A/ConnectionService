using MessageBrokerModelsLibrary.Configurations;
using MessageBrokerModelsLibrary.Models;
using MessageBrokerToolkit.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace MessageBrokerToolkit.Services;

public class ConsumerServiceMBT<TAppSettings> : IConsumerServiceMBT, IDisposable
    where TAppSettings : AppSettingsBase, new()
{
    protected readonly ILogger<ConsumerServiceMBT<TAppSettings>> _logger;
    protected IConnection _connection;
    protected IChannel _channel;
    protected RabbitMQSettings _rabbitMqSettings;
    protected readonly Dictionary<string, TaskCompletionSource<string>> _pendingRequests;
    protected readonly AppSettingsBase _appSettings;

    public ConsumerServiceMBT(IOptions<TAppSettings> appSettings, ILogger<ConsumerServiceMBT<TAppSettings>> logger)
    {
        _appSettings = appSettings.Value;
        _rabbitMqSettings = _appSettings.RabbitMQ;
        _pendingRequests = new Dictionary<string, TaskCompletionSource<string>>();
        _logger = logger;
        
        Task.Run(InitializeComponentsAsync).Wait(); // Инициализируем соединение с RabbitMQ
    }

    protected async Task InitializeComponentsAsync()
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
        
        _logger.LogInformation("✅ Подключение к RabbitMQ установлено.");

    }

    public virtual async Task StartConsumingAsync(string queue)
    {

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var correlationId = ea.BasicProperties.CorrelationId;

                _logger.LogInformation($"Получено сообщение с CorrelationId: {correlationId}");

                if (_pendingRequests.TryGetValue(correlationId, out var tcs))
                {
                    tcs.TrySetResult(message);
                    _pendingRequests.Remove(correlationId);
                    _logger.LogInformation($"Сообщение с CorrelationId: {correlationId} обработано.");
                }
                else
                {
                    _logger.LogWarning($"Сообщение с CorrelationId: {correlationId} не имеет ожидающего запроса.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обработке сообщения.");
            }

            // Явно возвращаем Task, чтобы удовлетворить сигнатуру асинхронного метода
            await Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(
            queue: queue,
            autoAck: true,
            consumer: consumer);
    }

    public Task<string> WaitForResponseAsync(string correlationId)
    {
        var tcs = new TaskCompletionSource<string>();
        _pendingRequests[correlationId] = tcs;
        return tcs.Task;
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
        }

        _channel = null;
        _connection = null;
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }
}