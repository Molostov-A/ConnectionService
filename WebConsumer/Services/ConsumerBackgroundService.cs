using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using CommonData.Services;
using WebConsumer.Configurations;
using Microsoft.Extensions.Options;
using System.Text.Json;
using MessageBrokerModelsLibrary.Models;
using System.Runtime.CompilerServices;
using MessageBrokerToolkit.Interfaces;

namespace WebConsumer.Services;

public class ConsumerBackgroundService : BackgroundService, IDisposable
{
    private readonly ILogger<ConsumerBackgroundService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IProduserServiceMBT _produserService;
    private readonly AppSettings _appSettings;
    private IConnection _connection;
    private IChannel _channel;
    private readonly IConsumerService _consumerService;
    public ConsumerBackgroundService(IConsumerService consumerService, IProduserServiceMBT produserService, ILogger<ConsumerBackgroundService> logger, IServiceScopeFactory serviceScopeFactory, IOptions<AppSettings> appSettings)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _appSettings = appSettings.Value;
        _produserService = produserService;
        _consumerService = consumerService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        _consumerService.MessageReceived += async (sender, e) =>
        {
            try
            {
                var request = JsonSerializer.Deserialize<UserConnectionMessage>(e.Body);
                if (request == null)
                {
                    _logger.LogWarning("❌ Получено некорректное сообщение.");
                    return;
                }

                long userId = request.UserId;
                string address = request.IpAddress;
                string protocol = request.Protocol;

                object result;

                // ✅ Создаём область видимости (scope) для использования `IDataService`
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();

                    // 🛠 Ожидаем результат выполнения `SaveConnectionAsync`
                    result = await dataService.SaveConnectionAsync(userId, address, protocol);
                }

                // ✅ Отправляем результат в `ResponseQueue`
                await _produserService.SendAsync(result, e.CorrelationId, _appSettings.RabbitMQ.ResponseQueue);
                _logger.LogInformation($"📨 Отправлен результат для {userId}: {result}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Ошибка обработки сообщения: {ex.Message}");
            }
        };

        await _consumerService.StartConsumingAsync(_appSettings.RabbitMQ.RequestQueue);

        await Task.CompletedTask;

    }

    public override async void Dispose()
    {
        if (_channel != null)
        {
            await _channel.CloseAsync();
        }
        if (_connection != null)
        {
            await _connection.CloseAsync();
        }

        base.Dispose();
    }
}
