using ConnectionLogger.Messaging.Configurations;

namespace ConnectionLogger.Api.Configurations;

public class AppSettings : AppSettingsBase
{
    public required ConnectionStringsSettings ConnectionStrings { get; set; }
}
