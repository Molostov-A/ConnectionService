namespace WebConsumer.Configurations;

public class AppSettings
{
    public LoggingSettings Logging { get; set; } = new();
    public RabbitMQSettings RabbitMQ { get; set; } = new();
    public string AllowedHosts { get; set; } = "*";
}
