namespace ModelsLibrary.Configurations;

public class RabbitMQSettings
{
    public string HostName { get; set; } = string.Empty;

    public int Port { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string RequestQueue { get; set; } = string.Empty;

    public string ResponseQueue { get; set; } = string.Empty;
}
