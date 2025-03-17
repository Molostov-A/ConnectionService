namespace ConnectionLogger.Messaging.Configurations;

public class AppSettingsBase
{
    public LoggingSettings Logging { get; set; } = new();

    public RabbitMQSettings RabbitMQ { get; set; } = new();

    public string AllowedHosts { get; set; } = "*";
}
