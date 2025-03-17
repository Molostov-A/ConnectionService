using ConnectionLogger.Messaging.Configurations;

namespace ConnectionLogger.WebConsumer.Configurations;

public class AppSettings : AppSettingsBase
{
    public ConnectionStringsSettings ConnectionStrings { get; set; } = new();
}
