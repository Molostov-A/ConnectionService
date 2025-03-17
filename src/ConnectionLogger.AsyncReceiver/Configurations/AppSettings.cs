using ConnectionLogger.Messaging.Configurations;

namespace ConnectionLogger.AsyncReceiver.Configurations;

public class AppSettings : AppSettingsBase
{
    public ApiSettings ApiSettings { get; set; }
}
