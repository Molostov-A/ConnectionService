using ConnectionLogger.Messaging.Configurations;

namespace ConnectionLogger.Api.Configurations;

public class AppSettings : AppSettingsBase
{
    public ConnectionStringsSettings ConnectionStrings { get; set; } = new();
}
