namespace WebConsumer.Services;

public class ConsumerServiceHosted : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public ConsumerServiceHosted(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope()) // Создаем область для разрешения scoped сервисов
        {
            var consumerService = scope.ServiceProvider.GetRequiredService<ConsumerBackgroundService>();
            return consumerService.StartAsync(cancellationToken); // Запуск логики ConsumerBackgroundService
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Остановка ConsumerServiceMBT
        return Task.CompletedTask;
    }
}