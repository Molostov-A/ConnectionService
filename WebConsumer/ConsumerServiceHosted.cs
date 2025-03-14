using WebConsumer.Services;

namespace WebConsumer;

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
            var consumerService = scope.ServiceProvider.GetRequiredService<ConsumerService>();
            return consumerService.StartAsync(cancellationToken); // Запуск логики ConsumerService
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // Остановка ConsumerService
        return Task.CompletedTask;
    }
}