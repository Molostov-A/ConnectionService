namespace WebConsumer.Configurations;

public class AppSettings
{
    public ConnectionStringsSettings ConnectionStrings { get; set; } = new();
    public LoggingSettings Logging { get; set; } = new();
    public RabbitMQSettings RabbitMQ { get; set; } = new();
    public string AllowedHosts { get; set; } = "*";
}
