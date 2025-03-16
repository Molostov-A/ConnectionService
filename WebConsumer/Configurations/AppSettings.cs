using ModelsLibrary.Configurations;

namespace WebConsumer.Configurations;

public class AppSettings : AppSettingsBase
{
    public ConnectionStringsSettings ConnectionStrings { get; set; } = new();
}
